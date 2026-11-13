using UnityEngine;
using UnityEngine.UIElements;

/// <summary>One bounded vertex-color light plane; no backdrop capture, shader or render pass.</summary>
public sealed class LivingGlassAtmosphere : VisualElement
{
    private const int Segments = 48;
    private readonly Vertex[] vertices = new Vertex[Segments+1];
    private readonly ushort[] indices = new ushort[Segments*3];
    private Color tint;
    public Color Tint
    {
        get { return tint; }
        set { if (tint == value) return; tint = value; MarkDirtyRepaint(); }
    }
    public LivingGlassAtmosphere()
    {
        pickingMode = PickingMode.Ignore;
        for (int i=0;i<Segments;i++)
        {
            indices[i*3] = 0;
            indices[i*3+1] = (ushort)(i+1);
            indices[i*3+2] = (ushort)((i+1)%Segments+1);
        }
        generateVisualContent += Draw;
    }
    private void Draw(MeshGenerationContext context)
    {
        if (tint.a <= 0 || contentRect.width <= 0 || contentRect.height <= 0) return;
        Vector2 center = contentRect.center;
        vertices[0] = new Vertex { position = new Vector3(center.x, center.y, Vertex.nearZ), tint = tint };
        for (int i=0;i<Segments;i++)
        {
            float angle = i*Mathf.PI*2/Segments;
            vertices[i+1] = new Vertex
            {
                position = new Vector3(center.x+Mathf.Cos(angle)*contentRect.width*.5f, center.y+Mathf.Sin(angle)*contentRect.height*.5f, Vertex.nearZ),
                tint = new Color(tint.r,tint.g,tint.b,0)
            };
        }
        MeshWriteData mesh = context.Allocate(vertices.Length,indices.Length);
        mesh.SetAllVertices(vertices); mesh.SetAllIndices(indices);
    }
}
