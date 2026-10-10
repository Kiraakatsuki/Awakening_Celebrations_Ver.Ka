# Validation Report

Date: 2026-10-10

## Checks completed

- Used PowerShell inline Python with the standard library (`json`, `xml.etree.ElementTree`, `pathlib`, `re`, `urllib.parse`) for read-only manifest, snapshot, UXML-tree, asset-path, catalog GUID and interaction-ID comparisons. No generated check files were written.
- Used `rg` to inspect callbacks, state classes, imported pseudo-state rules, control labels, audio/camera/analytics/QR service symbols, and event subscription/unsubscription pairs.
- Read the project version, package manifest, build scene settings, scene YAML, Panel Settings and all existing C# scripts.
- Baseline scene inspection found both Home UIDocument GameObjects inactive and the generic active UIDocument referencing Privacy EN; the implementation now saves both Home objects active and attaches the navigation controller.
- Parsed the generated UXML files as XML; checked screen/USS/image file counts and inspected the Home background path and importer metadata.
- Inspected connector manifest root IDs, descendant paths, tags/classes and duplicate asset names.
- Inspected Figma page inventory, English/Chinese canonical Home and Privacy, tutorial source trees, and the interactive prototype route graph via read-only tools.
- Confirmed the Figma design-context call itself could not provide context because Figma reported nothing selected; screenshots/metadata/tree and prototype data were available by other read-only calls.
- Added eight English tutorial UXML variants; checked every replacement label against the Figma source text/path export.
- Checked all 2,808 connector node paths across the 106 source rows against the corresponding on-disk/generated UXML. Every path resolved to the manifest's declared tag.
- Checked that the runtime catalog contains 106 unique logical-screen/language entries and that every referenced UXML GUID resolves to a local `.meta`.
- Added `Tools > Awakening Celebrations > Validate Figma Runtime Mapping` and a build preprocessing check. Static simulation of the manifest/catalog language and UXML-path expectations found 106 unique catalog entries covering all manifest roots with no mismatches.
- Rechecked all 2,808 manifest node paths against UXML from the imported visual root (excluding the outer UXML document wrapper): zero path/tag mismatches.
- Revalidated 388 direct and dynamically derived runtime targets across Home roles, active/disabled Settings controls, 12 overlay control sets, six performer-card groups in both chooser views, 60 performer-stage cues, and Closing/Souvenir button surface-label pairs: zero source ID/path/tag mismatches.
- Parsed all 106 current UXML files, checked their 99 stylesheet references, and verified all 126 raster image references resolve on disk. The current project contains 47 raster files total, including the developer's manually added artwork.
- Confirmed the 2,808 imported UXML nodes consist only of `VisualElement` and `Label`; no native Button/Toggle/TextField/Slider/DropdownField exists. Confirmed there are no imported USS pseudo-state or `ac-*` rules to conflict with the app-owned interaction classes.
- Inspected Settings and assistant overlay controls. Sound, Accessibility, Tracking, Restart, Settings Guide, assistant prompt chips, Ask placeholder/button are now visibly unavailable and inert where no verified action/service exists. Language, Privacy, Ask assistant navigation, Back, and overlay Close remain active.
- Confirmed shared interaction styles do not override imported colors: UXML screen styles stay in place; no imported USS pseudo-state or `ac-*` selectors conflict with app-owned classes. Home gold focus is now Home-scoped; imported screens use neutral focus feedback.
- Lexical delimiter scans passed for the runtime and Editor C# files. These scans are not compilation.
- Checked that the scene references the new controller/catalog and that the Home objects are active in the saved scene.
- No Figma mutation, actual re-import, package installation, or Unity Library modification was performed.

## Not run / limitations

- **Unity C# compilation: not run.** Unity Editor 6000.4.7f1 was not found at the standard install paths and no Editor process was available. Static checks are not a substitute for Unity compilation.
- **Play Mode, Windows Player, runtime navigation, keyboard, lifecycle, consent, overlay, or visual screenshot tests: not run.** Do not treat code inspection as runtime verification.
- **Current unsaved UI Builder state: not verifiable.** No Unity Editor UI was available.
- **Full per-control Figma-to-UXML action map: partially verified.** Runtime-bound controls and unavailable controls listed in `Documentation/InteractionAudit.md` were checked against the current snapshot; the connector manifest omits source names/labels, and unlisted visual affordances still need a manual Figma-to-Unity review.
- **Focus-screen Assistant entry: not implemented.** The prototype has Assistant Entry actions, but imported Focus UXML has no corresponding named or text node, so a visible source-confirmed trigger could not be identified from its import mapping.
- **Assistant Ask responses and QR delivery: blocked by missing integration.** No assistant backend/response contract or QR destination/provider is configured; overlay prompts are inert and visibly unavailable.
- **Settings Sound, Accessibility, Tracking, and Restart: blocked by missing specification or integration.** They are explicitly disabled with unavailable status; no preference or reset side effects are claimed.
- **Artwork correspondence: requires developer runtime verification.** The 47 raster files are present and every UXML image reference resolves, but actual crop, scaling, and per-frame correspondence have not been compared in the player.
- **Alignment discrepancies: deferred visual adjustment.** The static pass did not identify coordinate-specific defects; compare the imported screens manually against Figma before spending time on cosmetic adjustment.

## Feature status after implementation (Unity runtime verification pending)

| Feature | Status |
|---|---|
| Home hover/pressed/consent and language interaction code | Implemented; statically inspected; runtime verification required |
| Privacy flow | Implemented with session consent gate; runtime not verified |
| Tutorial navigation | Implemented; English variants recovered; runtime not verified |
| Attract/Color Release/Performer Select | Implemented; pointer/keyboard fallback; runtime not verified |
| Six performer stage screens | Implemented routes for all five stages × six performers × two languages; distinct-art fidelity not fully verified |
| Assistant overlays and Settings | Settings/chooser/overlay close routes implemented; unverified Settings controls and assistant prompts are disabled; Focus entry and service responses remain blocked |
| Closing and Souvenir | Implemented route; QR delivery/provider remains unimplemented |
| Session awakening lifecycle | Implemented in memory; set after Color Release, reset on process restart; runtime not verified |
| English/Chinese localization | Catalog and localized variants cover all 53 logical IDs; runtime fit/layout not verified |
| Unity compile/Play Mode/visual tests | Requires developer runtime verification; not run by this task |
