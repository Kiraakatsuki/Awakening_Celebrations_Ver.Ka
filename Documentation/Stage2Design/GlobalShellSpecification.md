# Global application shell

Proposal reconciled with Stage 1; availability, safe-exit destination and capture policy still require approval.

## Stable and cinematic forms

Normal shell exposes **Settings / 設定**, **Ask AI / 詢問 AI**, **Help / 說明**, **Privacy / 隱私**, language and honest camera status. A uses a stable lower band, B a floating dock, C a quiet edge band. Body content reserves its space; no hidden overlapping hit targets. Initial targets are 60–64 logical pixels high with 8–16px gaps.

Protected Activation/portrait preparation/playback/exit use a reduced edge shell outside the artwork’s important content. It shows context, “Settings & AI after this moment / 此段落結束後可開啟設定與 AI”, and an explicit safe exit. Existing video Back cancellation is preserved; extending immediate safe exit to Activation is a proposed behavioral change, not an approved one. No automatic pause/resume mode is invented.

## Capability presentation

| State | EN / Traditional Chinese copy | Behavior |
| --- | --- | --- |
| Available | Open Settings / 開啟設定 | Open one overlay; capture opener/focus context |
| Deferred | After this moment / 此段落結束後 | Explain protected boundary; never surprise-open later |
| Ready after deferral | Settings ready — Open / 設定已可使用 — 開啟 | Explicit activation only; old intent expires on route/reset/cancel |
| Unavailable | Assistant unavailable. Try again later. / 助理目前無法使用，請稍後再試。 | Recovery action only when a real service exists |
| Camera off | Camera off / 相機已關閉 | Does not imply permanent denial; no feed preview |
| Suspended | Capture suspended / 已暫停擷取 | Session decision retained; Settings/Privacy policy approval required |

Stable Home/tutorial/selection/Focus/Release/PostVideo/Closing may offer global actions subject to active consent dialogs and modal priority. Initial Color Release presentation defers nonessential interruptions until its state commits. Privacy decision dialogs take priority over AI. At most one ordinary overlay plus one explicit confirmation child exists.

## Modal/input/focus behavior

Input order: privacy stop → top modal dismissal → focused control/editor → shell command → experience command. Shell clicks and overlay keystrokes never bubble into full-page stage progression. The prototype implements background `inert`, focus entry, Tab/Shift+Tab containment, Escape close and restoration to a connected opener or intentional fallback. This is browser evidence only, not a Unity focus certification.

Locale updates preserve route, selected performer, awakening state and consent; shell/overlay labels can update without rebuilding video. During protected playback, defer page-layout changes. Existing assistant text is not silently translated/re-sent. CJK IME composition must not trigger submission in production; this prototype has no functional chat input.

Pointer and supported keyboard actions remain available. Existing **point→fist** is illustrated only for performer selection. No generic shell gesture activation is claimed. Existing both-palms start and open-palm Back semantics are preserved on paper; review-only event buttons do not change detector thresholds.

## Privacy and cancellation policy proposals

Consent, temporary capture suspension, readiness and preview visibility remain separate. Opening Settings should preserve the current expectation of capture suspension while keeping the visitor’s decision; resumption uses the existing gate and only an eligible route. Assistant overlay capture suspension is still a product choice because current entry contexts differ. Privacy stop must deny capture immediately, then cancel owned operations safely. The prototype’s privacy screen is explanatory and does not grant permission.

End/Restart is a deferred business feature. Its confirmation must state what clears: selected performer, conversation, history, awakening flags and consent, with language persistence considered independently. Cancel preserves the session. No reset is executed here. Safe exit destination, consent-free pointer participation and post-Souvenir next visitor behavior remain in [approval checklist](DesignApprovalChecklist.md).
