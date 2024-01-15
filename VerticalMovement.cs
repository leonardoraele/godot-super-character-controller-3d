namespace Raele.SuperCharacter3D;

public struct VerticalMovement {
    public float Speed { get; init; } = 0;
	public float Acceleration { get; init; } = float.PositiveInfinity;
    public bool SnapToFloor { get; init; } = false;
    public VerticalMovement() {}
}
