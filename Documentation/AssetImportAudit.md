# Asset and Import Audit

Audit date: 2026-10-10. Read-only inventory of the existing imported Figma folder and referenced project assets.

## Counts and duplicate outputs

- Connector manifest: 106 screen rows.
- Original unique generated UXML assets: 98 (98 `.uxml` and 99 `.uss` including shared `tokens.uss`).
- Manifest names duplicated: the eight `TUTORIAL---*` screens, each listed once in English and once in Chinese but emitted to the same UXML/USS filenames. The originals contained Chinese. This implementation added eight `---ENGLISH.uxml` siblings with separate importer GUIDs and exact English source text.
- Current project now contains 106 UXML assets for 106 screen/language registry entries; the connector manifest itself remains unchanged and still records the original filename collision.
- The developer has since manually imported artwork. The current project contains 45 raster files under `Assets/Art/Figma` plus two generated image files under the imported Figma folder, 47 raster files total.
- The current 106 UXML files contain 126 raster image references. All 126 resolve to existing on-disk assets in the static path check. Presence is confirmed; per-screen visual correspondence and crop/fidelity still require a player-side comparison.

## Verified image references

| Asset | Current importer/reference | Effect |
|---|---|---|
| `HOME BACKGROUND.png` | Exists; valid PNG; its GUID is referenced by both Home UXMLs. Texture importer is Sprite (single); supported by UI Toolkit `background-image`. | Home background is present. |
| `SANCTUARY — DIMMED PRIVACY BACKGROUND.png` | Exists; referenced by the localized Privacy UXMLs. | Privacy visual is present. |
| Manually imported performer, selection, Attract and Release art | Current files exist under `Assets/Art/Figma` in performer-specific folders. | File presence and referenced paths are verified. The rendered frame-to-asset match is **Requires developer runtime verification**. |
| Two generated experience PNGs | Remain in the generated image folder and have references in the current import. | File presence is verified; whether every visual use matches its intended Figma node is not established by path checks alone. |

All 106 current UXML files parsed as XML and all 2,808 connector paths resolved to the declared tags during the structural audit. The latest static pass rechecked all 126 raster references and found no missing image path. Package font URI references resolve through the installed `com.unity.dt.app-ui` package cache, not through `Packages/` source files. Home and privacy backgrounds are embedded as `background-image` inline styles in UXML; Home-specific USS files are empty. The UXML files also reference shared `tokens.uss`.

## Other import limitations

- `Library/UIToolkitConnector/last-import.json` describes the latest imported screen, not the full 106-row project map. It is inspection evidence only and should not be edited.
- `.figma-manifest.json` is the project-level import record. It has root IDs, descendant IDs, snapshot paths, tags, classes and properties, but it does not retain Figma layer names or text values in the `nodes` entries.
- `Assets/Resources/RuntimeNodeBindings.json` is a read-only runtime copy of the import's 106 root records and 2,808 node ID/path/tag/class tuples. It is used for binding assertions; it does not alter or replace connector metadata.
- `matchedMappings` is 0. There is no separate semantic-name-to-UXML mapping database.
- File presence is not a visual-fidelity metric. Compare each imported frame against Figma before marking an asset as visually correct or complete.
- Home image import error `figma_stage_image: png_base64 is not a PNG` is not reproducible from the current valid local Home PNG. The exact failed source payload is unavailable locally, so the original connector failure cause is not verifiable.

## Recommended repair order

1. Compare the current artwork in the Unity Player against its intended Figma frames and verify crop/scale for all six performers.
2. Keep English tutorial variants separately named; the source manifest itself still records the original filename collision.
3. Repair visual assets only against a verified Figma source node/export. Do not substitute unrelated art.

No Figma, Unity Library, or source image/import settings changes were made. The eight new English tutorial UXML files are intentional application-side localized variants; their source copy was taken from the verified Figma English text nodes. They share the imported tutorial USS because those screen USS files carry the existing visual rules.
