using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Ai_Mon_Ink_DivineAnimal_Bite : AbilityInstance
{
	public struct Ad_CheckBiteDuplication
	{
		public float LastHitTime;
	}

	public DewCollider range;

	public float length;

	public float chargeDuration;

	public float fowardDistance;

	public float displaceDuration;

	public float stunDuration;

	public DewEase ease;

	public float ripAtkDelay;

	public float dmgInterval;

	public ScalingValue biteDmgFactor;

	public ScalingValue ripDmgFactor;

	public AbilityTargetValidator hittable;

	public GameObject catchSound;

	public DewAnimationClip failedAnim;

	public GameObject biteEffect;

	public GameObject ripEffect;

	public float chainStunPreventTime;

	private List<Entity> _entsHit;

	private Vector3 _originPos;

	public override bool reuseInRoom => true;

	protected override IEnumerator OnCreateSequenced()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		DestroyOnDeath(info.caster);
		_entsHit = new List<Entity>();
		_originPos = info.caster.agentPosition;
		info.caster.Control.StartDaze(chargeDuration);
		info.caster.Control.StartDisplacement(new DispByDestination
		{
			destination = info.caster.position + info.forward * length,
			duration = chargeDuration,
			ease = ease,
			isFriendly = true,
			rotateForward = true,
			canGoOverTerrain = false
		});
		List<Entity> entities = range.GetEntities(out var _, hittable, info.caster);
		Vector3 end = _originPos + info.forward * (fowardDistance + length);
		Vector3 validAgentDestination_Closest = Dew.GetValidAgentDestination_Closest(info.caster.position, end);
		for (int i = 0; i < entities.Count; i++)
		{
			Entity entity = entities[i];
			if (_entsHit.Contains(entity))
			{
				continue;
			}
			if (entity.HasData<Ad_CheckBiteDuplication>())
			{
				if (Time.time - entity.GetData<Ad_CheckBiteDuplication>().LastHitTime < chainStunPreventTime)
				{
					continue;
				}
				entity.RemoveData<Ad_CheckBiteDuplication>();
			}
			if (entity.Status.hasUnstoppable)
			{
				CreateDamage(DamageData.SourceType.Default, biteDmgFactor).Dispatch(entity);
				FxPlay(biteEffect, entity);
				continue;
			}
			entities[i].Control.StartDaze(displaceDuration);
			entities[i].Control.StartDisplacement(new DispByDestination
			{
				canGoOverTerrain = false,
				affectedByMovementSpeed = false,
				destination = validAgentDestination_Closest,
				duration = displaceDuration,
				ease = ease,
				isFriendly = false,
				rotateForward = false,
				isCanceledByCC = false
			});
			if ((Object)(object)entity != null)
			{
				FxPlayNetworked(catchSound);
				CreateBasicEffect(entity, new StunEffect(), stunDuration);
				_entsHit.Add(entity);
				if (!entity.HasData<Ad_CheckBiteDuplication>())
				{
					entity.AddData(new Ad_CheckBiteDuplication
					{
						LastHitTime = Time.time
					});
				}
			}
		}
		yield return new SI.WaitForSeconds(chargeDuration + 0.001f);
		if (_entsHit.Count == 0)
		{
			info.caster.Animation.StopAbilityAnimation(firstTrigger.currentConfig.endAnim);
			info.caster.Animation.PlayAbilityAnimation(failedAnim);
			ApplyCooldownReductionByRatio(firstTrigger, 0.5f);
			Destroy();
		}
		int count = 0;
		while (count < 3)
		{
			for (int j = 0; j < _entsHit.Count; j++)
			{
				Entity ent = _entsHit[j];
				if (count == 2)
				{
					yield return new SI.WaitForSeconds(ripAtkDelay);
					CreateDamage(DamageData.SourceType.Default, ripDmgFactor).Dispatch(ent);
					FxPlay(ripEffect, ent);
					count++;
				}
				else
				{
					CreateDamage(DamageData.SourceType.Default, biteDmgFactor).Dispatch(ent);
					FxPlay(biteEffect, ent);
					count++;
				}
			}
			yield return new SI.WaitForSeconds(dmgInterval);
		}
		Destroy();
	}

	private void DoDamage(GameObject eff, ScalingValue damage)
	{
		for (int i = 0; i < _entsHit.Count; i++)
		{
			Entity entity = _entsHit[i];
			CreateDamage(DamageData.SourceType.Default, damage).Dispatch(entity);
			FxPlay(eff, entity);
		}
	}

	private void MirrorProcessed()
	{
	}
}
