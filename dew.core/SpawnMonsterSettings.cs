using System;
using UnityEngine;

public class SpawnMonsterSettings
{
	public Hero hero;

	public RoomSection section;

	public MonsterSpawnRule rule;

	public RoomMonsters.MonsterSpawnData monsterSpawnData = new RoomMonsters.MonsterSpawnData();

	public Action<Entity> beforeSpawn;

	public Action<Entity> afterSpawn;

	public float spawnPopulationMultiplier = 1f;

	public float initDelayFlat;

	public float initDelayMultiplier = 1f;

	public Func<Vector3> spawnPosGetter;

	public Func<Quaternion> spawnRotGetter;

	public Action onFinish;

	public Func<bool> earlyFinishCondition;

	public bool ignoreTurnPopMultiplier;

	public bool ignoreCoopPopMultiplier;

	public bool invalidatePooledInstanceOnUse;

	public DewRandom random;

	public DewRandom positionRandom;

	internal bool isCutsceneSkipped;

	public SpawnMonsterSettings Clone()
	{
		return (SpawnMonsterSettings)MemberwiseClone();
	}
}
