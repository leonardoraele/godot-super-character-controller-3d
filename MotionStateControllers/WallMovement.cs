using System;
using Godot;

namespace Raele.SuperCharacter3D.MotionStateControllers;

public partial class WallMovement : MotionStateController
{
    // [Export] public WallStateUtil.FacingDirectionMode FacingDirection = WallStateUtil.FacingDirectionMode.TowardWall;
	[Export] public float MaxUpwardClimbSpeedUnPSec = 2f;
	[Export] public float MaxDownwardSpeedUnPSec = 4f;
	[Export] public float MaxHorizontalSpeedUnPSec = 2f;
	[Export] public float AccelerationUnPSecSq = 6f;
	// [Export] public float HorizontalMinWallAngleDeg = 15f;
	// [Export] public float WallSnapDistanceUn = 0.15f;
	[Export] string? StateTransitionWhenOnTopEdge;
	[Export] string? StateTransitionWhenOnBottomEdge;
	[Export] string? StateTransitionWhenOnSideEdge;

    // public override void OnEnter(ControlledState this.State, MotionStateTransition transition)
    // {
    //     this.State.Character.Rotation = Vector3.Up
    //         * WallStateUtil.GetTargetFacingDirection(this.FacingDirection, this.Character)
    //             .AngleTo(Vector2.Up);
    // }

    public override void OnPhysicsProcessStateActive(float delta)
    {
		if (this.State.Character.IsOnWall()) {
			Vector3 wallNormal = this.State.Character.GetWallNormal();
			Vector3 wallTangentX = wallNormal.Rotated(Vector3.Up, Mathf.Pi / 2);
			this.State.Character.Rotation = Vector3.Up
				* GodotUtil.V3ToHV2(wallNormal).AngleTo(Vector2.Down);

			float hSpeed = Mathf.MoveToward(new Vector2(this.State.Character.Velocity.X, this.State.Character.Velocity.Z).Length(), this.MaxHorizontalSpeedUnPSec, this.AccelerationUnPSecSq * delta);
			this.State.Character.Velocity =
				Mathf.MoveToward(
					this.State.Character.Velocity.Length(),
					this.MaxUpwardClimbSpeedUnPSec * Math.Max(0, this.State.Character.InputController.MovementInput.Y * -1)
						+ this.MaxDownwardSpeedUnPSec * Math.Max(0, this.State.Character.InputController.MovementInput.Y)
						+ this.MaxHorizontalSpeedUnPSec * Math.Abs(this.State.Character.InputController.MovementInput.X),
					this.AccelerationUnPSecSq * delta
				)
				* GodotUtil.HV2ToV3(this.State.Character.InputController.MovementInput.Rotated(this.State.Character.Rotation.Y * -1))
					.Rotated(wallTangentX, Mathf.Pi / 2)
				// + this.State.Character.GetWallNormal() * -1
				;
		}

		if (this.State.Character.Velocity.Length() < 0.01f) {
			return;
		}

		// Aabb aabb = this.ActiveCollisionShape!.Shape!.GetDebugMesh().GetAabb();
		// Vector3 bottomLeftPos = this.TopLeftRayCast?.GlobalPosition ?? this.State.Character.Transform.TranslatedLocal(Vector3.Left * aabb.Size.X / 2).Origin;
		// Vector3 bottomRightPos = this.BottomRightRayCast?.GlobalPosition ?? this.State.Character.Transform.TranslatedLocal(Vector3.Right * aabb.Size.X / 2).Origin;
		// Vector3 topLeftPos = this.TopLeftRayCast?.GlobalPosition ?? this.State.Character.Transform.TranslatedLocal(Vector3.Left * aabb.Size.X / 2 + Vector3.Up * aabb.Size.Y).Origin;
		// Vector3 topRightPos = this.TopRightRayCast?.GlobalPosition ?? this.State.Character.Transform.TranslatedLocal(Vector3.Right * aabb.Size.X / 2 + Vector3.Up * aabb.Size.Y).Origin;

        // Godot.Collections.Array<Rid> exclude = new() { this.State.Character.GetRid() };
		// Vector3 velocityNormal = this.State.Character.Velocity.Normalized();
		// bool movingLeft = velocityNormal.Dot(this.State.Character.Basis.X) < -0.5f;
		// bool movingRight = velocityNormal.Dot(this.State.Character.Basis.X) > 0.5f;
		// bool movingUp = velocityNormal.Dot(this.State.Character.Basis.Y) > 0.5f;
		// bool movingDown = velocityNormal.Dot(this.State.Character.Basis.Y) < -0.5f;
        // bool topLeftGrip = !movingUp
		// 	&& !movingLeft
		// 	|| this.TopLeftRayCast.IsColliding() == true
		// 	|| this.State.Character.GetWorld3D().DirectSpaceState.IntersectRay(new() {
		// 		From = this.TopLeftRayCast.GlobalPosition + this.State.Character.Velocity * delta,
		// 		To = this.TopLeftRayCast.GlobalPosition + this.State.Character.Velocity * delta + this.State.Character.Basis.Z * -1 * 0.5f,
		// 		CollisionMask = this.TopLeftRayCast.CollisionMask,
		// 		Exclude = exclude,
		// 	})
		// 	.Count > 0;
		// bool topRightGrip = !movingUp
		// 	&& !movingRight
		// 	|| this.State.Character.GetWorld3D().DirectSpaceState.IntersectRay(new() {
		// 		From = this.TopRightRayCast.GlobalPosition + this.State.Character.Velocity * delta,
		// 		To = this.TopRightRayCast.GlobalPosition + this.State.Character.Velocity * delta + this.State.Character.Basis.Z * -1 * 0.5f,
		// 		CollisionMask = this.TopRightRayCast.CollisionMask,
		// 		Exclude = exclude,
		// 	})
		// 	.Count > 0;
		// bool bottomLeftGrip = !movingDown
		// 	&& !movingLeft
		// 	|| this.State.Character.GetWorld3D().DirectSpaceState.IntersectRay(new() {
		// 		From = this.BottomLeftRayCast.GlobalPosition + this.State.Character.Velocity * delta,
		// 		To = this.BottomLeftRayCast.GlobalPosition + this.State.Character.Velocity * delta + this.State.Character.Basis.Z * -1 * 0.5f,
		// 		CollisionMask = this.BottomLeftRayCast.CollisionMask,
		// 		Exclude = exclude,
		// 	})
		// 	.Count > 0;
		// bool bottomRightGrip = !movingDown
		// 	&& !movingRight
		// 	|| this.State.Character.GetWorld3D().DirectSpaceState.IntersectRay(new() {
		// 		From = this.BottomRightRayCast.GlobalPosition + this.State.Character.Velocity * delta,
		// 		To = this.BottomRightRayCast.GlobalPosition + this.State.Character.Velocity * delta + this.State.Character.Basis.Z * -1 * 0.5f,
		// 		CollisionMask = this.BottomRightRayCast.CollisionMask,
		// 		Exclude = exclude,
		// 	})
		// 	.Count > 0;

		// DebugDraw3D.DrawSphere(this.TopLeftRayCast.GlobalPosition, 0.05f, topLeftGrip ? Colors.Green : Colors.Red);
		// DebugDraw3D.DrawSphere(this.TopRightRayCast.GlobalPosition, 0.05f, topRightGrip ? Colors.Green : Colors.Red);
		// DebugDraw3D.DrawSphere(this.BottomLeftRayCast.GlobalPosition, 0.05f, bottomLeftGrip ? Colors.Green : Colors.Red);
		// DebugDraw3D.DrawSphere(this.BottomRightRayCast.GlobalPosition, 0.05f, bottomRightGrip ? Colors.Green : Colors.Red);

		// if (!topLeftGrip || !topRightGrip || !bottomLeftGrip || !bottomRightGrip) {
		// 	if (this.StateTransitionWhenOnBottomEdge != null && !bottomLeftGrip && !bottomRightGrip) {
		// 		this.StateMachine.Transition(this.StateTransitionWhenOnBottomEdge.Name);
		// 		return;
		// 	} else if (this.StateTransitionWhenOnTopEdge != null && !topLeftGrip && !topRightGrip) {
		// 		this.StateMachine.Transition(this.StateTransitionWhenOnTopEdge.Name);
		// 		return;
		// 	} else if (this.StateTransitionWhenOnSideEdge != null && (!topRightGrip && !bottomRightGrip || !topLeftGrip && !bottomLeftGrip)) {
		// 		this.StateMachine.Transition(this.StateTransitionWhenOnSideEdge.Name);
		// 		return;
		// 	} else {
		// 		this.State.Character.Velocity = Vector3.Zero;
		// 	}
		// }

		// this.State.Character.MoveAndSlide();
    }
}
