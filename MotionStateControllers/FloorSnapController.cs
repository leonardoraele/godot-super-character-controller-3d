using Godot;

namespace Raele.SuperCharacter3D.MotionStateControllers;

public partial class FloorSnapController : MotionStateController
{
    public override void OnPhysicsProcessStateActive(float delta)
    {
		this.Character.ApplyFloorSnap();
	}
}
