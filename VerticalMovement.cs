namespace Raele.SuperCharacter3D;

public struct VerticalMovement {
    public float TargetVerticalSpeed = 0;
	public float Acceleration = float.PositiveInfinity;
    public bool SnapToFloor = false;
    public VerticalMovement() {}

    public override string ToString() {
        return $"VerticalMovement (toward {this.TargetVerticalSpeed}un/s at {this.Acceleration}un/s² )";
    }

    public static VerticalMovement operator+(VerticalMovement a, VerticalMovement b)
        => new() {
            TargetVerticalSpeed = a.TargetVerticalSpeed + b.TargetVerticalSpeed,
            Acceleration = a.Acceleration + b.Acceleration,
            SnapToFloor = a.SnapToFloor || b.SnapToFloor,
        };

    public static VerticalMovement operator*(VerticalMovement a, float b)
        => new() {
            TargetVerticalSpeed = a.TargetVerticalSpeed * b,
            Acceleration = a.Acceleration * b,
            SnapToFloor = a.SnapToFloor,
        };

    public static VerticalMovement operator/(VerticalMovement a, float b)
        => b == 0
            ? new() {
                TargetVerticalSpeed = float.PositiveInfinity,
                Acceleration = float.PositiveInfinity,
                SnapToFloor = a.SnapToFloor,
            }
            : new() {
                TargetVerticalSpeed = a.TargetVerticalSpeed / b,
                Acceleration = a.Acceleration / b,
                SnapToFloor = a.SnapToFloor,
            };
}
