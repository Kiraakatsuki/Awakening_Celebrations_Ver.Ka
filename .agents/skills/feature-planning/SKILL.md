---
name: feature-planning
description: "Build evidence-based Awakening Celebrations requirements, feature inventories, implementation matrices, dependency maps, and phased plans. Use to distinguish working, partial, deferred, design-only, legacy, and unconfirmed features before scope decisions."
---

# Feature planning

Follow repository AGENTS.md. Planning does not authorize application changes. Inspect relevant current source, serialized assets and approved user decisions; use Documentation/ScreenAndNavigationInventory.md, RuntimeArchitecture.md, ImplementationPlanAndRunbook.md and Figma exports as context that may lag current implementation. Older reports describe missing gesture/assistant services while current source contains integrations; reconcile evidence instead of copying old status.

## Classify each capability explicitly

| Status | Evidence required |
| --- | --- |
| Implemented | Current functional path and wiring; state runtime verification separately. |
| Partially implemented | Concrete working portion plus specific missing behavior. |
| Deliberately deferred | Explicit postponement decision and its scope/approval source. |
| Design-only | Reference design or prototype without confirmed functional wiring. |
| Legacy candidates | Possible superseded path; references and removal approval remain unresolved. |
| Unconfirmed | Insufficient or conflicting evidence; identify the question to resolve. |

Distinguish implementation status from verification status and approval status. A visible frame, disabled control, method name or old report is insufficient to infer a working feature, deliberate deferral or permission. Where design and partial implementation coexist, classify the functional capability and link its design separately.

For each item record route/component, performer and locale coverage, source evidence, current behavior, gaps, dependencies, consent/accessibility implications, approval source, phase and acceptance criteria. Cover all six performers, Living Portrait, camera/gestures, Settings and AI global access. Track uncertainty explicitly rather than inventing decisions.

## Preserve scope across phases

During redesign planning, include approved deferred features in UI/UX architecture: entry points, navigation relationships, layout capacity, reusable component boundaries and understandable unavailable states. Postpone their functional implementation until after the primary overhaul and code cleanup and separate authorization. Do not activate controls or introduce speculative backend/service infrastructure to reserve a place.

Sequence preservation/baselines, approved primary overhaul, authorized code cleanup and separately approved deferred functionality. Make dependencies and review gates explicit; destructive cleanup and major architectural changes require approval. Deliver a requirements/implementation matrix and unresolved decisions, without starting an audit or redesign unless requested.
