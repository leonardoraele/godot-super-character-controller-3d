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

    public override HorizontalMovement GetHorizontalMovement()
    {
        HorizontalMovement hMovement = this.Character.CalculateHorizontalMovement();
        hMovement.TargetSpeedUnPSec *= this.Character.Settings.Jump.AerialSpeedMultiplier;
        hMovement.AccelerationUnPSecSq *= this.Character.Settings.Jump.AerialAccelerationMultiplier;
        return hMovement;
    }

    public override VerticalMovement GetVerticalMovement()
        => new() {
            TargetVerticalSpeed = this.Character.Settings.Jump.Gravity.MaxFallSpeedUnPSec * -1,
            Acceleration = this.Character.Settings.Jump.Gravity.FallAccelerationUnPSecSq,
        };
}
