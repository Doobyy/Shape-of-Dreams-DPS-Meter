using System;
using System.Collections;
using Mirror;
using UnityEngine;

public class Ai_C_IceClaw : InstantDamageInstance
{
	public Dash dash;

	public Knockback knockback;

	public float knockbackStartStrength;

	public float knockbackEndStrength;

	public float knockbackEndDistanceThreshold;

	public int nextConfig = -1;

	public float changeTime;

	public float stunDuration = 1f;

	public float healRatio = 0.15f;

	public override bool reuseInRoom => true;

	protected override IEnumerator OnCreateSequenced()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			yield return base.OnCreateSequenced();
			yield break;
		}
		if (nextConfig > 0)
		{
			firstTrigger.ChangeConfigTimedOnce(nextConfig, changeTime);
		}
		dash.ApplyByDirection(info.caster, info.forward);
		ActorEvent_OnDealDamage += (Action<EventInfoDamage>)((EventInfoDamage damage) =>
		{
			if (!((UnityEngine.Object)(object)damage.actor != (UnityEngine.Object)(object)this))
			{
				Heal(damage.damage.amount * healRatio).SetAmountOrigin(damage.damage).SetCanMerge().Dispatch(info.caster, damage.chain.New(this));
			}
		});
		yield return base.OnCreateSequenced();
	}

	protected override void OnHit(Entity entity)
	{
		base.OnHit(entity);
		float distance = knockback.distance;
		knockback.distance *= Mathf.Lerp(knockbackStartStrength, knockbackEndStrength, Vector3.Distance(position, entity.position) / knockbackEndDistanceThreshold);
		knockback.ApplyWithDirection(info.forward, entity);
		knockback.distance = distance;
		CreateBasicEffect(entity, new StunEffect(), stunDuration, "IceClawStun", DuplicateEffectBehavior.UsePrevious);
	}

	private void MirrorProcessed()
	{
	}
}
