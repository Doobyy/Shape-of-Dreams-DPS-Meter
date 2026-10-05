using Mirror;
using UnityEngine;

public class Ai_Gem_E_Predation_Pickup : PickupInstance
{
	public ScalingValue addedAp;

	public ScalingValue addedAd;

	public ScalingValue addedHealth;

	internal Hero _targetHero;

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (((NetworkBehaviour)this).isServer && _targetHero.IsNullInactiveDeadOrKnockedOut())
		{
			Destroy();
		}
	}

	protected override bool CanBeUsedBy(Hero hero)
	{
		if (base.CanBeUsedBy(hero))
		{
			return (Object)(object)_targetHero == (Object)(object)hero;
		}
		return false;
	}

	protected override void OnPickup(Hero hero)
	{
		base.OnPickup(hero);
		if (!((Object)(object)hero != (Object)(object)_targetHero))
		{
			if (!hero.Status.TryGetStatusEffect<Se_Gem_E_Predation_StatBonus>(out var effect))
			{
				effect = hero.CreateStatusEffect<Se_Gem_E_Predation_StatBonus>(hero, new CastInfo(hero));
			}
			switch (Random.Range(0, 3))
			{
			case 0:
				effect.bonus.abilityPowerFlat += GetValue(addedAp);
				break;
			case 1:
				effect.bonus.attackDamageFlat += GetValue(addedAd);
				break;
			default:
				effect.bonus.maxHealthFlat += GetValue(addedHealth);
				break;
			}
			if ((Object)(object)gem != null && gem is Gem_E_Predation gem_E_Predation)
			{
				gem_E_Predation.UpdateStack();
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
