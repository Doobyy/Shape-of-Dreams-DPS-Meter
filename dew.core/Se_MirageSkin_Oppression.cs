using UnityEngine;

public class Se_MirageSkin_Oppression : MirageSkinEffect
{
	private class Ad_LastStunTime
	{
		public float time;
	}

	public GameObject fxStun;

	public float stunDuration = 0.5f;

	public float stunCooldownTime = 2f;

	public override void OnSpecialAttack()
	{
		base.OnSpecialAttack();
		CreateAbilityInstance<Ai_MirageSkin_Oppression_Explode>(victim.agentPosition, null, new CastInfo(victim));
	}

	public override void OnDealDamage(EventInfoDamage obj)
	{
		base.OnDealDamage(obj);
		if (!(obj.actor is Se_Elm_Fire) && obj.victim is Hero)
		{
			if (!victim.TryGetData<Ad_LastStunTime>(out var data))
			{
				data = new Ad_LastStunTime();
				victim.AddData(data);
			}
			if (Time.time - data.time > stunCooldownTime)
			{
				data.time = Time.time;
				CreateBasicEffect(obj.victim, new StunEffect(), stunDuration);
				FxPlayNewNetworked(fxStun, obj.victim);
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
