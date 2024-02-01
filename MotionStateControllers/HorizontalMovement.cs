using System;
using Godot;

namespace Raele.SuperCharacter3D.MotionStateControllers;

[GlobalClass]
public partial class HorizontalMovement : MotionStateController
{
	[Export] public float MaxSpeedUnPSec { get; private set; } = 6;
	[Export] public float AccelerationUnPSecSq { get; private set; } = 20;
	[Export] public float NormalDecelerationUnPSecSq { get; private set; } = 20;
	[Export] public float BreakDecelerationUnPSecSq { get; private set; } = 20;

	[ExportGroup("On Enter")]
	[Export] public float InitialSpeedMultiplier { get; private set; } = 1;
	[Export] public float InitialSpeedBoostUnPSec { get; private set; } = 0;
	[Export] public InitialFacingDirectionEnum InitialFacingDirection = InitialFacingDirectionEnum.NoChange;

	[ExportGroup("Turn Speed")]
	/// <summary>
	/// Maximum rotational speed at which the player turn their facing position, in degrees per second.
	///
	/// By default, this property is set to Infinity, which means the player can turn instantly to any direction.
	///
	/// If you set this to 0, or the player won't be able to change the character's facing direction. They will only be
	/// able to move forward, backward, or strafe sideways.
	///
	/// If you set this to a negative value, the player will rotate in the opposite direction of the input.
	/// </summary>
	[Export] public float TurnSpeedDegPSec = 720;
	/// <summary>
	/// Curve that modifies the turn speed based on the current speed of the character. Use this to make the character
	/// turn faster or slower depending on their speed.
	///
	/// The X represents the speed of the character, from 0 to the value set in <see cref="MaxSpeedUnPSec"/>. That is,
	/// the curve will be sampled close to X = 0 as the character's speed approaches 0, and at X = 1 when the character
	/// is moving at their maximum speed. The Y axis represents the multiplier to apply to the turn speed. Y values
	/// higher than 1 make the character turn faster and lower than 1 makes the character turn slower. If Y is 0, the
	/// character can't turn. For example, if Y = 0.5 at X = 1, the character will turn at half their normal
	/// turn speed (as determined by <see cref="TurnSpeedDegPSec"/>) when moving at their max speed (as determined
	/// by <see cref="MaxSpeedUnPSec"/>).
	///
	/// If this property not set, the turn speed is constant at all speed values.
	///
	/// Note that, if <see cref="TurnSpeedDegPSec"/> is set to Infinity (the default), this property has no effect.
	/// </summary>
	[Export] public Curve TurnSpeedModifier = null!;

	[ExportGroup("Velocity Loss On Turn")]
	/// <summary>
	/// Turn angle at which the character will start to lose velocity.
	///
	/// Set to 360 to disable.
	///
	/// Note: Velocity loss on turn might be detrimental to player experience, since it reduces their authonomy over the
	/// control of the character. This option is only recommended for games with slow pace or with "cinamic" aesthetics;
	/// and even then, it is important to animate the character properly to convey the sense of a thigh turn. If done
	/// correctly, it improves the sense of weight of the character.
	/// </summary>
	[Export(PropertyHint.Range, "0,180,15")] public float HarshTurnBeginAngleDeg = 120;
	/// <summary>
	/// Turn angle at which the character will lose all velocity to turn.
	/// Set to 360 to disable.
	/// </summary>
	[Export(PropertyHint.Range, "0,180,15")] public float HarshTurnMaxAngleDeg = 150f;
	/// <summary>
	/// Factor by which the character will lose velocity when turning. This is a power expoent on top of the proportion
	/// of the turn angle between <see cref="HarshTurnBeginAngleDeg"/> and <see cref="HarshTurnMaxAngleDeg"/>.
	/// Values higher than 1 will make the character lose more velocity with greater turn angles.
	/// </summary>
	[Export(PropertyHint.ExpEasing)] public float HarshTurnVelocityLossFactor = 1f;

	[ExportGroup("Debug")]
	[Export(PropertyHint.Flags, "Draw Velocity:1,Draw Input:2")] public int DebugDrawFlags;

	private float HarshTurnMaxAngleRad {
		get => Mathf.DegToRad(this.HarshTurnMaxAngleDeg);
		set => this.HarshTurnMaxAngleDeg = Mathf.RadToDeg(value);
	}
	private float HarshTurnBeginAngleRad {
		get => Mathf.DegToRad(this.HarshTurnBeginAngleDeg);
		set => this.HarshTurnBeginAngleDeg = Mathf.RadToDeg(value);
	}
	public float TurnSpeedRadPSec {
		get => Mathf.DegToRad(this.TurnSpeedDegPSec);
		set => this.TurnSpeedDegPSec = Mathf.RadToDeg(value);
	}

	public enum MovementModeEnum {
		DirectionalMovement = 1,
		DirectionalRotation = 2,
		SidewayStrafe = 4,
		ForwardAndBack = 8,
		TurnAround = 16,
	}

	public enum InitialFacingDirectionEnum {
		NoChange,
		InputDirection,
	}

	public override void OnEnter(ControlledState state, MotionStateTransition transition)
	{
		// Make the character face the input direction unless ForceForwardMotion is true
		if (
			this.InitialFacingDirection == InitialFacingDirectionEnum.InputDirection
			&& state.Character.InputController.MovementInput.LengthSquared() >= 0.0001f
		) {
			state.Character.Rotation = Vector3.Up
				* (
					state.Character.InputController.MovementInput.AngleTo(Vector2.Up)
					+ GodotUtil.V3ToHV2(state.Character.GetViewport().GetCamera3D().Basis.Z * -1).AngleTo(Vector2.Up)
					// + this.GetViewport().GetCamera3D().Rotation.Y
				);
		}

		// Set initial velocity
		state.Character.Velocity = state.Character.Basis.Z * -1
			* (
				state.Character.Velocity.Length()
					* this.InitialSpeedMultiplier
					+ this.InitialSpeedBoostUnPSec
			);
	}

    public override void OnPhysicsProcessStateActive(ControlledState state, float delta)
	{
		if ((this.DebugDrawFlags & 2) != 0) {
			DebugDraw3D.DrawArrow(state.Character.GlobalPosition, state.Character.GlobalPosition + state.Character.InputController.GlobalMovementInput, Colors.Green);
		}
		float turnAngleRad = state.Character.InputController.GlobalMovementInput.LengthSquared() > Mathf.Epsilon
			&& state.Character.Velocity.LengthSquared() > Mathf.Epsilon
			? state.Character.InputController.GlobalMovementInput.AngleTo(state.Character.Velocity)
			: 0;
		if (turnAngleRad > this.HarshTurnMaxAngleRad) {
			state.Character.ForwardSpeed = Mathf.MoveToward(
				state.Character.ForwardSpeed,
				0,
				this.BreakDecelerationUnPSecSq * delta
			);
		} else {
			float turnSpeedRadPSec = Math.Abs(state.Character.ForwardSpeed) <= Mathf.Epsilon
				? float.PositiveInfinity
				: this.TurnSpeedRadPSec
				* (
					// Avoid multiplying by Infinity because it might result in NaN if the multiplier is 0.
					!float.IsInfinity(this.TurnSpeedRadPSec) && this.MaxSpeedUnPSec != 0 && this.TurnSpeedModifier != null
						? this.TurnSpeedModifier.Sample(Mathf.Clamp(state.Character.ForwardSpeed / this.MaxSpeedUnPSec, 0, 1))
						: 1
				);
			float targetSpeedUnPSec = state.Character.InputController.GlobalMovementInput.Length()
				* this.MaxSpeedUnPSec
				* (
					turnAngleRad > this.HarshTurnBeginAngleRad
						? Mathf.Pow(
							1 - (turnAngleRad - this.HarshTurnBeginAngleRad) / (this.HarshTurnMaxAngleRad - this.HarshTurnBeginAngleRad),
							this.HarshTurnVelocityLossFactor
						)
						: 1
				);
			float accelerationUnPSecSq = targetSpeedUnPSec >= state.Character.ForwardSpeed ? this.AccelerationUnPSecSq
				: Math.Abs(targetSpeedUnPSec - this.MaxSpeedUnPSec) < 0.01f ? this.NormalDecelerationUnPSecSq
				: this.BreakDecelerationUnPSecSq;
			state.Character.ForwardSpeed = Mathf.MoveToward(state.Character.ForwardSpeed, targetSpeedUnPSec, accelerationUnPSecSq * delta);
			if (state.Character.InputController.GlobalMovementInput.LengthSquared() > Mathf.Epsilon) {
				state.Character.RotateTowardDirection(state.Character.InputController.GlobalMovementInput, turnSpeedRadPSec * delta, true);
			}
		}
		if ((this.DebugDrawFlags & 1) != 0) {
			DebugDraw3D.DrawArrow(state.Character.GlobalPosition, state.Character.GlobalPosition + state.Character.Velocity, Colors.Red);
		}
		state.Character.MoveAndSlide();
	}
}
