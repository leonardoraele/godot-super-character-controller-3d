#nullable enable
using Godot;

namespace Raele.SuperCharacter3D.MotionStateControllers;

public partial class OnCeilingEventEmitter : MotionStateController
{
	[Export] public bool Not = false;

	[Signal] public delegate void TriggerEventHandler();

    public override void OnProcessStateActive(float delta)
    {
        base.OnProcessStateActive(delta);
		if (this.Character.IsOnCeiling() != this.Not) {
			this.EmitSignal(SignalName.Trigger);
		}
    }
}
