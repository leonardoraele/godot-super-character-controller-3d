namespace Raele.SuperCharacter3D.MotionStates;

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
            this.Character.Settings.Climb?.SlideMaxSpeedUnPSec ?? 0,
            this.Character.Settings.Climb?.SlideAccelerationUnPSecSq ?? 0
        );
        this.Character.MoveAndSlide();
    }
}
