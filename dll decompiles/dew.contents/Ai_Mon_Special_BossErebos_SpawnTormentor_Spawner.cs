using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Ai_Mon_Special_BossErebos_SpawnTormentor_Spawner : AbilityInstance
{
	public int spawnCount;

	public float spawnInterval;

	public GameObject fxSpawn;

	public GameObject fxStartSlow;

	public GameObject fxStartAttacker;

	internal bool isSlowSpawn;

	private GameObject _prefab;

	protected override IEnumerator OnCreateSequenced()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		DestroyOnDeath(info.caster);
		if (isSlowSpawn)
		{
			FxPlayNetworked(fxStartSlow, info.caster);
		}
		else
		{
			FxPlayNetworked(fxStartAttacker, info.caster);
		}
		List<Vector2> points = GeneratePoints(7f, Vector2.one * 35f);
		for (int i = 0; i < spawnCount; i++)
		{
			Vector3 vector = points[i].ToXZ() - (Vector2.one * 17.5f).ToXZ();
			Vector3 vector2 = SingletonBehaviour<Erebos_BossRoomCenter>.instance.transform.position;
			vector2 += vector;
			FxPlayNewNetworked(fxSpawn, vector2, Quaternion.identity);
			if (isSlowSpawn)
			{
				CreateAbilityInstance<Ai_Mon_Special_BossErebos_SpawnTormentor_Slow>(vector2, Quaternion.identity, new CastInfo(info.caster));
			}
			else
			{
				CreateAbilityInstance<Ai_Mon_Special_BossErebos_SpawnTormentor_Attacker>(vector2, Quaternion.identity, new CastInfo(info.caster));
			}
			yield return new SI.WaitForSeconds(spawnInterval);
		}
		Destroy();
	}

	private List<Vector2> GeneratePoints(float radius, Vector2 sampleRegionSize, int numSamplesBeforeRejection = 30)
	{
		float num = radius / Mathf.Sqrt(2f);
		int[,] array = new int[Mathf.CeilToInt(sampleRegionSize.x / num), Mathf.CeilToInt(sampleRegionSize.y / num)];
		for (int i = 0; i < array.GetLength(1); i++)
		{
			for (int j = 0; j < array.GetLength(0); j++)
			{
				array[j, i] = -1;
			}
		}
		List<Vector2> list = new List<Vector2>();
		List<Vector2> list2 = new List<Vector2>();
		int num2 = 5;
		for (int k = 0; k < num2; k++)
		{
			Vector2 vector = new Vector2(UnityEngine.Random.Range(0f, sampleRegionSize.x), UnityEngine.Random.Range(0f, sampleRegionSize.y));
			if (IsValid(vector, sampleRegionSize, num, radius, list, array))
			{
				list.Add(vector);
				list2.Add(vector);
				array[(int)(vector.x / num), (int)(vector.y / num)] = list.Count - 1;
			}
		}
		if (list2.Count == 0)
		{
			Vector2 item = new Vector2(UnityEngine.Random.Range(0f, sampleRegionSize.x), UnityEngine.Random.Range(0f, sampleRegionSize.y));
			list.Add(item);
			list2.Add(item);
			array[(int)(item.x / num), (int)(item.y / num)] = 0;
		}
		while (list2.Count > 0)
		{
			int index = UnityEngine.Random.Range(0, list2.Count);
			Vector2 vector2 = list2[index];
			bool flag = false;
			for (int l = 0; l < numSamplesBeforeRejection; l++)
			{
				float f = UnityEngine.Random.value * (float)Math.PI * 2f;
				Vector2 vector3 = new Vector2(Mathf.Sin(f), Mathf.Cos(f));
				float num3 = UnityEngine.Random.Range(radius, 3f * radius);
				Vector2 vector4 = vector2 + vector3 * num3;
				if (IsValid(vector4, sampleRegionSize, num, radius, list, array))
				{
					list.Add(vector4);
					list2.Add(vector4);
					array[(int)(vector4.x / num), (int)(vector4.y / num)] = list.Count - 1;
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				list2.RemoveAt(index);
			}
		}
		return list;
	}

	private static bool IsValid(Vector2 candidate, Vector2 sampleRegionSize, float cellSize, float radius, List<Vector2> points, int[,] grid)
	{
		if (candidate.x >= 0f && candidate.x < sampleRegionSize.x && candidate.y >= 0f && candidate.y < sampleRegionSize.y)
		{
			int num = (int)(candidate.x / cellSize);
			int num2 = (int)(candidate.y / cellSize);
			int num3 = Mathf.Max(0, num - 2);
			int num4 = Mathf.Min(num + 2, grid.GetLength(0) - 1);
			int num5 = Mathf.Max(0, num2 - 2);
			int num6 = Mathf.Min(num2 + 2, grid.GetLength(1) - 1);
			for (int i = num5; i <= num6; i++)
			{
				for (int j = num3; j <= num4; j++)
				{
					int num7 = grid[j, i];
					if (num7 != -1 && (candidate - points[num7]).sqrMagnitude < radius * radius)
					{
						return false;
					}
				}
			}
			return true;
		}
		return false;
	}

	private void MirrorProcessed()
	{
	}
}
