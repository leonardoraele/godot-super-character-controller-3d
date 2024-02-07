using Godot;

namespace Raele.SuperCharacter3D.MotionStateControllers;

public partial class OnFloorTransitionController : MotionStateController
{
	[Export] public bool NotOnFloor;
	[Export] public string? NextState;

	public override void OnProcessStateActive(float delta)
	{
		base.OnProcessStateActive(delta);
		if (!string.IsNullOrEmpty(this.NextState) && this.Character.IsOnFloor() != this.NotOnFloor) {
			this.StateMachine.Transition(this.NextState);
		}
	}
}
