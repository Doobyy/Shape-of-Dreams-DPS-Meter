using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.AI;

public class Mon_Despair_AzurakMonsterSpawner : Monster
{
	public int maxSpawnCount;

	public float maxSpawnInterval;

	public Vector2 spawnRadius;

	public GameObject fxSpawn;

	public NavMeshObstacle obstacle;

	public List<MonsterPool.SpawnRuleEntry> entries;

	private List<Monster> _spawned = new List<Monster>();

	private float _lasSpawnTime;

	private float _spawnInterval;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			CreateBasicEffect(this, new UnstoppableEffect(), float.PositiveInfinity, "spawner_unstoppable");
			((Behaviour)(object)obstacle).enabled = true;
			_lasSpawnTime = creationTime;
			_spawnInterval = UnityEngine.Random.Range(1.5f, maxSpawnInterval);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		((Behaviour)(object)obstacle).enabled = false;
		if (((NetworkBehaviour)this).isServer)
		{
			_spawned.Clear();
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (!((NetworkBehaviour)this).isServer || _spawned.Count >= maxSpawnCount || Time.time - _lasSpawnTime <= _spawnInterval)
		{
			return;
		}
		_lasSpawnTime = Time.time;
		_spawnInterval = UnityEngine.Random.Range(1.5f, maxSpawnInterval);
		FxPlayNewNetworked(fxSpawn, this);
		Vector3 end = agentPosition + UnityEngine.Random.insideUnitCircle.ToXZ().normalized * UnityEngine.Random.Range(spawnRadius.x, spawnRadius.y);
		end = Dew.GetValidAgentDestination_Closest(agentPosition, end);
		Monster monster = Dew.SpawnEntity(entries[UnityEngine.Random.Range(0, entries.Count)].monster.asset, end, Quaternion.Euler(0f, UnityEngine.Random.Range(0, 360), 0f), null, DewPlayer.creep, level, (Monster b) =>
		{
			b.EntityEvent_OnDeath += (Action<EventInfoKill>)((EventInfoKill _) =>
			{
				if (((NetworkBehaviour)this).isServer)
				{
					_spawned.Remove(b);
				}
			});
			b.disableLoot = true;
		});
		monster.Control.StartDaze(0.65f);
		monster.AI.Aggro(Dew.GetClosestAliveHero(monster.agentPosition, fallbackToDead: true, monster));
		_spawned.Add(monster);
	}

	private void MirrorProcessed()
	{
	}
}
