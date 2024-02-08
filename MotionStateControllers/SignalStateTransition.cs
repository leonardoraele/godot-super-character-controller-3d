using Godot;

namespace Raele.SuperCharacter3D.MotionStateControllers;

public partial class SignalStateTransition : MotionStateController {
	[Export] public Node? Subject;
	[Export] public string Signal = "";
	[Export] public Node? NextState;

	private Callable Callable;

	public SignalStateTransition() {
		this.Callable = Callable.From(this.OnSignalEmitted);
	}

	public override void OnEnter(MotionStateTransition transition)
	{
		base.OnEnter(transition);
		this.Subject?.Connect(this.Signal, this.Callable);
	}

	public override void OnExit(MotionStateTransition transition)
	{
		base.OnExit(transition);
		this.Subject?.Disconnect(this.Signal, this.Callable);
	}

	private void OnSignalEmitted() {
		if (this.NextState != null) {
			this.StateMachine.Transition(this.NextState.Name);
		}
	}
}
