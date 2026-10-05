using System;
using Mirror;
using UnityEngine;

public class Se_Star_Lacerta_I_ShopMoreItemsWithPenalty : StarEffect
{
	public StarScalingValue pricePenaltyRatio;

	public override Type heroType => typeof(Hero_Lacerta);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			victim.owner.buyPriceMultiplier *= 1f + GetValue(pricePenaltyRatio);
			player.shopAddedItems++;
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			if ((UnityEngine.Object)(object)victim != null && (UnityEngine.Object)(object)victim.owner != null)
			{
				victim.owner.buyPriceMultiplier /= 1f + GetValue(pricePenaltyRatio);
			}
			if ((UnityEngine.Object)(object)player != null)
			{
				player.shopAddedItems--;
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
