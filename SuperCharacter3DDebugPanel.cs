using Godot;

namespace Raele.SuperCharacter3D.Debug;

public partial class SuperCharacter3DDebugPanel : ColorRect
{
	private SuperCharacter3DController Character = null!;

	private VBoxContainer Panel = null!;
    private Label PositionLabel = null!;
    private Meter2D AbsMovementInputGUI = null!;
    private Meter2D RelMovementInputGUI = null!;
    private Meter2D CharacterForwardGUI = null!;

    // private MeshInstance3D MovementInputNode = null!;
    public override void _Ready()
    {
        base._Ready();
		if (this.GetParent() is SuperCharacter3DController character) {
			this.Character = character;
		} else {
			GD.PushError(nameof(SuperCharacter3DDebugPanel), " must be a child of ", nameof(SuperCharacter3DController), ". It will be removed from the scene.");
			this.QueueFree();
			return;
		}
		this.Color = Colors.DarkGray;
		// this.MovementInputNode = new MeshInstance3D() {
		// 	Mesh = new SphereMesh() {
		// 		Material = new StandardMaterial3D() {
		// 			AlbedoColor = Colors.Red,
		// 			DiffuseMode = BaseMaterial3D.DiffuseModeEnum.Toon,
		// 			SpecularMode = BaseMaterial3D.SpecularModeEnum.Toon,
		// 		}
		// 	}
		// };
		this.AddChild(this.Panel = new());
		this.Panel.AddChild(this.PositionLabel = new());
		this.Panel.AddChild(this.AbsMovementInputGUI = new() {
			Label = "AbsMvInput",
			UpDirection = Vector2.Down,
		});
		this.Panel.AddChild(this.RelMovementInputGUI = new() {
			Label = "RelMvInput",
			UpDirection = Vector2.Down,
		});
		this.Panel.AddChild(this.CharacterForwardGUI = new() {
			Label = "ActorFwd",
			UpDirection = Vector2.Down,
		});
    }
    public override void _Process(double delta)
    {
        base._Process(delta);
		this.PositionLabel.Text = $"GPos: ({this.Character.GlobalPosition.X:n2}, {this.Character.GlobalPosition.Y:n2}, {this.Character.GlobalPosition.Z:n2})";
		this.AbsMovementInputGUI.Value = this.Character.InputController.MovementInput;
		this.AbsMovementInputGUI.Position = this.PositionLabel.Position + Vector2.Down * (this.PositionLabel.Size.Y + 4);
		this.RelMovementInputGUI.Value = this.Character.InputController.MovementInput.Rotated(this.Character.GetViewport().GetCamera3D().Rotation.Y * -1);
		this.RelMovementInputGUI.Position = this.AbsMovementInputGUI.Position + Vector2.Down * (32 + 4);
		this.CharacterForwardGUI.Value = new Vector2(this.Character.GlobalTransform.Basis.Z.X * -1, this.Character.GlobalTransform.Basis.Z.Z * -1);
		this.CharacterForwardGUI.Position = this.RelMovementInputGUI.Position + Vector2.Down * (32 + 4);
    }
}
