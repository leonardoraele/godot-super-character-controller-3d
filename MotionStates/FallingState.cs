using System;
using Godot;

namespace Raele.SuperCharacterController3D.MotionStates;

public partial class FallingState : BaseAirState
{
    public override void OnProcessState(float delta)
    {
        base.OnProcessState(delta);
        if (this.Character.IsOnFloor()) {
            this.Character.TransitionMotionState<OnFootState>();
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
        (Vector2 velocityXZ, Vector2 accelerationXZ) = this.CalculateHorizontalOnAirPhysics(delta);
        (float velocityY, float accelerationY) = this.CalculateVerticalOnAirPhysics(delta);
        this.Character.Accelerate(velocityXZ, velocityY, accelerationXZ, accelerationY);
        this.Character.MoveAndSlide();
    }
}
