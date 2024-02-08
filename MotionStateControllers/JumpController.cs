using System;
using Godot;

namespace Raele.SuperCharacter3D.MotionStateControllers;

public partial class JumpController : MotionStateController
{
    [Export] public JumpSettings Settings = null!;

    [Signal] public delegate void JumpApexReachedEventHandler();
    [Signal] public delegate void HitCeilingEventHandler();

    public ulong HitCeilingTimestamp;

    public override void OnProcessStateActive(float delta)
    {
        if (this.State.DurationActiveMs >= this.Settings.JumpDurationMs) {
            this.EmitSignal(SignalName.JumpApexReached);
        } else if (
            this.HitCeilingTimestamp != 0
            && (
                !this.Character.SlideOnCeiling
                || Time.GetTicksMsec() >= this.HitCeilingTimestamp + this.Settings.CeilingSlideLeniencyMs
            )
        ) {
            this.EmitSignal(SignalName.HitCeiling);
        }
    }

    public override void OnPhysicsProcessStateActive(float delta)
    {
        // TODO We could precalculate the jump height curve so that we don't need to read the curve twice every frame.
        float thisFrameProgress = this.State.DurationActiveMs / (float) this.Settings.JumpDurationMs;
        float thisFrameHeightMultiplier = this.Settings.JumpHeightCurve?.Sample(thisFrameProgress)
            ?? (float) Math.Sin(thisFrameProgress * Math.PI / 2);
        float lastFrameProgress = Math.Max(0, (this.State.DurationActiveMs - delta * 1000) / (float) this.Settings.JumpDurationMs);
        float lastFrameHeightMultiplier = this.Settings.JumpHeightCurve?.Sample(lastFrameProgress)
            ?? (float) Math.Sin(lastFrameProgress * Math.PI / 2);
        float heightDiffPx = this.Settings.JumpHeightUn * (thisFrameHeightMultiplier - lastFrameHeightMultiplier);
        this.Character.VerticalSpeed = heightDiffPx / delta;

        // Update the timer of head bonk at the ceiling
        if (this.HitCeilingTimestamp == 0 && this.Character.IsOnCeiling()) {
            this.HitCeilingTimestamp = Time.GetTicksMsec();
        } else if (this.HitCeilingTimestamp != 0 && !this.Character.IsOnCeiling()) {
            this.HitCeilingTimestamp = 0;
        }
    }
}
