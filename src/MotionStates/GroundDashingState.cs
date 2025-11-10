using Godot;

namespace Raele.SuperCharacterController2D;

public partial class GroundDashingState : GroundedState
{
    public override void OnExit(MotionState.TransitionInfo transition)
    {
		base.OnExit(transition);
		if (
			transition.NextStateName == nameof(FallingState)
			&& (this.Character.DashSettings?.GroundDashIgnoresGravity ?? false)
		) {
			transition.Cancel();
		}
    }

    public override void OnProcessState(float delta)
    {
        base.OnProcessState(delta);
		if (this.DurationActiveMs > (this.Character.DashSettings?.DashMaxDurationMs ?? 0)) {
			this.Character.TransitionMotionState<GroundControlState>();
		} else if (this.Character.DashSettings != null && this.Character.DashSettings.VariableDashLength) {
			if (!Input.IsActionPressed(this.Character.InputSettings.DashAction)) {
				this.Character.TransitionMotionState<GroundControlState>();
			} else if (this.Character.InputController.JumpInputBuffer.ConsumeInput()) {
				this.Character.TransitionMotionState<JumpingState>();
			}
		}
    }

	public override void OnPhysicsProcessState(float delta)
	{
		base.OnPhysicsProcessState(delta);
		(float velocityX, float accelerationX) = this.Character.DashSettings != null
			? (
				this.Character.DashSettings.DashMaxSpeedPxPSec * this.Character.FacingDirection,
				this.Character.DashSettings.DashAccelerationPxPSecSq * delta
			)
			: this.CalculateHorizontalOnFootPhysics(delta);
		(float velocityY, float accelerationY) = (this.Character.DashSettings?.GroundDashIgnoresGravity ?? false)
			&& !this.Character.IsOnFloor()
			? (0, float.PositiveInfinity)
			: this.CalculateVerticalOnFootPhysics();
		this.Character.Accelerate(velocityX, velocityY, accelerationX, accelerationY);
		this.Character.MoveAndSlide();
	}
}
