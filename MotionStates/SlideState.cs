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
        StateTransition dummyTransition = new StateTransition {
            PreviousStateName = transition.PreviousStateName,
            NextStateName = transition.NextStateName,
            Data = transition.Data,
        };
        base.OnEnter(dummyTransition);
        if (dummyTransition.Canceled) {
            transition.Cancel();
            return;
        }
		this.Character.StandUpShape?.Set("disabled", true);
		this.Character.CrouchShape?.Set("disabled", true);
		this.Character.CrawlShape?.Set("disabled", true);
		this.Character.SlideShape?.Set("disabled", false);
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
		this.Character.CrouchShape?.Set("disabled", true);
		this.Character.CrawlShape?.Set("disabled", true);
		this.Character.SlideShape?.Set("disabled", true);
		this.Character.StandUpShape?.Set("disabled", false);
    }
}
