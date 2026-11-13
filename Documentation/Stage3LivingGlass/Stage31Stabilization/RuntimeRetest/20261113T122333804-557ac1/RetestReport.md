# Stage 3.1 — authorized runtime retest

13 November 2026. One Editor Play Mode entry only. Armed at12:23:34 UTC; stopped and confirmed outside Play Mode at12:31:16 UTC, under eight minutes including pre-entry setup. No implementation corrections were made during this retest.

[Actual runtime screenshots](Review.html) · [capture audit](CaptureSummary.json) · [protected hashes](ProtectedAfter.json) · [exit observations](ExitEvidence.json) · [transition matrix](TransitionMatrix.jpg) · [runtime log](RuntimeLog.txt).

## Results

| Retest | Status | Actual result and limit |
| --- | --- | --- |
| Camera/network preflight | PASS | Before Play: consent=false, endpoint empty, active runners=0. Unsaved overrides and continuous guard retained; Send/suggestions/consent disabled |
| Isolation throughout sampled runtime | PASS | Every accepted capture records denied gate/session consent, rigInactive=true, requestAbsent=true. No guard failure logged; no camera or live assistant operation |
| Normal ghosting correction | PASS — bounded | Eleven initial successful transitions plus settled PANDEMIC Chinese retry cover all six identities in both locales. Sampled portrait/copy handoffs contain no doubled destination portrait/text; final outgoing/proxy elements absent |
| Initial PAN Chinese scripted attempt | FAIL — trial | Normal-7 remained COLOR-RELEASE rather than navigating. It cannot count as a transition pass. Settled retry PAN-zh-retry reached the correct Focus with clean handoff. Fixture/input-gate timing is suspected, exact cause unconfirmed; no threshold was changed |
| Repeated transitions and final cleanup | PASS | Successful cases settle without outgoing-content or transition-portrait. Current route and localized identity agree |
| Overlay interruption | PASS | Interrupt-start-0 proves actual GAIA Focus entry; Verified-interruption-0 ends in Focus with Settings open, no retained copies |
| Locale interruption | PASS | Interrupt-start-1 proves PANDEMIC Focus entry; Verified-interruption-1 preserves Focus after locale replacement with no copies |
| Back interruption | PASS | Interrupt-start-2 proves RE-BIRTH Focus entry; Verified-interruption-2 returns Gallery without copies |
| Initial interruption fixtures | NOT RUN as valid interruption evidence | Original Interrupted-* captures stayed Gallery because their immediate confirmation was not accepted. They are retained but excluded from passing results; the verified retries above used settled entry |
| English/Traditional Chinese typography | PASS — representative readability | Actual Home, Gallery, Focus and Settings show readable text, alignment and wrapping; TC consent text complete; no literal leading tags or missing glyphs seen |
| Long mixed Chinese/Latin assistant content | PASS — layout only | AI-long-zh is an explicitly local plain-text layout specimen, not an assistant reply. Content scrolls within the conversation viewport; no request sent |
| Exact Figma typography/material parity | FAIL — remaining known difference | TC display remains Noto Sans CJK TC rather than proposed Noto Serif TC; optical/tinted glass remains simpler than enhanced reference. No claim of complete pixel fidelity |
| Scrollbar appearance | PASS | Actual native Scroller dark track/thumb replaces the prior bright default; content viewport remains contained and scrollbar visible |
| Native keyboard scrolling | PASS — End | Native Tab focused window-scroll; End reached offset396=max396 and exposed final Settings rows |
| Native pointer wheel | PASS | Chinese Settings wheel moved offset0→243 of max325, showing calibration/end-experience rows |
| Thumb drag, repeat arrows, Home/PageUp/PageDown | NOT RUN | Styling visible; exhaustive native operations not exercised in this pass |
| Gallery keyboard/pointer selection | PASS — representative | Native Tab/Return committed SANCTUARY; Escape returned Gallery with selected focus; native GAIA portrait click entered GAIA Focus |
| Modal input blocking/dismissal | PASS | Native blank/backdrop click did not trigger underlying Play action; Escape closed Settings and restored play-performer-video focus |
| Settings/AI motion/repetition | PASS — bounded | Opening frame sequences, eight alternating open/close cycles, AI minimize/resume complete without a visible duplicate window or stuck modal |
| Reduced motion/opaque alternative | PASS — representative | Runtime review override applied to Gallery/WHITE-GUARDIAN; fixed layout and static pose; no deferred saved preference claimed |
| Actual local video decoding/completion | PASS — GAIA | Real-video-decoded shows playing=true/frame65; ActualLoop.txt records actual loopPointReached; subsequent capture is genuine Release. No completion callback was synthesized |
| Replay cancellation | NOT RUN successfully | Replay-decoded and Replay-cancelled both remained Gallery with idle player. Cancellation.txt proves idle cleanup only, not actual playback cancellation. Historical previous-pass cancellation evidence is not a retest |
| Screenshot dimensions and metadata | PASS |113 accepted PNGs, all1920×1080, all rendered end-of-frame, captured frame order monotone; paired JSON for each |
| Same-frame route replacement capture | PASS | Expected-rejection-0085 rejected with reason and no successful PNG, rather than pairing Home metadata with Gallery pixels |
| Previously missing chrome | PASS — not reproduced in sampled new path | Repeated Home and RE-BIRTH Focus readbacks include header/dock/status, agreeing with native Game view observations. Nine settled image samples have nonempty header/dock text regions; see CaptureSummary. This bounded result does not identify every cause of historical omissions |
| Console/application exceptions | PASS — bounded | No new application exception/decoder error observed; final Console contains existing Unity AI NoSubscription diagnostic |
| Owned-resource cleanup before exit | PASS — observed scope | Texture/request null, activeCapture=false, copies=0, proxies=0; no claim of a memory soak or universal allocation audit |
| Exit and scene restoration | PASS | Playing=false, scene clean, consent=false, VideoPlayers=0; helper discarded unsaved isolation overrides and restored locale/Game view |
| Original scene/build/protected files | PASS | All14 protected hashes unchanged. Original SampleScene remains production build entry; no package/importer/shared-script change |
| Real gestures, live AI, Windows builds, extended profiling/release readiness | BLOCKED | Outside authorization; not simulated or inferred from these results |
| Chinese IME, every localized route, long-duration leaks/performance | NOT RUN | Representative scope only; no60fps guarantee |

## Before/after findings

**Ghosting:** the previous120–280ms runtime frames showed both moving proxy and destination artwork plus doubled copy. New successful sequences show one portrait owner and exclusive outgoing/incoming text windows, followed by clean final Focus. The outgoing Gallery focus rectangle can still briefly remain around its vacated aperture while fading; this is a minor aesthetic refinement candidate, not residual UI after completion. Sampling cannot prove every intermediate frame or frame pacing.

**Typography/scrollbars:** Home leading and regular labels are readable in real Unity in both locales; Chinese consent remains unclipped. Settings now has a neutral dark scrollbar and genuine keyboard/wheel access to lower content. Full serif-family/material equivalence and physical-display accessibility remain open.

**Readbacks:** the new end-of-frame path produced113 dimension-correct paired captures, including repeated formerly problematic Home/RE-BIRTH Focus states, with visible chrome. An intentionally changed presentation was rejected. Historical Chrome omissions are not reproduced here; no application-rendering architecture change was needed. Metadata/frame checks support capture consistency, not a general performance or pixel-fidelity claim.

## Evidence qualification

Gallery readiness is a labelled offline presentation fixture, not a gesture result. Most transition-matrix steps use existing semantic commands; native pointer/keyboard operations are explicitly identified above. Early commands that did not enter Focus are excluded from passing transition/interruption counts.

The replay fixture failure was found during final route/video metadata audit after Play Mode had ended. A progress statement initially called it a cancellation pass; this report corrects that statement. No second session was started. PreExitResources.json was empty because JsonUtility does not serialize that anonymous record; ExitEvidence.json preserves values actually returned by Unity inspection, not a passing result inferred from the empty file.

CaptureSummary's bright-pixel checks only corroborate header/dock presence in selected settled PNGs. Actual images and native observations were also inspected; geometry alone is not visual acceptance. The transition sheet is a compact index; use original PNGs and route metadata for each frame.

## Remaining work

1. Diagnose the skipped scripted confirmation near transition/locale changes with controlled timing; distinguish expected input gating from a fixture or navigation defect. Preserve gesture semantics.
2. If separately authorized, perform genuine decoded replay cancellation, native scrollbar drag/repeat/Page/Home cases, IME and broader accessibility tests.
3. Review TC serif assets, final material fidelity and transient outgoing focus-ring treatment as scoped future work.
4. Keep camera/backend/performance/Player and production-cutover gates separate. No cleanup, implementation phase or release testing follows automatically.

No new production defect was corrected during this validation. The candidate remains **not production-ready**.

