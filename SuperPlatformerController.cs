using Godot;
using System;
using System.Collections.Generic;

namespace Raele.SuperPlatformer;

// TODO Make presets for:
// P1:
// - Mario Bros
// - Mega Man 3
// - Celeste
// - Super Meat Boy
// - Shovel Knight
// P2:
// - Castlevania
// - Spelunky
// - Super Metroid
// - Super Mario World
// - Mega Man X
// - Hollow Knight
// - Prince of Persia
// - Limbo
// P3:
// - Super Mario World 2: Yoshi's Island
// - Mega Man 11
// - Mega Man X4
// - Sonic
// - Terraria
// - Castlevania: Symphony of the Night
// P4:
// - Ori and the Blind Forest
// - Ninja Gaoden
// - Mark of the Ninja
// - Dead Cells
// - Blasphemous
public partial class SuperPlatformerController : CharacterBody2D
{
	// -----------------------------------------------------------------------------------------------------------------
	// EXPORTED FIELDS
	// -----------------------------------------------------------------------------------------------------------------

	[Export] public Platformer2DInputSettings InputSettings = null!;
	[Export] public Platformer2DMovementSettings MovementSettings = null!;
	[Export] public Platformer2DJumpSettings JumpSettings = null!;
	[Export] public Platformer2DDashSettings? DashSettings;
	[Export] public Platformer2DWallMotionSettings? WallMotionSettings;

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
	[Export] public ulong JumpInputBufferSensitivityMs { get; private set; } = 150;
	/// <summary>
	/// Late jump input tolerance time. (if player enters jump input after jump action is no longer possible, allow the
	/// character to jump anyway as long as it was possible up to this much time before)
	/// </summary>
	[Export] public ulong CoyoteJumpLeniencyMs { get; private set; } = 150;
	[Export] public ulong DashInputBufferSensitivityMs { get; private set; } = 150;
	/// <summary>
	/// Max time the character floats below a ceiling when they hit a ceiling during a jump before they start falling.
	/// Player can cancel ceiling slide by releasing the jump button earlier.
	/// </summary>
	[Export] public ulong CeilingSlideTimeMs { get; private set; } = 150;
	/// <summary>
	/// This property determines how much time, in miliseconds, the player must hold a directional input in the opposite
	/// direction to a wall to let go of that wall during a wall climb or wall slide. i.e. If this property is set to a
	/// positive value, the player will let go of a wall climb or wall slide only if they press a directional input in
	/// the direction opposite to the wall for at least this much time.
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
	[Export] public int WallDropPreventionLeniencyMs { get; private set; } = 150;

	// -----------------------------------------------------------------------------------------------------------------
	// SIGNALS
	// -----------------------------------------------------------------------------------------------------------------

	[Signal] public delegate void MotionStateChangedEventHandler(BaseMotionState newState, BaseMotionState? oldState);

	// -----------------------------------------------------------------------------------------------------------------
	// FIELDS
	// -----------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// This is the current motion state of the character.
	/// Use TransitionState method to change the state.
	/// </summary>
	public BaseMotionState? State { get; private set; } = null;
	/// <summary>
	/// This is the number of dashes performed by the character so far while they are in the air. By default, it is
	/// incremented every time the character starts a dash and is reset when the character lands on the floor.
	/// </summary>
	public int AirDashesPerformedCounter = 0;
	public SuperPlatformerInputController InputController { get; private set; } = null!; // Initialized on _Ready
	public Vector2 LastOnFloorPosition { get; private set; }
	private int _facingDirection = 0;
	public Dictionary<string, BaseMotionState> StateDict = new Dictionary<string, BaseMotionState>();

	// -----------------------------------------------------------------------------------------------------------------
	// PROPERTIES
	// -----------------------------------------------------------------------------------------------------------------

	public bool IsFacingLeft => this.FacingDirection < 0;
	public bool IsFacingRight => this.FacingDirection > 0;
	/// <summary>
	/// Determines the direction the character is facing. Any value lower than 0 means the character is facing left,
	/// any value greater than 0 means the character is facing right, and 0 means the character is not facing any.
	/// A value of 0 might be used if, for example, the character is facing the camera or away from the camera.
	/// </summary>
	public int FacingDirection {
		get => this._facingDirection;
		set => this._facingDirection = Math.Sign(value);
	}

	// -----------------------------------------------------------------------------------------------------------------
	// METHODS
	// -----------------------------------------------------------------------------------------------------------------

	public override void _Ready()
	{
		base._Ready();
		this.SetupCharacterBody2D();
		this.RegisterBuiltinMotionStates();
		this.InputController = new SuperPlatformerInputController(this);
		this.InputSettings ??= new Platformer2DInputSettings();
		this.MovementSettings ??= new Platformer2DMovementSettings();
		this.JumpSettings ??= new Platformer2DJumpSettings();
		this.FacingDirection = 1;
		this.ResetState();
	}

	public override void _EnterTree()
	{
		base._EnterTree();
		foreach (Node child in this.GetChildren()) {
			this.OnNodeEnteredTree(child);
		}
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
		this.AddChild(new JumpWindupState() { Name = nameof(JumpWindupState) });
		this.AddChild(new LandingRecoveryState() { Name = nameof(LandingRecoveryState) });
		this.AddChild(new LandingRecoveryState() { Name = nameof(LandingRecoveryState) });
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

	private void SetupCharacterBody2D()
	{
		// Any change that is made to the CharacterBody2D should be properly logged to the user.
		if (this.MotionMode != MotionModeEnum.Grounded) {
			GD.PushWarning(nameof(SuperPlatformerController), "expects CharacterBody2D to be in Grounded mode. Setting it now.");
			this.MotionMode = MotionModeEnum.Grounded;
		}
	}

	public override void _Process(double delta)
	{
		base._Process(delta);
		this.InputController.Update();
		this.UpdateLastOnFloorPosition();
		this.UpdateFacing();
		this.State?.OnProcessState((float) delta);
	}

	private void UpdateLastOnFloorPosition()
	{
		if (this.IsOnFloor() && this.Position.DistanceSquaredTo(this.LastOnFloorPosition) > 64) {
			this.LastOnFloorPosition = this.Position;
		}
	}

	private void UpdateFacing()
	{
		if (Math.Abs(this.Velocity.X) > 0.01f)
		{
			this.FacingDirection = this.Velocity.X > 0 ? 1 : -1;
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
			throw new Exception($"Failed to transition to motion state \"{nextStateName}\". Cause: State not found. Did you forget to add it as a child node of {nameof(SuperPlatformerController)}?");
		}
		BaseMotionState? previousState = this.State;
		if (previousState != null) {
			try {
				GD.PrintS(nameof(SuperPlatformerController), "Exiting state:", previousState.Name);
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
			GD.PrintS(nameof(SuperPlatformerController), "Entering state:", nextState.Name);
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

	public void Accelerate(Vector2 targetVelocity, Vector2 acceleration)
	{
		this.Velocity = SuperPlatformerController.ApplyAcceleration(this.Velocity, targetVelocity, acceleration);
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
		float accelerationX = float.PositiveInfinity,
		float accelerationY = float.PositiveInfinity
	) {
		this.Velocity = SuperPlatformerController.ApplyAcceleration(
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
			SuperPlatformerController.ApplyAcceleration(this.Velocity.X, targetVelocityX, accelerationX),
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
			SuperPlatformerController.ApplyAcceleration(this.Velocity.Y, targetVelocityY, accelerationY)
		);
	}

	public static Vector2 ApplyAcceleration(Vector2 currentVelocity, Vector2 targetVelocity, Vector2 acceleration)
	{
		return SuperPlatformerController.ApplyAcceleration(
			currentVelocity.X,
			currentVelocity.Y,
			targetVelocity.X,
			targetVelocity.Y,
			acceleration.X,
			acceleration.Y
		);
	}

	public static Vector2 ApplyAcceleration(
		float currentVelocityX,
		float currentVelocityY,
		float targetVelocityX,
		float targetVelocityY,
		float accelerationX,
		float accelerationY
	) {
		return new Vector2(
			SuperPlatformerController.ApplyAcceleration(currentVelocityX, targetVelocityX, accelerationX),
			SuperPlatformerController.ApplyAcceleration(currentVelocityY, targetVelocityY, accelerationY)
		);
	}

	public static float ApplyAcceleration(float currentVelocity, float targetVelocity, float acceleration)
	{
		return Mathf.MoveToward(currentVelocity, targetVelocity, acceleration);
	}
}
