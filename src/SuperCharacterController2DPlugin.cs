#if TOOLS
using Godot;

namespace Raele.SuperCharacterController2D;

[Tool]
public partial class SuperCharacterController2DPlugin : EditorPlugin
{
	public override void _EnterTree()
	{
		this.AddDebuggerPlugin(new EditorDebuggerPlugin());
		this.AddCustomType(nameof(SuperCharacterController2D), nameof(CharacterBody2D), GD.Load<Script>($"res://addons/{nameof(SuperCharacterController2D)}/src/{nameof(SuperCharacterController2D)}.cs"), null);
		this.AddCustomType(nameof(SuperCharacterController2DState), nameof(Node), GD.Load<Script>($"res://addons/{nameof(SuperCharacterController2D)}/src/{nameof(SuperCharacterController2DState)}.cs"), null);
	}

	public override void _ExitTree()
	{
		this.RemoveCustomType(nameof(SuperCharacterController2D));
		this.RemoveCustomType(nameof(SuperCharacterController2DState));
	}
}
#endif
