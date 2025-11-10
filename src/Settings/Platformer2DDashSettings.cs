using Godot;

namespace Raele.SuperCharacterController2D;

[GlobalClass]
public partial class Platformer2DDashSettings : Resource
{
	/// <summary>
	// /// <summary>
	// /// Determines how much time it takes for the character to regain the ability to dash, in miliseconds,
	// /// after they have ended the previous dash.
	// ///
	// /// The controller starts the timer as soon as the previous dash ends, either because it was canceled by the
	// /// player, because the dash duration ended, or because it was interruped by the character being taking damage.
	// ///
	// /// Set this property to float.PositiveInfinity (default) to disable the ability to dash on the ground.
	// /// </summary>
	// [Export] public float DashCooldownMs { get; private set; } = float.PositiveInfinity; // TODO
	/// <summary>
	/// Replaces the character's normal maximum Speed the Dash action is performed.
	/// </summary>
	[Export] public float DashMaxSpeedPxPSec { get; private set; } = 330;
	/// <summary>
	/// Acceleration applied to the character for as long as the Dash lasts.
	/// </summary>
	[Export] public float DashAccelerationPxPSecSq { get; private set; } = 600;
	/// <summary>
	/// This is the deceleration (negative acceleration) that is applies to the character's velocity every physics tick
	/// while the Dash is active, if the player inputs a direction that is opposite to the direction of the Dash.
	/// Increase this value if you want the character to turn around faster while dashing; or decrease it if you want
	/// the character to take some time to change the direction of the dash. Setting this property to 0 will make it
	/// impossible for the player to change the character's direction until the dash ends.
	///
	/// Note that, if <code>VariableDashLength</code> is true, the player is able to cancel the dash by releasing the
	/// Dash button or performing another action, such as jumping. In this case, waking deceleration or jump air control
	/// acceleration will be used instead.
	/// </summary>
	[Export] public float DashDecelerationPxPSecSq { get; private set; } = 900;
	/// <summary>
	/// How much time the Dash lasts if not canceled earlier. (see CanCancelDash)
	/// </summary>
	[Export] public ulong DashMaxDurationMs { get; private set; } = 500;
	/// <summary>
	/// It this is true, the player is able to adjust the duration of the dash by releasing the Dash button to end the
	/// dash earlier. Likewise, the player is also able to jump during the dash to end it earlier.
	/// </summary>
	[Export] public bool VariableDashLength { get; private set; } = true;
	/// <summary>
	/// If true, performing a Dash cancels any vertical momentum (in any direction) the character had before starting
	/// the Dash and makes the character unaffected by gravity for as long as the Dash lasts.
	/// </summary>
	[Export] public bool GroundDashIgnoresGravity { get; private set; } = false;

	[ExportGroup("Air Dashing")]
	/// <summary>
	/// Determines how many times a chracter can perform a dash in the air before they have to land on the floor to
	/// regain the ability to dash. Set this property to 0 (default) to disable the ability to Dash in the air
	/// altogether.
	/// </summary>
	[Export] public int MaxAirDashCount { get; private set; } = 0;
	/// <summary>
	/// If true, performing a Dash cancels any vertical momentum (in any direction) the character had before starting
	/// the Dash and makes the character unaffected by gravity for as long as the Dash lasts.
	/// </summary>
	[Export] public bool AirDashIgnoresGravity { get; private set; } = true;
}
