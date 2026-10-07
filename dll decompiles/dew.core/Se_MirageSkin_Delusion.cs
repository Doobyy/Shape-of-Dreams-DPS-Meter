using System.Collections.Generic;
using UnityEngine;

public class Se_MirageSkin_Delusion : MirageSkinEffect
{
	public GameObject fxTelegraph;

	public float missileRange = 10f;

	public float missileLandTime = 0.7f;

	public float randomMag = 3.5f;

	public float targetedChance = 0.25f;

	public Vector2 missilePredictionStrength;

	public override void OnSpecialAttack()
	{
		base.OnSpecialAttack();
		List<Entity> list = DewPhysics.OverlapCircleAllEntities(out var handle, victim.agentPosition, missileRange, tvDefaultHarmfulEffectTargets);
		Vector3 end;
		if (list.Count > 0 && Random.value < targetedChance)
		{
			end = AbilityTrigger.PredictPoint_Simple(victim, Random.Range(missilePredictionStrength.x, missilePredictionStrength.y), list[Random.Range(0, list.Count)], missileLandTime);
			end += Random.insideUnitSphere.Flattened() * randomMag;
			end = Dew.GetValidAgentDestination_Closest(victim.agentPosition, end);
		}
		else
		{
			end = Dew.GetValidAgentDestination_LinearSweep(victim.agentPosition, victim.agentPosition + Random.onUnitSphere.normalized * missileRange);
		}
		handle.Return();
		float num = 4f;
		if (Vector2.Distance(victim.agentPosition.ToXY(), end.ToXY()) < num)
		{
			end = victim.agentPosition + (end - victim.agentPosition).Flattened().normalized * num;
			end = Dew.GetPositionOnGround(end);
		}
		float finalFlatDist = Vector2.Distance(victim.agentPosition.ToXY(), end.ToXY());
		if (!(finalFlatDist < 1f))
		{
			FxPlayNewNetworked(fxTelegraph, end, null);
			CreateAbilityInstance(victim.agentPosition, null, new CastInfo(victim, end), (Ai_MirageSkin_Delusion_Missile ai) =>
			{
				ai.initialSpeed = finalFlatDist / missileLandTime;
			});
		}
	}

	public override void OnDealDamage(EventInfoDamage obj)
	{
		base.OnDealDamage(obj);
		if (!(obj.actor is Se_Elm_Fire) && obj.victim is Hero hero)
		{
			int num = (hero.IsMeleeHero() ? 30 : 90);
			if (obj.victim.Status.TryGetStatusEffect<Se_MirageSkin_Delusion_Delusional>(out var effect))
			{
				effect.AddStack(num);
				effect.ResetTimer();
				effect.FxPlayNetworked(effect.fxStackUpdated, obj.victim);
			}
			else
			{
				victim.CreateStatusEffect<Se_MirageSkin_Delusion_Delusional>(obj.victim, new CastInfo(obj.victim)).SetStack(num);
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
