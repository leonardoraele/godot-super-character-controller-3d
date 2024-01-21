using Godot;
using Raele.SuperCharacter3D.MotionStates;

namespace Raele.SuperCharacter3D;

public abstract partial class HorizontalMotionStateControl : Resource
{
	public abstract HorizontalMovement GetHorizontalMovement(BaseMotionState state);
}
