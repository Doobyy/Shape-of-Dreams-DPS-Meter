using System;
using Mirror;
using UnityEngine;

public class Se_Star_Husk_D_DamageAmpOnLowHpTarget : StarEffect
{
	public float healthThreshold = 0.35f;

	public StarScalingValue damageAmp;

	public override Type heroType => typeof(Hero_Husk);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			hero.dealtDamageProcessor.Add(Processor);
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
		if (!(target.normalizedHealth > healthThreshold) && hero.CheckEnemyOrNeutral(target) && !data.IsAmountModifiedBy(this))
		{
			data.SetAmountModifiedBy(this);
			data.ApplyAmplification(GetValue(damageAmp));
			data.SetAttr(DamageAttribute.IsCrit);
		}
	}

	private void MirrorProcessed()
	{
	}
}
