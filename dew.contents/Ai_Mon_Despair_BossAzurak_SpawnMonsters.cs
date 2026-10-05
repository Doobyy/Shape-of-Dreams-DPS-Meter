using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Ai_Mon_Despair_BossAzurak_SpawnMonsters : AbilityInstance
{
	public int spawnCount;

	public float spawnInterval;

	public Vector2 spawnRadius;

	private List<Monster> _spawned;

	protected override IEnumerator OnCreateSequenced()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		DestroyOnDeath(info.caster);
		_spawned = new List<Monster>();
		for (int i = 0; i < spawnCount; i++)
		{
			Mon_Despair_AzurakMonsterSpawner item = Dew.SpawnEntity(Dew.GetPositionOnGround(SingletonBehaviour<Room_BossArena>.instance.center + UnityEngine.Random.insideUnitCircle.ToXZ().normalized * UnityEngine.Random.Range(spawnRadius.x, spawnRadius.y)), Quaternion.Euler(0f, UnityEngine.Random.Range(0, 360), 0f), null, DewPlayer.creep, info.caster.level, (Mon_Despair_AzurakMonsterSpawner b) =>
			{
				b.EntityEvent_OnDeath += (Action<EventInfoKill>)((EventInfoKill _) =>
				{
					if (((NetworkBehaviour)this).isServer)
					{
						_spawned.Remove(b);
					}
				});
			});
			_spawned.Add(item);
			yield return new SI.WaitForSeconds(spawnInterval);
		}
		if (_spawned == null)
		{
			Destroy();
			yield break;
		}
		yield return new SI.WaitForCondition(() => _spawned.Count <= 0);
		Destroy();
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			_spawned.Clear();
		}
	}

	private void MirrorProcessed()
	{
	}
}
