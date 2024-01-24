using System;
using Godot;

namespace Raele.SuperCharacter3D.MotionStates;

public partial class WallSlideState : BaseMotionState
{
	[Export] public float MaxDownwardSpeedPSec = 2.5f;
	[Export] public float DownwardAccelerationUnPSecSq = 1.5f;
    [Export] public bool CancelDownwardMomentumOnEnter = false;
    [Export] public WallStateUtil.FacingDirectionMode FacingDirection = WallStateUtil.FacingDirectionMode.AwayFromWall;

    [ExportGroup("Wall Drop")]
    [Export] public float WallDropMinInputAngleDeg = 90f;
    [Export] public int WallDropPreventionLeniencyMs = 150;

    [ExportGroup("Upward Momentum")]
	[Export] public float MaxUpwardSpeedPSec = float.PositiveInfinity;
	[Export] public float UpwardDecelerationUnPSecSq = 4f;
	[Export] public float UpwardDecelerationLerpFactorPSec = 4f;

    private ulong WallDropOffInputStartTimestamp;

    public bool IsPlayerInputToDropFromWall =>
        this.Character.InputController.MovementInput.Length() < 0.01f
        ? this.WallDropPreventionLeniencyMs < 0
        : Math.Abs(
                GodotUtil.V3ToHV2(this.Character.GetWallNormal())
                    .AngleTo(this.Character.InputController.GetRelativeMovementInput())
            )
            < Mathf.DegToRad(this.WallDropMinInputAngleDeg);

    public override void OnEnter(StateTransition transition)
    {
        base.OnEnter(transition);
        if (transition.Canceled) {
            return;
        }
        this.Character.Rotation = Vector3.Up
            * WallStateUtil.GetTargetFacingDirection(this.FacingDirection, this.Character)
                .AngleTo(Vector2.Up);
        this.Character.Velocity = new Vector3(
            this.Character.Velocity.X,
            this.Character.Velocity.Y < 0 && this.CancelDownwardMomentumOnEnter
                ? 0
                : Math.Clamp(this.Character.Velocity.Y, this.MaxDownwardSpeedPSec, this.MaxUpwardSpeedPSec),
            this.Character.Velocity.Z
        );
        this.WallDropOffInputStartTimestamp = 0;
    }

    public override void OnProcessStateActive(float delta)
    {
         base.OnProcessStateActive(delta);

        // If player input to drop from wall for enough time, drop off
        if (this.IsPlayerInputToDropFromWall) {
            if (this.WallDropOffInputStartTimestamp == 0) {
                this.WallDropOffInputStartTimestamp = Time.GetTicksMsec();
            }
            if (this.WallDropPreventionLeniencyMs <= 0
                || Time.GetTicksMsec()
                    > this.WallDropOffInputStartTimestamp + (ulong) Math.Max(0, this.WallDropPreventionLeniencyMs)
            ) {
                if (this.StateTransitionWhenNotOnWall != null) {
                    this.Character.StateMachine.Transition(this.StateTransitionWhenNotOnWall.Name);
                }
            }
        } else {
            this.WallDropOffInputStartTimestamp = 0;
        }
    }

    public override VerticalMovement GetVerticalMovement()
        => new() {
            TargetVerticalSpeed = this.MaxDownwardSpeedPSec * -1,
            Acceleration = this.Character.Velocity.Y > 0.6f
                ? this.UpwardDecelerationUnPSecSq + this.Character.Velocity.Y * this.UpwardDecelerationLerpFactorPSec
                : this.DownwardAccelerationUnPSecSq,
        };
}
