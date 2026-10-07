using Mirror;
using UnityEngine;

public class Se_PolarisHallucination : StatusEffect
{
	protected override void OnCreate()
	{
		base.OnCreate();
		victim.Visual.model.fxDeath = null;
		victim.Visual.model.deathBehavior = EntityVisual.EntityDeathBehavior.HideModel;
		if (((NetworkBehaviour)this).isServer)
		{
			DoUnstoppable();
			DoDeathInterrupt((EventInfoKill k) =>
			{
				Destroy();
				victim.Status.SetHealth(1f);
				victim.Destroy();
			}, -1000);
			victim.dealtDamageProcessor.Add(VictimOndealtDamageProcessor);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (Object)(object)victim != null)
		{
			victim.dealtDamageProcessor.Remove(VictimOndealtDamageProcessor);
		}
	}

	private void VictimOndealtDamageProcessor(ref DamageData data, Actor actor, Entity target)
	{
		data.ApplyReduction(0.2f);
	}

	private void MirrorProcessed()
	{
	}
}
