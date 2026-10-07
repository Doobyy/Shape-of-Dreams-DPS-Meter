using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Ai_Mon_Special_BossPolaris_Holy_Summon : AbilityInstance
{
	[Serializable]
	public struct PoolEntry
	{
		public List<AssetRef<Monster>> monsters;
	}

	public float dispDuration = 1.5f;

	public float endDaze = 1.5f;

	public GameObject fxRift;

	public List<PoolEntry> spawnPool;

	protected override IEnumerator OnCreateSequenced()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		_ = (Mon_Special_BossPolaris)info.caster;
		CreateBasicEffect(info.caster, new UnstoppableEffect(), 3600f).DestroyOnDestroy(this);
		Channel channel = info.caster.Control.StartChannel(new Channel
		{
			blockedActions = Channel.BlockedAction.Everything,
			duration = 3600f,
			onCancel = DestroyIfActive
		});
		Vector3 center = SingletonBehaviour<Room_BossArena>.instance.center;
		info.caster.Control.Rotate(ManagerBase<CameraManager>.instance.entityCamAngle + 180f, immediately: false, 1f);
		info.caster.Control.StartDisplacement(new DispByDestination
		{
			destination = center + ManagerBase<CameraManager>.instance.entityCamAngleRotation * Vector3.forward * 1.5f,
			duration = dispDuration,
			ease = DewEase.EaseInOutQuad,
			canGoOverTerrain = true,
			rotateForward = false,
			isFriendly = true
		});
		yield return new SI.WaitForSeconds(dispDuration);
		FxPlayNetworked(fxRift, info.caster);
		yield return new SI.WaitForSeconds(1f);
		PoolEntry pool = spawnPool[UnityEngine.Random.Range(0, spawnPool.Count)];
		Vector3 spawnCenter = center + ManagerBase<CameraManager>.instance.entityCamAngleRotation * Vector3.back * 1.5f;
		int currentSpawnIteration = 0;
		float spawnAngle = ManagerBase<CameraManager>.instance.entityCamAngle + 180f;
		float spawnInterval = 0.1f;
		if (!channel.isAlive)
		{
			yield break;
		}
		Spawn(pool.monsters[0].asset, 1);
		yield return new SI.WaitForSeconds(spawnInterval);
		if (!channel.isAlive)
		{
			yield break;
		}
		Spawn(pool.monsters[0].asset, 1);
		yield return new SI.WaitForSeconds(spawnInterval);
		for (int i = 0; i < 4; i++)
		{
			if (channel.isAlive)
			{
				Spawn(pool.monsters[UnityEngine.Random.Range(1, pool.monsters.Count)].asset, 1);
				yield return new SI.WaitForSeconds(spawnInterval);
				continue;
			}
			yield break;
		}
		FxStopNetworked(fxRift);
		info.caster.Control.StartDaze(endDaze);
		info.caster.Control.StartChannel(new Channel
		{
			duration = 10f,
			blockedActions = Channel.BlockedAction.Ability
		});
		Destroy();
		if (channel.isAlive)
		{
			channel.Cancel();
		}
		Vector3 GetSpawnPos()
		{
			currentSpawnIteration++;
			if (currentSpawnIteration <= 1)
			{
				return spawnCenter;
			}
			return spawnCenter + ((currentSpawnIteration % 2 == 0) ? Vector3.left : Vector3.right) * ((float)(currentSpawnIteration / 2) * 2f) + UnityEngine.Random.insideUnitSphere.Flattened() * 1.5f;
		}
		Quaternion GetSpawnRot()
		{
			return Quaternion.Euler(0f, spawnAngle + UnityEngine.Random.Range(-30f, 30f), 0f);
		}
		void Spawn(Monster m, int count)
		{
			int num = DewMath.RandomRoundToInt(NetworkedManagerBase<GameManager>.instance.GetAdjustedMonsterSpawnPopulation(count, ignoreTurnMultiplier: true));
			for (int j = 0; j < num; j++)
			{
				Monster monster = Dew.SpawnEntity(m, GetSpawnPos(), GetSpawnRot(), null, DewPlayer.creep, 1, (Monster monster2) =>
				{
					monster2.Visual.skipSpawning = true;
				});
				monster.Control.CancelOngoingChannels();
				monster.Control.StartDaze(0.5f);
				CreateStatusEffect(monster, (Se_PortalTransition se) =>
				{
					se.playDisappearEffect = false;
				}).SetTimer(0.1f);
			}
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			FxStopNetworked(fxRift);
		}
	}

	private void MirrorProcessed()
	{
	}
}
