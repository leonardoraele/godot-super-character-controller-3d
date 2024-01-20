using Godot;
using Raele.SuperCharacter3D.MotionStates;

namespace Raele.SuperCharacter3D;

public abstract partial class HorizontalMotionStateControl : Node
{
	public abstract HorizontalMovement GetHorizontalMovement();

	public BaseMotionState State { get; private set; } = null!;

    public override void _EnterTree()
    {
        base._EnterTree();
		if (this.GetParent() is BaseMotionState motionState) {
			this.State = motionState;
			// TODO
			// this.State.HorizontalControl = this;
		}
    }
}
