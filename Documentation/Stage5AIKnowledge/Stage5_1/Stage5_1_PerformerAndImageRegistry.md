# Stage 5.1 — Performer and image registry

The canonical IDs below identify exhibition contexts based on JFC defiles. They were authored for Awakening Celebrations. The Unity route IDs and binding indices remain authoritative for navigation.

| Context ID | Unity performer ID | Slot | Chapter | Selectable image |
| --- | --- | ---: | --- | --- |
| `JFC26-GAIA` | `GAIA` | 0 | `PER-001` | `JFC26-GAIA-IMG01` |
| `JFC26-PANDEMIC` | `PANDEMIC` | 1 | `PER-002` | `JFC26-PANDEMIC-IMG01` |
| `JFC26-REBIRTH` | `RE-BIRTH` | 2 | `PER-003` | `JFC26-REBIRTH-IMG01` |
| `JFC26-WGUARDIAN` | `WHITE-GUARDIAN` | 3 | `PER-004` | `JFC26-WGUARDIAN-IMG01` |
| `JFC26-CONSCIENCE` | `CONSCIENCE` | 4 | `PER-005` | `JFC26-CONSCIENCE-IMG01` |
| `JFC26-SANCTUARY` | `SANCTUARY` | 5 | `PER-006` | `JFC26-SANCTUARY-IMG01` |
| `JFC26-NOISE` | — | — | — | — |
| `JFC26-ROOTS` | — | — | — | — |

The assistant routes follow `Assistant-<Unity ID>`. Focus and reflection follow `<Unity ID>---FOCUS-VIEW---<chapter>` and `<Unity ID>---POST-VIDEO-AI---<chapter>`. The gallery binds `presence-<slot>`; the chooser binds `assistant-<slot>`. Both use the same default portrait texture. No image ID is sent by the current app. Noise and Roots are valid sources of festival knowledge, with no exhibition route or image mapping.

## Current artwork

The scene references `LivingGlassAssets.asset`. The registry follows the serialized Texture2D arrays and existing metadata GUIDs. The files are PNGs, and all 18 were visually inspected locally alongside their role and slot. `IMG01` is the selectable default portrait, `IMG02` is the Activation artwork and `IMG03` is the Living Portrait stage still. The latter two are existing presentation variants. A still ID does not identify the video or imply a new selection control.

Each entry has an asset path, GUID, dimensions, bilingual labels, and a visual observation. Observations are limited to what can be seen, with careful terms like “fur-like,” and are unapproved draft descriptions. Exact costume names, pictured people, costume designers, creators, photograph provenance and redistribution rights are unresolved for all 18 entries. A matching visual theme is insufficient evidence for authorship. For example, the HEAL Gaia defile and Artwear both reference Raja Ampat, but the credit for a particular Artwear garment does not establish who made anything pictured in the Gaia exhibition art. [2026 defile coverage](https://www.detik.com/jatim/budaya/d-8591892/grand-carnival-jfc-2026-tampilkan-8-defile-spektakuler) · [RRI Artwear credit](https://rri.co.id/jember/regional/2603132/artwear-jfc-2026-jadi-panggung-kreativitas-desainer-lokal)

For later assets, allocate the next unused `IMGnn` within the parent, keep existing IDs stable, and attach a source/rights record before enabling specific credits. Use `approvedDescription` for reviewed bilingual interpretation and source refs. A file rename can update the recorded asset path while preserving its ID and GUID. The same path may have stage variants with separate IDs when the actual Unity GUIDs differ. Future souvenirs need a separate namespace such as `SOUV-...` and independent rights and delivery rules. No current visual should be assumed eligible for giveaway solely because it appears on screen.

[Performer bindings](../../../awakening-ai-backend/knowledge/data/performers.json) · [Image bindings](../../../awakening-ai-backend/knowledge/data/images.json)
