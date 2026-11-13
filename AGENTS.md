# Awakening Celebrations Ver.ka — project guidance

This repository's folder is currently named `Awakening Celebrations ver.RM`; do not rename it to match the product name. Read the relevant project skill under `.agents/skills/` for specialized procedures. Skill installation or planning is not permission to audit, redesign, or change the application.

- Preserve established working functionality. Inspect current source and serialized assets; historical `Documentation/` reports can be stale. Distinguish static evidence from runtime verification.
- Respect session-only camera consent and privacy. Never bypass `GestureCameraConsentGate`, auto-grant consent, or create duplicate capture pipelines. Preserve the existing MediaPipe gesture interactions and single camera owner; preview visibility is separate from capture consent.
- Maintain English and Traditional Chinese support, catalog locale pairs, input alternatives, and readable UI.
- Preserve all six performer experiences: GAIA, PANDEMIC, RE-BIRTH, WHITE-GUARDIAN, CONSCIENCE, SANCTUARY. Protect Living Portrait preparation, playback, completion, cancellation, and configured missing-video behavior.
- Keep Settings and AI accessibility requirements in global navigation: reachability, focus, overlay dismissal, locale switching, and unavailable/error states.
- Include approved deferred features in redesign UI/UX architecture, but postpone their functional implementation until after the primary overhaul and code cleanup and a separately authorized implementation task. Do not infer approval from a Figma frame or placeholder.
- Avoid unnecessary dependencies and speculative architecture. Obtain user approval before destructive cleanup or major architectural changes; prepare a concrete proposal and impact analysis first.
- Do not enter Unity Play Mode unless explicitly authorized. Do not use a test runner, batch command, MCP call, standalone Player, or profiling session to bypass a runtime-testing prohibition. Edit Mode tests also need inspection for runtime/camera side effects.

Project skills: `unity-ui-toolkit`, `premium-glass-rendering`, `motion-interaction`, `figma-to-unity`, `unity-architecture`, `unity-performance`, `accessibility-localization`, `regression-testing`, `feature-planning`. Use their distinct activation descriptions; read only those relevant to the task. Installed general Unity skills complement these project-specific instructions.
