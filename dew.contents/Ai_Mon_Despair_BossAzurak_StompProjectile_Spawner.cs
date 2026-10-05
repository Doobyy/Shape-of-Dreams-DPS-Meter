using System.Collections;
using Mirror;
using UnityEngine;

public class Ai_Mon_Despair_BossAzurak_StompProjectile_Spawner : AbilityInstance
{
	public int spawnCount;

	public float spawnInterval;

	public float spawnRadius;

	public float minSpawnDistance;

	protected override IEnumerator OnCreateSequenced()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		DestroyOnDeath(info.caster);
		int count = spawnCount;
		if (info.caster.normalizedHealth < 0.5f)
		{
			count += 2;
		}
		for (int i = 0; i < count; i++)
		{
			Vector3 end = info.point;
			if (i != 0)
			{
				end = info.point + Random.insideUnitCircle.ToXZ().normalized * Random.Range(minSpawnDistance, spawnRadius);
			}
			end = Dew.GetValidAgentDestination_Closest(info.point, end);
			CreateAbilityInstance<Ai_Mon_Despair_BossAzurak_StompProjectile_FirstAtk>(end, null, new CastInfo(info.caster, end));
			yield return new SI.WaitForSeconds(spawnInterval);
		}
		Destroy();
	}

	private void MirrorProcessed()
	{
	}
}
