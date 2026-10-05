using System;
using System.Collections;
using Mirror;
using UnityEngine;

public class Se_HealthCost : StatusEffect, IOtherPlayersTonedDownDisable
{
	[NonSerialized]
	public float totalAmount;

	public GameObject goldEffect;

	public GameObject blueEffect;

	public int ticks;

	public float tickInterval;

	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		FxPlay(victim.Visual.model.hasGoldDissolve ? goldEffect : blueEffect, victim);
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		FxStop(victim.Visual.model.hasGoldDissolve ? goldEffect : blueEffect);
	}

	protected override IEnumerator OnCreateSequenced()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		for (int i = 0; i < ticks; i++)
		{
			if (victim.IsNullInactiveDeadOrKnockedOut())
			{
				Destroy();
				yield break;
			}
			float value = totalAmount / (float)ticks;
			value = Mathf.Clamp(value, 0f, victim.currentHealth - 1f);
			PureDamage(value).SetAttr(DamageAttribute.IgnoreShield).SetAttr(DamageAttribute.IgnoreDamageImmunity).SetAttr(DamageAttribute.IgnoreArmor)
				.SetAttr(DamageAttribute.DamageOverTime)
				.Dispatch(victim);
			yield return new SI.WaitForSeconds(tickInterval);
		}
		Destroy();
	}

	private void MirrorProcessed()
	{
	}
}
