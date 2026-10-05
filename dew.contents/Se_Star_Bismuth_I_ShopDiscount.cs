using System;
using Mirror;
using UnityEngine;

public class Se_Star_Bismuth_I_ShopDiscount : StarEffect
{
	public StarScalingValue discountAmount;

	public override Type heroType => typeof(Hero_Bismuth);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			player.buyPriceMultiplier *= 1f - GetValue(discountAmount);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)player != null)
		{
			player.buyPriceMultiplier /= 1f - GetValue(discountAmount);
		}
	}

	private void MirrorProcessed()
	{
	}
}
