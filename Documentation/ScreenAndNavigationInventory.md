# Screen and Navigation Inventory

Audit date: 2026-10-10. Sources inspected: the local connector manifest and UXML files, Figma source frames, the Figma interactive prototype page, and the enabled scene.

## Baseline and source limitations

- The project on disk targets Unity **6000.4.7f1** (`ProjectSettings/ProjectVersion.txt`), not the 6000.7.4f1 expected in the implementation brief.
- The only enabled build scene is `Assets/Scenes/SampleScene.unity`.
- The Figma page inventory and prototype reactions were inspected read-only. `get_design_context` returned the Figma message “You currently have nothing selected”; the source page/tree, metadata and screenshots were available via the other read-only Figma tools. No Figma content was changed.
- The original connector manifest has 106 source/import rows but only 98 distinct generated asset names/files. Eight tutorial English/Chinese pairs collided. This implementation restores separate English tutorial UXML variants while leaving the original Chinese UXMLs unchanged; the runtime catalog now has 106 distinct localized records.
- The connector's `path` is a one-based descendant index snapshot. It is useful as a validated import locator for the current generated file; it is not a durable identity after hierarchy edits or re-import.

## Runtime inventory

| Logical group | Localized screen assets | Figma source/prototype evidence | Current behavior/evidence |
|---|---|---|---|
| Home & Consent | `HOME---ENGLISH---SANCTUARY.uxml` / `HOME--------SANCTUARY.uxml` | canonical EN `51:4`, ZH `53:2`; prototype EN `46:191`, ZH `46:219` | Home language, consent checkbox, keyboard focus and hover/pressed interactions are in `HomeLanguageController`; Privacy/Settings now raise events into the flow controller. Scene Home documents are active in the edited scene. |
| Privacy & Consent | `PRIVACY---ENGLISH---SANCTUARY.uxml` / `PRIVACY--------SANCTUARY.uxml` | canonical EN `51:39`, ZH `53:37`; prototype EN `58:142`, ZH `58:176` | Agree & Continue enters localized tutorial entry after session consent; Decline clears consent and returns Home; Back Home preserves consent. The old generic document is now the disabled navigation document until a screen is opened. |
| Tutorial | Entry, Introduction, Look/Move, Point, Hold/Select, Experience, Ready, Return/Dismiss | EN source roots `24:281`, `24:326`, `24:378`, `24:426`, `24:476`, `24:522`, `24:568`, `24:613`; ZH `105:307`, `105:352`, `105:434`, `105:493`, `105:552`, `105:608`, `105:666`, `105:711` | Prototype sequence is verified; the ready screen Start/Skip enters localized Attract. The eight current imported UXML filenames collide across languages and contain Chinese copy. English source labels were separately read from Figma and their paths are available for recovery. |
| Attract / entry | `04-01...EN...uxml`, `04-02...uxml` | prototype EN `58:1171`, ZH `58:2098` | Root key/click routes to localized Color Release. Initial-entry vs subsequent-entry branching is application session behavior, not modeled by Figma. |
| Color Release / Color Awakening | `04-03...EN...uxml`, `04-04...uxml` | prototype roots `63:334` EN, `63:362` ZH | Key/click routes to localized Performer Select. Figma has a transition screen; an explicit runtime completion trigger/duration is not exposed by the prototype as an app-level session flag. |
| Performer Select | `05-01...EN...uxml`, `05-02...uxml` | prototype EN/ZH selections route to one of six Focus views | Six identities confirmed: Gaia, Pandemic, Re-Birth, White Guardian, Conscience, Sanctuary. Runtime choice must be explicit and retained separately from screen/language. |
| Performer experience | `06-01` to `06-60` (six performers × Focus, Activation, Living Portrait, Release, Post-Video AI × EN/ZH) | prototype stage sequence verified: Focus→Activation→Living Portrait→Release→Post-Video AI→Closing | Six variants have imported UXML; corresponding image reference is incomplete (see asset audit). Trigger cues are gesture-oriented in design; mouse/keyboard fallback requires product-specific interpretation and is not proven to simulate physical gestures. |
| Assistant overlays | `07-01` to `07-12` (six performers × EN/ZH) | Settings chooser selection opens the matching overlay; Close returns to the underlying chooser. The Focus prototype entry has no corresponding imported Focus UXML node. | Runtime overlays and Close are wired. Suggestions, Ask placeholder, and Ask are marked unavailable because no assistant service/response contract exists. Focus entry remains blocked by the missing imported node. |
| Settings and assistant chooser | `08-01...ENGLISH`, `08-02`; prototype chooser `07-13`, `07-14` | Settings routes Language, Privacy, assistant chooser, back to Performer Select; chooser routes to assistant and back to localized Settings | Language, Privacy, Ask assistant and Back are implemented in both locales. Sound, Accessibility, Tracking, Restart and Guide are visibly disabled until behavior/service requirements are verified. Visual parity still requires developer review. |
| Closing | `09-01...ENGLISH`, `09-02` | souvenir→localized Souvenir; return to experience→Performer Select | Imported. |
| Souvenir | `09-03...ENGLISH`, `09-04` | Done→Performer Select | Imported. QR delivery outcome/service behavior is not present in the Unity project and cannot be claimed as implemented. |

## Prototype flow

Verified Figma prototype routes, in compact form:

1. Home language selector switches between Home EN and ZH. Privacy Details and Review Consent go to localized Privacy. Settings goes to localized Settings.
2. Privacy Agree & Continue enters localized Tutorial Entry; Decline/Back returns to localized Home.
3. Tutorial Entry's **Continue** action → Introduction; the Guide label advances Introduction → Look/Move → Point → Hold/Select → Experience → Ready. Tutorial Return/Dismiss title returns to Introduction. Ready Start and Skip both enter localized Attract. The UI maps these actions by verified Figma node ID/path/tag, not text values.
4. Attract → Color Release → Performer Select.
5. Performer Select → selected performer's Focus. Focus → Activation → Living Portrait → Release → Post-Video AI → Closing.
6. Performer Focus and Settings open the matching assistant/chooser screens; assistant Close returns to the underlying view; chooser selection opens that performer's assistant and Back returns to localized Settings.
7. Closing → Souvenir or Performer Select. Prototype Souvenir Done → Performer Select. The localized Souvenir frame copy says “return to Attract for the next visitor”; that canonical copy conflicts with the verified prototype route. Runtime follows the prototype route and the conflict remains documented.

The prototype is a view-state graph; it does not establish a persistent backend, camera/gesture recognition, QR service, consent storage policy, or a session-scoped awakening variable. The current implementation treats consent and awakening as session-only runtime values, and sets awakening after leaving Color Release for Performer Select. Pointer/keyboard activation is a desktop fallback for gesture cues, not gesture recognition.

## Runtime mapping

The manifest rows identify imported root IDs, `path`, tag and class. The new `RuntimeNodeBindings.json` keeps a runtime-sized snapshot of the 106 imported roots and 2,808 descendants. The controller requires the Figma node ID, exact snapshot path/tag and the owning registered root ID to agree, then verifies the runtime element tag and bounds before registering an action. Any failed assertion logs an actionable error and skips that binding. The paths remain re-import-sensitive snapshots and will need regeneration/review after connector changes.

## Implemented flow and known gaps

- `AwakeningApplicationController` wires Home, Privacy, tutorial progression, Attract, Color Release, performer selection, all six imported experience stage chains, Settings, the assistant chooser/overlays, Closing and Souvenir.
- Language and selected performer are kept in memory across navigation. The session awakening flag is memory-only and skips Color Release on a later Attract visit in the same process.
- Assistant Focus screens in the imported UXML do not contain the prototype's Assistant entry node, so the overlay is reachable through Settings' assistant chooser only. The Focus shortcut is unresolved rather than synthesized.
- Assistant prompt/Ask controls have no AI service in the project. QR delivery has no destination/provider. The Souvenir screen's existing placeholder remains a placeholder.
- Settings prototype actions establish Language, Privacy, Ask assistant and Back to Experience. Sound, Accessibility, Tracking, Restart and Guide have no verified prototype effect or required runtime integration; their imported affordances are disabled and marked unavailable.
- Mouse/keyboard fallback click advances the gesture-driven performer stages; the project has no gesture/camera interaction implementation.

## Remaining verification

- Unity Editor/Player execution and the UI Builder unsaved state were unavailable in this environment; no Play Mode test is claimed.
- Re-import stability is conditional on revalidating path/tag/type snapshots.
- Exact mouse/keyboard equivalents for gesture cues and any missing physical gesture input are not established by the source. The runtime should expose clear pointer/keyboard fallback actions without claiming actual gesture recognition.
- Full visual fidelity remains incomplete where distinct performer artwork is absent/reused. See the asset audit.
