using Godot;

namespace Raele.SuperCharacter3D;

[GlobalClass]
public partial class DashSettings : Resource
{
	[Export] public float InitialVelocityBoostUnPSec { get; private set; } = 0;
	[Export] public float InitialVelocityModifier { get; private set; } = 1;
	[Export] public float MaxSpeedUnPSec { get; private set; } = 10;
	[Export] public float AccelerationUnPSecSq { get; private set; } = 25;
	[Export] public float MaxDurationSec { get; private set; } = float.PositiveInfinity;
	[Export] public bool VariableLength { get; private set; } = true;
	/// <summary>
	/// If true, the character will not be affected by gravity while dashing, even if they leave the ground during the
	/// dash.
	/// </summary>
	[Export] public bool IgnoreGravity { get; private set; } = false;
}
