using Godot;

namespace Raele.SuperCharacter3D.MotionStateControllers;

[GlobalClass]
public partial class Gravity : MotionStateController
{
    [Export] public float MaxFallSpeedUnPSec { get; private set; } = 20;
    [Export] public float FallAccelerationUnPSecSq { get; private set; } = 30;

    public override void OnPhysicsProcessStateActive(float delta)
    {
        this.Character.VerticalSpeed = Mathf.MoveToward(
            this.Character.VerticalSpeed,
            this.MaxFallSpeedUnPSec * -1,
            this.FallAccelerationUnPSecSq * delta
        );
    }
}
