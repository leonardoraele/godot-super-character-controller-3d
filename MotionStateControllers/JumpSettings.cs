using Godot;

namespace Raele.SuperCharacter3D.MotionStateControllers;

[GlobalClass]
public partial class JumpSettings : Resource
{
	[Export] public float JumpHeightUn { get; private set; } = 3.5f;
	[Export] public ulong JumpDurationMs { get; private set; } = 500;
	[Export] public Curve? JumpHeightCurve;
    [ExportGroup("Assist Options")]
	[Export] public ulong CeilingSlideLeniencyMs { get; private set; } = 150;
}
