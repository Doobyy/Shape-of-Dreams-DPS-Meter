using Mirror;
using UnityEngine;

public class Se_DistantMemories : StatusEffect
{
	public float dmgAmpAmount;

	public float interval;

	public float movementSpeedReduction;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			DoSlow(movementSpeedReduction);
			DoUntargetable();
			DoProtected(null);
			DoUncollidable();
			victim.takenDamageProcessor.Add(AmplifyDamage);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (Object)(object)victim != null)
		{
			victim.takenDamageProcessor.Remove(AmplifyDamage);
		}
	}

	private void AmplifyDamage(ref DamageData data, Actor actor, Entity target)
	{
		if (!data.IsAmountModifiedBy(this) && !actor.firstEntity.IsNullInactiveDeadOrKnockedOut())
		{
			data.ApplyAmplification(dmgAmpAmount);
			data.SetAmountModifiedBy(this);
		}
	}

	private void MirrorProcessed()
	{
	}
}
