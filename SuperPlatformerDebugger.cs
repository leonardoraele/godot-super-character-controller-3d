using System;
using Godot;

namespace Raele.SuperPlatformer;

public partial class SuperPlatformerDebugger : Control
{
	[Export] private string ToggleVisibilityInputAction = "ui_menu";

    private SuperPlatformerController Character = null!;

    public override void _Ready()
    {
		if (this.GetParent() is SuperPlatformerController characterController) {
			this.Character = characterController;
		} else {
			GD.PushWarning(
				"Failed to setup CharacterController2DDebugger.",
				"Cause: It should be a child of a BasicCharacterController.",
				"This node will remove itself from the scene."
			);
			this.QueueFree();
			return;
		}
		this.ZIndex = 1000;
		this.Visible = false;
		this.ProcessMode = ProcessModeEnum.Always;
    }

    public override void _Draw()
    {
		float defaultSpeedBarLength = 16;
		float characterSpeed = this.Character.Velocity.Length();
		float characterMaxSpeed = this.Character.MovementSettings.MaxHorizontalSpeedPxPSec;
		float totalSpeedBarLength = defaultSpeedBarLength * (characterSpeed / characterMaxSpeed);
		float cappedSpeedBarLength = Math.Min(defaultSpeedBarLength, totalSpeedBarLength);
		this.DrawLine(Vector2.Zero, this.Character.Velocity.Normalized() * defaultSpeedBarLength, Colors.Gray, -1, true);
		this.DrawLine(Vector2.Zero, this.Character.Velocity.Normalized() * totalSpeedBarLength, Colors.Orange, -1, true);
		this.DrawLine(Vector2.Zero, this.Character.Velocity.Normalized() * cappedSpeedBarLength, Colors.Green, -1, true);
		this.DrawString(ThemeDB.FallbackFont, Vector2.Zero, $"{this.Character.Velocity.Length():N2} px/s", default, default, 8, Colors.Green);

		Vector2 inputPos = Vector2.Down * 32;
		this.DrawLine(inputPos, this.Character.InputController.MovementInput * defaultSpeedBarLength + inputPos, Colors.Green, -1, true);
		this.DrawCircle(inputPos, 2, Colors.Green);
		this.DrawArc(inputPos, defaultSpeedBarLength, 0f, 2 * (float) Math.PI, 360, Colors.Green);
		this.DrawString(ThemeDB.FallbackFont, inputPos, $"({this.Character.InputController.MovementInput.X:N2}, {this.Character.InputController.MovementInput.Y:N2}) {this.Character.InputController.MovementInput.Length():N2}/1 {this.Character.InputController.MovementInput.Angle():N2}º", default, default, 8, Colors.Green);

		if (this.Character.IsOnFloor()) {
			this.DrawLine(Vector2.Zero, this.Character.GetFloorNormal() * 8, Colors.White, 1, true);
		}
		if (this.Character.IsOnWall()) {
			this.DrawLine(this.Character.Velocity.Normalized() * 32, this.Character.Velocity.Normalized() * 32 + this.Character.GetWallNormal() * 8, Colors.White, 1, true);
		}
		this.DrawString(ThemeDB.FallbackFont, Vector2.Down * 8, this.Character.State?.Name ?? "null state", default, default, 8, Colors.White);
    }

    public override void _Process(double delta)
    {
		if (this.Visible) {
        	this.QueueRedraw();
		}
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed(this.ToggleVisibilityInputAction)) {
			this.Visible = !this.Visible;
		}
    }
}
