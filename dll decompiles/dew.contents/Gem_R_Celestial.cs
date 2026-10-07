using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gem_R_Celestial : Gem
{
	public int stars = 3;

	public float firstDelay = 0.15f;

	public float interval = 0.75f;

	public GameObject startEffect;

	protected override void OnCastCompleteBeforePrepare(EventInfoCast info)
	{
		base.OnCastCompleteBeforePrepare(info);
		if (!info.trigger.configs[info.configIndex].canConsumeCastBonus || !IsReady())
		{
			return;
		}
		NotifyUse();
		StartCooldown();
		FxPlayNewNetworked(startEffect, owner);
		List<Entity> affected = new List<Entity>();
		info.instance.ActorEvent_OnDealDamage += (Action<EventInfoDamage>)((EventInfoDamage dmg) =>
		{
			if (isValid && owner.CheckEnemyOrNeutral(dmg.victim) && !affected.Contains(dmg.victim))
			{
				affected.Add(dmg.victim);
				((MonoBehaviour)(object)this).StartCoroutine(StarPerVictimRoutine(dmg.victim, info.instance));
			}
		});
	}

	private IEnumerator StarPerVictimRoutine(Entity victim, AbilityInstance source)
	{
		source.LockDestroy();
		yield return new WaitForSeconds(firstDelay);
		source.UnlockDestroy();
		for (int i = 0; i < stars; i++)
		{
			if (!isValid)
			{
				break;
			}
			if (victim.IsNullInactiveDeadOrKnockedOut())
			{
				break;
			}
			CreateAbilityInstanceWithSource<Ai_Gem_R_Celestial>(source, victim.position, Quaternion.identity, new CastInfo(owner, victim));
			yield return new WaitForSeconds(interval);
		}
	}

	private void MirrorProcessed()
	{
	}
}
