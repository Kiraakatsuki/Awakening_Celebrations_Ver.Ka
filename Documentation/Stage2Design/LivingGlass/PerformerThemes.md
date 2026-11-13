# Six chapters, one exhibition

The current [reactive-theme specification](SpatialAddendum/ThemeAndInputSpecification.md) adds atmosphere/reflection tokens, stable-target ownership and direct palette blending to these six shared chapters. `PerformerThemes.json` includes the additive colors; exact spatial bounds remain pending final review.

The source artwork, performer names, catalog IDs, media associations and interaction meanings remain unchanged. Theme data changes ambient treatment and accent, never the component structure or state machine. These are visual/poetic interpretations requiring editorial approval, not new factual biographies or assistant knowledge.

| Performer / code | Accent | Ambient treatment | Target/transition treatment | Restraint |
| --- | --- | --- | --- | --- |
| GAIA / PER-001 | #5BD8ED | Ocean blue/cyan, dark neutral reading field | Cool peripheral illumination; artwork clarity settles into the chapter | No continuously moving water shader required |
| PANDEMIC / PER-002 | #C0AA91 | Mineral charcoal and warm sand | Soft neutral-gold edge, quiet reveal | Preserve the artwork's darkness without dim unreadable controls |
| RE-BIRTH / PER-003 | #E0B0C8 | Desaturated rose and pearl | Pearl/rose edge light and gentle first reveal | No scattered petals/particles needed for identity |
| WHITE-GUARDIAN / PER-004 | #C5E9FA | Cold slate and ice-white | Thin cool rim and clear silhouette | White art never becomes the reading surface behind white text |
| CONSCIENCE / PER-005 | #FF956A | Deep warm charcoal and ember | Bounded warm accent; settled luminance | No flicker, strobe or persistent fire effect |
| SANCTUARY / PER-006 | #F1C57A | Warm ink and amber | Soft amber perimeter and calm expansion | No full-screen bloom or saturated gold UI |

## Shared theme schema

`PerformerThemes.json` defines artwork reference, accent, atmospheric label and proposed bilingual line. The gallery itself keeps one neutral background; only the current target receives local light and full artwork clarity. Hover exit/target changes retarget that light within 220–260ms. No theme changes layout, navigation order, confirmation interaction, typography family, privacy behavior or media completion.

Focus extends a portrait into the chapter art window over the existing 650ms cinematic transition intent. Native Focus boards exist for all six; shared chapter states are demonstrated with GAIA in both locales. Activation uses the existing 3-second passage. Preparing retains actual artwork. The real first decoded frame owns the reveal; real playback completion owns the exit. Return reuses the same identity and restores the selected gallery focus without relaunching media.

Theme assets must remain data-driven in future implementation. One family presenter binds `PerformerId`, `AppLanguage` and semantic state. Keep assistant context independently selected; choosing an assistant theme must not overwrite the exhibition's active performer. Optional decorative effects are removable enhancement layers with no picking or timing authority.

Validate all six artwork crops with the artist, especially full-bleed compositions and faces near edges. Preserve aspect ratio; use per-art focal metadata only after review. Do not upscale the display to solve a crop/layout problem. Higher resolution remains a separate performance decision.

Review refinement: GAIA's narrow gallery crop is right-anchored to keep the face visible; the centered crop cut into the face. Native CROP transforms and the prototype's corresponding object-position propose that editorial adjustment without editing the source image. `CropRefinement.json` records native changes. Artist approval remains pending; chapter art retains its separate wide crop.
