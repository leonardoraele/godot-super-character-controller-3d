using System;
using Godot;

namespace Raele.SuperPlatformer;

public class SuperPlatformerInputController {
	public class InputBuffer {
		public ulong LastInputTime { get; private set; } = 0;
		public bool IsInputBuffered => this.LastInputTime >= Time.GetTicksMsec() - this.InputBufferDurationMs();
		// The respon why we get these variables as functions instead of plain values is so that we can always get the
		// most updated value, since they might change at runtime if the user tweaks with the inspector variables.
		// TODO A possible optimization would be to save the buffer duration and input action name in a field if game
		// was build for production so we don't need to call an anonymous function every frame; but I don't know how to
		// use (of if it's suppoerted) build-time variables.
		public Func<ulong> InputBufferDurationMs { get; init; } = () => 0;
		public Func<string> InputActionName { get; init; } = () => "";
		public bool ConsumeInput() {
            bool result = this.IsInputBuffered;
			this.LastInputTime = 0;
			return result;
		}
		public void Update() {
			if (Input.IsActionJustPressed(this.InputActionName())) {
				this.ProduceInput();
			}
		}
		public void ProduceInput() => this.LastInputTime = Time.GetTicksMsec();
	}

	public Vector2 MovementInput { get; private set; }
	public InputBuffer JumpInputBuffer { get; private set; }
	public InputBuffer DashInputBuffer { get; private set; }

	public bool Enabled = true;
	private ulong EnabledUntilTime = 0;
	private SuperPlatformerController Character;

	public SuperPlatformerInputController(SuperPlatformerController character)
	{
		this.Character = character;
		this.JumpInputBuffer = new InputBuffer() {
			InputBufferDurationMs = () => this.Character.JumpInputBufferSensitivityMs,
			InputActionName = () => this.Character.InputSettings.JumpAction
		};
		this.DashInputBuffer = new InputBuffer() {
			InputBufferDurationMs = () => this.Character.DashInputBufferSensitivityMs,
			InputActionName = () => this.Character.InputSettings.DashAction
		};
	}

	public void Update()
	{
		if (this.EnabledUntilTime > 0 && Time.GetTicksMsec() >= this.EnabledUntilTime) {
			this.Enabled = false;
			this.EnabledUntilTime = 0;
		}
		if (!this.Enabled) {
			this.MovementInput = Vector2.Zero;
			return;
		}
		this.MovementInput = Input.GetVector(
			this.Character.InputSettings.MoveLeftAction,
			this.Character.InputSettings.MoveRightAction,
			this.Character.InputSettings.MoveUpAction,
			this.Character.InputSettings.MoveDownAction
		);
		this.JumpInputBuffer.Update();
		this.DashInputBuffer.Update();
	}
}
