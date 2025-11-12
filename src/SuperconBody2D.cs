using Godot;
using Raele.GodotUtils;
using System;

namespace Raele.Supercon2D;

public partial class SuperconBody2D : CharacterBody2D
{
	// -----------------------------------------------------------------------------------------------------------------
	// EXPORTS
	// -----------------------------------------------------------------------------------------------------------------

	[Obsolete][Export] public Platformer2DInputSettings InputSettings = null!; // TODO
	[Obsolete][Export] public Platformer2DMovementSettings MovementSettings = null!; // TODO
	[Obsolete][Export] public Platformer2DJumpSettings JumpSettings = null!; // TODO
	[Obsolete][Export] public Platformer2DDashSettings? DashSettings; // TODO
	[Obsolete][Export] public Platformer2DWallMotionSettings? WallMotionSettings; // TODO

	// [ExportGroup("Systemic Behavior")]
	// /// Speed applied when the character jumps from a wall if they are pressing the Dash action.
	// /// The sign/direction of each axis is be ignored; only the module is used.
	// /// </summary>
	// [Export] public Vector2 DashWallJumpKickstartSpeedPxPSec { get; private set; } = Vector2.One * 220;

	/// <summary>
	/// Applies some physics force to the character and disables their control for a short duration.
	/// This method is intended to be used when the player character is damaged and becomes disabled for a short
	/// duration. (i.e. like in Spelunky)
	/// </summary>
	/// <param name="force"></param>
	/// <param name="cancelMomentum"></param>
	/// <param name="durationMs"></param>
	// public void DamagePush(Vector2 force, bool cancelMomentum, ulong durationMs) {
	// }

	/// <summary>
	/// Early jump input tolerance time. (if player enters jump input while jump action is not possible, saves the input
	/// for up to this much time to perform the jump later if it becomes possible within this time frame)
	/// </summary>
	[ExportGroup("Assist Options")]
	[Obsolete][Export] public ulong JumpInputBufferSensitivityMs { get; private set; } = 150; // TODO
	/// <summary>
	/// Late jump input tolerance time. (if player enters jump input after jump action is no longer possible, allow the
	/// character to jump anyway as long as it was possible up to this much time before)
	/// </summary>
	[Obsolete][Export] public ulong CoyoteJumpLeniencyMs { get; private set; } = 150; // TODO
	[Obsolete][Export] public ulong DashInputBufferSensitivityMs { get; private set; } = 150; // TODO
	/// <summary>
	/// Max time the character floats below a ceiling when they hit a ceiling during a jump before they start falling.
	/// Player can cancel ceiling slide by releasing the jump button earlier.
	/// </summary>
	[Obsolete][Export] public ulong CeilingSlideTimeMs { get; private set; } = 150; // TODO
	/// <summary>
	/// This property determines how much time, in miliseconds, the player must hold a directional input in the opposite
	/// direction to a wall to let go of that wall during a wall climb or wall slide.
	///
	/// This property is meant to be used as a prevention against accidental input mistakes by making so that the
	/// character will remain stuck to the wall even if the player press a directional input that would normally cause
	/// them to drop off the wall. Instead, the player has to hold the input for a short time to drop.
	///
	/// If this property is set to 0, the player will let go of the wall as soon as the player presses a directional
	/// input in a direction opposite to the wall they are climbing or sliding. They will still remain stuck to the wall
	/// if they don't enter any horizontal directional input.
	///
	/// If this property is set to -1, the character must hold the directional input toward the wall to remain stuck to
	/// it. Releasing the directional input will make the character let go of the wall.
	/// </summary>
	[Obsolete("Use WallClimbDirectionalInputDeadZone instead")]
	[Export] public int WallDropPreventionLeniencyMs { get; private set; } = 150;
	/// <summary>
	/// This property determines the minimum directional input value the player must input in the opposite direction to
	/// a wall to let go of that wall during a wall climb or wall slide.
	///
	/// This property is meant to be used as a prevention against accidental input mistakes by making so that the
	/// character will remain stuck to the wall even if the player accidentaly inputs a small directional input value
	/// that would normally cause them to drop off the wall. Instead, small input values will be ignored.
	///
	/// If this property is set to 0, the player will let go of the wall as soon as the player presses a directional
	/// input in a direction opposite to the wall they are climbing or sliding. They will still remain stuck to the wall
	/// if they don't enter any horizontal directional input.
	///
	/// If this property is set to a negative value, the character must hold that much directional input toward the wall
	/// to remain stuck to it. Releasing the directional input will make the character let go of the wall.
	/// </summary>
	[Export(PropertyHint.Range, "-1,1,0.01")]
	[Obsolete] public float WallClimbDirectionalInputDeadZone { get; private set; } = 0.05f;

	// -----------------------------------------------------------------------------------------------------------------
	// SIGNALS
	// -----------------------------------------------------------------------------------------------------------------

	[Signal] public delegate void StateChangedEventHandler(SuperconState newState, SuperconState? oldState);

	// -----------------------------------------------------------------------------------------------------------------
	// FIELDS
	// -----------------------------------------------------------------------------------------------------------------

	public SuperconStateMachine StateMachine => field != null ? field : field = this.RequireChild<SuperconStateMachine>();
	public SuperconInputManager InputManager => field != null ? field : field = this.RequireChild<SuperconInputManager>();
	public Vector2 LastOnFloorPosition { get; private set; }

	// -----------------------------------------------------------------------------------------------------------------
	// PROPERTIES
	// -----------------------------------------------------------------------------------------------------------------

	public bool IsFacingLeft => this.FacingDirection < 0;
	public bool IsFacingRight => this.FacingDirection > 0;
	public bool IsFacingNeutral => this.FacingDirection == 0;
	public bool IsOnSlope => this.IsOnFloor() && this.GetFloorAngle() > 0f;

	/// <summary>
	/// Determines the direction the character is facing. Any value lower than 0 means the character is facing left,
	/// any value greater than 0 means the character is facing right, and 0 means the character is not facing any.
	/// A value of 0 might be used if, for example, the character is facing the camera or away from the camera.
	/// </summary>
	public int FacingDirection
	{
		get;
		set => field = Math.Sign(value);
	} = 1;

	public float VelocityX
	{
		get => this.Velocity.X;
		set => this.Velocity = new Vector2(value, this.Velocity.Y);
	}

	public float VelocityY
	{
		get => this.Velocity.Y;
		set => this.Velocity = new Vector2(this.Velocity.X, value);
	}

	// -----------------------------------------------------------------------------------------------------------------
	// GODOT EVENTS
	// -----------------------------------------------------------------------------------------------------------------

	public override void _Ready()
	{
		base._Ready();
		this.InputSettings ??= new Platformer2DInputSettings();
		this.MovementSettings ??= new Platformer2DMovementSettings();
		this.JumpSettings ??= new Platformer2DJumpSettings();
	}

	public override void _Process(double delta)
	{
		base._Process(delta);
		this.UpdateLastOnFloorPosition();
		this.UpdateFacing();
	}

	// -----------------------------------------------------------------------------------------------------------------
	// METHODS
	// -----------------------------------------------------------------------------------------------------------------

	private void UpdateLastOnFloorPosition()
	{
		if (this.IsOnFloor() && this.Position.DistanceSquaredTo(this.LastOnFloorPosition) > 64)
		{
			this.LastOnFloorPosition = this.Position;
		}
	}

	private void UpdateFacing()
	{
		if (
			Math.Abs(this.Velocity.X) > Mathf.Epsilon
			&& Math.Abs(this.InputManager.MovementInput.X) > Mathf.Epsilon
			&& Math.Sign(this.Velocity.X) == Math.Sign(this.InputManager.MovementInput.X)
		)
		{
			this.FacingDirection = Math.Sign(this.Velocity.X);
		}
	}

	public void Accelerate(Vector2 targetVelocity, Vector2 acceleration)
		=> this.Velocity = GeneralUtil.MoveToward(this.Velocity, targetVelocity, acceleration);

	/// <summary>
	/// Accelerates the character toward the given target velocity. The acceleration is applied to each axis.
	/// If the character is already moving faster than the target velocity, the acceleration is applied in the opposite
	/// direction to slow down the character.
	/// If the character is already moving at the target velocity, no acceleration is applied.
	///
	/// Params <code>accelerationX</code> and <code>accelerationY</code> must be positive. If they are negative, the
	/// character will accelerate away from the target velocity.
	///
	/// This method only changes the character's Velocity. You still have to call <code>MoveAndSlide</code> or similar
	/// to actually move the character.
	/// </summary>
	public void Accelerate(float targetVelocityX, float targetVelocityY, float accelerationX, float accelerationY)
	{
		this.Velocity = GeneralUtil.MoveToward(
			this.Velocity.X,
			this.Velocity.Y,
			targetVelocityX,
			targetVelocityY,
			accelerationX,
			accelerationY
		);
	}

	/// <summary>
	/// Same as <code>Accelerate</code> but only applies acceleration to the X axis.
	/// </summary>
	public void AccelerateX(float targetVelocityX, float accelerationX)
	{
		this.Velocity = new Vector2(
			Mathf.MoveToward(this.Velocity.X, targetVelocityX, accelerationX),
			this.Velocity.Y
		);
	}

	/// <summary>
	/// Same as <code>Accelerate</code> but only applies acceleration to the Y axis.
	/// </summary>
	public void AccelerateY(float targetVelocityY, float accelerationY)
	{
		this.Velocity = new Vector2(
			this.Velocity.X,
			Mathf.MoveToward(this.Velocity.Y, targetVelocityY, accelerationY)
		);
	}
}
