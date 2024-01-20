using Godot;

namespace Raele.SuperCharacter3D;

public struct HorizontalMovement {
	public float TargetSpeedUnPSec = 0;
	public float AccelerationUnPSecSq = float.PositiveInfinity;
    public Vector2 TargetDirection = Vector2.Zero;
	public float RotationSpeedDegPSec = float.PositiveInfinity;
    public HorizontalMovement() {}
}
