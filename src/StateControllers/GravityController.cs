using System;
using Godot;

namespace Raele.SuperCharacterController2D.StateControllers;

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
			float velocityY = this.Character.JumpSettings.MaxFallSpeedPxPSec;
			float accelerationY = this.Character.JumpSettings.FallAccelerationPxPSecSq;
			this.Character.AccelerateY(velocityY, accelerationY);
		}
	}
}
