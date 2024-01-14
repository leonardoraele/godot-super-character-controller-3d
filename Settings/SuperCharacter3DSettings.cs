using Godot;

namespace Raele.SuperCharacter3D;

[GlobalClass]
public partial class SuperCharacter3DSettings : Resource
{
	[Export] public InputSettings Input = new InputSettings();
	[Export] public MovementSettings Movement = new MovementSettings();
	[Export] public JumpSettings Jump = new JumpSettings();
	[Export] public CrouchSettings? Crouch;
	[Export] public DashSettings? Dash;
	[Export] public AirDashSettings? AirDash;
	[Export] public ClimbSettings? Climb;

	[ExportGroup("Assist Options")]
	[Export] public ulong JumpInputBufferSensitivityMs { get; private set; } = 150;
	[Export] public ulong CoyoteJumpLeniencyMs { get; private set; } = 150;
	[Export] public ulong DashInputBufferSensitivityMs { get; private set; } = 150;
	[Export] public ulong CeilingSlideTimeMs { get; private set; } = 150;
	[Export] public int WallDropPreventionLeniencyMs { get; private set; } = 150;
}
