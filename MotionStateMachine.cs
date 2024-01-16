using System;
using Godot;
using Raele.SuperCharacter3D.MotionStates;

namespace Raele.SuperCharacter3D;

public partial class MotionStateMachine : Node
{
	public SuperCharacter3DController Character { get; private set; } = null!;
	public MotionState? CurrentState { get; private set; }
	public StateTransition? QueuedTransition;
	private ulong LastStateChangeTimestamp;

	public ulong TimeSinceLastStateChangeMs => Time.GetTicksMsec() - this.LastStateChangeTimestamp;

	[Signal] public delegate void StateChangedEventHandler(string newState, string oldState, Variant data);

	public override void _EnterTree()
	{
		base._EnterTree();
		if (!(this.GetParent() is SuperCharacter3DController character)) {
			GD.PushError(nameof(MotionStateMachine), " must be a child of ", nameof(SuperCharacter3DController));
			this.QueueFree();
			return;
		}
		this.Character = character;
	}

    public override void _Ready()
    {
        base._Ready();
		this.Reset();
    }

    public override void _Process(double delta)
	{
		try {
			this.CurrentState?.OnProcessState((float) delta);
		} catch (Exception e) {
			GD.PushError(e);
		}
		this.CallDeferred(nameof(this.PerformTransition));
	}

	public override void _PhysicsProcess(double delta)
	{
		this.CurrentState?.OnPhysicsProcessState((float) delta);
	}

    public void Transition<T>(Variant? data = null) where T : MotionState
    {
		this.Transition(typeof(T).Name, data);
    }

	public void Transition(string nextStateName, Variant? data = null)
	{
		this.QueuedTransition?.Cancel();
		this.QueuedTransition = new StateTransition() {
			Data = data,
			PreviousStateName = this.CurrentState?.Name,
			NextStateName = nextStateName,
		};
		this.QueuedTransition.CanceledEvent += this.CancelTransition;
	}

	public void CancelTransition()
	{
		this.QueuedTransition = null;
	}

	private void PerformTransition()
	{
		if (this.QueuedTransition == null) {
			return;
		}

		StateTransition currentTransition = this.QueuedTransition;

		// Exit current state
		{
			try {
				this.CurrentState?.OnExit(currentTransition);
			} catch (Exception e) {
				GD.PushError(e);
			}

			// This happens if this.Transition or this.CancelTransition have been called during OnExit.
			if (this.QueuedTransition != currentTransition) {
				if (this.QueuedTransition == null) {
					GD.PrintS(
						nameof(MotionStateMachine), nameof(this.PerformTransition), ":",
						"🔄", currentTransition.PreviousStateName, "->", currentTransition.NextStateName,
						"🚫", "Canceled by", $"{this.CurrentState?.Name}.{nameof(MotionState.OnExit)}."
					);
					return;
				} else {
					GD.PrintS(
						nameof(MotionStateMachine), nameof(this.PerformTransition), ":",
						"🔄", currentTransition.PreviousStateName, "->", currentTransition.NextStateName,
						"🔄", "Redirected by", $"{this.CurrentState?.Name}.{nameof(MotionState.OnExit)}",
						"to ->", this.QueuedTransition.NextStateName
					);
					currentTransition = this.QueuedTransition;
				}
			}
			this.CurrentState = null;
		}

		if (!(this.GetNode<MotionState>(this.QueuedTransition.NextStateName) is MotionState nextState)) {
			GD.PushError(
				"Failed to transition motion states. ",
				"Cause: State not found. ",
				"State: ", this.QueuedTransition.NextStateName, ". ",
				"Did you forget to add the state node to the ", nameof(MotionStateMachine), " node? ",
				nameof(MotionStateMachine), " path: ", this.GetPath()
			);
			return;
		}

		// Enter new state
		{
			try {
				nextState.OnEnter(this.QueuedTransition);
			} catch (Exception e) {
				currentTransition.Cancel();
				GD.PushError(e);
			}

			if (this.QueuedTransition != currentTransition) {
				if (this.QueuedTransition == null) {
					GD.PrintS(
						nameof(MotionStateMachine), nameof(this.PerformTransition), ":",
						"🚫", "Canceled by", $"{nextState.Name}.{nameof(nextState.OnEnter)}"
					);
					this.Reset();
					return;
				} else {
					GD.PrintS(
						nameof(MotionStateMachine), nameof(this.PerformTransition), ":",
						"🔄", "Redirected by", $"{nextState.Name}.{nameof(nextState.OnEnter)}",
						"->", this.QueuedTransition.NextStateName
					);
 					this.CallDeferred(nameof(this.PerformTransition));
					return;
				}
			}
		}

		GD.PrintS(
			nameof(MotionStateMachine), nameof(this.PerformTransition), ":",
			"🔄", this.QueuedTransition.PreviousStateName, "->", this.QueuedTransition.NextStateName,
			"(complete)"
		);

		this.QueuedTransition = null;
		this.CurrentState = nextState;
		this.LastStateChangeTimestamp = Time.GetTicksMsec();
		this.EmitSignal(
			SignalName.StateChanged,
			currentTransition.NextStateName,
			currentTransition.PreviousStateName ?? "",
			currentTransition.Data ?? 0
		);
	}

	/// <summary>
	/// Changes the character to a neutral state. The state machine will automatically transition to the correct state.
	/// </summary>
	public void Reset()
	{
		this.Character.ApplyFloorSnap();
		if (this.Character.IsOnFloor()) {
			this.Transition<OnFootState>();
		} else {
			this.Transition<FallingState>();
		}
	}
}
