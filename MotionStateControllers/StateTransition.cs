using Godot;

namespace Raele.SuperCharacter3D.MotionStateControllers;

[GlobalClass]
public partial class StateTransition : MotionStateController
{
	[Export] public StateTransitionConditionEnum Condition;
	[Export] public string? NextState;

	public enum StateTransitionConditionEnum {
		WhenOnFloor,
		WhenNotOnFloor,
		WhenOnWall,
		WhenNotOnWall,
	}

	public override void OnProcessStateActive(float delta)
	{
		if (string.IsNullOrEmpty(this.NextState)) {
			return;
		}
		switch(this.Condition) {
			case StateTransitionConditionEnum.WhenOnFloor:
				if (this.Character.IsOnFloor()) {
					this.StateMachine.Transition(this.NextState);
				}
				break;
			case StateTransitionConditionEnum.WhenNotOnFloor:
				if (!this.Character.IsOnFloor()) {
					this.StateMachine.Transition(this.NextState);
				}
				break;
			case StateTransitionConditionEnum.WhenOnWall:
				if (this.Character.IsOnWall()) {
					this.StateMachine.Transition(this.NextState);
				}
				break;
			case StateTransitionConditionEnum.WhenNotOnWall:
				if (!this.Character.IsOnWall()) {
					this.StateMachine.Transition(this.NextState);
				}
				break;
		}
	}
}
