using Mirror;
using UnityEngine;

public class Se_Star_L_DamageReductionFromChilled : StarEffect
{
	public StarScalingValue reductionAmount;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			victim.takenDamageProcessor.Add(Processor);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (Object)(object)victim != null)
		{
			victim.takenDamageProcessor.Remove(Processor);
		}
	}

	private void Processor(ref DamageData data, Actor actor, Entity target)
	{
		Entity entity = actor.firstEntity;
		if (!((Object)(object)entity == null) && entity is Monster && victim.CheckEnemyOrNeutral(entity) && entity.Status.hasCold && !data.IsAmountModifiedBy(this))
		{
			data.SetAmountModifiedBy(this);
			data.ApplyReduction(GetValue(reductionAmount));
		}
	}

	private void MirrorProcessed()
	{
	}
}
