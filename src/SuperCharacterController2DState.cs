using System;
using Godot;
using Raele.GodotUtils;

namespace Raele.SuperCharacterController2D;

public partial class SuperCharacterController2DState : Node
{
	// -----------------------------------------------------------------------------------------------------------------
	// SIGNALS
	// -----------------------------------------------------------------------------------------------------------------

	[Signal] public delegate void StateEnteredEventHandler();
	[Signal] public delegate void StateExitedEventHandler();

	// -----------------------------------------------------------------------------------------------------------------
	// FIELDS
	// -----------------------------------------------------------------------------------------------------------------

	public SuperCharacterController2D Character { get; private set; } = null!; // initialized in _EnterTree()
	private ulong ActivationTimeMs;

	// -----------------------------------------------------------------------------------------------------------------
	// PROPERTIES
	// -----------------------------------------------------------------------------------------------------------------

	public bool IsActive => this.ActivationTimeMs > 0;
	public TimeSpan ActiveDuration => this.IsActive
		? TimeSpan.FromMilliseconds(Time.GetTicksMsec() - this.ActivationTimeMs)
		: TimeSpan.Zero;
	public bool IsPreviousActiveState => this.Character.PreviousActiveState == this;

	// -----------------------------------------------------------------------------------------------------------------
	// GODOT EVENTS
	// -----------------------------------------------------------------------------------------------------------------

	public override void _EnterTree()
	{
		base._EnterTree();
		this.Character = this.RequireAncestor<SuperCharacterController2D>();
	}

	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);
		Callable.From(this.Character.MoveAndSlide).CallDeferred();
	}

	// -----------------------------------------------------------------------------------------------------------------
	// VIRTUAL METHODS
	// -----------------------------------------------------------------------------------------------------------------

	public virtual void _EnterState()
	{
		this.ActivationTimeMs = Time.GetTicksMsec();
		this.EmitSignal(SignalName.StateEntered);
	}

	public virtual void _ExitState()
	{
		this.ActivationTimeMs = 0;
		this.EmitSignal(SignalName.StateExited);
	}
}
