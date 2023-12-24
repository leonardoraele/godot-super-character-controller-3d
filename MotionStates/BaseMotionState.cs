using System;
using Godot;

namespace Raele.SuperCharacterController3D.MotionStates;

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
    protected SuperCharacterController3D Character { get; private set; } = null!;

	public ulong DurationActiveMs => Time.GetTicksMsec() - this.ActivationTime;

    public override void _EnterTree()
    {
        base._EnterTree();
		if (this.GetParent() is SuperCharacterController3D character) {
			this.Character = character;
		} else {
			GD.PushError($"MotionState node of type \"{this.GetType().Name}\" must be a child of {typeof(SuperCharacterController3D).Name}");
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

	private (Vector2 velocityXZ, Vector2 accelerationXZ) CalculateHorizontalPhysics(
		float delta,
		float maxSpeedUnPSec,
		float accelerationUnPSecSq
	) {
		Vector2 velocityXZ = maxSpeedUnPSec * this.Character.InputController.MovementInput.Normalized();
		return (velocityXZ, Vector2.One * accelerationUnPSecSq * delta);
	}

	protected virtual (Vector2 velocityXZ, Vector2 accelerationXZ) CalculateHorizontalOnFootPhysics(float delta)
	{
		return this.CalculateHorizontalPhysics(
			delta,
			this.Character.Settings.Movement.MaxSpeedUnPSec,
			this.Character.Settings.Movement.AccelerationUnPSecSq
		);
	}

	protected virtual (Vector2 velocityXZ, Vector2 accelerationXZ) CalculateHorizontalOnAirPhysics(float delta)
	{
		return this.CalculateHorizontalPhysics(
			delta,
			/*this.Character.Settings.Jump.AirControlSettings?.AerialHorizontalMaxSpeedPxPSec
				??*/ this.Character.Settings.Movement.MaxSpeedUnPSec,
			/*this.Character.Settings.Jump.AirControlSettings?.AerialHorizontalAccelerationPxPSecSq
				??*/ this.Character.Settings.Movement.AccelerationUnPSecSq
		);
	}

	protected virtual (float velocityY, float accelerationY) CalculateVerticalOnFootPhysics()
	{
		if (!this.Character.IsOnFloor()) {
			this.Character.ApplyFloorSnap();
		}
		return (
			this.Character.Settings.Movement.DownwardVelocityOnFloor * -1,
			float.PositiveInfinity
		);
	}

	protected virtual (float velocityY, float accelerationY) CalculateVerticalOnAirPhysics(float delta)
	{
		float velocityY = this.Character.Settings.Jump.MaxFallSpeedUnPSec * -1;
		float accelerationY = this.Character.Settings.Jump.FallAccelerationUnPSecSq * delta;
		return (velocityY, accelerationY);
	}
}
