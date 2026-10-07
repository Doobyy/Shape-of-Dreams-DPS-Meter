using System;
using UnityEngine;

public class Se_Star_Nachia_I_ForestStartDreamDust : EveryZoneStarEffect
{
	public StarScalingValue givenAmount;

	public override Type heroType => typeof(Hero_Nachia);

	public override void OnNewZoneReached()
	{
		if (NetworkedManagerBase<ZoneManager>.instance.currentZone.name == "Zone_Forest")
		{
			GiveDust();
		}
	}

	private void GiveDust()
	{
		NetworkedManagerBase<ZoneManager>.instance.CallOnReadyAfterTransition(() =>
		{
			if (!this.IsNullOrInactive() && !((UnityEngine.Object)(object)player == null))
			{
				player.EarnDreamDust(GetValueInt(givenAmount));
			}
		});
	}

	private void MirrorProcessed()
	{
	}
}
