using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Ai_Mon_Special_BossErebos_AntiGravity_Spawner : AbilityInstance
{
	public int spawnCount;

	public float spawnInterval;

	public float startDelay;

	public float minCastDuration;

	public float postDelay;

	public float spawnRangeRadius;

	public float minCloseSpawnDistance;

	public GameObject fxCastStart;

	public GameObject fxCastEnd;

	public DewAnimationClip castClip;

	public DewAnimationClip endClip;

	private Channel _channel;

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
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		DestroyOnDeath(info.caster);
		spawnCount += Dew.GetAliveHeroCount();
		CreateBasicEffect(info.caster, new UnstoppableEffect(), float.PositiveInfinity).DestroyOnDestroy(this);
		_channel = info.caster.Control.StartChannel(new Channel
		{
			blockedActions = Channel.BlockedAction.Everything,
			duration = float.PositiveInfinity,
			isAttack = false,
			onCancel = DestroyIfActive
		});
		BossMonster.RevealStealthedBeforeSpecialAttack();
		yield return new SI.WaitForSeconds(startDelay);
		info.caster.Animation.PlayAbilityAnimation(castClip);
		FxPlayNetworked(fxCastStart, info.caster);
		List<Hero> list = new List<Hero>();
		foreach (Hero allHero in NetworkedManagerBase<ActorManager>.instance.allHeroes)
		{
			if (!allHero.IsNullInactiveDeadOrKnockedOut())
			{
				list.Add(allHero);
			}
		}
		Vector3 center = SingletonBehaviour<Erebos_BossRoomCenter>.instance.transform.position;
		List<Vector2> list2 = new List<Vector2>(list.Count);
		foreach (Hero item in list)
		{
			Vector3 validAgentDestination_Closest = Dew.GetValidAgentDestination_Closest(item.GetAIAgentPosition(info.caster), AbilityTrigger.PredictPoint_Simple(info.caster, 1f, item, minCastDuration));
			validAgentDestination_Closest -= center;
			validAgentDestination_Closest += (Vector2.one * spawnRangeRadius).ToXZ();
			list2.Add(new Vector2(validAgentDestination_Closest.x, validAgentDestination_Closest.y));
		}
		List<Vector2> spawnPoints = GeneratePoints(minCloseSpawnDistance, Vector2.one * (spawnRangeRadius * 2f), list2);
		for (int i = 0; i < spawnCount; i++)
		{
			Vector3 vector = (spawnPoints[i] - Vector2.one * spawnRangeRadius).ToXZ();
			Vector3 point = center + vector;
			CreateAbilityInstance<Ai_Mon_Special_BossErebos_AntiGravity_Instance>(Dew.GetPositionOnGround(vector), null, new CastInfo(info.caster, point));
			yield return new SI.WaitForSeconds(spawnInterval);
		}
		yield return new SI.WaitForSeconds(minCastDuration);
		FxStopNetworked(fxCastStart);
		FxPlayNetworked(fxCastEnd, info.caster);
		info.caster.Animation.PlayAbilityAnimation(endClip);
		yield return new SI.WaitForSeconds(postDelay);
		Destroy();
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			FxStopNetworked(fxCastStart);
			FxStopNetworked(fxCastEnd);
			if (_channel != null && _channel.isAlive)
			{
				_channel.Cancel();
				_channel = null;
			}
		}
	}

	private List<Vector2> GeneratePoints(float radius, Vector2 sampleRegionSize, List<Vector2> initialPoints, int numSamplesBeforeRejection = 30)
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
		List<Vector2> list2 = new List<Vector2>(initialPoints);
		foreach (Vector2 item2 in list2)
		{
			list.Add(item2);
			array[(int)(item2.x / num), (int)(item2.y / num)] = list.Count - 1;
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
			Vector2 vector = list2[index];
			bool flag = false;
			for (int k = 0; k < numSamplesBeforeRejection; k++)
			{
				float f = UnityEngine.Random.value * (float)Math.PI * 2f;
				Vector2 vector2 = new Vector2(Mathf.Sin(f), Mathf.Cos(f));
				float num2 = UnityEngine.Random.Range(radius, 3f * radius);
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
