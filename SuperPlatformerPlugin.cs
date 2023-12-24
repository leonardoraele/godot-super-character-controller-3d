#if TOOLS
using Godot;

namespace Raele.SuperPlatformer;

[Tool]
public partial class SuperPlatformerPlugin : EditorPlugin
{
    public override void _EnterTree()
    {
        this.AddDebuggerPlugin(new EditorDebuggerPlugin());

        Script script = GD.Load<Script>($"res://addons/{nameof(SuperPlatformer)}/{nameof(SuperPlatformerController)}.cs");
        Texture2D icon = GD.Load<Texture2D>($"res://addons/{nameof(SuperPlatformer)}/icon.svg");
        this.AddCustomType(nameof(SuperPlatformerController), nameof(CharacterBody2D), script, icon);
    }
}
#endif
