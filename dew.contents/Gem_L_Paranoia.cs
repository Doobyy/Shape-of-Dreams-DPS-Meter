using System;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;

public class Gem_L_Paranoia : Gem
{
	public ScalingValue skillHaste;

	public float searchInterval = 0.25f;

	public DewCollider range;

	public float duration;

	public GameObject casterEffect;

	private float _lastSearchTime;

	private SkillBonus _bonus;

	private bool _isEnemyInRange;

	private float _lastDamageTime = float.NegativeInfinity;

	public float reducedRatio => 1f - 1f / (1f + GetValue(skillHaste) * 0.01f);

	public override bool IsReady()
	{
		if (base.IsReady() && _bonus != null)
		{
			return _bonus.cooldownMultiplier < 0.99f;
		}
		return false;
	}

	protected override void OnCastComplete(EventInfoCast info)
	{
		base.OnCastComplete(info);
		NotifyUse();
	}

	public override void OnEquipGem(Hero newOwner)
	{
		base.OnEquipGem(newOwner);
		if (((NetworkBehaviour)this).isServer)
		{
			newOwner.EntityEvent_OnTakeDamage += new Action<EventInfoDamage>(EntityEventOnTakeDamage);
		}
	}

	private void EntityEventOnTakeDamage(EventInfoDamage obj)
	{
		if (!(obj.damage.amount < 0.9f))
		{
			_lastDamageTime = Time.time;
			NotifyUse();
		}
	}

	public override void OnUnequipGem(Hero oldOwner)
	{
		base.OnUnequipGem(oldOwner);
		if (((NetworkBehaviour)this).isServer)
		{
			if ((UnityEngine.Object)(object)oldOwner != null)
			{
				oldOwner.EntityEvent_OnTakeDamage -= new Action<EventInfoDamage>(EntityEventOnTakeDamage);
			}
			FxStopNetworked(casterEffect);
			_lastDamageTime = float.NegativeInfinity;
			numberDisplay = 0;
		}
	}

	public override void OnEquipSkill(SkillTrigger newSkill)
	{
		base.OnEquipSkill(newSkill);
		if (((NetworkBehaviour)this).isServer)
		{
			_bonus = new SkillBonus();
			_bonus.cooldownMultiplier = 1f - reducedRatio;
			newSkill.AddSkillBonus(_bonus);
		}
	}

	public override void OnUnequipSkill(SkillTrigger oldSkill)
	{
		base.OnUnequipSkill(oldSkill);
		if (((NetworkBehaviour)this).isServer)
		{
			if ((UnityEngine.Object)(object)oldSkill != null)
			{
				oldSkill.RemoveSkillBonus(_bonus);
			}
			_bonus = null;
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (!((NetworkBehaviour)this).isServer || (UnityEngine.Object)(object)owner == null || (UnityEngine.Object)(object)skill == null)
		{
			return;
		}
		numberDisplay = Mathf.CeilToInt(Mathf.Clamp(duration - (Time.time - _lastDamageTime), 0f, duration));
		if (Time.time - _lastDamageTime > duration)
		{
			if ((double)_bonus.cooldownMultiplier < 0.9)
			{
				_bonus.cooldownMultiplier = 1f;
				FxStopNetworked(casterEffect);
			}
			return;
		}
		if (_bonus != null && Math.Abs(_bonus.cooldownMultiplier - (1f - reducedRatio)) > 0.001f)
		{
			_bonus.cooldownMultiplier = 1f - reducedRatio;
			FxPlayNetworked(casterEffect, owner);
		}
		if (Time.time - _lastSearchTime > searchInterval)
		{
			((Component)(object)this).transform.position = owner.agentPosition;
			_lastSearchTime = Time.time;
			List<Entity> entities = range.GetEntities(out var handle, (Entity e) => owner.GetRelation(e) == EntityRelation.Enemy);
			_isEnemyInRange = entities.Count > 0;
			handle.Return();
		}
		if (!_isEnemyInRange || owner.IsNullInactiveDeadOrKnockedOut() || owner.Control.ongoingChannels.Count > 0 || !skill.CanBeCast() || !skill.CanBeReserved() || owner.Control.queuedActions.Any((ActionBase c) => c is ActionCast actionCast && (UnityEngine.Object)(object)actionCast.trigger == (UnityEngine.Object)(object)skill) || NetworkedManagerBase<ZoneManager>.instance.isInAnyTransition || !skill.currentConfig.isActive)
		{
			return;
		}
		CastInfo info;
		if (skill.currentConfig.castMethod.type == CastMethodType.None)
		{
			info = new CastInfo(owner);
		}
		else
		{
			Entity entity = null;
			ListReturnHandle<Entity> handle2;
			foreach (Entity item in DewPhysics.OverlapCircleAllEntities(out handle2, ((Component)(object)this).transform.position, skill.currentConfig.effectiveRange, new CollisionCheckSettings
			{
				sortComparer = CollisionCheckSettings.DistanceFromCenter
			}))
			{
				if (skill.currentConfig.targetValidator.Evaluate(owner, item))
				{
					entity = item;
					break;
				}
			}
			handle2.Return();
			if ((UnityEngine.Object)(object)entity == null)
			{
				return;
			}
			info = skill.GetPredictedCastInfoToTarget(entity, UnityEngine.Random.Range(0f, 0.5f));
		}
		owner.Control.Cast(skill, skill.currentConfigIndex, info);
	}

	protected override void OnQualityChange(int oldQuality, int newQuality)
	{
		base.OnQualityChange(oldQuality, newQuality);
		if (((NetworkBehaviour)this).isServer && _bonus != null)
		{
			_bonus.cooldownMultiplier = 1f - reducedRatio;
		}
	}

	private void MirrorProcessed()
	{
	}
}
