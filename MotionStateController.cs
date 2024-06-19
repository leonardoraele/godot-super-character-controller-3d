#nullable enable
using Godot;

namespace Raele.SuperCharacter3D;

[GlobalClass]
public abstract partial class MotionStateController : Node
{
	public ControlledState State { get; private set; } = null!;
	public MotionStateMachine StateMachine => this.State.StateMachine;
	public SuperCharacter3DController Character => this.State.StateMachine.Character;

	public override void _EnterTree()
	{
        base._EnterTree();
		if (this.GetParent() is not ControlledState state) {
			GD.PushError("Failed to instantiate state controller ", this.GetType().Name, ". Cause: It must be a child of ", nameof(ControlledState));
			this.QueueFree();
			return;
		}
		this.State = state;
		this.State.Enter += (dict) => this.OnEnter(MotionStateTransition.FromDictionary(dict));
		this.State.Exit += (dict) => this.OnExit(MotionStateTransition.FromDictionary(dict));
	}

	public virtual void OnEnter(MotionStateTransition transition) {}
	public virtual void OnExit(MotionStateTransition transition) {}
	public override void _Process(double delta)
	{
		if (this.State.StateMachine.CurrentState == this.State) {
			this.OnProcessStateActive((float) delta);
		}
	}
	public override void _PhysicsProcess(double delta)
	{
		if (this.State.StateMachine.CurrentState == this.State) {
			this.OnPhysicsProcessStateActive((float) delta);
		}
	}
	public virtual void OnProcessStateActive(float delta) {}
	public virtual void OnPhysicsProcessStateActive(float delta) {}
}
