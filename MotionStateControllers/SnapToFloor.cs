using Godot;

namespace Raele.SuperCharacter3D.MotionStateControllers;

[GlobalClass]
public partial class SnapToFloor : MotionStateController
{
    public override void OnPhysicsProcessStateActive(float delta)
    {
		this.Character.ApplyFloorSnap();
	}
}
