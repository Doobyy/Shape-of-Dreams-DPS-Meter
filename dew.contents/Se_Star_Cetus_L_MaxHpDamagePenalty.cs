using System;
using Mirror;
using UnityEngine;

public class Se_Star_Cetus_L_MaxHpDamagePenalty : StarEffect
{
	public float damagePenaltyReduction = 0.05f;

	public StarScalingValue healthAmount;

	public override Type heroType => typeof(Hero_Cetus);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			hero.dealtDamageProcessor.Add(Process);
			DoStatBonus(new StatBonus
			{
				maxHealthFlat = GetValue(healthAmount)
			});
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (bool)(UnityEngine.Object)(object)hero)
		{
			hero.dealtDamageProcessor.Remove(Process);
		}
	}

	private void Process(ref DamageData data, Actor from, Entity to)
	{
		if (!data.IsAmountModifiedBy(this))
		{
			data.ApplyReduction(damagePenaltyReduction);
			data.SetAmountModifiedBy(this);
		}
	}

	private void MirrorProcessed()
	{
	}
}
