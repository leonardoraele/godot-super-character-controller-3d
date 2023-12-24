namespace Raele.SuperPlatformer;

public partial class WallSlidingState : OnWallState
{
    public override void OnProcessState(float delta)
    {
        base.OnProcessState(delta);
		if (this.Character.IsOnFloor()) {
			this.Character.TransitionMotionState<OnFootState>();
		}
    }

    public override void OnPhysicsProcessState(float delta)
    {
        base.OnPhysicsProcessState(delta);
        this.Character.AccelerateY(
            this.Character.WallMotionSettings?.SlideMaxSpeedPxPSec ?? 0,
            this.Character.WallMotionSettings?.SlideAccelerationPxPSecSq ?? 0
        );
        this.Character.MoveAndSlide();
    }
}
