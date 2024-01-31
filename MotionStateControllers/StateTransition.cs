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

	public override void OnProcessStateActive(ControlledState state, float delta)
	{
		if (string.IsNullOrEmpty(this.NextState)) {
			return;
		}
		switch(this.Condition) {
			case StateTransitionConditionEnum.WhenOnFloor:
				if (state.Character.IsOnFloor()) {
					state.StateMachine.Transition(this.NextState);
				}
				break;
			case StateTransitionConditionEnum.WhenNotOnFloor:
				if (!state.Character.IsOnFloor()) {
					state.StateMachine.Transition(this.NextState);
				}
				break;
			case StateTransitionConditionEnum.WhenOnWall:
				if (state.Character.IsOnWall()) {
					state.StateMachine.Transition(this.NextState);
				}
				break;
			case StateTransitionConditionEnum.WhenNotOnWall:
				if (!state.Character.IsOnWall()) {
					state.StateMachine.Transition(this.NextState);
				}
				break;
		}
	}
}
