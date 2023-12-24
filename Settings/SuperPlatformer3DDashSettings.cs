using Godot;

namespace Raele.SuperCharacterController3D;

[GlobalClass]
public partial class SuperPlatformer3DDashSettings : Resource
{
	[Export] public float MaxSpeedUnPSec { get; private set; } = 10;
	[Export] public float AccelerationUnPSecSq { get; private set; } = 25;
	[Export] public float DecelerationUnPSecSq { get; private set; } = 50;
	[Export] public float MaxDurationSec { get; private set; } = float.PositiveInfinity;
	[Export] public bool VariableLength { get; private set; } = true;
	[Export] public bool GroundDashIgnoresGravity { get; private set; } = false;
	[Export] public SuperPlatformer3DAirDashSettings? AirDash;
}
