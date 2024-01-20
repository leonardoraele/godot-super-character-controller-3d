using Godot;

namespace Raele.SuperCharacter3D.MotionStates;

public abstract partial class BaseMotionState : Node, IMotionState
{
	[ExportGroup("Abilities")]
	[Export] public Godot.Collections.Array<AbilityData>? RechargedAbilitiesOnEnter;
	[ExportGroup("Collision Shape")]
	[Export] public CollisionShape3D? ActiveCollisionShape;
	[Export] public bool DisableSiblingCollisionShapes;

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
	public virtual void OnProcessStateActive(float delta) {}
	public virtual void OnPhysicsProcessStateActive(float delta)
	{
		// if (this.HorizontalControl != null) this.Character.ApplyHorizontalMovement(this.HorizontalControl.GetHorizontalMovement());
		// if (this.VerticalControl != null) this.Character.ApplyVerticalMovement(this.VerticalControl.GetVerticalMovement());
		// this.Character.MoveAndSlide();
		this.Character.ApplyHorizontalMovement(this.GetHorizontalMovement());
		this.Character.ApplyVerticalMovement(this.GetVerticalMovement());
		this.Character.MoveAndSlide();
	}

	public virtual void Transition()
		=> this.StateMachine.Transition(this.Name);

	public virtual HorizontalMovement GetHorizontalMovement() => new();
	public virtual VerticalMovement GetVerticalMovement() => new();
}
