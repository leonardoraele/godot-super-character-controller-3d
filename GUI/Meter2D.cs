#nullable enable
using System;
using Godot;

namespace Raele.SuperCharacter3D.Debug;

[Tool]
public partial class Meter2D : Control {
	[Export] public Vector2 Value;
	[Export] public float UpperLimit = 1;
	[Export] public float LowerLimit = 0;
	[Export] public bool IsNormalized = true;
	[Export] public string Label = "";
	[Export] public Vector2 UpDirection = Vector2.Up;

	private Label LabelControl = null!;

	private Vector2 ClampedValue => this.Value.Abs().Length() > 0.01f
		? Math.Clamp(this.Value.Length(), this.LowerLimit, this.UpperLimit) * this.Value.Normalized()
		: Vector2.Zero;

	public enum OrientationEnum
	{
		Horizontal,
		Vertical,
	}

	public override void _Ready() {
		base._Ready();
		this.AddChild(this.LabelControl = new());
	}

    public override void _Draw()
    {
        base._Draw();

		bool isValid = this.Value.Length() >= this.LowerLimit - 0.01f
			&& this.Value.Length() <= this.UpperLimit + 0.01f;

		// Background
		Color bgColor = isValid || Time.GetTicksMsec() % 1000 < 500 ? Colors.Gray : Colors.Red;
		if (this.IsNormalized) {
			this.DrawCircle(Vector2.One * 16, 16, bgColor);
		} else {
			this.DrawRect(new Rect2(Vector2.Zero, 32, 32), bgColor);
		}

		// Draw vector
		this.DrawLine(Vector2.One * 16, Vector2.One * 16 + Vector2.Right * this.ClampedValue * 16, Colors.Red);
		this.DrawLine(Vector2.One * 16, Vector2.One * 16 + Vector2.Down * this.ClampedValue * 16 * this.UpDirection, Colors.Green);
		this.DrawLine(Vector2.One * 16, Vector2.One * 16 + this.ClampedValue * 16 * (this.UpDirection + Vector2.Right), Colors.White);
    }

	public override void _Process(double delta) {
		base._Process(delta);
		this.LabelControl.Position = new Vector2(32 + 4, 0);
		this.LabelControl.Text = $"{Label}: ({this.Value.X:n2}, {this.Value.Y:n2})";
		this.QueueRedraw();
	}
}
