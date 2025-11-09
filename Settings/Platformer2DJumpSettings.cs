using Godot;

namespace Raele.SuperPlatformer;

[GlobalClass]
public partial class Platformer2DJumpSettings : Resource
{
	/// <summary>
	/// Determines the upmost height the character is able to reach, in pixels.
	/// </summary>
	[Export] public float JumpHeightPx { get; private set; } = 100;
	/// <summary>
	/// Determines the time it takes for the character to reach the jump's apex, in miliseconds. If IsCancelable is
	/// true, the player can cancel the jump earlier by releasing the jump button. In this case, the character might not
	/// reach this height.
	/// </summary>
	[Export] public float JumpDurationMs { get; private set; } = 500;
	/// <summary>
	/// By default, the controller uses a simple sine-based easing function to calculate the character's jump height
	/// every frame and applying it to the JumpHeightPx property. Use this property to customize the jump height curve.
	/// The curve's X axis represents the jump progress, and the Y axis represents the jump height. The curve's Y axis
	/// values are relative to the JumpHeightPx property, while the curve's X axis values are relative to the
	/// JumpDurationMs property.
	/// </summary>
	[Export] public Curve? JumpHeightCurve;
	/// <summary>
	/// This is the maximum speed the character can reach while falling. i.e. the air resistance.
	/// </summary>
	[Export] public float MaxFallSpeedPxPSec { get; private set; } = 400f;
	/// <summary>
	/// When the character is falling this property is used as gravity acceleration. This is not a general gravity
	/// applied to the character at all times; it's used only while the character is in midair and not being propelled
	/// by a jump or other mechanic. (i.e. while the character is effectivelly falling while unaffected by any other
	/// force)
	/// </summary>
	[Export] public float FallAccelerationPxPSecSq { get; private set; } = 20;

	[ExportGroup("Air Control")]
	/// <summary>
	/// If this is not set, the controller will fallback to the same settings as the ground movement for air movement.
	/// i.e. the character will move horizontally the same way on the air as it does on the ground.
	/// </summary>
	[Export] public AirControlSettings? AirControlSettings { get; private set; } = null;

	[ExportGroup("Variable Jump Height")]
	/// <summary>
	/// Is true, the player can cancel the jump by releasing the jump button before the character reaches the maximum height.
	/// </summary>
	[Export] public bool VariableJumpHeightEnabled = true;
	/// <summary>
	/// This is the downard acceleration applied to the character (i.e. gravity) when they cancel the jump by releasing
	/// the jump button. This value replaces FallAccelerationPxPSecSq until the character reaches neutral speed. This is
	/// intended to be used as a force to cancel the upward momentum leftover by the jump after it is canceled. Once the
	/// character starts falling, FallAccelerationPxPSecSq takes over as the gravity acceleration applied to the
	/// character.
	///
	/// If IsCancelable is false, this has no effect.
	/// </summary>
	[Export] public float JumpCancelDecelerationPxPSecSq { get; private set; } = 30;

	// [ExportGroup("Air Jumping")]
	// [Export] public int NumberOfAerialJumpsAllowed { get; private set; } = 0;
	// [Export] public Vector2 AerialJumpKickstartSpeedPxPSec { get; private set; } = 200;
	// [Export] public float AerialJumpVerticalDecelerationPxPSec { get; private set; } = 4f;
	// [Export] public float AerialJumpCancelDecelerationMultiplier { get; private set; } = 4f;
	// [Export] public float AerialJumpVerticalMaxHeightPx { get; private set; } = 72;

	// [ExportGroup("Yoshi Fluttering")]
	// [Export] public int FlutterStartTimeMs { get; private set; } = 0;
	// [Export] public float FlutterAccelerationPxPSec { get; private set; } = 150;
	// [Export] public int FlutterDurationMs { get; private set; } = 150;
	// [Export] public float FlutterHorizontalAccelerationMultiplier { get; private set; } = 1.43f;
	// [Export] public float FlutterHorizontalMaxSpeedMultiplier { get; private set; } = 0.7f;
	// [Export] public float FlutterHorizontalControlMultiplier { get; private set; } = 0.7f;

	[ExportGroup("Jump Windup & Landing Recovery")]
	/// <summary>
	/// This is the time it takes for the character to perform a jump after the input has been given. During this time,
	/// the character can't move. If set to 0 (default), the character jumps immediately.
	///
	/// This option takes control away from the player and slows the pacing of the game. Use it in games with a slower
	/// pacing and with a punishing or retro aesthetics. Avoid this option in fast-paced games and precision
	/// platformers.
	///
	/// If you use this option, it is recommended that you add a windup/anticipation animation for the duration of this
	/// delay to properly communicate to the player the jump delay.
	/// </summary>
	[Export] public ulong JumpWindupDurationMs { get; private set; } = 0;
	/// <summary>
	/// If true, the character's momentum will be zeroed when they start a jump. This is particularly useful if you are
	/// using the JumpDelayDurationMs option and want the character completely still for the jump windup animation.
	/// </summary>
	[Export] public bool JumpingCancelsMomentum { get; private set; } = false;
	// /// <summary>
	// /// The minimum downward speed the character must be falling at when it lands on the ground in order to require
	// /// recovery time before being able to move and perform other actions.
	// /// </summary>
	// [Export] public float MinimumFallSpeedToRequireRecoveryPxPSec = 400f; // TODO
	/// <summary>
	/// This is the minimum downward Speed the character must be falling at to activate the Landing Recovery state.
	/// Set this to 0 to make the landing recovery activate regardless of the character's speed.
	/// </summary>
	[Export] public float LandingRecoverySpeedThresholdPxPSec { get; private set; } = float.PositiveInfinity;
	/// <summary>
	/// This is the time duration on which the character can't move or jump after they have landed. If set to 0
	/// (default), the character can move, dash, or jump immediately as soon as it touches the ground at the end of a
	/// jump.
	///
	/// This option takes control away from the player and slows the pacing of the game. Use it in games with a slower
	/// pacing and with a punishing or retro aesthetics. Avoid this option in fast-paced games and precision
	/// platformers.
	///
	/// If you use this option, it is recommended that you implement a recovery animation to communicate to the player
	/// why they can't move after landing.
	/// </summary>
	[Export] public ulong LandingRecoveryDurationMs { get; private set; } = 0;
	/// <summary>
	/// If true, the character's momentum will be zeroed when they land a jump. This is particularly useful if you are
	/// using the LandingDelayDurationMs option and want the character completely still for the landing animation.
	/// </summary>
	[Export] public bool LandingCancelsMomentum { get; private set; } = false;
	/// <summary>
	/// If true, the player can dash during the landing delay, canceling the delay. This option does nothing if
	/// LandingDelayDurationMs is 0 (which is the default).
	/// </summary>
	[Export] public bool DashingCancelsLandingRecovery { get; private set; } = false;
}
