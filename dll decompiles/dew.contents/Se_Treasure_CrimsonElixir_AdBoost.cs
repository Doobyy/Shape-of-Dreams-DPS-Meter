using System;
using Mirror;
using UnityEngine;

public class Se_Treasure_CrimsonElixir_AdBoost : TempEffect
{
	public float bonusRatio = 0.2f;

	public float healRatio = 0.02f;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			DoStatBonus(new StatBonus
			{
				attackDamagePercentage = bonusRatio * 100f
			});
			victim.EntityEvent_OnAttackEffectTriggered += new Action<EventInfoAttackEffect>(EntityEventOnAttackEffectTriggered);
		}
	}

	private void EntityEventOnAttackEffectTriggered(EventInfoAttackEffect obj)
	{
		Heal(healRatio * victim.maxHealth * obj.strength).Dispatch(victim);
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)victim != null)
		{
			victim.EntityEvent_OnAttackEffectTriggered -= new Action<EventInfoAttackEffect>(EntityEventOnAttackEffectTriggered);
		}
	}

	private void MirrorProcessed()
	{
	}
}
