namespace Raele.SuperPlatformer;

public abstract partial class GroundedState : MotionState
{
	public override void OnEnter(TransitionInfo transition)
	{
		base.OnEnter(transition);
		this.Character.MotionMode = Godot.CharacterBody2D.MotionModeEnum.Grounded;
	}

    public override void OnProcessState(float delta)
    {
        base.OnProcessState(delta);
        if (!this.Character.IsOnFloor()) {
            this.Character.TransitionMotionState<FallingState>();
        }
    }
}
