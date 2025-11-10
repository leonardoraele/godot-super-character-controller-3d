using System;
using Godot;

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

	public SuperCharacterController2D? Character => this.ParentCache as SuperCharacterController2D;

	private Node? ParentCache;
	private ulong ActivationTimeMs;

	// -----------------------------------------------------------------------------------------------------------------
	// PROPERTIES
	// -----------------------------------------------------------------------------------------------------------------

	public bool IsActive => this.ActivationTimeMs > 0;
	public TimeSpan ActiveDuration => this.IsActive
		? TimeSpan.FromMilliseconds(Time.GetTicksMsec() - this.ActivationTimeMs)
		: TimeSpan.Zero;

	// -----------------------------------------------------------------------------------------------------------------
	// GODOT EVENTS
	// -----------------------------------------------------------------------------------------------------------------

	public override void _Notification(int what)
	{
		base._Notification(what);
		if (what == NotificationEnterTree)
		{
			this.ParentCache = this.GetParent();
		}
		else if (what == NotificationExitTree)
		{
			this.ParentCache = null;
		}
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
