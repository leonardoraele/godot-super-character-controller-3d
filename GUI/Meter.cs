#nullable enable
using System;
using Godot;

namespace Raele.SuperCharacter3D.Debug;

public partial class Meter : ColorRect {
	[Export] public float Value;
	[Export] public float UpperLimit;
	[Export] public float LowerLimit;
	[Export] public OrientationEnum Orientation = OrientationEnum.Horizontal;

	private ColorRect FillValueRect = null!;
	private ColorRect ZeroMarker = null!;
	private Label Label = null!;
	private Color ValueColor;
	public float RawFillValue => this.UpperLimit > this.LowerLimit
		? (this.Value - this.LowerLimit) / (this.UpperLimit - this.LowerLimit)
		: 1;
	public float ClampedFillValue => Math.Clamp(this.RawFillValue, 0, 1);
	private float ZeroMarkerPosition => this.UpperLimit > this.LowerLimit
		? (-this.LowerLimit) / (this.UpperLimit - this.LowerLimit)
		: 0;

	public enum OrientationEnum
	{
		Horizontal,
		Vertical,
	}

	public override void _Ready() {
		base._Ready();
		this.ValueColor = this.Color;
		this.AddChild(this.FillValueRect = new());
		this.AddChild(this.ZeroMarker = new());
		this.ZeroMarker.Color = Colors.White;
		this.AddChild(this.Label = new());
		this.Label.AddThemeColorOverride("font_color", Colors.Black);
	}

	public override void _Process(double delta) {
		base._Process(delta);
		this.UpperLimit = Mathf.Max(this.UpperLimit, this.LowerLimit);
		this.FillValueRect.Size = this.Orientation == OrientationEnum.Horizontal
			? new Vector2(this.Size.X * this.ClampedFillValue, this.Size.Y)
			: new Vector2(this.Size.X, this.Size.Y * this.ClampedFillValue);
		this.FillValueRect.Position = this.Orientation == OrientationEnum.Horizontal
			? new Vector2(0, 0)
			: new Vector2(0, this.Size.Y - this.FillValueRect.Size.Y);
		this.ZeroMarker.Size = this.Orientation == OrientationEnum.Horizontal
			? new Vector2(1, this.Size.Y)
			: new Vector2(this.Size.X, 1);
		this.ZeroMarker.Position = this.Orientation == OrientationEnum.Horizontal
			? new Vector2(this.Size.X * this.ZeroMarkerPosition, 0)
			: new Vector2(0, this.Size.Y * this.ZeroMarkerPosition);
		this.FillValueRect.Color = this.RawFillValue > 1 ? Colors.Red : this.ValueColor;
		this.Color = this.RawFillValue < 0 ? Colors.Yellow : Colors.Gray;
		this.Label.Text = $"{this.Value}";
	}
}
