using Godot;

namespace Raele.SuperCharacter3D.MotionStates;

public partial class JumpCanceledState : BaseMotionState
{
    private JumpSettings Settings = null!;

    public override void OnEnter(StateTransition transition)
    {
        base.OnEnter(transition);
        this.Settings = transition.Data.HasValue && transition.Data.Value.AsUInt64() != 0
            ? GeneralUtility.GetResource<JumpSettings>(transition.Data.Value.AsUInt64())
            : this.Character.Settings.Jump;
    }

    public override void OnProcessState(float delta)
    {
        base.OnProcessState(delta);
        if (this.Character.Velocity.Y >= 0)
        {
            this.Character.StateMachine.Transition<FallingState>();
        }
    }

    public override void OnPhysicsProcessState(float delta)
    {
        base.OnPhysicsProcessState(delta);
        (Vector2 velocityXZ, Vector2 accelerationXZ) = this.Character.CalculateHorizontalOnAirPhysics(delta);
        this.Character.AccelerateXZ(velocityXZ, accelerationXZ);
        this.Character.AccelerateY(0, this.Character.Settings.Jump.Gravity.FallAccelerationUnPSecSq * this.Character.Settings.Jump.JumpCancelAccelerationMultiplier);
        this.Character.MoveAndSlide();
    }
}
