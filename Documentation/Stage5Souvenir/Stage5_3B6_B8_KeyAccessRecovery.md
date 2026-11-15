# Stages 5.3B6–B8 — Windows TPM key access recovery

**PASS for credential replacement and authentication-only validation.** Native Windows PowerShell, Unity Editor, and the Windows x64 validation Player authenticated with the replacement TPM key. The original key and retired registration remain preserved.

## Authorized replacement results

| Check | Result |
| --- | --- |
| Hardware and scope | Microsoft Platform Crypto Provider reported hardware implementation. The replacement is named, persisted ECDSA P-256 in the normal current-user store, with export policy `None` and no software fallback. |
| Independent access | Windows launched standard 64-bit Windows PowerShell via the native process service under the same SID and desktop session. The creation process exited before two independently launched processes reopened the same key in the conventional profile. A verification process signed a local digest and verified the signature against the public key. The key container path was verified using file attributes only. |
| Replacement identity | operator-held kiosk ID (omitted from the public report). Public SPKI SHA-256 `5d5c810346cd223ad5163a8e2ed8691c067478527cb39c51c3714e3b549cb9ae`. The previous fingerprint was `d57d45a500fc447e5172cc94556e89f512cde63e2568773dbb10dded35695247`. |
| Enrollment metadata | The normal Windows process atomically installed the replacement public metadata in the existing profile enrollment file and backed up the old public metadata locally. |
| D1 rotation | Staged the replacement inactive and matched its public fingerprint. One guarded D1 batch recorded both fingerprints in the audit trail, revoked old sessions, deactivated the old registration, incremented its key version, and activated the replacement. The old registration remains in D1. Exactly one kiosk is active. |
| Cloudflare from native PowerShell | New challenge and proof returned HTTP 200, issuing a 300-second session. Replaying the proof and requesting a challenge for the retired kiosk each returned HTTP 401. |
| Unity Editor | PASS using the existing Windows signer and souvenir client's authentication method in a temporary Play Mode probe. Cloudflare accepted the signature and issued a session. The probe exited Play Mode without opening a visit. |
| Windows x64 Player | PASS. A Windows x64 development validation build compiled with zero errors. A native Windows process launched it with the authentication-only switch. The existing signer and souvenir client opened the same replacement key, signed a fresh challenge, received a session, and exited without opening a visit. |
| Session cleanup | D1 confirmed all four successor test sessions were revoked, and zero unrevoked sessions remain for either kiosk registration. |
| Final issuance state | D1 confirmed issuance paused, one active kiosk, an empty public catalog, one rotation audit record, and the same three pre-existing assignments. No new visit or assignment was created. Production test authentication remained disabled. |

The credential cutover used the existing database and Worker. [Cloudflare D1 batch query API](https://developers.cloudflare.com/api/resources/d1/subresources/database/methods/query/)

Temporary probe scripts were removed and Unity's automatic build-time asset and settings changes were restored. The saved exhibition scene and integration code were preserved. Reboot persistence, a live visitor run, and phone testing were outside this authentication-only task.

## Original diagnosis

The persisted key container belongs to Codex's private Windows AppData view. A fresh process launched from the original enrollment environment could reopen the original key and sign locally. A separately launched native process under the same Windows identity could not see its container and received `NTE_BAD_KEYSET (0x80090016)` at `NCryptOpenKey`. The failure reproduced independently of Unity.

## Evidence

| Check | Result |
| --- | --- |
| Windows identity and profile | Both fresh processes had the same Windows user SID, the same logon authentication ID, and the same interactive desktop session. Both were 64-bit, non-elevated, and had a loaded user registry hive. SID comparison used the actual token SID. |
| Provider and flags | Both opened Microsoft Platform Crypto Provider and reported hardware implementation. Both used the exact existing kiosk key name and legacy key spec 0. The original environment opened it with both current-user flag variants (silent and non-silent). The native process returned `NTE_BAD_KEYSET` with both. Machine-scope probes returned `0x80090010`; the original key itself reported current-user scope. |
| Key persistence and custody | The reopened key reported non-ephemeral storage, 256 bits, and export policy 0. Its public-key fingerprint matched the enrolled fingerprint `d57d45a500fc447e5172cc94556e89f512cde63e2568773dbb10dded35695247`. A new process opened new handles and produced a local 64-byte signature, then released the handles. No previous open handle was needed. |
| Container location | Correlated the provider's unique key name to a single file. A Windows handle opened for file attributes only resolved the file to the private AppData store of the `OpenAI.Codex` package. The native process reported no file at the corresponding conventional user-store path. Both shells reported no direct package identity from `GetCurrentPackageFullName`; the resolved file path establishes the storage boundary. |
| Permissions | The successful handle allowed a read-only DACL query. The native process failed to locate the key before it could obtain a handle. No key or directory permissions were changed. |
| Unity Editor | Stage B6–B8 directly observed `0x80090016` in the signer even after the existing public enrollment metadata was made visible to Unity. The native probe now explains that result. Play Mode was not repeated once the storage boundary was confirmed. |
| Existing Windows Player | Verified the existing executable is x64 and its managed assembly contains the Windows signer and `NCryptOpenKey`. This Player has no authentication-only control, so no in-Player login was attempted for this diagnostic. Earlier Player build and launch passed; live Player authentication remains blocked. |

Only public-key bytes were read to compute the fingerprint. Container inspection used names and attributes. No container was copied, no private material was exported, and no software key was used. No production code or Unity scene was changed.

Microsoft documents that `NCryptOpenKey` defaults to the current-user store and returns `NTE_BAD_KEYSET` when the named key cannot be found. Its packaged-desktop documentation describes the separate AppData views for packaged and unpackaged processes. [NCryptOpenKey](https://learn.microsoft.com/en-us/windows/win32/api/ncrypt/nf-ncrypt-ncryptopenkey) · [Packaged desktop AppData behavior](https://learn.microsoft.com/en-us/windows/uwp/porting/desktop-to-uwp-behind-the-scenes)

## Prior remediation plan

1. Launch standard 64-bit Windows PowerShell from the normal interactive exhibition desktop under the account that will run Unity. Verify the SID and provider view with a second independently launched native process. Use the Microsoft Platform Crypto Provider to create a new named, persisted, current-user ECDSA P-256 signing key with `CngExportPolicies.None`. Use a fresh unique kiosk ID as the key name to match the current signer contract. Confirm the hardware flag, key scope, public-key fingerprint, and successful local signing in both processes before touching D1.
2. Stage only the replacement **public** SPKI as base64url and the non-secret kiosk metadata. Save the enrollment JSON under the normal profile’s `%LOCALAPPDATA%\AwakeningCelebrations\SouvenirKiosk` with the existing Worker origin. Preserve the original local key and public registration history.
3. Through the restricted Wrangler D1 operator workflow, stage the replacement registration inactive. In one audited cutover, verify the staged public fingerprint, deactivate the old registration, invalidate its key version and revoke its current sessions, and activate the successor. Keep no more than one active registration for the laptop. Do not change the production auth or issuance pause.
4. Verify fresh Cloudflare challenge authentication from the normal shell, then run authentication-only checks in Unity Editor and Windows Player. Confirm the same successor fingerprint is used and the retired registration is rejected. Keep visit creation and assignment out of this verification.

## Hosted state before replacement

A read-only D1 check confirmed issuance paused, an empty public catalog, zero active test grants, zero active kiosk sessions, and the same three historical assignments. No new hosted challenge or session was requested because native recovery failed. No credential rotation, deployment, assignment, rehearsal or reboot occurred.
