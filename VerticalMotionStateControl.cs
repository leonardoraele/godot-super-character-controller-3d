using Godot;
using Raele.SuperCharacter3D.MotionStates;

namespace Raele.SuperCharacter3D;

public abstract partial class VerticalMotionStateControl : Resource
{
	public abstract VerticalMovement GetVerticalMovement(BaseMotionState state);
}
