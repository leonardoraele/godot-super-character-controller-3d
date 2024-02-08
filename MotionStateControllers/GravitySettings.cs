using Godot;

namespace Raele.SuperCharacter3D.MotionStateControllers;

[GlobalClass]
public partial class GravitySettings : Resource
{
	[Export] public float MaxFallSpeedUnPSec { get; private set; } = 20;
	[Export] public float FallAccelerationUnPSecSq { get; private set; } = 30;
}
