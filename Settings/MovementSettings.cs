using Godot;

namespace Raele.SuperCharacter3D;

[GlobalClass]
public partial class MovementSettings : Resource
{
	[Export] public float MaxSpeedUnPSec { get; private set; } = 6;
	[Export] public float AccelerationUnPSecSq { get; private set; } = 20;
	[Export] public float DecelerationUnPSecSq { get; private set; } = 20;

	[ExportGroup("Turn Deceleration")]
	/// <summary>
	/// Turn angle at which the character will start to lose velocity.
	/// Set to 360 to disable.
	/// </summary>
	[Export(PropertyHint.Range, "0,180,15")] public float HarshTurnBeginAngleDg = 120;
	/// <summary>
	/// Turn angle at which the character will lose all velocity to turn.
	/// Set to 360 to disable.
	/// </summary>
	[Export(PropertyHint.Range, "0,180,15")] public float HarshTurnMaxAngleDg = 150;
	/// <summary>
	/// Factor by which the character will lose velocity when turning. This is a power expoent on top of the proportion
	/// of the turn angle between <see cref="HarshTurnBeginAngleDg"/> and <see cref="HarshTurnMaxAngleDg"/>.
	/// Values higher than 1 will make the character lose more velocity with greater turn angles.
	/// </summary>
	[Export(PropertyHint.ExpEasing)] public float HarshTurnVelocityLossFactor = 1f;
	/// <summary>
	/// If the player tries to turn an angle higher than <see cref="HarshTurnMaxAngleDg"/>, the character will retain it's
	/// velocity and this deceleration will be applied until the character comes to a complete halt. Then, they can
	/// turn.
	/// </summary>
	[Export] public float TurnDecelerationUnPSecSq = 20;
	/// <summary>
	/// Maximum turn angle per second, in degrees.
	/// </summary>
	[Export] public float TurnAngleDgPSec = 540;
}