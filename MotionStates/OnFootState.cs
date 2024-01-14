using Godot;

namespace Raele.SuperCharacter3D.MotionStates;

public partial class OnFootState : BaseGroundedState
{
    public override void OnProcessState(float delta)
    {
		base.OnProcessState(delta);
		if (this.Character.InputController.JumpInputBuffer.ConsumeInput()) {
			this.Character.StateMachine.Transition<JumpingState>();
		} else if (this.Character.InputController.DashInputBuffer.ConsumeInput()) {
			this.Character.StateMachine.Transition<GroundDashingState>();
		} else if (
			Input.IsActionJustPressed(this.Character.Settings.Input.CrouchToggleAction)
			|| Input.IsActionPressed(this.Character.Settings.Input.CrouchHoldAction)
		) {
			if (
				this.Character.Settings.Crouch?.Slide != null
				&& this.Character.Velocity.Length() >= (
					this.Character.Settings.Crouch.Slide.MinSpeedUnPSec < 0
						? this.Character.Settings.Movement.MaxSpeedUnPSec
						: this.Character.Settings.Crouch.Slide.MinSpeedUnPSec
				)
			) {
				this.Character.StateMachine.Transition<SlideState>();
			} else {
				this.Character.StateMachine.Transition<CrouchState>();
			}
		}
    }

    public override void OnPhysicsProcessState(float delta)
    {
        base.OnPhysicsProcessState(delta);

		// Calculate horizontal velocity
		(Vector2 velocityXZ, Vector2 accelerationXZ) = this.Character.CalculateHorizontalOnFootPhysics(delta);

		// Calculate vertical velocity
		(float velocityY, float accelerationY) = this.Character.CalculateVerticalOnFootPhysics();

		// Apply acceleration
		this.Character.Accelerate(velocityXZ, velocityY, accelerationXZ, accelerationY);

		// Updates facing direction
		this.Character.Rotation = this.Character.CalculateRotationEuler();

		// Perform movement
		this.Character.MoveAndSlide();
    }
}
