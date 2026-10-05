using System;
using Mirror;
using UnityEngine;

public class MirageSkinEffect : StatusEffect, IOtherPlayersTonedDownDisable
{
	public int tier = -1;

	public GameObject fxDamage;

	public GameObject fxDamageOverTime;

	public GameObject fxBreak;

	public float fxDamagedCooldown;

	public float breakStunDuration;

	public float maxHealthRatio;

	public float shieldMaxHealthRatio;

	public bool enableSpecialAttack;

	public Vector2 specialAttackInterval;

	public bool allowWhenChanneling;

	[NonSerialized]
	public float nextSpecialAttackTime;

	[NonSerialized]
	public float? customAmount;

	private float _lastDamageEffectTime;

	private float _lastDamageOverTimeEffectTime;

	private Action<EventInfoDamage> _cachedOnDealDamage;

	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (victim.Visual.mirageSkinObjects.TryGetValue(((object)this).GetType().Name, out var value))
		{
			foreach (GameObject item in value)
			{
				if (!(item == null))
				{
					item.SetActive(value: true);
				}
			}
		}
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		DoUnstoppable();
		victim.Status.CalculateStats();
		float num;
		if (customAmount.HasValue)
		{
			num = customAmount.Value;
		}
		else
		{
			num = victim.maxHealth * shieldMaxHealthRatio;
			victim.Status.AddStatBonus(new StatBonus
			{
				maxHealthPercentage = maxHealthRatio * 100f - 100f
			});
		}
		victim.Status.mirageSkinInitAmount = num;
		DoShield(num, (EventInfoDamageNegatedByShield obj) =>
		{
			bool flag = obj.damage.HasAttr(DamageAttribute.DamageOverTime);
			if (flag && Time.time - _lastDamageOverTimeEffectTime > fxDamagedCooldown)
			{
				FxPlayNewNetworked(fxDamageOverTime, victim);
			}
			else if (!flag && Time.time - _lastDamageEffectTime > fxDamagedCooldown)
			{
				FxPlayNewNetworked(fxDamage, victim);
			}
			if (isActive && obj.shield.amount < 0.0001f)
			{
				FxPlayNetworked(fxBreak, victim);
				Destroy();
				victim.Status.CalculateStats();
				if (breakStunDuration > 0.001f)
				{
					victim.Stagger(obj.damage.direction);
					CreateBasicEffect(victim, new StunEffect(), breakStunDuration, "MirageSkinBreakStun");
				}
			}
		});
		victim.ActorEvent_OnDealDamage += _cachedOnDealDamage ?? (_cachedOnDealDamage = OnDealDamage);
		nextSpecialAttackTime = Time.time + specialAttackInterval.RandomRange();
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (!((NetworkBehaviour)this).isServer || victim.Visual.isSpawning || victim.Visual.isRendererOff || !enableSpecialAttack || !(Time.time > nextSpecialAttackTime) || (!allowWhenChanneling && victim.Control.ongoingChannels.Count != 0))
		{
			return;
		}
		nextSpecialAttackTime = Time.time + specialAttackInterval.RandomRange();
		try
		{
			OnSpecialAttack();
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (victim.Visual.mirageSkinObjects.TryGetValue(((object)this).GetType().Name, out var value))
		{
			foreach (GameObject item in value)
			{
				if (!(item == null))
				{
					item.SetActive(value: false);
				}
			}
		}
		if (((NetworkBehaviour)this).isServer && !((UnityEngine.Object)(object)victim == null))
		{
			victim.ActorEvent_OnDealDamage -= _cachedOnDealDamage ?? (_cachedOnDealDamage = OnDealDamage);
		}
	}

	public virtual void OnDealDamage(EventInfoDamage obj)
	{
	}

	public virtual void OnSpecialAttack()
	{
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		nextSpecialAttackTime = 0f;
		_lastDamageEffectTime = 0f;
		_lastDamageOverTimeEffectTime = 0f;
		customAmount = null;
	}

	private void MirrorProcessed()
	{
	}
}
