"""Bounded file-only Whisper benchmark. No devices, network or Unity APIs."""
import argparse
import hashlib
import io
import json
import random
import re
import statistics
import subprocess
import struct
import tempfile
import time
import unicodedata
import wave
from array import array
from pathlib import Path

import psutil
import pyarrow.parquet as pq
from opencc import OpenCC

MODEL = "ggml-large-v3-turbo-q5_0.bin"
VAD = "ggml-silero-v6.2.0.bin"
MODEL_HASH = "394221709cd5ad1f40c46e6031ca61bce88931e6e088c188294c6d5a55ffa7e2"
VAD_HASH = "2aa269b785eeb53a82983a20501ddf7c1d9c48e33ab63a41391ac6c9f7fb6987"
FLAGS = ["-t", "4", "-bs", "1", "-bo", "1", "-mc", "0", "-nf", "-sns", "-nt",
         "-fa", "--vad", "-vt", "0.5", "-vspd", "250"]
TO_SIMPLIFIED = OpenCC("t2s")


def sha256(path):
    with path.open("rb") as stream:
        return hashlib.file_digest(stream, "sha256").hexdigest()


def read_pcm(source):
    with wave.open(source if isinstance(source, io.BytesIO) else str(source)) as wav:
        if wav.getnchannels() != 1 or wav.getframerate() != 16000:
            raise ValueError("Expected 16 kHz mono corpus audio")
        width = wav.getsampwidth()
        pcm = array({2: "h", 4: "i"}[width], wav.readframes(wav.getnframes()))
    return pcm if width == 2 else array("h", (n >> 16 for n in pcm))


def wav_bytes(pcm):
    out = io.BytesIO()
    with wave.open(out, "wb") as wav:
        wav.setparams((1, 2, 16000, 0, "NONE", "not compressed"))
        wav.writeframes(pcm.tobytes())
    return out.getvalue()


def fleurs_pcm(data):
    # FLEURS parquet uses IEEE float WAV. Read RIFF chunks and validate the format.
    if data[:4] != b"RIFF" or data[8:12] != b"WAVE":
        raise ValueError("Expected a RIFF Wave file")
    chunks = {}
    offset = 12
    while offset + 8 <= len(data):
        tag, size = struct.unpack_from("<4sI", data, offset)
        chunks[tag] = data[offset + 8:offset + 8 + size]
        offset += 8 + size + size % 2
    if struct.unpack_from("<HHI", chunks[b"fmt "]) != (3, 1, 16000):
        raise ValueError("Expected FLEURS float32 mono at 16 kHz")
    return array("h", (round(max(-1, min(1, x)) * 32767)
                       for (x,) in struct.iter_unpack("<f", chunks[b"data"])))


def corpus(root):
    samples = json.loads((root / "en.json").read_text(encoding="utf-8-sig"))
    samples += [
        dict(id="exhibition-question", language="en", kind="synthetic",
             reference="How does the light in Gaia change what I notice?"),
        dict(id="exhibition-names", language="en", kind="synthetic",
             reference="Gaia Pandemic Rebirth White Guardian Conscience Sanctuary")
    ]
    fields = ("id", "audio", "raw_transcription", "num_samples", "gender")
    short = []
    long = None
    for batch in pq.ParquetFile(root / "mandarin.parquet").iter_batches(batch_size=12, columns=fields):
        for row in batch.to_pylist():
            length = row["num_samples"] / 16000
            if 2 <= length <= 6 and len(short) < 6:
                short.append(row)
            if long is None and 12 <= length <= 24:
                long = row
        if len(short) == 6 and long is not None:
            break
    for row in short + [long]:
        sample_id = "zh-fleurs-" + row["audio"]["path"].removesuffix(".wav")
        data = wav_bytes(fleurs_pcm(row["audio"]["bytes"]))
        (root / "audio" / (sample_id + ".wav")).write_bytes(data)
        samples.append(dict(id=sample_id, reference=row["raw_transcription"], language="zh",
                            source="Google FLEURS cmn_hans_cn validation", kind="human",
                            gender=row["gender"], fleurs_id=row["id"]))

    randomizer = random.Random(4201)
    noise = array("h", (randomizer.randrange(-1900, 1901) for _ in range(64000)))
    base = read_pcm(root / "audio/exhibition-question.wav")
    noisy = array("h", (max(-32768, min(32767, s + noise[i % len(noise)])) for i, s in enumerate(base)))
    mixed = read_pcm(root / "audio" / (samples[-7]["id"] + ".wav"))
    mixed.extend([0] * 3200)
    mixed.extend(base)
    for key, pcm, lang, ref, kind in [
        ("silence", array("h", [0]) * 64000, "en", "", "negative"),
        ("noise", noise, "zh", "", "negative"),
        ("question-noisy", noisy, "en", samples[6]["reference"], "synthetic"),
        ("mixed-spliced", mixed, "zh", samples[-7]["reference"] + samples[6]["reference"], "spliced"),
    ]:
        (root / "audio" / (key + ".wav")).write_bytes(wav_bytes(pcm))
        samples.append(dict(id=key, reference=ref, kind=kind, language=lang))
    for sample in samples:
        path = root / "audio" / (sample["id"] + ".wav")
        sample["seconds"] = len(read_pcm(path)) / 16000
        sample["sha256"] = sha256(path)
    return samples


def normalized(text, language):
    text = unicodedata.normalize("NFKC", text).lower()
    if language == "zh":
        return [c for c in TO_SIMPLIFIED.convert(text) if c.isalnum()]
    return re.findall(r"[a-z0-9]+(?:'[a-z]+)?", text.replace("’", "'"))


def edits(a, b):
    row = list(range(len(b) + 1))
    for i, x in enumerate(a, 1):
        updated = [i]
        for j, y in enumerate(b, 1):
            updated.append(min(updated[-1] + 1, row[j] + 1, row[j-1] + (x != y)))
        row = updated
    return row[-1]


def invoke(root, backend, clips, language, pipe=False):
    exe = next((root / backend).rglob("whisper-cli.exe"))
    cmd = [str(exe), "-m", str(root / MODEL), "-vm", str(root / VAD), "-l", language, *FLAGS]
    if backend == "cpu":
        cmd += ["-ng"]
    paths = [root / "audio" / (clip["id"] + ".wav") for clip in clips]
    for path in (["-"] if pipe else paths):
        cmd += ["-f", str(path)]
    # The upstream CLI is one process with no descendants. Redirect to bounded local
    # benchmark files so polling can observe memory even while it writes diagnostics.
    with tempfile.TemporaryDirectory(dir=root) as directory, tempfile.TemporaryFile() as out, tempfile.TemporaryFile() as err:
        prefixes = [Path(directory) / str(i) for i in range(len(clips))]
        cmd += ["-oj"]
        for prefix in prefixes:
            cmd += ["-of", str(prefix)]
        start = time.perf_counter()
        proc = subprocess.Popen(cmd, cwd=exe.parent, stdout=out, stderr=err,
                                stdin=subprocess.PIPE if pipe else subprocess.DEVNULL,
                                creationflags=subprocess.CREATE_NO_WINDOW)
        peak = commit = 0
        try:
            observed = psutil.Process(proc.pid)
            if pipe:
                proc.stdin.write(paths[0].read_bytes())
                proc.stdin.close()
            while proc.poll() is None:
                if time.perf_counter() - start > 70:
                    raise TimeoutError("Whisper exceeded the bounded benchmark timeout")
                try:
                    mem = observed.memory_info()
                    peak = max(peak, mem.peak_wset)
                    commit = max(commit, mem.peak_pagefile)
                except psutil.NoSuchProcess:
                    pass
                time.sleep(.025)
        finally:
            if proc.poll() is None:
                proc.kill()
                proc.wait(timeout=5)
        elapsed = (time.perf_counter() - start) * 1000
        out.seek(0)
        err.seek(0)
        stdout, stderr = (f.read().decode("utf-8", errors="replace") for f in (out, err))
        if proc.returncode or (backend == "cuda" and "found GPU device 0: CUDA0" not in stderr):
            raise RuntimeError(f"{backend} exit {proc.returncode}: {stderr[-1400:]}")
        outputs = []
        for prefix in prefixes:
            payload = json.loads(prefix.with_suffix(".json").read_text(encoding="utf-8"))
            outputs.append("".join(s["text"] for s in payload["transcription"]).strip())
        if pipe and stdout.strip() != outputs[0]:
            # CLI segments can insert line breaks between utterances. Compare words.
            if stdout.split() != outputs[0].split():
                raise RuntimeError("Windows stdout transcription did not match JSON")
    metrics = {key: float(value) for key, value in re.findall(
        r"whisper_print_timings:\s+(load|encode|total) time\s+=\s+([\d.]+) ms", stderr)}
    return dict(backend=backend, clips=[s["id"] for s in clips], pipe=pipe, wall_ms=round(elapsed, 2),
                peak_working_bytes=peak, peak_commit_bytes=commit, outputs=outputs,
                vad_segments=[int(n) for n in re.findall(r"Final speech segments after filtering: (\d+)",stderr)], **metrics)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("cache", type=Path)
    parser.add_argument("results", type=Path)
    args = parser.parse_args()
    root = args.cache.resolve()
    for filename, expected in [(MODEL, MODEL_HASH), (VAD, VAD_HASH)]:
        if sha256(root / filename) != expected:
            raise ValueError(filename + " SHA-256 mismatch")
    samples = corpus(root)
    result = dict(flags=FLAGS, corpus=samples, trials=[])
    trials = result["trials"]
    gpu_path = args.results.with_name("GpuSamples.csv")
    with gpu_path.open("w") as gpu_out:
        gpu_sampler = subprocess.Popen(["nvidia-smi", "--query-gpu=timestamp,memory.used,utilization.gpu",
                                        "--format=csv", "-lms", "100"], stdout=gpu_out,
                                       creationflags=subprocess.CREATE_NO_WINDOW)
        try:
            time.sleep(.5)
            run(root, samples, result, trials)
        finally:
            gpu_sampler.terminate()
            gpu_sampler.wait(timeout=5)
            args.results.write_text(json.dumps(result, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def run(root, samples, result, trials):
        for sample in samples:
            trials.append(invoke(root, "cuda", [sample], sample["language"], pipe=True))
            print(sample["id"], trials[-1]["wall_ms"], repr(trials[-1]["outputs"][0]), flush=True)
        # Repeat with fresh processes, warm filesystem, separate contexts.
        short = [s for s in samples if s["kind"] == "human" and s["seconds"] <= 8]
        for sample in short:
            trials.append(invoke(root, "cuda", [sample], sample["language"], pipe=True))
        for lang in ("en", "zh"):
            group = [s for s in short if s["language"] == lang]
            trials.append(invoke(root, "cuda", group * 2, lang))
            trials.append(invoke(root, "cpu", [group[0]], lang, pipe=True))
        for lang in ("en", "zh"):
            scores = []
            for sample, trial in zip(samples, trials):
                if sample["language"] == lang and sample["kind"] == "human":
                    ref = normalized(sample["reference"], lang)
                    hyp = normalized(trial["outputs"][0], lang)
                    scores.append((edits(ref, hyp), len(ref)))
            result[lang + "_errors"] = [sum(x[i] for x in scores) for i in (0, 1)]
        latency = sorted(t["wall_ms"] for t in trials if t["backend"] == "cuda" and t["clips"] in [[s["id"]] for s in short])
        result["short_cuda_wall_ms"] = dict(n=len(latency), median=statistics.median(latency),
                                            p95=latency[-(len(latency)//20 + 1)], maximum=max(latency))


if __name__ == "__main__":
    main()
