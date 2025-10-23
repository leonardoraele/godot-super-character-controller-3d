namespace Raele.SuperPlatformer;

public partial class JumpCanceledState : MotionState
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
        (float velocityX, float accelerationX) = this.CalculateHorizontalOnAirPhysics(delta);
        this.Character.AccelerateX(velocityX, accelerationX);
        this.Character.AccelerateY(0, this.Character.JumpSettings.JumpCancelDecelerationPxPSecSq);
        this.Character.MoveAndSlide();
    }
}
