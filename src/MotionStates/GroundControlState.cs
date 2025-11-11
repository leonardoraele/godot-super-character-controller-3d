namespace Raele.SuperCharacterController2D;

public partial class GroundControlState : GroundedState
{
    public override void OnProcessState(float delta)
    {
		base.OnProcessState(delta);
		if (this.Character.InputManager.JumpInputBuffer.ConsumeInput()) {
			this.Character.TransitionMotionState<JumpWindupState>();
		} else if (this.Character.InputManager.DashInputBuffer.ConsumeInput()) {
			this.Character.TransitionMotionState<GroundDashingState>();
		}
    }

    public override void OnPhysicsProcessState(float delta)
    {
        base.OnPhysicsProcessState(delta);

		// Calculate horizontal velocity
		(float velocityX, float accelerationX) = this.CalculateHorizontalOnFootPhysics(delta);
		this.Character.AccelerateX(velocityX, accelerationX);

		// Calculate vertical velocity
		(float velocityY, float accelerationY) = this.CalculateVerticalOnFootPhysics();
		this.Character.AccelerateY(velocityY, accelerationY);

		// Perform movement
		this.Character.MoveAndSlide();
    }
}
