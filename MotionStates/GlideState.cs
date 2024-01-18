using System;
using Godot;

namespace Raele.SuperCharacter3D.MotionStates;

public partial class GlideState : BaseAirState
{
    private GlideSettings Settings = null!;
    private ulong AccumulatedTimeGlidingMs;
    public override void OnEnter(StateTransition transition)
    {
        base.OnEnter(transition);
        if (transition.Canceled) {
            return;
        }
        Settings = transition.Data.HasValue && transition.Data.Value.AsUInt64() != 0
            ? GodotUtil.GetResource<GlideSettings>(transition.Data.Value.AsUInt64())
            : this.Character.Settings.Glide
            ?? throw new Exception("GlideSettings is missing.");
        if (this.AccumulatedTimeGlidingMs >= this.Settings.MaxDurationSec) {
            transition.Cancel();
        }
    }
    public override void OnExit(StateTransition transition)
    {
        base.OnExit(transition);
        if (transition.Canceled) {
            return;
        }
        this.AccumulatedTimeGlidingMs += this.DurationActiveMs;
    }
    public override void _Process(double delta)
    {
        base._Process(delta);
        if (this.Character.IsOnFloor()) {
            this.AccumulatedTimeGlidingMs = 0;
        }
    }
    public override void OnProcessState(float delta)
    {
        base.OnProcessState(delta);
        if (this.Character.IsOnFloor()) {
            this.Character.StateMachine.Transition<OnFootState>();
        } else if (Input.IsActionJustPressed(this.Character.Settings.Input.DashAction)) {
            this.Character.StateMachine.Transition<AirDashingState>();
        } else if (
            !Input.IsActionPressed(this.Character.Settings.Input.JumpAction)
            || this.AccumulatedTimeGlidingMs + this.DurationActiveMs >= this.Settings.MaxDurationSec * 1000
        ) {
            this.Character.StateMachine.Transition<FallingState>();
        }
    }

    public override HorizontalMovement GetHorizontalMovement()
    {
        HorizontalMovement hMovement = this.Character.CalculateHorizontalMovement();
        hMovement.TargetSpeedUnPSec *= this.Settings.HorizontalMaxSpeedMultiplier;
        hMovement.AccelerationUnPSecSq *= this.Settings.HorizontalAccelerationMultiplier;
        return hMovement;
    }

    public override VerticalMovement GetVerticalMovement()
        => new() {
            TargetVerticalSpeed = this.Settings.GlideFall.MaxFallSpeedUnPSec * -1,
            Acceleration = this.Character.Velocity.Y < this.Settings.GlideFall.MaxFallSpeedUnPSec * -1
                ? float.PositiveInfinity
                : this.Settings.GlideFall.FallAccelerationUnPSecSq,
        };
}
