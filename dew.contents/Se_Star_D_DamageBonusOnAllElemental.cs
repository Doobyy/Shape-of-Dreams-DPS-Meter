using Mirror;
using UnityEngine;

public class Se_Star_D_DamageBonusOnAllElemental : StarEffect
{
	public StarScalingValue ampOnAllElemental;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			victim.dealtDamageProcessor.Add(Processor);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (Object)(object)victim != null)
		{
			victim.dealtDamageProcessor.Remove(Processor);
		}
	}

	private void Processor(ref DamageData data, Actor actor, Entity target)
	{
		if (!data.IsAmountModifiedBy(this) && victim.CheckEnemyOrNeutral(target) && target.Status.fireStack > 0 && target.Status.hasCold && target.Status.darkStack > 0 && target.Status.lightStack > 0)
		{
			data.SetAttr(DamageAttribute.IsCrit);
			data.SetAmountModifiedBy(this);
			data.ApplyAmplification(GetValue(ampOnAllElemental));
		}
	}

	private void MirrorProcessed()
	{
	}
}
