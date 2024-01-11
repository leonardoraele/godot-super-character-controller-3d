using System;
using Godot;

namespace Raele.SuperCharacter3D.MotionStates;

public partial class JumpingState : BaseAirState
{
    public ulong? TimeHitCeiling;
    private JumpSettings Settings = null!;

    public override void OnEnter(TransitionInfo transition)
    {
        base.OnEnter(transition);
        this.Settings = transition.Data.HasValue
            ? GeneralUtility.GetResource<JumpSettings>(transition.Data.Value.AsUInt64())
            : this.Character.Settings.Jump;
    }

    public override void OnProcessState(float delta)
    {
        base.OnProcessState(delta);
        // Transition Jump -> Jump Canceled
        if (this.Settings.VariableJumpHeightEnabled
            && this.DurationActiveMs >= this.Settings.MinJumpDurationMs
            && !Input.IsActionPressed(this.Character.Settings.Input.JumpAction)
            || this.DurationActiveMs >= this.Settings.JumpDurationMs
            || this.TimeHitCeiling != null
            && (
                !this.Character.SlideOnCeiling
                || Time.GetTicksMsec() >= this.TimeHitCeiling.Value + this.Character.Settings.CeilingSlideTimeMs
            )
        ) {
            this.Character.TransitionMotionState<JumpCanceledState>(this.Settings.GetInstanceId());
        }
    }

    public override void OnPhysicsProcessState(float delta)
    {
        base.OnPhysicsProcessState(delta);

        // Calculate horizontal velocity
        (Vector2 velocityXZ, Vector2 accelerationXZ) = this.CalculateHorizontalOnAirPhysics(delta, default, this.Settings);
        this.Character.AccelerateXZ(velocityXZ, accelerationXZ);

        // Calculate vertical velocity
        // TODO We could precalculate the jump height curve so that we don't need to read the curve twice every frame.
        float thisFrameProgress = this.DurationActiveMs / this.Settings.JumpDurationMs;
        float thisFrameHeightMultiplier = this.Settings.JumpHeightCurve?.Sample(thisFrameProgress)
            ?? (float) Math.Sin(thisFrameProgress * Math.PI / 2);
        float lastFrameProgress = Math.Max(0, (this.DurationActiveMs - delta * 1000) / this.Settings.JumpDurationMs);
        float lastFrameHeightMultiplier = this.Settings.JumpHeightCurve?.Sample(lastFrameProgress)
            ?? (float) Math.Sin(lastFrameProgress * Math.PI / 2);
        float heightDiffPx = this.Settings.JumpHeightUn * (thisFrameHeightMultiplier - lastFrameHeightMultiplier);
        float velocityY = heightDiffPx / delta;
        this.Character.AccelerateY(velocityY, float.PositiveInfinity);

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
