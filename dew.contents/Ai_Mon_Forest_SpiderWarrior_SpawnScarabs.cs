using System;
using System.Collections;
using Mirror;
using UnityEngine;

public class Ai_Mon_Forest_SpiderWarrior_SpawnScarabs : AbilityInstance
{
	public int numberOfScarabs;

	public float spawnInterval;

	public float radius;

	[NonSerialized]
	public Monster spawnedMonster;

	protected override IEnumerator OnCreateSequenced()
	{
		if (((NetworkBehaviour)this).isServer)
		{
			DestroyOnDeath(info.caster);
			yield return new SI.WaitForSeconds(spawnInterval);
			for (int i = 0; i < numberOfScarabs; i++)
			{
				Vector3 vector = info.caster.position + UnityEngine.Random.onUnitSphere * radius;
				vector = Dew.GetPositionOnGround(vector);
				vector = Dew.GetValidAgentDestination_LinearSweep(info.caster.agentPosition, vector);
				Dew.SpawnEntity(spawnedMonster, vector, Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f), info.caster, info.caster.owner, info.caster.level);
				yield return new SI.WaitForSeconds(spawnInterval);
			}
			Destroy();
		}
	}

	private void MirrorProcessed()
	{
	}
}
