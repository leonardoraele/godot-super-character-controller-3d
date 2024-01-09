namespace Raele.SuperCharacter3D.MotionStates;

public abstract partial class BaseAirState : BaseMotionState
{
    public override void OnProcessState(float delta)
    {
        base.OnProcessState(delta);
		// Transition Air -> Dash
		if (this.Character.AirDashesPerformedCounter < (this.Character.Settings.Dash?.AirDash?.Uses ?? 0)
			&& this.Character.InputController.DashInputBuffer.ConsumeInput()
		) {
			this.Character.TransitionMotionState<AirDashingState>();
		}
    }

	public override void OnPhysicsProcessState(float delta)
	{
		base.OnPhysicsProcessState(delta);
		this.Character.Rotation = this.CalculateRotationEuler();
	}

	// TODO This is not working property. Supposedly, we should check if the character is about to land every frame and
	// don't trigger the dash if it is—the dash input should be buffered and consumed when the character is on the floor
	// to start a ground dash.
	// private bool CheckCharacterIsAboutToLand()
	// {
	// 	float simulatedPhysicsDelta = 1f / 60;
	// 	(float targetVelocityX, float accelerationX) = this.CalculateHorizontalOnAirPhysics(simulatedPhysicsDelta);
	// 	(float targetVelocityY, float accelerationY) = this.CalculateVerticalOnAirPhysics();
	// 	Vector2 velocity = Platformer2DController.ApplyAcceleration(
	// 		this.Character.Velocity.X,
	// 		this.Character.Velocity.Y,
	// 		targetVelocityX,
	// 		targetVelocityY,
	// 		accelerationX,
	// 		accelerationY
	// 	);
	// 	int framesAhead = Mathf.CeilToInt(this.Character.DashInputBufferSensitivityMs / 1000f / simulatedPhysicsDelta);
	// 	GD.PrintS(new { this.Character.DashInputBufferSensitivityMs, simulatedPhysicsDelta, framesAhead, this.Character.GlobalPosition, velocity, TestMove_velocity = velocity * framesAhead });
	// 	return this.Character.TestMove(this.Character.Transform, velocity * framesAhead);
	// }
}
