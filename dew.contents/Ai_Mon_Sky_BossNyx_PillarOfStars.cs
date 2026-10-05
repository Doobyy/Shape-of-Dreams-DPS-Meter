using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Ai_Mon_Sky_BossNyx_PillarOfStars : AbilityInstance
{
	public float initDelay;

	public GameObject fxTelegraph;

	public GameObject fxPillar;

	public GameObject fxHit;

	public DewCollider range;

	public int tickCount;

	public float tickInterval;

	public ScalingValue firstDamage;

	public ScalingValue afterFirstDamage;

	public override bool reuseInRoom => true;

	protected override IEnumerator OnCreateSequenced()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		DestroyOnDeath(info.caster);
		FxPlayNetworked(fxTelegraph);
		yield return new SI.WaitForSeconds(initDelay);
		FxStopNetworked(fxTelegraph);
		FxPlayNetworked(fxPillar);
		List<Entity> hitEnts = new List<Entity>();
		for (int i = 0; i < tickCount; i++)
		{
			List<Entity> entities = range.GetEntities(out var handle, tvDefaultHarmfulEffectTargets);
			for (int j = 0; j < entities.Count; j++)
			{
				Entity entity = entities[j];
				if (hitEnts.Contains(entity))
				{
					Damage(afterFirstDamage).SetOriginPosition(position).Dispatch(entity);
				}
				else
				{
					hitEnts.Add(entity);
					Damage(firstDamage).SetOriginPosition(position).Dispatch(entity);
				}
				FxPlayNewNetworked(fxHit, entity);
			}
			handle.Return();
			yield return new SI.WaitForSeconds(tickInterval);
		}
		Destroy();
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			FxStopNetworked(fxTelegraph);
			FxStopNetworked(fxPillar);
		}
	}

	private void MirrorProcessed()
	{
	}
}
