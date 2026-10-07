using Mirror;

public class Ai_E_MassCleanse : InstantDamageInstance
{
	public ScalingValue healPerElemental;

	public float unstoppableDuration = 1f;

	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		position = info.point;
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		ListReturnHandle<Entity> handle;
		foreach (Entity entity in range.GetEntities(out handle, tvDefaultUsefulEffectTargets))
		{
			FxPlayNewNetworked(hitEffect, entity);
			CreateBasicEffect(entity, new UnstoppableEffect(), unstoppableDuration);
			int num = 0;
			StatusEffect[] array = entity.Status.statusEffects.ToArray();
			foreach (StatusEffect statusEffect in array)
			{
				if (!statusEffect.IsNullOrInactive())
				{
					if (statusEffect is CurseStatusEffect)
					{
						statusEffect.Destroy();
					}
					if (statusEffect.isCleansable)
					{
						statusEffect.Destroy();
					}
					else if (statusEffect is Se_GenericEffectContainer se_GenericEffectContainer && ((BasicEffectMask.Slow | BasicEffectMask.Cripple | BasicEffectMask.Root | BasicEffectMask.Silence | BasicEffectMask.Stun | BasicEffectMask.Blind | BasicEffectMask.ArmorReduction | BasicEffectMask.HealReduction) & se_GenericEffectContainer.effect.mask) != 0)
					{
						statusEffect.Destroy();
					}
					else if (statusEffect is Se_MirageSkin_Delusion_Delusional)
					{
						statusEffect.Destroy();
					}
					else if (statusEffect is ElementalStatusEffect)
					{
						num++;
						statusEffect.Destroy();
					}
				}
			}
			Heal(GetValue(healPerElemental) * (float)num).Dispatch(entity);
		}
		handle.Return();
		DestroyIfActive();
	}

	private void MirrorProcessed()
	{
	}
}
