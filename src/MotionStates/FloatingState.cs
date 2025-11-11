using Godot;

namespace Raele.SuperCharacterController2D;

public partial class FloatingState : MotionState
{
	public override void OnEnter(TransitionInfo transition)
	{
		base.OnEnter(transition);
		this.Character.MotionMode = CharacterBody2D.MotionModeEnum.Floating;
	}

	public override void OnPhysicsProcessState(float delta)
	{
		base.OnPhysicsProcessState(delta);
		Vector2 targetVelocity = this.Character.InputManager.MovementInput.Normalized()
			* this.Character.MovementSettings.MaxHorizontalSpeedPxPSec;
		this.Character.Velocity = this.Character.Velocity.MoveToward(
			targetVelocity,
			this.Character.MovementSettings.HorizontalAccelerationPxPSecSq
		);
		this.Character.MoveAndSlide();
	}
}
