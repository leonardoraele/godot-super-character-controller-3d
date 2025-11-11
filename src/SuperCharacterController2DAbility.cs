using System;
using Godot;

namespace Raele.SuperCharacterController2D;

public partial class SuperCharacterController2DAbility : Node
{
	// -----------------------------------------------------------------------------------------------------------------
	// EXPORTS
	// -----------------------------------------------------------------------------------------------------------------

	[ExportGroup("Limits")]
	/// <summary>
	/// If this is greater than zero, the ability will be disabled after this many uses. If this is zero, the ability
	/// can be used an unlimited number of times.
	/// </summary>
	[Export] public int MaxUseCount = -1;
	[Export] public float MaxUseTimeSec = float.PositiveInfinity;

	// -----------------------------------------------------------------------------------------------------------------
	// SIGNALS
	// -----------------------------------------------------------------------------------------------------------------

	[Signal] public delegate void AbilityUsedEventHandler(SuperCharacterController2DAbility ability);
	[Signal] public delegate void AbilityRechargedEventHandler(SuperCharacterController2DAbility ability);

	// -----------------------------------------------------------------------------------------------------------------
	// FIELDS
	// -----------------------------------------------------------------------------------------------------------------

	public int UseCount { get; private set; } = 0;
	public TimeSpan AccumulatedUseTime { get; private set; } = TimeSpan.Zero;

	// -----------------------------------------------------------------------------------------------------------------
	// PROPERTIES
	// -----------------------------------------------------------------------------------------------------------------

	public bool IsAvailable => !this.UseCountExceeded && !this.TimeLimitExceeded;
	public bool UseCountExceeded => this.MaxUseCount >= 0 && this.UseCount >= this.MaxUseCount;
	public bool TimeLimitExceeded => this.AccumulatedUseTime.TotalSeconds >= this.MaxUseTimeSec;

	// -----------------------------------------------------------------------------------------------------------------
	// METHODS
	// -----------------------------------------------------------------------------------------------------------------

	public void AddUseCount()
	{
		this.UseCount++;
		this.EmitSignal(SignalName.AbilityUsed, this);
	}

	public void AddUseTime(TimeSpan duration)
	{
		this.AccumulatedUseTime = this.AccumulatedUseTime.Add(duration);
		this.EmitSignal(SignalName.AbilityUsed, this);
	}

	public void Recharge()
	{
		this.UseCount = 0;
		this.AccumulatedUseTime = TimeSpan.Zero;
		this.EmitSignal(SignalName.AbilityRecharged, this);
	}
}
