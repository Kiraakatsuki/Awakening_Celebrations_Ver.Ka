# Reflect & Converse — shared screen refinement

Completed as a scoped UI change, with no Play Mode or service requests. [Visual review](Review.html), [layout measurements](LayoutChecks.csv), [Editor results](EditorChecks.txt), [protected files](ProtectedAfter.json), and [before measurements](BeforeGeometry.txt).

## Findings and composition

The previous shared PostVideo screen stacked chapter index, performer title, theme line, conversation title, privacy introduction, prompts, transcript, question, two actions and Closing inside a60%-wide chapter panel. That panel overlapped the artwork. The transcript measured105.6 desktop pixels at0.8 preview scale: **132 logical pixels**, despite its150px style, because the column was constrained and could shrink.

The approved [Cinematic Spatial/PostVideo reference](../../Stage2Design/LivingGlass/review-evidence/postvideo-en.png) separates artwork from reading and progression. The new screen applies that principle to a functioning conversation interface rather than copying a conceptual frame that lacks the actual chat controls.

The interaction glass now occupies the larger left zone; artwork has its own right zone with performer name, short theme line and quiet Closing action. Neither composer nor suggestions covers artwork. One conversation title and one concise privacy/context line replace the repetitive headings. Source artwork/focal cropping and performer accents are reused without six different layout implementations.

At1920×1080 the transcript measures approximately **365 logical pixels**, about2.8× its previous height. The composer remains144px high in every previewed message state; compact mode uses128px. Long messages scroll within the transcript and do not expand the entire page or move the question field. The48 measured performer/locale/viewport cases all retain at least160 logical pixels of transcript space. These are isolated native Editor layout measurements, not Game view or performance measurements.

## Interaction decisions

- Three balanced prompt buttons have equal flex allocation, consistent gutters and persistent neutral hover/focus states. Existing direct-submit prompt semantics are unchanged.
- The question field retains its visible associated label, multiline editing and3600-character cap. Ask sits in the same composer region with a stable position. Enter remains an editing key inside the field; explicit Ask submits through the existing binder.
- Cancel request is hidden only on this screen when its existing button is disabled. The controller already enables it only while requestInProgress, and disables it on completion/cancellation. The UI does not invent a new pending timer or state machine. A reserved control column prevents layout shifts when Cancel appears.
- Continue to Closing sits below performer identity in the artwork zone, using the quiet action treatment. It retains finish-chapter and the existing LivingContinue command, separate from assistant submission.
- Tab order remains prompt buttons → scroll region → question → Ask → Cancel when available → Closing → shell. Existing modal opener restoration, editor/IME exclusions and gesture recognition code are unchanged. Actual native input behavior after this change requires runtime authorization.
- New arrivals use the existing restrained page entrance. Suggestions retain120ms fill/edge feedback; no bubble movement, typewriter delay, response timer or animated composer was added. Reduced motion retains immediate page settling, and opaque mode includes the new interaction surface.

## State coverage and contract

The Editor specimens are explicitly labelled **layout specimens**, not generated replies, stored history or actual requests. They run with flow=null and never initialize the assistant, player or capture pipeline.

| State | Layout treatment / static verification | Functional validation |
| --- | --- | --- |
| Empty | One calm empty instruction inside the large transcript; Cancel hidden | NOT RUN in Play Mode |
| Suggestions | Three balanced, wrapping buttons; shared focus/hover styles | Existing prompt-submit binder preserved; live operation NOT RUN |
| Typing | Local draft specimen; fixed labelled editor and adjacent Ask | Native IME/caret/clipboard NOT RUN |
| Sending | Ask disabled, Cancel visible; fixed composer geometry | Real pending lifecycle NOT RUN; existing controller drives availability |
| Successful reply | Plain-text Editor layout specimen inside transcript | Actual response parsing/backend outcome NOT RUN |
| Long conversation | Labelled long TC/Latin/English specimens; internal scroll, stable composer | Real multi-turn lifecycle NOT RUN; no product history fabricated |
| Failure | Status remains inside transcript; same stable controls | Existing error mapping/status code unchanged; live failure NOT RUN |
| Retry | Retry caption fits the same Ask bounds | Existing failedMessage behavior unchanged; actual resend NOT RUN |
| Cancellation | Cancel returns to hidden layout, question region stable | Existing abort/session generation/late-reply protection unchanged; actual cancellation NOT RUN |
| Offline/unavailable | Existing status region fits explanatory copy, no fake connectivity toggle | Configuration error is still reported by existing submit validation; no new network detection or backend behavior |
| Continue to Closing | Separate visible quiet action, existing identifier/command | Static route contract retained; actual navigation NOT RUN |

All existing binding names remain unique: assistantChatInput, assistantChatSend, assistantCancel, assistantConversation, assistantChatStatus, assistant-empty and finish-chapter. Status stays inside its conversation because the controller reparents and scrolls to it. The existing binder, performer-scoped payload, request timeout, cancellation/session sequence, response renderer, late-response checks and locale/session replacement behavior are byte-preserved. No persistence or fictional transcript was introduced. Actual submission hides the empty instruction through the existing controller; the pending visual specimen concentrates on control availability rather than simulating a transport.

## Responsive scope

The existing PanelSettings scales with screen size against1920×1080; it was not changed. Isolated tests exercised1920×1080,1600×900,1280×720 and approximately1822×1138 logical canvases, the latter representing the16:10 aspect case. Both locales and all six performers share these rules. Below1800 logical pixels the artwork zone narrows to27% with a24px gap; below900 logical pixels in height, padding/type/prompt sizing reduce and the composer uses128px. Main actions remain at least48px high.

These are bounded desktop composition checks, not approval of arbitrary small windows, mobile layouts, Windows DPI or physical viewing distance. The actual1920-reference panel can map physical resolutions differently; operational resize and aspect/DPI trials remain required.

## Changes and verification

| File | Change |
| --- | --- |
| Assets/LivingGlass/Presentation/LivingGlassPresentation.cs | Dedicated shared PostVideo composition; spacious BuildChat option; stable composer; pending-only Cancel class; short-height class; split action variant classes correctly |
| Assets/LivingGlass/UI/Styles/LivingGlass.uss | Two zones, transcript flex allocation, shared suggestions, fixed composer/controls, quiet progression and compact/opaque variants; removed obsolete congested PostVideo rules |
| Assets/LivingGlass/Editor/LivingGlassConversationChecks.cs (+meta) | Isolated Editor layout/state checks with service owner absent; no runtime/test-runner side effects |

The action helper previously treated a multi-class variant string as one class; the first new preview exposed that Cancel's hidden rule did not apply. Splitting variant names fixed this during implementation. The initial Reflect-en-STATIC.png is excluded from final review. Reflect-pending-STATIC.png was occluded by Codex and is also excluded; use the final-named captures. No existing user assets/evidence were deleted.

| Check | Status | Evidence |
| --- | --- | --- |
| Unity C# compilation / USS import | PASS | Editor compiled and rendered the new builders; no new compiler/USS errors observed |
| Repository static verifier | PASS |24 checks; StaticVerification.json |
| Layouts | PASS |48 combinations: six performers×two locales×four canvases; no zone overlap; transcript/composer separation; controls contained; unique editor ID |
| State specimens | PASS — layout only |20 additional Editor states, ten in each locale; correct Cancel display and non-overlapping pending actions |
| Composer stability | PASS — layout only |144px across all FHD state specimens; compact128px; no response-driven reflow |
| Protected source/configuration | PASS |All15 hashes unchanged, including original/candidate scenes, build entry, packages, importers, existing shared modifications and LivingGlassFlowIntegration |
| Play Mode / live requests | NOT RUN |Not authorized for this task; no camera/player/assistant lifecycle initialized by the view checks |
| Runtime regression absence | NOT RUN |Compilation and Editor layouts do not establish operational parity |

Existing legacy obsolete/unused-field warnings and Unity AI NoSubscription/MCP transport diagnostics remain. No dependencies were added. No original scene, production build entry, video, MediaPipe, consent, backend, saved preference or deferred business feature changed. Git state and recoverable before sources are preserved here.

## Exact next runtime checks — require separate authorization

1. Candidate scene only, camera denied and runner inactive, backend URL blank, send controls guarded. Confirm empty layouts for six identities/both locales and stable artwork/shell at actual fullscreen/window/DPI conditions.
2. Native Tab/Shift+Tab through prompts, transcript, question, Ask and Closing; wheel/drag/Page/Home/End; focus-ring clipping; modal open/dismiss from the question and restored draft/focus; Back and permitted gesture behavior. Keep hardware off.
3. Native Traditional Chinese IME: composition, candidate selection, Enter/newline, paste near3600 characters, caret scrolling and resize while drafting. No Enter-to-send regression or composer movement.
4. Offline local UI specimens, explicitly labelled: pending visibility, long/error/status text, Retry caption, cancellation availability, reduced motion/opaque variants. Never invoke live requests under offline-only authorization.
5. Actual success, multi-turn replies, network failure, Retry, pending cancellation and delayed replies need **independent live-service authorization**, or a separately approved isolated test transport. They are not proven by the current specimens. Verify performer payload/session guards and leaving to Closing cancels/ignores late work using the real contract.
6. Verify real Continue to Closing and return/navigation; global AI overlay remains its existing compact family. Stop, inspect Console/resource cleanup and compare protected hashes. No cutover or cleanup follows automatically.

The shared layout is ready for bounded runtime review; it is not a newly accepted production build. TC font/material limitations from Stage3.1 remain unchanged.
