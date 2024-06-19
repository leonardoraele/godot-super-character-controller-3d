#nullable enable
using Godot;

namespace Raele.SuperCharacter3D;

// TODO Make presets for:
// - Super Mario 64/Odyssey
// - Tomb Raider
// - Devil May Cry 3/5
// - Fall Guys
// - Counter-Strike 2
public partial class SuperCharacter3DController : CharacterBody3D
{
	// -----------------------------------------------------------------------------------------------------------------
	// EXPORTED FIELDS
	// -----------------------------------------------------------------------------------------------------------------

	[ExportGroup("Directional Input Map")]
	[Export] public string MoveForwardAction { get; private set; } = "character_move_forward";
	[Export] public string MoveBackAction { get; private set; } = "character_move_back";
	[Export] public string MoveLeftAction { get; private set; } = "character_move_left";
	[Export] public string MoveRightAction { get; private set; } = "character_move_right";
	[Export] public InputController.CameraModeEnum CameraMode {
		get => this.InputController.CameraMode;
		set => this.InputController.CameraMode = value;
	}

	// -----------------------------------------------------------------------------------------------------------------
	// SIGNALS
	// -----------------------------------------------------------------------------------------------------------------

	[Signal] public delegate void OnFloorEnterEventHandler();
	[Signal] public delegate void OnFloorExitEventHandler();
	[Signal] public delegate void OnWallEnterEventHandler();
	[Signal] public delegate void OnWallExitEventHandler();
	[Signal] public delegate void OnCeilingEnterEventHandler();
	[Signal] public delegate void OnCeilingExitEventHandler();

	// -----------------------------------------------------------------------------------------------------------------
	// FIELDS & PROPERTIES
	// -----------------------------------------------------------------------------------------------------------------

	public int AirDashesPerformedCounter = 0;
    private float PhysicsDelta = 1 / 60f;
    public InputController InputController { get; private set; } = null!; // Initialized on _Ready
    public Vector3 LastOnFloorPosition { get; private set; }
	private bool WasOnFloorLastFrame;
	private bool WasOnWallLastFrame;
	private bool WasOnCeilingLastFrame;

	/// <summary>
	/// This is the character's velocity relative to their basis of rotation. That is, the character's forward direction
	/// is always the Z axis of their basis.
	/// </summary>
	public Vector3 LocalVelocity {
		get => this.Velocity.Rotated(Vector3.Up, this.Rotation.Y * -1);
		set => this.Velocity = value.Rotated(Vector3.Up, this.Rotation.Y);
	}
	public float ForwardSpeed {
		get => this.LocalVelocity.Z * -1;
		set => this.LocalVelocity = this.LocalVelocity with { Z = value * -1 };
	}
	public float SidewaySpeed {
		get => this.LocalVelocity.X;
		set => this.LocalVelocity = this.LocalVelocity with { X = value };
	}
	public float VerticalSpeed {
		get => this.Velocity.Y;
		set => this.Velocity = this.Velocity with { Y = value };
	}
	public Vector3 Forward => this.Basis.Z * -1;

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
		this.InputController = new InputController(this);
		this.SetupCharacterBody3D();
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
		this.UpdateLastOnFloorState();
		this.UpdateLastOnWallState();
		this.UpdateLastOnCeilingState();
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
		this.PhysicsDelta = (float) delta;
    }

    private void UpdateLastOnFloorPosition()
	{
		if (this.IsOnFloor() && this.Position.DistanceSquaredTo(this.LastOnFloorPosition) >= 1) {
			this.LastOnFloorPosition = this.Position;
		}
	}

    private void UpdateLastOnFloorState()
	{
		if (this.IsOnFloor() && !this.WasOnFloorLastFrame) {
			this.EmitSignal(SignalName.OnFloorEnter);
		} else if (!this.IsOnFloor() && this.WasOnFloorLastFrame) {
			this.EmitSignal(SignalName.OnFloorExit);
		}
		this.WasOnFloorLastFrame = this.IsOnFloor();
	}

    private void UpdateLastOnWallState()
	{
		if (this.IsOnWall() && !this.WasOnWallLastFrame) {
			this.EmitSignal(SignalName.OnWallEnter);
		} else if (!this.IsOnWall() && this.WasOnWallLastFrame) {
			this.EmitSignal(SignalName.OnWallExit);
		}
		this.WasOnWallLastFrame = this.IsOnWall();
	}

    private void UpdateLastOnCeilingState()
	{
		if (this.IsOnCeiling() && !this.WasOnCeilingLastFrame) {
			this.EmitSignal(SignalName.OnCeilingEnter);
		} else if (!this.IsOnCeiling() && this.WasOnCeilingLastFrame) {
			this.EmitSignal(SignalName.OnCeilingExit);
		}
		this.WasOnCeilingLastFrame = this.IsOnCeiling();
	}

    // -----------------------------------------------------------------------------------------------------------------
    // PHYSICS UTILITY METHODS
    // -----------------------------------------------------------------------------------------------------------------

	public void LookAt(Vector3 globalPosition, bool preserveLocalVelocity)
	{
		if (preserveLocalVelocity) {
			Vector3 previousLocalVelocity = this.LocalVelocity;
			this.LookAt(globalPosition);
			this.LocalVelocity = previousLocalVelocity;
		} else {
			this.LookAt(globalPosition);
		}
	}
	public void LookAtDirection(Vector3 globalDirection, bool preserveLocalVelocity = false)
		=> this.LookAt(this.GlobalPosition + globalDirection, preserveLocalVelocity);
	public void RotateToward(Vector3 globalPosition, float maxRadians, bool preserveLocalVelocity = false)
	{
		Vector3 direction = globalPosition - this.GlobalPosition;
		if (direction.LengthSquared() > Mathf.Epsilon) {
			this.RotateTowardDirection(direction.Normalized(), maxRadians, preserveLocalVelocity);
		}
	}
	public void RotateTowardDirection(Vector3 globalDirection, float maxRadians, bool preserveLocalVelocity = false)
		=> this.LookAt(this.GlobalPosition + GodotUtil.RotateToward(this.GlobalTransform.Basis.Z * -1, globalDirection, maxRadians), preserveLocalVelocity);
}
