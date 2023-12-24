#if TOOLS
using Godot;

namespace Raele.SuperCharacterController3D;

[Tool]
public partial class SuperCharacterController3DPlugin : EditorPlugin
{
    public override void _EnterTree()
    {
        this.AddDebuggerPlugin(new EditorDebuggerPlugin());

        Texture2D icon = GD.Load<Texture2D>($"res://addons/{nameof(Raele.SuperCharacterController3D)}/icon.svg");
        Script script = GD.Load<Script>($"res://addons/{nameof(Raele.SuperCharacterController3D)}/{nameof(SuperCharacterController3D)}.cs");
        this.AddCustomType(nameof(SuperCharacterController3D), nameof(CharacterBody3D), script, icon);
    }
}
#endif
