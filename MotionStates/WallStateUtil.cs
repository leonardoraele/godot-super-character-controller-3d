using System;
using Godot;

namespace Raele.SuperCharacter3D.MotionStates;

public static class WallStateUtil
{
    // [Export] public FacingDirectionMode FacingDirection = FacingDirectionMode.Preserve;
    // TODO
    // [Export] public float TurnSpeedDegPSec = float.PositiveInfinity;

    public enum FacingDirectionMode {
        Preserve,
        TowardWall,
        AwayFromWall,
        // TODO
        // PositiveTangent,
        // NegativeTangent,
        ClosestTangent,
        FurthestTangent,
        // TODO
        // StandOnWall,
        // _180TowardWall,
        // _180AwayFromWall,
    }

    // public override HorizontalMovement GetHorizontalMovement()
    //     => new() {
    //         TargetSpeedUnPSec = 0f,
    //         TargetDirection = this.GetTargetFacingDirection(),
    //         RotationSpeedDegPSec = this.TurnSpeedDegPSec,
    //     };

    public static Vector2 GetTargetFacingDirection(FacingDirectionMode facingDirection, SuperCharacter3DController character)
    {
        switch (facingDirection) {
            case FacingDirectionMode.Preserve:
                return GodotUtil.V3ToHV2(character.Basis.Z * -1);
            case FacingDirectionMode.TowardWall:
                return GodotUtil.V3ToHV2(character.GetWallNormal() * -1);
            case FacingDirectionMode.AwayFromWall:
                return GodotUtil.V3ToHV2(character.GetWallNormal());
            // case FacingDirectionMode.PositiveTangent:
            //     return GodotUtil.V3ToHV2(character.GetWallTangent());
            // case FacingDirectionMode.NegativeTangent:
            //     return GodotUtil.V3ToHV2(character.GetWallTangent() * -1);
            case FacingDirectionMode.ClosestTangent: {
                Vector2 tangent = GodotUtil.V3ToHV2(character.GetWallNormal()).Rotated((float) Math.PI / 2);
                return tangent.AngleTo(GodotUtil.V3ToHV2(character.Basis.Z * -1)) < Math.PI / 2
                    ? tangent
                    : tangent * -1;
            }
            case FacingDirectionMode.FurthestTangent: {
                Vector2 tangent = GodotUtil.V3ToHV2(character.GetWallNormal()).Rotated((float) Math.PI / 2);
                return tangent.AngleTo(GodotUtil.V3ToHV2(character.Basis.Z * -1)) < Math.PI / 2
                    ? tangent * -1
                    : tangent;
            }
            // case FacingDirectionMode.StandOnWall:
            //     return character.GetWallNormal().Rotated(Math.PI);
            // case FacingDirectionMode._180TowardWall:
            //     return character.GetWallNormal().Rotated(Math.PI / 2);
            // case FacingDirectionMode._180AwayFromWall:
            //     return character.GetWallNormal().Rotated(-Math.PI / 2);
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}
