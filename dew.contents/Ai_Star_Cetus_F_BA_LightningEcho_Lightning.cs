using System;
using Mirror;
using UnityEngine;

public class Ai_Star_Cetus_F_BA_LightningEcho_Lightning : AbilityInstance, ACH_THEYRE_JUST_BIG_CATS.ILightingActor
{
	public GameObject hitEffect;

	[NonSerialized]
	public float dmgAmount;

	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			MagicDamage(dmgAmount).SetElemental(ElementalType.Light).Dispatch(info.target);
			FxPlayNewNetworked(hitEffect, info.target);
			Destroy();
		}
	}

	private void MirrorProcessed()
	{
	}
}
