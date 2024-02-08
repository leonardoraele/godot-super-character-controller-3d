using Godot;

namespace Raele.SuperCharacter3D.MotionStateControllers;

/// <summary>
/// Rename to TimeoutStateTransition
/// </summary>
public partial class TimeoutEventEmitter : MotionStateController
{
	[Export] public ulong MaxDurationMs = 1000;

	[Signal] public delegate void TriggerEventHandler();

	public override void OnProcessStateActive(float delta)
	{
		if (this.State.DurationActiveMs >= this.MaxDurationMs) {
			this.EmitSignal(SignalName.Trigger);
		}
	}
}
