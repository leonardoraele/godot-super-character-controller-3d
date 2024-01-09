namespace Raele.SuperCharacter3D.MotionStates;

public partial class WallClimbingState : OnWallState
{
    public override void OnProcessState(float delta)
    {
        base.OnProcessState(delta);
		if (this.Character.Settings.Climb == null
			|| this.DurationActiveMs > this.Character.Settings.Climb.WallMaxDurationSec * 1000
		) {
			this.Character.TransitionMotionState<WallSlidingState>();
		} else if (this.Character.IsOnFloor() && this.Character.Settings.Climb.AutomaticDropOffOnFloor) {
			this.Character.TransitionMotionState<OnFootState>();
		}
    }

    public override void OnPhysicsProcessState(float delta)
    {
        base.OnPhysicsProcessState(delta);
		float targetVelocityY = this.Character.InputController.MovementInput.Y
			* (
				this.Character.InputController.MovementInput.Y > 0 ? this.Character.Settings.Climb!.DownardMaxSpeedUnPSec
					: this.Character.InputController.MovementInput.Y < 0 ? this.Character.Settings.Climb!.UpwardMaxSpeedUnPSec
					: 0
			);
		float accelerationY = targetVelocityY == 0 ? float.PositiveInfinity
			: this.Character.Velocity.Y < targetVelocityY ? this.Character.Settings.Climb!.UpwardAccelerationUnPSecSq * delta
			: this.Character.Settings.Climb!.DownwardAccelerationUnPSecSq * delta;
		this.Character.Accelerate(0, targetVelocityY, float.PositiveInfinity, accelerationY);
		this.Character.MoveAndSlide();
    }
}
