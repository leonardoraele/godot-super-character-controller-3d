using Godot;

namespace Raele.SuperCharacter3D;

[GlobalClass]
public partial class JumpSettings : Resource
{
	[Export] public float JumpHeightUn { get; private set; } = 3.5f;
	[Export] public float JumpDurationMs { get; private set; } = 500;
	[Export] public Curve? JumpHeightCurve;
	[Export] public GravitySettings Gravity { get; private set; } = new GravitySettings();

	[ExportGroup("Variable Jump Height")]
	/// <summary>
	/// The minimum duration of a jump, in milliseconds. Jump will only be canceled after this much time have passed,
	/// even if the player releases the jump button earlier. If the player releases the jump button before this amount
	/// of time, the jump will continue until this time has passed.
	///
	/// Use this setting to make sure that the player can always jump over the lowest platform in your game even when
	/// they just tap the jump button.
	///
	/// Set this setting to a value higher than <see cref="JumpDurationMs"/> (or just set it to 999999) to disable
	/// variable jump height.
	/// </summary>
	[Export] public float MinJumpDurationMs { get; private set; } = 100;
	/// <summary>
	/// If player cancels the jump earlier by releasing the jump button, this modifier will be applied to the gravity
	/// acceleration for as long as the character is still moving upwards. (carried by the jump's momentum).
	///
	/// Set this to a value higher than 1 to make the character start falling faster when the player cancels the jump.
	/// </summary>
	[Export(PropertyHint.Range, ".5,3,0.05,or_greater,or_less")] public float JumpCancelAccelerationMultiplier { get; private set; } = 1;

	[ExportGroup("Air Control")]
	[Export(PropertyHint.Range, "0,2,0.01")] public float AerialSpeedMultiplier { get; private set; } = 1f;
	[Export(PropertyHint.Range, "0,2,0.01")] public float AerialAccelerationMultiplier { get; private set; } = 1f;
	[Export(PropertyHint.Range, "0,2,0.01")] public float AerialRotationSpeedMultiplier { get; private set; } = 1f;

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
}
