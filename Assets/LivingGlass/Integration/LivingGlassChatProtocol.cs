using System;
using System.Text.RegularExpressions;
using UnityEngine;
using Newtonsoft.Json;

internal enum LivingGlassChatMode { Legacy, Contextual }

internal static class LivingGlassChatProtocol
{
    internal const string GeneralOverlay = "AssistantGeneral";
    private static readonly string[] CanonicalIds =
    {
        "JFC26-GAIA", "JFC26-PANDEMIC", "JFC26-REBIRTH",
        "JFC26-WGUARDIAN", "JFC26-CONSCIENCE", "JFC26-SANCTUARY"
    };
    private static readonly JsonSerializerSettings ContextJsonSettings = new JsonSerializerSettings
        { NullValueHandling = NullValueHandling.Ignore };

    internal static string DevelopmentBaseUrl
    {
        get
        {
#if UNITY_EDITOR
            return LoopbackBaseUrl(Environment.GetEnvironmentVariable("AWAKENING_CONTEXT_CHAT_URL"));
#else
            return null;
#endif
        }
    }

    internal static string ContextualOrigin(LivingGlassChatMode mode, string configuredOrigin, string editorLoopback) =>
        editorLoopback ?? (mode == LivingGlassChatMode.Contextual ? HttpsOrigin(configuredOrigin) : null);

    internal static string HttpsOrigin(string value)
    {
        if (!Uri.TryCreate(value?.Trim(), UriKind.Absolute, out Uri uri) ||
            uri.Scheme != Uri.UriSchemeHttps || uri.Host.Length == 0 ||
            uri.AbsolutePath != "/" || uri.UserInfo.Length != 0 || uri.Query.Length != 0 || uri.Fragment.Length != 0)
            return null;
        return uri.GetLeftPart(UriPartial.Authority);
    }

    internal static string LoopbackBaseUrl(string value)
    {
        string origin = HttpsOrigin(value);
        if (origin == null) return null;
        string host = new Uri(origin).Host;
        return host == "127.0.0.1" || host == "[::1]" ? origin : null;
    }

    internal static bool IsSessionToken(string value) =>
        value != null && Regex.IsMatch(value, @"\A[A-Za-z0-9_-]{43}\z");

    internal static string ContextBody(string message, AppLanguage language, string token, int performer, bool fromCommonChooser)
    {
        var request = new ContextRequest { message = message, locale = language == AppLanguage.Chinese ? "zh-TW" : "en", sessionId = token };
        if (performer >= 0 && performer < CanonicalIds.Length)
        {
            string canonical = CanonicalIds[performer];
            request.context = new Context
            {
                source = fromCommonChooser ? "common-ai" : "performer-page",
                performerId = canonical,
                imageId = fromCommonChooser ? canonical + "-IMG01" : null
            };
        }
        // JsonUtility may serialize an absent nested object as a default object.
        // The general conversation has an explicit common source and no selected IDs.
        else request.context = new Context { source = "common-ai" };
        return JsonConvert.SerializeObject(request, ContextJsonSettings);
    }

    internal static string LegacyBody(string message) => JsonUtility.ToJson(new LegacyRequest { message = message });
    internal static string TokenBody(string token) => JsonUtility.ToJson(new TokenRequest { sessionId = token });
    internal static string TokenFromResponse(string json)
    {
        try
        {
            string value = JsonUtility.FromJson<TokenRequest>(json)?.sessionId;
            return IsSessionToken(value) ? value : null;
        }
        catch (ArgumentException) { return null; }
    }

    [Serializable] private sealed class LegacyRequest { public string message; }
    [Serializable] private sealed class TokenRequest { public string sessionId; }
    [Serializable] private sealed class ContextRequest { public string message, locale, sessionId; public Context context; }
    [Serializable] private sealed class Context { public string source, performerId, imageId; }
}
