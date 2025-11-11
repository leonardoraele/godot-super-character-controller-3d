namespace Raele.SuperCharacterController2D;

public partial class WallClimbingState : OnWallState
{
    public override void OnProcessState(float delta)
    {
        base.OnProcessState(delta);
		if (this.Character.WallMotionSettings == null
			|| this.DurationActiveMs > this.Character.WallMotionSettings.WallClimbMaxDurationSec * 1000
		) {
			this.Character.TransitionMotionState<WallSlidingState>();
		} else if (this.Character.IsOnFloor() && this.Character.WallMotionSettings.AutomaticallyTransitionToFloor) {
			this.Character.TransitionMotionState<GroundControlState>();
		}
    }

    public override void OnPhysicsProcessState(float delta)
    {
        base.OnPhysicsProcessState(delta);
		float targetVelocityY = this.Character.InputManager.MovementInput.Y
			* (
				this.Character.InputManager.MovementInput.Y > 0 ? this.Character.WallMotionSettings!.DownardClimbMaxSpeed
					: this.Character.InputManager.MovementInput.Y < 0 ? this.Character.WallMotionSettings!.UpwardClimbMaxSpeedPxPSec
					: 0
			);
		float accelerationY = targetVelocityY == 0 ? float.PositiveInfinity
			: this.Character.Velocity.Y < targetVelocityY ? this.Character.WallMotionSettings!.UpwardClimbAccelerationPxPSecSq * delta
			: this.Character.WallMotionSettings!.DownwardClimbAccelerationPxPSecSq * delta;
		this.Character.Accelerate(0, targetVelocityY, float.PositiveInfinity, accelerationY);
		this.Character.MoveAndSlide();
    }
}
