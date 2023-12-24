using Godot;

namespace Raele.SuperCharacterController3D;

[GlobalClass]
public partial class SuperPlatformer3DClimbSettings : Resource
{
	/// <summary>
	/// If empty, character will be able to climb any vertical surface that is recognized as a wall. See CharacterBody3D
	/// settings for the definition of what is recognized as a wall.
	/// </summary>
	[Export] public string ClimbSurfaceGroup { get; private set; } = "Climbable";
	[Export] public float UpwardMaxSpeedUnPSec { get; private set; } = 2.5f;
	[Export] public float UpwardAccelerationUnPSecSq { get; private set; } = 6;
	[Export] public float HorizontalMaxSpeedUnPSec { get; private set; } = 3;
	[Export] public float HorizontalAccelerationUnPSecSq { get; private set; } = 6;
	[Export] public float DownardMaxSpeedUnPSec { get; private set; } = 4;
	[Export] public float DownwardAccelerationUnPSecSq { get; private set; } = 12;
	[Export] public float WallMaxDurationSec { get; private set; } = float.PositiveInfinity;
	[Export] public bool AutomaticDropOffOnFloor { get; private set; } = true;
	[Export] public float JumpKickoffSpeedUnPSec = 8;

	[ExportGroup("Wall Sliding")]
	[Export] public bool SlideEnabled { get; private set; } = false;
	[Export] public float SlideMaxSpeedUnPSec { get; private set; } = 3;
	[Export] public float SlideAccelerationUnPSecSq { get; private set; } = 12;

	// [ExportGroup("Wall Run")]
	// [Export] public float SlideAccelerationUnPSecSq { get; private set; } = 12;

	// [ExportGroup("Rail Slide")]
	// [Export] public float SlideAccelerationUnPSecSq { get; private set; } = 12;
}
