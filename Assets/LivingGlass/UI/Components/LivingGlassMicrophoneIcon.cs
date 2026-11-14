using UnityEngine;
using UnityEngine.UIElements;

internal sealed class LivingGlassMicrophoneIcon : VisualElement
{
    public LivingGlassMicrophoneIcon()
    {
        AddToClassList("lg-microphone-icon");
        pickingMode = PickingMode.Ignore;
        generateVisualContent += Draw;
    }

    private static void Draw(MeshGenerationContext context)
    {
        var pen = context.painter2D;
        pen.strokeColor = LivingGlassAssets.Hex("#c2cbd5");
        pen.lineWidth = 1.8f;
        pen.lineCap = LineCap.Round;

        pen.BeginPath();
        pen.MoveTo(new Vector2(10, 13));
        pen.LineTo(new Vector2(10, 7));
        pen.BezierCurveTo(new Vector2(10, 2), new Vector2(18, 2), new Vector2(18, 7));
        pen.LineTo(new Vector2(18, 13));
        pen.BezierCurveTo(new Vector2(18, 18), new Vector2(10, 18), new Vector2(10, 13));
        pen.ClosePath();
        pen.Stroke();

        pen.BeginPath();
        pen.MoveTo(new Vector2(6, 12));
        pen.LineTo(new Vector2(6, 13));
        pen.BezierCurveTo(new Vector2(6, 24), new Vector2(22, 24), new Vector2(22, 13));
        pen.LineTo(new Vector2(22, 12));
        pen.MoveTo(new Vector2(14, 22));
        pen.LineTo(new Vector2(14, 26));
        pen.MoveTo(new Vector2(10, 26));
        pen.LineTo(new Vector2(18, 26));
        pen.Stroke();
    }
}

public sealed class LivingGlassVoiceSymbol : VisualElement
{
    public LivingGlassVoiceSymbol(bool busy)
    {
        AddToClassList("lg-microphone-icon");
        pickingMode = PickingMode.Ignore;
        generateVisualContent += context =>
        {
            var pen = context.painter2D;
            pen.fillColor = LivingGlassAssets.Hex("#f3f0e8");
            if (busy)
            {
                for (int x = 6; x <= 22; x += 8)
                {
                    pen.BeginPath();
                    pen.Arc(new Vector2(x, 14), 1.75f, Angle.Degrees(0), Angle.Degrees(360));
                    pen.Fill();
                }
            }
            else
            {
                pen.BeginPath();
                pen.MoveTo(new Vector2(7, 7));
                pen.LineTo(new Vector2(21, 7));
                pen.LineTo(new Vector2(21, 21));
                pen.LineTo(new Vector2(7, 21));
                pen.ClosePath();
                pen.Fill();
            }
        };
    }
}
