using System;
using Mirror;
using UnityEngine;

public class Se_Star_Mist_I_StartGoldWithPenalty : StarEffect
{
	public StarScalingValue addedGoldAmount;

	public StarScalingValue buyPricePenalty;

	public override Type heroType => typeof(Hero_Mist);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			player.EarnGold(GetValueInt(addedGoldAmount));
			player.buyPriceMultiplier *= 1f + GetValue(buyPricePenalty);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)player != null)
		{
			player.buyPriceMultiplier /= 1f + GetValue(buyPricePenalty);
		}
	}

	private void MirrorProcessed()
	{
	}
}
