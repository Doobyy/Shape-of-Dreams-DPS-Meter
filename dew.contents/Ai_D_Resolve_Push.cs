using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Ai_D_Resolve_Push : AbilityInstance
{
	public DewCollider range;

	public Knockback knockback;

	public GameObject hitEffect;

	public float rotOverrideDuration;

	public float stunDuration;

	public ScalingValue maxHpRatio = ".04 .01x";

	public float minHealAmount;

	[NonSerialized]
	public MeleeAttackInstance attack;

	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		if (rotOverrideDuration > 0f)
		{
			info.caster.Control.Rotate(info.angle, immediately: true, rotOverrideDuration);
		}
		List<Entity> entities = range.GetEntities(out var handle, tvDefaultHarmfulEffectTargets);
		if (entities.Count > 0 || ((UnityEngine.Object)(object)attack != null && attack.didHit))
		{
			DoHeal(new HealData(Mathf.Max(minHealAmount, GetValue(maxHpRatio) * info.caster.maxHealth)), info.caster);
		}
		foreach (Entity item in entities)
		{
			knockback.ApplyWithDirection(info.rotation, item);
			if (stunDuration > 0f)
			{
				CreateBasicEffect(item, new StunEffect(), stunDuration, "determination_stun");
			}
			FxPlayNewNetworked(hitEffect, item);
		}
		handle.Return();
		Destroy();
	}

	private void MirrorProcessed()
	{
	}
}
