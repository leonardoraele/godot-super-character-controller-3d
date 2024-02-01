using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Raele.SuperCharacter3D.MotionStateControllers;

[GlobalClass]
public partial class CollisionShapeEnabler : MotionStateController
{
	// TODO When MotionStateControllers are refactored to nodes, this should become a reference to a CollisionShape3D
	[Export] public string? CollisionShape;
	[Export] public CollisionShapeEnablingMode Mode = CollisionShapeEnablingMode.DisableOtherShapes;

	[ExportGroup("Space Check")]
	[Export] public SpaceLackingBehavior OnSpaceLacking = SpaceLackingBehavior.Ignore;
	/// <summary>
	/// This is the margin for the physics space query used to check for available space before enabling new collision
	/// shapes. This is an extra value added to the shape's margin.
	///
	/// Set it to a negative value to create some tolerance for the space check. This is useful when you want to enable
	/// a shape that is just barely touching another shape. For example, if you want to enable a collision shape for
	/// when a character is crouching, you might want to set this to a negative value to allow the shape to be enabled
	/// when it's already touching the ground. Otherwise the controller will behave as if there's not enough space for
	/// the new shape to be enabled. If <see cref="OnSpaceLacking"/> is set to
	/// <see cref="SpaceLackingBehavior.Ignore"/>, no space check is performed, so this property is not used.
	/// </summary>
	[Export] public float SpaceQueryMarginOffset = -0.08f;

    private List<uint> AffectedShapeOwners = new();

    public enum CollisionShapeEnablingMode {
		// TODO // FIXME This option breaks when there are multiple CollisionShapeEnables in the same state, because
		// they override each other.
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
				Margin = shape.Margin + this.SpaceQueryMarginOffset,
				Transform = character.Transform * transform,
				Shape = shape,
				CollisionMask = character.CollisionMask,
				Exclude = new Godot.Collections.Array<Rid>() { character.GetRid() },
			});
		return collisions.Count == 0;
		// This might be needed if margins are not enough to prevent false positives
		// for (int i = 0; i < collisions.Count; i += 2) {
		// 	if (collisions[i].DistanceSquaredTo(collisions[i + 1]) > 0.0001f) {
		// 		return false;
		// 	}
		// }
		// return true;
	}
}
