using Godot;
using Raele.SuperCharacter3D.MotionStates;

namespace Raele.SuperCharacter3D;

public partial class PropelState : BaseMotionState
{
	[Export] public float InitialVelocityMultiplier { get; private set; } = 1;
	[Export] public float InitialVelocityAdditionUnPSec { get; private set; } = 0;
	[Export] public float MaxSpeedUnPSec { get; private set; } = 10;
	[Export] public float AccelerationUnPSecSq { get; private set; } = 25;
	[Export] public float MaxDurationSec { get; private set; } = float.PositiveInfinity;
	[Export] public bool PropelInputDirection = true;
	// [Export] public float TurnSpeedDgPSec { get; private set; } = 720; // TODO
	[Export] public BaseMotionState? VerticalMovementReference;

	[ExportGroup("Exit States")]
	[Export] public BaseMotionState? StateTransitionWhenOnFloor;
	[Export] public BaseMotionState? StateTransitionWhenOnWall;
	[Export] public BaseMotionState? StateTransitionWhenOnAir;

    public override void OnEnter(StateTransition transition)
    {
        base.OnEnter(transition);

		// Make the character face the input direction unless ForceForwardMotion is true
		if (this.PropelInputDirection && this.Character.InputController.MovementInput.Length() >= 0.01f) {
			this.Character.Rotation = Vector3.Up
				* (
					this.Character.InputController.MovementInput.AngleTo(Vector2.Up)
					+ GodotUtil.V3ToHV2(this.GetViewport().GetCamera3D().Basis.Z * -1).AngleTo(Vector2.Up)
					// + this.GetViewport().GetCamera3D().Rotation.Y
				);
		}

		// Setup initial velocity
		this.Character.Velocity = this.Character.Basis.Z * -1
			* (
				this.Character.Velocity.Length()
					* this.InitialVelocityMultiplier
					+ this.InitialVelocityAdditionUnPSec
			);
    }

    public override void OnProcessStateActive(float delta)
    {
        base.OnProcessStateActive(delta);
		if (this.DurationActiveMs > this.MaxDurationSec * 1000) {
			this.StateMachine.Reset();
		} else if (this.StateTransitionWhenOnFloor != null && this.Character.IsOnFloor()) {
			this.StateMachine.Transition(this.StateTransitionWhenOnFloor.Name);
		} else if (this.StateTransitionWhenOnWall != null && this.Character.IsOnWall()) {
			this.StateMachine.Transition(this.StateTransitionWhenOnWall.Name);
		} else if (this.StateTransitionWhenOnAir != null && !this.Character.IsOnFloor()) {
			this.StateMachine.Transition(this.StateTransitionWhenOnAir.Name);
		}
    }

	public override HorizontalMovement GetHorizontalMovement()
		=> new() {
			TargetSpeedUnPSec = this.MaxSpeedUnPSec,
			AccelerationUnPSecSq = this.AccelerationUnPSecSq,
			TargetDirection = GodotUtil.V3ToHV2(this.Character.Basis.Z * -1),
		};

	public override VerticalMovement GetVerticalMovement()
		=> this.VerticalMovementReference?.GetVerticalMovement()
			?? new() { SnapToFloor = true };
}
