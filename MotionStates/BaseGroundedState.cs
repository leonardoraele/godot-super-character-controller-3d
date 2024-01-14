namespace Raele.SuperCharacter3D.MotionStates;

public abstract partial class BaseGroundedState : BaseMotionState
{
    public override void OnProcessState(float delta)
    {
        base.OnProcessState(delta);
        if (!this.Character.IsOnFloor()) {
            this.Character.StateMachine.Transition<FallingState>();
        }
    }
}
