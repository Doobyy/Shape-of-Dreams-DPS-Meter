using System;
using UnityEngine;

public class Gem_R_Blood : Gem
{
	public GameObject fxActivate;

	public float activateHealthThreshold = 0.4f;

	public int sacrificedQuality = 80;

	public float healMaxHpRatio = 0.4f;

	public override void OnEquipGem(Hero newOwner)
	{
		base.OnEquipGem(newOwner);
		newOwner.EntityEvent_OnTakeDamage += new Action<EventInfoDamage>(EntityEventOnTakeDamage);
	}

	private void EntityEventOnTakeDamage(EventInfoDamage obj)
	{
		if (isValid && !(owner.normalizedHealth > activateHealthThreshold) && IsReady())
		{
			int num = 10;
			if (quality > num)
			{
				int num2 = Mathf.Min(80, quality - num);
				float num3 = healMaxHpRatio * (float)num2 / (float)sacrificedQuality;
				quality -= num2;
				Heal(num3 * owner.maxHealth).Dispatch(owner);
				StartCooldown();
				NotifyUse();
				FxPlayNewNetworked(fxActivate, owner);
			}
		}
	}

	public override void OnUnequipGem(Hero oldOwner)
	{
		base.OnUnequipGem(oldOwner);
		if ((UnityEngine.Object)(object)oldOwner != null)
		{
			oldOwner.EntityEvent_OnTakeDamage -= new Action<EventInfoDamage>(EntityEventOnTakeDamage);
		}
	}

	public override bool IsReady()
	{
		if (base.IsReady())
		{
			return quality > 10;
		}
		return false;
	}

	private void MirrorProcessed()
	{
	}
}
