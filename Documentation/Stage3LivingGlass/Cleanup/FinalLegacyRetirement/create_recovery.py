"""Non-destructive archival Git ref. Does not change HEAD, branch or real index.
Raw blobs retain line endings; excludes backend secrets/unrelated user documents.
Includes all Assets (including ignored recovery files), Packages, ProjectSettings
and the manifest's local MediaPipe tarball as an archival external dependency.
"""
from pathlib import Path
import datetime
import hashlib
import json
import os
import subprocess

OUT = Path(__file__).resolve().parent
ROOT = OUT.parents[3]

def git(*args, data=None, env=None):
    return subprocess.check_output(["git", *args], cwd=ROOT, input=data, env=env)

stamp = datetime.datetime.now(datetime.timezone.utc).strftime("%Y%m%dT%H%M%SZ")
archive = Path("D:/Coding/Unity/Archives/AwakeningCelebrations") / ("PreStage34-" + stamp)
archive.mkdir(parents=True, exist_ok=False)
head = git("rev-parse", "HEAD").decode().strip()
branch = git("branch", "--show-current").decode().strip()
staged = git("diff", "--cached", "--binary")
(OUT / "BaselineGitStatus.txt").write_bytes(git("status", "--porcelain=v1", "--untracked-files=all"))
(OUT / "BaselineWorkingChanges.patch").write_bytes(git("diff", "--binary"))
files = sorted(p for top in ["Assets", "Packages", "ProjectSettings"] for p in (ROOT / top).rglob("*")
               if p.is_file() and not any(x in {".git", "node_modules", "__pycache__"} for x in p.parts))
paths = [p.relative_to(ROOT).as_posix() for p in files]
hashes = {name: hashlib.sha256(p.read_bytes()).hexdigest() for name, p in zip(paths, files)}
(OUT / "BaselineHashes.json").write_text(json.dumps(hashes, indent=2), encoding="utf-8")
env = dict(os.environ, GIT_INDEX_FILE=str(archive / "snapshot.index"))
git("read-tree", head, env=env)
blob_ids = git("hash-object", "-w", "--no-filters", "--stdin-paths", data=("\n".join(paths)+"\n").encode("utf-8")).decode().splitlines()
assert len(blob_ids) == len(paths)
modes = {}
for entry in git("ls-files", "--stage", "-z").split(b"\0"):
    if entry:
        meta, name = entry.split(b"\t", 1)
        modes[name.decode("utf-8")] = meta.split()[0].decode()
index_data = b"".join((modes.get(p, "100644")+" "+oid+"\t"+p).encode("utf-8")+b"\0" for p, oid in zip(paths, blob_ids))
git("update-index", "-z", "--index-info", data=index_data, env=env)
external = Path("C:/Users/Nakul J S/Downloads/com.github.homuler.mediapipe-0.16.3.tgz")
external_oid = git("hash-object", "-w", "--no-filters", str(external)).decode().strip()
external_path = "RecoveryExternal/com.github.homuler.mediapipe-0.16.3.tgz"
git("update-index", "-z", "--index-info", data=("100644 "+external_oid+"\t"+external_path).encode()+b"\0", env=env)
tree = git("write-tree", env=env).decode().strip()
message = ("Archive pre-Stage3.4 demo and shared runtime for recovery\n\n"
           "Separate archival ref only; main branch/index/worktree unchanged.\n"
           "Includes current modified/untracked application dependencies as recovery material.\n"
           "Backend secrets and unrelated untracked user documents are not added.\n").encode()
commit = git("-c", "user.name=Codex Recovery Archive", "-c", "user.email=codex-recovery@local.invalid",
             "commit-tree", tree, "-p", head, data=message).decode().strip()
ref = "refs/archive/awakening-demo-pre-stage34-" + stamp.lower()
git("update-ref", ref, commit, "0"*40)
# Read back every archived blob and compare its raw SHA-256, not just its Git ID.
reader = subprocess.Popen(["git", "cat-file", "--batch"], cwd=ROOT, stdin=subprocess.PIPE, stdout=subprocess.PIPE)
for name, oid in list(zip(paths, blob_ids)) + [(external_path, external_oid)]:
    reader.stdin.write((oid+"\n").encode()); reader.stdin.flush()
    header = reader.stdout.readline().split(); size = int(header[2]); digest = hashlib.sha256()
    while size:
        chunk = reader.stdout.read(min(size, 1048576)); assert chunk
        digest.update(chunk); size -= len(chunk)
    assert reader.stdout.read(1) == b"\n"
    expected = hashes[name] if name in hashes else hashlib.sha256(external.read_bytes()).hexdigest()
    assert digest.hexdigest() == expected, name
reader.stdin.close(); assert reader.wait() == 0
assert git("rev-parse", "HEAD").decode().strip() == head
assert git("diff", "--cached", "--binary") == staged
assert all(hashlib.sha256((ROOT/p).read_bytes()).hexdigest() == digest for p, digest in hashes.items())
bundle = archive / "AwakeningDemo-PreStage34.bundle"
git("bundle", "create", str(bundle), ref)
verification = git("bundle", "verify", str(bundle)).decode(errors="replace")
result = dict(head=head, branch=branch, recovery_ref=ref, recovery_commit=commit, tree=tree,
              files=len(paths), raw_blob_verification="PASS", main_index_and_head_preserved=True,
              external_package=dict(path=external_path, original_path=str(external), blob=external_oid),
              bundle=str(bundle), bundle_sha256=hashlib.sha256(bundle.read_bytes()).hexdigest(), bundle_verification=verification)
(archive / "RecoveryManifest.json").write_text(json.dumps(result, indent=2), encoding="utf-8")
(OUT / "RecoveryManifest.json").write_text(json.dumps(result, indent=2), encoding="utf-8")
print(json.dumps(result))
