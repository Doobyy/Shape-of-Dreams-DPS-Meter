using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Gem_U_GlacialCore : Gem
{
	public float shootRadius;

	public GameObject healEffect;

	public GameObject healCritEffect;

	public ScalingValue healMaxHealthRatio;

	public float chillAmp = 1f;

	public float delay = 0.5f;

	public float shootMinDamage;

	public float shootMaxDamageMaxHpRatio;

	public AnimationCurve normalizedShootInterval;

	public float maxShootCount;

	public Vector2 procCoefficient;

	public ScalingValue healToDamageRatio;

	private float _lastShootTime;

	private float _pendingDamage;

	private float _pendingProc;

	private ReactionChain _pendingChain;

	private Action<Ai_Gem_U_GlacialCore_Projectile> _cachedInitProjectile;

	private Dictionary<ReactionChain, float> _remainingDamages = new Dictionary<ReactionChain, float>(new ReactionChainComparer());

	private List<ReactionChain> _keysToRemove = new List<ReactionChain>();

	private List<ReactionChain> _keysToDecrease = new List<ReactionChain>();

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnRoomLoaded += new Action<EventInfoLoadRoom>(ClientEventOnRoomLoaded);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && !((UnityEngine.Object)(object)NetworkedManagerBase<ZoneManager>.instance == null))
		{
			NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnRoomLoaded -= new Action<EventInfoLoadRoom>(ClientEventOnRoomLoaded);
		}
	}

	private void ClientEventOnRoomLoaded(EventInfoLoadRoom _)
	{
		_remainingDamages.Clear();
		numberDisplay = 0;
	}

	public override void OnEquipGem(Hero newOwner)
	{
		base.OnEquipGem(newOwner);
		if (((NetworkBehaviour)this).isServer)
		{
			newOwner.EntityEvent_OnTakeHeal += new Action<EventInfoHeal>(EntityEventOnTakeHeal);
		}
	}

	public override void OnUnequipGem(Hero oldOwner)
	{
		base.OnUnequipGem(oldOwner);
		if (((NetworkBehaviour)this).isServer)
		{
			if ((UnityEngine.Object)(object)oldOwner != null)
			{
				oldOwner.EntityEvent_OnTakeHeal -= new Action<EventInfoHeal>(EntityEventOnTakeHeal);
			}
			_remainingDamages.Clear();
		}
	}

	public override void OnUnequipSkill(SkillTrigger oldSkill)
	{
		base.OnUnequipSkill(oldSkill);
		if (((NetworkBehaviour)this).isServer)
		{
			_remainingDamages.Clear();
		}
	}

	private void EntityEventOnTakeHeal(EventInfoHeal obj)
	{
		if (!isValid)
		{
			return;
		}
		float num = (obj.amount + obj.discardedAmount) * GetValue(healToDamageRatio);
		if (_remainingDamages.ContainsKey(obj.chain))
		{
			_remainingDamages[obj.chain] += num;
		}
		else if (_remainingDamages.Count > 100)
		{
			ReactionChain key = default;
			foreach (KeyValuePair<ReactionChain, float> remainingDamage in _remainingDamages)
			{
				key = remainingDamage.Key;
			}
			_remainingDamages[key] += num;
		}
		else
		{
			_remainingDamages[obj.chain] = num;
		}
	}

	private void InitProjectile(Ai_Gem_U_GlacialCore_Projectile p)
	{
		p._damageAmount = _pendingDamage;
		p.Network_procCoefficient = _pendingProc;
		p.chain = _pendingChain.New(this);
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (!((NetworkBehaviour)this).isServer || !isValid)
		{
			return;
		}
		if (_remainingDamages.Count <= 0)
		{
			numberDisplay = 0;
			return;
		}
		float num = 0f;
		foreach (KeyValuePair<ReactionChain, float> remainingDamage in _remainingDamages)
		{
			num += remainingDamage.Value;
		}
		float num2 = shootMinDamage;
		float num3 = shootMaxDamageMaxHpRatio * owner.maxHealth;
		float num4 = Mathf.Max(num3, num / maxShootCount);
		_keysToRemove.Clear();
		_keysToDecrease.Clear();
		bool flag = false;
		ReactionChain pendingChain = default;
		float num5 = 0f;
		foreach (KeyValuePair<ReactionChain, float> remainingDamage2 in _remainingDamages)
		{
			if (!flag)
			{
				flag = true;
				pendingChain = remainingDamage2.Key;
			}
			if (remainingDamage2.Value > num4)
			{
				_keysToDecrease.Add(remainingDamage2.Key);
				num5 = num4;
				break;
			}
			num5 += remainingDamage2.Value;
			_keysToRemove.Add(remainingDamage2.Key);
			if (num5 >= num4)
			{
				break;
			}
		}
		float num6 = (num5 - num2) / (num3 - num2);
		float t = Mathf.Clamp01(num6);
		float num7 = normalizedShootInterval.Evaluate(num6);
		if (Time.time - _lastShootTime <= num7)
		{
			numberDisplay = Mathf.RoundToInt(num);
			return;
		}
		foreach (ReactionChain item in _keysToRemove)
		{
			_remainingDamages.Remove(item);
		}
		foreach (ReactionChain item2 in _keysToDecrease)
		{
			_remainingDamages[item2] -= num4;
		}
		numberDisplay = Mathf.RoundToInt(num - num5);
		_lastShootTime = Time.time;
		float pendingProc = Mathf.Lerp(procCoefficient.x, procCoefficient.y, t);
		List<Entity> list = DewPhysics.OverlapCircleAllEntities(out var handle, owner.position, shootRadius, tvDefaultHarmfulEffectTargets);
		if (list.Count > 0)
		{
			Entity target = list[UnityEngine.Random.Range(0, list.Count)];
			_pendingDamage = num5;
			_pendingProc = pendingProc;
			_pendingChain = pendingChain;
			CreateAbilityInstance<Ai_Gem_U_GlacialCore_Projectile>(owner.position, Quaternion.identity, new CastInfo(owner, target), InitProjectile);
			handle.Return();
			NotifyUse();
		}
	}

	protected override void OnDealDamage(EventInfoDamage info)
	{
		base.OnDealDamage(info);
		if (isValid)
		{
			((MonoBehaviour)(object)this).StartCoroutine(Routine());
		}
		IEnumerator Routine()
		{
			info.actor.LockDestroy();
			yield return new WaitForSeconds(delay);
			if (!isValid || !IsReady() || info.chain.DidReact(this) || owner.IsNullOrInactive() || !owner.CheckEnemyOrNeutral(info.victim))
			{
				info.actor.UnlockDestroy();
			}
			else
			{
				HealData heal = new HealData(GetValue(healMaxHealthRatio) * owner.maxHealth);
				if (info.damage.elemental == ElementalType.Cold)
				{
					FxPlayNetworked(healCritEffect, owner);
					heal.ApplyAmplification(chillAmp);
					heal.SetCrit();
				}
				else
				{
					FxPlayNetworked(healEffect, owner);
				}
				info.actor.DoHeal(heal, owner, info.chain.New(this));
				NotifyUse();
				StartCooldown();
				info.actor.UnlockDestroy();
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
