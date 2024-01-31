using Godot;

namespace Raele.SuperCharacter3D.MotionStateControllers;

[GlobalClass]
public partial class Gravity : MotionStateController
{
    [Export] public float MaxFallSpeedUnPSec { get; private set; } = 20;
    [Export] public float FallAccelerationUnPSecSq { get; private set; } = 30;

    public override void OnPhysicsProcessStateActive(ControlledState state, float delta)
    {
        state.Character.ApplyVerticalMovement(new() {
            TargetVerticalSpeed = this.MaxFallSpeedUnPSec * -1,
            Acceleration = this.FallAccelerationUnPSecSq,
        });
    }
}
