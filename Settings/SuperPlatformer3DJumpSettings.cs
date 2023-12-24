using Godot;

namespace Raele.SuperCharacterController3D;

[GlobalClass]
public partial class SuperPlatformer3DJumpSettings : Resource
{
	[Export] public float JumpHeightUn { get; private set; } = 3.5f;
	[Export] public float JumpDurationMs { get; private set; } = 500;
	[Export] public Curve? JumpHeightCurve;
	[Export] public float MaxFallSpeedUnPSec { get; private set; } = 20;
	[Export] public float FallAccelerationUnPSecSq { get; private set; } = 30;

	[ExportGroup("Variable Jump Height")]
	[Export] public bool VariableJumpHeightEnabled = true;
	[Export(PropertyHint.Range, "1,3,0.05,or_greater")] public float JumpCancelAccelerationMultiplier { get; private set; } = 1;

	// [ExportGroup("Air Control")]
	// [Export] public SuperPlatformer3DAirControlSettings? AirControl { get; private set; } = null;

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
