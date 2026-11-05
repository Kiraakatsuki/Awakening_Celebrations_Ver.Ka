using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>Editor-only capture helpers for the Phase 4 runtime regression evidence.</summary>
internal static class Phase4RuntimeScreenshotCapture
{
    private const string OutputDirectory = "Documentation/FigmaUnityContactSheets/Phase4";

    [MenuItem("Tools/Phase 4 Capture/Home EN")]
    private static void HomeEnglish() { Capture("home-en"); }

    [MenuItem("Tools/Phase 4 Capture/Privacy EN")]
    private static void PrivacyEnglish() { Capture("privacy-en"); }

    [MenuItem("Tools/Phase 4 Capture/Home ZH")]
    private static void HomeChinese() { Capture("home-zh"); }

    [MenuItem("Tools/Phase 4 Capture/Privacy ZH")]
    private static void PrivacyChinese() { Capture("privacy-zh"); }

    [MenuItem("Tools/Phase 4 Capture/Settings EN")]
    private static void SettingsEnglish() { Capture("settings-en"); }

    [MenuItem("Tools/Phase 4 Capture/Settings ZH")]
    private static void SettingsChinese() { Capture("settings-zh"); }

    [MenuItem("Tools/Phase 4 Capture/Assistant Chooser EN")]
    private static void AssistantChooserEnglish() { Capture("assistant-chooser-en"); }

    [MenuItem("Tools/Phase 4 Capture/Assistant Chooser ZH")]
    private static void AssistantChooserChinese() { Capture("assistant-chooser-zh"); }

    [MenuItem("Tools/Phase 4 Capture/Assistant Overlay EN")]
    private static void AssistantOverlayEnglish() { Capture("assistant-overlay-en"); }

    [MenuItem("Tools/Phase 4 Capture/Assistant Overlay ZH")]
    private static void AssistantOverlayChinese() { Capture("assistant-overlay-zh"); }

    private static void Capture(string name)
    {
        if (!EditorApplication.isPlaying)
        {
            Debug.LogWarning("Phase 4 captures require Play Mode so the screenshot reflects the rendered runtime UI.");
            return;
        }

        string directory = Path.GetFullPath(Path.Combine(Application.dataPath, "..", OutputDirectory));
        Directory.CreateDirectory(directory);
        string path = Path.Combine(directory, name + ".png");
        ScreenCapture.CaptureScreenshot(path);
        Debug.Log("Phase 4 runtime screenshot queued: " + path);
    }
}
