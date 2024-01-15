using Godot;

namespace Raele.SuperCharacter3D;

public struct HorizontalMovement {
	public float TargetSpeedUnPSec { get; init; } = 0;
	public float AccelerationUnPSecSq { get; init; } = float.PositiveInfinity;
    public Vector2 TargetDirection { get; init; } = Vector2.Zero;
	public float RotationalSpeedDgPSec { get; init; } = float.PositiveInfinity;
    public HorizontalMovement() {}
}
