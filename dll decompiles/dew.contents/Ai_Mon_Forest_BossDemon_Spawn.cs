using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Ai_Mon_Forest_BossDemon_Spawn : AbilityInstance
{
	public int summonBonusLevel;

	public int summonBonusCount;

	public Entity[] entries;

	public float summonInterval;

	public float postDelayAfterSummon;

	public int initDestroyCount = 2;

	public float maxDestroyStallTime;

	private List<Entity> _spawnedEnts;

	private List<(Monster mon, Action<EventInfoKill> handler)> _deathSubscriptions;

	protected override IEnumerator OnCreateSequenced()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		DestroyOnDeath(info.caster);
		int spawnCount = entries.Length + Mathf.RoundToInt((float)(Dew.allHeroes.Count + summonBonusCount) * NetworkedManagerBase<GameManager>.instance.GetSpecialSkillChanceMultiplier());
		float duration = (float)spawnCount * summonInterval + postDelayAfterSummon;
		_spawnedEnts = new List<Entity>(spawnCount);
		_deathSubscriptions = new List<(Monster, Action<EventInfoKill>)>(spawnCount);
		info.caster.Control.StartDaze(duration);
		for (int i = 0; i < spawnCount; i++)
		{
			Entity entity = entries[i % entries.Length];
			RoomSection roomSection = info.caster.section ?? info.caster.lastSection;
			Vector3 vector = ((roomSection != null) ? roomSection.GetAnyRandomNode() : info.caster.position);
			Hero closestAliveHero = Dew.GetClosestAliveHero(vector, fallbackToDead: true, info.caster);
			Vector3 forward = (((UnityEngine.Object)(object)closestAliveHero != null) ? (closestAliveHero.GetAIAgentPosition(info.caster) - vector) : ((Component)(object)info.caster).transform.forward);
			Entity entity2 = Dew.SpawnEntity(entity, vector, Quaternion.LookRotation(forward), info.caster, info.caster.owner, NetworkedManagerBase<GameManager>.instance.ambientLevel + summonBonusLevel);
			Monster mon = entity2 as Monster;
			if (mon != null)
			{
				mon.disableLoot = true;
				_spawnedEnts.Add(mon);
				Action<EventInfoKill> action = (EventInfoKill _) =>
				{
					_spawnedEnts.Remove(mon);
					if (_spawnedEnts.Count <= initDestroyCount)
					{
						DestroyIfActive();
					}
				};
				mon.EntityEvent_OnDeath += action;
				_deathSubscriptions.Add((mon, action));
			}
			yield return new SI.WaitForSeconds(summonInterval);
		}
		yield return new SI.WaitForSeconds(maxDestroyStallTime);
		DestroyIfActive();
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (_deathSubscriptions == null)
		{
			return;
		}
		foreach (var deathSubscription in _deathSubscriptions)
		{
			if ((UnityEngine.Object)(object)deathSubscription.mon != null)
			{
				deathSubscription.mon.EntityEvent_OnDeath -= deathSubscription.handler;
			}
		}
		_deathSubscriptions.Clear();
	}

	private void MirrorProcessed()
	{
	}
}
