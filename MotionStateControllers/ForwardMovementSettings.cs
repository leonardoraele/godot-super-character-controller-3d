#nullable enable
using Godot;

namespace Raele.SuperCharacter3D.MotionStateControllers;

[GlobalClass]
public partial class ForwardMovementSettings : Resource
{
	[Export] public float MaxSpeedUnPSec { get; private set; } = 6;
	[Export] public float AccelerationUnPSecSq { get; private set; } = 20;
	[Export] public float NormalDecelerationUnPSecSq { get; private set; } = 20;
	[Export] public float BreakDecelerationUnPSecSq { get; private set; } = 20;

	[ExportGroup("On Enter")]
	[Export] public float InitialSpeedMultiplier { get; private set; } = 1;
	[Export] public float InitialSpeedBoostUnPSec { get; private set; } = 0;
	[Export] public InitialFacingDirectionEnum InitialFacingDirection = InitialFacingDirectionEnum.NoChange;

	public enum InitialFacingDirectionEnum {
		NoChange,
		InputDirection,
	}

    internal void OnEnter(SuperCharacter3DController character)
    {
		// Make the character face the input direction unless ForceForwardMotion is true
		if (
			this.InitialFacingDirection == InitialFacingDirectionEnum.InputDirection
			&& character.InputController.RawMovementInput.LengthSquared() >= 0.0001f
		) {
			character.Rotation = Vector3.Up
				* (
					character.InputController.RawMovementInput.AngleTo(Vector2.Down)
					+ GodotUtil.V3ToHV2(character.GetViewport().GetCamera3D().Basis.Z * -1).AngleTo(Vector2.Up)
					// + this.GetViewport().GetCamera3D().Rotation.Y
				);
		}

		// Set initial velocity
		character.ForwardSpeed = character.ForwardSpeed * this.InitialSpeedMultiplier + this.InitialSpeedBoostUnPSec;
    }
}
