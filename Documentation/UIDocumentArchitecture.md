# UIDocument Architecture

**Status:** Implemented in the scene and source. Phase 3 previously verified localized Home/Privacy routes. Phase 4 has a persisted Home EN screenshot pair; the current automation run could not navigate from Home, so Phase 4 route recapture and the remaining families/overlay still need runtime regression.
**Main scene:** `Assets/Scenes/SampleScene.unity`

## Design goals

Give each independently navigable page family its own UIDocument while preserving the existing UXML assets, catalog IDs, localized variants, controller behavior, and scene. Different performers, tutorial steps, locales, and lifecycle stages remain content variants loaded into their family document; they do not multiply GameObjects. The design is a small document router, not a replacement navigation framework.

## Target document map

| UIDocument GameObject | UXML source(s) | Sorting layer | State owner |
|---|---|---:|---|
| `HomeScreen_EN` | English Home (fixed) | 0 | HomeLanguageController |
| `HomeScreen_ZH` | Traditional Chinese Home (fixed) | 0 | HomeLanguageController |
| `PrivacyScreen` | Privacy EN / ZH | 10 | AwakeningApplicationController |
| `TutorialScreen` | 8 steps × 2 locales | 10 | AwakeningApplicationController |
| `AttractScreen` | EN / ZH fallback | 10 | AwakeningApplicationController + consent gate |
| `ColorReleaseScreen` | EN / ZH | 10 | AwakeningApplicationController |
| `PerformerSelectionScreen` | EN / ZH fallback | 10 | AwakeningApplicationController |
| `FocusScreen` | 6 performers × EN / ZH | 10 | AwakeningApplicationController |
| `ActivationScreen` | 6 performers × EN / ZH | 10 | AwakeningApplicationController |
| `LivingPortraitScreen` | 6 performers × EN / ZH | 10 | AwakeningApplicationController |
| `ReleaseScreen` | 6 performers × EN / ZH | 10 | AwakeningApplicationController |
| `PostVideoAssistantScreen` | 6 performers × EN / ZH | 10 | AwakeningApplicationController |
| `AssistantChooserScreen` | EN / ZH | 10 | AwakeningApplicationController |
| `AssistantOverlay` | 6 performer themes × EN / ZH | 20 | AwakeningApplicationController overlay stack/backend session |
| `SettingsScreen` | EN / ZH | 10 | AwakeningApplicationController |
| `ClosingScreen` | EN / ZH | 10 | AwakeningApplicationController |
| `SouvenirScreen` | EN / ZH | 10 | AwakeningApplicationController |

This results in 17 UIDocument GameObjects (including the two existing Home docs). Screen-family UXMLs are selected from the unchanged 106-entry `RuntimeScreenCatalog`; no UI element names or logical route IDs are renamed. `AssistantChooserScreen` is a normal page. `AssistantOverlay` is a separate, higher-order document.

## Scene organization (current serialized scene)

The existing top-level layout is retained to avoid reparenting scene objects that already have controller and Home bindings. `ScreenDocumentManager` is added to the existing application/controller GameObject; no parallel controller or service object is created.

```text
SampleScene
├── Main Camera (unchanged)
├── Global Light 2D (unchanged)
├── GestureRecognitionRig (unchanged)
├── HomeLanguageController
│   ├── HomeLanguageController component (existing)
│   ├── AwakeningApplicationController component (existing)
│   └── ScreenDocumentManager component (new)
├── HomeUI_EN (existing UIDocument)
├── HomeUI_ZH (existing UIDocument)
├── PrivacyScreen (reused UIDocument)
├── TutorialScreen
├── AttractScreen
├── ColorReleaseScreen
├── PerformerSelectionScreen
├── FocusScreen
├── ActivationScreen
├── LivingPortraitScreen
├── ReleaseScreen
├── PostVideoAssistantScreen
├── AssistantChooserScreen
├── SettingsScreen
├── ClosingScreen
├── SouvenirScreen
└── AssistantOverlay (sorting order 20)
```

The Home GameObjects retain their existing names and serialized references; no UXML child structure was reparented or edited. All non-Home UIDocument components are serialized disabled so they cannot stack over Home in Edit Mode or at startup. Existing camera, lighting, consent, localization, and MediaPipe objects remain in their current hierarchy.

## Activation and cleanup contract

1. `AwakeningApplicationController` continues validating route constraints, consent, history, and catalog lookup.
2. It asks `ScreenDocumentManager` to activate the document family associated with the catalog `screenId`, passes the selected `VisualTreeAsset`, deactivates the previously active page document, and receives its root document for existing manifest-validated callback binding. Exactly one main page document is enabled.
3. `ShowHome` / localized Home return deactivate the active main page document and use the existing HomeLanguageController.
4. Opening a themed assistant overlay uses its own `AssistantOverlay` UIDocument. It is assigned the selected overlay UXML and enabled above the current page; the underlying page remains loaded but disabled for input as it is today. Existing overlay stack owns cleanup callbacks, assistant session cancellation, focus restoration, and close actions.
5. Closing or navigating away disables and clears the overlay document, removes subscriptions, and restores the active page. Repeated openings must not retain old trees or callbacks.
6. `HomeInteraction.uss` is attached to each active document root (page and overlay) as needed.
7. The manager does not call camera/consent APIs; production route checks and `GestureCameraConsentGate` remain in the existing controller unchanged.

## Incremental implementation sequence

1. Add `ScreenDocumentManager` with explicit family references and a single `Activate(screenId, languageAsset)` / `DeactivateMain()` / `ActivateOverlay(asset)` / `DeactivateOverlay()` contract. Family resolution derives only from the existing logical `screenId` patterns; the manager does not alter them.
2. Reuse the two Home documents and current generic Privacy document. Add family documents with the same PanelSettings. Their serialized initial UXML may be the corresponding English representative; startup disables them before use.
3. Route ordinary screens through the manager while retaining `AwakeningApplicationController` as the authority for business state and exact existing binding logic.
4. Move assistant tree assignment to the overlay document and preserve all overlay event cleanup and backend session code.
5. Confirm every catalog entry resolves to exactly one family and locale asset; ensure only one page document plus optional overlay is active.
6. Compile/reimport and perform runtime transition checks without consent bypass. Capture visual regression evidence against existing phase captures.

## Implemented serialized ownership

The current scene contains 17 UIDocuments. Two existing Home documents remain enabled for `HomeLanguageController`; the remaining 15 are serialized disabled in the initial scene and enabled by the manager only as needed. All documents use the same existing PanelSettings asset. The generic initial page document is named `PrivacyScreen`. `ScreenDocumentManager` is attached to the existing application-controller GameObject and holds 14 main-family slots plus the separate overlay document reference.

The exact per-locale UXML, USS, logical route, and Figma-root correspondence is listed in the catalog table in [UIDocumentArchitectureAudit.md](UIDocumentArchitectureAudit.md). Each imported screen UXML includes shared `tokens.uss` and its paired screen stylesheet.

## Validation status

Phase 3 recorded Play Mode pointer tests for English and Traditional Chinese Home → Privacy → Decline → localized Home, switching language both directions, and opening Settings from Home. Its finding that pre-awakening Back left Settings active exposed the bug corrected in Phase 4.2. The corrected Settings Back was verified in Play Mode for English and Traditional Chinese Home returns, repeated entry/exit, and the Assistant Chooser → Settings → Back history path. Escape from Assistant Chooser still did not navigate before awakening. Consent remained false. Post-awakening Back and camera-gated routes remain unverified. See [UIDocumentArchitectureAudit.md](UIDocumentArchitectureAudit.md) for the exact cause, fix, and results.
