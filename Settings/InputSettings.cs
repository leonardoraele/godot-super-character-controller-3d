using Godot;

namespace Raele.SuperCharacter3D;

[GlobalClass]
public partial class InputSettings : Resource
{
	[Export] public string MoveCameraFrontAction { get; private set; } = "character_move_camerafront";
	[Export] public string MoveCameraBackAction { get; private set; } = "character_move_cameraback";
	[Export] public string MoveCameraLeftAction { get; private set; } = "character_move_cameraleft";
	[Export] public string MoveCameraRightAction { get; private set; } = "character_move_cameraright";
	[Export] public string MoveForwardAction { get; private set; } = "character_move_forward";
	[Export] public string MoveBackwardAction { get; private set; } = "character_move_backward";
	[Export] public string StrafeLeftAction { get; private set; } = "character_strafe_left";
	[Export] public string StrafeRightAction { get; private set; } = "character_strafe_right";
	[Export] public string TurnLeftAction { get; private set; } = "character_turn_left";
	[Export] public string TurnRightAction { get; private set; } = "character_turn_right";
	[Export] public string JumpAction { get; private set; } = "character_jump";
	[Export] public string DashAction { get; private set; } = "character_dash";
	[Export] public string CrouchHoldAction { get; private set; } = "character_crouch_hold";
	[Export] public string CrouchToggleAction { get; private set; } = "character_crouch_toggle";
}
