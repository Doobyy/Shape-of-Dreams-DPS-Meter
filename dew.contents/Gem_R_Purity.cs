using UnityEngine;

public class Gem_R_Purity : Gem
{
	public ScalingValue healPerElemental;

	public GameObject fxHealEffect;

	protected override void OnCastComplete(EventInfoCast info)
	{
		base.OnCastComplete(info);
		if (!IsReady() || !info.trigger.configs[info.configIndex].canConsumeCastBonus)
		{
			return;
		}
		int num = 0;
		StatusEffect[] array = owner.Status.statusEffects.ToArray();
		foreach (StatusEffect statusEffect in array)
		{
			if (!statusEffect.IsNullOrInactive() && statusEffect is ElementalStatusEffect)
			{
				num++;
				statusEffect.Destroy();
			}
		}
		if (num != 0)
		{
			FxPlayNetworked(fxHealEffect, owner);
			info.instance.Heal(GetValue(healPerElemental) * (float)num).Dispatch(owner);
			NotifyUse();
			StartCooldown();
		}
	}

	private void MirrorProcessed()
	{
	}
}
