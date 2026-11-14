using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

internal static class TraditionalSpeechText
{
    // A longest-first pass over Han-only dictionary entries. No locale vocabulary
    // replacements, and no edits to punctuation, Latin text or manually typed input.
    public static string Convert(string input, string scriptDirectory)
    {
        var entries = new Dictionary<string, string>(StringComparer.Ordinal);
        int longest = 0;
        foreach (string name in new[] { "STCharacters.txt", "STPhrases.txt" })
        {
            foreach (string line in File.ReadLines(Path.Combine(scriptDirectory, name), Encoding.UTF8))
            {
                int tab = line.IndexOf('\t');
                if (tab < 1) continue;
                string key = line.Substring(0, tab);
                if (!IsHan(key)) continue;
                int end = line.IndexOf(' ', tab + 1);
                if (end < 0) end = line.Length;
                if (end <= tab + 1) continue;
                entries[key] = line.Substring(tab + 1, end - tab - 1);
                longest = Math.Max(longest, key.Length);
            }
        }
        var text = new StringBuilder(input.Length);
        for (int i = 0; i < input.Length;)
        {
            if (!IsHan(input[i]))
            {
                text.Append(input[i++]);
                continue;
            }
            int length = Math.Min(longest, input.Length - i);
            string replacement = null;
            while (length > 0 && !entries.TryGetValue(input.Substring(i, length), out replacement)) length--;
            if (length == 0) text.Append(input[i++]);
            else
            {
                text.Append(replacement);
                i += length;
            }
        }
        return text.ToString();
    }

    private static bool IsHan(string value)
    {
        foreach (char c in value) if (!IsHan(c)) return false;
        return true;
    }

    private static bool IsHan(char c) => (c >= '\u3400' && c <= '\u4dbf') || (c >= '\u4e00' && c <= '\u9fff');
}
