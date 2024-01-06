using Godot;
using Godot.Collections;

namespace Raele.SuperPlatformer;

public partial class InteractingState : BaseMotionState
{
	public double DurationSec = 1;
	public string? Hint;
	public Callable? OnFinished;
	public bool Ended { get; private set; } = false;
	private bool IsDue => this.DurationActiveMs >= this.DurationSec * 1000;

    public override void OnEnter(TransitionInfo transition)
    {
        base.OnEnter(transition);
        Dictionary<string, Variant>? dict = transition.Data?.AsGodotDictionary<string, Variant>();
		this.DurationSec = dict?.TryGetValue(nameof(this.DurationSec), out Variant durationSec) == true
			? durationSec.AsDouble()
			: 1.5f;
		if (dict?.TryGetValue(nameof(this.Hint), out Variant hint) == true) this.Hint = hint.AsString();
		if (dict?.TryGetValue(nameof(this.OnFinished), out Variant onFinished) == true) this.OnFinished = onFinished.AsCallable();
		this.Ended = false;
	}

    public override void OnProcessState(float delta)
    {
        base.OnProcessState(delta);
		if (this.IsDue) {
			this.Ended = true;
			if (this.OnFinished != null) {
				this.OnFinished.Value.Call();
			}
			if (this.Character.State == this && this.Ended) {
				this.Character.ResetState();
			}
		}
	}
}
