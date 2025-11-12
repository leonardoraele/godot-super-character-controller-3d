using Godot;

namespace Raele.Supercon2D.StateControllers;

public partial class GravityController : StateController
{
	// -----------------------------------------------------------------------------------------------------------------
	// EXPORTS
	// -----------------------------------------------------------------------------------------------------------------

	[Export] public float MaxDownwardSpeedPxPSec { get; set; } = 1000f;
	[Export] public float AccelerationPxPSecSqr { get; set; } = 1000f;

	// -----------------------------------------------------------------------------------------------------------------
	// PUBLIC METHODS
	// -----------------------------------------------------------------------------------------------------------------

	public override void _PhysicsProcessActive(double delta)
	{
		base._PhysicsProcessActive(delta);

		if (!this.Character.IsOnFloor())
		{
			float velocityY = this.MaxDownwardSpeedPxPSec;
			float accelerationY = this.AccelerationPxPSecSqr;
			this.Character.AccelerateY(velocityY, accelerationY);
		}
	}
}
