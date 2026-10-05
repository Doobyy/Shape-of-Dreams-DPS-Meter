using Mirror;
using UnityEngine;

public class Se_Curse_FragmentedBeing : CurseStatusEffect
{
	public float[] ampAmounts;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			victim.takenDamageProcessor.Add(AmplifyDamage);
			if (victim.Status.TryGetStatusEffect<Se_HeroOneShotProtection>(out var effect))
			{
				effect.DisableProtection();
			}
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (Object)(object)victim != null)
		{
			victim.takenDamageProcessor.Remove(AmplifyDamage);
			if (victim.Status.TryGetStatusEffect<Se_HeroOneShotProtection>(out var effect))
			{
				effect.ReenableProtection();
			}
		}
	}

	private void AmplifyDamage(ref DamageData data, Actor actor, Entity target)
	{
		if (!data.IsAmountModifiedBy(this))
		{
			Entity entity = actor.firstEntity;
			if (!((Object)(object)entity == null) && entity.GetRelation(target) == EntityRelation.Enemy)
			{
				data.ApplyAmplification(GetValue(ampAmounts));
				data.SetAmountModifiedBy(this);
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
