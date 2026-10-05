using UnityEngine;

public class RoomMod_GlacialBarrier : RoomModifierBase
{
	public Vector2 spawnCountBeforeStart;

	public float firstDelay;

	public float spawnDelay;

	public Vector2 perHeroSpawnDelay;

	public Vector2 nextSpawnDelay;

	public int maxSpawnCount;

	private int _spawnedCount;

	public override void OnStartServer()
	{
	}

	protected override void OnDestroyActor()
	{
	}

	private void MirrorProcessed()
	{
	}
}
