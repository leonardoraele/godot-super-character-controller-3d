using System;
using Godot;

namespace Raele.SuperCharacterController2D;

public partial class OnWallState : MotionState
{
	private int WallDirection;
	private float WallDropInputTimeMs;

	public bool IsPlayerInputToDropFromWall => this.Character.WallDropPreventionLeniencyMs >= 0
		? Math.Sign(this.Character.InputManager.MovementInput.X) == this.WallDirection * -1
		: Math.Sign(this.Character.InputManager.MovementInput.X) != this.WallDirection;

	public override void OnEnter(MotionState.TransitionInfo transition)
	{
		base.OnEnter(transition);
		if (!this.Character.IsOnWall()) {
			this.Character.TransitionMotionState<FallingState>();
		}
		this.WallDirection = Math.Sign(this.Character.GetWallNormal().X) * -1;
		this.Character.FacingDirection = this.WallDirection;
	}

	public override void OnProcessState(float delta)
	{
		base.OnProcessState(delta);

		// If no longer on wall, drop off
		if (this.Character.WallMotionSettings == null || !this.Character.IsOnWall())
		{
			this.Character.TransitionMotionState<FallingState>();
			return;
		}

		// If player input is to drop from wall, drop off
		if (this.IsPlayerInputToDropFromWall)
		{
			if (this.WallDropInputTimeMs == 0)
			{
				this.WallDropInputTimeMs = Time.GetTicksMsec();
			}
			else if (this.Character.WallDropPreventionLeniencyMs <= 0
				|| Time.GetTicksMsec() > this.WallDropInputTimeMs + this.Character.WallDropPreventionLeniencyMs
			)
			{
				this.Character.TransitionMotionState<FallingState>();
				return;
			}
		}
		else
		{
			this.WallDropInputTimeMs = 0;
		}

		// If player input is to jump, jump off the wall with a starting kickoff speed
		if (this.Character.InputManager.JumpInputBuffer.ConsumeInput())
		{
			this.Character.Velocity = Vector2.Right * this.WallDirection * -1 * this.Character.WallMotionSettings.WallJumpKickoffSpeedPxPSec;
			this.Character.TransitionMotionState<JumpingState>();
			return;
		}
	}
}
