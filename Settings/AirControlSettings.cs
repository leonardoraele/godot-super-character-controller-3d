using Godot;

namespace Raele.SuperPlatformer;

[GlobalClass]
public partial class AirControlSettings : Resource
{
	[Export] public float AerialHorizontalMaxSpeedPxPSec { get; private set; } = 200;
	[Export] public float AerialHorizontalAccelerationPxPSecSq { get; private set; } = 600;
    [Export] public float AerialNormalDecelerationPxPSecSq { get; private set; } = 400;
    [Export] public float AerialTurnDecelerationPxPSecSq { get; private set; } = 900;
}
