using Godot;
using Raele.GodotUtils;

namespace Raele.Supercon2D;

public partial class SuperconState : Raele.GodotUtils.StateMachine.BaseState
{
	// -----------------------------------------------------------------------------------------------------------------
	// FIELDS
	// -----------------------------------------------------------------------------------------------------------------

	public SuperconBody2D Character => field != null ? field : field = this.RequireAncestor<SuperconBody2D>();

	// -----------------------------------------------------------------------------------------------------------------
	// PROPERTIES
	// -----------------------------------------------------------------------------------------------------------------

	public SuperconInputManager InputManager => this.Character.InputManager;

	// -----------------------------------------------------------------------------------------------------------------
	// GODOT EVENTS
	// -----------------------------------------------------------------------------------------------------------------

	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);
		Callable.From(this.Character.MoveAndSlide).CallDeferred();
	}
}
