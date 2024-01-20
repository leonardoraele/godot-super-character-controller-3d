using System;
using Godot;

namespace Raele.SuperCharacter3D.MotionStates;

public partial class OnWallState : BaseMotionState
{
    private int WallDirection;
    private float WallDropInputTimeMs;

    public bool IsPlayerInputToDropFromWall => this.Character.Settings.WallDropPreventionLeniencyMs >= 0
        ? Math.Sign(this.Character.InputController.MovementInput.X) == this.WallDirection * -1
        : Math.Sign(this.Character.InputController.MovementInput.X) != this.WallDirection;

    public override void OnEnter(StateTransition transition)
    {
        base.OnEnter(transition);
        if (!this.Character.IsOnWall()) {
			this.Character.StateMachine.Transition<FallState>();
        }
        this.WallDirection = Math.Sign(this.Character.GetWallNormal().X) * -1;
    }
    public override void OnProcessStateActive(float delta)
    {
        base.OnProcessStateActive(delta);

        // If no longer on wall, drop off
        if (this.Character.Settings.Climb == null || !this.Character.IsOnWall()) {
			this.Character.StateMachine.Transition<FallState>();
            return;
		}

        // If player input is to drop from wall, drop off
        if (this.IsPlayerInputToDropFromWall) {
            if (this.WallDropInputTimeMs == 0) {
                this.WallDropInputTimeMs = Time.GetTicksMsec();
            } else if (this.Character.Settings.WallDropPreventionLeniencyMs <= 0
                || Time.GetTicksMsec() > this.WallDropInputTimeMs + this.Character.Settings.WallDropPreventionLeniencyMs
            ) {
                this.Character.StateMachine.Transition<FallState>();
                return;
            }
        } else {
            this.WallDropInputTimeMs = 0;
        }

        // If player input is to jump, jump off the wall with a starting kickoff speed
        if (this.Character.InputController.GetInputBuffer(this.Character.Settings.Input.JumpAction).ConsumeInput()) {
            this.Character.Velocity = this.Character.Basis.Z * this.Character.Settings.Climb.JumpKickoffSpeedUnPSec;
            this.Character.StateMachine.Transition<JumpState>();
            return;
        }
    }
}
