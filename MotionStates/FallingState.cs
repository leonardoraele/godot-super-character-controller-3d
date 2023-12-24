using System;
using Godot;

namespace Raele.SuperPlatformer;

public partial class FallingState : BaseAirState
{
    public override void OnProcessState(float delta)
    {
        base.OnProcessState(delta);
        if (this.Character.IsOnFloor()) {
            this.Character.TransitionMotionState<LandingRecoveryState>();
        } else if (
            this.Character.IsOnWall()
            && Math.Sign(this.Character.InputController.MovementInput.X) == Math.Sign(this.Character.GetWallNormal().X) * -1
        ) {
            this.Character.TransitionMotionState<WallClimbingState>();
        }
    }

    public override void OnPhysicsProcessState(float delta)
    {
        base.OnPhysicsProcessState(delta);
        (float velocityX, float accelerationX) = this.CalculateHorizontalOnAirPhysics(delta);
        (float velocityY, float accelerationY) = this.CalculateVerticalOnAirPhysics();
        this.Character.Accelerate(velocityX, velocityY, accelerationX, accelerationY);
        this.Character.MoveAndSlide();
    }
}
