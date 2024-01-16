using Godot;

namespace Raele.SuperCharacter3D;

[GlobalClass]
public partial class GlideSettings : Resource
{
	[Export] public float MaxDurationSec = float.PositiveInfinity;
	[Export] public float HorizontalMaxSpeedMultiplier = 1f;
	[Export] public float HorizontalAccelerationMultiplier = 1f;
	[Export] public GravitySettings GlideFall = new();
}
