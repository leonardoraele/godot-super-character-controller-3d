using Godot;

namespace Raele.SuperCharacterController3D;

[GlobalClass]
public partial class SuperPlatformer3DBaseSettings : Resource
{
	[Export] public SuperPlatformer3DInputSettings Input = new SuperPlatformer3DInputSettings();
	[Export] public SuperPlatformer3DMovementSettings Movement = new SuperPlatformer3DMovementSettings();
	[Export] public SuperPlatformer3DJumpSettings Jump = new SuperPlatformer3DJumpSettings();
	[Export] public SuperPlatformer3DDashSettings? Dash;
	[Export] public SuperPlatformer3DClimbSettings? Climb;

	[ExportGroup("Assist Options")]
	[Export] public ulong JumpInputBufferSensitivityMs { get; private set; } = 150;
	[Export] public ulong CoyoteJumpLeniencyMs { get; private set; } = 150;
	[Export] public ulong DashInputBufferSensitivityMs { get; private set; } = 150;
	[Export] public ulong CeilingSlideTimeMs { get; private set; } = 150;
	[Export] public int WallDropPreventionLeniencyMs { get; private set; } = 150;
}
