using System;
using Mirror;
using UnityEngine;

public class Se_D_MercyOfEl : StackedStatusEffect
{
	public ScalingValue hastePerStack;

	public ScalingValue adBonus;

	public GameObject fxFullyCharged;

	public GameObject fxChargeChanged;

	public ScalingValue lostHpHealRatio;

	private HasteEffect _haste;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			victim.Visual.genericStackIndicatorMax = maxStack;
			victim.EntityEvent_OnAttackEffectTriggered += new Action<EventInfoAttackEffect>(EntityEventOnAttackEffectTriggered);
			DoStatBonus(new StatBonus
			{
				attackDamageFlat = GetValue(adBonus)
			});
			_haste = DoHaste(0f);
		}
	}

	private void EntityEventOnAttackEffectTriggered(EventInfoAttackEffect obj)
	{
		if (stack == 0 || stack == maxStack || UnityEngine.Random.value < obj.strength)
		{
			AddStack();
		}
		if (stack >= maxStack)
		{
			float value = GetValue(lostHpHealRatio);
			DoHeal(new HealData(victim.Status.missingHealth * value * obj.strength), victim);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			FxStopNetworked(fxFullyCharged);
			if ((UnityEngine.Object)(object)victim != null)
			{
				victim.Visual.genericStackIndicatorMax = 0;
				victim.Visual.genericStackIndicatorValue = 0;
				victim.EntityEvent_OnAttackEffectTriggered -= new Action<EventInfoAttackEffect>(EntityEventOnAttackEffectTriggered);
			}
		}
	}

	protected override void OnStackChange(int oldStack, int newStack)
	{
		base.OnStackChange(oldStack, newStack);
		if (((NetworkBehaviour)this).isServer && isActive)
		{
			FxPlayNetworked(fxChargeChanged, victim);
			if (stack == maxStack)
			{
				FxPlayNetworked(fxFullyCharged, victim);
			}
			else
			{
				FxStopNetworked(fxFullyCharged);
			}
			victim.Visual.genericStackIndicatorValue = stack;
			_haste.strength = GetValue(hastePerStack) * (float)stack;
		}
	}

	private void MirrorProcessed()
	{
	}
}
