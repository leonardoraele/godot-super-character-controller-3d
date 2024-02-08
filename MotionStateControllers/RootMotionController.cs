using Godot;

namespace Raele.SuperCharacter3D.MotionStateControllers;

public partial class RootMotionController : MotionStateController
{
	[Export] public Node3D? AnimatedNode;

    public override void OnPhysicsProcessStateActive(float delta)
    {
        base.OnPhysicsProcessStateActive(delta);
		if (this.AnimatedNode == null) {
			return;
		}
		(this.Character.Velocity, this.AnimatedNode.Position) = (this.AnimatedNode.Position, Vector3.Zero);
    }
}
