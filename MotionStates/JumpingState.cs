using System;
using Godot;

namespace Raele.SuperCharacter3D.MotionStates;

public partial class JumpingState : BaseAirState
{
    public ulong? TimeHitCeiling;
    private JumpSettings Settings = null!;

    public override void OnEnter(StateTransition transition)
    {
        base.OnEnter(transition);
        this.Settings = transition.Data.HasValue && transition.Data.Value.AsUInt64() != 0
            ? GodotUtil.GetResource<JumpSettings>(transition.Data.Value.AsUInt64())
            : this.Character.Settings.Jump;
    }

    public override void OnProcessState(float delta)
    {
        base.OnProcessState(delta);
        // Transition check: Jump -> Jump Canceled
        if (
            this.DurationActiveMs >= this.Settings.JumpDurationMs
            || this.TimeHitCeiling != null
            && (
                !this.Character.SlideOnCeiling
                || Time.GetTicksMsec() >= this.TimeHitCeiling.Value + this.Character.Settings.CeilingSlideTimeMs
            )
        ) {
            this.Character.StateMachine.Transition<FallingState>();
        } else if (this.DurationActiveMs >= this.Settings.MinJumpDurationMs
            && !Input.IsActionPressed(this.Character.Settings.Input.JumpAction)
        ) {
            this.Character.StateMachine.Transition<JumpCanceledState>(this.Settings.GetInstanceId());
        }
    }

    public override void OnPhysicsProcessState(float delta)
    {
        // Apply horizontal velocity
        HorizontalMovement hMovement = this.Character.CalculateHorizontalMovement();
        hMovement.TargetSpeedUnPSec *= this.Settings.AerialSpeedMultiplier;
        hMovement.AccelerationUnPSecSq *= this.Settings.AerialAccelerationMultiplier;
        this.Character.ApplyHorizontalMovement(hMovement);

        // Calculate vertical velocity
        // TODO We could precalculate the jump height curve so that we don't need to read the curve twice every frame.
        float thisFrameProgress = this.DurationActiveMs / this.Settings.JumpDurationMs;
        float thisFrameHeightMultiplier = this.Settings.JumpHeightCurve?.Sample(thisFrameProgress)
            ?? (float) Math.Sin(thisFrameProgress * Math.PI / 2);
        float lastFrameProgress = Math.Max(0, (this.DurationActiveMs - delta * 1000) / this.Settings.JumpDurationMs);
        float lastFrameHeightMultiplier = this.Settings.JumpHeightCurve?.Sample(lastFrameProgress)
            ?? (float) Math.Sin(lastFrameProgress * Math.PI / 2);
        float heightDiffPx = this.Settings.JumpHeightUn * (thisFrameHeightMultiplier - lastFrameHeightMultiplier);
        this.Character.Velocity = this.Character.Velocity with { Y = heightDiffPx / delta };

        // Perform movement
        this.Character.MoveAndSlide();

        // Update the timer of head bonk at the ceiling
        if (this.TimeHitCeiling == null && this.Character.IsOnCeiling()) {
            this.TimeHitCeiling = Time.GetTicksMsec();
        } else if (this.TimeHitCeiling != null && !this.Character.IsOnCeiling()) {
            this.TimeHitCeiling = null;
        }
    }
}
