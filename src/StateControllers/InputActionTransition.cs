using System;
using Godot;

namespace Raele.Supercon2D.StateControllers;

public partial class InputActionTransition : StateController
{
	// -----------------------------------------------------------------------------------------------------------------
	// LOCAL TYPES
	// -----------------------------------------------------------------------------------------------------------------

	public enum AbilityActivationMode
	{
		InputIsDown,
		InputIsJustDown,
		InputIsReleased,
	}

	public enum AbilityCancelingMode
	{
		Never,
		OnRelease,
		OnToggle,
	}

	// -----------------------------------------------------------------------------------------------------------------
	// EXPORTS
	// -----------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// Name of the input action to be read for this ability.
	/// </summary>
	[Export] public string? InputActionName;
	[Export] public SuperconState? TargetState;
	[Export] public AbilityActivationMode InputMode = AbilityActivationMode.InputIsJustDown;

	// [ExportGroup("Activated State End Override")]
	// /// <summary>
	// /// This is the this.State the character will be transitioned to when this ability is ended. The ability is ended when
	// /// it comes to an end by its own logic. For example, the PropelState comes to an end when after it lasts for enough
	// /// time, according to the configured settings; and the FallState comes to an end when the character touches the
	// /// ground.
	// /// </summary>
	// [Export] public BaseMotionState? OnEnded;

	[ExportGroup("Activation Conditions")]
	/// <summary>
	/// Time in seconds from the start of the this.State to enable this ability. This ability can only be triggered
	/// after the corresponding this.State has been active for at least this amount of time. Activation attempts prior
	/// to then are ignored.
	/// </summary>
	[Export] public float EnabledFromTimeSec = 0f;
	/// <summary>
	/// Time in seconds from the start of the this.State to disable this ability. This ability can no longer be triggered
	/// after the corresponding this.State has been active for this amount of time.
	/// </summary>
	[Export] public float DisabledAfterTimeSec = float.PositiveInfinity;
	/// <summary>
	/// If set, this ability can only be activated if the previous active this.State was the one specified here.
	/// </summary>
	[Export] public SuperconState? PreviousState;
	/// <summary>
	/// Time in milliseconds to buffer input for this ability. Set to 0 to disable input buffering for this ability.
	/// </summary>
	[Export] public ulong InputBufferTimeMs = 150;
	[Export] public SuperconAbility? Ability;

	[ExportGroup("Action Canceling")]
	[Export] public AbilityCancelingMode CancelingInputMode = AbilityCancelingMode.Never;
	/// <summary>
	/// This is the this.State the character will be transitioned to when this ability is canceled. The ability is canceled
	/// when the input is released (if input mode is Hold) or when the input is pressed again (if input mode is Toggle).
	/// If the input mode is Trigger, this property is ignored. If this property is null, the character will not be
	/// transitioned to another this.State when the ability is canceled.
	/// </summary>
	[Export] public SuperconState? StateTransitionOnAbilityCanceled;

	// -----------------------------------------------------------------------------------------------------------------
	// FIELDS
	// -----------------------------------------------------------------------------------------------------------------

	private bool CancelActionCheckActive = false;

	// -----------------------------------------------------------------------------------------------------------------
	// GODOT EVENTS
	// -----------------------------------------------------------------------------------------------------------------

	// public override void _Ready()
	// {
	//     base._Ready();
	// 	Callable.From(() =>
	// 			this.InputManager.GetInputBuffer(this.ActionName).InputBufferDurationMs
	// 				= () => this.InputBufferTimeMs
	// 		)
	// 		.CallDeferred();
	// }

	public override void _Process(double delta)
	{
		base._Process(delta);
		if (this.CheckTransitionConditions(delta))
		{
			if (this.TargetState == null)
			{
				GD.PushError("Failed to activate input action. Name: ", this.InputActionName, ". Cause: ", this.TargetState, " this.State node is not set.");
				return;
			}
			if (this.Ability != null)
			{
				this.Ability.AddUseCount();
			}
			GD.PrintS(Time.GetTicksMsec(), nameof(InputActionTransition), ":", "⚡", "Action triggered.", "State:", this.State.Name, "Action:", this.InputActionName, "Transition:", this.TargetState);
			this.CancelActionCheckActive = true;
			this.StateMachine.QueueTransition(this.TargetState.Name);
		}
		else if (this.CancelActionCheckActive)
		{
			if (this.StateMachine.ActiveState != this.TargetState || !this.State.IsPreviousActiveState)
			{
				this.CancelActionCheckActive = false;
				return;
			}
			this.Ability?.AddUseTime(TimeSpan.FromMilliseconds(delta));
			if (this.CheckTransitionCancelingConditions())
			{
				if (this.Ability?.TimeLimitExceeded == true)
				{
					GD.PrintS(Time.GetTicksMsec(), nameof(InputActionTransition), ":", "⛔", "Ability time limit exceeded. State:", this.State.Name, "Action:", this.InputActionName);
				}
				else
				{
					GD.PrintS(Time.GetTicksMsec(), nameof(InputActionTransition), ":", "↩", "Ability canceled.", "State:", this.State.Name, "Action:", this.InputActionName);
				}
				this.StateMachine.QueueTransition(this.StateTransitionOnAbilityCanceled ?? this.State);
			}
		}
	}

	private bool CheckTransitionConditions(double delta)
		=> this.State.IsActive
			&& this.Ability?.IsAvailable != false
			&& this.State.ActiveDuration.TotalMilliseconds >= this.EnabledFromTimeSec * 1000
			&& this.State.ActiveDuration.TotalMilliseconds < this.DisabledAfterTimeSec * 1000
			&& (
				this.PreviousState == null
				|| this.StateMachine.PreviousState == this.PreviousState
			)
			&& (
				this.InputMode == AbilityActivationMode.InputIsDown
				&& !string.IsNullOrEmpty(this.InputActionName)
				&& Input.IsActionPressed(this.InputActionName)
				|| this.InputMode == AbilityActivationMode.InputIsJustDown
				&& !string.IsNullOrEmpty(this.InputActionName)
				&& this.InputManager.GetInputBuffer(this.InputActionName).ConsumeInput()
				|| this.InputMode == AbilityActivationMode.InputIsReleased
				&& !string.IsNullOrEmpty(this.InputActionName)
				&& !Input.IsActionPressed(this.InputActionName)
			);

	private bool CheckTransitionCancelingConditions()
		=> this.Ability?.TimeLimitExceeded == true
			|| this.CancelingInputMode == AbilityCancelingMode.OnRelease
			&& !string.IsNullOrEmpty(this.InputActionName)
			&& !Input.IsActionPressed(this.InputActionName)
			|| this.CancelingInputMode == AbilityCancelingMode.OnToggle
			&& !string.IsNullOrEmpty(this.InputActionName)
			&& this.InputManager.GetInputBuffer(this.InputActionName).ConsumeInput();
}
