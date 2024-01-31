using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Raele.SuperCharacter3D.MotionStateControllers;

[GlobalClass]
public partial class CollisionShapeEnabler : MotionStateController
{
	// TODO When MotionStateControllers are refactored to nodes, this should become a reference to a CollisionShape3D
	// TODO Should be an array, because if the user needs to enable multiple shapes, they can't add multiple
	// CollisionShapeEnabler controllers since, when the mode is set to CollisionShapeEnablingMode.DisableOtherShapes,
	// it overrides the other shapes.
	[Export] public string? CollisionShape;
	[Export] public CollisionShapeEnablingMode Mode = CollisionShapeEnablingMode.DisableOtherShapes;
	[Export] public SpaceLackingBehavior OnSpaceLacking = SpaceLackingBehavior.Ignore;

    private List<uint> AffectedShapeOwners = new();

    public enum CollisionShapeEnablingMode {
		DisableOtherShapes,
		KeepOtherShapes,
	}

	public enum SpaceLackingBehavior {
		Ignore,
		CancelTransition,
		// PrioritizeOther,
		// PrioritizeThis,
	}

    public override void OnEnter(ControlledState state, MotionStateTransition transition)
    {
		if (string.IsNullOrEmpty(this.CollisionShape) || state.Character.GetNode(this.CollisionShape) is not CollisionShape3D shape) {
			throw new System.Exception($"{nameof(CollisionShapeEnabler)}.{nameof(this.CollisionShape)} property is not set, does not exist, or is not a CollisionShape3D. State: {state.Name}");
		}
		if (
			shape.Shape != null
			&& (
				this.OnSpaceLacking == SpaceLackingBehavior.CancelTransition
				// || this.OnSpaceLacking == SpaceLackingBehavior.PrioritizeOther
			)
			&& !this.CheckForSpace(state.Character, shape)
		) {
			transition.Cancel();
			return;
		}
		this.AffectedShapeOwners.Clear();
		if (this.Mode == CollisionShapeEnablingMode.DisableOtherShapes) {
			foreach (uint ownerId in state.Character.GetShapeOwners()) {
				if (!state.Character.IsShapeOwnerDisabled(ownerId)) {
					this.AffectedShapeOwners.Add(ownerId);
				}
			}
		}
		foreach (var ownerId in this.AffectedShapeOwners) {
			state.Character.ShapeOwnerSetDisabled(ownerId, true);
		}
		shape.Disabled = false;
	}

	public override void OnExit(ControlledState state, MotionStateTransition transition)
	{
		if (
			(
				this.OnSpaceLacking == SpaceLackingBehavior.CancelTransition
				// || this.OnSpaceLacking == SpaceLackingBehavior.PrioritizeThis
			)
			&& !this.AffectedShapeOwners.All(ownerId => this.CheckForSpace(state.Character, ownerId))
		) {
			transition.Cancel();
			return;
		}
		if (!string.IsNullOrEmpty(this.CollisionShape) && state.Character.GetNode(this.CollisionShape) is CollisionShape3D shape) {
			shape.Disabled = true;
		}
		foreach (uint ownerId in this.AffectedShapeOwners) {
			state.Character.ShapeOwnerSetDisabled(ownerId, false);
		}
    }

	private bool CheckForSpace(CollisionObject3D character, CollisionShape3D shape)
	{
		return this.CheckForSpace(character, shape.Shape, shape.Transform);
	}

	private bool CheckForSpace(CollisionObject3D character, uint shapeOwnerId)
	{
        return Enumerable.Range(0, character.ShapeOwnerGetShapeCount(shapeOwnerId))
			.Select(shapeIndex => character.ShapeOwnerGetShape(shapeOwnerId, shapeIndex))
			.All(shape => this.CheckForSpace(character, shape, character.ShapeOwnerGetTransform(shapeOwnerId)));
	}

	private bool CheckForSpace(CollisionObject3D character, Shape3D shape, Transform3D transform)
	{
        Godot.Collections.Array<Vector3> collisions = character.GetWorld3D()
			.DirectSpaceState
			.CollideShape(new() {
				Transform = character.Transform * transform,
				Shape = shape,
				CollisionMask = character.CollisionMask,
				Exclude = new Godot.Collections.Array<Rid>() { character.GetRid() },
			});
		for (int i = 0; i < collisions.Count; i += 2) {
			if (collisions[i].DistanceSquaredTo(collisions[i + 1]) > 0.0001f) {
				return false;
			}
		}
		return true;
	}
}
