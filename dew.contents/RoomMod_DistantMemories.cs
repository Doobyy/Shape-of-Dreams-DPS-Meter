using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;

public class RoomMod_DistantMemories : RoomModifierBase
{
	public float spawnDensity;

	public float delay;

	public GameObject fxEnd;

	private PropEnt_Stone_Nightmare _prop;

	private Rift_RoomExit _portal;

	protected override void OnCreate()
	{
		base.OnCreate();
		ManagerBase<MusicManager>.instance.Stop();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		SingletonDewNetworkBehaviour<Room>.instance.rewards.DisableRegularRewards();
		SingletonDewNetworkBehaviour<Room>.instance.openRoomExitOnClear = false;
		Dew.CallDelayed(() =>
		{
			SingletonDewNetworkBehaviour<Room>.instance.RemoveObstacles();
			SingletonDewNetworkBehaviour<Room>.instance.RemoveCombat(clearRoomOnEnteringLastSection: false);
		});
		_portal = Rift_RoomExit.instance;
		Vector3 goodWanderPosition = SingletonDewNetworkBehaviour<Room>.instance.GetFinalSection().GetGoodWanderPosition(((Component)(object)_portal).transform.position);
		_prop = SpawnEntity<PropEnt_Stone_Nightmare>(goodWanderPosition, null, DewPlayer.environment, 1);
		_prop.EntityEvent_OnDeath += new Action<EventInfoKill>(OnPropKilled);
		ModifyEntities((Entity e) =>
		{
			if (e is Monster)
			{
				e.CreateStatusEffect<Se_DistantMemories>(e, new CastInfo(e));
			}
		}, (Entity e) =>
		{
			if (e.Status.TryGetStatusEffect<Se_DistantMemories>(out var effect))
			{
				effect.Destroy();
			}
		});
		List<MonsterPool.SpawnRuleEntry> filteredEntries = SingletonDewNetworkBehaviour<Room>.instance.monsters.defaultRule.pool.GetFilteredEntries();
		int count = filteredEntries.Count;
		foreach (RoomSection section in SingletonDewNetworkBehaviour<Room>.instance.sections)
		{
			int num = Mathf.RoundToInt(section.area / spawnDensity);
			for (int num2 = 0; num2 < num; num2++)
			{
				Monster asset = filteredEntries[UnityEngine.Random.Range(0, count)].monster.asset;
				Vector3 anyRandomNode = section.GetAnyRandomNode();
				Monster monster = SpawnEntity(asset, anyRandomNode, null, DewPlayer.environment, NetworkedManagerBase<GameManager>.instance.ambientLevel, (Monster b) =>
				{
					b.Visual.skipSpawning = true;
				});
				monster.Sound.voiceStart = null;
				monster.Sound.voiceIdle = null;
			}
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && !((UnityEngine.Object)(object)_prop == null))
		{
			_prop.EntityEvent_OnDeath -= new Action<EventInfoKill>(OnPropKilled);
		}
	}

	private void OnPropKilled(EventInfoKill obj)
	{
		Vector3 deathPos = obj.victim.agentPosition;
		((MonoBehaviour)(object)this).StartCoroutine(Routine());
		IEnumerator Routine()
		{
			FxPlayNetworked(fxEnd, deathPos, null);
			yield return new WaitForSeconds(1f);
			Entity[] array = NetworkedManagerBase<ActorManager>.instance.allEntities.ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i] is Monster monster && !monster.IsNullInactiveDeadOrKnockedOut())
				{
					monster.Destroy();
				}
			}
			SingletonDewNetworkBehaviour<Room>.instance.ClearRoom();
			RemoveModifier();
			ManagerBase<MusicManager>.instance.Play(SingletonDewNetworkBehaviour<Room>.instance.music);
			yield return new WaitForSeconds(delay);
			_portal.Open();
			CreateActor<Shrine_CorruptedChaos>(deathPos, null);
		}
	}

	private void MirrorProcessed()
	{
	}
}
