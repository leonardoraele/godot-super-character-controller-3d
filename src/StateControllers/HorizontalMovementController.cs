using System;
using Godot;

namespace Raele.Supercon2D.StateControllers;

public partial class HorizontalMovementController : StateController
{
	// -----------------------------------------------------------------------------------------------------------------
	// LOCAL TYPES
	// -----------------------------------------------------------------------------------------------------------------

	public enum AutomaticMovementDirection : byte
	{
		None = 0,
		Left = 1,
		Right = 2
	}

	// -----------------------------------------------------------------------------------------------------------------
	// EXPORTS
	// -----------------------------------------------------------------------------------------------------------------

	[Export] public float MaxSpeedPxPSec { get; set; } = 600f;
	[Export] public float AccelerationPxPSecSqr { get; set; } = 1200f;
	/// <summary>
	/// Deceleration when no input is given.
	/// </summary>
	[Export] public float SoftDecelerationPxPSecSqr { get; set; } = 1200f;
	/// <summary>
	/// Deceleration when current input is lower than the current speed or opposite to current movement direction.
	/// </summary>
	[Export] public float HardDecelerationPxPSecSqr { get; set; } = 2400f;
	/// <summary>
	/// If true, this controller will also adjust the vertical velocity to follow slopes when on the ground. If false,
	/// vertical velocity won't be modified.
	/// </summary>
	[Export] public bool FollowSlopes { get; set; } = true;

	[ExportGroup("Options")]
	/// <summary>
	/// Which directions the player is allowed to move the character. If either is disabled, the player cannot move the
	/// character to that direction.
	/// </summary>
	// [Export(PropertyHint.Flags, "1:Left,2:Right")] public byte Direction = 3;
	/// <summary>
	/// If either direction is set, the character will move automatically to that direction as if the player was
	/// constantly inputting the directional movement to that direction. Actual player input is ignored in this case.
	/// </summary>
	[Export] public AutomaticMovementDirection AutomaticMovement = AutomaticMovementDirection.None;
	/// <summary>
	/// If true, inverts the direction of movement.
	/// (useful to implement things like status effects that reverse controls)
	/// </summary>
	[Export] public bool InvertDirections = false;

	// -----------------------------------------------------------------------------------------------------------------
	// PROPERTIES
	// -----------------------------------------------------------------------------------------------------------------

	private Vector2 ResolvedInput
		=> this.AutomaticMovement switch
			{
				AutomaticMovementDirection.Left => Vector2.Left,
				AutomaticMovementDirection.Right => Vector2.Right,
				_ => this.Character.InputManager.MovementInput
			}
			* (this.InvertDirections ? -1 : 1);

	// -----------------------------------------------------------------------------------------------------------------
	// PUBLIC METHODS
	// -----------------------------------------------------------------------------------------------------------------

	public override void _PhysicsProcessActive(double delta)
	{
		base._PhysicsProcessActive(delta);
		this.CalculateHorizontalPhysics((float)delta);
		this.CalculateVerticalPhysics();
	}

	private void CalculateHorizontalPhysics(float delta)
	{
		float targetVelocityX = this.MaxSpeedPxPSec * this.ResolvedInput.X;
		float accelerationX =
			Math.Abs(this.Character.Velocity.X) < float.Epsilon
			|| Math.Abs(targetVelocityX) > Math.Abs(this.Character.Velocity.X)
				&& Math.Sign(targetVelocityX) == Math.Sign(this.Character.Velocity.X)
				? this.AccelerationPxPSecSqr * delta
			: Math.Abs(this.ResolvedInput.X) < float.Epsilon ? this.SoftDecelerationPxPSecSqr * delta
			: this.HardDecelerationPxPSecSqr * delta;
		this.Character.AccelerateX(targetVelocityX, accelerationX);
	}

	private void CalculateVerticalPhysics()
	{
		if (this.FollowSlopes)
		{
			if (this.Character.IsOnSlope)
			{
				this.Character.Velocity = (Vector2.Right * this.Character.Velocity).Rotated(this.Character.GetFloorAngle());
				// this.Character.VelocityY = this.Character.MovementSettings.DownwardVelocityOnFloor;
			} else if (this.Character.IsOnFloor()) {
				this.Character.VelocityY = 0;
			}
		}
	}
}
