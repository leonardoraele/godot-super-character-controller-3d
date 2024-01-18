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
	// FIELDS & PROPERTIES
	// -----------------------------------------------------------------------------------------------------------------

	public MotionStateMachine StateMachine { get; private set; } = null!;
	public int AirDashesPerformedCounter = 0;
    private float PhysicsDelta = 1 / 60f;
    public InputController InputController { get; private set; } = null!; // Initialized on _Ready
    public Vector3 LastOnFloorPosition { get; private set; }

	// -----------------------------------------------------------------------------------------------------------------
	// INTERNAL TYPES
	// -----------------------------------------------------------------------------------------------------------------

	public enum CollisionShape { StandUp, Crouch, Crawl, Slide };

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

	public HorizontalMovement CalculateHorizontalMovement(MovementSettings? settings = null) {
		settings ??= this.Settings.Movement;
		Vector2 inputDirection = this.InputController.MovementInput
			.Rotated(this.GetViewport().GetCamera3D().Rotation.Y * -1);
		float currentSpeedUnPSec = GodotUtil.V3ToHV2(this.Velocity).Length();
		float turnSpeedDgPSec = currentSpeedUnPSec < 0.01f
			? float.PositiveInfinity
			: settings.TurnSpeedDgPSec
			* (
				settings.MaxSpeedUnPSec != 0 && settings.TurnSpeedModifier != null
					? settings.TurnSpeedModifier.Sample(Mathf.Min(1, currentSpeedUnPSec / settings.MaxSpeedUnPSec))
					: 1
			);
		float targetSpeedUnPSec = inputDirection.Length() * settings.MaxSpeedUnPSec;
		float turnAngleDg = targetSpeedUnPSec > 0.01f && currentSpeedUnPSec > 0.01f
			? Math.Abs(Mathf.RadToDeg(GodotUtil.V3ToHV2(this.Velocity).AngleTo(inputDirection)))
			: 0;
		if (turnAngleDg > settings.HarshTurnMaxAngleDg) {
			return new HorizontalMovement {
				TargetDirection = GodotUtil.V3ToHV2(this.Velocity).Normalized(),
				TargetSpeedUnPSec = 0,
				AccelerationUnPSecSq = settings._180TurnDecelerationUnPSecSq
			};
		} else if (turnAngleDg > settings.HarshTurnBeginAngleDg) {
			float velocityMultiplier = Mathf.Pow(
				1 - (turnAngleDg - settings.HarshTurnBeginAngleDg) / (settings.HarshTurnMaxAngleDg - settings.HarshTurnBeginAngleDg),
				settings.HarshTurnVelocityLossFactor
			);
			return new HorizontalMovement {
				TargetDirection = inputDirection,
				RotationalSpeedDgPSec = turnSpeedDgPSec,
				TargetSpeedUnPSec = currentSpeedUnPSec * velocityMultiplier,
				AccelerationUnPSecSq = float.PositiveInfinity
			};
		}
		float accelerationUnPSecSq = targetSpeedUnPSec > currentSpeedUnPSec
			? settings.AccelerationUnPSecSq
			: settings.DecelerationUnPSecSq;
		return new HorizontalMovement {
			TargetDirection = inputDirection,
			RotationalSpeedDgPSec = turnSpeedDgPSec,
			TargetSpeedUnPSec = targetSpeedUnPSec,
			AccelerationUnPSecSq = accelerationUnPSecSq
		};
	}

	public void ApplyHorizontalMovement(HorizontalMovement movement)
	{
		// Vector2.AngleTo -> clockwise is positive (returns positive if the given vector is in clockwise direction to this vector)
		float turnAngleRad = movement.TargetDirection.Length() > 0.01f
			? movement.TargetDirection.AngleTo(GodotUtil.V3ToHV2(this.Basis.Z * -1))
			: 0;
		float rotationalSpeedRadPSec = Mathf.DegToRad(movement.RotationalSpeedDgPSec);
		Vector3 newDirection = (this.Basis.Z * -1)
			.Rotated(
				Vector3.Up,
				Math.Min(Math.Abs(turnAngleRad), rotationalSpeedRadPSec * this.PhysicsDelta) * Math.Sign(turnAngleRad)
			);
		this.Rotation = Vector3.Up * GodotUtil.V3ToHV2(newDirection).AngleTo(Vector2.Up);
		float currentHorizontalSpeed = GodotUtil.V3ToHV2(this.Velocity).Length();
		float newHorizontalSpeed = Mathf.MoveToward(currentHorizontalSpeed, movement.TargetSpeedUnPSec, movement.AccelerationUnPSecSq * this.PhysicsDelta);
		this.Velocity = newDirection * newHorizontalSpeed + Vector3.Up * this.Velocity.Y;
	}

	public void ApplyVerticalMovement(VerticalMovement movement)
	{
		if (movement.SnapToFloor) {
			this.ApplyFloorSnap();
		}
		this.Velocity = this.Velocity with {
			Y = Mathf.MoveToward(this.Velocity.Y, movement.TargetVerticalSpeed, movement.Acceleration * this.PhysicsDelta)
		};
	}

    public void SetCollisionShape(CollisionShape shape)
    {
		this.DisableAllCollisionShapes();
        switch(shape) {
			case CollisionShape.StandUp: this.StandUpShape?.Set("disabled", false); break;
			case CollisionShape.Crouch: this.CrouchShape?.Set("disabled", false); break;
			case CollisionShape.Crawl: this.CrawlShape?.Set("disabled", false); break;
			case CollisionShape.Slide: this.SlideShape?.Set("disabled", false); break;
		}
    }

	public void DisableAllCollisionShapes()
	{
		this.StandUpShape?.Set("disabled", true);
		this.CrouchShape?.Set("disabled", true);
		this.CrawlShape?.Set("disabled", true);
		this.SlideShape?.Set("disabled", true);
	}
}
