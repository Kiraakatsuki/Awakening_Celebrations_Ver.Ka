# Stage 5.3B3 — Production security preparation

**Status** — Local validation passed. Cloudflare account checks, Windows key enrollment, and hosted acceptance are pending.

## Files and fixture

| Path under `souvenir-service/` | Change |
| --- | --- |
| `src/kiosk.ts`, `src/index.ts` | Kiosk challenges and sessions, production configuration, authenticated visit reset and request admission |
| `src/store.ts`, `migrations/0003_production_preparation.sql` | Key and session records, visit ceilings, scoped counters, assignment pause, and local catalog fixture |
| `src/claim-page.ts` | Same-origin mobile claim page with protected preview and download |
| `assets/media/placeholder-test.webp`, `.gitignore` | Single reproducible test image. Other local catalog images remain ignored. |
| `wrangler.jsonc` | Explicit local mode, observability disabled, existing Worker-first asset routing retained |
| `test/foundation.test.ts` | Production-auth, quota, catalog isolation, and real-asset coverage |

The placeholder is an original, clearly labeled 800 × 500 WebP, 10,178 bytes. SHA-256 is `9a6783283e73c669fab1dd3ab11f1a305919b4218674536dd2744f57a651e07d`. Migration 0003 makes it the sole active local catalog entry. The six original metadata-only entries remain available as fixtures. Production catalog selection and claim resolution exclude test assets.

## Kiosk enrollment and key custody

1. A trusted operator registers a unique kiosk ID and base64url DER SubjectPublicKeyInfo for its ECDSA P-256 public key in `kiosk_registry`. Use dedicated production D1 data with only authorized installations. The Worker has no credential enrollment route.
2. The kiosk calls `POST /api/kiosks/challenge` with `{ kioskId }`. It gets `challengeId`, `expiresAt` (UTC Unix seconds), and the exact message. Before signing, the client verifies that the message consists of these UTF-8 lines, separated by LF with no final LF.

   ```text
   awakening-souvenir/kiosk/v1
   <configured HTTPS origin>
   <expected kiosk ID>
   <challenge ID>
   <expiry in Unix seconds>
   ```

3. The client signs with ECDSA P-256 and SHA-256. Send `{ challengeId, signature }` to `POST /api/kiosks/session`, with the signature encoded as unpadded base64url of the 64-byte IEEE P1363 `r || s` form. D1 consumes the challenge in the same batch that creates the session. The challenge lasts 60 seconds and the hash-only bearer session lasts five minutes. Reauthenticate on expiry using a new challenge. Keep the visit ID and visit token scoped to the current visitor and reuse them after session renewal.
4. Use the session as `Authorization: Bearer` on kiosk operations. Start with an `Idempotency-Key`, then include `X-Visit-Token` for progress, completion, assignment, and `POST /api/visits/{id}/reset` with `{}`. D1 keeps one open visit per kiosk. On successful assignment the service closes the visit, while the same visit can recover the identical claim through a retry. Reset closes an abandoned visit and bars first issuance for it. The stored journey must advance through entry, journey start, journey completion and Closing. The eventual Unity binding must prove it emits these events from the real routes. An enrolled kiosk under attacker control can report false progress.
5. Disable a kiosk by clearing its `active` flag. Revoke a session through its `revoked_at` field or increment `key_version` to invalidate sessions and outstanding challenges together. Rotate a public key and increment the version in one update before re-enabling a compromised kiosk. Operate these controls through restricted D1 access.

For Windows, provision a non-exportable P-256 key with Windows CNG, preferably TPM-backed if supported. Restrict signing access to the kiosk application principal. The future Unity adapter should use the platform signer and keep bearer sessions in memory. Git and Unity assets carry only the public origin and protocol configuration. The Worker needs an independently generated 256-bit `TOKEN_KEY` in protected secret storage for visit, QR and image grants. Changing it invalidates outstanding credentials, so plan any rotation around their validity window. Production requires the exact HTTPS `PUBLIC_ORIGIN`, `LOCAL_TEST_AUTH=disabled`, and no `LOCAL_KIOSK_KEY`. Local test auth requires explicit opt-in and loopback. Windows key custody and client integration are pending.

## Quotas and protected claims

- **100 new assignments per UTC day globally.** A D1 batch checks eligibility, pause and quota, then records the assignment, QR and counter together. The unique visit constraint makes concurrent retries converge on the same result. Authorized retries preserve the original assignment even while issuance is paused. Use the restricted D1 operator control `UPDATE souvenir_controls SET issuance_paused = 1 WHERE id = 1` to pause.
- **100 new visits per kiosk per UTC day** by default. Abandoned visits count. For one demo kiosk, `VISITS_PER_KIOSK_PER_DAY=125` leaves room for abandonment while the global assignment cap stays at 100. Multiple kiosks share the same global allocation. Authenticated kiosk traffic defaults to 60/minute and 5,000/day, with an additional assignment-attempt cap of 24/minute and 500/day per kiosk.
- Public minute/day limits are separate D1 buckets. Kiosk authentication is 60/500, catalog 30/500, claim resolution 60/1,200, preview 60/1,000 and download 40/500. Invalid kiosk sessions cannot spend kiosk or assignment allocations. A flood can exhaust a public bucket, interrupt new kiosk authentication, or consume the provider request allowance before an application check runs. Use provider usage monitoring and the issuance pause during the demo.
- QR validity is fixed at 30 minutes by default and capped at 60 minutes. Repeat scans resolve to the original assignment. Expired or revoked claims return HTTP 410, and expired visits cannot draw another souvenir. The production QR puts the unchanged bearer token in `/claim#…`. The page removes the fragment from the address bar and sends the token in a same-origin POST. Preview and download use separate grants valid for up to five minutes and capped by QR expiry. Delivery rechecks expiry and revocation before streaming. A copy already delivered can still be saved or shared.
- Worker-first routing intercepts every asset request. Only the authorized handler uses the Static Assets binding, with an allowlisted WebP path resolved from D1. The response is `no-store` and `no-referrer`. The page uses a nonce-based CSP, with same-origin fetches and browser-local image previews. Application code emits no raw tokens. Keep request body/header capture and URL-bearing invocation logs disabled. The loopback fixture retains the earlier token-in-path test route.

## Free-tier deployment preflight

Official limits checked November 15, 2026. Workers Free allows 100,000 requests per day and 10 ms CPU per request. D1 Free includes 5 million rows read/day, 100,000 rows written/day and 5 GB total storage. Worker-first Static Assets delivery uses Worker invocations and is subject to Worker limits. [Workers limits](https://developers.cloudflare.com/workers/platform/limits/) · [D1 pricing](https://developers.cloudflare.com/d1/platform/pricing/) · [Static Assets billing and limits](https://developers.cloudflare.com/workers/static-assets/billing-and-limitations/)

1. In the target Cloudflare account, open **Workers & Pages** and verify the account's Workers plan is **Free**. In **Manage Account → Billing → Subscriptions**, verify Workers Paid and paid add-ons are absent from the proposed setup. Review **Billing → Billable Usage** for metered products. Reject R2, paid subscriptions, automatic upgrades, and unapproved services or bindings. Application limits are operational controls. The account plan and billing check are the cost gate. [Workers pricing](https://developers.cloudflare.com/workers/platform/pricing/) · [Cloudflare billing dashboard](https://developers.cloudflare.com/billing/understand/how-billing-works/)
2. Require production mode, an approved HTTPS route on the exact configured origin, explicit test-auth disablement, no local test secret, a secure token key, enrolled kiosk credentials and migrated D1. Keep preview URLs disabled. The local config disables `workers.dev`, so confirm an approved existing HTTPS route or review use of a free Worker hostname separately before provisioning. Reject a missing credential, placeholder D1 ID, or build containing test imagery.
3. Require `assets.run_worker_first: true` across all routes and keep the asset binding internal. Reject a second asset-only host, directory listing, public media mapping, cache overrides, or logging that records credentials. Review effective dashboard settings as well as configuration. [Worker routing](https://developers.cloudflare.com/workers/static-assets/billing-and-limitations/) · [Workers Logs](https://developers.cloudflare.com/workers/observability/logs/workers-logs/)

## Validation

| Check | Result |
| --- | --- |
| `pnpm typecheck` | PASS |
| `pnpm test` | PASS, 21 tests. Covered migration, fixed expiry and revocation, rollback, duplicate issuance, key proof and replay, session revocation, reset, quota isolation, and direct asset path checks. |
| Local Wrangler HTTP smoke test | PASS. Applied all three migrations to an isolated D1 store. Preview and download matched the fixture SHA-256 and returned the expected disposition and private cache policy. Direct media URL returned 404. After a real Worker restart against that store, retry returned the same assignment, QR and expiry, and the image bytes still matched. |
| Cloudflare hosted acceptance | PENDING. Check all five direct asset path variants, aliases and methods on the approved hostname. Verify mobile QR scanning, file saving, CDN cache isolation, revocation, log redaction, production credential use and bounded demo load under Free limits. Account billing and quotas were reviewed from docs only. |

## Replace the fixture with approved artwork

Validate each approved input, convert to WebP, record its digest and bilingual metadata, and use six new stable catalog IDs and immutable media paths. Stage only those six authorized files in the eventual production Static Assets artifact. Register their catalog rows as inactive, verify every binding path and digest, then activate the reviewed set together. Exclude the placeholder from the production artifact and keep it local for regression tests. Keep retired asset versions available to their existing, still-valid assignments.
