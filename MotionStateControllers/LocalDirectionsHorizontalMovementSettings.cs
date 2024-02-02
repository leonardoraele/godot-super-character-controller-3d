using Godot;

namespace Raele.SuperCharacter3D.MotionStateControllers;

public partial class LocalDirectionsHorizontalMovementSettings : Resource
{
	[Export] public ForwardMovementSettings ForwardMovement = new();
    [Export] public float MaxBackwardSpeedUnPSec = 3;
	[Export] public float BackwardAccelerationUnPSecSqr = 10;

	[Export] public float TurnSpeedDegPSec = 270;
    public float TurnSpeedRadPSec {
		get => Mathf.DegToRad(this.TurnSpeedDegPSec);
		set => this.TurnSpeedDegPSec = Mathf.RadToDeg(value);
	}
}
