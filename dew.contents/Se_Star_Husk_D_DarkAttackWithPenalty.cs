using System;
using Mirror;
using UnityEngine;

public class Se_Star_Husk_D_DarkAttackWithPenalty : StarEffect
{
	public float healthPenaltyPercentage = 15f;

	public StarScalingValue atkSpdBonus;

	public override Type heroType => typeof(Hero_Husk);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			DoStatBonus(new StatBonus
			{
				attackSpeedPercentage = GetValue(atkSpdBonus),
				maxHealthPercentage = 0f - healthPenaltyPercentage
			});
			hero.dealtDamageProcessor.Add(Processor, -2000);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)hero != null)
		{
			hero.dealtDamageProcessor.Remove(Processor);
		}
	}

	private void Processor(ref DamageData data, Actor actor, Entity target)
	{
		AttackEffectType attackEffectType = data.attackEffectType;
		if (attackEffectType == AttackEffectType.BasicAttackMain || attackEffectType == AttackEffectType.BasicAttackSub)
		{
			data.SetElemental(ElementalType.Dark);
		}
	}

	private void MirrorProcessed()
	{
	}
}
