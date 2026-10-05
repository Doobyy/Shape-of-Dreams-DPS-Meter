using System;
using Mirror;
using UnityEngine;

public class Se_Star_Vesper_I_ShopDiscount : StarEffect
{
	public StarScalingValue discountRatio;

	public override Type heroType => typeof(Hero_Vesper);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			player.buyPriceMultiplier *= 1f - GetValue(discountRatio);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)player != null)
		{
			player.buyPriceMultiplier /= 1f - GetValue(discountRatio);
		}
	}

	private void MirrorProcessed()
	{
	}
}
