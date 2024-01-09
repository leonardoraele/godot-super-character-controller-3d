using Godot;

namespace Raele.SuperCharacter3D;

[GlobalClass]
public partial class AirDashSettings : Resource
{
	[Export] public int Uses { get; private set; } = 1;
	[Export] public bool IgnoresGravity { get; private set; } = true;
}
