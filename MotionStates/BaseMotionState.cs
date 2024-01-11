using System;
using Godot;

namespace Raele.SuperCharacter3D.MotionStates;

public abstract partial class BaseMotionState : Node
{
	public record TransitionInfo {
		private static Action Noop = () => {};
		public string? PreviousState { get; init; }
		public string NextState { get; init; } = "";
		public Variant? Data { get; init; }
		public Action Cancel { get; init; } = TransitionInfo.Noop;
	}

	public ulong ActivationTime { get; private set; } = 0;
    protected SuperCharacter3DController Character { get; private set; } = null!;

	public ulong DurationActiveMs => Time.GetTicksMsec() - this.ActivationTime;

    public override void _EnterTree()
    {
        base._EnterTree();
		if (this.GetParent() is SuperCharacter3DController character) {
			this.Character = character;
		} else {
			GD.PushError($"MotionState node of type \"{this.GetType().Name}\" must be a child of {typeof(SuperCharacter3DController).Name}");
		}
	}

    /// <summary>
    /// Called when this becomes the active state.
    /// </summary>
    public virtual void OnEnter(TransitionInfo transition)
	{
		this.ActivationTime = Time.GetTicksMsec();
	}

	/// <summary>
	/// Called when this is cases to be the active state.
	/// </summary>
	public virtual void OnExit(TransitionInfo transition) {}

	/// <summary>
	/// Called every frame (on _Process) while this state is active.
	/// Not called if this is not the activestate.
	/// If you need to read input while this state is not active, do it in <code>Node._Process</code> instead. This is
	/// usually necessary if you want to buffer input that might be relevant soon (e.g. doouble tapping tro dash) or if
	/// you want to check a condition that might cause a state transition from any other state to this state.
	/// </summary>
	public virtual void OnProcessState(float delta) {}

	/// <summary>
	/// Called every physics tick (on _PhysicsProcess) while this state is active.
	/// Not called if this is not the activestate.
	/// </summary>
	public virtual void OnPhysicsProcessState(float delta) {}

	private (Vector2 targetVelocityXZ, Vector2 accelerationXZ) CalculateHorizontalPhysics(
		float delta,
		float maxSpeedUnPSec,
		float accelerationUnPSecSq,
		float normalDecelerationUnPSecSq,
		float breakDecelerationUnPSecSq
	) {
		Vector2 GetXZ(Vector3 v) => new Vector2(v.X, v.Z);
		Vector2 targetVelocityXZ = maxSpeedUnPSec * this.Character.InputController.MovementInput
			.Rotated(this.CalculateCameraRotationAngleDg());
		float angleDiffToTargetVelocity = targetVelocityXZ.Length() > 0.01f
			? targetVelocityXZ.AngleTo(GetXZ(this.Character.Velocity))
			: 0;
		float breakFactor = (float) (Math.Abs(angleDiffToTargetVelocity) / Math.PI);
		Vector2 accelerationXZ = targetVelocityXZ.Length() < 0.01f
			? GetXZ((this.Character.Velocity * -1).Normalized().Abs()) * normalDecelerationUnPSecSq
			: Vector2.One * (
					(targetVelocityXZ.Length() > this.Character.Velocity.Length() ? accelerationUnPSecSq : normalDecelerationUnPSecSq)
					* (1 - breakFactor)
					+ breakDecelerationUnPSecSq
					* breakFactor
			);
		return (targetVelocityXZ, accelerationXZ * delta);
	}

	protected virtual (Vector2 velocityXZ, Vector2 accelerationXZ) CalculateHorizontalOnFootPhysics(float delta, MovementSettings? settings = null)
	{
		settings ??= this.Character.Settings.Movement;
		return this.CalculateHorizontalPhysics(
			delta,
			settings.MaxSpeedUnPSec,
			settings.AccelerationUnPSecSq,
			settings.NormalDecelerationUnPSecSq,
			settings.BreakDecelerationUnPSecSq
		);
	}

	protected virtual (Vector2 velocityXZ, Vector2 accelerationXZ) CalculateHorizontalOnAirPhysics(float delta, MovementSettings? movementSettings = null, JumpSettings? jumpSettings = null)
	{
		movementSettings ??= this.Character.Settings.Movement;
		jumpSettings ??= this.Character.Settings.Jump;
		return this.CalculateHorizontalPhysics(
			delta,
			movementSettings.MaxSpeedUnPSec,
			movementSettings.AccelerationUnPSecSq * jumpSettings.AerialAccelerationMultiplier,
			movementSettings.NormalDecelerationUnPSecSq,
			movementSettings.BreakDecelerationUnPSecSq
		);
	}

	protected virtual (float velocityY, float accelerationY) CalculateVerticalOnFootPhysics()
	{
		if (!this.Character.IsOnFloor()) {
			this.Character.ApplyFloorSnap();
		}
		// Apply a small downward velocity to trigger collision with the floor so that this.Character.IsOnFloor() will
		// remain true while the character is on the floor
		return (-1, float.PositiveInfinity);
	}

	protected virtual (float velocityY, float accelerationY) CalculateVerticalOnAirPhysics(float delta, JumpSettings? jumpSettings = null)
	{
		jumpSettings ??= this.Character.Settings.Jump;
		float velocityY = jumpSettings.Gravity.MaxFallSpeedUnPSec * -1;
		float accelerationY = jumpSettings.Gravity.FallAccelerationUnPSecSq * delta;
		return (velocityY, accelerationY);
	}

	protected virtual float CalculateCameraRotationAngleDg() {
		return this.Character.GetViewport().GetCamera3D().Rotation.Y * -1;
	}

	/// <summary>
	/// Calculates ideal rotation angle of the character based on the user input and camera rotation.
	/// </summary>
	protected virtual float CalculateRotationAngleDg()
	{
		return this.Character.InputController.MovementInput.Length() > 0.01f
			? this.Character.InputController.MovementInput.Rotated(this.CalculateCameraRotationAngleDg())
				.AngleTo(Vector2.Up)
			: this.Character.Rotation.Y;
	}

	protected Vector3 CalculateRotationEuler()
	{
		return Vector3.Up * this.CalculateRotationAngleDg();
	}
}
