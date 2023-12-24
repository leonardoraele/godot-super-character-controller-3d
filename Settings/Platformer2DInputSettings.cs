using Godot;

namespace Raele.SuperPlatformer;

[GlobalClass]
public partial class Platformer2DInputSettings : Resource
{
	[Export] public string MoveRightAction { get; private set; } = "platformer_move_right";
	[Export] public string MoveLeftAction { get; private set; } = "platformer_move_left";
	[Export] public string MoveUpAction { get; private set; } = "platformer_move_up";
	[Export] public string MoveDownAction { get; private set; } = "platformer_move_down";
	[Export] public string JumpAction { get; private set; } = "platformer_jump";
	[Export] public string DashAction { get; private set; } = "platformer_dash";
	/// <summary>
	/// Sets the maximum time between two button presses of a directional button to be recognized as a dash input by
	/// double-tap. Set to 0 to disable double-tap dash.
	/// </summary>
	[Export] public ulong DoubleTapToDashSensitivityMs { get; private set; } = 200;
}
