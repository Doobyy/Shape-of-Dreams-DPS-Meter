using System;
using Mirror;
using UnityEngine;

public class Se_Star_Bismuth_F_IT_SmallerRingMoreDamage : StarEffect
{
	public float radiusReduction = 0.25f;

	public float damageAmp = 0.35f;

	public override Type heroType => typeof(Hero_Bismuth);

	public override Type skillType => typeof(St_QR_InfernalTales);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			hero.ActorEvent_OnAbilityInstanceBeforePrepare += new Action<EventInfoAbilityInstance>(ActorEventOnAbilityInstanceBeforePrepare);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)hero != null)
		{
			hero.ActorEvent_OnAbilityInstanceBeforePrepare -= new Action<EventInfoAbilityInstance>(ActorEventOnAbilityInstanceBeforePrepare);
		}
	}

	private void ActorEventOnAbilityInstanceBeforePrepare(EventInfoAbilityInstance obj)
	{
		if (obj.instance is Ai_QR_InfernalTales ai_QR_InfernalTales)
		{
			ai_QR_InfernalTales.dmgFactor *= 1f + damageAmp;
			ai_QR_InfernalTales.NetworksizeMultiplier = ai_QR_InfernalTales.sizeMultiplier * (1f - radiusReduction);
		}
	}

	private void MirrorProcessed()
	{
	}
}
