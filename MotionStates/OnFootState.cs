using Godot;

namespace Raele.SuperCharacterController3D.MotionStates;

public partial class OnFootState : BaseGroundedState
{
    public override void OnProcessState(float delta)
    {
		base.OnProcessState(delta);
		if (this.Character.InputController.JumpInputBuffer.ConsumeInput()) {
			this.Character.TransitionMotionState<JumpingState>();
		} else if (this.Character.InputController.DashInputBuffer.ConsumeInput()) {
			this.Character.TransitionMotionState<GroundDashingState>();
		}
    }

    public override void OnPhysicsProcessState(float delta)
    {
        base.OnPhysicsProcessState(delta);

		// Calculate horizontal velocity
		(Vector2 velocityXZ, Vector2 accelerationXZ) = this.CalculateHorizontalOnFootPhysics(delta);
		this.Character.AccelerateXZ(velocityXZ, accelerationXZ);

		// Calculate vertical velocity
		(float velocityY, float accelerationY) = this.CalculateVerticalOnFootPhysics();
		this.Character.AccelerateY(velocityY, accelerationY);

		// Perform movement
		this.Character.MoveAndSlide();
    }
}
