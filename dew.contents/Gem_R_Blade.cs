using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gem_R_Blade : Gem
{
	private struct Ad_BladeImmunity
	{
		public Dictionary<Entity, float> applyTime;
	}

	public ScalingValue baseCount;

	public float delay;

	public AnimationCurve intervalCurve;

	public float perEnemyCooldown;

	public DewAudioClip castVoice;

	private float _reducedCooldownTime;

	protected override void Awake()
	{
		base.Awake();
		ClientGemEvent_OnCooldownReduced += new Action<float>(ClientGemEventOnCooldownReduced);
		ClientGemEvent_OnCooldownReducedByRatio += new Action<float>(ClientGemEventOnCooldownReducedByRatio);
	}

	private void ClientGemEventOnCooldownReduced(float obj)
	{
		_reducedCooldownTime += obj;
	}

	private void ClientGemEventOnCooldownReducedByRatio(float obj)
	{
		_reducedCooldownTime += obj * perEnemyCooldown;
	}

	protected override void OnDealDamage(EventInfoDamage info)
	{
		base.OnDealDamage(info);
		int count;
		float interval;
		if (!info.chain.DidReact(this) && owner.CheckEnemyOrNeutral(info.victim))
		{
			count = Mathf.RoundToInt(GetValue(baseCount));
			interval = intervalCurve.Evaluate(count);
			((MonoBehaviour)(object)this).StartCoroutine(Routine());
		}
		IEnumerator Routine()
		{
			if (IsReady() && isValid && !((UnityEngine.Object)(object)owner == null) && !((UnityEngine.Object)(object)info.victim == null) && owner.CheckEnemyOrNeutral(info.victim))
			{
				if (info.victim.TryGetData<Ad_BladeImmunity>(out var data))
				{
					if (data.applyTime.TryGetValue(owner, out var value) && Time.time + _reducedCooldownTime - value < perEnemyCooldown)
					{
						yield break;
					}
					data.applyTime[owner] = Time.time + _reducedCooldownTime;
				}
				else
				{
					info.victim.AddData(new Ad_BladeImmunity
					{
						applyTime = new Dictionary<Entity, float> { 
						{
							owner,
							Time.time + _reducedCooldownTime
						} }
					});
				}
				owner.Sound.Say(castVoice, interruptPrevious: true);
				info.actor.LockDestroyFor(delay + (float)count * interval + 0.1f);
				yield return new WaitForSeconds(delay);
				for (int i = 0; i < count; i++)
				{
					if (!IsReady())
					{
						break;
					}
					if ((UnityEngine.Object)(object)owner == null)
					{
						break;
					}
					if (info.victim.IsNullInactiveDeadOrKnockedOut())
					{
						break;
					}
					CreateAbilityInstanceWithSource(info.actor, owner.position, Quaternion.identity, new CastInfo(owner, info.victim), (Ai_Gem_R_Blade a) =>
					{
						a.chain = info.chain.New(this);
					});
					NotifyUse();
					yield return new WaitForSeconds(interval);
				}
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
