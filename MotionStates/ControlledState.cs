using Godot;

namespace Raele.SuperCharacter3D;

public partial class ControlledState : Node
{
	// FIELDS
	public MotionStateMachine StateMachine { get; private set; } = null!;

	// ACCESSORS
    public SuperCharacter3DController Character => this.StateMachine.Character;
	public bool IsActive => this.StateMachine.CurrentState == this;
	public bool IsPreviousActiveState => this.StateMachine.PreviousState == this;
	public ulong DurationActiveMs => this.StateMachine.TimeSinceLastStateChangeMs;

	// SIGNALS
    [Signal] public delegate void EnterEventHandler(Godot.Collections.Dictionary<string, Variant> transition);
    [Signal] public delegate void ExitEventHandler(Godot.Collections.Dictionary<string, Variant> transition);

	// METHODS
    public override void _EnterTree()
    {
        base._EnterTree();
		if (this.GetParent() is not MotionStateMachine stateMachine) {
			GD.PushError("Node ", this.GetType().Name, " must be a child of ", nameof(MotionStateMachine));
			this.QueueFree();
			return;
		}
		this.StateMachine = stateMachine;
	}

	public void on_enter(Godot.Collections.Dictionary<string, Variant> transition)
		=> this.EmitSignal(SignalName.Enter, transition);

	public void on_exit(Godot.Collections.Dictionary<string, Variant> transition)
		=> this.EmitSignal(SignalName.Exit, transition);

	public override void _PhysicsProcess(double delta)
	{
		if (this.IsActive) {
			this.Character.CallDeferred("move_and_slide");
		}
	}

	public void Transition()
	{
		this.StateMachine.Transition(this.Name);
	}
}
