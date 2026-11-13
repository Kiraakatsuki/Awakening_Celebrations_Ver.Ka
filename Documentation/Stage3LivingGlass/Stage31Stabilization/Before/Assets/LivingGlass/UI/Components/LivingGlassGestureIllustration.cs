using UnityEngine;
using UnityEngine.UIElements;

/// <summary>Original vector pose diagrams. These never synthesize recognizer input.</summary>
public sealed class LivingGlassGestureIllustration : VisualElement
{
    private readonly string gesture;
    public LivingGlassGestureIllustration(string pose)
    {
        gesture = pose;
        pickingMode = PickingMode.Ignore;
        generateVisualContent += Draw;
    }
    private void Draw(MeshGenerationContext context)
    {
        Painter2D p = context.painter2D;
        p.strokeColor = LivingGlassAssets.Hex("#C2CBD5");
        p.fillColor = LivingGlassAssets.Hex("#1F3040");
        p.lineWidth = 2.8f;
        float size = Mathf.Min(contentRect.width, contentRect.height);
        Vector2 origin = contentRect.center - new Vector2(size*.5f, size*.5f);
        bool pair = gesture == "raised" || gesture == "clap";
        DrawHand(p, origin, size, pair ? .2f : .32f, pair ? .38f : .53f, gesture == "fist", gesture == "point", false);
        if (pair) DrawHand(p, origin, size, .59f, .38f, false, false, true);
        if (gesture == "wave")
        {
            Line(p, origin, size, new Vector2(.16f,.28f), new Vector2(.05f,.21f), new Vector2(.16f,.14f));
            Line(p, origin, size, new Vector2(.83f,.14f), new Vector2(.94f,.21f), new Vector2(.83f,.28f));
        }
        if (gesture == "raised")
        {
            Line(p, origin, size, new Vector2(.3f,.83f), new Vector2(.3f,.93f));
            Line(p, origin, size, new Vector2(.7f,.83f), new Vector2(.7f,.93f));
        }
        if (gesture == "clap")
        {
            Line(p, origin, size, new Vector2(.47f,.12f), new Vector2(.5f,.23f), new Vector2(.53f,.12f));
            Line(p, origin, size, new Vector2(.48f,.87f), new Vector2(.5f,.78f), new Vector2(.52f,.87f));
        }
    }
    private static void DrawHand(Painter2D p, Vector2 origin, float size, float x, float width, bool fist, bool point, bool mirror)
    {
        // Outline has an articulated thumb and four independently shaped fingers.
        Vector2[] open = { new Vector2(.2f,.9f), new Vector2(.2f,.68f), new Vector2(.04f,.49f), new Vector2(.02f,.4f), new Vector2(.08f,.37f), new Vector2(.23f,.5f), new Vector2(.25f,.2f), new Vector2(.28f,.12f), new Vector2(.35f,.12f), new Vector2(.39f,.41f), new Vector2(.41f,.1f), new Vector2(.45f,.05f), new Vector2(.52f,.05f), new Vector2(.56f,.41f), new Vector2(.59f,.14f), new Vector2(.63f,.1f), new Vector2(.7f,.12f), new Vector2(.73f,.45f), new Vector2(.78f,.27f), new Vector2(.85f,.26f), new Vector2(.91f,.31f), new Vector2(.9f,.63f), new Vector2(.81f,.76f), new Vector2(.81f,.9f) };
        if (fist) open = new[] { new Vector2(.2f,.9f), new Vector2(.2f,.62f), new Vector2(.1f,.51f), new Vector2(.12f,.39f), new Vector2(.23f,.33f), new Vector2(.32f,.38f), new Vector2(.33f,.29f), new Vector2(.46f,.27f), new Vector2(.54f,.31f), new Vector2(.66f,.29f), new Vector2(.77f,.32f), new Vector2(.88f,.34f), new Vector2(.92f,.47f), new Vector2(.87f,.72f), new Vector2(.79f,.79f), new Vector2(.79f,.9f) };
        if (point) open = new[] { new Vector2(.2f,.9f), new Vector2(.2f,.68f), new Vector2(.05f,.49f), new Vector2(.07f,.4f), new Vector2(.16f,.37f), new Vector2(.28f,.48f), new Vector2(.28f,.12f), new Vector2(.33f,.05f), new Vector2(.4f,.05f), new Vector2(.45f,.12f), new Vector2(.46f,.4f), new Vector2(.56f,.35f), new Vector2(.68f,.39f), new Vector2(.79f,.38f), new Vector2(.9f,.43f), new Vector2(.91f,.64f), new Vector2(.8f,.78f), new Vector2(.8f,.9f) };
        p.BeginPath();
        for (int i=0;i<open.Length;i++)
        {
            Vector2 v = origin + new Vector2(x + (mirror ? 1-open[i].x : open[i].x)*width*.65f, .1f + open[i].y*.75f)*size;
            if (i==0) p.MoveTo(v); else p.LineTo(v);
        }
        p.ClosePath(); p.Fill(); p.Stroke();
    }
    private static void Line(Painter2D p, Vector2 origin, float size, params Vector2[] points)
    {
        p.BeginPath();
        for (int i=0;i<points.Length;i++) { Vector2 v = origin + points[i]*size; if (i==0) p.MoveTo(v); else p.LineTo(v); }
        p.Stroke();
    }
}
