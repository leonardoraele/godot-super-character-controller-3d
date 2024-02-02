using System;
using Godot;

namespace Raele.SuperCharacter3D.MotionStateControllers;

[GlobalClass]
public partial class FixedDirectionHorizontalMovementController : MotionStateController
{
	[Export] FixedDirectionHorizontalMovementSettings Settings = new();

	public override void OnEnter(MotionStateTransition transition)
		=> this.Settings.ForwardMovement.OnEnter(this.Character);

    public override void OnPhysicsProcessStateActive(float delta)
	{
		float currentSpeed = this.Character.Velocity.Length();
		bool hasVelocity = currentSpeed > Mathf.Epsilon;
		Vector3 currentGlobalDirection = hasVelocity ? this.Character.Velocity.Normalized() : this.Character.Forward;
		float inputStrength = this.Character.InputController.GlobalMovementInput.Length();
		bool hasInput = inputStrength > Mathf.Epsilon;
		Vector3 inputDirection = hasInput
			? this.Character.InputController.GlobalMovementInput.Normalized()
			: Vector3.Zero;
		Vector3 newGlobalDirection = hasVelocity && hasInput
				? GodotUtil.RotateToward(
					currentGlobalDirection,
					this.Character.InputController.GlobalMovementInput.Normalized(),
					this.Settings.TurnSpeedRadPSec * delta
				)
			: hasVelocity ? currentGlobalDirection
			: hasInput ? inputDirection
			: this.Character.Forward;
		Vector3 newLocalDirection = newGlobalDirection.Rotated(Vector3.Up, this.Character.Rotation.Y * -1);
		float maxSpeed = new Vector3(
				newLocalDirection.X * this.Settings.MaxSidewaySpeedUnPSec,
				0,
				newLocalDirection.Z * (
					newLocalDirection.Z < 0
						? this.Settings.ForwardMovement.MaxSpeedUnPSec
						: this.Settings.MaxBackwardSpeedUnPSec
				)
			)
			.Length();
		float targetSpeed = maxSpeed * inputStrength;
		float acceleration = targetSpeed >= currentSpeed ? this.Settings.ForwardMovement.AccelerationUnPSecSq
			: Math.Abs(targetSpeed - maxSpeed) < Mathf.Epsilon ? this.Settings.ForwardMovement.NormalDecelerationUnPSecSq
			: this.Settings.ForwardMovement.BreakDecelerationUnPSecSq;
		float newSpeed = Mathf.MoveToward(currentSpeed, targetSpeed, acceleration * delta);
		this.Character.Velocity = newGlobalDirection * newSpeed;
	}
}
