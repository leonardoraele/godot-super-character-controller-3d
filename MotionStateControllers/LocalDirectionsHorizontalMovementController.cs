using System;
using Godot;

namespace Raele.SuperCharacter3D.MotionStateControllers;

[GlobalClass]
public partial class LocalDirectionsHorizontalMovementController : MotionStateController
{
	[Export] LocalDirectionsHorizontalMovementSettings Settings = new();

	public override void OnEnter(MotionStateTransition transition)
		=> this.Settings.ForwardMovement.OnEnter(this.Character);

    public override void OnPhysicsProcessStateActive(float delta)
	{
		// Forward movement
		float targetForwardSpeed = this.Character.InputController.RawMovementInput.Y >= 0
			? this.Settings.ForwardMovement.MaxSpeedUnPSec * this.Character.InputController.RawMovementInput.Y
			: this.Settings.MaxBackwardSpeedUnPSec * this.Character.InputController.RawMovementInput.Y;
		float currentForwardSpeed = this.Character.ForwardSpeed;
		DebugDraw2D.SetText(
			"acceleration",
			currentForwardSpeed >= 0 && targetForwardSpeed > currentForwardSpeed ? "forward"
				: currentForwardSpeed <= 0 && targetForwardSpeed < currentForwardSpeed ? "backward"
				: targetForwardSpeed > 0 && Math.Abs(this.Character.InputController.RawMovementInput.Y - 1) < Mathf.Epsilon
				|| targetForwardSpeed < 0 && Math.Abs(this.Character.InputController.RawMovementInput.Y * -1 - 1) < Mathf.Epsilon
					? "normal"
				: "break"
		);
		float acceleration = currentForwardSpeed >= 0 && targetForwardSpeed > currentForwardSpeed
				? this.Settings.ForwardMovement.AccelerationUnPSecSq
			: currentForwardSpeed <= 0 && targetForwardSpeed < currentForwardSpeed
				? this.Settings.BackwardAccelerationUnPSecSqr
			: currentForwardSpeed > 0 && Math.Abs(this.Character.InputController.RawMovementInput.Y - 1) < Mathf.Epsilon
			|| currentForwardSpeed < 0 && Math.Abs(this.Character.InputController.RawMovementInput.Y * -1 - 1) < Mathf.Epsilon
				? this.Settings.ForwardMovement.NormalDecelerationUnPSecSq
			: this.Settings.ForwardMovement.BreakDecelerationUnPSecSq;
		float newSpeed = Mathf.MoveToward(currentForwardSpeed, targetForwardSpeed, acceleration * delta);
		this.Character.LocalVelocity = new Vector3(0, this.Character.Velocity.Y, newSpeed * -1);

		// Turning
		this.Character.Rotation = Vector3.Up * (
			this.Character.Rotation.Y
				+ this.Settings.TurnSpeedRadPSec * this.Character.InputController.RawMovementInput.X * delta * -1
		);
	}
}
