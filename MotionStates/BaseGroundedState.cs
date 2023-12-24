namespace Raele.SuperCharacterController3D.MotionStates;

public abstract partial class BaseGroundedState : BaseMotionState
{
    public override void OnProcessState(float delta)
    {
        base.OnProcessState(delta);
        if (!this.Character.IsOnFloor()) {
            this.Character.TransitionMotionState<FallingState>();
        }
    }
}
