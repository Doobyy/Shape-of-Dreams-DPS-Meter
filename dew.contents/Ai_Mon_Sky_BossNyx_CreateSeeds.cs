using System.Collections;
using Mirror;
using UnityEngine;

public class Ai_Mon_Sky_BossNyx_CreateSeeds : AbilityInstance
{
	public GameObject spawnEffectOnCaster;

	public GameObject spawnEffectOnSpawnPos;

	public float spawnCount = 3f;

	public float spawnDistance = 6f;

	public float spawnInterval;

	public float countPerExtraPlayer = 1.5f;

	protected override IEnumerator OnCreateSequenced()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		int count = Mathf.RoundToInt(spawnCount + countPerExtraPlayer * NetworkedManagerBase<GameManager>.instance.GetMultiplayerDifficultyFactor(reduceWhenDead: true));
		info.caster.Control.StartChannel(new Channel
		{
			duration = spawnInterval * (float)count + 1f,
			onCancel = () =>
			{
				if (isActive)
				{
					Destroy();
				}
			}
		});
		DestroyOnDeath(info.caster);
		float spawnAngle = Random.Range(0f, 360f);
		for (int i = 0; i < count; i++)
		{
			spawnAngle += 360f / spawnCount;
			Vector3 vector = info.caster.position + Quaternion.Euler(0f, spawnAngle, 0f) * Vector3.forward * spawnDistance;
			Quaternion value = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
			Dew.SpawnEntity(vector, value, info.caster, DewPlayer.creep, info.caster.level, (Mon_Sky_StarSeed s) =>
			{
				s.canSelfDestruct = true;
			});
			FxPlayNetworked(spawnEffectOnCaster, info.caster);
			FxPlayNewNetworked(spawnEffectOnSpawnPos, vector, value);
			yield return new SI.WaitForSeconds(spawnInterval);
		}
		Destroy();
	}

	private void MirrorProcessed()
	{
	}
}
