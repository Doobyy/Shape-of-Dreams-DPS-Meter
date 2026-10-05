using System.Collections.Generic;
using UnityEngine;

public class Se_MirageSkin_Oblivion : MirageSkinEffect
{
	public float silenceDuration = 3f;

	public float searchRange = 10f;

	public Vector2 predictionStrength;

	public override void OnSpecialAttack()
	{
		base.OnSpecialAttack();
		List<Entity> list = DewPhysics.OverlapCircleAllEntities(out var handle, victim.agentPosition, searchRange, tvDefaultHarmfulEffectTargets);
		float angle;
		if (list.Count > 0)
		{
			Ai_MirageSkin_Oblivion_Orb byType = DewResources.GetByType<Ai_MirageSkin_Oblivion_Orb>(default(ResourceLoadSettings));
			float strength = Random.Range(predictionStrength.x, predictionStrength.y);
			Entity target = list[Random.Range(0, list.Count)];
			angle = AbilityTrigger.PredictAngle_SpeedAcceleration(victim, strength, target, victim.agentPosition, 0f, byType.startInFrontDistance, byType.initialSpeed, byType.targetSpeed, byType.acceleration);
		}
		else
		{
			angle = Random.Range(0f, 360f);
		}
		handle.Return();
		CreateAbilityInstance<Ai_MirageSkin_Oblivion_Orb>(victim.agentPosition, null, new CastInfo(victim, angle));
	}

	public override void OnDealDamage(EventInfoDamage obj)
	{
		base.OnDealDamage(obj);
		if (obj.actor is Se_Elm_Fire || !(obj.victim is Hero))
		{
			return;
		}
		if (obj.victim.Status.TryGetStatusEffect<Se_MirageSkin_Oblivion_Silenced>(out var effect))
		{
			effect.ResetTimer();
			return;
		}
		obj.victim.CreateStatusEffect(obj.victim, new CastInfo(obj.victim), (Se_MirageSkin_Oblivion_Silenced se) =>
		{
			se.duration = silenceDuration;
		});
	}

	private void MirrorProcessed()
	{
	}
}
