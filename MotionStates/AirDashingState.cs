namespace Raele.SuperPlatformer;

public partial class AirDashingState : OnAirState
{
    public override void OnExit(MotionState.TransitionInfo transition)
    {
        // TODO // FIXME There's a problem here. Even though we are preventing the player from perfoming a dash over
        // another when CanCancelDash is false, the dash input will already have been consumed, which means the player
        // can't buffer dash inputs to perform a dash as soon as possible.
        base.OnExit(transition);
        if (
            transition.NextStateName != nameof(AirDashingState)
            || (this.Character.DashSettings?.VariableDashLength ?? true)
        ) {
            transition.Cancel();
        }
    }

    public override void OnProcessState(float delta)
    {
        base.OnProcessState(delta);
        if (this.DurationActiveMs > (this.Character.DashSettings?.DashMaxDurationMs ?? 0))
        {
            this.Character.TransitionMotionState<FallingState>();
        }
    }

    public override void OnPhysicsProcessState(float delta)
    {
        base.OnPhysicsProcessState(delta);
        (float velocityX, float accelerationX) = this.Character.DashSettings != null
            ? (
                this.Character.DashSettings.DashMaxSpeedPxPSec * this.Character.FacingDirection,
                this.Character.DashSettings.DashAccelerationPxPSecSq * delta
            )
            : this.CalculateHorizontalOnFootPhysics(delta);
        (float velocityY, float accelerationY) = this.Character.DashSettings?.AirDashIgnoresGravity == true
            ? (0, float.PositiveInfinity)
            : this.CalculateVerticalOnAirPhysics();
        this.Character.Accelerate(velocityX, velocityY, accelerationX, accelerationY);
        this.Character.MoveAndSlide();
    }
}
