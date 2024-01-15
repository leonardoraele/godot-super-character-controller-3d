using Godot;

namespace Raele.SuperCharacter3D.MotionStates;

public partial class JumpCanceledState : BaseMotionState
{
    private JumpSettings Settings = null!;

    public override void OnEnter(StateTransition transition)
    {
        base.OnEnter(transition);
        this.Settings = transition.Data.HasValue && transition.Data.Value.AsUInt64() != 0
            ? GodotUtil.GetResource<JumpSettings>(transition.Data.Value.AsUInt64())
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
        this.Character.ApplyHorizontalMovement(this.Character.CalculateOnAirHorizontalMovement());
        this.Character.ApplyVerticalMovement(new() {
            Speed = 0,
            Acceleration = this.Settings.Gravity.FallAccelerationUnPSecSq * this.Settings.JumpCancelAccelerationMultiplier,
        });
        this.Character.MoveAndSlide();
    }
}
