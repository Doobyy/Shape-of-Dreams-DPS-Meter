using System.Collections.Generic;
using UnityEngine;

public class Gem_E_Insight : Gem
{
	public float explodeRadius;

	public ScalingValue maxHitCount;

	public GameObject activateEffect;

	public bool spawnOnBase;

	public float startDelay;

	public float interval;

	protected override void OnDealDamage(EventInfoDamage info)
	{
		base.OnDealDamage(info);
		if (!isValid || !IsReady())
		{
			return;
		}
		List<Entity> list = DewPhysics.OverlapCircleAllEntities(out var handle, info.victim.position, explodeRadius, tvDefaultHarmfulEffectTargets, new CollisionCheckSettings
		{
			sortComparer = CollisionCheckSettings.DistanceFromCenter
		});
		if (list.Count < 0)
		{
			handle.Return();
			return;
		}
		int num = Mathf.RoundToInt(GetValue(maxHitCount));
		float delay = startDelay;
		for (int i = 0; i < list.Count && i < num; i++)
		{
			Entity entity = list[i];
			CreateAbilityInstanceWithSource(info.actor, spawnOnBase ? entity.position : entity.Visual.GetCenterPosition(), Quaternion.identity, new CastInfo(owner, entity), (Ai_Gem_E_Insight_Damage p) =>
			{
				p.chain = info.chain.New(this);
				p._delay = delay;
			});
			delay += interval;
		}
		if (owner.Status.TryGetStatusEffect<Se_Gem_E_Insight_Buff>(out var effect))
		{
			effect.Destroy();
		}
		CreateStatusEffectWithSource<Se_Gem_E_Insight_Buff>(info.actor, owner, new CastInfo(owner));
		FxPlayNewNetworked(activateEffect, info.victim);
		NotifyUse();
		StartCooldown();
		handle.Return();
	}

	private void MirrorProcessed()
	{
	}
}
