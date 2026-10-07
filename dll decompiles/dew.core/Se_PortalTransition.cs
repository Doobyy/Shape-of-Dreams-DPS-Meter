using Mirror;
using UnityEngine;

public class Se_PortalTransition : StatusEffect, IOtherPlayersTonedDownDisable
{
	public GameObject disappearEffect;

	public bool playDisappearEffect;

	public float healMissingHealthRatio;

	public float healMaxHealthRatio;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			if (playDisappearEffect)
			{
				FxPlayNetworked(disappearEffect, victim);
			}
			DoProtected(null);
			DoUntargetable();
			DoUncollidable();
			DoInvisible(ignoreReveal: true);
			DoRoot();
			DoSilence();
			victim.Control.Stop();
			victim.Visual.DisableRenderers();
			victim.Control.CancelOngoingChannels();
			victim.Status.DisableSectionTriggering();
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (Object)(object)victim != null)
		{
			if (victim.isAlive)
			{
				victim.Status.SetHealth(victim.currentHealth + victim.maxHealth * healMaxHealthRatio + victim.Status.missingHealth * healMissingHealthRatio);
			}
			victim.Visual.EnableRenderers();
			victim.Status.EnableSectionTriggering();
		}
	}

	private void MirrorProcessed()
	{
	}
}
