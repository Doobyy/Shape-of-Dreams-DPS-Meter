using System.Collections.Generic;
using UnityEngine;

public class Ai_Mon_Special_AmbushSpawner_Projectile : StandardProjectile
{
	public Vector2 spawnDistance;

	private List<MonsterPool.SpawnRuleEntry> _entries;

	private Monster _monster;

	internal int _waveIndex;

	protected override void OnPrepare()
	{
		base.OnPrepare();
		_entries = SingletonDewNetworkBehaviour<Room>.instance.monsters.defaultRule.pool.GetFilteredEntries();
		int num = 10;
		for (int i = 0; i < num; i++)
		{
			_monster = _entries[Random.Range(0, _entries.Count)].monster.asset;
			if (_monster.type != Monster.MonsterType.Lesser)
			{
				break;
			}
		}
		Vector3 end = info.caster.agentPosition + Random.insideUnitCircle.normalized.ToXZ() * Random.Range(spawnDistance.x, spawnDistance.y);
		Vector3 validAgentDestination_LinearSweep = Dew.GetValidAgentDestination_LinearSweep(((Component)(object)this).transform.position, end);
		targetPosition = validAgentDestination_LinearSweep;
	}

	protected override void OnComplete()
	{
		base.OnComplete();
		if (!info.caster.IsNullInactiveDeadOrKnockedOut())
		{
			Monster monster = Dew.SpawnEntity(_monster, targetPosition, Quaternion.Euler(0f, Random.Range(0, 360), 0f), null, DewPlayer.creep, NetworkedManagerBase<GameManager>.instance.ambientLevel, (Monster b) =>
			{
				b.Visual.skipSpawning = true;
			});
			monster.Control.StartDaze(0.7f);
			float maxHealthPercentage = Mathf.Min((float)_waveIndex * 6f * Mathf.Pow(1.03f, _waveIndex), 100000f);
			monster.Status.AddStatBonus(new StatBonus
			{
				maxHealthPercentage = maxHealthPercentage,
				attackDamagePercentage = (float)_waveIndex * 8f,
				abilityPowerFlat = (float)_waveIndex * 8f,
				attackSpeedPercentage = (float)_waveIndex * 8f,
				abilityHasteFlat = (float)_waveIndex * 8f,
				movementSpeedPercentage = Mathf.Min((float)_waveIndex * 4f, 120f)
			});
			Hero closestAliveHero = Dew.GetClosestAliveHero(targetPosition, fallbackToDead: true, monster);
			monster.AI.Aggro(closestAliveHero);
		}
	}

	private void MirrorProcessed()
	{
	}
}
