using System;

/// <summary>Pure route classification. Catalog identifiers remain the compatibility contract.</summary>
public static class LivingGlassRoutes
{
    public static readonly string[] Performers = { "GAIA", "PANDEMIC", "RE-BIRTH", "WHITE-GUARDIAN", "CONSCIENCE", "SANCTUARY" };
    public static readonly string[] Stages = { "FOCUS-VIEW", "ACTIVATION", "LIVING-PORTRAIT", "RELEASE", "POST-VIDEO-AI" };
    public static readonly string[] Tutorials = { "TUTORIAL---ENTRY---DIM", "TUTORIAL---INTRODUCTION", "TUTORIAL---LOOK---MOVE", "TUTORIAL---POINT", "TUTORIAL---HOLD---SELECT", "TUTORIAL---EXPERIENCE", "TUTORIAL---READY" };

    public static bool TryChapter(string route, out int performer, out int stage)
    {
        for (int p = 0; p < Performers.Length; p++)
            for (int s = 0; s < Stages.Length; s++)
                if (route == Chapter(p, s)) { performer = p; stage = s; return true; }
        performer = stage = -1;
        return false;
    }

    public static string Chapter(int performer, int stage)
    {
        return Performers[performer] + "---" + Stages[stage] + "---PER-00" + (performer + 1);
    }

    public static string Family(string route)
    {
        if (TryChapter(route, out _, out int stage)) return Stages[stage];
        if (Array.IndexOf(Tutorials, route) >= 0 || route == "TUTORIAL---RETURN---DISMISS") return "Tutorial";
        if (route != null && route.StartsWith("Assistant-", StringComparison.Ordinal) && Array.IndexOf(Performers, route.Substring(10)) >= 0) return "Assistant";
        switch (route)
        {
            case "Home": case "Privacy": case "AssistantChooser": return route;
            case "ATTRACT---MONOCHROME---FALLBACK-INPUT": return "Attract";
            case "COLOR-RELEASE": return "Gallery";
            case "SETTINGS---EXPERIENCE-CONTROLS": return "Settings";
            case "CLOSING---CELEBRATION": return "Closing";
            case "SOUVENIR---QR-DELIVERY": return "Souvenir";
            default: return null;
        }
    }
    public static bool Supports(string route) { return Family(route) != null; }
    public static bool Protected(string route) { return TryChapter(route, out _, out int s) && (s == 1 || s == 2); }
}
