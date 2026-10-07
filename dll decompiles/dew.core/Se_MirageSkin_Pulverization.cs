using System.Collections.Generic;
using UnityEngine;

public class Se_MirageSkin_Pulverization : MirageSkinEffect
{
	private class Ad_LastTimes
	{
		public float knockbackTime;

		public float breakShieldTime;
	}

	public GameObject fxBreakShield;

	public Knockback knockback;

	public float breakShieldStun = 1f;

	public override void OnSpecialAttack()
	{
		base.OnSpecialAttack();
		List<Entity> list = DewPhysics.OverlapCircleAllEntities(out var handle, victim.agentPosition, 6.5f, tvDefaultHarmfulEffectTargets);
		if (list.Count > 0)
		{
			Ai_MirageSkin_Oblivion_Orb byType = DewResources.GetByType<Ai_MirageSkin_Oblivion_Orb>(default(ResourceLoadSettings));
			float predictionStrength = NetworkedManagerBase<GameManager>.instance.GetPredictionStrength();
			Entity target = list[Random.Range(0, list.Count)];
			float y = AbilityTrigger.PredictAngle_SpeedAcceleration(victim, predictionStrength, target, victim.agentPosition, 0f, byType.startInFrontDistance, byType.initialSpeed, byType.targetSpeed, byType.acceleration);
			handle.Return();
			CreateAbilityInstance<Ai_MirageSkin_Pulverization_Stomp>(victim.agentPosition, Quaternion.Euler(0f, y, 0f), new CastInfo(victim));
		}
		else
		{
			handle.Return();
		}
	}

	public override void OnDealDamage(EventInfoDamage obj)
	{
		base.OnDealDamage(obj);
		if (obj.chain.DidReact(this) || obj.actor is Se_Elm_Fire || !(obj.victim is Hero))
		{
			return;
		}
		if (!obj.victim.TryGetData<Ad_LastTimes>(out var data))
		{
			data = new Ad_LastTimes();
			obj.victim.AddData(data);
		}
		if (!obj.victim.Control.isAirborne && Time.time - data.knockbackTime > 1.5f)
		{
			knockback.ApplyWithOrigin(victim.agentPosition, obj.victim);
			data.knockbackTime = Time.time;
		}
		if (obj.victim.Status.currentShield > 0f && Time.time - data.breakShieldTime > breakShieldStun + 0.25f)
		{
			float num;
			for (num = 999f; num < obj.victim.Status.currentShield * 10f; num = num * 10f + 9f)
			{
			}
			DefaultDamage(num).SetAttr(DamageAttribute.IsCrit).SetAttr(DamageAttribute.IgnoreArmor).SetAttr(DamageAttribute.DamageShieldOnly)
				.Dispatch(obj.victim, chain.New(this));
			CreateBasicEffect(obj.victim, new StunEffect(), breakShieldStun);
			FxPlayNewNetworked(fxBreakShield, obj.victim);
			data.breakShieldTime = Time.time;
		}
	}

	private void MirrorProcessed()
	{
	}
}
