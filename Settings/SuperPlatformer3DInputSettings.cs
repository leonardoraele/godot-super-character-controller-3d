using Godot;

namespace Raele.SuperCharacterController3D;

public partial class SuperPlatformer3DInputSettings : Resource
{
	[Export] public string MoveForwardAction { get; private set; } = "platformer_move_forward";
	[Export] public string MoveLeftAction { get; private set; } = "platformer_move_left";
	[Export] public string MoveRightAction { get; private set; } = "platformer_move_right";
	[Export] public string MoveBackwardAction { get; private set; } = "platformer_move_backward";
	[Export] public string JumpAction { get; private set; } = "platformer_jump";
	[Export] public string DashAction { get; private set; } = "platformer_dash";
}
