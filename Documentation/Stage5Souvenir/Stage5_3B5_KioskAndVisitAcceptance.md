# Stage 5.3B5 — Windows kiosk and visit acceptance

**PASS for controlled production-protocol acceptance.** One laptop was enrolled and one GAIA API visit received one placeholder souvenir. Public issuance stayed paused. Unity integration was outside this test.

## Kiosk enrollment

| Item | Verified result |
| --- | --- |
| Laptop | ASUS ROG Strix G614JV, Windows 11 Home Single Language, build 26300 |
| TPM | Intel TPM 2.0, initialized and ready for storage. The CNG provider reported hardware implementation. |
| Key | ECDSA P-256 signing key in Microsoft Platform Crypto Provider, current-user persisted CNG storage. `ExportPolicy=None`. No software fallback or private-key export. The key signed successfully after reopening in separate PowerShell processes. |
| Kiosk ID | operator-held kiosk ID (omitted from the public report) |
| Public-key registration | Registered through restricted Wrangler D1 access. The stored public key matched the local public key. Public SPKI SHA-256 `d57d45a500fc447e5172cc94556e89f512cde63e2568773dbb10dded35695247`. |
| Local configuration | Non-secret enrollment metadata under `%LOCALAPPDATA%\AwakeningCelebrations\SouvenirKiosk\enrollment.json`. The private key is held by Windows CNG. Visit, session and QR credentials stayed in process memory or were passed to the fresh client through redirected stdin for the recovery check. |

Windows reported the hardware provider through `NCryptGetProperty` on the provider handle. Non-exportability was checked through the persisted key policy. The Worker verifies signatures with the registered public key. It does not perform remote TPM attestation. [Microsoft Platform Crypto Provider](https://learn.microsoft.com/en-us/windows/win32/seccertenroll/cng-key-storage-providers) · [CNG implementation and export properties](https://learn.microsoft.com/en-us/windows/win32/seccng/key-storage-property-identifiers)

## Live authentication and visit

| Check | Hosted result |
| --- | --- |
| Challenge and login | HTTP 200 for the registered kiosk. The Windows key signed the origin-, kiosk-, challenge- and expiry-bound message. Cloudflare accepted the signature. The challenge and session lifetimes were within 60 and 300 seconds respectively. |
| Authentication denials | HTTP 401 for an invalid signature, unknown kiosk, replay, expired challenge, expired session, revoked session, disabled-kiosk login and existing session, old session after kiosk reactivation, and a synthetic test-style bearer. The expiry cases used operator-shortened D1 timestamps. The kiosk was restored with an incremented key version. |
| Journey mapping | Reviewed `AwakeningApplicationController.cs` and `LivingGlassFlowIntegration.cs`. The existing flow starts at Home, confirms a performer from the Gallery, runs the performer sequence, then reaches Closing. The acceptance client submitted the existing backend milestones `entered`, `journey-started`, `journey-finished`, `closing-reached`. The client drove the live API. Unity did not run or emit these events. |
| Visit validation | Visit `7fb7d081-ed88-496a-89b5-2b90eb74b1e1`, performer `GAIA`. Four ordered milestones and completion returned HTTP 200. Out-of-order progress, premature completion and premature assignment each returned HTTP 409. A complete visit without authorization remained paused with HTTP 503. |
| Assignment | Four concurrent calls returned one HTTP 201 and three HTTP 200 responses with identical assignment identity, QR and expiry. D1 confirmed exactly one row for the visit and a closed visit. Assignment `03d536b9-e71c-413b-9a3a-f230588454e3` used `placeholder-test`. |
| QR and protected image | Issued 2026-11-15 21:18:42 Taipei time. Fixed expiry 21:48:42, exactly 1,800 seconds later. Two rescans kept the original expiry. Protected preview and download returned HTTP 200 and matched the existing WebP SHA-256. |
| Recovery | A fresh PowerShell process reopened the same persisted key, authenticated with a new challenge and recovered the identical assignment, QR and expiry. A separate D1 read confirmed the persisted timestamps and assignment. A Windows reboot was outside the test. |
| Revocation | Revoked the test issuance authorization. A retry before individual QR revocation returned the existing assignment only. After QR revocation, claim resolution, the prior download grant and assignment retry each returned HTTP 410. Natural expiry of this QR was not timed because it was revoked after the test. The fixed deadline was independently checked. Stage 5.3B4 previously exercised hosted expired QR handling. |

## Controlled authorization and final state

Migration `0004_controlled_kiosk_acceptance.sql` adds D1 operator grants scoped to an exact kiosk and visit. The live grant was valid for at most ten minutes and could be consumed only once. A D1 batch checked the completed visit, exact enrolled kiosk, unexpired grant, test-only asset and D1 pause, then atomically stored the assignment, QR, consumed grant and visit closure. The Worker requires its own issuance pause for this path. The same production kiosk authentication and visit credentials apply. The catalog excludes the placeholder and grants are created through restricted D1 administration.

After the test, an independent query confirmed **one enrolled kiosk, zero active test sessions, zero reusable grants, zero public catalog entries, and D1 issuance paused**. The temporary authorization and QR are revoked. The grant, consumption and revocation audit records remain. The service stays on the existing [Worker](https://awakening-souvenir.awakening-celebrations-verka.workers.dev/health), version `85294e63-f47e-4dcc-9c0a-b10aa019734b` at 100% of traffic, with production test authentication disabled. No new cloud resources or paid features were created.

TypeScript and all 22 souvenir tests passed. Changes were limited to `souvenir-service/src/index.ts`, `src/store.ts`, `migrations/0004_controlled_kiosk_acceptance.sql`, `test/foundation.test.ts` and this report. The local acceptance harness is in ignored `souvenir-service/.wrangler/b5-acceptance.ps1`. Unity, Render and the existing AI and voice systems were untouched. No GitHub push occurred.

## Recovery and revocation

On the same Windows user profile, open the persisted platform key using the enrollment key name and authenticate with a fresh challenge. If the laptop is lost, disable its D1 kiosk record and revoke outstanding sessions or increment its key version. If the TPM key is lost, provision a new non-exportable key and register the replacement public key through restricted D1 administration, with an incremented version. The private key is intentionally unrecoverable from a copy of the configuration. Restore kiosk access only after verifying the new signature path. Keep visitor issuance paused until Unity integration and visitor acceptance are explicitly authorized.
