using Godot;
using Raele.GodotUtils;

namespace Raele.Supercon2D.StateControllers;

public partial class StateController : Node
{
	// -----------------------------------------------------------------------------------------------------------------
	// PROPERTIES
	// -----------------------------------------------------------------------------------------------------------------

	public SuperconState State => field != null ? field : field = this.RequireAncestor<SuperconState>();
	public SuperconBody2D Character => this.State.Character;
	public SuperconInputManager InputManager => this.Character.InputManager;
	public SuperconStateMachine StateMachine => this.Character.StateMachine;

	// -----------------------------------------------------------------------------------------------------------------
	// GODOT EVENTS
	// -----------------------------------------------------------------------------------------------------------------

	public override void _EnterTree()
	{
		base._EnterTree();
		this.State.StateEntered += this._EnterState;
		this.State.StateExited += this._ExitState;
	}

	public override void _Process(double delta)
	{
		base._Process(delta);
		if (this.State.IsActive == true)
		{
			this._ProcessActive(delta);
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);
		if (this.State?.IsActive == true)
		{
			this._PhysicsProcessActive(delta);
		}
	}

	// -----------------------------------------------------------------------------------------------------------------
	// VIRTUAL METHODS
	// -----------------------------------------------------------------------------------------------------------------

	public virtual void _EnterState() { }
	public virtual void _ExitState() { }
	public virtual void _ProcessActive(double delta) { }
	public virtual void _PhysicsProcessActive(double delta) {}

}
