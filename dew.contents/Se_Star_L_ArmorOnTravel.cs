using Mirror;

public class Se_Star_L_ArmorOnTravel : EveryZoneStarEffect
{
	public StarScalingValue armorAmount;

	[SaveVar(SaveVarFlags.Default)]
	private StatBonus _bonus = new StatBonus();

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			_bonus = DoStatBonus(_bonus);
		}
	}

	public override void OnNewZoneReached()
	{
		_bonus.armorFlat += GetValue(armorAmount);
	}

	private void MirrorProcessed()
	{
	}
}
