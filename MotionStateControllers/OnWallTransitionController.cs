using Godot;

namespace Raele.SuperCharacter3D.MotionStateControllers;

public partial class OnWallTransitionController : MotionStateController
{
	[Export] public bool NotOnWall;
	[Export] public string? NextState;

	public override void OnProcessStateActive(float delta)
	{
		base.OnProcessStateActive(delta);
		if (!string.IsNullOrEmpty(this.NextState) && this.Character.IsOnWall() != this.NotOnWall) {
			this.StateMachine.Transition(this.NextState);
		}
	}
}
