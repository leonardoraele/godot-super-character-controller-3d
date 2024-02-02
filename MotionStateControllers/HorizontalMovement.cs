using System;
using Godot;

namespace Raele.SuperCharacter3D.MotionStateControllers;

[GlobalClass]
public partial class HorizontalMovement : MotionStateController
{
	[Export] HorizontalMovementSettings Settings = new();
	[ExportGroup("Debug")]
	[Export(PropertyHint.Flags, "Draw Velocity:1,Draw Input:2")] public int DebugDrawFlags;

	public override void OnEnter(MotionStateTransition transition)
	{
		// Make the character face the input direction unless ForceForwardMotion is true
		if (
			this.Settings.InitialFacingDirection == HorizontalMovementSettings.InitialFacingDirectionEnum.InputDirection
			&& this.Character.InputController.MovementInput.LengthSquared() >= 0.0001f
		) {
			this.Character.Rotation = Vector3.Up
				* (
					this.Character.InputController.MovementInput.AngleTo(Vector2.Up)
					+ GodotUtil.V3ToHV2(this.Character.GetViewport().GetCamera3D().Basis.Z * -1).AngleTo(Vector2.Up)
					// + this.GetViewport().GetCamera3D().Rotation.Y
				);
		}

		// Set initial velocity
		this.Character.ForwardSpeed = this.Character.ForwardSpeed
			* this.Settings.InitialSpeedMultiplier
			+ this.Settings.InitialSpeedBoostUnPSec;
	}

    public override void OnPhysicsProcessStateActive(float delta)
	{
		if ((this.DebugDrawFlags & 2) != 0) {
			DebugDraw3D.DrawArrow(
				this.Character.GlobalPosition,
				this.Character.GlobalPosition + this.Character.InputController.GlobalMovementInput,
				Colors.Green
			);
		}
		float turnAngleRad = this.Character.InputController.GlobalMovementInput.LengthSquared() > Mathf.Epsilon
			&& this.Character.Velocity.LengthSquared() > Mathf.Epsilon
			? this.Character.InputController.GlobalMovementInput.AngleTo(this.Character.Velocity)
			: 0;
		if (turnAngleRad > this.Settings.HarshTurnMaxAngleRad) {
			this.Character.ForwardSpeed = Mathf.MoveToward(
				this.Character.ForwardSpeed,
				0,
				this.Settings.BreakDecelerationUnPSecSq * delta
			);
		} else {
			float turnSpeedRadPSec = Math.Abs(this.Character.ForwardSpeed) <= Mathf.Epsilon
				? float.PositiveInfinity
				: this.Settings.TurnSpeedRadPSec
				* (
					// Avoid multiplying by Infinity because it might result in NaN if the multiplier is 0.
					!float.IsInfinity(this.Settings.TurnSpeedRadPSec) && this.Settings.MaxSpeedUnPSec != 0 && this.Settings.TurnSpeedModifier != null
						? this.Settings.TurnSpeedModifier.Sample(Mathf.Clamp(this.Character.ForwardSpeed / this.Settings.MaxSpeedUnPSec, 0, 1))
						: 1
				);
			float targetSpeedUnPSec = this.Character.InputController.GlobalMovementInput.Length()
				* this.Settings.MaxSpeedUnPSec
				* (
					turnAngleRad > this.Settings.HarshTurnBeginAngleRad
						? Mathf.Pow(
							1 - (turnAngleRad - this.Settings.HarshTurnBeginAngleRad) / (this.Settings.HarshTurnMaxAngleRad - this.Settings.HarshTurnBeginAngleRad),
							this.Settings.HarshTurnVelocityLossFactor
						)
						: 1
				);
			float accelerationUnPSecSq = targetSpeedUnPSec >= this.Character.ForwardSpeed ? this.Settings.AccelerationUnPSecSq
				: Math.Abs(targetSpeedUnPSec - this.Settings.MaxSpeedUnPSec) < 0.01f ? this.Settings.NormalDecelerationUnPSecSq
				: this.Settings.BreakDecelerationUnPSecSq;
			this.Character.ForwardSpeed = Mathf.MoveToward(this.Character.ForwardSpeed, targetSpeedUnPSec, accelerationUnPSecSq * delta);
			if (this.Character.InputController.GlobalMovementInput.LengthSquared() > Mathf.Epsilon) {
				this.Character.RotateTowardDirection(this.Character.InputController.GlobalMovementInput, turnSpeedRadPSec * delta, true);
			}
		}
		this.Character.SidewaySpeed = 0;
		if ((this.DebugDrawFlags & 1) != 0) {
			DebugDraw3D.DrawArrow(this.Character.GlobalPosition, this.Character.GlobalPosition + this.Character.Velocity, Colors.Red);
		}
	}
}
