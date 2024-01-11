using System;
using Godot;

namespace Raele.SuperCharacter3D.MotionStates;

public partial class GroundDashingState : BaseGroundedState
{
    public override void OnEnter(TransitionInfo transition)
    {
        base.OnEnter(transition);
		if (this.Character.Settings.Dash == null) {
			GD.PushError(nameof(GroundDashingState), "Failed to start Dash action. Cause: Dash settings are missing.");
			transition.Cancel();
		}
    }
    public override void OnExit(BaseMotionState.TransitionInfo transition)
    {
		base.OnExit(transition);
		if (
			transition.NextState == nameof(FallingState)
			&& (this.Character.Settings.Dash?.GroundDashIgnoresGravity ?? false)
		) {
			transition.Cancel();
		}
    }

    public override void OnProcessState(float delta)
    {
        base.OnProcessState(delta);
		if (this.DurationActiveMs > (this.Character.Settings.Dash?.MaxDurationSec ?? 0) * 1000) {
			this.Character.TransitionMotionState<OnFootState>();
		} else if (this.Character.Settings.Dash != null && this.Character.Settings.Dash.VariableLength) {
			if (!Input.IsActionPressed(this.Character.Settings.Input.DashAction)) {
				this.Character.TransitionMotionState<OnFootState>();
			} else if (this.Character.InputController.JumpInputBuffer.ConsumeInput()) {
				this.Character.TransitionMotionState<JumpingState>();
			}
		}
    }

	public override void OnPhysicsProcessState(float delta)
	{
		base.OnPhysicsProcessState(delta);
		Vector2 targetVelocityXZ = this.Character.Settings.Dash!.MaxSpeedUnPSec
			* Vector2.Up.Rotated(this.Character.Rotation.Y * -1);
		Vector2 accelerationXZ = Vector2.One * this.Character.Settings.Dash.AccelerationUnPSecSq * delta;
		(float targetVelocityY, float accelerationY) = this.Character.Settings.Dash!.GroundDashIgnoresGravity
			&& !this.Character.IsOnFloor()
			? (0, float.PositiveInfinity)
			: this.CalculateVerticalOnFootPhysics();
		this.Character.Accelerate(targetVelocityXZ, targetVelocityY, accelerationXZ, accelerationY);
		this.Character.MoveAndSlide();
	}
}
