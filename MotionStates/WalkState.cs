using System;
using Godot;

namespace Raele.SuperCharacter3D.MotionStates;

public partial class WalkState : BaseMotionState
{
	[Export] public float MaxSpeedUnPSec { get; private set; } = 6;
	[Export] public float AccelerationUnPSecSq { get; private set; } = 20;
	[Export] public float DecelerationUnPSecSq { get; private set; } = 20;
	[Export] public Node? FallState;

	[ExportGroup("Turn Speed")]
	/// <summary>
	/// Maximum rotational speed at which the player turn their facing position, in degrees per second.
	///
	/// If set to a very high value (e.g. 999999), the player will be able to turn instantly to any direction. Do not
	/// set it to 0, or the character will only be able to move forward.
	/// </summary>
	[Export] public float TurnSpeedDgPSec = 540;
	/// <summary>
	/// Curve that modifies the turn speed based on the current speed of the character. The X axis represents the speed
	/// of the character, from 0 to the value set in <see cref="MaxSpeedUnPSec"/>; and the Y axis represents the
	/// multiplier to apply to the turn speed. If not set, the turn speed is constant.
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
	[Export(PropertyHint.Range, "0,180,15")] public float HarshTurnBeginAngleDg = 120;
	/// <summary>
	/// Turn angle at which the character will lose all velocity to turn.
	/// Set to 360 to disable.
	/// </summary>
	[Export(PropertyHint.Range, "0,180,15")] public float HarshTurnMaxAngleDg = 150;
	/// <summary>
	/// Factor by which the character will lose velocity when turning. This is a power expoent on top of the proportion
	/// of the turn angle between <see cref="HarshTurnBeginAngleDg"/> and <see cref="HarshTurnMaxAngleDg"/>.
	/// Values higher than 1 will make the character lose more velocity with greater turn angles.
	/// </summary>
	[Export(PropertyHint.ExpEasing)] public float HarshTurnVelocityLossFactor = 1f;
	/// <summary>
	/// If the player tries to turn an angle higher than <see cref="HarshTurnMaxAngleDg"/>, the character will retain it's
	/// velocity and this deceleration will be applied until the character comes to a complete halt. Then, they can
	/// turn.
	/// </summary>
	[Export] public float _180TurnDecelerationUnPSecSq = 40;

    public override void OnProcessStateActive(float delta)
    {
		base.OnProcessStateActive(delta);
		if (this.FallState != null && !this.Character.IsOnFloor()) {
			this.StateMachine.Transition(this.FallState.Name);
		}
		// if (this.Character.InputController.JumpInputBuffer.ConsumeInput()) {
		// 	this.Character.StateMachine.Transition<JumpingState>();
		// } else
		// if (this.Character.InputController.DashInputBuffer.ConsumeInput()) {
		// 	this.Character.StateMachine.Transition<GroundDashingState>();
		// } else
		// if (
		// 	Input.IsActionJustPressed(this.Character.Settings.Input.CrouchToggleAction)
		// 	|| Input.IsActionJustPressed(this.Character.Settings.Input.CrouchHoldAction)
		// ) {
		// 	if (
		// 		this.Character.Settings.Crouch?.Slide != null
		// 		&& this.Character.Velocity.Length() + 0.01f >= this.Character.Settings.Crouch.Slide.MinSpeedUnPSec
		// 	) {
		// 		this.Character.StateMachine.Transition<SlideState>();
		// 	} else {
		// 		this.Character.StateMachine.Transition<CrouchState>();
		// 	}
		// }
    }

    public override HorizontalMovement GetHorizontalMovement()
	{
		Vector2 inputDirection = this.Character.InputController.MovementInput
			.Rotated(this.GetViewport().GetCamera3D().Rotation.Y * -1);
		float currentSpeedUnPSec = GodotUtil.V3ToHV2(this.Character.Velocity).Length();
		float turnSpeedDgPSec = currentSpeedUnPSec < 0.01f
			? float.PositiveInfinity
			: this.TurnSpeedDgPSec
			* (
				this.MaxSpeedUnPSec != 0 && this.TurnSpeedModifier != null
					? this.TurnSpeedModifier.Sample(Mathf.Min(1, currentSpeedUnPSec / this.MaxSpeedUnPSec))
					: 1
			);
		float targetSpeedUnPSec = inputDirection.Length() * this.MaxSpeedUnPSec;
		float turnAngleDg = targetSpeedUnPSec > 0.01f && currentSpeedUnPSec > 0.01f
			? Math.Abs(Mathf.RadToDeg(GodotUtil.V3ToHV2(this.Character.Velocity).AngleTo(inputDirection)))
			: 0;
		if (turnAngleDg > this.HarshTurnMaxAngleDg) {
			return new HorizontalMovement {
				TargetDirection = GodotUtil.V3ToHV2(this.Character.Velocity).Normalized(),
				TargetSpeedUnPSec = 0,
				AccelerationUnPSecSq = this._180TurnDecelerationUnPSecSq
			};
		} else if (turnAngleDg > this.HarshTurnBeginAngleDg) {
			float velocityMultiplier = Mathf.Pow(
				1 - (turnAngleDg - this.HarshTurnBeginAngleDg) / (this.HarshTurnMaxAngleDg - this.HarshTurnBeginAngleDg),
				this.HarshTurnVelocityLossFactor
			);
			return new HorizontalMovement {
				TargetDirection = inputDirection,
				RotationSpeedDegPSec = turnSpeedDgPSec,
				TargetSpeedUnPSec = currentSpeedUnPSec * velocityMultiplier,
				AccelerationUnPSecSq = float.PositiveInfinity
			};
		}
		float accelerationUnPSecSq = targetSpeedUnPSec > currentSpeedUnPSec
			? this.AccelerationUnPSecSq
			: this.DecelerationUnPSecSq;
		return new HorizontalMovement {
			TargetDirection = inputDirection,
			RotationSpeedDegPSec = turnSpeedDgPSec,
			TargetSpeedUnPSec = targetSpeedUnPSec,
			AccelerationUnPSecSq = accelerationUnPSecSq
		};
	}

	public override VerticalMovement GetVerticalMovement()
		=> new() { SnapToFloor = true };
}
