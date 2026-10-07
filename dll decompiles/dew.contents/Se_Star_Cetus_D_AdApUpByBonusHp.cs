using System;
using Mirror;
using UnityEngine;

public class Se_Star_Cetus_D_AdApUpByBonusHp : StarEffect
{
	public StarScalingValue ampValue;

	public override Type heroType => typeof(Hero_Cetus);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			hero.Status.finalStatsProcessors.Add(Processor, 101);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (bool)(UnityEngine.Object)(object)hero)
		{
			hero.Status.finalStatsProcessors.Remove(Processor);
		}
	}

	private void Processor(ref FinalStats data)
	{
		float num = Mathf.Max(0f, data.maxHealth - data.maxHealthWithoutBonus) * GetValue(ampValue);
		data.abilityPower += num * (1f + hero.Status.bonusStats.abilityPowerPercentage * 0.01f);
		data.attackDamage += num * (1f + hero.Status.bonusStats.attackDamagePercentage * 0.01f);
	}

	private void MirrorProcessed()
	{
	}
}
