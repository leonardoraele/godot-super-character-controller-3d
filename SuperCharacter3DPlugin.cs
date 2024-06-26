#nullable enable
#if TOOLS
using Godot;
using Raele.SuperCharacter3D.MotionStateControllers;

namespace Raele.SuperCharacter3D;

[Tool]
public partial class SuperCharacter3DPlugin : EditorPlugin
{
    public override void _EnterTree()
    {
        Texture2D icon = ImageTexture.CreateFromImage(Image.Create(16, 16, false, Image.Format.L8));
        this.AddCustomType(
            nameof(SuperCharacter3DController),
            nameof(CharacterBody3D),
            GD.Load<Script>($"res://addons/{nameof(SuperCharacter3D)}/{nameof(SuperCharacter3DController)}.cs"),
            icon
        );
        this.AddCustomType(
            nameof(MotionStateMachine),
            nameof(Node),
            GD.Load<Script>($"res://addons/{nameof(SuperCharacter3D)}/{nameof(MotionStateMachine)}.cs"),
            icon
        );
        this.AddCustomType(
            nameof(SuperCharacter3DDebugger),
            nameof(Node),
            GD.Load<Script>($"res://addons/{nameof(SuperCharacter3D)}/{nameof(SuperCharacter3DDebugger)}.cs"),
            icon
        );
        this.AddCustomType(
            nameof(ControlledState),
            nameof(Node),
            GD.Load<Script>($"res://addons/{nameof(SuperCharacter3D)}/MotionStates/{nameof(ControlledState)}.cs"),
            icon
        );

        // Motion state controllers
        string[] states = new string[] {
            // Velocity controllers
            nameof(BasicMovementController),
            nameof(DirectionalMovementController),
            nameof(TankMovementController),
            nameof(GravityController),
            nameof(JumpController),

            // Misc controllers
            nameof(CollisionShapeEnablingController),
            nameof(FloorSnapController),

            // State transitions
            nameof(InputActionController),
            nameof(OnFloorEventEmitter),
            nameof(OnWallEventEmitter),
            nameof(OnCeilingEventEmitter),
            nameof(TimeoutEventEmitter),
        };
        foreach (string stateName in states) {
            Script script = GD.Load<Script>($"res://addons/{nameof(SuperCharacter3D)}/{nameof(MotionStateControllers)}/{stateName}.cs");
            this.AddCustomType(stateName, nameof(Node), script, icon);
        }
    }
}
#endif
