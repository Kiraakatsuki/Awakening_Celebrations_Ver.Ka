# Stage 5.3B2 — Souvenir security and reliability audit

The Worker passed the local foundation audit after four targeted fixes. Production release remains unapproved and unvalidated. The audit used the local Workers runtime, Wrangler and a separate persisted synthetic D1 database on loopback.

## Findings by severity

| Severity | Finding and reproduction | Resolution |
| --- | --- | --- |
| Medium | **Revocation during asset lookup.** Resolve a valid claim and request its image. Make the test asset binding revoke the QR before returning its asset body. The baseline still responded 200. | Recheck the QR, asset path and grant expiry after the asset lookup and before returning bytes. The same test now returns 410. Bytes already sent on a stream remain outside revocation. |
| Medium | **Test authentication lacked an explicit disable switch.** With synthetic keys supplied and `LOCAL_TEST_AUTH=disabled`, send an authenticated visit request with a loopback URL. The baseline created the visit. | Wrangler config defaults to disabled. Only an explicit local opt-in plus loopback enables the app, including when secrets are present. The retest returned 404 and stored no visit. The local example and the existing ignored local vars were updated. |
| Medium | **Unauthenticated kiosk calls could exhaust the request budget.** Set the minute cap to one. Send an unauthenticated POST to `/api/visits`, then request the catalog. The baseline returned 401 followed by 429. | Check the kiosk bearer before reserving the shared quota on visit routes. The rejected POST still returns 401 and the following catalog request succeeds. Recognized public API calls can still exhaust the shared quota. |
| Low | **Default Wrangler request logs contained QR bearers.** Fetch a synthetic claim with default info logging and inspect the local output. A request line contained the full token. | The project dev command now logs at error level. The restart retest issued a valid claim request with no bearer in Wrangler output. This control applies to the project dev command. Future production access logs still need a redaction or exchange design. |

No critical or high-severity defect was reproduced in the reviewed local surface.

## Reproduction and evidence

From `souvenir-service/` run `pnpm typecheck` and `pnpm test`. The complete suite passed **17/17** on the local Workers runtime. Three added regressions failed as described above on the Stage 5.3B1 baseline and passed after the fixes. The Wrangler logging finding was reproduced separately over HTTP.

| Audit area | Adversarial check and result |
| --- | --- |
| D1 transactions and completion | **PASS.** Parallel completion requests returned one stored timestamp, including after assignment. A temporary trigger aborted the QR insert mid-batch. The assignment, QR, issuance counter and visit closure were absent after rollback. Retry succeeded after removing the trigger. |
| Assignment concurrency | **PASS.** Twelve independent loopback HTTP requests using closed connections yielded one HTTP 201 and eleven retries with identical assignment and QR. The automated test verified one assignment row, one QR row and one issuance charge. |
| QR entropy, expiry and replay | **PASS.** Reviewed the 32-byte Web Crypto nonce, purpose-bound HMAC-SHA-256 and unique indexed SHA-256 stored hash. Six repeat claims returned the same assignment and expiry. Tests exercised the fixed 30-minute default, rejected a lifetime beyond 60 minutes, crossed an actual expiry boundary, verified HTTP 410 after expiry and revocation, and checked irreversible revocation and its audit entry. Entropy assessment is by construction and code inspection. |
| Private delivery and headers | **PASS.** Guessed QR paths returned 404 and missing or forged grants returned 401 without asset access. Five requests for a real temporary text asset returned 404, including GET, HEAD, encoded path, dot-segment path and guessed query grant. The authenticated stream test used synthetic bytes with a mocked asset binding. Claim responses used `no-store`, `no-referrer` and `nosniff`, and denied cross-origin preflight provided no CORS allow header. |
| Input validation and failure handling | **PASS.** Malformed visit IDs returned 404, well-formed unknown IDs without a matching credential returned 401, and out-of-order or incomplete milestones returned 409. A simulated database error returned a generic 503 without internal details. |
| Quotas and local isolation | **PASS.** Issuance exhaustion returned 429 while allowing a retry of the existing assignment. At a daily request cap of three, twelve concurrent public calls yielded exactly three successes and nine denials, with a D1 count of three. Disabled local auth rejected requests even with valid local credentials. A non-loopback hostname returned 404. |
| Persisted restart | **PASS.** Ran Wrangler twice on loopback with the same isolated `--persist-to` directory and synthetic secrets. After a clean stop and restart, completion and assignment retries returned the original timestamp, assignment ID, QR and expiry. The repeated claim still resolved. |

To repeat the HTTP checks, apply migrations using `pnpm exec wrangler d1 migrations apply DB --local --persist-to .wrangler/security-audit`. Create an ignored local test vars file with `LOCAL_TEST_AUTH=enabled` and separate synthetic keys. Start `pnpm dev --port 8791 --persist-to .wrangler/security-audit --env-file .dev.vars.audit`. Use the [local API contract](Stage5_3B1_LocalFoundation.md) to create a visit, submit its four milestones and send parallel completion and assignment requests. Stop and restart with the same flags and retry with the same credentials. For the routing check, put a disposable text marker under ignored `assets/media/` and try the direct path and its variants. The audit listener, database, temporary keys and marker were removed afterward.

## Production readiness

**Local validation passed. Production readiness has not been established.** The kiosk bearer is a test credential. Production enrollment, authoritative milestone verification, key lifecycle and rotation, and trusted operator access for revocation still need decisions. Public claim and catalog access can consume the shared application quota, and local D1 tests provide no evidence for edge-level denial-of-service controls or production quota behavior. Production claim transport and proxy/access-log redaction need verification because path bearers remain in the contract. Verify private media packaging, Worker-first routing and failure behavior in the eventual hosted environment before enabling real assets. The authenticated stream test used a synthetic binding, and the hosted asset boundary was outside this audit.

Files changed are `src/index.ts`, `test/foundation.test.ts`, `wrangler.jsonc`, `vitest.config.ts`, `package.json` and `.dev.vars.example` under `souvenir-service/`, plus this report. For an existing local checkout, add `LOCAL_TEST_AUTH=enabled` to its ignored `.dev.vars` before running the dev server.

[D1 batch transactions](https://developers.cloudflare.com/d1/worker-api/d1-database/#batch) · [Worker-first Static Assets configuration](https://developers.cloudflare.com/workers/static-assets/binding/#run_worker_first)
