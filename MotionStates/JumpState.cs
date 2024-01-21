using System;
using Godot;

namespace Raele.SuperCharacter3D.MotionStates;

public partial class JumpState : BaseMotionState
{
	[Export] public float JumpHeightUn { get; private set; } = 3.5f;
	[Export] public float JumpDurationMs { get; private set; } = 500;
	[Export] public Curve? JumpHeightCurve;
    [Export] public Node? StateTransitionOnJumpApex;

    [ExportGroup("Air Control")]
    [Export] public BaseMotionState? HorizontalMovementReference;
    [Export] public float AerialSpeedMultiplier { get; private set; } = 1f;
    [Export] public float AerialAccelerationMultiplier { get; private set; } = 1f;
    [Export] public float AerialRotationSpeedMultiplier { get; private set; } = 1f;

    public ulong HitCeilingTimestamp;

    public override void OnProcessStateActive(float delta)
    {
        base.OnProcessStateActive(delta);
        if (
            this.DurationActiveMs >= this.JumpDurationMs
            || this.HitCeilingTimestamp != 0
            && (
                !this.Character.SlideOnCeiling
                || Time.GetTicksMsec() >= this.HitCeilingTimestamp + this.Character.Settings.CeilingSlideTimeMs
            )
        ) {
            this.Character.StateMachine.Transition(this.StateTransitionOnJumpApex?.Name ?? nameof(FallState));
        }
        // else if (this.DurationActiveMs >= this.Settings.MinJumpDurationMs
        //     && !Input.IsActionPressed(this.Character.Settings.Input.JumpAction)
        // ) {
        //     this.Character.StateMachine.Transition<JumpCanceledState>(this.Settings.GetInstanceId());
        // }
    }

    public override void OnPhysicsProcessStateActive(float delta)
    {
        // Apply horizontal velocity
        HorizontalMovement hMovement = this.HorizontalMovementReference?.GetHorizontalMovement() ?? new();
        GD.PrintS(hMovement);
        hMovement.TargetSpeedUnPSec *= this.AerialSpeedMultiplier;
        hMovement.AccelerationUnPSecSq *= this.AerialAccelerationMultiplier;
        hMovement.RotationSpeedDegPSec *= this.AerialRotationSpeedMultiplier;
        this.Character.ApplyHorizontalMovement(hMovement);

        // Calculate vertical velocity
        // TODO We could precalculate the jump height curve so that we don't need to read the curve twice every frame.
        float thisFrameProgress = this.DurationActiveMs / this.JumpDurationMs;
        float thisFrameHeightMultiplier = this.JumpHeightCurve?.Sample(thisFrameProgress)
            ?? (float) Math.Sin(thisFrameProgress * Math.PI / 2);
        float lastFrameProgress = Math.Max(0, (this.DurationActiveMs - delta * 1000) / this.JumpDurationMs);
        float lastFrameHeightMultiplier = this.JumpHeightCurve?.Sample(lastFrameProgress)
            ?? (float) Math.Sin(lastFrameProgress * Math.PI / 2);
        float heightDiffPx = this.JumpHeightUn * (thisFrameHeightMultiplier - lastFrameHeightMultiplier);
        this.Character.Velocity = this.Character.Velocity with { Y = heightDiffPx / delta };

        // Perform movement
        this.Character.MoveAndSlide();

        // Update the timer of head bonk at the ceiling
        if (this.HitCeilingTimestamp == 0 && this.Character.IsOnCeiling()) {
            this.HitCeilingTimestamp = Time.GetTicksMsec();
        } else if (this.HitCeilingTimestamp != 0 && !this.Character.IsOnCeiling()) {
            this.HitCeilingTimestamp = 0;
        }
    }
}
