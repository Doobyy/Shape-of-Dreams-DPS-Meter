using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gem_L_PureWhite : Gem
{
	public GameObject castEffect;

	public float initDelay;

	public float interval;

	public float maxShootTime;

	public int minProjectiles;

	protected override void OnCastCompleteBeforePrepare(EventInfoCast info)
	{
		base.OnCastCompleteBeforePrepare(info);
		if (!IsReady() || !info.trigger.configs[info.configIndex].canConsumeCastBonus)
		{
			return;
		}
		List<Entity> affected = new List<Entity>();
		info.instance.ActorEvent_OnDealDamage += (Action<EventInfoDamage>)((EventInfoDamage obj) =>
		{
			if (isValid && !obj.chain.DidReact(this) && owner.CheckEnemyOrNeutral(obj.victim))
			{
				((MonoBehaviour)(object)this).StartCoroutine(Routine());
			}
			IEnumerator Routine()
			{
				obj.actor.LockDestroyFor(initDelay + 0.1f);
				yield return new WaitForSeconds(initDelay);
				if (isValid && minProjectiles + obj.victim.Status.lightStack > 0 && !affected.Contains(obj.victim))
				{
					affected.Add(obj.victim);
					int count = minProjectiles + obj.victim.Status.lightStack;
					float adjustedInterval = Mathf.Min(interval, maxShootTime / (float)count);
					obj.actor.LockDestroyFor((float)count * adjustedInterval + 0.1f);
					for (int i = 0; i < count; i++)
					{
						if (!isValid)
						{
							break;
						}
						if (obj.victim.IsNullInactiveDeadOrKnockedOut())
						{
							break;
						}
						CreateAbilityInstanceWithSource(obj.actor, owner.position, Quaternion.identity, new CastInfo(owner, obj.victim), (Ai_Gem_L_PureWhite_Projectile p) =>
						{
							p.damageData = obj.damage;
							p.chain = obj.chain.New(this);
						});
						yield return new WaitForSeconds(adjustedInterval);
					}
				}
			}
		});
		NotifyUse();
		StartCooldown();
		FxPlayNewNetworked(castEffect, owner);
	}

	private void MirrorProcessed()
	{
	}
}
