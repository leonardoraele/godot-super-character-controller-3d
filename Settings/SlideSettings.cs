using Godot;

namespace Raele.SuperCharacter3D;

[GlobalClass]
public partial class SlideSettings : Resource
{
	/// <summary>
	/// Minimum velocity to start sliding.
	///
	/// If set to 0, the character will able to slide regardless of their speed, even from idle position.
	///
	/// Set to any negative number to make the character only be able to slide when their at their maximum movement
	/// speed.
	/// </summary>
	[Export] public float MinSpeedUnPSec = -1;
	[Export] public DashSettings? Momentum;
}
