using System;
using Godot;

namespace Raele.SuperCharacterController3D;

public class InputController
{
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
		public bool ConsumeInput()
		{
            bool result = this.IsInputBuffered;
			this.LastInputTime = 0;
			return result;
		}
		public void Update()
		{
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
	private ISuperPlatformer3DCharacter Character;

	public interface ISuperPlatformer3DCharacter {
		public SuperPlatformer3DBaseSettings Settings { get; }
	}

	public InputController(ISuperPlatformer3DCharacter character)
	{
		this.Character = character;
		this.JumpInputBuffer = new InputBuffer() {
			InputBufferDurationMs = () => this.Character.Settings.JumpInputBufferSensitivityMs,
			InputActionName = () => this.Character.Settings.Input.JumpAction
		};
		this.DashInputBuffer = new InputBuffer() {
			InputBufferDurationMs = () => this.Character.Settings.DashInputBufferSensitivityMs,
			InputActionName = () => this.Character.Settings.Input.DashAction
		};
	}

	public void Update()
	{
		if (!this.Enabled) {
			this.MovementInput = Vector2.Zero;
			return;
		}
		this.MovementInput = Input.GetVector(
			this.Character.Settings.Input.MoveLeftAction,
			this.Character.Settings.Input.MoveRightAction,
			this.Character.Settings.Input.MoveForwardAction,
			this.Character.Settings.Input.MoveBackwardAction
		);
		this.JumpInputBuffer.Update();
		this.DashInputBuffer.Update();
	}
}
