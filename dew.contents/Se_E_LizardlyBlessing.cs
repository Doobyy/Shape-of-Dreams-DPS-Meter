using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Se_E_LizardlyBlessing : StatusEffect, ACH_THEYRE_JUST_BIG_CATS.ILightingActor
{
	public GameObject fxKill;

	public GameObject fxFirstDamageHit;

	public ScalingValue firstDamage;

	public ScalingValue speedAmount;

	public ScalingValue hasteAmount;

	public ScalingValue takenDamageAmp;

	public ScalingValue goldPerKillAmount;

	public float duration = 6f;

	private KillTracker _tracker;

	private List<Entity> _hitEntities = new List<Entity>();

	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			SetTimer(duration);
			ShowOnScreenTimer();
			DoSpeed(GetValue(speedAmount));
			DoHaste(GetValue(hasteAmount));
			victim.takenDamageProcessor.Add(Processor);
			victim.ActorEvent_OnDealDamage += new Action<EventInfoDamage>(ActorEventOnDealDamage);
			if (victim is Hero hero)
			{
				hero.ClientHeroEvent_OnKillOrAssist += new Action<EventInfoKill>(ClientHeroEventOnKillOrAssist);
			}
		}
	}

	private void ActorEventOnDealDamage(EventInfoDamage obj)
	{
		if (!(obj.actor is ElementalStatusEffect) && !_hitEntities.Contains(obj.victim) && victim.CheckEnemyOrNeutral(obj.victim))
		{
			_hitEntities.Add(obj.victim);
			FxPlayNewNetworked(fxFirstDamageHit, obj.victim);
			Damage(firstDamage).SetOriginPosition(victim.agentPosition).SetElemental(ElementalType.Light).Dispatch(obj.victim);
		}
	}

	private void ClientHeroEventOnKillOrAssist(EventInfoKill obj)
	{
		if (victim.CheckEnemyOrNeutral(obj.victim))
		{
			NetworkedManagerBase<PickupManager>.instance.DropGold(isKillGold: false, isGivenByOtherPlayer: false, DewMath.RandomRoundToInt(GetValue(goldPerKillAmount)), obj.victim.agentPosition, victim as Hero);
			FxPlayNewNetworked(fxKill, obj.victim);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			if ((UnityEngine.Object)(object)victim != null)
			{
				victim.takenDamageProcessor.Remove(Processor);
				victim.ActorEvent_OnDealDamage -= new Action<EventInfoDamage>(ActorEventOnDealDamage);
			}
			if (victim is Hero hero)
			{
				hero.ClientHeroEvent_OnKillOrAssist -= new Action<EventInfoKill>(ClientHeroEventOnKillOrAssist);
			}
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		_hitEntities.Clear();
	}

	private void Processor(ref DamageData data, Actor from, Entity to)
	{
		if (!data.IsAmountModifiedBy(this))
		{
			Entity entity = from.firstEntity;
			if (!entity.IsNullOrInactive() && victim.CheckEnemyOrNeutral(entity))
			{
				data.SetAmountModifiedBy(this);
				data.ApplyAmplification(GetValue(takenDamageAmp));
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
