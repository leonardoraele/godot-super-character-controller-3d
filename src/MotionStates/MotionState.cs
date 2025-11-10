using System;
using Godot;

namespace Raele.SuperCharacterController2D;

public partial class MotionState : Node {
	public record TransitionInfo {
		public string? PreviousStateName { get; init; }
		public required string NextStateName { get; init; }
		public Variant? Data { get; init; }
		public required Action Cancel { get; init; }
	}

	/// <summary>
	/// The moment in time, in milisecond ticks, when this state was activated; or 0 if it is not active.
	/// </summary>
	public ulong ActivationTime { get; private set; } = 0;
    protected SuperCharacterController2D Character { get; private set; } = null!;

	public ulong DurationActiveMs => this.IsActive ? Time.GetTicksMsec() - this.ActivationTime : 0;
	public bool IsActive => this.Character.CurrentState == this;

    public override void _EnterTree() {
        base._EnterTree();
		if (this.GetParent() is SuperCharacterController2D character) {
			this.Character = character;
		} else {
			GD.PushError($"{nameof(MotionState)} node of type \"{this.GetType().Name}\" must be a child of {typeof(SuperCharacterController2D).Name}");
		}
		this.Name = this.GetType().Name;
	}

    /// <summary>
    /// Called when this becomes the active state.
    /// </summary>
    public virtual void OnEnter(TransitionInfo transition) {
		this.ActivationTime = Time.GetTicksMsec();
	}

	/// <summary>
	/// Called when this is cases to be the active state.
	/// </summary>
	public virtual void OnExit(TransitionInfo transition) {
		this.ActivationTime = 0;
	}

	/// <summary>
	/// Called every frame (on _Process) while this state is active.
	/// Not called if this is not the active state.
	/// If you need to read input while this state is not active, do it in <code>Node._Process</code> instead. This is
	/// usually necessary if you want to buffer input that might be relevant soon (e.g. doouble tapping tro dash) or if
	/// you want to check a condition that might cause a state transition from any other state to this state.
	/// </summary>
	public virtual void OnProcessState(float delta) {}

	/// <summary>
	/// Called every physics tick (on _PhysicsProcess) while this state is active.
	/// Not called if this is not the active state.
	/// </summary>
	public virtual void OnPhysicsProcessState(float delta) {}

	private (float velocityX, float accelerationX) CalculateHorizontalPhysics(
		float delta,
		float maxSpeedPxPSec,
		float accelerationPxPSecSq,
		float normalDecelerationPxPSecSq,
		float turnDecelerationPxPSecSq
	) {
		float velocityX = maxSpeedPxPSec * this.Character.InputController.MovementInput.X;
		float accelerationX =
			Math.Abs(velocityX) > Math.Abs(this.Character.Velocity.X) ? accelerationPxPSecSq * delta
			: this.Character.InputController.MovementInput.X == 1 * Math.Sign(this.Character.Velocity.X)
			? normalDecelerationPxPSecSq * delta
			: turnDecelerationPxPSecSq * delta;
		return (velocityX, accelerationX);
	}

	protected virtual (float velocityX, float accelerationX) CalculateHorizontalOnFootPhysics(float delta) {
		return this.CalculateHorizontalPhysics(
			delta,
			this.Character.MovementSettings.MaxHorizontalSpeedPxPSec,
			this.Character.MovementSettings.HorizontalAccelerationPxPSecSq,
			this.Character.MovementSettings.NormalDecelerationPxPSecSq,
			this.Character.MovementSettings.TurnDecelerationPxPSecSq
		);
	}

	protected virtual (float velocityX, float accelerationX) CalculateHorizontalOnAirPhysics(float delta) {
		return this.CalculateHorizontalPhysics(
			delta,
			this.Character.JumpSettings.AirControlSettings?.AerialHorizontalMaxSpeedPxPSec
				?? this.Character.MovementSettings.MaxHorizontalSpeedPxPSec,
			this.Character.JumpSettings.AirControlSettings?.AerialHorizontalAccelerationPxPSecSq
				?? this.Character.MovementSettings.HorizontalAccelerationPxPSecSq,
			this.Character.JumpSettings.AirControlSettings?.AerialNormalDecelerationPxPSecSq
				?? this.Character.MovementSettings.NormalDecelerationPxPSecSq,
			this.Character.JumpSettings.AirControlSettings?.AerialTurnDecelerationPxPSecSq
				?? this.Character.MovementSettings.TurnDecelerationPxPSecSq
		);
	}

	protected virtual (float velocityY, float accelerationY) CalculateVerticalOnFootPhysics() {
		if (this.Character.IsOnFloor()) {
			float angle = this.Character.GetFloorNormal().Angle() - Vector2.Up.Angle();
			// If is going down a slope
			if (this.Character.IsFacingLeft && angle < -0.01f || this.Character.IsFacingRight && angle > 0.01f) {
				return (this.Character.MovementSettings.DownwardVelocityOnFloor, float.PositiveInfinity);
				// velocity = velocity.Rotated(angle);
				// acceleration = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
				// // acceleration = new Vector2(acceleration.X, acceleration.X).Normalized();
				// GD.PrintS(new { angle, Velocity, velocity, acceleration });
			// If going up a slope
			} else {
				return (0, float.PositiveInfinity);
			}
		} else {
			this.Character.ApplyFloorSnap();
			return (0, float.PositiveInfinity);
		}
	}

	protected virtual (float velocityY, float accelerationY) CalculateVerticalOnAirPhysics()
	{
		float velocityY = this.Character.JumpSettings.MaxFallSpeedPxPSec;
		float accelerationY = this.Character.JumpSettings.FallAccelerationPxPSecSq;
		return (velocityY, accelerationY);
	}
}
