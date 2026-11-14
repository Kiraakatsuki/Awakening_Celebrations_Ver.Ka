# Stage 5.1 — JFC knowledge foundation

The knowledge collection is ready for review and local validation. All additions are data, a local validator, and documentation. The existing Unity scene, voice systems, Express server, request/response wiring, dependency manifest, and production deployment configuration were left untouched. No live requests or deployment were performed.

| Coverage | Result |
| --- | --- |
| Numbered historical editions | 24 evidence-backed edition records from 2003 through 2026. Two editions occurred in 2003. |
| Annual umbrella themes | 21 sourced titles. The first three editions have listed costume groups and no established separate umbrella title in the reviewed evidence. |
| Related program | The 2020 virtual World Kids Carnival is documented outside numbered editions. |
| History and overview | 16 chronological milestones and three overview records. |
| HEAL 2026 | Eight official defile profiles, event dates, three program days, source-attributed presentation details, narrative relationships and unresolved credits. |
| Sources | 39 records, comprising 38 public references and one Unity repository reference. One public reference is an indexed excerpt whose direct retry timed out. |
| Language | Narrative, historical, defile, contributor, exhibition and image descriptive content in English and Traditional Chinese. Source titles retain their original languages. |
| Performer identity | Eight application-authored IDs. Six are selectable and map to existing Unity IDs. Noise and Roots have festival profiles only. |
| Costume-image registry | 18 verified PNG/GUID bindings. Six are selectable portraits and twelve are stage-art variants. No pictured-person, costume authorship or rights attribution has been confirmed for these assets. |
| Contributors | 15 scoped records with source evidence, including the founder, festival leadership and event-specific designers and performers. |
| Evaluation | 21 question pairs in English and Traditional Chinese, with expected behaviors and record citations. Includes unknown image, conflicting context, absent credits and follow-up cases. |

## Research decisions and open questions

- Use **HEAL — Humanity, Earth, and Life** and the eight names from the December 2025 release. The organizer’s concept cards support the individual themes. A separate scheduled order and observed opening distinguish the story from the event sequence. [Official HEAL release](https://www.jemberfashioncarnaval.com/heal/press) · [Organizer profiles](https://www.jemberfashioncarnaval.com/heal) · [RRI finale](https://rri.co.id/jember/regional/2603138/grand-carnival-jfc-2026-ditutup-meriah-tuai-apresiasi-tamu-nasional)
- Earlier editions have uneven primary documentation. Multiple sources support the sequence and many titles. Early organizer archives, complete defile lists, legal organization history, exact dates for some years and some umbrella/subtitle spellings remain research gaps. [Historical evidence and variants](Stage5_1_JFCHistory.md)
- HEAL defile leads and costume authorship remain unresolved where no individual documentation was found. Artwear and Grand Carnival are separate credit contexts, even when public descriptions share a motif. LED staging is documented as an announcement. Publicity rankings, unverified totals and speculative materials were excluded from factual answers. [HEAL and event-specific credits](Stage5_1_HEAL2026.md)
- The corpus stores original factual summaries, attribution and source locators. It contains no copies of articles or private media. Visual observations are draft language and need curator review before becoming approved captions. [Source registry](Stage5_1_SourceRegistry.md) · [Image registry](Stage5_1_PerformerAndImageRegistry.md)

## Local verification

Run from the Unity project root with Node.js 20 or newer.

```sh
node awakening-ai-backend/scripts/validate-knowledge.mjs
node --check awakening-ai-backend/server.js
```

Both commands passed on Node 24.22.1. Validation checks canonical IDs, dates and editions, provenance and source refs, official chapters and exhibition scope, locale fields, the scene’s serialized artwork references, asset GUIDs and PNG dimensions, context relationships, and evaluation expectations. It enforces internal evidence and data contracts. Authorship, translation quality and the accuracy of a referenced publisher remain editorial checks. No service or model was started for validation.

The implementation baseline was `cf2f5013911304ce18c7ff2b40c13d62648d238e`. The nine existing screenshot deletions and local Stage 4.3E checkpoint annotation were preserved. No branch, tag or remote changed. Runtime and deployment status were not re-tested in this stage.

## Review package

- [Canonical data and schema guide](../../../awakening-ai-backend/knowledge/README.md)
- [Source registry](Stage5_1_SourceRegistry.md) and [history](Stage5_1_JFCHistory.md)
- [HEAL 2026](Stage5_1_HEAL2026.md) and [performer and image registry](Stage5_1_PerformerAndImageRegistry.md)
- [Bilingual policy](Stage5_1_BilingualKnowledgePolicy.md) and [evaluation data](../../../awakening-ai-backend/knowledge/data/evaluation.json)
- [Proposed Stage 5.2 integration](Stage5_1_Stage52IntegrationDesign.md)

Stage 5.2 review should settle the context contract, evidence and caption rules, and session privacy behavior before wiring retrieval into the existing Express route. It should then test retrieval coverage, citation grounding and prompt budgets offline before any separately authorized live integration.
