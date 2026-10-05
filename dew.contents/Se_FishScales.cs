using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

[SaveActor(true)]
public class Se_FishScales : StatusEffect
{
	public float normalRatio = 0.3f;

	public float bossRatio = 0.4f;

	public float maxShieldHpRatio = 1f;

	public float meleeMultiplier = 0.65f;

	public float guidanceRestoreMaxHpRatio = 0.25f;

	public float blessedGuidanceRestoreMaxHpRatio = 0.5f;

	[SaveVar(SaveVarFlags.Default)]
	private float _currentAmount;

	private StatBonus _bonus;

	private FakeMaxHealthEffect _fakeMaxHealthEffect;

	private readonly List<(Shrine shrine, Action<Entity> handler)> _useSubscriptions = new List<(Shrine, Action<Entity>)>();

	private float _maxShieldAmount;

	private float _maxShieldAmountCacheTime;

	private ActorRef<Gem_L_Supersymmetry> _supersymmetry;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			_maxShieldAmountCacheTime = float.NegativeInfinity;
			_supersymmetry = null;
			_bonus = DoStatBonus();
			float currentHealth = victim.currentHealth;
			_fakeMaxHealthEffect = DoFakeMaxHealthEffect(_currentAmount);
			SetAmount(_currentAmount);
			victim.Status.CalculateStats();
			victim.Status.SetHealth(currentHealth);
			victim.EntityEvent_OnTakeDamage += new Action<EventInfoDamage>(EntityEventOnTakeDamage);
			victim.takenShieldProcessor.Add(TakenShieldProcessor, 9999);
			if (victim is Hero hero)
			{
				hero.ClientHeroEvent_OnKnockedOut += new Action<EventInfoKill>(ClientHeroEventOnKnockedOut);
			}
			NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnZoneLoaded += new Action<EventInfoLoadZone>(ClientEventOnZoneLoaded);
			NetworkedManagerBase<ActorManager>.instance.ClientEvent_OnActorAdd += new Action<Actor>(ClientEventOnActorAdd);
		}
	}

	private void ClientEventOnActorAdd(Actor obj)
	{
		if (obj is Shrine_Guidance shrine_Guidance)
		{
			Action<Entity> action = (Entity entity) =>
			{
				if (!this.IsNullOrInactive())
				{
					float amount = GetAmount();
					amount -= guidanceRestoreMaxHpRatio * (amount + entity.Status.maxHealth);
					amount = Mathf.Max(0f, amount);
					SetAmount(amount);
				}
			};
			shrine_Guidance.ClientEvent_OnSuccessfulUse += action;
			_useSubscriptions.Add((shrine_Guidance, action));
		}
		if (!(obj is Shrine_BlessedGuidance shrine_BlessedGuidance))
		{
			return;
		}
		Action<Entity> action2 = (Entity entity) =>
		{
			if (!this.IsNullOrInactive())
			{
				float amount = GetAmount();
				amount -= blessedGuidanceRestoreMaxHpRatio * (amount + entity.Status.maxHealth);
				amount = Mathf.Max(0f, amount);
				SetAmount(amount);
			}
		};
		shrine_BlessedGuidance.ClientEvent_OnSuccessfulUse += action2;
		_useSubscriptions.Add((shrine_BlessedGuidance, action2));
	}

	private void TakenShieldProcessor(ref HealData data, Actor actor, Entity target)
	{
		if ((UnityEngine.Object)(object)actor != null && (UnityEngine.Object)(object)actor.FindFirstOfType<Gem_L_Supersymmetry>() != null)
		{
			return;
		}
		float maxShieldAmount = GetMaxShieldAmount();
		float num = target.Status.currentShield;
		Gem_L_Supersymmetry gem_L_Supersymmetry = _supersymmetry.Get();
		if (!gem_L_Supersymmetry.IsNullOrInactive())
		{
			num = Mathf.Max(num - gem_L_Supersymmetry.currentShieldAmount, 0f);
		}
		float num2 = Mathf.Max(maxShieldAmount - num, 0f);
		if (data.currentAmount > num2)
		{
			if (float.IsInfinity(data.currentAmount))
			{
				data.SetOriginalAmount(100f);
			}
			data.ApplyRawMultiplier(num2 / data.currentAmount);
		}
	}

	private float GetMaxShieldAmount()
	{
		if (Time.time - _maxShieldAmountCacheTime > 0.5f)
		{
			_maxShieldAmountCacheTime = Time.time;
			_supersymmetry = null;
			_maxShieldAmount = victim.maxHealth * maxShieldHpRatio;
			if (victim is Hero hero)
			{
				foreach (KeyValuePair<GemLocation, Gem> gem in hero.Skill.gems)
				{
					if (gem.Value is Gem_L_Supersymmetry gem_L_Supersymmetry)
					{
						_supersymmetry = gem_L_Supersymmetry;
						_maxShieldAmount = gem_L_Supersymmetry.maxHealthBeforeReduction * maxShieldHpRatio;
						break;
					}
				}
			}
		}
		return _maxShieldAmount;
	}

	private void ClientEventOnZoneLoaded(EventInfoLoadZone obj)
	{
		if (obj.isTraveling)
		{
			SetAmount(0f);
		}
	}

	private void ClientHeroEventOnKnockedOut(EventInfoKill obj)
	{
		SetAmount(0f);
	}

	private void EntityEventOnTakeDamage(EventInfoDamage obj)
	{
		Monster monster = obj.actor.FindFirstOfType<Monster>();
		if ((UnityEngine.Object)(object)monster == null)
		{
			return;
		}
		float num = obj.damage.amount - obj.negatedAmountByShield;
		if (!(num < 1f))
		{
			float num2 = (monster.IsAnyBoss() ? bossRatio : normalRatio);
			if (victim is Hero hero && hero.IsMeleeHero())
			{
				num2 *= meleeMultiplier;
			}
			SetAmount(GetAmount() + num * num2);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		if ((UnityEngine.Object)(object)victim != null)
		{
			victim.EntityEvent_OnTakeDamage -= new Action<EventInfoDamage>(EntityEventOnTakeDamage);
			victim.takenShieldProcessor.Remove(TakenShieldProcessor);
		}
		if (victim is Hero hero)
		{
			hero.ClientHeroEvent_OnKnockedOut -= new Action<EventInfoKill>(ClientHeroEventOnKnockedOut);
		}
		if ((UnityEngine.Object)(object)NetworkedManagerBase<ZoneManager>.instance != null)
		{
			NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnZoneLoaded -= new Action<EventInfoLoadZone>(ClientEventOnZoneLoaded);
		}
		if ((UnityEngine.Object)(object)NetworkedManagerBase<ActorManager>.instance != null)
		{
			NetworkedManagerBase<ActorManager>.instance.ClientEvent_OnActorAdd -= new Action<Actor>(ClientEventOnActorAdd);
		}
		foreach (var (shrine, action) in _useSubscriptions)
		{
			if ((UnityEngine.Object)(object)shrine != null)
			{
				shrine.ClientEvent_OnSuccessfulUse -= action;
			}
		}
		_useSubscriptions.Clear();
	}

	public float GetAmount()
	{
		return _fakeMaxHealthEffect.strength;
	}

	public void SetAmount(float decreasedAmount)
	{
		_currentAmount = decreasedAmount;
		_bonus.maxHealthFlat = 0f - decreasedAmount;
		_fakeMaxHealthEffect.strength = decreasedAmount;
	}

	private void MirrorProcessed()
	{
	}
}
