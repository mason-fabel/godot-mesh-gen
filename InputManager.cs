using Godot;

public class InputManager : Spatial
{
    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("terrain_generate")) {
            var terrain = GetNode(new NodePath("/root/MeshGen/Terrain")) as Terrain;
            terrain.GenerateTerrain();
        }

        if (@event.IsActionPressed("meshgen_debug_toggle")) {
            var meshGen = GetNode(new NodePath("/root/MeshGen")) as MeshGen;
            meshGen.ToggleDebugDraw();
        }

        if (@event.IsActionPressed("camera_rotate_toggle")) {
            var cameraOrigin = GetNode(new NodePath("/root/MeshGen/CameraOrigin")) as CameraOrigin;
            cameraOrigin.EnableRotation = !cameraOrigin.EnableRotation;
        }
    }
}
