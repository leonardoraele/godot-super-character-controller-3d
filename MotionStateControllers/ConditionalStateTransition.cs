using Godot;

namespace Raele.SuperCharacter3D.MotionStateControllers;

public partial class ConditionalStateTransition : MotionStateController {
	[Export] public Node? Subject;
	// TODO Should get the condition from a resource type so that we can make several condition types. One of the
	// condition types could be an expression.
	[Export] public string Property = "";
	[Export] public bool Not = false;
	[Export] public Node? NextState;

    private bool IsMethod;

	public enum ValueTypeEnum {
		Property,
		Method
	}

    public override void OnEnter(MotionStateTransition transition)
    {
        base.OnEnter(transition);
		this.IsMethod = this.Subject?.HasMethod(this.Property) == true;
    }

    public override void OnProcessStateActive(float delta)
    {
        base.OnProcessStateActive(delta);
		if (
			this.NextState != null
			&& this.Subject != null
			&& !string.IsNullOrEmpty(this.Property)
			&& (
					this.IsMethod
						? this.Subject.Call(this.Property).AsBool()
						: this.Subject.Get(this.Property).AsBool()
				)
				!= this.Not
		) {
			this.StateMachine.Transition(this.NextState.Name);
		}
    }
}
