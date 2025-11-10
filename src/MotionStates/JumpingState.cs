using System;
using Godot;

namespace Raele.SuperCharacterController2D;

public partial class JumpingState : OnAirState
{
    public ulong? TimeHitCeiling;

    public override void OnProcessState(float delta)
    {
        base.OnProcessState(delta);
        // Transition Jump -> Jump Canceled
        if (this.Character.JumpSettings.VariableJumpHeightEnabled
            && !Input.IsActionPressed(this.Character.InputSettings.JumpAction)
            || this.DurationActiveMs >= this.Character.JumpSettings.JumpDurationMs
            || this.TimeHitCeiling != null
            && Time.GetTicksMsec() >= this.TimeHitCeiling.Value + this.Character.CeilingSlideTimeMs
        ) {
            this.Character.TransitionMotionState<JumpCanceledState>();
        }
    }

    public override void OnPhysicsProcessState(float delta)
    {
        base.OnPhysicsProcessState(delta);

        // Calculate horizontal velocity
        (float velocityX, float accelerationX) = this.CalculateHorizontalOnAirPhysics(delta);
        this.Character.AccelerateX(velocityX, accelerationX);

        // Calculate vertical velocity
        // TODO We could precalculate the jump height curve so that we don't need to read the curve twice every frame.
        float thisFrameProgress = this.DurationActiveMs / this.Character.JumpSettings.JumpDurationMs;
        float thisFrameHeightMultiplier = this.Character.JumpSettings.JumpHeightCurve?.Sample(thisFrameProgress)
            ?? (float) Math.Sin(thisFrameProgress * Math.PI / 2);
        float lastFrameProgress = Math.Max(0, (this.DurationActiveMs - delta * 1000) / this.Character.JumpSettings.JumpDurationMs);
        float lastFrameHeightMultiplier = this.Character.JumpSettings.JumpHeightCurve?.Sample(lastFrameProgress)
            ?? (float) Math.Sin(lastFrameProgress * Math.PI / 2);
        float heightDiffPx = this.Character.JumpSettings.JumpHeightPx * (thisFrameHeightMultiplier - lastFrameHeightMultiplier);
        float velocityY = heightDiffPx / delta * -1;
        this.Character.AccelerateY(velocityY, float.PositiveInfinity);

        // Perform movement
        this.Character.MoveAndSlide();

        if (this.TimeHitCeiling == null && this.Character.IsOnCeiling()) {
            this.TimeHitCeiling = Time.GetTicksMsec();
        } else if (this.TimeHitCeiling != null && !this.Character.IsOnCeiling()) {
            this.TimeHitCeiling = null;
        }
    }
}
