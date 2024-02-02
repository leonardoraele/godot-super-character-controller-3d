using Godot;

namespace Raele.SuperCharacter3D.MotionStateControllers;

public partial class FixedDirectionHorizontalMovementSettings : Resource
{
	[Export] public ForwardMovementSettings ForwardMovement = new();

	[Export] public float MaxSidewaySpeedUnPSec = 6;
	[Export] public float MaxBackwardSpeedUnPSec = 4;
	[Export] public float TurnSpeedDegPSec = 720;

	public float TurnSpeedRadPSec {
		get => Mathf.DegToRad(this.TurnSpeedDegPSec);
		set => this.TurnSpeedDegPSec = Mathf.RadToDeg(value);
	}
}
