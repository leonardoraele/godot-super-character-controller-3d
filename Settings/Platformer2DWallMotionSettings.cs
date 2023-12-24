using Godot;

namespace Raele.SuperPlatformer;

[GlobalClass]
public partial class Platformer2DWallMotionSettings : Resource
{
	// /// <summary>
	// /// If true, wall climbing automatically takes priority over wall sliding; otherwise, requires wall climb input to grab onto the wall.
	// /// </summary>
	// [Export] public bool WallClimbByPressingAgainstWall { get; private set; } = false;
	// /// <summary>
	// /// If false, player upward velocity will be preserved when they start climbing, and deceleration will be applied.
	// /// </summary>
	// [Export] public bool WallClimbCancelsUpwardVelocity { get; private set; } = false;

	[ExportGroup("Wall Climbing")]
	[Export] public float UpwardClimbMaxSpeedPxPSec { get; private set; } = 150;
	[Export] public float UpwardClimbAccelerationPxPSecSq { get; private set; } = 300;
	[Export] public float DownardClimbMaxSpeed { get; private set; } = 250;
	[Export] public float DownwardClimbAccelerationPxPSecSq { get; private set; } = 1000;
	/// <summary>
	/// Maximum duration the character is able to remain stick to the wall. After this time, the character will slide
	/// down the wall. Set this property to 0 to make the character only able to slide and not climb.
	/// </summary>
	[Export] public float WallClimbMaxDurationSec { get; private set; } = float.PositiveInfinity;
	/// <summary>
	/// Is true, the character will automatically transition to the floor when they reach it. Otherwise, the character
	/// remains on the wall climb state until the player release the wall climb button or move the character away from
	/// the wall.
	/// </summary>
	[Export] public bool AutomaticallyTransitionToFloor { get; private set; } = false;

	[ExportGroup("Wall Sliding")]
	/// <summary>
	/// This downward max speed replaces fall max speed while the player is pressing themselves against a wall; if <= 0, player can't slide.
	/// </summary>
	[Export] public float SlideMaxSpeedPxPSec { get; private set; } = 120;
	/// <summary>
	/// This vertical acceleration replaces gravity while the player is pressing themselves against a wall.
	/// </summary>
	[Export] public float SlideAccelerationPxPSecSq { get; private set; } = 600;

	[ExportGroup("Wall Jump")]
	/// <summary>
	/// Speed applied when the character jumps from a wall.
	/// The sign/direction of each axis is be ignored; only the module is used.
	///
	/// This speed is allied when the character jumps from a wall regardless if coming them a wall climb, wall slide,
	/// or neither (i.e. even if jump climb and slide are disabled and the character is just jumping while pressing
	/// against a wall).
	/// </summary>
	[Export] public float WallJumpKickoffSpeedPxPSec = 150;

	// public enum InputType // TODO
	// {
	// 	/// <summary>
	// 	/// The character will climb the wall automatically when they press against it.
	// 	/// </summary>
	// 	Automatic,
	// 	/// <summary>
	// 	/// The character will climb the wall when they press against it and press the climb button, and will remain
	// 	/// stick to the wall until the player release the climb button.
	// 	/// </summary>
	// 	Manual,
	// 	/// <summary>
	// 	/// The character will climb the wall when they press against it and press the climb button. Once they are on
	// 	/// the wall, they will remain stick to the wall until the player moves away from the wall or jumps off.
	// 	/// Pressing the climb button again will also make the character let go off the wall.
	// 	/// </summary>
	// 	ManualToggle,
	// }
}
