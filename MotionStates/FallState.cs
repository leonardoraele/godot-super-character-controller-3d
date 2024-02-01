using Godot;

namespace Raele.SuperCharacter3D.MotionStates;

public partial class FallState : BaseMotionState
{
	[Export] public float MaxFallSpeedUnPSec { get; private set; } = 20;
	[Export] public float FallAccelerationUnPSecSq { get; private set; } = 30;
    [Export] public Node? OnFloorState;

    [ExportGroup("Horizontal Air Control")]
    [Export] public BaseMotionState? HorizontalMovementReference;
    [Export] public float AirAccelerationMultiplier = 1f;
    [Export] public float AirSpeedMultiplier = 1f;
    [Export] public float AirRotationSpeedMultiplier = 1f;

    public override void OnProcessStateActive(float delta)
    {
        base.OnProcessStateActive(delta);
        if (this.OnFloorState != null && this.Character.IsOnFloor()) {
            this.StateMachine.Transition(this.OnFloorState.Name);
        }
        //  else if (
        //     Input.IsActionPressed(this.Character.Settings.Input.JumpAction)
        //     // && this.Character.StateMachine.GetNode(nameof(GlideState)) is GlideState glideState
        //     // && glideState.CanGlide != null
        // ) {
        //     this.Character.StateMachine.Transition<GlideState>();
        // }
        // TODO
        // else if (
        //     this.Character.IsOnWall()
        //     && Math.Sign(this.Character.InputController.MovementInput.X) == Math.Sign(this.Character.GetWallNormal().X) * -1
        // ) {
        //     this.Character.State.Transition<WallClimbingState>();
        // }
    }

    public override HorizontalMovement GetHorizontalMovement()
    {
        HorizontalMovement hMovement = this.HorizontalMovementReference?.GetHorizontalMovement() ?? new();
        hMovement.TargetForwardSpeedUnPSec *= this.AirSpeedMultiplier;
        hMovement.ForwardAccelerationUnPSecSq *= this.AirAccelerationMultiplier;
        hMovement.RotationSpeedDegPSec *= this.AirRotationSpeedMultiplier;
        return hMovement;
    }

    public override VerticalMovement GetVerticalMovement()
        => new() {
            TargetVerticalSpeed = this.MaxFallSpeedUnPSec * -1,
            Acceleration = this.FallAccelerationUnPSecSq,
        };
}
