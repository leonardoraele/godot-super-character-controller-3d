using System;
using Godot;

namespace Raele.SuperCharacterController3D.MotionStates;

public partial class JumpingState : BaseAirState
{
    public ulong? TimeHitCeiling;

    public override void OnProcessState(float delta)
    {
        base.OnProcessState(delta);
        // Transition Jump -> Jump Canceled
        if (this.Character.Settings.Jump.VariableJumpHeightEnabled
            && !Input.IsActionPressed(this.Character.Settings.Input.JumpAction)
            || this.DurationActiveMs >= this.Character.Settings.Jump.JumpDurationMs
            || this.TimeHitCeiling != null
            && Time.GetTicksMsec() >= this.TimeHitCeiling.Value + this.Character.Settings.CeilingSlideTimeMs
        ) {
            this.Character.TransitionMotionState<JumpCanceledState>();
        }
    }

    public override void OnPhysicsProcessState(float delta)
    {
        base.OnPhysicsProcessState(delta);

        // Calculate horizontal velocity
        (Vector2 velocityXZ, Vector2 accelerationXZ) = this.CalculateHorizontalOnAirPhysics(delta);
        this.Character.AccelerateXZ(velocityXZ, accelerationXZ);

        // Calculate vertical velocity
        // TODO We could precalculate the jump height curve so that we don't need to read the curve twice every frame.
        float thisFrameProgress = this.DurationActiveMs / this.Character.Settings.Jump.JumpDurationMs;
        float thisFrameHeightMultiplier = this.Character.Settings.Jump.JumpHeightCurve?.Sample(thisFrameProgress)
            ?? (float) Math.Sin(thisFrameProgress * Math.PI / 2);
        float lastFrameProgress = Math.Max(0, (this.DurationActiveMs - delta * 1000) / this.Character.Settings.Jump.JumpDurationMs);
        float lastFrameHeightMultiplier = this.Character.Settings.Jump.JumpHeightCurve?.Sample(lastFrameProgress)
            ?? (float) Math.Sin(lastFrameProgress * Math.PI / 2);
        float heightDiffPx = this.Character.Settings.Jump.JumpHeightUn * (thisFrameHeightMultiplier - lastFrameHeightMultiplier);
        float velocityY = heightDiffPx / delta;
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
