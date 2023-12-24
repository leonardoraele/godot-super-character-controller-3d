using Godot;

namespace Raele.SuperCharacterController3D.MotionStates;

public partial class AirDashingState : BaseAirState
{
    public override void OnExit(BaseMotionState.TransitionInfo transition)
    {
        // TODO // FIXME There's a problem here. Even though we are preventing the player from perfoming a dash over
        // another when CanCancelDash is false, the dash input will already have been consumed, which means the player
        // can't buffer dash inputs to perform a dash as soon as possible.
        base.OnExit(transition);
        if (transition.NextState != nameof(AirDashingState) || (this.Character.Settings.Dash?.VariableLength ?? true)) {
            transition.Cancel();
        }
    }

    public override void OnProcessState(float delta)
    {
        base.OnProcessState(delta);
        if (this.DurationActiveMs > (this.Character.Settings.Dash?.MaxDurationSec ?? 0) * 1000)
        {
            this.Character.TransitionMotionState<FallingState>();
        }
    }

    public override void OnPhysicsProcessState(float delta)
    {
        Vector2 GetXZ(Vector3 v3d) => new Vector2(v3d.X, v3d.Z);
        base.OnPhysicsProcessState(delta);
        (Vector2 velocityXZ, Vector2 accelerationXZ) = this.Character.Settings.Dash != null
            ? (
                this.Character.Settings.Dash.MaxSpeedUnPSec * GetXZ(this.Character.Transform.Basis.Z.Normalized()),
                Vector2.One * this.Character.Settings.Dash.AccelerationUnPSecSq * delta
            )
            : this.CalculateHorizontalOnFootPhysics(delta);
        (float velocityY, float accelerationY) = this.Character.Settings.Dash?.AirDash?.IgnoresGravity == true
            ? (0, float.PositiveInfinity)
            : this.CalculateVerticalOnAirPhysics(delta);
        this.Character.Accelerate(velocityXZ, velocityY, accelerationXZ, accelerationY);
        this.Character.MoveAndSlide();
    }
}
