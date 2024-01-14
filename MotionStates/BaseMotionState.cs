using Godot;

namespace Raele.SuperCharacter3D.MotionStates;

public abstract partial class BaseMotionState : Node, MotionState
{
    protected SuperCharacter3DController Character { get; private set; } = null!;

	public ulong DurationActiveMs => this.Character.StateMachine.TimeSinceLastStateChangeMs;

    public override void _EnterTree()
    {
        base._EnterTree();
		if (this.GetParent() is MotionStateMachine stateMachine) {
			this.Character = stateMachine.Character;
		} else {
			GD.PushError("Node ", this.GetType().Name, " must be a child of ", nameof(MotionStateMachine));
			this.QueueFree();
		}
	}

    public virtual void OnEnter(StateTransition transition) {}
	public virtual void OnExit(StateTransition transition) {}
	public virtual void OnProcessState(float delta) {}
	public virtual void OnPhysicsProcessState(float delta) {}
}
