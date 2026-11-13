---
name: accessibility-localization
description: "Review or implement Awakening Celebrations accessibility and English/Traditional Chinese localization, including glass readability, input alternatives, focus, typography, and bilingual layout. Use for inclusive interaction and locale quality."
---

# Accessibility and localization

Follow repository AGENTS.md. Inspect HomeLanguageController.cs, RuntimeScreenCatalog.cs, locale-aware bindings in AwakeningApplicationController.cs (Assets/Scripts/), current UXML/USS and fonts. The catalog uses en/zh and AppLanguage.Chinese; preserve identifiers while ensuring visitor-facing Chinese is Traditional Chinese. Do not assume a localization package or introduce one without need.

Verify English/Traditional Chinese parity for navigation, consent/privacy, gesture instructions, Settings, assistant loading/errors/retry, video placeholders and all six performer stages. Check actual bound controls rather than counting translated files. Preserve state when changing language and avoid text-based element selectors.

Assess text contrast over bright, dark and moving backgrounds; use a solid/tinted fallback where translucency cannot remain readable. Check focus/selected/disabled states without relying on color alone. Use comfortable exhibition viewing sizes, CJK glyph coverage, font fallback, line height, punctuation, wrapping and overflow; include long English strings and multi-line Traditional Chinese assistant replies.

Inspect pointer target sizes, gesture dwell/cancel behavior, reachability, clear consent status and non-gesture alternatives. Verify existing Tab, Enter/Space, Escape/Backspace and numeric selection where supported. Keep focus visible, move it sensibly into overlays, restore it on dismissal and avoid duplicate stops in composite controls. Document unsupported keyboard or assistive-technology behavior instead of assuming UI Toolkit provides it automatically.

Include reduced-motion and reduced-effects alternatives in authorized design work. Keep Settings and AI reachable through global navigation, with understandable unavailable states. A designed or disabled accessibility setting is not an implemented capability: route scope decisions through feature-planning and do not enable deferred behavior.

Use static locale/font/binding checks within scope. Mark visual, screen-reader and runtime input checks pending unless actually performed with authorization.
