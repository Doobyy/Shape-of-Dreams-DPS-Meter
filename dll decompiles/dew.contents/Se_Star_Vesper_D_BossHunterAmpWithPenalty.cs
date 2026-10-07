using System;
using Mirror;
using UnityEngine;

public class Se_Star_Vesper_D_BossHunterAmpWithPenalty : StarEffect
{
	public StarScalingValue eliteDamageAmp;

	public float healthPenaltyPercentage;

	public override Type heroType => typeof(Hero_Vesper);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			DoStatBonus(new StatBonus
			{
				maxHealthPercentage = 0f - healthPenaltyPercentage
			});
			victim.dealtDamageProcessor.Add(Processor, 1000);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)victim != null)
		{
			victim.dealtDamageProcessor.Remove(Processor);
		}
	}

	private void Processor(ref DamageData data, Actor actor, Entity target)
	{
		if ((data.elemental == ElementalType.Fire || data.elemental == ElementalType.Light) && !data.IsAmountModifiedBy(this) && (target.IsAnyBoss() || target is Monster { isHunter: not false }))
		{
			data.ApplyAmplification(GetValue(eliteDamageAmp));
			data.SetAmountModifiedBy(this);
			data.SetAttr(DamageAttribute.IsCrit);
		}
	}

	private void MirrorProcessed()
	{
	}
}
