using Godot;

namespace Raele.SuperCharacter3D;

// TODO Rename to PropelSettings, and rename all dash states to use the "propel" term instead. I think we can have a
// single state for ground dash and air dash. PropelState is a generic state for any movement method where the character
// is propelled in a direction with limited ability to stir to other directions. These settings are generic enough to
// allow the user to implement dash all sorts of movement options, like dash, sprint, dive, slide, dodge roll, etc.
[GlobalClass]
public partial class DashSettings : Resource
{
	[Export] public float InitialVelocityMultiplier { get; private set; } = 1;
	[Export] public float InitialVelocityAdditionUnPSec { get; private set; } = 0;
	[Export] public float MaxSpeedUnPSec { get; private set; } = 10;
	[Export] public float AccelerationUnPSecSq { get; private set; } = 25;
	[Export] public float MaxDurationSec { get; private set; } = float.PositiveInfinity;
	[Export] public bool VariableLength { get; private set; } = true;
	/// <summary>
	/// If true, the character will not be affected by gravity while dashing, even if they leave the ground during the
	/// dash.
	/// </summary>
	[Export] public bool IgnoresGravity { get; private set; } = false;
	// [Export] public float TurnSpeedDgPSec { get; private set; } = 720; // TODO
}
