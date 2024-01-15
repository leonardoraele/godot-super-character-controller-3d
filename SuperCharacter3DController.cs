using Godot;
using System;
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
	// FIELDS & PROPERTIES
	// -----------------------------------------------------------------------------------------------------------------

	public MotionStateMachine StateMachine { get; private set; } = null!;
	public int AirDashesPerformedCounter = 0;
    private float PhysicsDelta = 1 / 60f;
    public InputController InputController { get; private set; } = null!; // Initialized on _Ready
    public Vector3 LastOnFloorPosition { get; private set; }

	public Vector3 Forward => this.Basis.Z * -1;

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
		if (!GodotUtil.FindChildByType(this, out MotionStateMachine? stateMachine)) {
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
		this.PhysicsDelta = (float) delta;
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

	private HorizontalMovement CalculateHorizontalMovement(MovementSettings? settings = null) {
		settings ??= this.Settings.Movement;
		Vector2 inputDirection = this.InputController.MovementInput
			.Rotated(this.GetViewport().GetCamera3D().Rotation.Y * -1);
		float currentSpeedUnPSec = GodotUtil.V3ToHV2(this.Velocity).Length();
		float targetSpeedUnPSec = inputDirection.Length() * settings.MaxSpeedUnPSec;
		float turnAngleDg = targetSpeedUnPSec > 0.01f && currentSpeedUnPSec > 0.01f
			? Math.Abs(Mathf.RadToDeg(GodotUtil.V3ToHV2(this.Velocity).AngleTo(inputDirection)))
			: 0;
		if (turnAngleDg > settings.HarshTurnMaxAngleDg) {
			return new HorizontalMovement {
				TargetDirection = GodotUtil.V3ToHV2(this.Velocity).Normalized(),
				TargetSpeedUnPSec = 0,
				AccelerationUnPSecSq = settings.TurnDecelerationUnPSecSq
			};
		} else if (Math.Abs(turnAngleDg - settings.HarshTurnBeginAngleDg) > 0.01f) {
			float velocityMultiplier = Mathf.Pow(
				1 - (turnAngleDg - settings.HarshTurnBeginAngleDg) / (settings.HarshTurnMaxAngleDg - settings.HarshTurnBeginAngleDg),
				settings.HarshTurnVelocityLossFactor
			);
			return new HorizontalMovement {
				TargetDirection = inputDirection,
				RotationalSpeedDgPSec = settings.TurnAngleDgPSec,
				TargetSpeedUnPSec = currentSpeedUnPSec * velocityMultiplier,
				AccelerationUnPSecSq = float.PositiveInfinity
			};
		}
		float accelerationUnPSecSq = targetSpeedUnPSec > currentSpeedUnPSec
			? settings.AccelerationUnPSecSq
			: settings.DecelerationUnPSecSq;
		return new HorizontalMovement {
			TargetDirection = inputDirection,
			RotationalSpeedDgPSec = settings.TurnAngleDgPSec,
			TargetSpeedUnPSec = targetSpeedUnPSec,
			AccelerationUnPSecSq = accelerationUnPSecSq
		};
	}

	public virtual HorizontalMovement CalculateOnFootHorizontalMovement(MovementSettings? settings = null)
	{
		return this.CalculateHorizontalMovement(settings);
	}

	public virtual HorizontalMovement CalculateOnAirHorizontalMovement(
		MovementSettings? movementSettings = null,
		JumpSettings? jumpSettings = null
	) {
		jumpSettings ??= this.Settings.Jump;
        HorizontalMovement horizontalMovement = this.CalculateOnFootHorizontalMovement(movementSettings);
		return horizontalMovement with {
			AccelerationUnPSecSq = horizontalMovement.AccelerationUnPSecSq * jumpSettings.AerialAccelerationMultiplier
		};
	}

	public virtual VerticalMovement CalculateOnFootVerticalMovement()
	{
		return new VerticalMovement { Speed = 0, Acceleration = float.PositiveInfinity, SnapToFloor = true };
	}

	public virtual VerticalMovement CalculateOnAirVerticalMovement(JumpSettings? settings = null)
	{
		settings ??= this.Settings.Jump;
		return new VerticalMovement {
			Speed = settings.Gravity.MaxFallSpeedUnPSec * -1,
			Acceleration = settings.Gravity.FallAccelerationUnPSecSq,
		};
	}

	public void ApplyHorizontalMovement(HorizontalMovement movement)
	{
		// Vector2.AngleTo -> clockwise is positive (returns positive if the given vector is in clockwise direction to this vector)
		float turnAngleRad = movement.TargetSpeedUnPSec > 0.01f && this.Velocity.Length() > 0.01f
			? GodotUtil.V3ToHV2(this.Velocity).AngleTo(movement.TargetDirection) * -1
			: 0;
		float RotationalSpeedRadPSec = Mathf.DegToRad(movement.RotationalSpeedDgPSec);
		Vector3 newDirection = Math.Abs(turnAngleRad) < 0.001f
			? movement.TargetDirection.Length() > 0.01f
				? GodotUtil.HV2ToV3(movement.TargetDirection)
				: this.Basis.Z * -1
			: (this.Basis.Z * -1).Rotated(
				Vector3.Up,
				RotationalSpeedRadPSec * this.PhysicsDelta > Math.Abs(turnAngleRad)
					? turnAngleRad
					: RotationalSpeedRadPSec * this.PhysicsDelta * Math.Sign(turnAngleRad)
			);
		this.Rotation = Vector3.Up * GodotUtil.V3ToHV2(newDirection).AngleTo(Vector2.Up);
		float currentHorizontalSpeed = new Vector2(this.Velocity.X, this.Velocity.Z).Length();
		float newHorizontalSpeed = Mathf.MoveToward(currentHorizontalSpeed, movement.TargetSpeedUnPSec, movement.AccelerationUnPSecSq * this.PhysicsDelta);
		this.Velocity = newDirection * newHorizontalSpeed + Vector3.Up * this.Velocity.Y;
	}

	public void ApplyVerticalMovement(VerticalMovement movement)
	{
		if (movement.SnapToFloor) {
			this.ApplyFloorSnap();
		}
		this.Velocity = this.Velocity with {
			Y = Mathf.MoveToward(this.Velocity.Y, movement.Speed, movement.Acceleration * this.PhysicsDelta)
		};
	}
}
