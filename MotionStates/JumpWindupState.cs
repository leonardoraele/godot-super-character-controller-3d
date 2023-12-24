namespace Raele.SuperPlatformer;

public partial class JumpWindupState : BaseGroundedState
{
    public override void OnEnter(BaseMotionState.TransitionInfo transition)
    {
        base.OnEnter(transition);
        if (this.Character.JumpSettings.JumpWindupDurationMs <= 0) {
            this.Character.TransitionMotionState<JumpingState>();
        }
    }

    public override void OnProcessState(float delta)
    {
        base.OnProcessState(delta);
        if (this.DurationActiveMs > this.Character.JumpSettings.JumpWindupDurationMs)
        {
            this.Character.TransitionMotionState<JumpingState>();
        }
    }

    public override void OnPhysicsProcessState(float delta)
    {
        base.OnPhysicsProcessState(delta);
        (float targetVelocityX, float accelerationX) = this.Character.JumpSettings.JumpingCancelsMomentum
            ? (0, float.PositiveInfinity)
            : this.CalculateHorizontalOnFootPhysics(delta);
        (float targetVelocityY, float accelerationY) = this.CalculateVerticalOnFootPhysics();
        this.Character.Accelerate(targetVelocityX, targetVelocityY, accelerationX, accelerationY);
        this.Character.MoveAndSlide();
    }
}
