using System;
using UnityEngine;

public class Se_Star_Aurena_I_GoldEveryZone : EveryZoneStarEffect
{
	public StarScalingValue goldAmount;

	public override Type heroType => typeof(Hero_Aurena);

	public override void OnNewZoneReached()
	{
		if (!this.IsNullOrInactive() && !((UnityEngine.Object)(object)player == null))
		{
			player.EarnGold(GetValueInt(goldAmount));
		}
	}

	private void MirrorProcessed()
	{
	}
}
