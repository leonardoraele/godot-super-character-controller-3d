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
		this.AddCustomType(nameof(AirDashingState), nameof(Node), GD.Load<Script>($"res://addons/{nameof(SuperCharacterController2D)}/src/MotionStates/{nameof(AirDashingState)}.cs"), null);
		this.AddCustomType(nameof(FallingState), nameof(Node), GD.Load<Script>($"res://addons/{nameof(SuperCharacterController2D)}/src/MotionStates/{nameof(FallingState)}.cs"), null);
	}

	public override void _ExitTree()
	{
		this.RemoveCustomType(nameof(SuperCharacterController2D));
		this.RemoveCustomType(nameof(AirDashingState));
		this.RemoveCustomType(nameof(FallingState));
	}
}
#endif
