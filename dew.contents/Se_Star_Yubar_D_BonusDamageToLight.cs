using System;
using Mirror;
using UnityEngine;

public class Se_Star_Yubar_D_BonusDamageToLight : StarEffect
{
	public StarScalingValue ampAmount;

	public override Type heroType => typeof(Hero_Yubar);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			victim.dealtDamageProcessor.Add(Processor);
		}
	}

	private void Processor(ref DamageData data, Actor actor, Entity target)
	{
		if (victim.CheckEnemyOrNeutral(target) && target.Status.lightStack >= 5 && !data.IsAmountModifiedBy(this))
		{
			data.ApplyAmplification(GetValue(ampAmount));
			data.SetAttr(DamageAttribute.IsCrit);
			data.SetAmountModifiedBy(this);
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

	private void MirrorProcessed()
	{
	}
}
