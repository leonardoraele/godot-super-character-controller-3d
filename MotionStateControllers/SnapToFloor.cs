using Godot;

namespace Raele.SuperCharacter3D.MotionStateControllers;

[GlobalClass]
public partial class SnapToFloor : MotionStateController
{
    public override void OnPhysicsProcessStateActive(ControlledState state, float delta)
    {
		state.Character.ApplyFloorSnap();
	}
}
