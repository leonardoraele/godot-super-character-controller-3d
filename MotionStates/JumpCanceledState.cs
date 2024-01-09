using Godot;

namespace Raele.SuperCharacter3D.MotionStates;

public partial class JumpCanceledState : BaseMotionState
{
    public override void OnProcessState(float delta)
    {
        base.OnProcessState(delta);
        if (this.Character.Velocity.Y >= 0)
        {
            this.Character.TransitionMotionState<FallingState>();
        }
    }

    public override void OnPhysicsProcessState(float delta)
    {
        base.OnPhysicsProcessState(delta);
        (Vector2 velocityXZ, Vector2 accelerationXZ) = this.CalculateHorizontalOnAirPhysics(delta);
        this.Character.AccelerateXZ(velocityXZ, accelerationXZ);
        this.Character.AccelerateY(0, this.Character.Settings.Jump.FallAccelerationUnPSecSq * this.Character.Settings.Jump.JumpCancelAccelerationMultiplier);
        this.Character.MoveAndSlide();
    }
}
