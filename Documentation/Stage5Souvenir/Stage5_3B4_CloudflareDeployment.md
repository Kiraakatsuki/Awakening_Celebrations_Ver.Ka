# Stage 5.3B4 — Controlled Cloudflare deployment

**Result — Deployed and verified on Workers Free. Visitor issuance remains paused.**

| Item | Verified on November 15, 2026 |
| --- | --- |
| Account | The intended account matched Wrangler OAuth and the dashboard, ID ending `c8c1`. Dashboard Billing → Subscriptions showed **Workers Free Active**. |
| Worker | [awakening-souvenir](https://awakening-souvenir.awakening-celebrations-verka.workers.dev) |
| Final version | `3ca96920-8c60-43b8-89d5-946b3d0a8d26` at 100% of traffic. Wrangler deployment ID `5e4d1424-9c36-44e3-887b-b5b59b4fa7d9`. |
| D1 | `awakening-souvenir-d1`, `c2443cd8-8553-417d-a4c5-e605ee28e779`, APAC. The account held zero D1 databases before creation. |
| Resources | One Worker, one Free D1 database, Static Assets, and the account's first free `workers.dev` subdomain. No payment method, paid upgrade, R2, paid monitoring, or Git-triggered deployment was added. |

Cloudflare documents up to ten D1 databases on Workers Free. Worker-first asset requests count toward the Worker quota. When the Free request limit is exhausted, Cloudflare returns HTTP 429 on Worker-first routes. Application counters control issuance and traffic admitted by the service. Public traffic can still exhaust provider quotas. [Workers Free limits](https://developers.cloudflare.com/workers/platform/limits/) · [D1 Free pricing](https://developers.cloudflare.com/d1/platform/pricing/) · [Static Assets billing](https://developers.cloudflare.com/workers/static-assets/billing-and-limitations/)

## Deployment configuration and data

- The original Wrangler configuration gained a `controlled` environment with the explicit account, D1 binding and exact HTTPS origin. It uses the existing Worker-first Static Assets setup, disables preview URLs and observability, and uses `SERVICE_MODE=production`, `LOCAL_TEST_AUTH=disabled` and `ISSUANCE_MODE=paused`. Cloudflare confirmed the route is enabled, preview routes are disabled, Logpush is off, and `TOKEN_KEY` is a secret binding. There is no local kiosk key binding. Application code does not log credentials. Downstream log capture was not independently audited.
- Applied all three migrations in order. `0001_foundation.sql` and `0002_local_catalog.sql` are the original two. `0003_production_preparation.sql` is required for kiosk challenges, auth, pause, limits and catalog isolation. Verified three migration records, the expected tables and triggers, the unique open-visit index and assignment uniqueness. An orphaned progress insert and a duplicate assignment insert were rejected by D1. `PRAGMA foreign_key_check` returned no violations.
- The D1 pause is set, the seeded local kiosk is inactive, and there are zero enrolled active kiosks. No production-eligible images are present in the catalog. The 100-assignment daily cap, per-kiosk limit, and 12/minute and 60/day per-claim ceilings remain configured. The normal API issuance counter has zero rows.
- Changed `souvenir-service/wrangler.jsonc`, `src/index.ts`, `src/store.ts`, and `test/foundation.test.ts` under the service. The narrowly scoped controlled-claim mechanism requires production mode, the deployment and D1 pauses, a matching stored QR hash, an allowlisted assignment ID, and the exact test-only placeholder. It cannot issue assignments or expose the placeholder in the public catalog. The local default-pause and per-claim safeguards were carried forward from the earlier preflight.

## Hosted acceptance

| Check | Result |
| --- | --- |
| Health, D1 connectivity, catalog and claim page | **PASS.** HTTP 200 for health, empty production catalog and claim page. The claim page had a restrictive CSP. |
| Anonymous and test authentication | **PASS.** Anonymous visit, inactive local kiosk challenge and unknown challenge proof returned HTTP 401. Final bindings independently confirm test auth is off. |
| Direct access and alternate paths | **PASS.** Direct GET and HEAD, encoded filename and slash, dot path, query guess, alternate assets path, directory, auxiliary asset and legacy claim URL returned HTTP 404. Retested the direct, encoded and alternate asset paths on the final version. |
| Authorized preview and download | **PASS on the controlled test version** `cf420fbc-5ac6-4670-8934-70369e55ab3e`. A restricted D1 operator fixture provided one live QR and one expired QR, while both issuance pauses stayed on. Live resolution returned HTTP 200 and repeated scans preserved expiry. Preview and download returned HTTP 200, the matching inline or attachment disposition, `image/webp`, and the expected 10,178 bytes and SHA-256 `9a6783283e73c669fab1dd3ab11f1a305919b4218674536dd2744f57a651e07d`. Responses used `no-store` and `no-referrer`. A repeated preview did not show a cache hit. |
| Missing, wrong-purpose and expired credentials | **PASS.** Unknown QR or missing QR header returned HTTP 404. Missing image grant, a preview grant sent to download, and an expired grant-shaped request returned HTTP 401. A QR seeded past its fixed 30-minute window returned HTTP 410 for resolve and image access. The valid image grant was 300 seconds from the hosted response time. The expired grant-shaped request used synthetic fields. Natural expiry of an originally signed grant was not timed. |
| Revocation | **PASS.** Individual QR revocation returned HTTP 410 for resolution and both previously obtained grants. D1 recorded the operator audit action. |
| Redeployment and cleanup | **PASS.** A second deployment removed the temporary assignment allowlist. Both fixture QR tokens then returned HTTP 404. The expired and revoked synthetic records and revocation audit survived in D1. Health remained HTTP 200. |
| Valid Windows kiosk sign-in, visit and concurrent API assignment | **PENDING.** No Windows kiosk was enrolled. The two restricted D1 fixture records exercised hosted claim delivery and persisted across versions. They bypassed normal visit issuance for test setup only. Local automated tests cover signed challenge replay, session revocation and atomic assignment concurrency. |
| D1 outage, provider exhaustion and mobile download UX | **NOT RUN.** No hosted outage or load was deliberately induced. Preview and download validation used HTTP and byte comparison. |

TypeScript checking and all 21 local tests passed after the controlled-claim scope change. The final Worker upload was approximately 33.22 KiB, 9.42 KiB gzip. Wrangler measured 1 ms startup on the final deployment. D1 reported 180,224 bytes allocated. The hosted checks were bounded functional probes. They provide no sustained-load or account-wide quota measurement.

## Disable, rollback and next steps

- Keep production issuance paused in the Wrangler `controlled` environment and D1. To immediately remove public access, disable the Worker’s `workers.dev` route in **Workers & Pages → awakening-souvenir → Settings → Domains & Routes**. Keep the D1 database and audit records. If the route is re-enabled, use the final verified version. The earlier version carried the test claim allowlist and is unsuitable as the steady-state rollback target. Worker rollback does not restore D1 data. [Cloudflare rollback behavior](https://developers.cloudflare.com/workers/versions-and-deployments/rollbacks/)
- Before any visitor acceptance, enroll the Windows kiosk with a non-exportable ECDSA P-256 signing key, preferably TPM-backed, and register only its public key. Verify signing, session revocation and the actual journey milestones with that kiosk. Complete mobile scanning and download acceptance and obtain approval for the real artwork and any issuance change. See the [B3 kiosk protocol](Stage5_3B3_ProductionPreparation.md#kiosk-enrollment-and-key-custody).
- This phase made no Unity, Render, GPT, Whisper or Qwen changes and pushed no Git commits.
