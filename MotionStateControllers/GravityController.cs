#nullable enable
using Godot;

namespace Raele.SuperCharacter3D.MotionStateControllers;

public partial class GravityController : MotionStateController
{
    [Export] public GravitySettings Settings = null!;

    public override void OnPhysicsProcessStateActive(float delta)
    {
        this.Character.VerticalSpeed = Mathf.MoveToward(
            this.Character.VerticalSpeed,
            this.Settings.MaxFallSpeedUnPSec * -1,
            this.Settings.FallAccelerationUnPSecSq * delta
        );
    }
}
