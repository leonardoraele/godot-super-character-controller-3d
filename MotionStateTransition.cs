using System;
using Godot;

namespace Raele.SuperCharacter3D;

public class MotionStateTransition {
	public string? PreviousStateName { get; init; }
	public string NextStateName { get; init; } = "";
	public Variant? Data { get; init; }
	public bool Canceled { get; private set; }
	public event Action? CanceledEvent;
	public void Cancel() {
		this.Canceled = true;
		this.CanceledEvent?.Invoke();
	}

	public Godot.Collections.Dictionary<string, Variant> AsDictionary()
		=> new Godot.Collections.Dictionary<string, Variant>() {
			{ "previous_state_name", this.PreviousStateName ?? "" },
			{ "next_state_name", this.NextStateName },
			{ "data", this.Data ?? 0 },
			{ "cancel", Callable.From(this.Cancel) }
		};

	public static MotionStateTransition FromDictionary(Godot.Collections.Dictionary<string, Variant> dict)
	{
		MotionStateTransition transition = new() {
			PreviousStateName = dict["previous_state_name"].AsString(),
			NextStateName = dict["next_state_name"].AsString(),
			Data = dict["data"],
		};
		transition.CanceledEvent += () => dict["cancel"].As<Callable>().Call();
		return transition;
	}
}
