# Implementation Plan and Runbook

## Unity setup and launch

1. Open the project with its recorded Unity Editor version, **6000.4.7f1** (`ProjectSettings/ProjectVersion.txt`). The project brief mentioned 6000.7.4f1; the current files do not match that version.
2. Open `Assets/Scenes/SampleScene.unity` (it is the enabled build scene).
3. Press **Play**. The saved scene now activates both Home UIDocuments; the flow controller hides the legacy Privacy document until a screen is requested.

Unity Editor and Play Mode were unavailable to this implementation session, so these launch steps and routes still require an actual Editor run. On opening the project, first check the Console for script compile errors or the controller's fail-closed Figma path messages.

## Interaction walkthrough

1. On Home, click English/中文 repeatedly and select the consent checkbox. Current language and consent live for the running process.
2. Open **Privacy Details** or **Review Consent**. The Review action stays unavailable until the Home checkbox is selected. On Privacy, **Agree & Continue** enters the tutorial; **Decline** clears the session consent and returns Home; **Back to Home** returns without clearing it.
3. In Tutorial Entry, click **Continue** (or press Enter/Space). On Introduction through Experience, activate the **Guide** element to advance. On Ready, choose Start or Skip. The return/dismiss title returns to Introduction.
4. Click or press Enter/Space on Attract. If awakening has not completed in this process, this opens Color Release; activating it completes the one-time session state and opens Performer Select. Later Attract visits skip Color Release.
5. Select one of six performers by clicking its card or pressing **1–6**. On the five stage views, pointer click or Enter/Space advances through Activation, Living Portrait, Release, Post-Video AI and Closing. This is an explicit desktop fallback for the gesture cue; it does not implement camera gesture recognition.
6. Open Settings from Home. Privacy, Language, Ask assistant (chooser), and Back to Experience are wired. Choose an assistant theme by card or 1–6; the matching assistant overlay appears over the chooser. Close or press Escape to dismiss it.
   - Sound, Accessibility, Tracking, Restart, and the Settings Guide are displayed as unavailable and cannot be activated. The prototype does not establish their effects/reset scope, and no audio, accessibility, tracking, or restart integration exists.
   - Assistant suggestion chips and the Ask placeholder/button are unavailable without a response service. Closing an overlay works.
7. Closing can open Souvenir or return to Performer Select. Souvenir Done returns to Performer Select per the actual prototype connection. Its QR artwork remains a placeholder without a runtime destination.
8. Escape/Backspace returns through the runtime navigation stack or closes an assistant overlay. Keyboard focus and Enter/Space activation are available on controls that have verified Figma path bindings.

## Manual Unity validation still required

- Resolve any compile/import issue reported by the Unity Console; no Unity or .NET SDK compiler was available during implementation.
- Enter Play Mode and verify startup from Home, repeated language changes, checkbox state, Privacy gates, tutorial control response, all six performer selections, all 30 stage/language combinations, Settings, overlay open/close, Closing/Souvenir, Escape and session reset after a full Player restart.
- Verify the language setting refreshes Settings in place and that language/performer/consent/awakening remain independent.
- Inspect focus indication, pointer hover/pressed, and layout at the 1920×1080 target in both locales.
- Confirm unavailable controls are visibly disabled and absent from Tab order, then verify language/consent/performer/awakening state remains unchanged when they are clicked or revisited.
- Compare performer art and QR area against Figma source; all referenced assets now exist, but visual correspondence/cropping still needs review.
- Ask product/design owners to resolve the canonical Souvenir “return to Attract” copy versus prototype “Done → Performer Select” route before a release.
- Provide an assistant service/response contract, QR URL/service, and the exact Sound/Accessibility/Tracking behavior plus Restart destination/reset scope before enabling those controls.
