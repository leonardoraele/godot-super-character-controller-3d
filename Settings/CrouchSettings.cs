using Godot;

namespace Raele.SuperCharacter3D;

[GlobalClass]
public partial class CrouchSettings : Resource
{
	[Export] public float VelocityModifier = 0.5f;
	[Export] public float AccelerationyModifier = 0.5f;
	[Export] public JumpSettings? CrouchJump;
}
