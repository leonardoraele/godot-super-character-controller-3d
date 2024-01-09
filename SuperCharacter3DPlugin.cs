#if TOOLS
using Godot;

namespace Raele.SuperCharacter3D;

[Tool]
public partial class SuperCharacter3DPlugin : EditorPlugin
{
    public override void _EnterTree()
    {
        Texture2D icon = GD.Load<Texture2D>($"res://addons/{nameof(SuperCharacter3D)}/icon.svg");
        Script controller = GD.Load<Script>($"res://addons/{nameof(SuperCharacter3D)}/{nameof(SuperCharacter3DController)}.cs");
        Script debugPanel = GD.Load<Script>($"res://addons/{nameof(SuperCharacter3D)}/{nameof(Debug.SuperCharacter3DDebugPanel)}.cs");
        this.AddCustomType(nameof(SuperCharacter3DController), nameof(CharacterBody3D), controller, icon);
        this.AddCustomType(nameof(Debug.SuperCharacter3DDebugPanel), nameof(ColorRect), debugPanel, icon);
    }
}
#endif
