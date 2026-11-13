# Scene wiring and exact shared changes

New scene: `Assets/Scenes/AwakeningLivingGlass.unity`.

The existing Main Camera and Global Light2D remain in the new copy with the existing render setup. Camera clear color is the Living Glass floor. The copied `GestureRecognitionRig` retains its runner, bridge, bootstrap prefab and every serialized recognition setting. There is no second capture rig, additive load or second application controller.

`LivingGlassApplication` has one `AwakeningApplicationController` and one `LivingGlassPresentation`. The controller's new serialized `livingGlass` field references that view. The view references `LivingGlassUI`'s UIDocument and `Assets/LivingGlass/Assets/LivingGlassAssets.asset`. The UIDocument references `Assets/LivingGlass/UI/LivingGlassPanel.asset`, with1920×1080 reference sizing and a dedicated TextCore configuration/fallback. The new scene has no legacy Home controller or document manager. Those references are intentionally null; Awake selects the explicit new presentation branch first.

The six clip slots remain GAIA, PANDEMIC, RE-BIRTH, WHITE-GUARDIAN, CONSCIENCE, SANCTUARY with identical GUIDs/order. VideoPlayer remains the existing lazy single-owner operation on the application object; the scene does not add a decorative player. Automatic missing-video fallback remains enabled with the existing10-second configuration. No video importer, codec, package or media asset changed.

The asset configuration references the existing approved, Activation and Living artwork for all six performers. Original artwork is not copied or reimported. Source Sans3 / Source Serif4 are reused; a new SIL OFL Noto Sans CJK TC Regular asset supplies Traditional Chinese and fallback glyphs. See its `OFL.txt` and `Source.json`. TC display uses the bundled sans face as an explicit readable baseline; the Stage2 proposal for a TC serif face is not silently claimed implemented.

Exactly three existing shared source files changed:

| File | Change | Old-scene compatibility |
| --- | --- | --- |
| `Assets/Scripts/AwakeningApplicationController.cs` | Partial declaration; optional serialized presentation reference and guarded dispatch; content-root selection for fades; optional Cancel button; guarded new assistant view-active check | Absent reference preserves old imported binding path, field names and serialized GUID; optional Cancel remains null |
| `Assets/Scripts/Interaction/MediaPipeGestureBridge.cs` | Read-only `HasCameraFailure` forwarded from the existing runner | No recognition, camera ownership or threshold change |
| `Assets/Samples/MediaPipe Unity Plugin/0.16.3/Official Solutions/Scenes/Hand Landmark Detection/HandLandmarkerRunner.cs` | Read-only failure type assigned at existing bootstrap/pipeline failures and cleared at existing Play | No extra retry, capture start, consent grant or timing change |

The controller's implementation continues in `Assets/LivingGlass/Integration/LivingGlassFlowIntegration.cs`; it is the same class/authority, not a second MonoBehaviour controller. The original component script's `.meta` GUID remains unchanged. New files have Unity-generated metadata.

Settings/Privacy suspend capture. Privacy now offers an explicit session-only permission choice in the new view; checking it does not start capture on that screen. Closing into an eligible experience can resume through the existing gate. Revocation is immediate. There is no persistence of camera consent. The original scene's Home UI is unchanged.

Static evidence: `BaselineHashes.json`, `BaselineAdditionalHashes.json`, `SerializedValidation.txt`, `StaticVerification.json`, `PureContractChecks.txt`. Original scene/build/package byte hashes match. Runtime parity for both scenes is pending; static preservation is not represented as a passed operational regression test.
