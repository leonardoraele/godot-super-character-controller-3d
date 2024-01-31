using Godot;

namespace Raele.SuperCharacter3D;

[GlobalClass]
public abstract partial class MotionStateController : Resource
{
	public virtual void OnEnter(ControlledState state, MotionStateTransition transition) {}
	public virtual void OnExit(ControlledState state, MotionStateTransition transition) {}
	public virtual void OnProcess(ControlledState state, float delta) {}
	public virtual void OnProcessStateActive(ControlledState state, float delta) {}
	public virtual void OnPhysicsProcessStateActive(ControlledState state, float delta) {}
}
