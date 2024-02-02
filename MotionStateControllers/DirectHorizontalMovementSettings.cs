using Godot;

namespace Raele.SuperCharacter3D.MotionStateControllers;

public partial class DirectHorizontalMovementSettings : Resource
{
	[Export] public ForwardMovementSettings ForwardMovement = new();

	[ExportGroup("Turn Speed")]
	/// <summary>
	/// Maximum rotational speed at which the player turn their facing position, in degrees per second.
	///
	/// By default, this property is set to Infinity, which means the player can turn instantly to any direction.
	///
	/// If you set this to 0, or the player won't be able to change the character's facing direction. They will only be
	/// able to move forward, backward, or strafe sideways.
	///
	/// If you set this to a negative value, the player will rotate in the opposite direction of the input.
	/// </summary>
	[Export] public float TurnSpeedDegPSec = 720;
	/// <summary>
	/// Curve that modifies the turn speed based on the current speed of the character. Use this to make the character
	/// turn faster or slower depending on their speed.
	///
	/// The X represents the speed of the character, from 0 to the value set in <see cref="MaxSpeedUnPSec"/>. That is,
	/// the curve will be sampled close to X = 0 as the character's speed approaches 0, and at X = 1 when the character
	/// is moving at their maximum speed. The Y axis represents the multiplier to apply to the turn speed. Y values
	/// higher than 1 make the character turn faster and lower than 1 makes the character turn slower. If Y is 0, the
	/// character can't turn. For example, if Y = 0.5 at X = 1, the character will turn at half their normal
	/// turn speed (as determined by <see cref="TurnSpeedDegPSec"/>) when moving at their max speed (as determined
	/// by <see cref="MaxSpeedUnPSec"/>).
	///
	/// If this property not set, the turn speed is constant at all speed values.
	///
	/// Note that, if <see cref="TurnSpeedDegPSec"/> is set to Infinity (the default), this property has no effect.
	/// </summary>
	[Export] public Curve TurnSpeedModifier = null!;

	[ExportGroup("Velocity Loss On Turn")]
	/// <summary>
	/// Turn angle at which the character will start to lose velocity.
	///
	/// Set to 360 to disable.
	///
	/// Note: Velocity loss on turn might be detrimental to player experience, since it reduces their authonomy over the
	/// control of the character. This option is only recommended for games with slow pace or with "cinamic" aesthetics;
	/// and even then, it is important to animate the character properly to convey the sense of a thigh turn. If done
	/// correctly, it improves the sense of weight of the character.
	/// </summary>
	[Export(PropertyHint.Range, "0,180,15")] public float HarshTurnBeginAngleDeg = 120;
	/// <summary>
	/// Turn angle at which the character will lose all velocity to turn.
	/// Set to 360 to disable.
	/// </summary>
	[Export(PropertyHint.Range, "0,180,15")] public float HarshTurnMaxAngleDeg = 150f;
	/// <summary>
	/// Factor by which the character will lose velocity when turning. This is a power expoent on top of the proportion
	/// of the turn angle between <see cref="HarshTurnBeginAngleDeg"/> and <see cref="HarshTurnMaxAngleDeg"/>.
	/// Values higher than 1 will make the character lose more velocity with greater turn angles.
	/// </summary>
	[Export(PropertyHint.ExpEasing)] public float HarshTurnVelocityLossFactor = 1f;

	[ExportGroup("Automatic Movement")]
	/// <summary>
	/// Minimum input value to consider the character is moving forward. This is used to automatically move the
	/// character forward when the input is below this value. For example, if you set this to 0.5, the character will
	/// move forward automatically when the input is between 0 and 0.5, but the player can still make the character move
	/// faster by pressing a input higher than 0.5.
	///
	/// This might be useful for implementing "Dash" mechanics, where the game forces the character to move forward for
	/// a small time duration before giving control back to the player. It might also useful to implement movement in
	/// games where the player has limited control over speed of the character, but can control the direction.
	/// </summary>
	[Export(PropertyHint.Range, "0,1")] public float MinForwardInput = 0; // TODO Not implemented yet

	public float HarshTurnMaxAngleRad {
		get => Mathf.DegToRad(this.HarshTurnMaxAngleDeg);
		set => this.HarshTurnMaxAngleDeg = Mathf.RadToDeg(value);
	}
	public float HarshTurnBeginAngleRad {
		get => Mathf.DegToRad(this.HarshTurnBeginAngleDeg);
		set => this.HarshTurnBeginAngleDeg = Mathf.RadToDeg(value);
	}
	public float TurnSpeedRadPSec {
		get => Mathf.DegToRad(this.TurnSpeedDegPSec);
		set => this.TurnSpeedDegPSec = Mathf.RadToDeg(value);
	}
}
