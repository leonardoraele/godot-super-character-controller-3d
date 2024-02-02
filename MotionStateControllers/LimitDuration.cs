using Godot;

namespace Raele.SuperCharacter3D.MotionStateControllers;

[GlobalClass]
public partial class LimitDuration : MotionStateController
{
	[Export] public ulong MaxDurationMs = 1000;
	[Export] public string? StateTransitionOnLimitReached;

	public override void OnProcessStateActive(float delta)
	{
		if (
			!string.IsNullOrEmpty(this.StateTransitionOnLimitReached)
			&& this.State.DurationActiveMs >= this.MaxDurationMs
		) {
			this.State.StateMachine.Transition(this.StateTransitionOnLimitReached);
		}
	}
}
