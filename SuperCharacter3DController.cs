using Godot;
using System;
using System.Collections.Generic;
using Raele.SuperCharacter3D.MotionStates;

namespace Raele.SuperCharacter3D;

// TODO Make presets for:
// P1:
// - Super Mario 64
// - Crash Bandicoot 4
// - Minecraft
// - Half-Life 2
// P2:
// - Fall Guys
// - Pseudoregalia
// - Crash Bandicoot
// - Super Mario Odyssey
// - Devil May Cry 3
// - A Hat in Time
public partial class SuperCharacter3DController : CharacterBody3D, InputController.ISuperPlatformer3DCharacter
{
	// -----------------------------------------------------------------------------------------------------------------
	// EXPORTED FIELDS
	// -----------------------------------------------------------------------------------------------------------------

	[Export] public SuperCharacter3DSettings Settings { get; private set; } = null!; // Initialized on _Ready
	[ExportGroup("Collision Nodes")]
	[Export] public CollisionShape3D? StandUpShape { get; private set; }
	[Export] public CollisionShape3D? CrouchShape { get; private set; }
	[Export] public CollisionShape3D? CrawlShape { get; private set; }
	[Export] public CollisionShape3D? SlideShape { get; private set; }

	// -----------------------------------------------------------------------------------------------------------------
	// SIGNALS
	// -----------------------------------------------------------------------------------------------------------------

	[Signal] public delegate void MotionStateChangedEventHandler(BaseMotionState newState, BaseMotionState? oldState);

	// -----------------------------------------------------------------------------------------------------------------
	// FIELDS
	// -----------------------------------------------------------------------------------------------------------------

	public MotionStateMachine StateMachine { get; private set; } = null!;
	public int AirDashesPerformedCounter = 0;
	public InputController InputController { get; private set; } = null!; // Initialized on _Ready
    public Vector3 LastOnFloorPosition { get; private set; }

    // -----------------------------------------------------------------------------------------------------------------
    // METHODS
    // -----------------------------------------------------------------------------------------------------------------

    public override void _Ready()
    {
		base._Ready();
		this.Settings ??= new SuperCharacter3DSettings();
		this.InputController = new InputController(this);
		this.SetupCharacterBody3D();
		this.SetupChildNodes();
    }

	private void SetupChildNodes()
	{
		if (!GeneralUtility.FindChildByType(this, out MotionStateMachine? stateMachine)) {
            stateMachine = new MotionStateMachine {
                Owner = this.Owner
            };
                // this.AddChild(stateMachine);
        }
		this.StateMachine = stateMachine;
	}

    private void SetupCharacterBody3D()
	{
		// Any change that is made to the CharacterBody3D should be properly logged to the user.
		if (this.MotionMode != MotionModeEnum.Grounded) {
			this.MotionMode = MotionModeEnum.Grounded;
			GD.PushWarning("Changed ", nameof(CharacterBody3D), ".", nameof(CharacterBody3D.MotionMode) ," to Grounded mode.");
		}
	}

    public override void _Process(double delta)
    {
		if (Engine.IsEditorHint()) {
			return;
		}
        base._Process(delta);
		this.InputController.Update();
		this.UpdateLastOnFloorPosition();
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
		if (Engine.IsEditorHint()) {
			return;
		}
    }

    private void UpdateLastOnFloorPosition()
	{
		if (this.IsOnFloor() && this.Position.DistanceSquaredTo(this.LastOnFloorPosition) >= 1) {
			this.LastOnFloorPosition = this.Position;
		}
	}

    // -----------------------------------------------------------------------------------------------------------------
    // PHYSICS UTILITY METHODS
    // -----------------------------------------------------------------------------------------------------------------

	private (Vector2 targetVelocityXZ, Vector2 accelerationXZ) CalculateHorizontalPhysics(
		float delta,
		float maxSpeedUnPSec,
		float accelerationUnPSecSq,
		float normalDecelerationUnPSecSq,
		float breakDecelerationUnPSecSq
	) {
		Vector2 GetXZ(Vector3 v) => new Vector2(v.X, v.Z);
		Vector2 targetVelocityXZ = maxSpeedUnPSec * this.InputController.MovementInput
			.Rotated(this.CalculateCameraRotationAngleDg());
		float angleDiffToTargetVelocity = targetVelocityXZ.Length() > 0.01f
			? targetVelocityXZ.AngleTo(GetXZ(this.Velocity))
			: 0;
		float breakFactor = (float) (Math.Abs(angleDiffToTargetVelocity) / Math.PI);
		Vector2 accelerationXZ = targetVelocityXZ.Length() < 0.01f
			? GetXZ((this.Velocity * -1).Normalized().Abs()) * normalDecelerationUnPSecSq
			: Vector2.One * (
					(targetVelocityXZ.Length() > this.Velocity.Length() ? accelerationUnPSecSq : normalDecelerationUnPSecSq)
					* (1 - breakFactor)
					+ breakDecelerationUnPSecSq
					* breakFactor
			);
		return (targetVelocityXZ, accelerationXZ * delta);
	}

	public virtual (Vector2 velocityXZ, Vector2 accelerationXZ) CalculateHorizontalOnFootPhysics(float delta, MovementSettings? settings = null)
	{
		settings ??= this.Settings.Movement;
		return this.CalculateHorizontalPhysics(
			delta,
			settings.MaxSpeedUnPSec,
			settings.AccelerationUnPSecSq,
			settings.NormalDecelerationUnPSecSq,
			settings.BreakDecelerationUnPSecSq
		);
	}

	public virtual (Vector2 velocityXZ, Vector2 accelerationXZ) CalculateHorizontalOnAirPhysics(float delta, MovementSettings? movementSettings = null, JumpSettings? jumpSettings = null)
	{
		movementSettings ??= this.Settings.Movement;
		jumpSettings ??= this.Settings.Jump;
		return this.CalculateHorizontalPhysics(
			delta,
			movementSettings.MaxSpeedUnPSec,
			movementSettings.AccelerationUnPSecSq * jumpSettings.AerialAccelerationMultiplier,
			movementSettings.NormalDecelerationUnPSecSq,
			movementSettings.BreakDecelerationUnPSecSq
		);
	}

	public virtual (float velocityY, float accelerationY) CalculateVerticalOnFootPhysics()
	{
		if (!this.IsOnFloor()) {
			this.ApplyFloorSnap();
		}
		// Apply a small downward velocity to trigger collision with the floor so that this.IsOnFloor() will
		// remain true while the character is on the floor
		return (0, float.PositiveInfinity);
	}

	public virtual (float velocityY, float accelerationY) CalculateVerticalOnAirPhysics(float delta, JumpSettings? jumpSettings = null)
	{
		jumpSettings ??= this.Settings.Jump;
		float velocityY = jumpSettings.Gravity.MaxFallSpeedUnPSec * -1;
		float accelerationY = jumpSettings.Gravity.FallAccelerationUnPSecSq * delta;
		return (velocityY, accelerationY);
	}

	public virtual float CalculateCameraRotationAngleDg() {
		return this.GetViewport().GetCamera3D().Rotation.Y * -1;
	}

	/// <summary>
	/// Calculates ideal rotation angle of the character based on the user input and camera rotation.
	/// </summary>
	public virtual float CalculateRotationAngleDg()
	{
		return this.InputController.MovementInput.Length() > 0.01f
			? this.InputController.MovementInput.Rotated(this.CalculateCameraRotationAngleDg())
				.AngleTo(Vector2.Up)
			: this.Rotation.Y;
	}

	public Vector3 CalculateRotationEuler()
	{
		return Vector3.Up * this.CalculateRotationAngleDg();
	}

    public void Accelerate(Vector3 targetVelocity, Vector3 acceleration)
    {
		this.Velocity = SuperCharacter3DController.ApplyAcceleration(this.Velocity, targetVelocity, acceleration);
    }

    public void Accelerate(Vector2 targetVelocityXZ, float targetVelocityY, Vector2 accelerationXZ, float accelerationY)
	{
		this.Velocity = SuperCharacter3DController.ApplyAcceleration(
			this.Velocity.X,
			this.Velocity.Y,
			this.Velocity.Z,
			targetVelocityXZ.X,
			targetVelocityY,
			targetVelocityXZ.Y,
			accelerationXZ.X,
			accelerationY,
			accelerationXZ.Y
		);
    }

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
    public void Accelerate(
		float targetVelocityX,
		float targetVelocityY,
		float targetVelocityZ,
		float accelerationX = float.PositiveInfinity,
		float accelerationY = float.PositiveInfinity,
		float accelerationZ = float.PositiveInfinity
	) {
		this.Velocity = SuperCharacter3DController.ApplyAcceleration(
			this.Velocity.X,
			this.Velocity.Y,
			this.Velocity.Z,
			targetVelocityX,
			targetVelocityY,
			targetVelocityZ,
			accelerationX,
			accelerationY,
			accelerationZ
		);
    }

	/// <summary>
	/// Same as <code>Accelerate</code> but only applies acceleration to the X and Z axis.
	/// </summary>
	public void AccelerateXZ(Vector2 targetVelocityXZ, Vector2 accelerationXZ)
	{
		this.Velocity = new Vector3(
			SuperCharacter3DController.ApplyAcceleration(this.Velocity.X, targetVelocityXZ.X, accelerationXZ.X),
			this.Velocity.Y,
			SuperCharacter3DController.ApplyAcceleration(this.Velocity.Z, targetVelocityXZ.Y, accelerationXZ.Y)
		);
	}

	/// <summary>
	/// Same as <code>Accelerate</code> but only applies acceleration to the Y axis.
	/// </summary>
	public void AccelerateY(float targetVelocityY, float accelerationY)
	{
		this.Velocity = new Vector3(
			this.Velocity.X,
			SuperCharacter3DController.ApplyAcceleration(this.Velocity.Y, targetVelocityY, accelerationY),
			this.Velocity.Z
		);
	}

	public static Vector3 ApplyAcceleration(Vector3 currentVelocity, Vector3 targetVelocity, Vector3 acceleration)
	{
		return SuperCharacter3DController.ApplyAcceleration(
			currentVelocity.X,
			currentVelocity.Y,
			currentVelocity.Z,
			targetVelocity.X,
			targetVelocity.Y,
			targetVelocity.Z,
			acceleration.X,
			acceleration.Y,
			acceleration.Z
		);
	}

	public static Vector3 ApplyAcceleration(
		float currentVelocityX,
		float currentVelocityY,
		float currentVelocityZ,
		float targetVelocityX,
		float targetVelocityY,
		float targetVelocityZ,
		float accelerationX,
		float accelerationY,
		float accelerationZ
	) {
		return new Vector3(
			SuperCharacter3DController.ApplyAcceleration(currentVelocityX, targetVelocityX, accelerationX),
			SuperCharacter3DController.ApplyAcceleration(currentVelocityY, targetVelocityY, accelerationY),
			SuperCharacter3DController.ApplyAcceleration(currentVelocityZ, targetVelocityZ, accelerationZ)
		);
	}

	public static float ApplyAcceleration(float currentVelocity, float targetVelocity, float acceleration)
	{
		return Mathf.MoveToward(currentVelocity, targetVelocity, acceleration);
	}
}
