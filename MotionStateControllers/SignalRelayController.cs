using Godot;

namespace Raele.SuperCharacter3D.MotionStateControllers;

public partial class SignalRelayController : MotionStateController {
	[Signal] public delegate void OnRelayEventHandler();

	public void Relay()
	{
		if (this.State.IsActive) {
			this.EmitSignal(SignalName.OnRelay);
		}
	}
}
