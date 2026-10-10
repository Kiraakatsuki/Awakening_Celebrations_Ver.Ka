# Interaction Consistency and Settings Audit

Audit date: 2026-10-10. This is a static source audit; it does not claim Unity compilation, Play Mode, or visual verification.

## Baseline

- `HomeLanguageController` binds both Home variants through the connector ID/path/tag snapshot. It owns pointer hover/exit/press, click, Enter/Space, focus, language selection, and the consent checkbox. Language selection is stored in inline background/border styles; checkbox state is stored in inline colors and a runtime checkmark. Both persist while the Home documents remain alive.
- Home uses grouped hit targets, but each target previously received its own Tab stop. The group now keeps one Tab stop on the primary surface and leaves companion labels clickable without making duplicate focus stops.
- `AwakeningApplicationController.BindPath` owns the imported-screen event callbacks. Screen replacement removes its subscriptions before a new UXML tree is installed. Assistant overlay callbacks have a separate cleanup stack and are removed when overlays close.
- Generated UXML carries most per-screen color, position, typography, and surface treatment inline. The 99 imported `.uss` files and `tokens.uss` are present; the token definitions are not referenced by current USS rules. No imported screen USS uses `:hover`, `:active`, `:focus`, or the app-owned `ac-*` classes, so the app feedback rules do not compete with existing pseudo-state rules.
- The 2,808 imported UI elements are `VisualElement` and `Label` nodes; the 106 UXML files contain no native `Button`, `Toggle`, `TextField`, `Slider`, or `DropdownField`. Interactive behavior therefore uses the imported custom elements and explicit controller callbacks rather than replacing their layout with native controls.
- Shared `Assets/Resources/HomeInteraction.uss` supplies transient opacity feedback only. It now uses a neutral white focus outline on imported screens and scopes Home’s existing gold focus outline to `.ac-screen-home`. The imported screens retain their own colors and default styling.
- The Settings screen offers Language, Sound, Accessibility, Tracking, Restart, Privacy, Ask the assistant, Back to Experience, and a Guide affordance. The verified prototype routes are Language, Privacy, Ask assistant, and Back to Experience. No verified action or runtime integration exists for Sound, Accessibility, Tracking, Restart, or Guide.
- The project currently contains 47 raster assets; all 126 image references in the 106 UXML files resolve on disk. No audio/video assets or audio, camera, analytics, QR-generation, or assistant-service code were found. Visual matching of each asset to its Figma frame remains a separate manual check.

## State rules

| State | Implementation | Persistence/exit behavior |
|---|---|---|
| Default | Existing screen UXML/USS appearance; app state classes leave opacity at 1. | Remains unchanged when no transient state is active. |
| Hover | `ac-hover`; opacity 0.94 on controls, 0.98 on full-screen Attract/Color Release surfaces. | Pointer exit removes hover. Home also clears transient state when hidden or language changes. |
| Pressed | `ac-pressed`; opacity 0.82 on controls, 0.94 on full-screen input surfaces. | Pointer up/cancel/click/exit clears it; re-entry during a held pointer restores it. |
| Selected | `ac-selected` plus existing inline selected language colors. | Persists until language changes. Navigation controls are not made persistently selected. |
| Checked | `ac-checked`, inline checkbox colors, and a runtime checkmark. | Persists for the application session; pointer exit does not clear it. |
| Disabled/unavailable | Home Review Consent uses native disable plus `ac-unavailable` at opacity 0.58. Unsupported Settings and assistant service controls use native disable, `PickingMode.Ignore`, and `ac-control-unavailable` at opacity 0.55; unavailable labels are shown. | Settings/overlay UXML is recreated with the same disabled state on every visit. Disabled controls have no active callback or Tab stop. |
| Keyboard focus | One Tab stop per grouped action; Enter/Space activation; neutral white focus on imported screens, Home gold scoped to Home. | Focus state is removed on focus-out and Home navigation; Home and overlay return focus to a useful active control/root. |

## Interactive control inventory

All localized controls below are checked against the 106-row connector manifest, the runtime snapshot, and their on-disk UXML. Active imported-screen actions require Figma node ID, path, tag, and owning root to match before a callback is attached.

| Logical screen | Variants | Control / runtime target | Action / handler / destination | State source | Status |
|---|---|---|---|---|---|
| Home | EN `51:4`, ZH `53:2` | LanguageEnglish, LanguageChinese; paths `1/12–1/15` | `SelectLanguage` updates the visible Home and raises `LanguageChanged`; Settings language uses the same app state. | Home grouped callbacks; selected language colors; one focus stop per selector. | **Implemented; statically inspected.** |
| Home | EN/ZH | ConsentCheckbox, label and alternate label; paths `1/22–1/24` | `ToggleConsent` updates session consent and raises `ConsentChanged`; Review stays disabled until accepted. | `ac-checked`, inline colors/checkmark; disabled Review state. | **Implemented; statically inspected.** |
| Home | EN/ZH | Privacy Details, Settings, Review Consent; mapped surface/label paths | `PrivacyRequested`, `SettingsRequested`, `ReviewConsentRequested` route to Privacy/Settings; Settings Access also opens Settings. | Shared Home interaction groups; Review is disabled until consent. | **Implemented; statically inspected.** |
| Privacy | EN `51:39`, ZH `53:37` | Agree & Continue, Decline, Back to Home; surface and label pairs at `1/26–1/31` | `ContinueFromPrivacy` is consent-gated; Decline clears session consent; Back preserves it and returns Home. | Shared imported-screen hover/press/focus callbacks. | **Implemented; statically inspected.** |
| Tutorial Entry | EN `24:281`, ZH `105:307` | Continue surface `1/29/5` and label `1/29/5/1` | `Navigate(TutorialScreens[1])` advances to Introduction. | One focus stop on the surface; companion label remains clickable. | **Implemented; statically inspected.** |
| Tutorial | 7 localized steps | Guide actions at EN paths `1/37, 1/41, 1/43, 1/39, 1/39`; corresponding localized IDs are explicitly paired in code. | Advances through Introduction, Look/Move, Point, Hold/Select, Experience and Ready. Root Enter/Space remains a keyboard fallback. | One focused mapped Guide label; root fallback for Enter/Space. | **Implemented; statically inspected.** |
| Tutorial Ready / Return | EN/ZH | Start, Skip; Return/Dismiss title | Start and Skip enter Attract; Return/Dismiss returns to Introduction. | Shared imported-screen callbacks; one focus stop for Start surface, separate Skip. | **Implemented; statically inspected.** |
| Attract / Color Release | EN/ZH | Full-screen input surface | Pointer click or Enter/Space advances; session awakening state determines whether Color Release is skipped on later visits. | Restrained full-surface hover/pressed/focus classes. | **Partially implemented.** This is a mouse/keyboard fallback, not gesture recognition. |
| Performer Select | EN/ZH | Six performer cards; surfaces and labels use the six-card path formula | `SelectPerformer` stores the performer and enters its Focus view; key `1–6` selects the same card. | One focus stop per card; companion labels are clickable. Selection leaves this screen immediately. | **Implemented; statically inspected.** |
| Performer stages | 6 performers × 5 stages × 2 languages | Verified stage cue label per screen; path/ID offset is checked by stage in code | `AdvancePerformerStage` advances Focus → Activation → Living Portrait → Release → Post-Video AI → Closing. | Hover/pressed/focus on the cue label; Enter/Space screen fallback. | **Partially implemented.** Pointer/keyboard is a desktop fallback for the Figma gesture cues. |
| Focus Assistant entry | 6 performers × 2 languages | No corresponding imported runtime control | The Figma prototype has an Assistant entry, but current Focus UXML has no verified mapped node. | Not fabricated. | **Blocked by missing specification or integration.** A source-mapped Focus entry is required. |
| Assistant chooser | EN/ZH | Six theme/performer cards and Back to Settings | Card or key `1–6` opens the matching overlay; Back returns to Settings. | One focus stop per card; shared hover/pressed/focus classes. | **Implemented; statically inspected.** |
| Assistant overlays | 6 performers × 2 languages | Close surface/label remains active. Suggestion chips, Ask placeholder and Ask action are disabled. | Close returns to the chooser/underlying screen. No assistant response is fabricated. | Close uses normal interaction states; unavailable prompt controls are dimmed, inert and removed from Tab order. | **Partially implemented.** Assistant responses require an approved service/response contract. |
| Settings | EN `124:5218`, ZH `124:5272` | Language row surface/value/Adjust; Privacy row surface/Review; Ask assistant surface/label; Back to Experience | Switches app language; opens Privacy; opens assistant chooser; returns to Performer Select. | One focus stop per logical row/action; common pointer/focus feedback. | **Implemented; statically inspected.** |
| Settings | EN/ZH | Sound, Accessibility, Tracking, Restart | No verified prototype destination/effect; Sound has no audio system, Accessibility has no specified options, Tracking has no calibration/camera service, and Restart has no verified destination/reset scope. Values/action labels show Unavailable and are noninteractive. | Disabled native state plus `ac-control-unavailable`; no consent, language, or awakening state is changed. | **Blocked by missing specification or integration.** |
| Settings Guide | EN/ZH | `? GUIDE`/`? 指南` | No verified action in the current prototype; marked unavailable. | Disabled and removed from pointer/focus interaction. | **Blocked by missing specification or integration.** |
| Closing | EN/ZH | Souvenir and Return to Performer Select surfaces | Routes to Souvenir or Performer Select. | Surface is the single Tab stop; imported-screen hover/press/focus. | **Implemented; statically inspected.** |
| Souvenir | EN/ZH | Done surface | Returns to Performer Select per the verified prototype connection. | Imported-screen hover/press/focus. | **Partially implemented.** QR art is a placeholder; no QR destination or delivery service exists. |

## Source and mapping verification

- The connector manifest has 106 root rows and 2,808 node records. Each current node path/tag was resolved against its generated UXML from the imported visual root.
- The Settings disabled-state IDs and the active Settings row controls were verified for both language roots. Six performer stage formulas, 12 performer/assistant card groups, and 12 overlay control sets were checked against the manifest.
- The runtime catalog contains both locale records for every imported screen. The four Settings actions that remain active use existing IDs from the same verified Settings root; no visible text is used as a selector.
- Screen paths remain re-import-sensitive snapshots. The existing build-time/Editor validator checks the full source snapshot and catalog, while these additional checks specifically cover the active and disabled interactive targets.

## Manual verification still required

- **Requires developer runtime verification.** Open the project in its recorded Unity version, run the mapping menu validator, compile, and check Console before testing.
- **Requires developer runtime verification.** In both languages, exercise hover → pointer exit → press/cancel; verify selected language and checked consent remain persistent after pointer exit; verify disabled Review cannot be activated or tabbed to until consent is selected.
- **Requires developer runtime verification.** Tab through Privacy, tutorial, performer selection, Settings, chooser, overlay Close, Closing and Souvenir. Confirm there is one focus stop per logical control and Enter/Space triggers only one route.
- **Requires developer runtime verification.** Open/close/revisit Settings and overlays; confirm disabled values remain unavailable, language remains selected, and consent/performer/awakening state does not change.
- **Deferred visual adjustment.** Compare Home, Privacy, Settings, Tutorial, Performer Select/stages, overlays, Closing and Souvenir against Figma at the target window size. This static pass does not record coordinate-specific defects or validate clipping/contrast in the rendered player.
- **Blocked by missing specification or integration.** Supply sound integration and intended persistence; exact Accessibility options/effects; tracking/calibration service; Restart destination and reset scope; assistant service behavior; and QR generation/destination before enabling the corresponding controls.
