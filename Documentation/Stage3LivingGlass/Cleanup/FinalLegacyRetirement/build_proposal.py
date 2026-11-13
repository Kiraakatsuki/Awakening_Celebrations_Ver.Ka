"""Stage syntax-aware, named-member edits outside Assets before applying them.
Member/body spans come from the installed Roslyn parser, not brace/regex deletion.
"""
from pathlib import Path
import json

OUT = Path(__file__).resolve().parent
ROOT = OUT.parents[3]
nodes = json.loads((OUT / "SourceMembersBefore.json").read_text(encoding="utf-8-sig"))
main = "Assets/Scripts/AwakeningApplicationController.cs"
port = "Assets/LivingGlass/Integration/LivingGlassFlowIntegration.cs"
removed_methods = set("SetCameraPreviewStatus BindHandTrackingStatus UpdateHandTrackingStatus OnLanguageChanged OnConsentChanged OnPrivacyRequested OnSettingsRequested OnReviewConsentRequested ReturnHomeFromSettings BindAssistantPerformerChoices BindColorReleasePerformerChoices SetLabelTextAtPath RegisterTargetHover RegisterButtonHoverProxy OpenAssistantOverlay ConfigureAssistantChat ConfigurePerformerAgentPage CloseTopOverlay GetUxmlRoot FocusFirstFocusable BindPath FindManifestNode ApplyInteractionStyles ApplyInteractionStylesTo RegisterClick RegisterPageClick DisableSettingsControls MarkUnavailable RegisterKeyOnRoot RegisterKeyHandling TryResolvePath ResolveRuntimeElementPath ArrayIndexOf".split())
removed_methods.update({"UpdateCameraPreview", "BindCurrentScreen", "PrepareColorReleaseSelection", "ReturnHomeFromDone"})
removed_fields = set("screenDocumentManager screenCatalog homeController screenUnbind overlays overlayUnbind activeBindingScreen colorReleaseRoot cameraPreviewFrame cameraPreviewImage cameraPreviewDocumentRoot cameraPreviewStatus nodeSnapshot interactionStyles handTrackingStatus PerformerCodes".split())
removed_classes = {"OverlayState", "ManifestSnapshot", "ManifestScreen", "ManifestNode"}
bodies = {
"Awake": '''{
        gestureCursor = new GestureCursorController(gestureCursorSmoothingSpeed);
        GestureCameraConsentGate.SetConsent(false);
        if (livingGlass == null || livingGlass.Document == null)
        {
            Debug.LogError("Living Glass presentation and UIDocument must be assigned.", this);
            enabled = false;
            return;
        }
        navigationDocument = livingGlass.Document;
    }''',
"OnEnable": "{\n        TryConnectGestureBridge();\n    }",
"Start": '''{
        yield return null;
        livingGlass.Initialize(this);
        language = PlayerPrefs.GetString("AwakeningCelebrations.Language", "en") == "zh" ? AppLanguage.Chinese : AppLanguage.English;
        LivingGlassRender();
    }''',
"UpdateCameraPreview": "{\n        LivingGlassUpdateStatus();\n    }",
"ShowHome": "{\n        LivingGlassHome(false);\n    }",
"ReturnHomeFromDone": "{\n        LivingGlassHome(true);\n    }",
"BindCurrentScreen": "{\n        LivingGlassRender();\n    }",
"PrepareColorReleaseSelection": "{\n        LivingGlassRender();\n    }",
"SelectPerformer": "{\n        selectedPerformer = index;\n        Navigate(GetPerformerScreen(index, 0), language, true);\n    }",
"IsAssistantSessionViewActive": '''{
        return session != null && livingGlass != null && livingGlass.IsAssistantActive(session.root)
            && selectedPerformer == session.performerIndex;
    }''',
"GetPerformerScreen": "{\n        return LivingGlassRoutes.Chapter(performer, stage);\n    }",
"IsPerformerStage": "{\n        return LivingGlassRoutes.TryChapter(screen, out performer, out stage);\n    }",
}
changes = []
for path in [main, port]:
    source = (OUT / "Before" / path).read_bytes().decode("utf-8-sig")
    newline = "\r\n" if "\r\n" in source else "\n"
    assert not any(ord(c) > 65535 for c in source), "UTF16 span conversion required"
    edits = []
    for node in (n for n in nodes if n["path"] == path):
        name, kind = node["name"], node["kind"]
        if path == main and ((kind == "MethodDeclarationSyntax" and name in removed_methods)
            or (kind == "FieldDeclarationSyntax" and node["parent"] == "AwakeningApplicationController" and name in removed_fields)
            or (kind == "FieldDeclarationSyntax" and node["parent"] == "AssistantSession" and name in {"assistantScreenId", "placeholder"})
            or (kind == "ClassDeclarationSyntax" and name in removed_classes)):
            edits.append((node["fullStart"], node["start"]+node["length"], "")); changes.append(dict(path=path, action="remove legacy member", kind=kind, name=name)); continue
        if path == main and kind == "MethodDeclarationSyntax" and name in bodies:
            edits.append((node["bodyStart"], node["bodyStart"]+node["bodyLength"], bodies[name])); changes.append(dict(path=path, action="retain Living Glass branch / pure route authority", name=name)); continue
        if path == main and kind == "FieldDeclarationSyntax" and name in {"TutorialScreens", "Performers", "Stages"}:
            shared = {"TutorialScreens":"Tutorials", "Performers":"Performers", "Stages":"Stages"}[name]
            edits.append((node["start"],node["start"]+node["length"],"private static readonly string[] "+name+" = LivingGlassRoutes."+shared+";"))
    for start,end,replacement in sorted(edits, reverse=True): source = source[:start]+replacement.replace("\n",newline)+source[end:]
    if path == main:
        # Exact statement substitutions within retained methods; no generic rewrite.
        exact = {
            "        UpdateHandTrackingStatus();\n":"",
            "        cameraPreviewFrame?.RemoveFromHierarchy();\n        if (cameraPreviewImage != null)\n            cameraPreviewImage.image = null;\n        cameraPreviewFrame = null;\n        cameraPreviewImage = null;\n        cameraPreviewDocumentRoot = null;\n        cameraPreviewStatus = null;\n":"",
            "        HomeLanguageController.LanguageChanged -= OnLanguageChanged;\n        HomeLanguageController.PrivacyRequested -= OnPrivacyRequested;\n        HomeLanguageController.SettingsRequested -= OnSettingsRequested;\n        HomeLanguageController.ReviewConsentRequested -= OnReviewConsentRequested;\n        HomeLanguageController.ConsentChanged -= OnConsentChanged;\n":"",
            "        ClearBindings(screenUnbind);\n":"",
            "        while (overlays.Count > 0)\n            CloseTopOverlay();\n":"",
            "                    overlays.Count == 0 && gestureTarget >= 0 &&":"                    gestureTarget >= 0 &&",
            "            overlays.Count > 0 || performerTargets == null)":"            performerTargets == null)",
            "ArrayIndexOf(TutorialScreens, screenId)":"Array.IndexOf(TutorialScreens, screenId)",
            "        VisualTreeAsset asset;\n        asset = null;\n        if (livingGlass != null ? !LivingGlassRoutes.Supports(screenId) : (screenCatalog == null || !screenCatalog.TryGet(screenId, targetLanguage, out asset)))":"        if (!LivingGlassRoutes.Supports(screenId))",
            "        if (navigationDocument == null && screenDocumentManager == null)":"        if (navigationDocument == null)",
            "        colorReleaseRoot = null;\n":"",
            '''        if (livingGlass != null)
        {
            // Render after flow state is committed; the view may retain an inert art proxy.
        }
        else if (screenDocumentManager != null)
            navigationDocument = screenDocumentManager.Activate(screenId, asset);
        else
        {
            navigationDocument.enabled = false;
            navigationDocument.visualTreeAsset = asset;
            navigationDocument.enabled = true;
        }
''':"",
            "        ApplyInteractionStyles();\n":"",
            "        activeBindingScreen = screenId;\n":"",
            "        if (homeController != null)\n            homeController.SetHomeVisible(false);\n":"",
            "        if (overlays.Count > 0)\n        {\n            CloseTopOverlay();\n            return;\n        }\n":"",
            "SelectPerformer(targetedPerformer, false)":"SelectPerformer(targetedPerformer)",
            "private void SelectPerformer(int index, bool openAssistant)":"private void SelectPerformer(int index)",
            "UpdateCameraPreview();":"LivingGlassUpdateStatus();",
            "BindCurrentScreen();":"LivingGlassRender();",
            "PrepareColorReleaseSelection();":"LivingGlassRender();",
        }
        for before,after in exact.items():
            before, after = before.replace("\n",newline), after.replace("\n",newline)
            assert before in source, before
            source = source.replace(before,after)
        source = source.replace("The imported Color Release screen has no completion event or duration; this configurable presentation interval is a provisional default.", "Presentation interval before gallery selection becomes ready; preserves the existing session timing.")
    else:
        source = source.replace("Compatibility port into the single existing flow/service authority.", "Living Glass command port into the single flow/service authority.")
        source = source.replace("livingGlass != null ? livingGlass.Content : navigationDocument?.rootVisualElement", "livingGlass != null ? livingGlass.Content : null")
        source = source.replace("        colorReleaseRoot = currentScreen == ColorReleaseScreen ? root : null;"+newline, "")
        source = source.replace("        ClearBindings(screenUnbind);"+newline, "")
        source = source.replace("currentScreen = activeBindingScreen = HomeScreen;", "currentScreen = HomeScreen;")
        source = source.replace("performerId = Performers[performer], assistantScreenId = screen,", "performerId = Performers[performer],")
        source = source.replace("ReturnHomeFromDone();", "LivingGlassHome(true);")
    dest = OUT / "Proposed" / path; dest.parent.mkdir(parents=True,exist_ok=True);dest.write_bytes(source.encode("utf-8"))
(OUT / "SourceRefactorPlan.json").write_text(json.dumps(changes,indent=2),encoding="utf-8")
print(json.dumps({"planned_named_member_changes":len(changes),"proposed_files":[main,port]}))
