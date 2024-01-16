namespace Raele.SuperCharacter3D;

public struct VerticalMovement {
    public float TargetVerticalSpeed = 0;
	public float Acceleration = float.PositiveInfinity;
    public bool SnapToFloor = false;
    public VerticalMovement() {}
}
