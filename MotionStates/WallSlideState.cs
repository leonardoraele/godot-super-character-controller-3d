namespace Raele.SuperCharacter3D.MotionStates;

public partial class WallSlideState : OnWallState
{
    public override void OnProcessStateActive(float delta)
    {
        base.OnProcessStateActive(delta);
		if (this.Character.IsOnFloor()) {
			this.Character.StateMachine.Transition<WalkState>();
		}
    }

    public override void OnPhysicsProcessStateActive(float delta)
    {
        base.OnPhysicsProcessStateActive(delta);
        // this.Character.AccelerateY(
        //     this.Character.Settings.Climb?.SlideMaxSpeedUnPSec ?? 0,
        //     this.Character.Settings.Climb?.SlideAccelerationUnPSecSq ?? 0
        // );
        this.Character.MoveAndSlide();
    }
}
