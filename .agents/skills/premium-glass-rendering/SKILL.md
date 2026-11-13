---
name: premium-glass-rendering
description: "Plan or implement translucent glass, backdrop blur, refraction, lighting, depth, and compositing for Awakening Celebrations on Windows exhibition hardware. Use for rendering materials and passes rather than ordinary USS layout."
---

# Premium glass rendering

Follow repository AGENTS.md; shader or package recommendations require pipeline inspection first.

## Establish the rendering path

Inspect Packages/manifest.json, ProjectSettings/GraphicsSettings.asset, every relevant quality override in ProjectSettings/QualitySettings.asset, referenced pipeline/renderer assets, scene cameras, and Assets/UI Toolkit/PanelSettings.asset. Observed baseline: URP 17.4.0 installed, no Graphics default pipeline, quality profiles referencing a pipeline asset. Installed packages alone do not establish the active renderer. Reconfirm target quality, graphics API, color space, panel render mode and camera composition before proposing a technique.

Determine what lies behind each surface: scene, camera preview, video RenderTexture, or UI in another document. Map capture/pass ordering and alpha handling. Transparent USS color is not a backdrop blur; do not assume browser CSS filters exist in UI Toolkit or that a camera texture includes overlaid UIDocuments.

## Choose and validate a bounded approach

Compare tinted translucency, preblurred artwork and a shared downsampled dynamic blur against the actual moving background requirement. Prefer the smallest compatible solution; inspect existing shaders and packages before proposing dependencies. Explain render-pass changes and obtain approval for major architectural changes.

Budget full-screen passes, overdraw, blur samples, RenderTexture resolution/format, update frequency and GPU memory for the actual Windows exhibition GPU and display. Establish hardware targets rather than inventing them. Share captures when compatible, release owned resources and provide a readable reduced-effects fallback. Never open another camera feed for blur.

Validate contrast over bright and dark video, text clarity, premultiplied/straight alpha, compositing order, depth and subtle refraction without distorting controls. Measure using unity-performance only within runtime authorization; otherwise deliver a pending measurement plan. Preserve all six performer palettes and both locales.
