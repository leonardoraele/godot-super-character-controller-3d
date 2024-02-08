using System;
using Godot;

namespace Raele.SuperCharacter3D.MotionStateControllers;

/// <summary>
/// Moves the player toward the direction the player enters with directional input.
///
/// You can set different speed values for sideway movement and backward movement if you want, but this controller does
/// not update the character's facing direction, so you should combine it with another controller that does so for a
/// better result.
///
/// This controller only updates the character's X and Z velocity. You can combine it with other controllers that update
/// the character's facing direction and Y velocity.
/// </summary>
public partial class BasicMovementController : MotionStateController
{
	[Export] BasicMovementSettings Settings = null!;

	public override void OnEnter(MotionStateTransition transition)
		=> this.Settings.OnEnter(this.Character);

    public override void OnPhysicsProcessStateActive(float delta)
	{
		Vector3 horizontalVelocity = new Vector3(this.Character.Velocity.X, 0, this.Character.Velocity.Z);
		float currentSpeed = horizontalVelocity.Length();
		bool hasVelocity = currentSpeed > Mathf.Epsilon;
		Vector3 currentGlobalDirection = hasVelocity ? horizontalVelocity.Normalized() : this.Character.Forward;
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
						? this.Settings.MaxSpeedUnPSec
						: this.Settings.MaxBackwardSpeedUnPSec
				)
			)
			.Length();
		float targetSpeed = maxSpeed * inputStrength;
		float acceleration = targetSpeed >= currentSpeed ? this.Settings.AccelerationUnPSecSq
			: Math.Abs(targetSpeed - maxSpeed) < Mathf.Epsilon ? this.Settings.NormalDecelerationUnPSecSq
			: this.Settings.BreakDecelerationUnPSecSq;
		float newSpeed = Mathf.MoveToward(currentSpeed, targetSpeed, acceleration * delta);
		this.Character.Velocity = newGlobalDirection * newSpeed + Vector3.Up * this.Character.Velocity.Y;
	}
}
