using System;
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

    public override void OnEnter(StateTransition transition)
    {
        if (this.Character.Settings.Crouch == null) {
            throw new Exception($"{nameof(CrouchState)}: {nameof(this.Character.Settings.Crouch)} settings is null. Did you forget to assign this property?");
        } else if (this.Character.CrouchShape == null) {
            throw new Exception($"{nameof(CrouchState)}: {nameof(this.Character.CrouchShape)} is null. Did you forget to assign this property?");
        }
        base.OnEnter(transition);
        this.IsHoldingCrouch = Input.IsActionPressed(this.Character.Settings.Input.CrouchHoldAction);
    }
    public override void OnExit(StateTransition transition)
    {
        base.OnExit(transition);
        if (!this.CanStandUp) {
            transition.Cancel();
            return;
        }
    }
    public override void OnProcessState(float delta)
    {
        base.OnProcessState(delta);
        if (this.Character.InputController.JumpInputBuffer.ConsumeInput()) {
            if (this.Character.Settings.Crouch?.CrouchJump != null) {
                this.Character.StateMachine.Transition(nameof(JumpingState), this.Character.Settings.Crouch.CrouchJump.GetInstanceId());
            } else {
                this.Character.StateMachine.Transition(nameof(JumpingState));
            }
        } else if (
            this.IsHoldingCrouch && !Input.IsActionPressed(this.Character.Settings.Input.CrouchHoldAction)
            || Input.IsActionJustPressed(this.Character.Settings.Input.CrouchToggleAction)
        ) {
            this.Character.StateMachine.Reset();
        }
    }
    public override void OnPhysicsProcessState(float delta)
    {
        base.OnPhysicsProcessState(delta);
        HorizontalMovement horizontal = this.Character.CalculateOnAirHorizontalMovement();
        this.Character.ApplyHorizontalMovement(horizontal with {
            TargetSpeedUnPSec = horizontal.TargetSpeedUnPSec * this.Character.Settings.Crouch?.VelocityModifier ?? 1,
            AccelerationUnPSecSq = horizontal.AccelerationUnPSecSq * this.Character.Settings.Crouch?.AccelerationyModifier ?? 1,
        });
        this.Character.ApplyVerticalMovement(this.Character.CalculateOnFootVerticalMovement());
		this.Character.MoveAndSlide();
    }
}
