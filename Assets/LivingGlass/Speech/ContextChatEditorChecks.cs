#if UNITY_EDITOR
using System;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine.Networking;

// Pure Edit Mode serialization checks. The transport fixture is run separately.
internal static class ContextChatEditorChecks
{
    [MenuItem("Awakening Celebrations/Living Glass/Check contextual chat contract")]
    internal static void Run()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Edit Mode only.");
        string result = Verify();
        UnityEngine.Debug.Log(result);
    }

    internal static string Verify()
    {
        string token = new string('a', 43);
        string[] ids = { "GAIA", "PANDEMIC", "REBIRTH", "WGUARDIAN", "CONSCIENCE", "SANCTUARY" };
        for (int index = 0; index < ids.Length; index++)
        {
            string expected = "JFC26-" + ids[index];
            foreach (var language in new[] { AppLanguage.English, AppLanguage.Chinese })
            {
                foreach (bool chooser in new[] { false, true })
                {
                    var json = JObject.Parse(LivingGlassChatProtocol.ContextBody("Fixture", language, token, index, chooser));
                    var context = (JObject)json["context"];
                    Need((string)json["message"] == "Fixture" && (string)json["sessionId"] == token, "request envelope");
                    Need((string)json["locale"] == (language == AppLanguage.English ? "en" : "zh-TW"), "locale");
                    Need((string)context["source"] == (chooser ? "common-ai" : "performer-page"), "surface");
                    Need((string)context["performerId"] == expected, "canonical performer");
                    Need((string)context["imageId"] == (chooser ? expected + "-IMG01" : null), "portrait mapping");
                }
            }
        }
        var general = (JObject)JObject.Parse(LivingGlassChatProtocol.ContextBody("Fixture", AppLanguage.Chinese, token, -1, true))["context"];
        Need(general.Count == 1 && (string)general["source"] == "common-ai", "general context omits IDs");
        var legacy = JObject.Parse(LivingGlassChatProtocol.LegacyBody("Only the question"));
        Need(legacy.Count == 1 && (string)legacy["message"] == "Only the question", "fallback data minimization");
        Need(LivingGlassChatProtocol.TokenFromResponse(LivingGlassChatProtocol.TokenBody(token)) == token, "server token");
        Need(LivingGlassChatProtocol.TokenFromResponse("{\"sessionId\":\"guessable\"}") == null, "invalid token");
        Need(LivingGlassChatProtocol.LoopbackBaseUrl("https://127.0.0.1:8743") == "https://127.0.0.1:8743", "loopback URL");
        foreach (string rejected in new[] { "http://127.0.0.1:8743", "https://example.com", "https://127.0.0.1/a", "https://user@127.0.0.1", "https://127.0.0.1/?key=1" })
            Need(LivingGlassChatProtocol.LoopbackBaseUrl(rejected) == null, "unsafe development URL");
        foreach (long status in new long[] {400, 401, 409, 410, 413, 429, 500})
            Need(!LivingGlassChatProtocol.EligibleForFallback(UnityWebRequest.Result.ProtocolError, status), "validation must not fall back");
        foreach (long status in new long[] {502, 503, 504})
            Need(LivingGlassChatProtocol.EligibleForFallback(UnityWebRequest.Result.ProtocolError, status), "service fallback");
        Need(LivingGlassChatProtocol.EligibleForFallback(UnityWebRequest.Result.ConnectionError, 0), "connection fallback");
        Need(!LivingGlassChatProtocol.EligibleForFallback(UnityWebRequest.Result.DataProcessingError, 200), "protocol failure must not fall back");
        return "PASS 24 bilingual dedicated/portrait payloads, general context, token parsing, HTTPS loopback and fallback policy.";
    }

    private static void Need(bool condition, string label)
    {
        if (!condition) throw new InvalidOperationException("Context chat contract: " + label);
    }
}
#endif
