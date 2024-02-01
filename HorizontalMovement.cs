using Godot;

namespace Raele.SuperCharacter3D;

public struct HorizontalMovement {
	public float TargetForwardSpeedUnPSec = 0;
	public float ForwardAccelerationUnPSecSq = float.PositiveInfinity;
	public float TargetSidewaySpeedUnPSec = 0;
	public float SidewayAccelerationUnPSecSq = float.PositiveInfinity;
	public float TargetRotationDeg = float.PositiveInfinity;
	public float RotationSpeedDegPSec = float.PositiveInfinity;

	public float TargetRotationRad {
		get => Mathf.DegToRad(this.TargetRotationDeg);
		set => this.TargetRotationDeg = Mathf.RadToDeg(value);
	}
    public Vector2 TargetDirection {
		get => this.TargetRotationRad == float.PositiveInfinity
			? Vector2.Zero
			: Vector2.Up.Rotated(this.TargetRotationRad);
		set => this.TargetRotationRad = value.Length() > 0.01f
			? Vector2.Up.AngleTo(value)
			: float.PositiveInfinity;
	}

    public HorizontalMovement() {}

	public override string ToString() {
		return $"HorizontalMovement (toward {this.TargetRotationDeg}deg at {this.RotationSpeedDegPSec}deg/s; and {this.TargetForwardSpeedUnPSec:n2}un/s at {this.RotationSpeedDegPSec}deg/s, {this.ForwardAccelerationUnPSecSq:n2}un/s² )";
	}

	// public static HorizontalMovement operator+(HorizontalMovement a, HorizontalMovement b)
	// 	=> new() {
	// 		TargetSpeedUnPSec = a.TargetSpeedUnPSec + b.TargetSpeedUnPSec,
	// 		AccelerationUnPSecSq = a.AccelerationUnPSecSq + b.AccelerationUnPSecSq,
	// 		TargetRotationDeg = a.TargetRotationDeg + b.TargetRotationDeg,
	// 		RotationSpeedDegPSec = a.RotationSpeedDegPSec + b.RotationSpeedDegPSec,
	// 	};

	// public static HorizontalMovement operator*(HorizontalMovement a, float b)
	// 	=> new() {
	// 		TargetSpeedUnPSec = a.TargetSpeedUnPSec * b,
	// 		AccelerationUnPSecSq = a.AccelerationUnPSecSq * b,
	// 		TargetRotationDeg = a.TargetRotationDeg * b,
	// 		RotationSpeedDegPSec = a.RotationSpeedDegPSec * b,
	// 	};

	// public static HorizontalMovement operator/(HorizontalMovement a, float b)
	// 	=> b == 0
	// 		? new() {
	// 			TargetSpeedUnPSec = float.PositiveInfinity,
	// 			AccelerationUnPSecSq = float.PositiveInfinity,
	// 			TargetRotationDeg = float.PositiveInfinity,
	// 			RotationSpeedDegPSec = float.PositiveInfinity,
	// 		}
	// 		: new() {
	// 			TargetSpeedUnPSec = a.TargetSpeedUnPSec / b,
	// 			AccelerationUnPSecSq = a.AccelerationUnPSecSq / b,
	// 			TargetRotationDeg = a.TargetRotationDeg / b,
	// 			RotationSpeedDegPSec = a.RotationSpeedDegPSec / b,
	// 		};
}
