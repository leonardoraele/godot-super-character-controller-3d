using System;
using Godot;

namespace Raele.SuperCharacter3D.MotionStates;

public partial class GroundDashingState : BaseGroundedState
{
	protected DashSettings Settings = null!;

    public override void OnEnter(StateTransition transition)
    {
        base.OnEnter(transition);
		this.Settings = (
			transition.Data.HasValue && transition.Data.Value.AsUInt64() != 0
				? GodotUtil.GetResourceOrDefault<DashSettings>(transition.Data.Value.AsUInt64())
				: this.Character.Settings.Dash
			)
			?? throw new Exception("Failed to start Dash action. Cause: Dash settings are missing.");
		if (this.Character.InputController.MovementInput.Length() >= 0.01f) {
			this.Character.Rotation = Vector3.Up
				* (
					this.Character.InputController.MovementInput.AngleTo(Vector2.Up)
					+ GodotUtil.V3ToHV2(this.GetViewport().GetCamera3D().Basis.Z * -1).AngleTo(Vector2.Up)
				);
		}
		this.Character.Velocity = this.Character.Basis.Z * -1
			* (
				this.Character.Velocity.Length()
					* this.Settings.InitialVelocityMultiplier
					+ this.Settings.InitialVelocityAdditionUnPSec
			);
    }
    public override void OnExit(StateTransition transition)
    {
		base.OnExit(transition);
		if (transition.NextStateName == nameof(FallingState) && (this.Settings?.IgnoresGravity ?? false)) {
			transition.Cancel();
		}
    }

    public override void OnProcessState(float delta)
    {
        base.OnProcessState(delta);
		if (this.DurationActiveMs > (this.Settings?.MaxDurationSec ?? 0) * 1000) {
			this.Character.StateMachine.Transition<OnFootState>();
		} else if (this.Settings?.VariableLength == true) {
			// TODO // FIXME Slide is a subclass of ground dash, but for the slide state with variable length, the dash
			// should end when the crouch button is released, not when the dash button is released.
			if (!Input.IsActionPressed(this.Character.Settings.Input.DashAction)) {
				this.Character.StateMachine.Transition<OnFootState>();
			} else if (this.Character.IsOnFloor() && this.Character.InputController.JumpInputBuffer.ConsumeInput()) {
				this.Character.StateMachine.Transition<JumpingState>();
			}
		}
    }

	public override void OnPhysicsProcessState(float delta)
	{
		base.OnPhysicsProcessState(delta);
		this.Character.ApplyHorizontalMovement(new() {
			TargetSpeedUnPSec = this.Settings.MaxSpeedUnPSec,
			AccelerationUnPSecSq = this.Settings.AccelerationUnPSecSq,
			TargetDirection = GodotUtil.V3ToHV2(this.Character.Basis.Z * -1),
		});
		if (!this.Settings.IgnoresGravity) {
			this.Character.ApplyVerticalMovement(this.Character.CalculateOnFootVerticalMovement());
		}
		this.Character.MoveAndSlide();
	}
}
