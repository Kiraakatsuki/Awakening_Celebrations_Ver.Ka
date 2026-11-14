# Stage 4.1 Living Glass composer and voice interaction

The shared composer now places Ask at the lower-right in Reflect & Converse, the assistant page and the global assistant window. The microphone sits beside the question field with a visible **Voice unavailable** or **語音尚未提供** label. The window uses the existing text assistant for visitor conversations. Future voice states live in the isolated Editor review and the [voice interaction specification](VoiceInteractionSpecification.md).

## Composer changes

The original Reflect & Converse preview placed Ask 24 logical pixels from the top of an edge-aligned action column. The assistant window used a separate row. The shared component now places Ask about 13 logical pixels from the right edge and 11 from the bottom, including the edge stroke. The native multiline question editor fills the center, and its lower edge aligns with Ask. The composer bounds long drafts and lets the input scroll internally. Its width and height stay fixed when Cancel appears or the transcript fills. The transcript scrolls independently on all three surfaces. See [baseline preview coordinates](BeforeGeometry.txt) and [final layout measurements](LayoutChecks.csv).

The new frame uses a dark reading surface and a subdued edge. The original vector microphone uses Painter2D and takes a restrained top-edge tint from the existing performer accent. GAIA, PANDEMIC, RE-BIRTH, WHITE-GUARDIAN, CONSCIENCE and SANCTUARY share the same geometry. English and Traditional Chinese use the existing locale IDs and typefaces. The compact layout gives the transcript enough room to absorb the suggestion row narrowing or disappearing.

Ask retains hover, press, focus and disabled treatments. The microphone is an inert visual element with visible status text. The text field retains its label, 3,600-character limit and native editing behavior. Pressing Enter in the field is still editing, and the existing explicit Ask binding sends the question. Cancel request is visible only when the existing request lifecycle enables it. Its reserved spot above Ask keeps both actions separate and preserves the active controls’ tab order. Suggestions, reply rendering, Retry, request context, lifecycle and late-response handling use the existing controller and bindings.

The production composer uses the existing entrance, overlay and button motion. The design specification describes preparing, listening, processing and returning to idle with the existing easing, bounded optical feedback and a reduced-motion alternative. The Editor listening waveform is a fixed graphic. Listening, processing, editable transcription and spoken response in the review are local presentation specimens with no service owner. Permission, error, cancellation, playback and onboarding states have bilingual design specifications.

## Visual review

In Unity Edit Mode, open **Awakening Celebrations → Living Glass → Voice composer design review**. Use the performer, language, state and surface selectors to compare any of the six themes in English or Traditional Chinese. The reduced-effects switch exercises the opaque material alongside reduced motion. The review displays a persistent design-preview label. It closes on assembly reload and releases its temporary presentation and cloned asset configuration when closed.

| Capture | What to inspect |
| --- | --- |
| [Idle, English](Composer-en.jpg) | GAIA’s unavailable microphone, generous center editor and inset Ask |
| [Listening, Traditional Chinese](Listening-zh.jpg) | RE-BIRTH’s restrained accent, static waveform and Stop / Cancel placement specimen |
| [Editable draft, Traditional Chinese](Transcription-zh.jpg) | PANDEMIC’s wrapped draft inside the assistant window |
| [Spoken response, English](Spoken-response-en.jpg) | SANCTUARY’s reply-level playback placement with reduced effects |

These are scaled native Editor captures of the 1920 × 1080 logical composition. The programmable review covers the remaining combinations.

## Files changed

| File under `Assets/LivingGlass/` | Scope |
| --- | --- |
| `Presentation/LivingGlassPresentation.cs` | Shared composer, display-only microphone and fixed composer placement outside the transcript scroll view |
| `UI/Styles/LivingGlass.uss` | Composer material, spacing, compact layout, focus and pending controls |
| `UI/Components/LivingGlassMicrophoneIcon.cs` and metadata | Original vector microphone silhouette |
| `Editor/LivingGlassConversationChecks.cs` | Extends the existing isolated regression suite to the other conversation surfaces and anchor stability |
| `Editor/LivingGlassPreviewWindow.cs` | Reuses the existing isolated view host and reserves room for the design-review controls |
| `Editor/LivingGlassVoiceReview.cs` and metadata | Editor-only theme, locale, surface and future-state specimens |

Baseline is `e57d4fa0c72e087c4a8a86f6e5c4657ec51e4488`. Nine screenshot and metadata deletions were already present when this phase started. The phase leaves those working-tree changes as found. Source scope and script metadata are recorded in [static verification](StaticVerification.json).

## Validation

| Check | Result |
| --- | --- |
| Unity 6000.4.7f1 compilation and USS import | PASS. Final Editor reports no script compilation failure. |
| Conversation regressions | PASS. 144 performer, locale and viewport combinations across three surfaces and the existing 20 state specimens. Checked viewport settling, transcript geometry, Ask insets and stability with hidden suggestions, long draft and pending Cancel, plus scroll range and End handling on the long reply specimens. [Results](EditorChecks.txt) |
| Viewports | PASS at 1920 × 1080, 1600 × 900, 1280 × 720 and 1822 × 1138 logical layouts within UI Toolkit. |
| Voice review states | PASS. 144 specimen trees across both review surfaces, six performers, two locales and six selected design states. Verified the unavailable voice marker is display-only and draft text is limited to the transcription specimen. [Results](VoiceSpecimenChecks.txt) |
| Localization and asset references | PASS. 381 CJK glyphs checked against the bundled font with no omissions. Both new script metadata GUIDs are unique. [Static checks](StaticVerification.json) |
| Scene and route contracts | PASS. Primary development entry, six video references, one service rig, 52 routes and 104 locale pairs, plus pure motion and crop contracts. [Scene verification](SerializedValidation.txt) · [Pure contracts](PureContractChecks.txt) |
| Play Mode, native IME and input lifecycle | NOT RUN. The tests use isolated UI Toolkit views and simulated UI events. Application focus restoration, native IME candidate placement, physical pointer and keyboard behavior and live request cancellation still require runtime verification. |
| Hardware, speech, live AI, Windows build and profiling | NOT RUN. |

The first 1280 × 720 test exposed a composer shift when hiding suggestions while entering a long draft. The compact spacing correction passes the same test. During review, an Editor utility window surviving a script reload produced a null reference from an inspection call. The review now closes before that reload. The Editor also reported the existing Unity AI `NoSubscription` diagnostic. The Stage 3.4 raw GUID debt remains outside this phase's asset changes.

## Stage 4.2 and Stage 4.3 requirements

Stage 4.2 needs an explicit microphone consent decision independent of the camera, an owned capture lifecycle, local versus remote processing selection, editable drafts and cancellation that discards stale transcriptions. It should verify permission denial, unavailable service, late results and typing with native IME. Submission continues through the existing Ask binding.

Stage 4.3 needs one scoped speech playback owner, an agreed synthesis and privacy policy, reply identity and interruption tests, and arbitration with guide, environmental audio and protected media. Guide lines need bilingual script, delivery, replay, mute and subtitle review. The full state copy, motion proposals and route placements are in the [voice interaction specification](VoiceInteractionSpecification.md).
