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
            ? GodotUtil.GetResource<DashSettings>(transition.Data.Value.AsUInt64())
            : this.Character.Settings.AirDash?.OverrideSettings ?? this.Character.Settings.Dash
            ?? throw new Exception("No dash settings found.");
		this.Character.Velocity = (this.Character.InputController.MovementInput3DOrNull ?? this.Character.Basis.Z * -1)
			* (
				this.Character.Velocity.Length()
				* this.Settings.InitialVelocityMultiplier
				+ this.Settings.InitialVelocityAdditionUnPSec
			);
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
        this.Character.ApplyHorizontalMovement(new() {
            TargetDirection = GodotUtil.V3ToHV2(this.Character.Basis.Z * -1),
            TargetSpeedUnPSec = this.Settings.MaxSpeedUnPSec,
            AccelerationUnPSecSq = this.Settings.AccelerationUnPSecSq,
        });
        this.Character.ApplyVerticalMovement(
            this.Settings.IgnoresGravity
                ? new() { TargetVerticalSpeed = 0 }
                : this.Character.CalculateOnAirVerticalMovement()
        );
        this.Character.MoveAndSlide();
    }
}
