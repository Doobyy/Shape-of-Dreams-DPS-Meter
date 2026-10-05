using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Ai_Mon_Despair_WretchedArtillery_BarrageAtk_AoE : AbilityInstance
{
	public float radius;

	public int tickCount;

	public float tickInterval;

	public ScalingValue tickDamage;

	public float hitInterval;

	public bool doSlow;

	public float slowAmount;

	public float slowDuration;

	public bool isSlowDecay;

	public GameObject hitEffect;

	public GameObject sizzleSound;

	[NonSerialized]
	public bool playSizzleSound;

	public override bool reuseInRoom => true;

	protected override IEnumerator OnCreateSequenced()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		if (playSizzleSound)
		{
			FxPlayNetworked(sizzleSound);
		}
		Dictionary<Entity, float> lastHitTimes = new Dictionary<Entity, float>();
		for (int i = 0; i < tickCount; i++)
		{
			List<Entity> list = DewPhysics.OverlapCircleAllEntities(out var handle, position, radius, tvDefaultHarmfulEffectTargets);
			for (int j = 0; j < list.Count; j++)
			{
				Entity entity = list[j];
				if (!lastHitTimes.ContainsKey(entity) || !(Time.time - lastHitTimes[entity] < hitInterval))
				{
					lastHitTimes[entity] = Time.time;
					Damage(tickDamage).Dispatch(entity);
					FxPlayNewNetworked(hitEffect, entity);
					if (doSlow)
					{
						CreateBasicEffect(entity, new SlowEffect
						{
							decay = isSlowDecay,
							strength = slowAmount
						}, slowDuration, "artillery_slow", DuplicateEffectBehavior.UsePrevious);
					}
				}
			}
			handle.Return();
			yield return new SI.WaitForSeconds(tickInterval);
		}
		Destroy();
	}

	private void MirrorProcessed()
	{
	}
}
