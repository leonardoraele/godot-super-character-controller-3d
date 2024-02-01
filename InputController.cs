using System;
using System.Collections.Generic;
using Godot;

namespace Raele.SuperCharacter3D;

public class InputController
{
	public class InputBuffer {
		public ulong LastInputTime { get; private set; } = 0;
		// TODO // FIXME If this.InputBufferDurationMs() is 0, we will never get input. Also, if the game lags beyond
		// this.InputBufferDurationMs() time, we will lose input.
		public bool IsInputBuffered => this.LastInputTime >= Time.GetTicksMsec() - this.InputBufferDurationMs();
		// The respon why we get these variables as functions instead of plain values is so that we can always get the
		// most updated value, since they might change at runtime if the user tweaks with the inspector variables.
		// TODO A possible optimization would be to save the buffer duration and input action name in a field if game
		// was build for production so we don't need to call an anonymous function every frame; but I don't know how to
		// use build-time variables. (nor if it's even supported)
		public Func<ulong> InputBufferDurationMs { get; set; } = () => 150;
		public string InputActionName { get; set; } = "";
		public bool ConsumeInput()
		{
            bool result = this.IsInputBuffered;
			this.LastInputTime = 0;
			return result;
		}
		public void Update()
		{
			if (!string.IsNullOrEmpty(this.InputActionName) && Input.IsActionJustPressed(this.InputActionName)) {
				this.ProduceInput();
			}
		}
		public void ProduceInput() => this.LastInputTime = Time.GetTicksMsec();
	}

	[Obsolete]
	public Vector2 MovementInput { get; private set; }
    public Vector3 GlobalMovementInput { get; private set; }
    public Vector2 LocalMovementInput { get; private set; }
    public float TurnInput { get; private set; }
    private Dictionary<string, InputBuffer> InputBufferDict = new();

	private ISuperPlatformer3DCharacter Character;

	public interface ISuperPlatformer3DCharacter {
		public InputSettings InputSettings { get; }
		public Viewport GetViewport();
	}

	public InputController(ISuperPlatformer3DCharacter character)
	{
		this.Character = character;
	}

	public void Update()
	{
		this.MovementInput = Input.GetVector(
			this.Character.InputSettings.MoveCameraLeftAction,
			this.Character.InputSettings.MoveCameraRightAction,
			this.Character.InputSettings.MoveCameraFrontAction,
			this.Character.InputSettings.MoveCameraBackAction
		);
		this.GlobalMovementInput = GodotUtil.HV2ToV3(
			this.MovementInput.Rotated(this.Character.GetViewport().GetCamera3D().Rotation.Y * -1)
		);
		this.LocalMovementInput = Input.GetVector(
			this.Character.InputSettings.StrafeLeftAction,
			this.Character.InputSettings.StrafeRightAction,
			this.Character.InputSettings.MoveBackwardAction,
			this.Character.InputSettings.MoveForwardAction
		);
		this.TurnInput = Input.GetAxis(this.Character.InputSettings.TurnLeftAction, this.Character.InputSettings.TurnRightAction);
		foreach (var inputBuffer in this.InputBufferDict.Values) {
			inputBuffer.Update();
		}
	}

	public InputBuffer GetInputBuffer(string actionName)
	{
		if (!this.InputBufferDict.ContainsKey(actionName)) {
			this.InputBufferDict[actionName] = new InputBuffer() { InputActionName = actionName };
		}
		return this.InputBufferDict[actionName];
	}

	/// <summary>
	/// Returns the movement input relative to the camera's perspective.
	/// </summary>
	public Vector2 GetRelativeMovementInput()
		=> this.MovementInput.Rotated(this.Character.GetViewport().GetCamera3D().Rotation.Y * -1);
}
