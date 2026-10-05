using System;
using Mirror;
using UnityEngine;

public class Se_Star_Nachia_L_HealAndShieldAmp : StarEffect
{
	public StarScalingValue healShieldAmpSelf;

	public StarScalingValue healShieldAmpAlly;

	public override Type heroType => typeof(Hero_Nachia);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			hero.dealtHealProcessor.Add(HealAmp);
			hero.dealtShieldProcessor.Add(HealAmp);
		}
	}

	private void HealAmp(ref HealData data, Actor actor, Entity target)
	{
		if (!data.IsAmountModifiedBy(this) && !hero.CheckEnemyOrNeutral(target))
		{
			data.SetAmountModifiedBy(this);
			data.ApplyAmplification(GetValue(((UnityEngine.Object)(object)target == (UnityEngine.Object)(object)hero) ? healShieldAmpSelf : healShieldAmpAlly));
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)hero != null)
		{
			hero.dealtHealProcessor.Remove(HealAmp);
			hero.dealtShieldProcessor.Remove(HealAmp);
		}
	}

	private void MirrorProcessed()
	{
	}
}
