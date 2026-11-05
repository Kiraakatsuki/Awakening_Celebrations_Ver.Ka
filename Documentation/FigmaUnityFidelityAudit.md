# Figma–Unity Visual Fidelity Audit

**Audit date:** 2026-10-27 (Asia/Taipei)  
**Project:** Awakening Celebrations ver.RM — Unity 6000.4.7f1, Windows Editor  
**Canonical source:** [Awakening Celebrations — Visual Redesign](https://www.figma.com/design/iEezkYHEiPrP7ThAErwdh2/Awakening-Celebrations-%E2%80%94-Visual-Redesign?node-id=51-2), file key `iEezkYHEiPrP7ThAErwdh2`.

## Audit outcome

This pass enumerated all 12 Figma pages from the live connected document and all 655 structural frames, groups, sections, components, component sets, and instances. The metadata contains 9,100 nodes total (including the 12 page roots). The Unity import manifest contains 106 runtime screen roots and 2,808 element mappings.

The structural comparison found **0 missing node paths, 0 element-type mismatches, and 0 remaining numeric width/height/left/top discrepancies** across those 2,808 mapped nodes. Local image-path checks reported 126 placements across 47 unique images and no missing files. These checks do not establish visual fidelity by themselves.

The Phase 1 broad capture pass saved 1920×1080 Unity screenshots for **104 of 106** catalog roots and paired Figma references for all 106; its 13 contact sheets covered the 104 accessible pairs. Phase 3 added isolated 1920×1080 UI renders for both Attract fallback roots, so all 106 roots now have a visual capture. Those two renders did not call controller navigation and do not verify input behavior; the consent gate remained intact. Root inventories and contact sheets record the per-root results. **No page is marked complete:** the screenshot pass is still a coarse root comparison, not an element-by-element visual and flow audit of all 655 structural records.

## Phase 3 update — post-fix recaptures and safe fallback verification (2026-10-27)

This update continues the existing 106-root / 655-frame inventory. The inventories now use only the controlled status values. Reconciled current counts are **roots: 106 (9 PASS, 95 PARTIAL, 2 FAIL, 0 NOT VERIFIED)** and **frame records: 655 (9 PASS, 644 PARTIAL, 2 FAIL, 0 NOT VERIFIED)**. The two newly passed roots are the localized Attract fallback UXMLs; the two Privacy failures remain. No page is complete.

### Tutorial post-fix captures

Captured all 16 corrected localized tutorial UXMLs in Play Mode at 1920×1080 Game View: eight steps in English and Traditional Chinese. Captures are in `Assets/Screenshots/FigmaUnityRuntime/phase3/`; the root inventory records each exact before/after path and the frame inventory records the source/Unity evidence for each tutorial root. The labeled comparison sheets are [English tutorial comparisons](FigmaUnityContactSheets/phase3-tutorial-en-before-after.png) and [Traditional Chinese tutorial comparisons](FigmaUnityContactSheets/phase3-tutorial-zh-before-after.png). They label each exact Figma node and Unity step/locale.

The live Figma frames are 1920×1080. The previously saved Figma PNG references used in the two contact sheets are downsampled to 768×432; they preserve the 16:9 aspect ratio and were not stretched. I also directly inspected fresh 1920×1080 Figma screenshots for the Entry roots `24:281` and `105:307` against their 1920×1080 Unity renders. The other 14 pairs are useful composition-level evidence, but fine typography and edge-level comparison at matched 1920×1080 source resolution remains outstanding. For that reason all 16 tutorial roots remain `PARTIAL`, and no corrected tutorial is represented as `FIXED—VERIFIED` yet.

The Entry English and Traditional Chinese title labels, plus the Traditional Chinese body label, were moved up 12 px in their existing UXMLs after direct comparison showed the rendered text below Figma. The resulting Entry captures are `24_281__en__postoffset.png` and `105_307__zh__postoffset.png`; both now align more closely vertically. Remaining visible differences include a heavier English display title, a one-word English paragraph wrap difference, and a slightly dimmer lower motion-annotation fill. The main glass panel, CTA position and overall geometry are close. The new captures do not justify a broad alpha change; the other 14 localized screens still need detailed comparison before changing shared patterns.

### Privacy discrepancy

Privacy remains `FAIL` for English `51:39` and Traditional Chinese `53:37`. The previously inspected Figma roots have a transparent frame fill and layer their background image and veil separately; Unity’s corresponding screen has an opaque backing. The saved captures do not establish the intended parent composition or whether that backing is required behind the screen, and an uncomposited transparent Figma root is not an equivalent pixel target. No backing, opacity, or consent behavior was changed. Resolution requires a same-background Figma parent/composition capture (both locales) or explicit approved composition evidence, then a matched Unity capture.

### Attract fallback screens — safe visual render

Both fallback UXMLs were rendered through the active Unity `UIDocument` in Play Mode without calling the application navigation route or changing the consent gate. Before rendering, the gate reported `consent=False`; one camera runner existed and remained under that false gate. No camera initialization was granted or invoked. The active UI document was temporarily assigned the actual fallback `VisualTreeAsset`, captured, and Play Mode was stopped so the temporary runtime assignment was discarded. Evidence: Figma `124:3734` at `Documentation/FigmaScreenshots/124_3734.png` compared with Unity `Assets/Screenshots/FigmaUnityRuntime/phase3/124_3734__en__isolated-ui.png`; Figma `124:3767` at `Documentation/FigmaScreenshots/124_3767.png` compared with Unity `Assets/Screenshots/FigmaUnityRuntime/phase3/124_3767__zh__isolated-ui.png`. At a common 16:9 scale, the performer imagery, image sizing/crops, fallback card, negative-space zone, ordering and text composition correspond; see [Attract safe-render comparisons](FigmaUnityContactSheets/phase3-attract-safe-comparison.png). Both root rows are `PASS` for visual rendering. This was a visual fixture only and does not verify the controller’s guarded route or fallback input behavior.

### Navigation and runtime status

No button-driven navigation transition was tested in Play Mode in this update. Tutorial routes were selected through the controller to capture each state; Attract used a temporary `UIDocument` visual-tree assignment solely for a safe render. These actions do not verify the visible buttons or navigation wiring. Camera consent remained false and the production guard was not modified. Existing source-inspection findings remain source-only. Back, Done, consent, localized Home return, performer-state transitions, assistant overlays in actual composition, Settings, Closing and Souvenir still need Play Mode interaction tests.

### Page completion and remaining inventory

| Figma page | Phase 3 outcome |
|---|---|
| 00 — Home & Consent | In progress; prior design override retained; locale and interaction verification outstanding |
| 01 — Privacy & Consent | In progress; both Privacy roots remain FAIL pending parent-composition evidence |
| 02 — Visual Foundation | In progress; token/component usage not exhaustively verified in rendered contexts |
| 03 — Living Master Canvas | In progress; PARTIAL state roots remain |
| 04 — Focus & Living Portrait | In progress; performer/state and runtime flow verification remains |
| 05 — System States | In progress; state-by-state runtime comparison remains |
| 06 — Glass & Interaction Components | In progress; shared component contexts remain PARTIAL |
| 07 — Assistant, Settings & Souvenir | In progress; assistant contexts previously resolved visually, navigation and other roots remain PARTIAL |
| 08 — Tutorial & Onboarding | In progress; 16 localized captures exist; detailed matched-resolution comparison remains |
| 09 — Interactive Prototype | In progress; connection map and runtime paths not exercised |
| 10 — Six Performer Experience Variants | In progress; performer states remain PARTIAL |
| UNITY IMPORT — Runtime Screens | In progress; both Attract fallback roots now visually PASS; other runtime roots remain PARTIAL/FAIL |

No records are MISSING or AMBIGUOUS in the reconciled inventories. The two Privacy records are FAIL. The 95 PARTIAL roots / 644 PARTIAL structural records remain visible; they represent unresolved per-frame, per-component, localization, or runtime evidence requirements, not a claim that those records passed. Two Attract entries are no longer NOT VERIFIED for visual rendering; their interaction path is still untested. The current status fields are exact controlled values; rationale and evidence are retained in this audit and the `phase3_evidence` columns.

### Phase 3 files changed

- Two Entry tutorial UXMLs received a targeted 12 px text-label offset after direct 1920×1080 comparison: `TUTORIAL---ENTRY---DIM.uxml` and `TUTORIAL---ENTRY---DIM---ENGLISH.uxml`.
- `FigmaUnityRuntimeRootInventory.csv` gained explicit Phase 3 before/after screenshot paths; the 16 tutorial captures and two Attract visual renders are recorded.
- `FigmaUnityFrameInventory.csv` gained a `phase3_evidence` field for the 16 tutorial roots and two Attract roots. Both inventories now contain only controlled status values.
- Added `phase3-tutorial-en-before-after.png`, `phase3-tutorial-zh-before-after.png`, and `phase3-attract-safe-comparison.png` under `Documentation/FigmaUnityContactSheets/`.
- Added Unity evidence captures under `Assets/Screenshots/FigmaUnityRuntime/phase3/`.
- No Figma, consent, camera, MediaPipe, gesture-recognition, or production navigation source was modified.

## Phase 2 update — targeted visual failures and overlay context (2026-10-27; historical snapshot superseded where stated by Phase 3)

This update preserves the Phase 1 inventory and capture set; it does not restart the broad screenshot pass. The current stable inventories remain 655 structural records and 106 Unity runtime roots.

### Resolved assistant overlay ambiguity

The seven prior `AMBIGUOUS` roots were compared using matched-size crops of their existing Figma and Unity captures. Evidence: [assistant overlay crop comparison](FigmaUnityContactSheets/phase2-assistant-overlay-crops.png). The six performer-themed assistant bars are transparent overlays; their Figma captures show the neutral Figma canvas behind the transparent surface, while Unity captures show the Home screen beneath it. The overlay content, theme treatment, dimensions, and arrangement correspond. The English assistant-theme chooser is a full themed screen; its six image choices and selection panel align in the paired captures. These rows are now `PASS`; the background difference is composition context, not an overlay mismatch. No overlay or scene names changed.

### Tutorial glass correction (recapture pending)

The paired Entry capture showed the main translucent tutorial surface at RGB 75/77/78 in Unity and RGB 24/25/25 in Figma at matched proportional points. Direct Figma source inspection for 24:281 and 24:326 confirms a white glass fill of 0.08 alpha and border of 0.16; the corresponding UXML values matched. The Unity project is Linear color space, where that unchanged alpha produces a much brighter sRGB screenshot. This identifies a color-space compositing mismatch rather than an importer geometry error.

Applied local corrections only to translucent tutorial UXML surfaces: main glass fill alpha `0.08 → 0.009`, border alpha `0.16 → 0.026`, cyan CTA fill/border `0.16/0.55 → 0.041/0.311`, and remaining tutorial glass-card/annotation fills and edges were remapped using their source colors and dark backdrops. The values minimize sRGB output error after Unity Linear compositing. Both English and Traditional Chinese UXML variants were updated. No shared/global style, project color-space setting, text, hierarchy, source art, or element name was changed. The earlier captures are baseline evidence only; there is no post-correction Unity capture in this phase, so tutorial roots and affected component rows remain `PARTIAL` pending visual recapture. This correction is not represented as runtime-verified.

The root inventory’s English tutorial mappings now point to their actual `---ENGLISH.uxml` variants; before this update, those eight rows incorrectly pointed to the Chinese UXML despite the catalog selecting the English UXML at runtime.

### Privacy and Attract outcomes

Privacy roots `51:39` and `53:37` remain `FAIL`. The live source for 51:39 specifies a transparent root, a background image at 0.28 opacity, and a separate `rgba(9,8,7,0.80)` veil. Unity has those image/veil layers separated, but the prior opaque root backing and the Figma screenshot canvas context make a context-matched pixel target uncertain. No further opacity or backing change was guessed. Both locales need an isolated, context-matched source/render comparison before a safe repair can be selected.

Attract roots `124:3734` (English) and `124:3767` (Traditional Chinese) remain `NOT VERIFIED`. The controller rejects the Attract route while consent is false. No route was called, no test bypass was introduced, and no camera/consent code was edited. Static source review confirms the consent guard; it does not establish rendered fidelity. The latest request asked for inspection without a new run, so no editor-only cloned runtime capture was attempted.

### Phase 2 completion ledger

| Measure | Phase 2 status |
|---|---:|
| Runtime roots | 106 inventoried: 7 PASS, 95 PARTIAL, 2 FAIL, 2 NOT VERIFIED |
| Structural inventory rows | 655 inventoried: 7 PASS, 644 PARTIAL, 2 FAIL, 2 NOT VERIFIED |
| Assistant roots resolved | 7 / 7 PASS using existing matched-size captures |
| Tutorial localized UXMLs corrected | 16 / 16 (8 steps × 2 languages); post-fix runtime comparison pending |
| Privacy roots fixed | 0 / 2; both remain FAIL pending source/render context resolution |
| Attract fallback roots captured | 0 / 2; consent guard preserved, remain NOT VERIFIED |
| Pages complete | 0 / 12; all PARTIAL/FAIL/NOT VERIFIED records remain visible |
| Navigation exercised in Play Mode this phase | 0; existing controller source was inspected only |

The available post-video assistant and chooser screenshots were reused; no fresh Unity render was captured after the tutorial changes. No page is declared complete. The exhaustive node inventory remains the checklist for all remaining PARTIAL entries; its current status is not a claim that all 655 records received per-node visual verification.

### Phase 2 files and evidence

- Changed these 16 existing tutorial UXML files for local glass/CTA alpha compensation. Geometry, order, hierarchy, and names were preserved:
  - `TUTORIAL---ENTRY---DIM.uxml`, `TUTORIAL---ENTRY---DIM---ENGLISH.uxml`
  - `TUTORIAL---INTRODUCTION.uxml`, `TUTORIAL---INTRODUCTION---ENGLISH.uxml`
  - `TUTORIAL---LOOK---MOVE.uxml`, `TUTORIAL---LOOK---MOVE---ENGLISH.uxml`
  - `TUTORIAL---POINT.uxml`, `TUTORIAL---POINT---ENGLISH.uxml`
  - `TUTORIAL---HOLD---SELECT.uxml`, `TUTORIAL---HOLD---SELECT---ENGLISH.uxml`
  - `TUTORIAL---EXPERIENCE.uxml`, `TUTORIAL---EXPERIENCE---ENGLISH.uxml`
  - `TUTORIAL---READY.uxml`, `TUTORIAL---READY---ENGLISH.uxml`
  - `TUTORIAL---RETURN---DISMISS.uxml`, `TUTORIAL---RETURN---DISMISS---ENGLISH.uxml`
- Updated `FigmaUnityRuntimeRootInventory.csv` with correct English tutorial asset paths and current statuses; resolved the seven assistant root context rows to PASS.
- Updated `FigmaUnityFrameInventory.csv` for the seven assistant roots and tutorial rows; unresolved Privacy and Attract statuses remain explicit.
- Added `FigmaUnityContactSheets/phase2-assistant-overlay-crops.png` as the seven-root matched-size overlay-context evidence.
- No Figma, camera-consent, MediaPipe, gesture-recognition, or navigation source was edited in Phase 2.

### Still outstanding by page

- **Page 00 Home:** supplied dark reference remains an explicit user override; source/image context and interactions remain PARTIAL.
- **Page 01 Privacy:** both localized roots FAIL; policy and consent action details remain PARTIAL.
- **Page 02 Visual Foundation:** source board is indexed; Unity token/context use remains PARTIAL.
- **Page 03 Living Master:** Attract roots NOT VERIFIED; other state mappings and actions remain PARTIAL.
- **Page 04 Focus & Living Portrait:** six performers' lifecycle roots are paired, but per-frame details and transitions remain PARTIAL.
- **Page 05 System States:** handoff/state board correspondence remains PARTIAL.
- **Page 06 Glass & Interaction:** component context reuse, accessible controls, cursor behavior, and tutorial glass recapture remain PARTIAL.
- **Page 07 Assistant/Settings/Souvenir:** seven prior overlay ambiguities resolved; remaining screens, text details, and actions remain PARTIAL.
- **Page 08 Tutorial:** local alpha correction applied; all post-fix screenshot comparisons and motion/transition checks remain PARTIAL.
- **Page 09 Interactive Prototype:** connector destinations and runtime transitions remain PARTIAL.
- **Page 10 Performer Variants:** all six performers are represented, but node-level checks for 120 rows and flow connections remain PARTIAL.
- **Page 12 Unity Import:** 104 previous pairs remain available; Attract fallback is NOT VERIFIED and other roots remain PARTIAL.

## Inventory files

- [Frame and component inventory — 655 structural records](FigmaUnityFrameInventory.csv). Each row includes the Figma page and node IDs, parent container, type, exact name, geometry, role, direct imported root mapping when applicable, and status.
- [Unity runtime-root mapping — all 106 imported roots](FigmaUnityRuntimeRootInventory.csv). Each row links the stable Figma root ID to the existing UXML/USS without changing names, and records mapping, geometry, screenshot, and verification status.
- Unity importer manifest: `Assets/Figma/Awakening-Celebrations---Visual-Redesign/.figma-manifest.json`.

The page/node IDs in the CSVs are the stable checklist. Structural rows not directly captured are `PARTIAL`, with visual or runtime checks still outstanding. No documentation-only, duplicate, or ambiguous frame was silently excluded. The canvas nodes are page roots rather than screens and are counted in the 9,100-node total, not the 655 structural rows.

## Page-by-page status

| Figma page | Page node | Structural inventory | Page status | Evidence / remaining work |
|---|---:|---:|---|---|
| 00 — Home & Consent | `51:2` | 2 frames | **PARTIAL** | Both languages paired; preserve the supplied darker Home reference. Consent/language/settings actions are not runtime-tested. |
| 01 — Privacy & Consent | `51:3` | 2 frames | **FAIL** | Both locales paired; Privacy remains darker than canonical Figma after background-layer repair. Actions/return remain runtime-unverified. |
| 02 — Visual Foundation | `0:1` | 1 frame | **PARTIAL** | Foundation frame indexed; shared Unity tokens/styles were not exhaustively compared against each specification. |
| 03 — Living Master Canvas | `1:241` | 14 structural nodes | **PARTIAL** | Attract fallback blocked by consent; Color Release and Performer Select are paired at root level. Complete board/state mapping and transitions remain outstanding. |
| 04 — Focus & Living Portrait | `1:242` | 5 frames | **PARTIAL** | All six performer themes’ catalogued Focus/Activation/Living Portrait/Release roots were paired across available locales; individual annotation/state and navigation checks remain outstanding. |
| 05 — System States | `1:243` | 6 frames | **PARTIAL** | State board and handoff designs indexed; application counterpart comparisons remain NOT VERIFIED. |
| 06 — Glass & Interaction Components | `1:244` | 4 frames | **PARTIAL** | Shared components indexed; contexts, accessible fallback controls and cursor rendering remain NOT VERIFIED. |
| 07 — Assistant, Settings & Souvenir | `14:56` | 35 structural nodes | **PARTIAL** | Settings, Closing, Souvenir, QR, chooser, and assistant roots paired; assistant background context ambiguous and button flows untested. |
| 08 — Tutorial & Onboarding | `23:2` | 71 structural nodes | **FAIL** | Localized tutorial roots paired; translucent tutorial surfaces are materially brighter in Unity. Motion annotations and transitions are not fully verified. |
| 09 — Interactive Prototype | `14:318` | 180 frames, 14 instances | **PARTIAL** | Prototype screens and 24 line elements are inventoried; connector semantics and end-to-end runtime navigation have not been verified. |
| 10 — Six Performer Experience Variants | `19:20` | 120 frames, 6 sections | **PARTIAL** | Six themes and catalogued lifecycle states were compared in paired sheets; 120 source frame rows remain only PARTIAL for node-level checks and flow verification. |
| UNITY IMPORT — Runtime Screens | `124:2825` | 181 frames, 14 instances | **PARTIAL** | 106 catalog roots map to Unity; 104 are screenshot-paired, two Attract fallback roots remain NOT VERIFIED because consent is not accepted. |

The 655 structural records are distributed as: 2, 2, 1, 14, 5, 6, 4, 35, 71, 194, 126, and 195 respectively. The Unity import page’s 195 structural records include its 181 frames and 14 instances; the importer/catalog maps 106 of those to current runtime assets.

## Confirmed discrepancies and repairs

| Defect | Evidence and cause | Repair | Recheck |
|---|---|---|---|
| Privacy background image and veil were combined on one imported element. | Imported child carried both the image and `rgba(9,8,7,0.80)` veil; source specifies a separate image at 0.28 opacity and a 0.80 veil. | In both existing Privacy UXMLs, preserved the two existing child slots and separated image opacity from veil. No hierarchy or names changed. | Layer structure is corrected, but both paired screenshots remain materially darker than Figma; overall Privacy visual defect remains FAIL. |
| Transparent Home/Privacy roots exposed the blue Unity camera clear color. | After the layer separation, the photographed backgrounds still appeared blue, unlike the warm neutral Figma previews and the supplied Home reference. The four roots had no opaque base behind translucent artwork. | Added the documented foundation token `--stage-deep = rgba(14,14,14,1)` as the root backing on the two Home and two Privacy UXMLs. | All four localized runtime captures now use a warm neutral base; image crop and panel placement remain aligned. |
| Small imported geometry differences. | Numeric comparison against the runtime import manifest found 168 width/height/left/top values off by at least 1px across 24 imported UXML screens. | Corrected those values from the mapped Figma/import manifest. Fixed one-pixel Home/assistant shifts and tutorial motion-label top offsets in the final exact audit. | Re-ran the complete structural path/type/geometry comparison: 2,808 mapped entries, 0 path/type mismatches, 0 geometry mismatches (0.05px tolerance). |

### Home reference/source conflict

The supplied Home reference is darker than the live Figma Home frame. The current Home UXML already carries the values previously set to follow the supplied reference: stronger veil (`0.80`), subtler warm field (`0.12`), subtler glow (`0.025`), and more opaque panel surfaces. The live Figma frame 51:4 specifies veil `0.62`, warm field `0.22`, glow `0.09`, and different surface opacity. I preserved the explicit supplied Home reference values and did not overwrite them with the conflicting live Figma values. Treat this as an intentional reference override; all other comparisons use live Figma as the source of truth.

## Screenshots

Unity runtime captures are 1920×1080 Game View screenshots. The 104 accessible roots are paired with Figma captures obtained through the connected integration. Figma root captures are stored in `Documentation/FigmaScreenshots/`; Unity root captures are in `Assets/Screenshots/FigmaUnityRuntime/`; the 13 visual comparison sheets are in `Documentation/FigmaUnityContactSheets/`.

| Screen | Before | After |
|---|---|---|
| Home — English | [Before](../Assets/Screenshots/fidelity-audit-home-en-runtime.png) | [After](../Assets/Screenshots/fidelity-audit-home-en-after-root-fix.png) |
| Home — Traditional Chinese | — | [Current](../Assets/Screenshots/fidelity-audit-home-zh-runtime.png) |
| Privacy — English | [Before](../Assets/Screenshots/fidelity-audit-privacy-en-runtime.png) | [After](../Assets/Screenshots/fidelity-audit-privacy-en-after-root-fix.png) |
| Privacy — Traditional Chinese | — | [Current](../Assets/Screenshots/fidelity-audit-privacy-zh-runtime.png) |

The [Figma source captures](FigmaScreenshots/) provide 106 catalog-root screenshots, one 51:39 context capture, and five separate source-board captures. The [Unity runtime captures](../Assets/Screenshots/FigmaUnityRuntime/) provide 100 catalog-root PNGs; the four localized Home/Privacy captures are linked above. The [13 comparison sheets](FigmaUnityContactSheets/) show 104 paired roots. This is a coverage pass, not pixel-accurate per-element verification. The two consent-gated roots have source Figma captures only.

## Navigation and application rules

- Static inspection of `Assets/Scripts/AwakeningApplicationController.cs` confirms route lookups use the existing catalog and language argument. No screen, element, or asset names were changed.
- Direct Unity route calls displayed English and Chinese Privacy, and `ShowHome(false)` displayed Home. These calls verified route rendering, **not** button-driven navigation.
- The controller rejects the Attract route unless consent is accepted and calls the consent gate with camera permission disabled for Home/Privacy. This was source inspection; the camera and consent logic were not edited.
- Tutorial progression, performer stage changes, assistant/settings overlays, Back after awakening, and Done-to-localized-Home were not exercised in Play Mode. Their visible destinations and wiring remain PARTIAL / NOT VERIFIED.
- No MediaPipe, gesture-recognition, camera-consent, or Figma source files were modified in this audit pass. Unity’s Play Mode is stopped after capture.

## Files changed in this audit pass

### Imported UXML

Geometry and/or root backing corrections were applied to these 24 existing UXML files. Names and hierarchy were preserved:

- `07-01---GAIA---FOCUS-AI-POP-UP---ENGLISH.uxml`
- `07-03---PANDEMIC---FOCUS-AI-POP-UP---ENGLISH.uxml`
- `07-05---RE-BIRTH---FOCUS-AI-POP-UP---ENGLISH.uxml`
- `07-07---WHITE-GUARDIAN---FOCUS-AI-POP-UP---ENGLISH.uxml`
- `07-09---CONSCIENCE---FOCUS-AI-POP-UP---ENGLISH.uxml`
- `07-10---PROTOTYPE---ASSISTANT---CONSCIENCE.uxml`
- `07-11---SANCTUARY---FOCUS-AI-POP-UP---ENGLISH.uxml`
- `08-01---SETTINGS---EXPERIENCE-CONTROLS---ENGLISH.uxml`
- `08-02---SETTINGS---EXPERIENCE-CONTROLS.uxml`
- `09-01---CLOSING---CELEBRATION---ENGLISH.uxml`
- `09-02---CLOSING---CELEBRATION.uxml`
- `09-03---SOUVENIR---QR-DELIVERY---ENGLISH.uxml`
- `09-04---SOUVENIR---QR-DELIVERY.uxml`
- `HOME--------SANCTUARY.uxml`
- `HOME---ENGLISH---SANCTUARY.uxml`
- `PRIVACY--------SANCTUARY.uxml`
- `PRIVACY---ENGLISH---SANCTUARY.uxml`
- `TUTORIAL---ENTRY---DIM.uxml`
- `TUTORIAL---LOOK---MOVE.uxml`
- `TUTORIAL---POINT.uxml`
- `TUTORIAL---HOLD---SELECT.uxml`
- `TUTORIAL---EXPERIENCE.uxml`
- `TUTORIAL---READY.uxml`
- `TUTORIAL---RETURN---DISMISS.uxml`

### New audit evidence

- `Documentation/FigmaUnityFidelityAudit.md`
- `Documentation/FigmaUnityFrameInventory.csv`
- `Documentation/FigmaUnityRuntimeRootInventory.csv`
- 104 paired Unity runtime screenshot PNGs, 106 Figma catalog-root screenshots, five additional source-board captures, one context capture, 13 comparison sheets, and Unity `.meta` files.

No image source asset, USS, scene, controller, gesture, or camera file was edited during this audit pass. The repository already contained other unrelated or prior-turn modifications; those were not attributed to this pass.

## Phase 1 remaining work / blockers (superseded by the Phase 2 ledger above)

- **Privacy background:** both locales remain FAIL; Unity backdrop is visibly darker than Figma even after separating the image and veil. The root backing changed from blue-clear exposure to the warm base token, but the remaining contrast difference has not been corrected.
- **Tutorial translucent surfaces:** the paired baseline showed the English and Chinese tutorial panels brighter in Unity (Figma sample 24/25/25 RGB versus Unity 75/77/78). Phase 2 isolated Linear color compositing and adjusted local alphas in both localized sets; post-correction render comparison remains pending.
- **Assistant roots:** the seven previously ambiguous roots were crop-compared and resolved to PASS; Figma's neutral canvas and Unity's Home underlay are different composition contexts.
- **Prototype and experience map:** the 24 line elements and all frames are indexed, but connector destinations were not exhaustively extracted and runtime navigation/button interactions were not exercised. Static controller inspection confirms an Attract consent guard, Back guard before awakening and at Performer Select, and a Done handler that returns to Home. This is source inspection, not runtime flow verification.
- **Responsive behavior:** captures use the fixed 1920×1080 design size. Alternative Windows viewport sizes remain NOT VERIFIED.
- **Foundation/shared components:** indexed; full token and per-component context comparison remains PARTIAL. No value was guessed to reconcile the tutorial opacity discrepancy.

### Current status totals after Phase 2

- Figma pages indexed: **12 / 12**
- Structural frames/components/instances inventoried: **655 / 655**
- Metadata nodes enumerated: **9,100**
- Unity runtime roots inventoried: **106 / 106**
- Mapped Unity nodes structurally checked: **2,808**
- Path/type mismatches remaining: **0**
- Numeric geometry mismatches remaining: **0**
- Roots with paired runtime and Figma screenshots: **104 / 106**
- Current root statuses (Phase 2): **95 PARTIAL, 7 PASS, 2 FAIL, 2 NOT VERIFIED; 0 AMBIGUOUS**
- Root screenshot comparisons reviewed: **104 / 106** (contact-sheet scale; close element checks remain outstanding)
- Current frame inventory status totals (Phase 2): **644 PARTIAL, 7 PASS, 2 FAIL, 2 NOT VERIFIED; 0 AMBIGUOUS**
- Figma source screenshots saved: **106 / 106**
- Pages complete: **0 / 12** (all still contain PARTIAL, FAIL, or NOT VERIFIED work)



## Phase 1 extended paired-screen findings (2026-10-27; status snapshot superseded by Phase 2 above)

### Coverage and status reconciliation

- All 12 Figma pages were enumerated from live metadata, producing 655 structural records and 9,100 total nodes. The IDs and dimensions are in the full inventory CSV.
- The 106 Unity runtime catalog roots were each paired with a Figma screenshot. Unity rendering was captured for 104; the two missing Unity captures are `124:3734` (Attract fallback, English) and `124:3767` (Attract fallback, Chinese). Calling `Navigate` correctly rejected Attract because consent was false. Consent and camera gating were not bypassed.
- All 13 comparison sheets (`comparison-01.png` through `comparison-13.png`) were inspected. They cover every captured pair. This is a triage comparison: downscaled pairs reveal major composition and color problems but do not certify every text glyph, edge, and child component.
- Phase 1 runtime-root statuses: 87 PARTIAL, 10 FAIL, 7 AMBIGUOUS, 2 NOT VERIFIED. The current Phase 2 counts are listed above. `FigmaUnityRuntimeRootInventory.csv` is the per-root status checklist; `FigmaUnityFrameInventory.csv` carries an explicit current status for all 655 structural nodes.

### Paired comparison findings

- **Performer visual states:** the six performer themes (Gaia, Pandemic, Re-Birth, White Guardian, Conscience, Sanctuary) have paired Focus, Activation, Living Portrait, Release, and post-video assistant roots where the catalog includes them, with English and Traditional Chinese counterparts. Main artwork crops, image scale, and major positions appear consistent across the visible Focus/Activation/Living Portrait/Release pairs. This is a contact-sheet-level `PARTIAL`, not a certification of individual text and annotation nodes.
- **Tutorial surfaces — FAIL:** Entry/DIM and the tutorial panel system are materially brighter in Unity than Figma. A sample flat panel area measured 24/25/25 RGB in the Figma capture and 75/77/78 in Unity at matched proportional coordinates. Similar lift appears on tutorial panel and translucent component comparisons. The exact rendering root cause is not established. The project's Linear color-space setting is a possible contributor only; changing color space or applying a global alpha multiplier would risk unrelated screens, so no such speculative change was made.
- **Privacy backdrop — FAIL:** English and Chinese imported Privacy screens still show a substantially darker backdrop than the Figma captures. The image/veil layering and missing root backing were repaired, but the resulting composition does not yet match Figma. The supplied user Home screenshot is darker than live Figma Home and was preserved as an explicit Home-only override; it does not establish a Privacy override.
- **Assistant source context — AMBIGUOUS:** the Figma component roots are gray standalone canvases; Unity’s catalog roots show them as overlays on Home. These are not equivalent screenshot contexts, and the approved design does not establish a single background for this component-only node. Kept the existing overlay behavior and did not fabricate a background.
- **Settings, Closing, Souvenir, assistant chooser, performer screens:** major element placement and content grouping are visually aligned in contact-sheet comparisons; status remains PARTIAL pending full-size text/edge inspection and actual action testing.
- **Home screenshot conflict:** the user-provided screenshot remains the Home visual target. Existing Home-local layer values preserve that darker target. Live Figma frame `51:4` is lighter and specifies different values, so the report records this intentional exception rather than replacing the supplied visual.

### Extra source-board inspection and flow correspondence

The following design roots were separately captured from the live Figma file and visually inspected in addition to the 106 runtime-catalog frames: `14:63` (end-to-end visitor flow, 1920×760), `3:45` (interaction-to-visual-state board, 1920×1764), `9:2` (final Unity handoff, 1920×1480), `3:113` (glass and interaction primitives, 1920×1200), and `1:204` (visual foundation, 1920×1080). PNGs are in `Documentation/FigmaScreenshots/` with `-audit` suffixes.

The flow map states the visitor sequence above. Unity source routes Home, Privacy, tutorial assets, performer selection/stages, assistant overlays, Closing, and Souvenir to named catalog entries/callbacks. This confirms source-level correspondences, not that every prototype connector invokes the intended callback in runtime. The flow map screenshot itself has instructional text overlapping the Closing/Souvenir/Reset row; this is visible in the Figma source and is therefore recorded as a source-board layout issue, not an import defect.

The foundation and handoff boards establish a persistent living world, artwork-first hierarchy, contextual glass, and no hard-cut transitions. Unity currently swaps catalog `VisualTreeAsset`s in `AwakeningApplicationController.Navigate`; whether the user-visible transitions preserve those principles has not been runtime-verified. No major navigation architecture rewrite was attempted because it could alter application state/consent behavior beyond a safe targeted visual repair.

### Navigation/source inspection

Static inspection of `AwakeningApplicationController` confirms: Attract navigation checks consent before loading the screen; the consent gate is forced false on startup; Back returns without action until `awakeningSequenceCompletedThisSession`; Back does not leave Performer Select after awakening; Done clears history, disables runtime navigation, and shows Home. The controller routes gesture completion through Color Release and then Performer Select. **These are source-level findings only.** Prototype connector destination extraction and runtime testing of consent, tutorial Continue/dismissal, assistant, Settings, performer transitions, Closing, Souvenir, Back, and Done remain NOT VERIFIED.

### Remaining gaps

- Page 02 foundation and Page 06 glass/fallback/cursor frames were directly captured and inspected as source boards, but the actual Unity token and shared-component behavior has not been checked node-by-node across every reuse context.
- The Page 07 flow map text was inspected: Home → Privacy/Consent → Tutorial → Select → Focus → Assistant → Living Portrait → Post-video Assistant → Closing → Souvenir → Reset. Controller source provides identifiable routes for most stages, but assistant overlay context and the exact prototype connector destinations have not been exhaustively reconciled, and runtime navigation remains untested.
- Nested structural inventory records are complete, but most of the 655 rows have not had an individual visual check; they remain PARTIAL by design. The 104 paired runtime roots provide broad coverage only.
- Attract fallback captures were blocked by the consent guard. All interaction/navigation tests remain unrun; consent and camera state were not bypassed or changed.
- Other Windows viewport sizes and 1920×1080 text edge/cropping inspection at full resolution remain NOT VERIFIED.

## Phase 4 — UI Document architecture regression snapshot (2026-11-05)

### Evidence captured

| Screen | Figma | Unity | Status | Comparison |
|---|---|---|---|---|
| Home EN (consent false) | Root `51:4`; `Documentation/FigmaUnityContactSheets/Phase4/figma-home-en.png` | `Documentation/FigmaUnityContactSheets/Phase4/home-en.png`; paired sheet `Documentation/FigmaUnityContactSheets/Phase4/home-en-comparison.png` | PARTIAL | Both source/runtime images are 1920×1080. Direct inspection shows the major frame geometry, background composition, heading/text placement, language selector, consent card, and button arrangement in their intended positions. The Figma source is lighter than the Unity Home image (full-image mean absolute channel difference around 18–19 RGB levels). The darker supplied Home reference remains the explicit Home-only approved override from the prior audit, so no style change was made. Fine typography/rasterization and other locales/screens are not certified by this one pair. |

### Phase 4 runtime regression result

- Play Mode was active in Unity 6000.4.7f1 with the Game View set to Full HD (1920×1080). The current English Home screenshot was saved successfully. `Assets/Editor/Phase4RuntimeScreenshotCapture.cs` is an editor-only helper that captures the screen currently displayed; it does not switch routes or assert route identity.
- Attempts to click the Traditional Chinese selector, Privacy Details, and Settings, followed by Return, produced visual hover feedback but left the visible scene on English Home. The user clarified that Privacy opened once after an accidental click, but it is not opening reliably; the later foreground Unity capture still showed Home. Record the occurrence as user-reported only, not repeatable route verification or Privacy visual evidence. No Privacy, Settings, Chinese, assistant, or camera-gated runtime capture was made in this phase.
- Consent stayed false. Camera initialization was not authorized or attempted. The user must manually activate Privacy Details in the running Game View to establish the first reliable route interaction; following normal privacy review, the user decides whether to accept consent before camera-gated visual checks.
- The existing frame/root inventory status values were not changed because this phase produced only one paired runtime screen and did not establish evidence to upgrade the other per-frame/root statuses. All earlier PARTIAL, FAIL, AMBIGUOUS, and NOT VERIFIED counts remain the prior inventory snapshot, not new Phase 4 completion totals.

### Architecture relationship

The independent-document source and serialized scene wiring were inspected. The audit confirms 14 distinct main document families plus the overlay document, while Home retains its two localized documents. This inspection is source/scene evidence, not runtime visual verification for those families. Refer to [UIDocumentArchitectureAudit.md](UIDocumentArchitectureAudit.md) for the document lifecycle review and exact Phase 4 interaction blocker. No production UI assets or behavior were changed.





## UIDocument architecture refactor note (2026-11-05)

The UI now has 17 UIDocument GameObjects: two existing localized Home documents, 14 main page-family documents, and a dedicated assistant overlay document. The 106 catalog UXML variants retain their existing logical IDs and are selected by `ScreenDocumentManager`; the existing controller retains route/state, consent, interaction-binding, and assistant backend ownership. Play Mode pointer testing covered both localized Home/Privacy paths with consent false. This architecture change does **not** upgrade the existing frame/root visual-fidelity statuses: no post-refactor screenshot files were persisted, and other screen families/overlay layering remain pending visual regression. See [UIDocumentArchitectureAudit.md](UIDocumentArchitectureAudit.md) for ownership mapping and implementation status.
