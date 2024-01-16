using Godot;

namespace Raele.SuperCharacter3D.MotionStates;

public partial class FallingState : BaseAirState
{
    public override void OnProcessState(float delta)
    {
        base.OnProcessState(delta);
        if (this.Character.IsOnFloor()) {
            this.Character.StateMachine.Transition<OnFootState>();
        } else if (
            Input.IsActionPressed(this.Character.Settings.Input.JumpAction)
            // && this.Character.StateMachine.GetNode(nameof(GlideState)) is GlideState glideState
            // && glideState.CanGlide != null
        ) {
            this.Character.StateMachine.Transition<GlideState>();
        }
        // TODO
        // else if (
        //     this.Character.IsOnWall()
        //     && Math.Sign(this.Character.InputController.MovementInput.X) == Math.Sign(this.Character.GetWallNormal().X) * -1
        // ) {
        //     this.Character.State.Transition<WallClimbingState>();
        // }
    }

    public override void OnPhysicsProcessState(float delta)
    {
        base.OnPhysicsProcessState(delta);
        this.Character.ApplyHorizontalMovement(this.Character.CalculateOnAirHorizontalMovement());
        this.Character.ApplyVerticalMovement(this.Character.CalculateOnAirVerticalMovement());
        this.Character.MoveAndSlide();
    }
}
