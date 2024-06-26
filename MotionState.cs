#nullable enable
using Godot;

namespace Raele.SuperCharacter3D;

public interface IMotionState {
	public StringName Name { get; }
	public Error EmitSignal(StringName signalName, params Variant[] args);
    /// <summary>
    /// Called when this becomes the active state.
    /// </summary>
	public void OnEnter(MotionStateTransition transition);
	/// <summary>
	/// Called when this is cases to be the active state.
	/// </summary>
	public void OnExit(MotionStateTransition transition);
	/// <summary>
	/// Called every frame (on _Process) while this state is active.
	/// Not called if this is not the activestate.
	/// If you need to read input while this state is not active, do it in <code>Node._Process</code> instead. This is
	/// usually necessary if you want to buffer input that might be relevant soon (e.g. doouble tapping tro dash) or if
	/// you want to check a condition that might cause a state transition from any other state to this state.
	/// </summary>
	public void OnProcessStateActive(float delta);
	/// <summary>
	/// Called every physics tick (on _PhysicsProcess) while this state is active.
	/// Not called if this is not the activestate.
	/// </summary>
	public void OnPhysicsProcessStateActive(float delta);
}
