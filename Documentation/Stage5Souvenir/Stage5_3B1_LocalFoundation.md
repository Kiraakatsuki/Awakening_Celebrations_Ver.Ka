# Stage 5.3B1 — Local souvenir foundation

The independent TypeScript Worker lives in `souvenir-service/`. Wrangler runs on loopback and uses an emulated D1 database and the Worker-first Static Assets binding. The configured database ID is an emulator identifier. Public and preview deployment routes and Wrangler usage metrics are disabled in the project configuration. There is no alternative CLI, shared backend code or remote deployment script.

[Wrangler](https://developers.cloudflare.com/workers/wrangler/) · [D1 batches](https://developers.cloudflare.com/d1/worker-api/d1-database/#batch) · [Worker-first routing](https://developers.cloudflare.com/workers/static-assets/binding/#run_worker_first)

## Local setup

Use Node 22.12 or later and pnpm 11.25. All commands below run from `souvenir-service/`.

```powershell
pnpm install --frozen-lockfile
if (!(Test-Path -LiteralPath .dev.vars)) {
    Copy-Item -LiteralPath .dev.vars.example -Destination .dev.vars
}
pnpm db:migrate
pnpm dev
```

For a fresh setup, fill the two entries in `.dev.vars` before starting the Worker. Generate two different random values with at least 32 bytes of entropy each and keep them stable for the life of the local database. Never copy the example over an existing secret file. Wrangler binds to `127.0.0.1:8787`. The migrations and server commands explicitly use local mode. `.wrangler/`, `.dev.vars` and local media are ignored. Resetting the emulator or changing the token key invalidates existing local credentials. Restarting with the same files preserves issued assignments.

`0001_foundation.sql` defines the eight tables, integrity constraints, quota counters and revocation audit. `0002_local_catalog.sql` adds one synthetic kiosk and six metadata rows. These can be drawn locally, with `hasImage: false`. The schema requires recorded completion for assignment and prevents editing issued QR timestamps or undoing recorded revocations. To revoke in local D1, set `revoked_at = unixepoch()`, `revoked_by` and `revocation_reason` on the target `qr_tokens.assignment_id` with `WHERE revoked_at IS NULL`. Use the local Wrangler D1 shell/command with `--local`. The same update records an audit event. There is no public or local HTTP admin endpoint.

## HTTP contract

All errors return `{ "error": { "code": "...", "message": "..." } }`. POST calls use JSON. Kiosk routes require `Authorization: Bearer <LOCAL_KIOSK_KEY>` and visit operations additionally require the returned `X-Visit-Token`. Visit and QR credentials are separate.

| Endpoint | Request and result |
| --- | --- |
| `GET /health` | 200 if configuration and migrated local catalog are usable. Dependency failures return 503. |
| `GET /api/souvenirs/catalog` | 200 with active fixture IDs, bilingual titles and `hasImage`. |
| `POST /api/visits` | `Idempotency-Key` header and `{"performer":"GAIA"}`, or any of the other five existing performer IDs. Returns visit ID, bearer and next milestone. A second open visit or reuse of the key with a different performer returns 409. |
| `POST /api/visits/{id}/progress` | `{eventId, sequence, milestone}`. Submit sequences 1–4 in order: `entered`, `journey-started`, `journey-finished`, `closing-reached`. Identical retries succeed. Invalid order returns 409. |
| `POST /api/visits/{id}/complete` | `{}`. Returns the recorded completion time or 409 for missing progress. |
| `POST /api/visits/{id}/assignment` | `{}`. Returns the same assignment, souvenir and claim URL on retry, 201 on first issue and 200 for a valid retry. Incomplete returns 409, expired/revoked returns 410, and an issuance cap returns 429. |
| `GET /api/claims/{token}` | Returns the original souvenir, expiry and an image grant when a file is configured. Unknown returns 404, expired or revoked returns 410. |
| `GET /api/claims/{token}/image` | Use the returned image authorization as a Bearer. Missing or invalid grant returns 401. Expiry/revocation returns 410. Authorized requests stream the assignment's allowlisted WebP only. |

Issuance draws uniformly with cryptographic rejection sampling. A D1 batch compares the eligible catalog snapshot, commits at most one assignment per visit with its QR hash, and charges issuance only to the winner. Lost responses and concurrent requests recover the committed row and original credential. A completed visit may request its assignment after a transient outage. Issuance closes the open visit, and idle visits are closed as needed after 20 minutes on a later create request. Fixture milestones are server-ordered claims from an authorized local test client. They are not proof of a production journey.

The QR has fixed server UTC expiry, 1,800 seconds by default, with a hard 3,600-second configuration and storage maximum. A fresh random nonce and purpose-bound HMAC produce the QR bearer. D1 stores the hash and nonce, not the raw token. Image grants are scoped to one assignment and expire within five minutes and no later than the QR. Each media request rechecks expiry and revocation. `assets/media/` is ignored to prevent committing supplied art to the public repository. Later, validate and approve immutable WebP files through a separately authorized private workflow, put them in that directory and associate their exact `/media/<version>.webp` path with the approved catalog row. The Worker denies direct asset URLs. Already downloaded copies cannot be revoked.

Default app limits are 60 requests per minute, 5,000 per UTC day and 100 assignments per UTC day. Configurable maxima are 600, 10,000 and 500 respectively. Recognized API requests consume the shared request budget; health and unmatched routes do not. These DB counters fail closed and add writes to accepted calls. They are local starting limits, not a verified production load envelope or a replacement for edge abuse controls.

## Validation and remaining decisions

Both migrations applied to local D1. `pnpm typecheck` passed and all 10 tests passed. The Workers Vitest suite covers the six metadata entries, invalid and duplicate progress, 12 simultaneous assignment requests for one visit, issuance/request ceilings, six-way random sampling, retirement, fixed expiry, revocation audit, protected streaming with synthetic bytes and dependency failure. A separate Wrangler loopback smoke test returned HTTP 200 for health and catalog and 404 for the direct URL of an existing temporary text asset. The probe was removed and the server stopped.

The shared kiosk secret is strictly a local test fixture. Production enrollment, lifecycle and key custody, real Unity milestone mapping, distributed rate limiting, log redaction and private asset ingestion need design approval before any deployment. Claim tokens are in API paths because this milestone specifies path endpoints. Wrangler can log paths locally. A public mobile page should use a fragment and a redacted exchange transport before release. Combined workload and Free-tier capacity were not tested. No real images, Unity integration, or remote resources are part of this milestone.

Files added under `souvenir-service/`: `.dev.vars.example`, `.gitignore`, `assets/.gitkeep`, `migrations/0001_foundation.sql`, `migrations/0002_local_catalog.sql`, `package.json`, `pnpm-lock.yaml`, `pnpm-workspace.yaml`, `src/index.ts`, `src/security.ts`, `src/store.ts`, `test/foundation.test.ts`, `tsconfig.json`, `vitest.config.ts` and `wrangler.jsonc`. This report is the only addition outside that directory.
