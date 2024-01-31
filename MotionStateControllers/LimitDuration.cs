using Godot;

namespace Raele.SuperCharacter3D.MotionStateControllers;

[GlobalClass]
public partial class LimitDuration : MotionStateController
{
	[Export] public ulong MaxDurationMs = 1000;
	[Export] public string? StateTransitionOnLimitReached;

	public override void OnProcessStateActive(ControlledState state, float delta)
	{
		if (
			!string.IsNullOrEmpty(this.StateTransitionOnLimitReached)
			&& state.DurationActiveMs >= this.MaxDurationMs
		) {
			state.StateMachine.Transition(this.StateTransitionOnLimitReached);
		}
	}
}
