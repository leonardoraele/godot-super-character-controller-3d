using Godot;

namespace Raele.SuperCharacter3D.MotionStates;

public partial class JumpCanceledState : BaseMotionState
{
    public override void OnProcessState(float delta)
    {
        base.OnProcessState(delta);
        if (this.Character.Velocity.Y <= 0)
        {
            this.Character.StateMachine.Transition<FallingState>();
        }
    }

    public override HorizontalMovement GetHorizontalMovement()
    {
        HorizontalMovement hMovement = this.Character.CalculateHorizontalMovement();
        hMovement.TargetSpeedUnPSec *= this.Character.Settings.Jump.AerialSpeedMultiplier;
        hMovement.AccelerationUnPSecSq *= this.Character.Settings.Jump.AerialAccelerationMultiplier;
        return hMovement;
    }

    public override VerticalMovement GetVerticalMovement()
        => new() {
            TargetVerticalSpeed = float.NegativeInfinity,
            Acceleration = this.Character.Settings.Jump.Gravity.FallAccelerationUnPSecSq * this.Character.Settings.Jump.JumpCancelAccelerationMultiplier,
        };
}
