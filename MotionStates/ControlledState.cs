using Godot;

namespace Raele.SuperCharacter3D;

public partial class ControlledState : Node, IMotionState
{
	[Export] public Godot.Collections.Array<MotionStateController>? Controls;

	public MotionStateMachine StateMachine { get; private set; } = null!;
    public SuperCharacter3DController Character => this.StateMachine.Character;
	public bool IsActive => this.StateMachine.CurrentState == this;
	public bool IsPreviousActiveState => this.StateMachine.PreviousState == this;
	public ulong DurationActiveMs => this.Character.StateMachine.TimeSinceLastStateChangeMs;

    [Signal] public delegate void EnterEventHandler();
    [Signal] public delegate void ExitEventHandler();

    public override void _EnterTree()
    {
        base._EnterTree();
		if (this.GetParent() is MotionStateMachine stateMachine) {
			this.StateMachine = stateMachine;
		} else {
			GD.PushError("Node ", this.GetType().Name, " must be a child of ", nameof(MotionStateMachine));
			this.QueueFree();
		}
	}

    public void OnEnter(MotionStateTransition transition)
    {
		if (this.Controls != null) {
			foreach (MotionStateController control in this.Controls) {
				control.OnEnter(this, transition);
			}
		}
		this.EmitSignal(SignalName.Enter);
    }

	public void OnExit(MotionStateTransition transition)
	{
		if (this.Controls != null) {
			foreach (MotionStateController control in this.Controls) {
				control.OnExit(this, transition);
			}
		}
		this.EmitSignal(SignalName.Exit);
	}

	public override void _Process(double delta)
	{
		if (this.Controls != null) {
			foreach (MotionStateController control in this.Controls) {
				control.OnProcess(this, (float) delta);
			}
		}
	}

	public void OnProcessStateActive(float delta)
	{
		if (this.Controls != null) {
			foreach (MotionStateController control in this.Controls) {
				control.OnProcessStateActive(this, delta);
			}
		}
	}

    public void OnPhysicsProcessStateActive(float delta)
    {
		if (this.Controls != null) {
			foreach (MotionStateController control in this.Controls) {
				control.OnPhysicsProcessStateActive(this, delta);
			}
		}
    }
}
