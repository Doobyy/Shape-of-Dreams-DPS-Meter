using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Ai_Mon_Special_BossErebos_Gaze_Spawner : AbilityInstance
{
	public int spawnCount;

	public float eachFarDistance;

	public float spawnRadius;

	public float interval;

	private int _baseSpawnCount;

	private bool _cachedBaseSpawnCount;

	protected override void Awake()
	{
		base.Awake();
		if (!_cachedBaseSpawnCount)
		{
			_baseSpawnCount = spawnCount;
			_cachedBaseSpawnCount = true;
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if (_cachedBaseSpawnCount)
		{
			spawnCount = _baseSpawnCount;
		}
	}

	protected override IEnumerator OnCreateSequenced()
	{
		if (((NetworkBehaviour)this).isServer)
		{
			DestroyOnDeath(info.caster);
			spawnCount += Mathf.RoundToInt(NetworkedManagerBase<GameManager>.instance.GetSpecialSkillChanceMultiplier());
			Vector3 center = SingletonBehaviour<Erebos_BossRoomCenter>.instance.transform.position;
			List<Vector2> points = GeneratePoints(eachFarDistance, Vector2.one * (spawnRadius * 2f));
			float baseAngle = UnityEngine.Random.Range(0f, 360f);
			int baseAngleValue = 360 / spawnCount;
			for (int i = 0; i < spawnCount; i++)
			{
				Vector3 vector = (points[i] - Vector2.one * spawnRadius).ToXZ();
				vector += center;
				vector = Dew.GetPositionOnGround(vector);
				CreateAbilityInstance<Ai_Mon_Special_BossErebos_Gaze_Instance>(vector, null, new CastInfo(info.caster, baseAngle + UnityEngine.Random.Range((float)baseAngleValue * 0.75f, (float)baseAngleValue * 1.25f) * (float)i));
				yield return new SI.WaitForSeconds(interval);
			}
			Destroy();
		}
	}

	private List<Vector2> GeneratePoints(float radius, Vector2 sampleRegionSize, int numSamplesBeforeRejection = 15)
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
		list2.Add(sampleRegionSize / 2f);
		while (list2.Count > 0)
		{
			int index = UnityEngine.Random.Range(0, list2.Count);
			Vector2 vector = list2[index];
			bool flag = false;
			for (int k = 0; k < numSamplesBeforeRejection; k++)
			{
				float f = UnityEngine.Random.value * (float)Math.PI * 2f;
				Vector2 vector2 = new Vector2(Mathf.Sin(f), Mathf.Cos(f));
				float num2 = UnityEngine.Random.Range(radius, 2f * radius);
				Vector2 vector3 = vector + vector2 * num2;
				if (IsValid(vector3, sampleRegionSize, num, radius, list, array))
				{
					list.Add(vector3);
					list2.Add(vector3);
					array[(int)(vector3.x / num), (int)(vector3.y / num)] = list.Count - 1;
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
