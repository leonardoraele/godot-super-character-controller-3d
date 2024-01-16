using System;
using Godot;
using Raele.SuperCharacter3D.MotionStates;

namespace Raele.SuperCharacter3D;

public partial class SlideState : GroundDashingState
{
    public bool CanStandUp => !this.Character.IsOnCeiling()
        && this.Character.StandUpShape != null
        && this.Character.SlideShape != null
        && this.Character.MoveAndCollide(
                Vector3.Up * (
                    this.Character.StandUpShape.Shape.GetDebugMesh().GetAabb().End.Y
                    - this.Character.SlideShape.Shape.GetDebugMesh().GetAabb().End.Y
                ),
                true
            )
            == null;

    public override void OnEnter(StateTransition transition)
    {
        base.OnEnter(transition.Chain(new() {
            Data = this.Character.Settings.Crouch?.Slide?.Momentum?.GetInstanceId()
                ?? throw new Exception("Failed to enter Slide state. Cause: Slide momentum settings are missing."),
        }));
        if (transition.Canceled) {
            return;
        }
		this.Character.SetCollisionShape(SuperCharacter3DController.CollisionShape.Slide);
    }

    public override void OnExit(StateTransition transition)
    {
        base.OnExit(transition);
		if (transition.Canceled) {
			return;
		}
		if (!this.CanStandUp || Input.IsActionPressed(this.Character.Settings.Input.CrouchHoldAction)) {
			this.Character.StateMachine.Transition<CrouchState>();
		}
        this.Character.SetCollisionShape(SuperCharacter3DController.CollisionShape.StandUp);
    }
}
