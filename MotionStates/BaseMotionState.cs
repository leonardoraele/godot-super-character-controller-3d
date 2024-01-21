using Godot;

namespace Raele.SuperCharacter3D.MotionStates;

public abstract partial class BaseMotionState : Node, IMotionState
{
	[ExportGroup("Ability")]
	[Export] public Godot.Collections.Array<AbilityData>? RechargedAbilitiesOnEnter;
	[ExportGroup("Collision Shape")]
	[Export] public CollisionShape3D? ActiveCollisionShape;
	[Export] public bool DisableSiblingCollisionShapes;
	[ExportCategory("Exit States")]
	[Export] public float MaxDurationSec = float.PositiveInfinity;
	[Export] public Node? StateTransitionWhenMaxDurationReached;
	[Export] public Node? StateTransitionWhenOnFloor;
	[Export] public Node? StateTransitionWhenNotOnFloor;
	[Export] public Node? StateTransitionWhenOnWall;
	[Export] public Node? StateTransitionWhenNotOnWall;


    public SuperCharacter3DController Character => this.StateMachine.Character;
	public MotionStateMachine StateMachine { get; private set; } = null!;
	public bool IsActive => this.StateMachine.CurrentState == this;
	public bool IsLastActiveState => this.StateMachine.PreviousState == this;

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

    public virtual void OnEnter(StateTransition transition)
	{
		if (this.DisableSiblingCollisionShapes) {
			foreach (Node? child in this.ActiveCollisionShape?.GetParent()?.GetChildren() ?? new()) {
				if (child != null && child is CollisionShape3D shape) {
					shape.Disabled = true;
				}
			}
		}
		if (this.RechargedAbilitiesOnEnter != null) {
			foreach (AbilityData ability in this.RechargedAbilitiesOnEnter ?? new()) {
				ability.Recharge();
			}
		}
		if (this.ActiveCollisionShape != null) {
			this.ActiveCollisionShape.Disabled = false;
		}
	}
	public virtual void OnExit(StateTransition transition) {}
	public virtual void OnProcessStateActive(float delta)
	{
		this.CheckExitTransitions();
	}
	public virtual void OnPhysicsProcessStateActive(float delta)
	{
		// if (this.HorizontalControl != null) this.Character.ApplyHorizontalMovement(this.HorizontalControl.GetHorizontalMovement());
		// if (this.VerticalControl != null) this.Character.ApplyVerticalMovement(this.VerticalControl.GetVerticalMovement());
		// this.Character.MoveAndSlide();
		this.Character.ApplyHorizontalMovement(this.GetHorizontalMovement());
		this.Character.ApplyVerticalMovement(this.GetVerticalMovement());
		this.Character.MoveAndSlide();
	}

	private void CheckExitTransitions()
	{
		if (this.DurationActiveMs >= this.MaxDurationSec * 1000) {
			if (this.StateTransitionWhenMaxDurationReached != null) {
				this.StateMachine.Transition(this.StateTransitionWhenMaxDurationReached.Name);
			} else {
				this.StateMachine.Reset();
			}
		} else if (this.StateTransitionWhenOnFloor != null && this.Character.IsOnFloor()) {
			this.StateMachine.Transition(this.StateTransitionWhenOnFloor.Name);
		} else if (this.StateTransitionWhenOnWall != null && this.Character.IsOnWall()) {
			this.StateMachine.Transition(this.StateTransitionWhenOnWall.Name);
		} else if (this.StateTransitionWhenNotOnFloor != null && !this.Character.IsOnFloor()) {
			this.StateMachine.Transition(this.StateTransitionWhenNotOnFloor.Name);
		} else if (this.StateTransitionWhenNotOnWall != null && !this.Character.IsOnWall()) {
			this.StateMachine.Transition(this.StateTransitionWhenNotOnWall.Name);
		}
	}

	public virtual void Transition()
		=> this.StateMachine.Transition(this.Name);

	public virtual HorizontalMovement GetHorizontalMovement() => new();
	public virtual VerticalMovement GetVerticalMovement() => new();
}
