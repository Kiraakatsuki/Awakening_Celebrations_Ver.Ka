"""Package existing static Editor captures. Does not run or connect to the application."""
from pathlib import Path
from html import escape
from PIL import Image, ImageDraw, ImageFont
import json

OUT = Path(__file__).resolve().parent
ROOT = OUT.parents[1]
IMAGES = OUT / "EditorPreviews/Final"
font_path = ROOT / "Assets/Fonts/SourceSans3-Regular.ttf"
font = ImageFont.truetype(str(font_path), 23)
small = ImageFont.truetype(str(font_path), 17)
files = sorted(IMAGES.glob("*.png"))
assert len(files) == 42, f"Expected 42 captured views, found {len(files)}"
for path in files:
    image = Image.open(path).convert("RGB")
    if image.size == (1536, 960):
        # Verified design canvas bounds from EditorPreviewGeometry.csv. Crop only letterboxing.
        canvas = image.crop((0, 48, 1536, 912))
        result = Image.new("RGB", (1536, 900), "#080b10")
        result.paste(canvas, (0, 0))
        ImageDraw.Draw(result).text((18, 870), "STATIC UNITY EDITOR VIEW  |  1920 x 1080 logical design canvas fitted to 1536 x 864  |  NOT PLAY MODE", fill="#c2cbd5", font=small)
        result.save(path)

def sheet(names, filename, columns, width=512):
    tile_height = int(width * 900 / 1536)
    rows = (len(names) + columns - 1) // columns
    image = Image.new("RGB", (columns * width, rows * (tile_height + 34)), "#0c1117")
    draw = ImageDraw.Draw(image)
    for i, name in enumerate(names):
        x, y = (i % columns) * width, (i // columns) * (tile_height + 34)
        source = Image.open(IMAGES / (name + ".png")).convert("RGB")
        image.paste(source.resize((width, tile_height)), (x, y + 34))
        draw.text((x + 12, y + 6), name + " · static Editor", fill="#f3f0e8", font=small)
    image.save(OUT / filename)

themes = ["Theme-" + name for name in ["GAIA", "PANDEMIC", "RE-BIRTH", "WHITE-GUARDIAN", "CONSCIENCE", "SANCTUARY"]]
sheet(themes, "SixThemes.png", 3)
overview = [family + "-" + locale for family in ["Home", "Gallery", "Window-Settings", "Window-Assistant-GAIA"] for locale in ["en", "zh"]]
sheet(overview, "BilingualOverview.png", 2)
sheet([path.stem for path in files], "AllViews.png", 6, 384)

reference = Image.open(ROOT / "Documentation/Stage2Design/LivingGlass/review-evidence/Gallery-Arc-en.png").convert("RGB")
actual = Image.open(IMAGES / "Gallery-en.png").convert("RGB").crop((0, 0, 1536, 864))
comparison = Image.new("RGB", (1536, 490), "#0c1117")
draw = ImageDraw.Draw(comparison)
draw.text((18, 12), "Approved Gallery Arc reference", fill="#f3f0e8", font=font)
draw.text((786, 12), "Unity static Editor view · optical baseline", fill="#f3f0e8", font=font)
comparison.paste(reference.resize((768, 432)), (0, 58))
comparison.paste(actual.resize((768, 432)), (768, 58))
comparison.save(OUT / "GalleryComparison.png")

cards = "\n".join(f'<figure><a href="EditorPreviews/Final/{escape(p.name)}"><img loading="lazy" src="EditorPreviews/Final/{escape(p.name)}" alt="{escape(p.stem)} — static Unity Editor view"></a><figcaption>{escape(p.stem)} · static Editor view</figcaption></figure>' for p in files)
html = '''<!doctype html><html lang="en"><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>Living Glass · Stage 3 review</title><style>body{margin:0;background:#0c1117;color:#f3f0e8;font:18px/1.6 system-ui,sans-serif}main{max-width:1600px;margin:auto;padding:40px}h1{font-weight:400;font-size:38px}p{max-width:1000px;color:#c2cbd5}a{color:#a4d9e6}section{display:grid;grid-template-columns:repeat(2,minmax(0,1fr));gap:24px}figure{margin:0;padding:12px;background:#17212c;border-radius:18px}img{width:100%;height:auto}figcaption{padding:10px;font-size:16px}@media(max-width:800px){section{grid-template-columns:1fr}}</style><main><h1>Living Glass — parallel implementation review</h1><p>42 native UI Toolkit views rendered in an isolated Unity Editor preview, using the production view builders. These are <strong>static Editor evidence</strong>, not Play Mode, interaction, camera, video, live AI or performance verification. The final implementation remains a candidate pending operational acceptance.</p><p><a href="README.md">Package index</a> · <a href="GalleryComparison.png">Approved reference comparison</a> · <a href="SixThemes.png">Six performer themes</a> · <a href="ValidationAndAcceptance.md">Validation boundaries</a></p><section>''' + cards + "</section></main></html>"
(OUT / "Review.html").write_text(html, encoding="utf-8")
(OUT / "VisualManifest.json").write_text(json.dumps({"kind": "static Unity Editor preview", "logical_canvas": [1920, 1080], "captured_canvas": [1536, 864], "runtime": False, "views": [p.name for p in files]}, indent=2), encoding="utf-8")
print("Packaged 42 static Editor previews, comparison, bilingual overview and six-theme sheet.")
