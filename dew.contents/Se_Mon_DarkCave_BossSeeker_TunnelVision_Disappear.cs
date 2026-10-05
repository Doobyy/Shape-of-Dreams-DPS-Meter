using Mirror;

public class Se_Mon_DarkCave_BossSeeker_TunnelVision_Disappear : StatusEffect
{
	protected override void OnCreate()
	{
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		DoUncollidable();
		DoUntargetable();
		DoInvulnerable();
		DoInvisible(ignoreReveal: true);
		victim.Visual.DisableRenderers();
		StatusEffect[] array = victim.Status.statusEffects.ToArray();
		foreach (StatusEffect statusEffect in array)
		{
			if (statusEffect is ElementalStatusEffect)
			{
				statusEffect.Destroy();
			}
		}
		victim.Control.freeMovement = true;
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && !victim.IsNullOrInactive())
		{
			victim.Visual.EnableRenderers();
			victim.Control.freeMovement = false;
		}
	}

	private void MirrorProcessed()
	{
	}
}
