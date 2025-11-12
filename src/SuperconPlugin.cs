#if TOOLS
using Godot;
using Raele.Supercon2D.StateControllers;

namespace Raele.Supercon2D;

[Tool]
public partial class SuperconPlugin : EditorPlugin
{
	public override void _EnterTree()
	{
		this.AddDebuggerPlugin(new EditorDebuggerPlugin());

		this.AddCustomType(nameof(SuperconBody2D), nameof(CharacterBody2D), GD.Load<Script>($"res://addons/{nameof(Supercon2D)}/src/{nameof(SuperconBody2D)}.cs"), null);
		this.AddCustomType(nameof(SuperconStateMachine), nameof(Node), GD.Load<Script>($"res://addons/{nameof(Supercon2D)}/src/{nameof(SuperconStateMachine)}.cs"), null);
		this.AddCustomType(nameof(SuperconState), nameof(Node), GD.Load<Script>($"res://addons/{nameof(Supercon2D)}/src/{nameof(SuperconState)}.cs"), null);
		this.AddCustomType(nameof(SuperconInputManager), nameof(Node), GD.Load<Script>($"res://addons/{nameof(Supercon2D)}/src/{nameof(SuperconInputManager)}.cs"), null);

		// State Controllers
		this.AddCustomType(nameof(HorizontalMovementController), nameof(Node), GD.Load<Script>($"res://addons/{nameof(Supercon2D)}/src/{nameof(StateControllers)}/{nameof(HorizontalMovementController)}.cs"), null);
		this.AddCustomType(nameof(GravityController), nameof(Node), GD.Load<Script>($"res://addons/{nameof(Supercon2D)}/src/{nameof(StateControllers)}/{nameof(GravityController)}.cs"), null);
		this.AddCustomType(nameof(InputActionTransition), nameof(Node), GD.Load<Script>($"res://addons/{nameof(Supercon2D)}/src/{nameof(StateControllers)}/{nameof(InputActionTransition)}.cs"), null);
	}

	public override void _ExitTree()
	{
		this.RemoveCustomType(nameof(SuperconBody2D));
		this.RemoveCustomType(nameof(SuperconStateMachine));
		this.RemoveCustomType(nameof(SuperconState));
		this.RemoveCustomType(nameof(SuperconInputManager));

		this.RemoveCustomType(nameof(HorizontalMovementController));
		this.RemoveCustomType(nameof(GravityController));
		this.RemoveCustomType(nameof(InputActionTransition));
	}
}
#endif
