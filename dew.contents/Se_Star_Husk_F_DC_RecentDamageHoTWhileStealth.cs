using System;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;

public class Se_Star_Husk_F_DC_RecentDamageHoTWhileStealth : StarEffect
{
	public float recentDamageTime = 4f;

	public int healTicks = 12;

	public float healTickInterval = 0.25f;

	public float trackerCleanupInterval = 2f;

	private float _lastTrackerCleanupTime;

	private List<(float, float)> _takeDmgTracker = new List<(float, float)>();

	private readonly List<(Actor target, Action<Actor> onDestroyed)> _deceptionDestroySubscriptions = new List<(Actor, Action<Actor>)>();

	public override Type heroType => typeof(Hero_Husk);

	public override Type skillType => typeof(St_R_Deception);

	public float healOverTime => (float)healTicks * healTickInterval;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			victim.EntityEvent_OnTakeDamage += new Action<EventInfoDamage>(EntityEventOnTakeDamage);
			victim.EntityEvent_OnCastCompleteBeforePrepare += new Action<EventInfoCast>(EntityEventOnCastCompleteBeforePrepare);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		foreach (var deceptionDestroySubscription in _deceptionDestroySubscriptions)
		{
			if ((UnityEngine.Object)(object)deceptionDestroySubscription.target != null)
			{
				deceptionDestroySubscription.target.ClientActorEvent_OnDestroyed -= deceptionDestroySubscription.onDestroyed;
			}
		}
		_deceptionDestroySubscriptions.Clear();
		if ((bool)(UnityEngine.Object)(object)victim)
		{
			victim.EntityEvent_OnTakeDamage -= new Action<EventInfoDamage>(EntityEventOnTakeDamage);
			victim.EntityEvent_OnCastCompleteBeforePrepare -= new Action<EventInfoCast>(EntityEventOnCastCompleteBeforePrepare);
		}
	}

	private void EntityEventOnTakeDamage(EventInfoDamage obj)
	{
		_takeDmgTracker.Add((Time.time, obj.damage.amount));
	}

	private void EntityEventOnCastCompleteBeforePrepare(EventInfoCast obj)
	{
		if (!(obj.instance is Se_R_Deception))
		{
			return;
		}
		CleanupDamageTracker();
		float totalTakeDmg = _takeDmgTracker.Sum(((float, float) takeDmg) => takeDmg.Item2);
		_takeDmgTracker.Clear();
		if (totalTakeDmg <= 0f)
		{
			return;
		}
		Se_GenericHealOverTime newSe = obj.instance.CreateStatusEffect(victim, new CastInfo(victim), (Se_GenericHealOverTime se) =>
		{
			se.totalAmount = totalTakeDmg;
			se.ticks = healTicks;
			se.tickInterval = healTickInterval;
		});
		AbilityInstance instance = obj.instance;
		Action<Actor> action = (Actor _) =>
		{
			if (!newSe.IsNullOrInactive())
			{
				newSe.Destroy();
			}
		};
		instance.ClientActorEvent_OnDestroyed += action;
		_deceptionDestroySubscriptions.Add((instance, action));
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (((NetworkBehaviour)this).isServer && !(Time.time - _lastTrackerCleanupTime < trackerCleanupInterval))
		{
			_lastTrackerCleanupTime = Time.time;
			CleanupDamageTracker();
		}
	}

	private void CleanupDamageTracker()
	{
		int num = 0;
		float time = Time.time;
		for (int i = 0; i < _takeDmgTracker.Count && time - _takeDmgTracker[i].Item1 > recentDamageTime; i++)
		{
			num++;
		}
		if (num > 0)
		{
			_takeDmgTracker.RemoveRange(0, num);
		}
	}

	private void MirrorProcessed()
	{
	}
}
