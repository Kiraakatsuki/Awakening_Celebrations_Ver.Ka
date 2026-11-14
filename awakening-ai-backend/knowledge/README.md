# Awakening Celebrations knowledge data

Stage 5.1 stores canonical research and exhibit mappings as UTF-8 JSON. The existing Express process does not import these files. Validate from the Unity project root with Node.js 20 or newer.

```sh
node awakening-ai-backend/scripts/validate-knowledge.mjs
```

| Collection | What it contains |
| --- | --- |
| [sources](data/sources.json) | Stable source IDs, published date or precision, access date/method and concise evidence notes |
| [overview](data/overview.json), [timeline](data/timeline.json) | Bilingual festival and exhibition overview and dated milestones |
| [editions](data/editions.json) | Numbered editions and the separate virtual program, umbrella names, variants, coverage and defile examples |
| [heal2026](data/heal2026.json), [defiles](data/defiles.json) | Official narrative, announced and observed programming, and independently attributable defile sections |
| [performers](data/performers.json), [images](data/images.json) | Canonical exhibit IDs with explicit Unity references, actual file roles and unresolved identity or rights fields |
| [contributors](data/contributors.json) | Reported roles scoped to the organizer or a dated event |
| [evaluation](data/evaluation.json) | Matched English/Traditional Chinese prompts, references, answer expectations and uncertainty behavior |
| [proposed context schema](schema/context.schema.json) | Request-context shape for Stage 5.2, with registry relationships exercised locally by the validator |

Records use unique IDs and nonempty source references. Descriptive visitor-facing fields use `en` and `zh-TW`. Source records keep the original language and a short independently written evidence note. A null date means no reliable publication date was captured, and null credit fields mean attribution is unresolved. `defileCoverage` distinguishes a published list from selected examples. Curatorial and visual claims have separate provenance from official and observed festival claims.

When editing, preserve stable IDs and put new facts next to their source references. Record contradictions with their scope and date. Link back to the original source without embedding articles or media. Update both locales in one change. For a newly approved artwork description, add bilingual `approvedDescription.text`, reference the documented approval, mark the field `approved-exhibition`, and continue to track rights independently. The validator tests today’s 18 serialized bindings as a baseline; a later standalone image catalog needs its own verified source and optional Unity binding fields.

The proposal is one validated read-only process index over these files when integration is authorized. Current IDs do not control media access. See the [foundation report](../../Documentation/Stage5AIKnowledge/Stage5_1/Stage5_1_KnowledgeFoundationReport.md) for decisions, gaps and next-stage scope.
