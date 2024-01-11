using System;
using Godot;

namespace Raele.SuperCharacter3D.MotionStates;

public partial class OnFootState : BaseGroundedState
{
    public override void OnProcessState(float delta)
    {
		base.OnProcessState(delta);
		if (this.Character.InputController.JumpInputBuffer.ConsumeInput()) {
			this.Character.TransitionMotionState<JumpingState>();
		} else if (this.Character.InputController.DashInputBuffer.ConsumeInput()) {
			this.Character.TransitionMotionState<GroundDashingState>();
		} else if (
			Input.IsActionJustPressed(this.Character.Settings.Input.CrouchToggleAction)
			|| Input.IsActionPressed(this.Character.Settings.Input.CrouchHoldAction)
		) {
			this.Character.TransitionMotionState<CrouchState>();
		}
    }

    public override void OnPhysicsProcessState(float delta)
    {
        base.OnPhysicsProcessState(delta);

		// Calculate horizontal velocity
		(Vector2 velocityXZ, Vector2 accelerationXZ) = this.CalculateHorizontalOnFootPhysics(delta);

		// Calculate vertical velocity
		(float velocityY, float accelerationY) = this.CalculateVerticalOnFootPhysics();

		// Apply acceleration
		this.Character.Accelerate(velocityXZ, velocityY, accelerationXZ, accelerationY);

		// Updates facing direction
		this.Character.Rotation = this.CalculateRotationEuler();

		// Perform movement
		this.Character.MoveAndSlide();
    }
}
