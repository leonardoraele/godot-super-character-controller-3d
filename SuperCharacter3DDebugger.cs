#nullable enable
using System.Linq;
using Godot;

namespace Raele.SuperCharacter3D;

public partial class SuperCharacter3DDebugger : Node
{
	[ExportCategory("Debug Drawings")]
	[Export(PropertyHint.Flags, "Draw Velocity:1,Draw Input:2")] public int DebugDraw;
	[ExportGroup("State Machine - State Transition Shadows")]
	[Export] private MotionStateMachine? StateMachine;
	[Export] private float ShadowDurationSec = 30;
	[ExportCategory("Frame-by-Frame Step Debugger")]
	[Export] private string InputActionPause = "debug_pause";
	[Export] private string InputActionStep = "debug_step";

	private SuperCharacter3DController Character = null!;
    private int ProcessFrames = -1;

	private bool ShouldDrawVelocity => (this.DebugDraw & 1) != 0;
	private bool ShouldDrawInput => (this.DebugDraw & 2) != 0;

    public override void _EnterTree()
    {
        base._EnterTree();
		if (!(this.GetParent() is SuperCharacter3DController character)) {
			GD.PushWarning(nameof(SuperCharacter3DDebugger), "must be a child of", nameof(SuperCharacter3DController));
			this.QueueFree();
			return;
		}
		this.Character = character;
    }

    public override void _Ready()
	{
		if (this.StateMachine != null) {
			this.StateMachine.StateChanged += this.OnStateChanged;
		}
		this.ProcessMode = ProcessModeEnum.Always;
	}

	private void OnStateChanged(string newState, string oldState, Variant data) {
        var meshes = this.Character.GetChildren()
			.Select(child => child is MeshInstance3D mesh ? mesh : null)
			.Where(mesh => mesh != null)
			.Select(mesh => mesh!);
		foreach (var mesh in meshes) {
            MeshInstance3D shadow = new() {
                Mesh = mesh.Mesh,
                MaterialOverride = new StandardMaterial3D {
					AlbedoColor = Color.FromHsv((float) newState.Hash() / uint.MaxValue, 1, 1) with { A = 0.5f },
					Transparency = BaseMaterial3D.TransparencyEnum.Alpha,
				},
				Position = this.Character.Position + mesh.Position,
            };
			Label3D text = new() {
				Text = $"{newState} @({this.Character.Position.X:n2}, {this.Character.Position.Y:n2}, {this.Character.Position.Z:n2}):{Time.GetTicksMsec()}",
				Billboard = BaseMaterial3D.BillboardModeEnum.Enabled,
				Position = shadow.Position + Vector3.Up,
			};
			// Use a Godot timer to dispose of the objects instead of a C# timer so that it syncs with the engine
			// timescale and can be paused for debugging.
			Timer timer = new() {
				OneShot = true,
				WaitTime = this.ShadowDurationSec,
				Autostart = true,
			};
			timer.Timeout += () => {
				shadow.QueueFree();
				text.QueueFree();
				timer.QueueFree();
			};
            this.Character.AddSibling(shadow);
			this.Character.AddSibling(text);
			this.Character.AddSibling(timer);
		}
	}

    public override void _Process(double delta)
    {
        base._Process(delta);
		this.ProcessMode = ProcessModeEnum.Always;

		// Frame-by-frame step debugger
		if (this.ProcessFrames == 0) {
			this.GetViewport().ProcessMode = ProcessModeEnum.Disabled;
		} else {
			this.GetViewport().ProcessMode = ProcessModeEnum.Always;
			if (this.ProcessFrames > 0) {
				this.ProcessFrames--;
			}
		}
		// TODO Implement an action to step physics frames instead of logic frames
		if (InputMap.HasAction(this.InputActionPause) && Input.IsActionJustPressed(this.InputActionPause)) {
			this.ProcessFrames = this.ProcessFrames >= 0 ? -1 : 0;
		} else if (InputMap.HasAction(this.InputActionStep) && Input.IsActionJustPressed(this.InputActionStep)) {
			this.ProcessFrames = 1;
		}

		// Debug drawings
		// TODO DebugDraw3D might not be present in the project
		// if (this.ShouldDrawVelocity) {
		// 	DebugDraw3D.DrawArrow(
		// 		this.Character.GlobalPosition,
		// 		this.Character.GlobalPosition + this.Character.Velocity,
		// 		Colors.Red
		// 	);
		// }
		// if (this.ShouldDrawInput) {
		// 	DebugDraw3D.DrawArrow(
		// 		this.Character.GlobalPosition,
		// 		this.Character.GlobalPosition + this.Character.InputController.GlobalMovementInput,
		// 		Colors.Green
		// 	);
		// }
    }
}
