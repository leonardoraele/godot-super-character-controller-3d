using System;
using Godot;

namespace Raele.SuperCharacter3D.MotionStates;

public partial class WallClimbState : BaseMotionState
{
    // [Export] public WallStateUtil.FacingDirectionMode FacingDirection = WallStateUtil.FacingDirectionMode.TowardWall;
	[Export] public float MaxUpwardClimbSpeedUnPSec = 2f;
	[Export] public float MaxDownwardSpeedUnPSec = 4f;
	[Export] public float MaxHorizontalSpeedUnPSec = 2f;
	[Export] public float AccelerationUnPSecSq = 6f;
	// [Export] public float HorizontalMinWallAngleDeg = 15f;
	// [Export] public float WallSnapDistanceUn = 0.15f;
	[Export] public RayCast3D TopLeftRayCast = null!;
	[Export] public RayCast3D TopRightRayCast = null!;
	[Export] public RayCast3D BottomLeftRayCast = null!;
	[Export] public RayCast3D BottomRightRayCast = null!;
	[Export] BaseMotionState? StateTransitionWhenOnTopEdge;
	[Export] BaseMotionState? StateTransitionWhenOnBottomEdge;
	[Export] BaseMotionState? StateTransitionWhenOnSideEdge;

    public override void OnEnter(StateTransition transition)
    {
        base.OnEnter(transition);
        // this.Character.Rotation = Vector3.Up
        //     * WallStateUtil.GetTargetFacingDirection(this.FacingDirection, this.Character)
        //         .AngleTo(Vector2.Up);
		this.Character.Velocity = Vector3.Zero;
		if (this.TopLeftRayCast != null) this.TopLeftRayCast.Enabled = true;
		if (this.TopRightRayCast != null) this.TopRightRayCast.Enabled = true;
		if (this.BottomLeftRayCast != null) this.BottomLeftRayCast.Enabled = true;
		if (this.BottomRightRayCast != null) this.BottomRightRayCast.Enabled = true;
    }

    public override void OnExit(StateTransition transition)
    {
		base.OnExit(transition);
		if (!transition.Canceled) {
			if (this.TopLeftRayCast != null) this.TopLeftRayCast.Enabled = false;
			if (this.TopRightRayCast != null) this.TopRightRayCast.Enabled = false;
			if (this.BottomLeftRayCast != null) this.BottomLeftRayCast.Enabled = false;
			if (this.BottomRightRayCast != null) this.BottomRightRayCast.Enabled = false;
		}
    }

    public override void OnProcessStateActive(float delta)
    {
        base.OnProcessStateActive(delta);
		// if (
		// 	this.StateTransitionWhenOnTopEdge != null
		// 	&& this.TopLeftRayCast?.IsColliding() == false
		// 	&& this.TopRightRayCast?.IsColliding() == false
		// ) {
		// 	this.StateMachine.Transition(this.StateTransitionWhenOnTopEdge.Name);
		// } else if (
		// 	this.StateTransitionWhenOnBottomEdge != null
		// 	&& this.BottomLeftRayCast?.IsColliding() == false
		// 	&& this.BottomRightRayCast?.IsColliding() == false
		// ) {
		// 	this.StateMachine.Transition(this.StateTransitionWhenOnBottomEdge.Name);
		// } else if (
		// 	this.StateTransitionWhenOnSideEdge != null
		// 	&& (
		// 		this.TopLeftRayCast?.IsColliding() == false && this.BottomLeftRayCast?.IsColliding() == false
		// 		|| this.TopRightRayCast?.IsColliding() == false && this.BottomRightRayCast?.IsColliding() == false
		// 	)
		// ) {
		// 	this.StateMachine.Transition(this.StateTransitionWhenOnSideEdge.Name);
		// }
    }

    public override void OnPhysicsProcessStateActive(float delta)
    {
		this.Character.Rotation = Vector3.Up
			* GodotUtil.V3ToHV2(this.Character.GetWallNormal()).AngleTo(Vector2.Down);
		this.Character.Velocity = Mathf.MoveToward(
				this.Character.Velocity.Length(),
				this.MaxUpwardClimbSpeedUnPSec * Math.Max(0, this.Character.InputController.MovementInput.Y * -1)
					+ this.MaxDownwardSpeedUnPSec * Math.Max(0, this.Character.InputController.MovementInput.Y)
					+ this.MaxHorizontalSpeedUnPSec * Math.Abs(this.Character.InputController.MovementInput.X),
				this.AccelerationUnPSecSq * delta
			)
			* GodotUtil.HV2ToV3(this.Character.InputController.MovementInput.Rotated(this.Character.Rotation.Y * -1))
				.Rotated(this.Character.GetWallNormal().Rotated(Vector3.Up, Mathf.Pi / 2), Mathf.Pi / 2)
			// + this.Character.GetWallNormal() * -1
			;

		if (this.Character.Velocity.Length() < 0.01f) {
			return;
		}

		// Aabb aabb = this.ActiveCollisionShape!.Shape!.GetDebugMesh().GetAabb();
		// Vector3 bottomLeftPos = this.TopLeftRayCast?.GlobalPosition ?? this.Character.Transform.TranslatedLocal(Vector3.Left * aabb.Size.X / 2).Origin;
		// Vector3 bottomRightPos = this.BottomRightRayCast?.GlobalPosition ?? this.Character.Transform.TranslatedLocal(Vector3.Right * aabb.Size.X / 2).Origin;
		// Vector3 topLeftPos = this.TopLeftRayCast?.GlobalPosition ?? this.Character.Transform.TranslatedLocal(Vector3.Left * aabb.Size.X / 2 + Vector3.Up * aabb.Size.Y).Origin;
		// Vector3 topRightPos = this.TopRightRayCast?.GlobalPosition ?? this.Character.Transform.TranslatedLocal(Vector3.Right * aabb.Size.X / 2 + Vector3.Up * aabb.Size.Y).Origin;

        Godot.Collections.Array<Rid> exclude = new() { this.Character.GetRid() };
		Vector3 velocityNormal = this.Character.Velocity.Normalized();
		bool movingLeft = velocityNormal.Dot(this.Character.Basis.X) < -0.5f;
		bool movingRight = velocityNormal.Dot(this.Character.Basis.X) > 0.5f;
		bool movingUp = velocityNormal.Dot(this.Character.Basis.Y) > 0.5f;
		bool movingDown = velocityNormal.Dot(this.Character.Basis.Y) < -0.5f;
        bool topLeftGrip = !movingUp
			&& !movingLeft
			|| this.TopLeftRayCast.IsColliding() == true
			|| this.Character.GetWorld3D().DirectSpaceState.IntersectRay(new() {
				From = this.TopLeftRayCast.GlobalPosition + this.Character.Velocity * delta,
				To = this.TopLeftRayCast.GlobalPosition + this.Character.Velocity * delta + this.Character.Basis.Z * -1 * 0.5f,
				CollisionMask = this.TopLeftRayCast.CollisionMask,
				Exclude = exclude,
			})
			.Count > 0;
		bool topRightGrip = !movingUp
			&& !movingRight
			|| this.Character.GetWorld3D().DirectSpaceState.IntersectRay(new() {
				From = this.TopRightRayCast.GlobalPosition + this.Character.Velocity * delta,
				To = this.TopRightRayCast.GlobalPosition + this.Character.Velocity * delta + this.Character.Basis.Z * -1 * 0.5f,
				CollisionMask = this.TopRightRayCast.CollisionMask,
				Exclude = exclude,
			})
			.Count > 0;
		bool bottomLeftGrip = !movingDown
			&& !movingLeft
			|| this.Character.GetWorld3D().DirectSpaceState.IntersectRay(new() {
				From = this.BottomLeftRayCast.GlobalPosition + this.Character.Velocity * delta,
				To = this.BottomLeftRayCast.GlobalPosition + this.Character.Velocity * delta + this.Character.Basis.Z * -1 * 0.5f,
				CollisionMask = this.BottomLeftRayCast.CollisionMask,
				Exclude = exclude,
			})
			.Count > 0;
		bool bottomRightGrip = !movingDown
			&& !movingRight
			|| this.Character.GetWorld3D().DirectSpaceState.IntersectRay(new() {
				From = this.BottomRightRayCast.GlobalPosition + this.Character.Velocity * delta,
				To = this.BottomRightRayCast.GlobalPosition + this.Character.Velocity * delta + this.Character.Basis.Z * -1 * 0.5f,
				CollisionMask = this.BottomRightRayCast.CollisionMask,
				Exclude = exclude,
			})
			.Count > 0;

		DebugDraw3D.DrawSphere(this.TopLeftRayCast.GlobalPosition, 0.05f, topLeftGrip ? Colors.Green : Colors.Red);
		DebugDraw3D.DrawSphere(this.TopRightRayCast.GlobalPosition, 0.05f, topRightGrip ? Colors.Green : Colors.Red);
		DebugDraw3D.DrawSphere(this.BottomLeftRayCast.GlobalPosition, 0.05f, bottomLeftGrip ? Colors.Green : Colors.Red);
		DebugDraw3D.DrawSphere(this.BottomRightRayCast.GlobalPosition, 0.05f, bottomRightGrip ? Colors.Green : Colors.Red);

		if (!topLeftGrip || !topRightGrip || !bottomLeftGrip || !bottomRightGrip) {
			if (this.StateTransitionWhenOnBottomEdge != null && !bottomLeftGrip && !bottomRightGrip) {
				this.StateMachine.Transition(this.StateTransitionWhenOnBottomEdge.Name);
				return;
			} else if (this.StateTransitionWhenOnTopEdge != null && !topLeftGrip && !topRightGrip) {
				this.StateMachine.Transition(this.StateTransitionWhenOnTopEdge.Name);
				return;
			} else if (this.StateTransitionWhenOnSideEdge != null && (!topRightGrip && !bottomRightGrip || !topLeftGrip && !bottomLeftGrip)) {
				this.StateMachine.Transition(this.StateTransitionWhenOnSideEdge.Name);
				return;
			} else {
				this.Character.Velocity = Vector3.Zero;
			}
		}

		this.Character.MoveAndSlide();
    }
}
