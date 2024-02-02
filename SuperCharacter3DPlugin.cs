#if TOOLS
using Godot;

namespace Raele.SuperCharacter3D;

[Tool]
public partial class SuperCharacter3DPlugin : EditorPlugin
{
    public override void _EnterTree()
    {
        Texture2D icon = ImageTexture.CreateFromImage(Image.Create(16, 16, false, Image.Format.L8));
        Script controller = GD.Load<Script>($"res://addons/{nameof(SuperCharacter3D)}/{nameof(SuperCharacter3DController)}.cs");
        Script stateMachine = GD.Load<Script>($"res://addons/{nameof(SuperCharacter3D)}/{nameof(MotionStateMachine)}.cs");
        this.AddCustomType(nameof(SuperCharacter3DController), nameof(CharacterBody3D), controller, icon);
        this.AddCustomType(nameof(MotionStateMachine), nameof(Node), stateMachine, icon);

        // Motion state nodes
        string[] states = new string[] {
            nameof(ControlledState),
        };
        foreach (string stateName in states) {
            Script script = GD.Load<Script>($"res://addons/{nameof(SuperCharacter3D)}/MotionStates/{stateName}.cs");
            this.AddCustomType(stateName, nameof(Node), script, icon);
        }
    }
}
#endif
