using Godot;

namespace Raele.SuperCharacter3D.MotionStates;

public partial class CrouchState : BaseGroundedState
{
    private bool IsHoldingCrouch;

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

    public override void OnEnter(TransitionInfo transition)
    {
        base.OnEnter(transition);
        if (this.Character.Settings.Crouch == null) {
            GD.PushError($"{nameof(CrouchState)}: {nameof(this.Character.Settings.Crouch)} settings is null. Did you forget to assign this property?");
            transition.Cancel();
            return;
        } else if (this.Character.CrouchShape == null) {
            GD.PushError($"{nameof(CrouchState)}: {nameof(this.Character.CrouchShape)} is null. Did you forget to assign this property?");
            transition.Cancel();
            return;
        }
        this.IsHoldingCrouch = Input.IsActionPressed(this.Character.Settings.Input.CrouchHoldAction);
        this.Character.StandUpShape?.Set("disabled", true);
        this.Character.CrouchShape?.Set("disabled", false);
        this.Character.CrawlShape?.Set("disabled", true);
    }
    public override void OnExit(TransitionInfo transition)
    {
        base.OnExit(transition);
        if (!this.CanStandUp) {
            transition.Cancel();
            return;
        }
        this.Character.StandUpShape?.Set("disabled", false);
        this.Character.CrouchShape?.Set("disabled", true);
        this.Character.CrawlShape?.Set("disabled", true);
    }
    public override void OnProcessState(float delta)
    {
        base.OnProcessState(delta);
        if (this.Character.InputController.JumpInputBuffer.ConsumeInput()) {
            if (this.Character.Settings.Crouch?.CrouchJump != null) {
                this.Character.TransitionMotionState(nameof(JumpingState), this.Character.Settings.Crouch.CrouchJump.GetInstanceId());
            } else {
                this.Character.TransitionMotionState(nameof(JumpingState));
            }
        } else if (
            this.IsHoldingCrouch && !Input.IsActionPressed(this.Character.Settings.Input.CrouchHoldAction)
            || Input.IsActionJustPressed(this.Character.Settings.Input.CrouchToggleAction)
        ) {
            this.Character.ResetState();
        }
    }
    public override void OnPhysicsProcessState(float delta)
    {
		(Vector2 velocityXZ, Vector2 accelerationXZ) = this.CalculateHorizontalOnFootPhysics(delta);
        velocityXZ *= this.Character.Settings.Crouch?.VelocityModifier ?? 1;
        accelerationXZ *= this.Character.Settings.Crouch?.AccelerationyModifier ?? 1;
        (float velocityY, float accelerationY) = this.CalculateVerticalOnFootPhysics();
		this.Character.Accelerate(velocityXZ, velocityY, accelerationXZ, accelerationY);
		this.Character.MoveAndSlide();
        this.Character.Rotation = this.CalculateRotationEuler();
    }
}
