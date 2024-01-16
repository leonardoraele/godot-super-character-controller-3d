using System;
using Godot;

namespace Raele.SuperCharacter3D;

public class StateTransition {
	public string? PreviousStateName { get; init; }
	public string NextStateName { get; init; } = "";
	public Variant? Data { get; init; }
	public bool Canceled { get; private set; }
	public event Action? CanceledEvent;
	public void Cancel() {
		this.Canceled = true;
		this.CanceledEvent?.Invoke();
	}
	public StateTransition Chain(StateTransition other) {
		other.CanceledEvent += this.Cancel;
		return other;
	}
}
