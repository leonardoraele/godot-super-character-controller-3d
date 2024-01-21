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

	public void ApplyHorizontalMovement(HorizontalMovement movement)
	{
		float turnAngleRad = movement.TargetDirection.Length() > 0.01f
			? movement.TargetDirection.AngleTo(GodotUtil.V3ToHV2(this.Basis.Z * -1))
			: 0;
		float rotationalSpeedRadPSec = Mathf.DegToRad(movement.RotationSpeedDegPSec);
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
