using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Ai_Q_SylvanCall : InstantDamageInstance
{
	public float stunDuration = 0.4f;

	public int maxCount = 5;

	private readonly List<Summon> _summonedHounds = new List<Summon>();

	private readonly List<(Entity ent, Action<EventInfoKill> h)> _deathSubscriptions = new List<(Entity, Action<EventInfoKill>)>();

	private int _baseMaxCount;

	public override bool reuseInRoom => true;

	protected override void Awake()
	{
		base.Awake();
		_baseMaxCount = maxCount;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		maxCount = _baseMaxCount;
	}

	protected override void OnPrepare()
	{
		base.OnPrepare();
		CastInfo castInfo = info;
		castInfo.point = Dew.GetValidAgentDestination_Closest(info.caster.agentPosition, castInfo.point);
		info = castInfo;
	}

	protected override void OnCreate()
	{
		((Component)(object)this).transform.position = info.point;
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		if (info.caster is Hero hero)
		{
			_summonedHounds.Clear();
			foreach (Summon summon2 in hero.summons)
			{
				if (summon2 is Sum_Q_SylvanCall_LeafHound item)
				{
					_summonedHounds.Add(item);
				}
			}
			while (_summonedHounds.Count >= maxCount)
			{
				Summon summon = Dew.SelectBestWithScore((IList<Summon>)_summonedHounds, (Func<Summon, int, float>)((Summon s, int _) => 0f - s.creationTime), 0f, (DewRandom)null);
				_summonedHounds.Remove(summon);
				if ((UnityEngine.Object)(object)summon != null)
				{
					summon.Kill();
				}
			}
		}
		SpawnSummon(info.point, Quaternion.Euler(0f, UnityEngine.Random.Range(0, 360), 0f), (Sum_Q_SylvanCall_LeafHound b) =>
		{
			Action<EventInfoKill> action = (EventInfoKill kill) =>
			{
				_summonedHounds.Remove(b);
			};
			b.EntityEvent_OnDeath += action;
			_deathSubscriptions.Add((b, action));
		});
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		for (int i = 0; i < _deathSubscriptions.Count; i++)
		{
			(Entity, Action<EventInfoKill>) tuple = _deathSubscriptions[i];
			if ((UnityEngine.Object)(object)tuple.Item1 != null)
			{
				tuple.Item1.EntityEvent_OnDeath -= tuple.Item2;
			}
		}
		_deathSubscriptions.Clear();
		_summonedHounds.Clear();
	}

	protected override void OnHit(Entity entity)
	{
		base.OnHit(entity);
		CreateBasicEffect(entity, new StunEffect(), stunDuration);
	}

	private void MirrorProcessed()
	{
	}
}
