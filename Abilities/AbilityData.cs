using Godot;

namespace Raele.SuperCharacter3D;

public partial class AbilityData : Node
{
	[ExportGroup("Usage Limits")]
	/// <summary>
	/// If this is greater than zero, the ability will be disabled after this many uses. If this is zero, the ability
	/// can be used an unlimited number of times.
	/// </summary>
	[Export] public int MaxUseCount = -1;
	[Export] public double MaxUseTimeSec = double.PositiveInfinity;

	public int UseCount = 0;
	public double AccumulatedUseTimeSec = 0;

	public bool IsAvailable => !this.UseCountExceeded && !this.TimeLimitExceeded;
	public bool UseCountExceeded => this.MaxUseCount != -1 && this.UseCount >= this.MaxUseCount;
	public bool TimeLimitExceeded => this.AccumulatedUseTimeSec >= this.MaxUseTimeSec;

    public void Recharge()
    {
		this.UseCount = 0;
		this.AccumulatedUseTimeSec = 0;
    }
}
