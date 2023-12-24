using Godot;
using System;
using System.Collections.Generic;
using Raele.SuperCharacterController3D.MotionStates;

namespace Raele.SuperCharacterController3D;

// TODO Make presets for:
// - Devil May Cry 3
// - Prince of Persia Sands of Time
// - Super Mario 64
// - Crash Bandicoot
// - Fall Guys
public partial class SuperCharacterController3D : CharacterBody3D, InputController.ISuperPlatformer3DCharacter
{
	// -----------------------------------------------------------------------------------------------------------------
	// EXPORTED FIELDS
	// -----------------------------------------------------------------------------------------------------------------

	[Export] public SuperPlatformer3DBaseSettings Settings { get; private set; } = null!; // Initialized on _Ready

	// -----------------------------------------------------------------------------------------------------------------
	// SIGNALS
	// -----------------------------------------------------------------------------------------------------------------

	[Signal] public delegate void MotionStateChangedEventHandler(BaseMotionState newState, BaseMotionState? oldState);

	// -----------------------------------------------------------------------------------------------------------------
	// FIELDS
	// -----------------------------------------------------------------------------------------------------------------

	public BaseMotionState? State { get; private set; } = null;
	public int AirDashesPerformedCounter = 0;
	public InputController InputController { get; private set; } = null!; // Initialized on _Ready
    public Vector3 LastOnFloorPosition { get; private set; }
	public Dictionary<string, BaseMotionState> StateDict = new Dictionary<string, BaseMotionState>();

    // -----------------------------------------------------------------------------------------------------------------
    // METHODS
    // -----------------------------------------------------------------------------------------------------------------

    public override void _Ready()
    {
		base._Ready();
		this.Settings ??= new SuperPlatformer3DBaseSettings();
		this.InputController = new InputController(this);
		this.SetupCharacterBody3D();
		RegisterBuiltinMotionStates();
		ResetState();
    }

    private void RegisterBuiltinMotionStates()
    {
		this.ChildEnteredTree += this.OnNodeEnteredTree;
		this.ChildExitingTree += this.OnNodeExitingTree;
        this.AddChild(new OnFootState() { Name = nameof(OnFootState) });
		this.AddChild(new FallingState() { Name = nameof(FallingState) });
		this.AddChild(new WallClimbingState() { Name = nameof(WallClimbingState) });
		this.AddChild(new WallSlidingState() { Name = nameof(WallSlidingState) });
		this.AddChild(new JumpCanceledState() { Name = nameof(JumpCanceledState) });
		this.AddChild(new GroundDashingState() { Name = nameof(GroundDashingState) });
		this.AddChild(new AirDashingState() { Name = nameof(AirDashingState) });
		this.AddChild(new DeadState() { Name = nameof(DeadState) });
		this.AddChild(new InteractingState() { Name = nameof(InteractingState) });
		this.AddChild(new JumpingState() { Name = nameof(JumpingState) });
    }

	private void OnNodeEnteredTree(Node node)
	{
		if (node is BaseMotionState state) {
			this.StateDict[state.Name] = state;
		}
	}

	private void OnNodeExitingTree(Node node)
	{
		if (node is BaseMotionState state) {
			this.StateDict.Remove(state.Name);
		}
	}

    private void SetupCharacterBody3D()
	{
		// Any change that is made to the CharacterBody2D should be properly logged to the user.
		if (this.MotionMode != MotionModeEnum.Grounded) {
			GD.PushWarning(nameof(SuperCharacterController3D), "expects CharacterBody2D to be in Grounded mode. Setting it now.");
			this.MotionMode = MotionModeEnum.Grounded;
		}
	}

    public override void _Process(double delta)
    {
        base._Process(delta);
		this.InputController.Update();
		this.UpdateLastOnFloorPosition();
		this.State?.OnProcessState((float) delta);
    }

	private void UpdateLastOnFloorPosition()
	{
		if (this.IsOnFloor() && this.Position.DistanceSquaredTo(this.LastOnFloorPosition) > 1) {
			this.LastOnFloorPosition = this.Position;
		}
	}

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
		this.State?.OnPhysicsProcessState((float) delta);
    }

    public void TransitionMotionState<T>(Variant? data = null) where T : BaseMotionState
    {
		this.TransitionMotionState(typeof(T).Name, data);
    }

	public void TransitionMotionState(string nextStateName, Variant? data = null)
	{
		if (!this.StateDict.TryGetValue(nextStateName, out BaseMotionState? nextState)) {
			throw new Exception($"Failed to transition to motion state \"{nextStateName}\". Cause: State not found. Did you forget to add it as a child node of {nameof(SuperCharacterController3D)}?");
		}
		BaseMotionState? previousState = this.State;
		if (previousState != null) {
			try {
				GD.PrintS(nameof(SuperCharacterController3D), "Exiting state:", previousState.Name);
				previousState.OnExit(new BaseMotionState.TransitionInfo() {
					PreviousState = previousState.Name,
					NextState = nextStateName,
					Cancel = () => nextState = null,
					Data = data,
				});
			} catch (Exception e) {
				GD.PushError(e);
			}
		}
		if (nextState == null || this.State != previousState) {
			return;
		}
		try {
			GD.PrintS(nameof(SuperCharacterController3D), "Entering state:", nextState.Name);
			nextState.OnEnter(new BaseMotionState.TransitionInfo() {
				PreviousState = this.State?.Name,
				NextState = nextState.Name,
				Cancel = () => nextState = null,
				Data = data,
			});
		} catch(Exception e) {
			GD.PushError(e);
			this.State = null;
			this.ResetState();
			return;
		}
		if (nextState == null || this.State != previousState) {
			return;
		}
		this.State = nextState;
		this.EmitSignal(SignalName.MotionStateChanged, this.State, previousState!);
	}

	/// <summary>
	/// Changes the character to a neutral state. The state machine will automatically transition to the correct state.
	/// </summary>
	public void ResetState()
	{
		if (this.IsOnFloor())
		{
			this.TransitionMotionState<OnFootState>();
		}
		else
		{
			this.TransitionMotionState<FallingState>();
		}
	}

    public void Accelerate(Vector3 targetVelocity, Vector3 acceleration)
    {
		this.Velocity = SuperCharacterController3D.ApplyAcceleration(this.Velocity, targetVelocity, acceleration);
    }

    public void Accelerate(Vector2 targetVelocityXZ, float targetVelocityY, Vector2 accelerationXZ, float accelerationY)
	{
		this.Velocity = SuperCharacterController3D.ApplyAcceleration(
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
		this.Velocity = SuperCharacterController3D.ApplyAcceleration(
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
			SuperCharacterController3D.ApplyAcceleration(this.Velocity.X, targetVelocityXZ.X, accelerationXZ.X),
			this.Velocity.Y,
			SuperCharacterController3D.ApplyAcceleration(this.Velocity.Z, targetVelocityXZ.Y, accelerationXZ.Y)
		);
	}

	/// <summary>
	/// Same as <code>Accelerate</code> but only applies acceleration to the Y axis.
	/// </summary>
	public void AccelerateY(float targetVelocityY, float accelerationY)
	{
		this.Velocity = new Vector3(
			this.Velocity.X,
			SuperCharacterController3D.ApplyAcceleration(this.Velocity.Y, targetVelocityY, accelerationY),
			this.Velocity.Z
		);
	}

	public static Vector3 ApplyAcceleration(Vector3 currentVelocity, Vector3 targetVelocity, Vector3 acceleration)
	{
		return SuperCharacterController3D.ApplyAcceleration(
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
			SuperCharacterController3D.ApplyAcceleration(currentVelocityX, targetVelocityX, accelerationX),
			SuperCharacterController3D.ApplyAcceleration(currentVelocityY, targetVelocityY, accelerationY),
			SuperCharacterController3D.ApplyAcceleration(currentVelocityZ, targetVelocityZ, accelerationZ)
		);
	}

	public static float ApplyAcceleration(float currentVelocity, float targetVelocity, float acceleration)
	{
		if (acceleration < 0) {
			GD.PushWarning("Accelerating character with a negative value. This means the character will never reach the target velocity and will likely move in the wrong direction. Acceleration must be positive. Got: " + acceleration);
		}
		return Mathf.MoveToward(currentVelocity, targetVelocity, acceleration);
	}
}
