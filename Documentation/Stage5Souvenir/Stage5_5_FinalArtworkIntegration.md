# Stage 5.5 — Final digital souvenir artwork integration

## Deployment

The existing `awakening-souvenir` Worker serves version `89b443fc-79a6-408a-8016-6559d3607dbe` at 100% under deployment `6cb3c615-12d9-4d6c-9f62-5eef9428ce4e`. The account's Free plan was confirmed by the operator. The release reused Worker-first Static Assets and `awakening-souvenir-d1`. Version preview URLs are disabled. No new service, resource, billing feature, or GitHub change was created. [Worker-first configuration](https://developers.cloudflare.com/workers/static-assets/binding/) · [Static Assets limits](https://developers.cloudflare.com/workers/static-assets/billing-and-limitations/)

The catalog has exactly six production-eligible entries. All six rows were staged inactive, checked against the private manifest, then activated in one guarded D1 statement. Public issuance remains paused in both the Worker and D1. Local test authentication is disabled in production and there are zero live test grants. The four historical assignments, four QR records, and three QR revocations remain in D1. The placeholder bytes and test-only catalog row remain available for historical references and local tests. Normal production assignment and the public catalog exclude the placeholder.

## Artwork validation

Each supplied original is an RGB PNG. The Worker accepts WebP, so the six source files were converted to full-size lossless WebP. Every decoded pixel matched its original, including the branding, and original and derivative dimensions match. All six are landscape and within 0.12% of 16:9. No resizing or cropping was applied. Source byte hashes and decoded pixel hashes were unique. The lossless files total 10.54 MiB and are staged under the existing Git-ignored `souvenir-service/assets/media/` rule. Wrangler uploaded exactly six new assets at immutable, versioned paths. The exact protected paths and deployment manifest are retained in the operator's private workspace. Originals remain at the operator's supplied local location.

| Catalog ID | Display name | Source PNG | Dimensions | PNG → WebP bytes |
| --- | --- | --- | --- | --- |
| `jfc26-gaia` | Gaia / 蓋婭 | `Gaia_souvenir.png` | 1672×941 | 2,816,781 → 2,118,622 |
| `jfc26-pandemic` | Pandemic / 大流行 | `Pandemic_souvenir.png` | 1672×941 | 2,197,255 → 1,472,142 |
| `jfc26-re-birth` | Re-Birth / 重生 | `Re-Birth_souvenir.png` | 1671×941 | 3,028,802 → 2,019,192 |
| `jfc26-sanctuary` | Sanctuary / 庇護所 | `Sanctuary_souvenir.png` | 1672×941 | 2,433,055 → 1,746,000 |
| `jfc26-white-guardian` | White Guardian / 白色守護者 | `White_Guardian_souvenir.png` | 1671×941 | 2,942,284 → 1,871,892 |
| `jfc26-conscience` | Conscience / 良知 | `Conscience_souvenir.png` | 1672×941 | 2,629,787 → 1,822,648 |

| Catalog ID | PNG SHA-256 | WebP SHA-256 |
| --- | --- | --- |
| `jfc26-gaia` | `e866002a351d9015e2f0a2e4842980d4b9dfb4851733e59926fc9abaa85a1317` | `af4ea3502722a607f212c36614c41e27c536076072ef900dccda3ae54709a472` |
| `jfc26-pandemic` | `43a02584c35a800105cc15ea2c550c0a1b437e782ed4afd19eead42c6df84f78` | `fdc206f76c6b825b3e6bddbee42ab1c1f1c926eb29d33cd1a12fe699fbe56333` |
| `jfc26-re-birth` | `2312c8d54dbb74c98fce1b4535e42bcb1133a4d516452bc01e220f9dd81de77a` | `b0e6d08077859d2a832b8eb7186a77d09b9183a36e7db7c67ea79fc88d5e7c83` |
| `jfc26-sanctuary` | `de0700d80cd230f6f0acf4be58334fac43c145653012addd9ce611df5130d9a6` | `f30afe548d15b822c8da857d362898e6142e4e332f6008a361c14527143da363` |
| `jfc26-white-guardian` | `05eef06d9c2f0da7e2a1f43d0e87dd051646f5e71eed98418eaf89a4057e24bc` | `49580409dc7e8b8098fbd432d65732c61636ebe3f883fb0987b8da775a73bdfa` |
| `jfc26-conscience` | `8148d8f73e84c6b1c7e67a758b870bce9898e3a06fa8c6c59563b192cbe0c398` | `b57990a63f902cd9953014c3c3b466f50f5a8e7a97a71a9afedf2d1d08f8aa80` |

## Verification

- All 22 existing souvenir Worker tests passed, including uniform sampling, concurrent idempotency, expiry, revocation and placeholder rules. Worker code, D1 schema, QR TTL, and the 100-per-day cap were unchanged. Normal assignment still samples the eligible catalog independently of the completed performer.
- A focused local Worker test used the actual Static Assets binding. Both authorized response types returned the expected SHA-256 for all six artworks with WebP MIME, the appropriate inline or attachment disposition, `no-store` and no referrer. Missing authorization failed, the expired fixture returned HTTP 410 and a revoked fixture's prior image grant returned HTTP 410. The test used only synthetic local claim credentials.
- The hosted Worker returned HTTP 404 JSON on 52 probes covering direct, encoded, alternate, navigation, range and fallback routes plus unauthenticated media endpoints. All six direct paths returned HTTP 404 again after catalog activation. Hosted health returned HTTP 200. The public catalog returned the six bilingual IDs and image-availability flags without disclosing storage paths. No new hosted assignment was generated. Validation of authorized delivery to a physical phone and hosted expiry or revocation on these final artworks remains pending.

## Rollback

Keep both issuance pauses in place. From the `souvenir-service` directory, verify the operator is on the existing `controlled` environment. Apply the guarded statement in the ignored `.wrangler/stage5.5/deactivate.sql` file to deactivate the six release rows without deleting them, and verify the public catalog and intended pause. Preserve catalog history, claims, revocations and any asset version referenced by an outstanding valid claim. The previous verified Worker version is `85294e63-f47e-4dcc-9c0a-b10aa019734b`. If a Worker rollback is necessary and the prior version can serve every outstanding claim, use `pnpm exec wrangler rollback 85294e63-f47e-4dcc-9c0a-b10aa019734b --env controlled`. Recheck hosted health, direct-path denial, D1 pause and production test-auth after rollback. Worker rollback does not roll back D1. [Cloudflare rollback limits](https://developers.cloudflare.com/workers/versions-and-deployments/rollbacks/)
