using System;
using Godot;

namespace Raele.SuperCharacter3D.MotionStates;

public partial class AirDashingState : BaseAirState
{
    private DashSettings Settings = null!;
    public override void OnEnter(StateTransition transition)
    {
        base.OnEnter(transition);
        this.Settings = transition.Data.HasValue && transition.Data.Value.AsUInt64() != 0
            ? GeneralUtility.GetResource<DashSettings>(transition.Data.Value.AsUInt64())
            : this.Character.Settings.AirDash?.Dash ?? this.Character.Settings.Dash
            ?? throw new Exception("No dash settings found.");
    }
    public override void OnExit(StateTransition transition)
    {
        base.OnExit(transition);
        // TODO // FIXME There's a problem here. Even though we are preventing the player from perfoming a dash over
        // another when CanCancelDash is false, the dash input will already have been consumed, which means the player
        // can't buffer dash inputs to perform a dash as soon as possible.
        if (transition.NextStateName == nameof(AirDashingState) || !this.Settings.VariableLength) {
            transition.Cancel();
        }
    }

    public override void OnProcessState(float delta)
    {
        base.OnProcessState(delta);
        if (this.DurationActiveMs > this.Settings.MaxDurationSec * 1000)
        {
            this.Character.StateMachine.Transition<FallingState>();
        }
    }

    public override void OnPhysicsProcessState(float delta)
    {
        base.OnPhysicsProcessState(delta);

        // Horizontal movement
        Vector2 targetVelocityXZ = this.Settings.MaxSpeedUnPSec * Vector2.Up.Rotated(this.Character.Rotation.Y * -1);
        Vector2 accelerationXZ = Vector2.One * this.Settings.AccelerationUnPSecSq * delta;

        // Vertical movement
        (float targetVelocityY, float accelerationY) = this.Settings.IgnoreGravity == true
            ? (0, float.PositiveInfinity)
            : this.Character.CalculateVerticalOnAirPhysics(delta);

        // Apply movement
        this.Character.Accelerate(targetVelocityXZ, targetVelocityY, accelerationXZ, accelerationY);
        this.Character.MoveAndSlide();
    }
}
