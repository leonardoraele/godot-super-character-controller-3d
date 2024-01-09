using Godot;

namespace Raele.SuperCharacter3D;

[GlobalClass]
public partial class MovementSettings : Resource
{
	[Export] public float MaxSpeedUnPSec { get; private set; } = 6;
	[Export] public float AccelerationUnPSecSq { get; private set; } = 20;
	// [Export] public float TurnSpeedRadPSec { get; private set; } = 1;
	// [Export] public float DecelerationUnPSecSq { get; private set; } = 10;
	[Export] public float DownwardVelocityOnFloor = 10;
	[Export] public float RotationLerpFactor = 0.15f;
}
