using Godot;
using static Godot.Viewport;

public class MeshGen : Spatial
{
    public DebugDrawEnum DebugMode { get; set; }

    public MeshGen() {
        VisualServer.SetDebugGenerateWireframes(true);
        DebugMode = DebugDrawEnum.Disabled;
    }

    public void ToggleDebugDraw() {
        switch (DebugMode) {
            case Viewport.DebugDrawEnum.Disabled:
                DebugMode = Viewport.DebugDrawEnum.Wireframe;
                break;
            case Viewport.DebugDrawEnum.Wireframe:
                DebugMode = Viewport.DebugDrawEnum.Unshaded;
                break;
            case Viewport.DebugDrawEnum.Unshaded:
                DebugMode = Viewport.DebugDrawEnum.Overdraw;
                break;
            case Viewport.DebugDrawEnum.Overdraw:
                DebugMode = Viewport.DebugDrawEnum.Disabled;
                break;
        }

        GetViewport().DebugDraw = DebugMode;
    }
}
