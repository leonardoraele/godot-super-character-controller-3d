using Godot;

namespace Raele.SuperCharacter3D;

// TODO Make presets for:
// P1:
// - Super Mario Odyssey
// - Crash Bandicoot 4
// - Minecraft
// P2:
// - Fall Guys
// - Pseudoregalia
// - Devil May Cry 3
// - A Hat in Time
public partial class SuperCharacter3DController : CharacterBody3D, InputController.ISuperPlatformer3DCharacter
{
	// -----------------------------------------------------------------------------------------------------------------
	// EXPORTED FIELDS
	// -----------------------------------------------------------------------------------------------------------------

	[Export] public InputSettings InputSettings { get; private set; } = new InputSettings();

	// -----------------------------------------------------------------------------------------------------------------
	// FIELDS & PROPERTIES
	// -----------------------------------------------------------------------------------------------------------------

	public MotionStateMachine StateMachine { get; private set; } = null!;
	public int AirDashesPerformedCounter = 0;
    private float PhysicsDelta = 1 / 60f;
    public InputController InputController { get; private set; } = null!; // Initialized on _Ready
    public Vector3 LastOnFloorPosition { get; private set; }

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
