using Godot;

namespace Raele.SuperCharacter3D.MotionStates;

public partial class CrouchState : WalkState
{
    public bool CanStandUp => !this.Character.IsOnCeiling()
        && this.Character.StandUpShape != null
        && this.Character.CrouchShape != null
        && this.Character.MoveAndCollide(
                Vector3.Up * (
                    this.Character.StandUpShape.Shape.GetDebugMesh().GetAabb().End.Y
                    - this.Character.CrouchShape.Shape.GetDebugMesh().GetAabb().End.Y
                ),
                true
            )
            == null;

    public override void OnExit(MotionStateTransition transition)
    {
        if (!this.CanStandUp) {
            transition.Cancel();
            return;
        }
        base.OnExit(transition);
    }
}
