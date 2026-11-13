# Deferred and conditional feature interfaces

The user's prior clarification confirms these capabilities were deliberately deferred, including the Souvenir placeholder. This task approves their design coverage, not their functional implementation. Preserve sequencing: primary overhaul → stabilization/authorized cleanup → separately authorized deferred functions. Existing scaffolding is not deleted.

| Feature | Living Glass interface | State/return contract and recommendation |
| --- | --- | --- |
| Sound / 聲音 | Settings row and a future reading subview; master/environment scope, mute and output status | Include. Current preview disabled with reason. Later current/applying/applied/output error/cancel require actual audio service. Approve defaults/persistence and retain muted portrait-video policy unless separately changed |
| Accessibility / 無障礙 | Settings row; motion/effects/text preferences in shared reading surface | Include. Readable defaults apply across the core redesign; preference business logic remains deferred. External preview overrides are not saved settings. Applied/error/cancel reflect actual implementation only |
| Camera & tracking / 相機與追蹤 | Settings→Privacy, global status and contextual tracking feedback | Include status. Separate consent/capture suspension/device readiness/hand status/preview. No auto-grant or second capture pipeline. Source capture-suspension differences require policy approval |
| Safe End / Restart / 結束／重新開始 | Settings row → consequence confirmation; safe exit separated from full session reset | Include design. List actual fields to clear, destination, Cancel and real cleanup pending/failure. No reset performed. Immediate Activation exit remains pending approval |
| Contextual Help / 情境說明 | Global Help; source gesture cues plus pointer/keyboard alternatives; tutorial replay and conditional alignment | Recommend merging redundant Guide into Help, while preserving tutorial route/content. Replay cannot silently discard session |
| Calibration / 校正 | Conditional Help subview with explanation and Cancel | Retain optional design, exclude from primary navigation until measured alignment need and a real algorithm are approved. Consent/device check→stable acquire→evaluate→actual result/retry. No fabricated calibration success |
| Souvenir / 紀念留存 | Closing→optional artwork/content preview; Done always available | Retain conditional design. Require useful content, real destination, expiry/retention/privacy and service approval. Unavailable/preparing/ready/expired/offline/retry/cancel layouts share the panel. No pseudo QR, generated code, invented URL or service |

`DeferredFeatureStates.csv` carries 53 paired EN/Traditional Chinese state entries inherited from Stage 2A. Success/applied/ready labels are **future state specifications** and only appear after a real capability confirms the outcome; they are not simulated in the prototype. `BilingualCopy.csv` adds Living Glass-specific navigation and interaction copy.

## Reserved subview anatomy

Sound and Accessibility use title, concise description, grouped rows, current values, error/status and explicit Cancel/Apply only if the eventual capability warrants transactional settings. Camera uses a status list, permission explanation and revoke; preview visibility is a separate row. End/reset uses a readable consequence list and explicit confirmation, never a generic “OK”. Calibration shows one stable target only when an approved algorithm is active. Souvenir hides the delivery region until actual content exists, removes expired codes before refresh and allows Done even offline.

Use the same focus, scroll, close and bilingual rules as Settings/AI. No deferred subview is allowed to resurrect a camera or request after dismissal. Ordinary unavailable rows should explain the missing capability without pretending it is toggled on. The prototype intentionally has no functional sound slider, preference persistence, reset, capture grant, calibration evaluation or delivery.
