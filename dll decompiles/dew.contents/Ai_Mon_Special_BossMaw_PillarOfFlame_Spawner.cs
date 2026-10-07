using System.Collections;
using Mirror;
using UnityEngine;

public class Ai_Mon_Special_BossMaw_PillarOfFlame_Spawner : AbilityInstance
{
	public int spawnCount;

	public float horizontalDeviation;

	public float eachSpawnedDistance;

	public float spawnInterval;

	protected override IEnumerator OnCreateSequenced()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		DestroyOnDeath(info.caster);
		Vector3 vector = Vector3.Cross(Vector3.up, info.forward).normalized;
		Vector3 vector2 = info.caster.agentPosition + info.forward * (eachSpawnedDistance + 1f);
		int i;
		for (i = 0; i < spawnCount; i++)
		{
			Vector3 point = vector2 + info.forward * eachSpawnedDistance * i;
			if (i > 0)
			{
				point += vector * Random.Range(0f, horizontalDeviation);
				vector = -vector;
			}
			CreateAbilityInstance(info.caster.agentPosition, null, new CastInfo(info.caster, point), (Ai_Mon_Special_BossMaw_PillarOfFlame_Projectile b) =>
			{
				b.duration += spawnInterval * (float)i;
			});
		}
		Destroy();
	}

	private void MirrorProcessed()
	{
	}
}
