using Godot;

namespace Raele.Supercon2D;

[GlobalClass]
public partial class Platformer2DMovementSettings : Resource
{
	/// <summary>
	/// The character's maximum walking speed. The player's horizontal movement input is applied as a modifier (with 0
	/// being no input and 1 being maximum axis input) to determine the player's intended speed.
	/// </summary>
	[Export] public float MaxHorizontalSpeedPxPSec { get; private set; } = 200;
	/// <summary>
	/// Horizontal acceleration applied while the player's intended speed is higher than the current velocity.
	/// </summary>
	[Export] public float HorizontalAccelerationPxPSecSq { get; private set; } = 600;
	/// <summary>
	/// This is the negative horizontal acceleration applied to the character to reduce their speed if the character
	/// becomes faster than their normal max speed (as defined by <code>MaxHorizontalSpeedPxPSec</code>). This might
	/// happen at the end of a Dash, for example, or if the character is affected by any game-specific mechanic that
	/// changes their velocity. If this happens, this acceleration will be applied every physics tick until the
	/// character's speed is reduced to be within it's max speed range.
	/// Set this propery to 0 to allow the player to maintain any momentum they build indefinitely, until they release
	/// the directional input.
	/// Note that <code>TurnDecelerationPxPSecSq</code> property has priority over this one in a situation where both
	/// apply, so the player can decelerate faster by entering the opposite directional input.
	/// </summary>
	[Export] public float NormalDecelerationPxPSecSq { get; private set; } = 400;
	/// <summary>
	/// This is the negative horizontal acceleration applied to the character while the player's intended speed is lower
	/// than the current speed or in the opposite direction. This happens if the player inputs the opposite directional
	/// button while the character is moving.
	/// This deceleration has priority over <code>NormalDecelerationPxPSecSq</code> in a situation where both apply.
	/// </summary>
	[Export] public float TurnDecelerationPxPSecSq { get; private set; } = 900;

	[ExportGroup("Slopes")]
	[Export] public float MaxSlopeClimbAngle { get; private set; } = 45f;
	[Export] public float SlopeClimbSpeedModifier { get; private set; } = 0.5f;
	[Export] public float SlopeDescendSpeedModifier { get; private set; } = 0.5f;
	/// <summary>
	/// Applies this downard velocity to the character even if they are on the floor. This is useful to exagerate the
	/// effect of slopes and reduce the effect of horizontal physical forces.
	/// </summary>
	[Export] public float DownwardVelocityOnFloor { get; private set; } = 50f;
}
