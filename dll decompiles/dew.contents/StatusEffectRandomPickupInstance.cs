using UnityEngine;

public class StatusEffectRandomPickupInstance : RandomPickupInstance
{
	public bool destroyExisting = true;

	public StatusEffect statusEffect;

	protected override void OnPickup(Hero hero)
	{
		base.OnPickup(hero);
		if (destroyExisting)
		{
			StatusEffect statusEffect = hero.Status.FindStatusEffect((StatusEffect se) => ((object)se).GetType() == ((object)this.statusEffect).GetType());
			if ((Object)(object)statusEffect != null)
			{
				statusEffect.Destroy();
			}
		}
		CreateStatusEffect(this.statusEffect, hero, default, null);
	}

	private void MirrorProcessed()
	{
	}
}
