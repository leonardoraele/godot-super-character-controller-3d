using Godot;

namespace Raele.SuperCharacterController3D;

[GlobalClass]
public partial class SuperPlatformer3DAirDashSettings : Resource
{
	[Export] public int Uses { get; private set; } = 1;
	[Export] public bool IgnoresGravity { get; private set; } = true;
}
