using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Ai_R_SummonLittleBaam : AbilityInstance
{
	public int maxCount = 10;

	private readonly List<Summon> _summonedBaams = new List<Summon>();

	private readonly List<(Entity ent, Action<EventInfoKill> h)> _deathSubscriptions = new List<(Entity, Action<EventInfoKill>)>();

	protected override void OnPrepare()
	{
		base.OnPrepare();
		CastInfo castInfo = info;
		castInfo.point = Dew.GetValidAgentDestination_Closest(info.caster.agentPosition, castInfo.point);
		info = castInfo;
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		if (info.caster is Hero hero)
		{
			_summonedBaams.Clear();
			foreach (Summon summon2 in hero.summons)
			{
				if (summon2 is Sum_R_SummonLittleBaam_Baam item)
				{
					_summonedBaams.Add(item);
				}
			}
			while (_summonedBaams.Count >= maxCount)
			{
				Summon summon = Dew.SelectBestWithScore((IList<Summon>)_summonedBaams, (Func<Summon, int, float>)((Summon s, int _) => 0f - s.creationTime), 0f, (DewRandom)null);
				_summonedBaams.Remove(summon);
				if ((UnityEngine.Object)(object)summon != null)
				{
					summon.Kill();
				}
			}
		}
		SpawnSummon(info.point, Quaternion.Euler(0f, UnityEngine.Random.Range(0, 360), 0f), (Sum_R_SummonLittleBaam_Baam b) =>
		{
			Action<EventInfoKill> action = (EventInfoKill kill) =>
			{
				_summonedBaams.Remove(b);
			};
			b.EntityEvent_OnDeath += action;
			_deathSubscriptions.Add((b, action));
		});
		Destroy();
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
		_summonedBaams.Clear();
	}

	private void MirrorProcessed()
	{
	}
}
