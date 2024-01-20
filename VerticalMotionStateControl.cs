using Godot;
using Raele.SuperCharacter3D.MotionStates;

namespace Raele.SuperCharacter3D;

public abstract partial class VerticalMotionStateControl : Node
{
	public abstract VerticalMovement GetVerticalMovement();

	public BaseMotionState State { get; private set; } = null!;

    public override void _EnterTree()
    {
        base._EnterTree();
		if (this.GetParent() is BaseMotionState motionState) {
			this.State = motionState;
			// TODO
			// this.State.VerticalControl = this;
		}
    }
}
