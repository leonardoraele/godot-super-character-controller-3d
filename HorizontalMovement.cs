using Godot;

namespace Raele.SuperCharacter3D;

public struct HorizontalMovement {
	public float TargetSpeedUnPSec = 0;
	public float AccelerationUnPSecSq = float.PositiveInfinity;
    public Vector2 TargetDirection {
		get => this.TargetRotationRad == float.PositiveInfinity
			? Vector2.Zero
			: Vector2.Up.Rotated(this.TargetRotationRad);
		set => this.TargetRotationRad = value.Length() > 0.01f
			? Vector2.Up.AngleTo(value)
			: float.PositiveInfinity;
	}
	public float TargetRotationRad {
		get => Mathf.DegToRad(this.TargetRotationDeg);
		set => this.TargetRotationDeg = Mathf.RadToDeg(value);
	}
	public float TargetRotationDeg = float.PositiveInfinity;
	public float RotationSpeedDegPSec = float.PositiveInfinity;

    public HorizontalMovement() {}

	public override string ToString() {
		return $"HorizontalMovement (toward {this.TargetRotationDeg}deg at {this.RotationSpeedDegPSec}deg/s; and {this.TargetSpeedUnPSec:n2}un/s at {this.RotationSpeedDegPSec}deg/s, {this.AccelerationUnPSecSq:n2}un/s² )";
	}
}
