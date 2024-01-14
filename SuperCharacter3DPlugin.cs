#if TOOLS
using Godot;
using Raele.SuperCharacter3D.MotionStates;

namespace Raele.SuperCharacter3D;

[Tool]
public partial class SuperCharacter3DPlugin : EditorPlugin
{
    public override void _EnterTree()
    {
        Texture2D icon = GD.Load<Texture2D>($"res://addons/{nameof(SuperCharacter3D)}/icon.svg");
        Script controller = GD.Load<Script>($"res://addons/{nameof(SuperCharacter3D)}/{nameof(SuperCharacter3DController)}.cs");
        Script stateMachine = GD.Load<Script>($"res://addons/{nameof(SuperCharacter3D)}/{nameof(MotionStateMachine)}.cs");
        Script debugPanel = GD.Load<Script>($"res://addons/{nameof(SuperCharacter3D)}/{nameof(Debug.SuperCharacter3DDebugPanel)}.cs");
        this.AddCustomType(nameof(SuperCharacter3DController), nameof(CharacterBody3D), controller, icon);
        this.AddCustomType(nameof(MotionStateMachine), nameof(Node), stateMachine, icon);
        this.AddCustomType(nameof(Debug.SuperCharacter3DDebugPanel), nameof(ColorRect), debugPanel, icon);

        // Motion state nodes
        string[] states = new string[] {
            nameof(AirDashingState),
            nameof(CrouchState),
            nameof(FallingState),
            nameof(GroundDashingState),
            nameof(InteractingState),
            nameof(JumpCanceledState),
            nameof(JumpingState),
            nameof(OnFootState),
            nameof(SlideState),
            nameof(WallClimbingState),
            nameof(WallSlidingState),
        };
        foreach (string stateName in states) {
            Script script = GD.Load<Script>($"res://addons/{nameof(SuperCharacter3D)}/{nameof(MotionStates)}/{stateName}.cs");
            this.AddCustomType(stateName, nameof(Node), script, icon);
        }
    }
}
#endif
