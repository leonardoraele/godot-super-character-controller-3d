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

    public override void OnEnter(BaseMotionState.TransitionInfo transition)
    {
        base.OnEnter(transition);
        if (!this.Character.IsOnWall()) {
			this.Character.TransitionMotionState<FallingState>();
        }
        this.WallDirection = Math.Sign(this.Character.GetWallNormal().X) * -1;
    }
    public override void OnProcessState(float delta)
    {
        base.OnProcessState(delta);

        // If no longer on wall, drop off
        if (this.Character.Settings.Climb == null || !this.Character.IsOnWall()) {
			this.Character.TransitionMotionState<FallingState>();
            return;
		}

        // If player input is to drop from wall, drop off
        if (this.IsPlayerInputToDropFromWall) {
            if (this.WallDropInputTimeMs == 0) {
                this.WallDropInputTimeMs = Time.GetTicksMsec();
            } else if (this.Character.Settings.WallDropPreventionLeniencyMs <= 0
                || Time.GetTicksMsec() > this.WallDropInputTimeMs + this.Character.Settings.WallDropPreventionLeniencyMs
            ) {
                this.Character.TransitionMotionState<FallingState>();
                return;
            }
        } else {
            this.WallDropInputTimeMs = 0;
        }

        // If player input is to jump, jump off the wall with a starting kickoff speed
        if (this.Character.InputController.JumpInputBuffer.ConsumeInput()) {
            this.Character.Velocity = this.Character.Basis.Z * this.Character.Settings.Climb.JumpKickoffSpeedUnPSec;
            this.Character.TransitionMotionState<JumpingState>();
            return;
        }
    }
}
