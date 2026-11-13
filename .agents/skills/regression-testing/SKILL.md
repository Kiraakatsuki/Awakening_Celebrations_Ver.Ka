---
name: regression-testing
description: "Plan or execute authorized Awakening Celebrations regression checks, Unity Edit Mode/Play Mode tests, navigation matrices, and lifecycle validation. Use to prevent behavioral regressions while respecting runtime and camera restrictions."
---

# Regression testing

Follow repository AGENTS.md. First establish permitted checks. Never enter Play Mode without explicit authorization, bypass a runtime prohibition through batch/MCP/Player execution, auto-grant camera consent, or start duplicate capture pipelines. Inspect Edit Mode tests and validators for runtime, camera, network and asset-write side effects before running them; test naming alone is not proof of safety.

Inspect Packages/manifest.json (observed Unity Test Framework 1.6.0), existing test assemblies and Editor validators before adding tests. Documentation/ValidationReport.md, ScreenAndNavigationInventory.md and UIDocumentArchitecture.md are leads, not evidence that the current application passes. Use installed Unity validation skills for general tooling when available.

## Choose meaningful coverage

Start with static route/locale/catalog coverage, UXML/USS references, root/path/tag binding integrity, serialized document assignments, pipeline/resource references and handler cleanup. For a behavioral change, add focused Edit Mode tests for isolated state or mapping contracts where useful; avoid tests that merely repeat implementation text.

Plan navigation coverage across Home, Privacy, tutorial, Attract, Color Release, selection, six performers times five stages in both locales, Settings, assistant chooser/overlay, Closing and Souvenir. Include decline/revocation, repeated language switching, Back, overlay dismissal and focus restoration.

When runtime checks are authorized, test video preparation, actual playback completion, cancellation, missing clips, repeated entry/exit and stale callbacks; assistant late responses and errors; interrupted transitions; consent-denied/granted/revoked camera lifecycle and single-owner enforcement. Camera tests require an approved consent scenario or isolated mocks that cannot activate hardware. Mocks must not weaken production consent gates.

Record baseline versus introduced failures, exact checks and results. Distinguish passed, failed, blocked and not run. Do not report static validation as runtime success. If runtime is prohibited, complete safe static checks and deliver the pending runtime matrix without executing it.
