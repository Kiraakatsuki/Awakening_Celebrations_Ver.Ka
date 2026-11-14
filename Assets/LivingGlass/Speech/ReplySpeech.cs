using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

internal enum ReplySpeechError { None, Unavailable, Busy, Timeout, Cancelled, Startup, Worker, InvalidAudio, TooLong, Incomplete }

internal readonly struct ReplySpeechResult
{
    public readonly ReplySpeechError Error;
    public readonly float[] Samples;
    public ReplySpeechResult(ReplySpeechError error, float[] samples = null) { Error = error; Samples = samples; }
}

internal interface IReplySpeech
{
    bool IsAvailable { get; }
    Task<ReplySpeechResult> GenerateAsync(string reply, string locale, CancellationToken cancellation);
}

internal static class ReplySpeechText
{
    internal const int MaximumLength = 2400;
    // The existing assistant answers in the question's language, not necessarily the UI locale.
    internal static string Language(string line, string locale)
    {
        int han = 0, latin = 0;
        foreach (char c in line)
        {
            if ((c >= '\u3400' && c <= '\u9fff') || (c >= '\uf900' && c <= '\ufaff')) han++;
            else if ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z')) latin++;
        }
        if (han > 0) return latin > 0 ? "mixed" : "zh";
        return latin > 0 ? "en" : locale;
    }

    internal static List<string> Split(string reply, int limit = 145)
    {
        if (string.IsNullOrWhiteSpace(reply) || reply.Length > MaximumLength || reply.IndexOf('\0') >= 0)
            return null;
        const string ends = ".!?。？！；";
        var parts = new List<string>();
        var elements = StringInfo.GetTextElementEnumerator(reply.Trim());
        var text = new StringBuilder();
        int softBreak = 0, sentenceBreak = 0;
        while (elements.MoveNext())
        {
            string item = elements.GetTextElement();
            if (item.Length > limit) return null;
            if (text.Length + item.Length > limit)
            {
                int split = sentenceBreak > 0 ? sentenceBreak : softBreak > 0 ? softBreak : text.Length;
                string part = text.ToString(0, split).Trim();
                if (part.Length > 0) parts.Add(part);
                text.Remove(0, split);
                softBreak = Math.Max(0, softBreak - split);
                sentenceBreak = Math.Max(0, sentenceBreak - split);
                if (text.Length + item.Length > limit)
                {
                    parts.Add(text.ToString().Trim());
                    text.Clear();
                    softBreak = sentenceBreak = 0;
                }
            }
            bool space = string.IsNullOrWhiteSpace(item);
            if (!space || (text.Length > 0 && text[text.Length - 1] != ' ')) text.Append(space ? " " : item);
            if (space) softBreak = text.Length;
            else if (ends.IndexOf(item[0]) >= 0) sentenceBreak = text.Length;
        }
        string last = text.ToString().Trim();
        if (last.Length != 0) parts.Add(last);
        return parts.Count > 0 && parts.Count <= 40 ? parts : null;
    }
}

internal static class ReplySpeechWave
{
    internal const int Rate = 24000;
    internal const int MaximumFrames = Rate * 180;
    private const int MaximumPartFrames = Rate * 40;
    internal const int MaximumLine = 2_600_000;
    // The pinned worker yields exactly 1,920 samples per codec frame and
    // caps output at 450 frames. A full cap is not evidence of natural EOS.
    internal static bool ReachedTokenLimit(int frames) => frames >= 450 * 1920;

    internal static float[] FinishPhrase(float[] speech, bool final)
    {
        const int edge = 192; // Bridge only after the last source sample.
        int silence = final ? 5760 : 2880;
        var result = new float[speech.Length + edge + silence];
        Array.Copy(speech, result, speech.Length);
        float last = speech[speech.Length - 1];
        for (int i = 0; i < edge; i++)
            result[speech.Length + i] = last * (1f - (i + 1f) / edge);
        return result;
    }

    public static float[] Decode(string base64)
    {
        if (string.IsNullOrEmpty(base64) || base64.Length > ((MaximumPartFrames * 2 + 46) / 3) * 4)
            return null;
        byte[] b;
        try { b = Convert.FromBase64String(base64); }
        catch (FormatException) { return null; }

        // Exact PCM16 mono header emitted by the pinned worker's WAV writer.
        if (b.Length < 46 || Read(b, 0) != 0x46464952 || Read(b, 4) != b.Length - 8 ||
            Read(b, 8) != 0x45564157 || Read(b, 12) != 0x20746d66 || Read(b, 16) != 16 ||
            Read(b, 20) != 0x00010001 || Read(b, 24) != Rate || Read(b, 28) != Rate * 2 ||
            Read(b, 32) != 0x00100002 || Read(b, 36) != 0x61746164 || Read(b, 40) != b.Length - 44 ||
            (b.Length & 1) != 0 || (b.Length - 44) / 2 > MaximumPartFrames)
            return null;

        var pcm = new float[(b.Length - 44) / 2];
        for (int i = 0, p = 44; i < pcm.Length; i++, p += 2)
            pcm[i] = unchecked((short)(b[p] | b[p + 1] << 8)) / 32768f;
        return pcm;
    }

    private static int Read(byte[] bytes, int offset) =>
        bytes[offset] | bytes[offset + 1] << 8 | bytes[offset + 2] << 16 | bytes[offset + 3] << 24;
}

// External process ownership only. STT keeps its existing bounded/serialized workflow;
// TTS never queues behind it and the explicit mic action cancels Qwen before capture.
internal static class LocalVoiceInference
{
    internal static readonly SemaphoreSlim Slot = new SemaphoreSlim(1, 1);
}
