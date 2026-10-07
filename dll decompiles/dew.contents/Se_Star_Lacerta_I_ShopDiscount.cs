using System;
using Mirror;
using UnityEngine;

public class Se_Star_Lacerta_I_ShopDiscount : StarEffect
{
	public StarScalingValue discountRate;

	public override Type heroType => typeof(Hero_Lacerta);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			victim.owner.buyPriceMultiplier *= 1f - GetValue(discountRate);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)victim != null && (UnityEngine.Object)(object)victim.owner != null)
		{
			victim.owner.buyPriceMultiplier /= 1f - GetValue(discountRate);
		}
	}

	private void MirrorProcessed()
	{
	}
}
