using Godot;
using Raele.SuperCharacter3D.MotionStates;

namespace Raele.SuperCharacter3D;

public partial class InputActionAbility : Node
{
	[Export] public BaseMotionState? OnActivated;

	[ExportGroup("Activation Conditions")]
	/// <summary>
	/// Name of the input action to be read for this ability.
	/// </summary>
	[Export] public string InputActionName = "";
	[Export] public AbilityActivationMode InputMode = AbilityActivationMode.InputIsJustPressed;
	[Export] public float EnabledFromTimeSec = 0f;
	[Export] public float DisabledAfterTimeSec = float.PositiveInfinity;
	[Export] public BaseMotionState? PreviousState;
	/// <summary>
	/// Time in milliseconds to buffer input for this ability. Set to 0 to disable input buffering for this ability.
	/// </summary>
	[Export] public ulong InputBufferTimeMs = 150;
	[Export] public AbilityData? AbilityData;

	[ExportGroup("Ability Canceling")]
	[Export] public AbilityCancelingMode CancelingInputMode = AbilityCancelingMode.Never;
	/// <summary>
	/// This is the state the character will be transitioned to when this ability is canceled. The ability is canceled
	/// when the input is released (if input mode is Hold) or when the input is pressed again (if input mode is Toggle).
	/// If the input mode is Trigger, this property is ignored. If this property is null, the character will not be
	/// transitioned to another state when the ability is canceled.
	/// </summary>
	[Export] public BaseMotionState? OnCanceled;

	// [ExportGroup("Activated State End Override")]
	// /// <summary>
	// /// This is the state the character will be transitioned to when this ability is ended. The ability is ended when
	// /// it comed to an end by its own logic. For example, the PropelState comes to an end when after it lasts for enough
	// /// time, according to the configured settings; and the FallState comes to an end when the character touches the
	// /// ground.
	// /// </summary>
	// [Export] public BaseMotionState? OnEnded;

    private BaseMotionState State = null!;

	public enum AbilityActivationMode {
		InputIsPressed,
		InputIsJustPressed,
	}

	public enum AbilityCancelingMode {
		Never,
		OnRelease,
		OnToggle,
	}

	public override void _EnterTree()
	{
		if (this.GetParent() is BaseMotionState motionState) {
			this.State = motionState;
		} else {
			GD.PushError("Node ", this.GetType().Name, " must be a child of ", nameof(BaseMotionState));
			this.QueueFree();
		}
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

    public override void _Process(double delta)
    {
        base._Process(delta);
		if (
			this.State.IsActive
			&& this.AbilityData?.IsAvailable != false
			&& this.State.DurationActiveMs >= this.EnabledFromTimeSec * 1000
			&& this.State.DurationActiveMs < this.DisabledAfterTimeSec * 1000
			&& (
				this.PreviousState == null
				|| this.State.StateMachine.PreviousState?.Name == this.PreviousState.Name
			)
			&& (
				this.InputMode == AbilityActivationMode.InputIsPressed
				&& !string.IsNullOrEmpty(this.InputActionName)
				&& Input.IsActionPressed(this.InputActionName)
				|| this.InputMode == AbilityActivationMode.InputIsJustPressed
				&& this.State.StateMachine.Character.InputController.GetInputBuffer(this.InputActionName).ConsumeInput()
			)
		) {
			if (this.OnActivated == null) {
				GD.PushError("Failed to activate ability. Ability node: ", this.Name, ". Cause: ", nameof(this.OnActivated)," state node is not set.");
			} else {
				if (this.AbilityData != null) {
					this.AbilityData.UseCount++;
				}
				GD.PrintS(Time.GetTicksMsec(), nameof(InputActionAbility), ":", "⚡", this.Name, "activated.");
				this.State.StateMachine.Transition(this.OnActivated.Name);
			}
		} else if (
			this.State.IsPreviousActiveState
			&& this.State.StateMachine.CurrentState?.Name == this.OnActivated?.Name
		) {
			if (this.AbilityData != null) {
				this.AbilityData.AccumulatedUseTimeSec += delta;
			}
			if (
				this.AbilityData?.TimeLimitExceeded == true
				|| this.CancelingInputMode == AbilityCancelingMode.OnRelease
				&& !string.IsNullOrEmpty(this.InputActionName)
				&& !Input.IsActionPressed(this.InputActionName)
				|| this.CancelingInputMode == AbilityCancelingMode.OnToggle
				&& this.State.StateMachine.Character.InputController.GetInputBuffer(this.InputActionName).ConsumeInput()
			) {
				if (this.AbilityData?.TimeLimitExceeded == true) {
					GD.PrintS(Time.GetTicksMsec(), nameof(InputActionAbility), ":", "⛔", this.Name, "time limit exceeded.");
				} else {
					GD.PrintS(Time.GetTicksMsec(), nameof(InputActionAbility), ":", "↩", this.Name, "canceled.");
				}
				this.State.Character.StateMachine.Transition(this.OnCanceled?.Name ?? this.State.Name);
			}
		}
	}
}
