using System;
using Godot;

namespace Raele.SuperCharacter3D.StateControllers;

[GlobalClass]
public partial class Jump : MotionStateController
{
	[Export] public float JumpHeightUn { get; private set; } = 3.5f;
	[Export] public ulong JumpDurationMs { get; private set; } = 500;
	[Export] public Curve? JumpHeightCurve;
    [Export] public string StateTransitionOnJumpApex = "FallState";
    [Export] public string StateTransitionOnHitCeiling = "FallState";

    [ExportGroup("Assist Options")]
	[Export] public ulong CeilingSlideLeniencyMs { get; private set; } = 150;

    public ulong HitCeilingTimestamp;

    public override void OnProcessStateActive(ControlledState state, float delta)
    {
        if (
            !string.IsNullOrEmpty(this.StateTransitionOnJumpApex)
            && state.DurationActiveMs >= this.JumpDurationMs
        ) {
            state.StateMachine.Transition(this.StateTransitionOnJumpApex);
        } else if (
            !string.IsNullOrEmpty(this.StateTransitionOnHitCeiling)
            && this.HitCeilingTimestamp != 0
            && (
                !state.Character.SlideOnCeiling
                || Time.GetTicksMsec() >= this.HitCeilingTimestamp + this.CeilingSlideLeniencyMs
            )
        ) {
            state.StateMachine.Transition(this.StateTransitionOnHitCeiling);
        }
    }

    public override void OnPhysicsProcessStateActive(ControlledState state, float delta)
    {
        // TODO We could precalculate the jump height curve so that we don't need to read the curve twice every frame.
        float thisFrameProgress = state.DurationActiveMs / (float) this.JumpDurationMs;
        float thisFrameHeightMultiplier = this.JumpHeightCurve?.Sample(thisFrameProgress)
            ?? (float) Math.Sin(thisFrameProgress * Math.PI / 2);
        float lastFrameProgress = Math.Max(0, (state.DurationActiveMs - delta * 1000) / (float) this.JumpDurationMs);
        float lastFrameHeightMultiplier = this.JumpHeightCurve?.Sample(lastFrameProgress)
            ?? (float) Math.Sin(lastFrameProgress * Math.PI / 2);
        float heightDiffPx = this.JumpHeightUn * (thisFrameHeightMultiplier - lastFrameHeightMultiplier);
        state.Character.Velocity = state.Character.Velocity with { Y = heightDiffPx / delta };

        // Update the timer of head bonk at the ceiling
        if (this.HitCeilingTimestamp == 0 && state.Character.IsOnCeiling()) {
            this.HitCeilingTimestamp = Time.GetTicksMsec();
        } else if (this.HitCeilingTimestamp != 0 && !state.Character.IsOnCeiling()) {
            this.HitCeilingTimestamp = 0;
        }
    }
}
