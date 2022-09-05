using Godot;

public class CoordinateAxis : ImmediateGeometry
{
    private int AxisLength { get; } = 50;

    public override void _Ready()
    {

    }

    public override void _Process(float delta)
    {
        Clear();

        Begin(Mesh.PrimitiveType.Lines);
        SetColor(new Color(1, 0, 0, 1));
        AddVertex(new Vector3(AxisLength, 0, 0));
        AddVertex(new Vector3(-1 * AxisLength, 0, 0));
        End();

        Begin(Mesh.PrimitiveType.Lines);
        SetColor(new Color(0, 1, 0, 1));
        AddVertex(new Vector3(0, AxisLength, 0));
        AddVertex(new Vector3(0, -1 * AxisLength, 0));
        End();

        Begin(Mesh.PrimitiveType.Lines);
        SetColor(new Color(0, 0, 1, 1));
        AddVertex(new Vector3(0, 0, AxisLength));
        AddVertex(new Vector3(0, 0, -1 * AxisLength));
        End();
    }
}