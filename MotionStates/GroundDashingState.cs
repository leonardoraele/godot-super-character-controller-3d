using System;
using Godot;

namespace Raele.SuperCharacter3D.MotionStates;

public partial class GroundDashingState : BaseGroundedState
{
	private DashSettings Settings = null!;

    public override void OnEnter(StateTransition transition)
    {
        base.OnEnter(transition);
		this.Settings = (
			transition.Data.HasValue && transition.Data.Value.AsUInt64() != 0
				? GeneralUtility.GetResourceOrDefault<DashSettings>(transition.Data.Value.AsUInt64())
				: this.Character.Settings.Dash
			)
			?? throw new Exception("Failed to start Dash action. Cause: Dash settings are missing.");
		this.Character.Velocity = (this.Character.InputController.MovementInput3DOrNull ?? this.Character.Basis.Z)
			* (
				this.Character.Velocity.Length()
				* this.Settings.InitialVelocityModifier
				+ this.Settings.InitialVelocityBoostUnPSec
			);
    }
    public override void OnExit(StateTransition transition)
    {
		base.OnExit(transition);
		if (transition.NextStateName == nameof(FallingState) && (this.Settings?.IgnoreGravity ?? false)) {
			transition.Cancel();
		}
    }

    public override void OnProcessState(float delta)
    {
        base.OnProcessState(delta);
		if (this.DurationActiveMs > (this.Settings?.MaxDurationSec ?? 0) * 1000) {
			this.Character.StateMachine.Transition<OnFootState>();
		} else if (this.Settings != null && this.Settings.VariableLength) {
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

		// Horizontal movement
		Vector2 targetVelocityXZ = this.Settings.MaxSpeedUnPSec * Vector2.Up.Rotated(this.Character.Rotation.Y * -1);
		Vector2 accelerationXZ = Vector2.One * this.Settings.AccelerationUnPSecSq * delta;

		// Vertical movement
		(float targetVelocityY, float accelerationY) = this.Settings.IgnoreGravity
			&& !this.Character.IsOnFloor()
			? (0, float.PositiveInfinity)
			: this.Character.CalculateVerticalOnFootPhysics();

		// Apply movement
		this.Character.Accelerate(targetVelocityXZ, targetVelocityY, accelerationXZ, accelerationY);
		this.Character.MoveAndSlide();
	}
}
