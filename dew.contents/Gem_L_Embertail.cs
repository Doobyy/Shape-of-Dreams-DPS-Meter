using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Gem_L_Embertail : Gem
{
	public DewCollider burnCheckRange;

	public AbilityTargetValidator burnCheckable;

	public DewCollider bounceRange;

	public AbilityTargetValidator hittable;

	public GameObject attackEffect;

	public ScalingValue ampPerBurning;

	public ScalingValue skillHastePerBurning;

	public float burningCheckInterval = 0.4f;

	public int critThresholdEnemies;

	private float _lastCheckTime;

	private int _burningEnemies;

	private SkillBonus _bonus;

	private static readonly Stack<(ReactionChain chain, float strength)> _pendingEmbertail = new Stack<(ReactionChain, float)>();

	private static readonly Action<Ai_Gem_L_Embertail_Projectile> _applyPendingEmbertail = ApplyPendingEmbertail;

	public override void OnEquipGem(Hero newOwner)
	{
		base.OnEquipGem(newOwner);
		if (((NetworkBehaviour)this).isServer)
		{
			newOwner.EntityEvent_OnAttackEffectTriggered += new Action<EventInfoAttackEffect>(SpawnBouncingFire);
		}
	}

	public override void OnUnequipGem(Hero oldOwner)
	{
		base.OnUnequipGem(oldOwner);
		if (((NetworkBehaviour)this).isServer && !((UnityEngine.Object)(object)oldOwner == null))
		{
			oldOwner.EntityEvent_OnAttackEffectTriggered -= new Action<EventInfoAttackEffect>(SpawnBouncingFire);
		}
	}

	public override void OnEquipSkill(SkillTrigger newSkill)
	{
		base.OnEquipSkill(newSkill);
		if (((NetworkBehaviour)this).isServer)
		{
			newSkill.dealtDamageProcessor.Add(AmpDamage);
			newSkill.dealtHealProcessor.Add(AmpHeal);
			_bonus = new SkillBonus();
			newSkill.AddSkillBonus(_bonus);
		}
	}

	public override void OnUnequipSkill(SkillTrigger oldSkill)
	{
		base.OnUnequipSkill(oldSkill);
		if (((NetworkBehaviour)this).isServer && !((UnityEngine.Object)(object)oldSkill == null))
		{
			oldSkill.dealtDamageProcessor.Remove(AmpDamage);
			oldSkill.dealtHealProcessor.Remove(AmpHeal);
			oldSkill.RemoveSkillBonus(_bonus);
			_bonus = null;
		}
	}

	private void AmpDamage(ref DamageData data, Actor actor, Entity target)
	{
		if (!data.IsAmountModifiedBy(this) && _burningEnemies > 0 && owner.CheckEnemyOrNeutral(target))
		{
			float value = GetValue(ampPerBurning) * (float)_burningEnemies;
			if (_burningEnemies >= critThresholdEnemies)
			{
				data.SetAttr(DamageAttribute.IsCrit);
			}
			data.ApplyAmplification(value);
			data.SetAmountModifiedBy(this);
		}
	}

	private void AmpHeal(ref HealData data, Actor actor, Entity target)
	{
		if (!data.IsAmountModifiedBy(this) && _burningEnemies > 0)
		{
			float value = GetValue(ampPerBurning) * (float)_burningEnemies;
			if (_burningEnemies >= critThresholdEnemies)
			{
				data.SetCrit();
			}
			data.ApplyAmplification(value);
			data.SetAmountModifiedBy(this);
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (!((NetworkBehaviour)this).isServer || (UnityEngine.Object)(object)owner == null || _bonus == null || !(Time.time - _lastCheckTime > burningCheckInterval))
		{
			return;
		}
		((Component)(object)this).transform.position = owner.agentPosition;
		_lastCheckTime = Time.time;
		int num = 0;
		ListReturnHandle<Entity> handle;
		foreach (Entity entity in burnCheckRange.GetEntities(out handle, burnCheckable, owner))
		{
			if (entity.Status.fireStack > 0)
			{
				num++;
			}
		}
		handle.Return();
		_burningEnemies = Mathf.Max(num, _burningEnemies - 1);
		_bonus.cooldownMultiplier = 100f / (100f + GetValue(skillHastePerBurning) * (float)_burningEnemies);
		numberDisplay = _burningEnemies;
	}

	private void SpawnBouncingFire(EventInfoAttackEffect obj)
	{
		if (obj.chain.DidReact(this))
		{
			return;
		}
		FxPlayNewNetworked(attackEffect, obj.victim);
		bounceRange.transform.position = obj.victim.position;
		List<Entity> entities = bounceRange.GetEntities(out var handle, hittable, owner);
		if (entities.Count > 0)
		{
			Entity entity = entities[UnityEngine.Random.Range(0, entities.Count)];
			if (entities.Count > 2)
			{
				if ((UnityEngine.Object)(object)entity == (UnityEngine.Object)(object)obj.victim)
				{
					entity = entities[UnityEngine.Random.Range(0, entities.Count)];
				}
				if ((UnityEngine.Object)(object)entity == (UnityEngine.Object)(object)obj.victim)
				{
					entity = entities[UnityEngine.Random.Range(0, entities.Count)];
				}
				if ((UnityEngine.Object)(object)entity == (UnityEngine.Object)(object)obj.victim)
				{
					entity = entities[UnityEngine.Random.Range(0, entities.Count)];
				}
			}
			_pendingEmbertail.Push((obj.chain.New(this), obj.strength));
			try
			{
				CreateAbilityInstance(obj.victim.Visual.GetCenterPosition(), null, new CastInfo(owner, entity), _applyPendingEmbertail);
			}
			finally
			{
				_pendingEmbertail.Pop();
			}
		}
		handle.Return();
		NotifyUse();
	}

	private static void ApplyPendingEmbertail(Ai_Gem_L_Embertail_Projectile p)
	{
		(p.chain, p._strengthMultiplier) = _pendingEmbertail.Peek();
	}

	private void MirrorProcessed()
	{
	}
}
