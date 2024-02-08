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
	public Vector2 RawMovementInput { get; private set; }
    public Vector3 GlobalMovementInput { get; private set; }
	/// <summary>
	/// Determines how the InputController deals with player directional input when there are changes in camera angle.
	///
	/// This property changes how <see cref="GlobalMovementInput"/> is updated. If your character controls don't rely on
	/// global input, this property is irrelevant. For example, games that use "tank controls" don't rely on the camera
	/// perspective for character control.
	///
	/// For games that alternate bewtween different camera modes, (static and dynamic) it is recommended that this
	/// property is properly updated whenever a different camera mode is used. e.g. set this to StaticCamera in sections
	/// where the camera is static and then set it to DynamicCameraCut when the camera becomes dynamic again.
	/// </summary>
	public CameraModeEnum CameraMode = CameraModeEnum.DynamicCamera;

    private Dictionary<string, InputBuffer> InputBufferDict = new();
	private SuperCharacter3DController Character;
    private float CameraRotation;

	public enum CameraModeEnum {
		/// <summary>
		/// In this mode, the camera angle will be considered at every frame. This means
		/// <see cref="GlobalMovementInput"/> will be updated every frame according to the currently active camera, even
		/// if the camera moves, rotates, or another camera becomes active, the input direction will be updated every
		/// frame based to the new camera parameters.
		///
		/// For example, if the player is pressing input Forward while the camrea rotates, the input will point to the
		/// forward direction of the camera at every frame as it rotates. Likewise, if a camera cut happens, the input
		/// will point toward the forward direction of the new active camera.
		///
		/// This mode is best suited for games where the camera moves during gameplay, specially if the player is able
		/// to control the camera.
		///
		/// For games that feature dynamic cameras and also performs camera cuts, see <see cref="DynamicCameraCut"/>.
		/// </summary>
		DynamicCamera,
		/// <summary>
		/// In this mode, the InputController will remember the angle of the camera when the player starts a directional
		/// input and will update <see cref="GlobalMovementInput"/> every frame according to that fixed camera angle,
		/// even if the camera moves, rotates, or another camera becomes active while the player enters directional
		/// input.
		///
		/// This mode allows the player to keep their input direction even after a camera cut.
		///
		/// This mode is best suited for games that performs camera cuts between several static camera angles.
		/// </summary>
		StaticCamera,
		/// <summary>
		/// This mode behaves like StaticCamera, but the InputController will change this camera mode to
		/// <see cref="DynamicCamera"/> automatically when the player releases the directional input.
		///
		/// This mode is intended to be used when the game performs a camera cut to a dynamic camera. If this is is the
		/// case, change the camera mode to this mode every time a camera cut is performed.
		/// </summary>
		DynamicCameraCut,
	}

	public InputController(SuperCharacter3DController character)
		=> this.Character = character;

	public void Update()
	{
		this.RawMovementInput = Input.GetVector(
			this.Character.MoveLeftAction,
			this.Character.MoveRightAction,
			this.Character.MoveBackAction,
			this.Character.MoveForwardAction
		);
		this.MovementInput = new Vector2(this.RawMovementInput.X, this.RawMovementInput.Y * -1);
		if (this.CameraMode == CameraModeEnum.DynamicCamera) {
			this.CameraRotation = this.Character.GetViewport().GetCamera3D().Rotation.Y * -1;
			this.GlobalMovementInput = GodotUtil.HV2ToV3(this.MovementInput.Rotated(this.CameraRotation));
		} else {
			if (this.RawMovementInput.LengthSquared() <= Mathf.Epsilon) {
				this.CameraRotation = this.Character.GetViewport().GetCamera3D().Rotation.Y * -1;
				this.GlobalMovementInput = Vector3.Zero;
				if (this.CameraMode == CameraModeEnum.DynamicCameraCut) {
					this.CameraMode = CameraModeEnum.DynamicCamera;
				}
			} else {
				this.GlobalMovementInput = GodotUtil.HV2ToV3(this.MovementInput.Rotated(this.CameraRotation));
			}
		}
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
