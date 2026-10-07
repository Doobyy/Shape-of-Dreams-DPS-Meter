using Mirror;
using UnityEngine;

public class Se_Star_D_BossDamageAmp : StarEffect
{
	public StarScalingValue damageToBossAmp;

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
		if (!data.IsAmountModifiedBy(this) && victim.CheckEnemyOrNeutral(target) && target.IsAnyBoss())
		{
			data.SetAmountModifiedBy(this);
			data.ApplyAmplification(GetValue(damageToBossAmp));
		}
	}

	private void MirrorProcessed()
	{
	}
}
