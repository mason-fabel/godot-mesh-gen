using Godot;

public class CameraOrigin : Spatial
{
    public bool EnableRotation { get; set; } = true;

    public override void _Process(float delta)
    {
        if (EnableRotation) RotateY(0.1f * delta);
    }
}