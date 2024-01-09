using Godot;

namespace Raele.SuperCharacter3D.MotionStates;

public partial class GroundDashingState : BaseGroundedState
{
    public override void OnExit(BaseMotionState.TransitionInfo transition)
    {
		base.OnExit(transition);
		if (
			transition.NextState == nameof(FallingState)
			&& (this.Character.Settings.Dash?.GroundDashIgnoresGravity ?? false)
		) {
			transition.Cancel();
		}
    }

    public override void OnProcessState(float delta)
    {
        base.OnProcessState(delta);
		if (this.DurationActiveMs > (this.Character.Settings.Dash?.MaxDurationSec ?? 0) * 1000) {
			this.Character.TransitionMotionState<OnFootState>();
		} else if (this.Character.Settings.Dash != null && this.Character.Settings.Dash.VariableLength) {
			if (!Input.IsActionPressed(this.Character.Settings.Input.DashAction)) {
				this.Character.TransitionMotionState<OnFootState>();
			} else if (this.Character.InputController.JumpInputBuffer.ConsumeInput()) {
				this.Character.TransitionMotionState<JumpingState>();
			}
		}
    }

	public override void OnPhysicsProcessState(float delta)
	{
		base.OnPhysicsProcessState(delta);
		Vector2 GetXZ(Vector3 v3d) => new Vector2(v3d.X, v3d.Z);
		(Vector2 velocityXZ, Vector2 accelerationXZ) = this.Character.Settings.Dash != null
			? (
				this.Character.Settings.Dash.MaxSpeedUnPSec * GetXZ(this.Character.Transform.Basis.Z.Normalized()),
				Vector2.One * this.Character.Settings.Dash.AccelerationUnPSecSq * delta
			)
			: this.CalculateHorizontalOnFootPhysics(delta);
		(float velocityY, float accelerationY) = (this.Character.Settings.Dash?.GroundDashIgnoresGravity ?? false)
			&& !this.Character.IsOnFloor()
			? (0, float.PositiveInfinity)
			: this.CalculateVerticalOnFootPhysics();
		this.Character.Accelerate(velocityXZ, velocityY, accelerationXZ, accelerationY);
		this.Character.MoveAndSlide();
	}
}
