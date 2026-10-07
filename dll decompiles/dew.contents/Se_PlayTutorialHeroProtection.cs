using Mirror;

public class Se_PlayTutorialHeroProtection : StatusEffect
{
	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			DoDeathInterrupt((EventInfoKill k) =>
			{
				k.victim.Status.SetHealth(1f);
			}, 0);
			victim.takenDamageProcessor.Add(DamageTakenProcessor);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			victim.takenDamageProcessor.Remove(DamageTakenProcessor);
		}
	}

	private void DamageTakenProcessor(ref DamageData data, Actor actor, Entity target)
	{
		if (data.source != DamageData.SourceType.Pure && victim.normalizedHealth < 0.5f)
		{
			data.ApplyRawMultiplier(victim.normalizedHealth);
		}
	}

	private void MirrorProcessed()
	{
	}
}
