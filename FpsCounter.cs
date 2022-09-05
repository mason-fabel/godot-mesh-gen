using System;
using System.Text;
using Godot;
using static Godot.Viewport;

public class FpsCounter : Label
{
    private MeshGen MeshGen { get; set; }
    private Terrain Terrain { get; set; }

    public override void _Ready()
    {
        Modulate = new Color(0, 1, 0, 1);
    }

    public override void _Process(float delta)
    {
        if (MeshGen == null) {
            MeshGen = GetNode(new NodePath("/root/MeshGen")) as MeshGen;
        }

        if (Terrain == null) {
            Terrain = GetNode(new NodePath("/root/MeshGen/Terrain")) as Terrain;
        }

        var stringBuilder = new StringBuilder();
        stringBuilder.Append(Terrain.ElapsedMs.HasValue ? $"Generated in {Terrain.ElapsedMs.Value}" : "Generating terrain...");
        stringBuilder.Append($"\nDebug render mode: {Enum.GetName(typeof(DebugDrawEnum), MeshGen.DebugMode)}");
        stringBuilder.Append($"\nFPS: {Engine.GetFramesPerSecond()}");

        Text = stringBuilder.ToString();
    }
}