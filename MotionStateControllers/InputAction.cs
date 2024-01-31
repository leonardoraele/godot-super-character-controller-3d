using Godot;

namespace Raele.SuperCharacter3D.MotionStateControllers;

[GlobalClass]
public partial class InputAction : MotionStateController
{
	/// <summary>
	/// Name of the input action to be read for this ability.
	/// </summary>
	[Export] public string? InputActionName;
	[Export] public string? StateTransition;
	[Export] public AbilityActivationMode InputMode = AbilityActivationMode.InputIsJustPressed;

	[ExportGroup("Activation Conditions")]
	/// <summary>
	/// Time in seconds from the start of the state to enable this ability. This ability can only be triggered after the
	/// corresponding state has been active for at least this amount of time.
	/// </summary>
	[Export] public float EnabledFromTimeSec = 0f;
	/// <summary>
	/// Time in seconds from the start of the state to disable this ability. This ability can no longer be triggered
	/// after the corresponding state has been active for this amount of time.
	/// </summary>
	[Export] public float DisabledAfterTimeSec = float.PositiveInfinity;
	[Export] public string? PreviousState;
	/// <summary>
	/// Time in milliseconds to buffer input for this ability. Set to 0 to disable input buffering for this ability.
	/// </summary>
	[Export] public ulong InputBufferTimeMs = 150;
	[Export] public AbilityData? AbilityData;

	[ExportGroup("Action Canceling")]
	[Export] public AbilityCancelingMode CancelingInputMode = AbilityCancelingMode.Never;
	/// <summary>
	/// This is the state the character will be transitioned to when this ability is canceled. The ability is canceled
	/// when the input is released (if input mode is Hold) or when the input is pressed again (if input mode is Toggle).
	/// If the input mode is Trigger, this property is ignored. If this property is null, the character will not be
	/// transitioned to another state when the ability is canceled.
	/// </summary>
	[Export] public string? StateTransitionOnAbilityCanceled;

	private bool CancelActionCheckActive = false;

	// [ExportGroup("Activated State End Override")]
	// /// <summary>
	// /// This is the state the character will be transitioned to when this ability is ended. The ability is ended when
	// /// it comed to an end by its own logic. For example, the PropelState comes to an end when after it lasts for enough
	// /// time, according to the configured settings; and the FallState comes to an end when the character touches the
	// /// ground.
	// /// </summary>
	// [Export] public BaseMotionState? OnEnded;

	public enum AbilityActivationMode {
		InputIsPressed,
		InputIsJustPressed,
	}

	public enum AbilityCancelingMode {
		Never,
		OnRelease,
		OnToggle,
	}

    // public override void _Ready()
    // {
    //     base._Ready();
	// 	Callable.From(() =>
	// 			this.State.StateMachine.Character.InputController.GetInputBuffer(this.ActionName).InputBufferDurationMs
	// 				= () => this.InputBufferTimeMs
	// 		)
	// 		.CallDeferred();
    // }

    public override void OnProcess(ControlledState state, float delta)
    {
		if (
			state.IsActive
			&& this.AbilityData?.IsAvailable != false
			&& state.StateMachine.TimeSinceLastStateChangeMs >= this.EnabledFromTimeSec * 1000
			&& state.StateMachine.TimeSinceLastStateChangeMs < this.DisabledAfterTimeSec * 1000
			&& (
				string.IsNullOrEmpty(this.PreviousState)
				|| state.StateMachine.PreviousState?.Name == this.PreviousState
			)
			&& (
				this.InputMode == AbilityActivationMode.InputIsPressed
				&& !string.IsNullOrEmpty(this.InputActionName)
				&& Input.IsActionPressed(this.InputActionName)
				|| this.InputMode == AbilityActivationMode.InputIsJustPressed
				&& !string.IsNullOrEmpty(this.InputActionName)
				&& state.StateMachine.Character.InputController.GetInputBuffer(this.InputActionName).ConsumeInput()
			)
		) {
			if (string.IsNullOrEmpty(this.StateTransition)) {
				GD.PushError("Failed to activate input action. Name: ", this.InputActionName, ". Cause: ", this.StateTransition," state node is not set.");
			} else {
				if (this.AbilityData != null) {
					this.AbilityData.UseCount++;
				}
				GD.PrintS(Time.GetTicksMsec(), nameof(InputAction), ":", "⚡", "Action triggered.", "State:", state.Name, "Action:", this.InputActionName, "Transition:", this.StateTransition);
				this.CancelActionCheckActive = true;
				state.StateMachine.Transition(this.StateTransition);
			}
		} else if (this.CancelActionCheckActive) {
			this.CancelActionCheckActive = !string.IsNullOrEmpty(this.StateTransition)
				&& state.IsPreviousActiveState
				&& state.StateMachine.CurrentState?.Name == this.StateTransition;
			if (this.CancelActionCheckActive) {
				if (this.AbilityData != null) {
					this.AbilityData.AccumulatedUseTimeSec += delta;
				}
				if (
					this.AbilityData?.TimeLimitExceeded == true
					|| this.CancelingInputMode == AbilityCancelingMode.OnRelease
					&& !string.IsNullOrEmpty(this.InputActionName)
					&& !Input.IsActionPressed(this.InputActionName)
					|| this.CancelingInputMode == AbilityCancelingMode.OnToggle
					&& !string.IsNullOrEmpty(this.InputActionName)
					&& state.StateMachine.Character.InputController.GetInputBuffer(this.InputActionName).ConsumeInput()
				) {
					if (this.AbilityData?.TimeLimitExceeded == true) {
						GD.PrintS(Time.GetTicksMsec(), nameof(InputAction), ":", "⛔", "Ability time limit exceeded. State:", state.Name, "Action:", this.InputActionName);
					} else {
						GD.PrintS(Time.GetTicksMsec(), nameof(InputAction), ":", "↩", "Ability canceled.", "State:", state.Name, "Action:", this.InputActionName);
					}
					state.StateMachine.Transition(string.IsNullOrEmpty(this.StateTransitionOnAbilityCanceled) ? state.Name : this.StateTransitionOnAbilityCanceled);
				}
			}
		}
	}
}
