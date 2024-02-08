using Godot;

namespace Raele.SuperCharacter3D.MotionStateControllers;

public partial class OnFloorEventEmitter : MotionStateController
{
	[Export] public bool Not = false;

	[Signal] public delegate void TriggerEventHandler();

    public override void OnProcessStateActive(float delta)
    {
        base.OnProcessStateActive(delta);
		if (this.Character.IsOnFloor() != this.Not) {
			this.EmitSignal(SignalName.Trigger);
		}
    }
}
