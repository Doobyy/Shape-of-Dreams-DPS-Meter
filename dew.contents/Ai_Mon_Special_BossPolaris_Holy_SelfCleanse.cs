using Mirror;

public class Ai_Mon_Special_BossPolaris_Holy_SelfCleanse : AbilityInstance
{
	protected override void OnCreate()
	{
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		StatusEffect[] array = info.caster.Status.statusEffects.ToArray();
		foreach (StatusEffect statusEffect in array)
		{
			if (statusEffect is ElementalStatusEffect)
			{
				statusEffect.Destroy();
			}
		}
		DefaultDamage(info.caster.currentHealth * 0.05f).Dispatch(info.caster);
		Destroy();
	}

	private void MirrorProcessed()
	{
	}
}
