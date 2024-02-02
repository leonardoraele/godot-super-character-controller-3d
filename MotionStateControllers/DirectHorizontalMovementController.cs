using System;
using Godot;

namespace Raele.SuperCharacter3D.MotionStateControllers;

[GlobalClass]
public partial class DirectHorizontalMovementController : MotionStateController
{
	[Export] public DirectHorizontalMovementSettings Settings = new();
	[ExportGroup("Exit Conditions")]
	[Export] public string? StateTransitionOnHarshTurn;

	[Signal] public delegate void HarshTurnEventHandler();

	public override void OnEnter(MotionStateTransition transition)
		=> this.Settings.ForwardMovement.OnEnter(this.Character);

    public override void OnPhysicsProcessStateActive(float delta)
	{
		float turnAngleRad = this.Character.InputController.GlobalMovementInput.LengthSquared() > Mathf.Epsilon
			&& this.Character.Velocity.LengthSquared() > Mathf.Epsilon
			? this.Character.InputController.GlobalMovementInput.AngleTo(this.Character.Velocity)
			: 0;
		if (turnAngleRad > this.Settings.HarshTurnMaxAngleRad) {
			this.Character.ForwardSpeed = Mathf.MoveToward(
				this.Character.ForwardSpeed,
				0,
				this.Settings.ForwardMovement.BreakDecelerationUnPSecSq * delta
			);
			if (!string.IsNullOrEmpty(this.StateTransitionOnHarshTurn)) {
				this.StateMachine.Transition(this.StateTransitionOnHarshTurn);
			}
			this.EmitSignal(SignalName.HarshTurn);
		} else {
			float turnSpeedRadPSec = Math.Abs(this.Character.ForwardSpeed) <= Mathf.Epsilon
				? float.PositiveInfinity
				: this.Settings.TurnSpeedRadPSec
				* (
					// Avoid multiplying by Infinity because it might result in NaN if the multiplier is 0.
					!float.IsInfinity(this.Settings.TurnSpeedRadPSec) && this.Settings.ForwardMovement.MaxSpeedUnPSec != 0 && this.Settings.TurnSpeedModifier != null
						? this.Settings.TurnSpeedModifier.Sample(Mathf.Clamp(this.Character.ForwardSpeed / this.Settings.ForwardMovement.MaxSpeedUnPSec, 0, 1))
						: 1
				);
			float targetSpeedUnPSec = this.Character.InputController.GlobalMovementInput.Length()
				* this.Settings.ForwardMovement.MaxSpeedUnPSec
				* (
					turnAngleRad > this.Settings.HarshTurnBeginAngleRad
						? Mathf.Pow(
							1 - (turnAngleRad - this.Settings.HarshTurnBeginAngleRad) / (this.Settings.HarshTurnMaxAngleRad - this.Settings.HarshTurnBeginAngleRad),
							this.Settings.HarshTurnVelocityLossFactor
						)
						: 1
				);
			float accelerationUnPSecSq = targetSpeedUnPSec >= this.Character.ForwardSpeed ? this.Settings.ForwardMovement.AccelerationUnPSecSq
				: Math.Abs(targetSpeedUnPSec - this.Settings.ForwardMovement.MaxSpeedUnPSec) < 0.01f ? this.Settings.ForwardMovement.NormalDecelerationUnPSecSq
				: this.Settings.ForwardMovement.BreakDecelerationUnPSecSq;
			this.Character.ForwardSpeed = Mathf.MoveToward(this.Character.ForwardSpeed, targetSpeedUnPSec, accelerationUnPSecSq * delta);
			this.Character.SidewaySpeed = 0;
			if (this.Character.InputController.GlobalMovementInput.LengthSquared() > Mathf.Epsilon) {
				this.Character.RotateTowardDirection(this.Character.InputController.GlobalMovementInput, turnSpeedRadPSec * delta, true);
			}
		}
	}
}
