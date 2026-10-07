using System.Collections.Generic;
using System.Linq;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class RoomMod_PureDream : RoomModifierBase, IPrewarmRoomContributor
{
	public Vector2Int dreamProps;

	public Vector2Int dreamStatues;

	public Material matDreamDust;

	private List<(int, int)> _indices;

	public void ContributeMonsterPrewarm(Dictionary<Monster, int> counts)
	{
		if ((Object)(object)SingletonDewNetworkBehaviour<Room>.instance == null)
		{
			return;
		}
		MonsterSpawnRule defaultRule = SingletonDewNetworkBehaviour<Room>.instance.monsters.defaultRule;
		if (defaultRule == null || defaultRule.pool == null)
		{
			return;
		}
		List<MonsterPool.SpawnRuleEntry> filteredEntries = defaultRule.pool.GetFilteredEntries();
		if (filteredEntries == null || filteredEntries.Count == 0)
		{
			return;
		}
		List<MonsterPool.SpawnRuleEntry> list = filteredEntries.Where((MonsterPool.SpawnRuleEntry e) =>
		{
			Monster asset2 = e.monster.asset;
			return asset2 != null && !(asset2 is Mon_Sky_StarSeed);
		}).ToList();
		if (list.Count == 0)
		{
			return;
		}
		int y = dreamStatues.y;
		if (y <= 0)
		{
			return;
		}
		int num = Mathf.CeilToInt((float)y / (float)list.Count);
		foreach (MonsterPool.SpawnRuleEntry item in list)
		{
			Monster asset = item.monster.asset;
			if (asset != null)
			{
				counts.TryGetValue(asset, out var value);
				counts[asset] = value + num;
			}
		}
	}

	public override void OnStartServer()
	{
		base.OnStartServer();
		if (!isNewInstance)
		{
			return;
		}
		SingletonDewNetworkBehaviour<Room>.instance.rewards.DisableRegularRewards();
		_indices = new List<(int, int)>(SingletonDewNetworkBehaviour<Room>.instance.map.mapData.innerPropNodeIndices);
		int num = Random.Range(dreamProps.x, dreamProps.y + 1);
		for (int i = 0; i < num; i++)
		{
			Vector3 pos = CalSpawnPosition();
			SpawnEntity<PropEnt_Stone_DreamDust>(pos, Quaternion.Euler(0f, Random.Range(0f, 360f), 0f), DewPlayer.environment, 1);
		}
		ModifyEntities((Entity e) =>
		{
			if (e is Monster)
			{
				e.CreateStatusEffect<Se_PureDream>(e, new CastInfo(e));
			}
		}, (Entity e) =>
		{
			if (e.Status.TryGetStatusEffect<Se_PureDream>(out var effect))
			{
				effect.Destroy();
			}
		});
		GameManager.CallOnReady(() =>
		{
			Routine();
		});
	}

	private Vector3 CalSpawnPosition()
	{
		if (_indices.Count == 0)
		{
			return SingletonDewNetworkBehaviour<Room>.instance.sections[Random.Range(0, SingletonDewNetworkBehaviour<Room>.instance.sections.Count)].GetAnyRandomNode();
		}
		int index = Random.Range(0, _indices.Count);
		Vector3 positionOnGround = Dew.GetPositionOnGround(SingletonDewNetworkBehaviour<Room>.instance.map.mapData.cells.GetWorldPos(_indices[index]).ToXZ() + Vector3.up * 100f, 200f);
		Vector3 positionOnGround2 = Dew.GetPositionOnGround(positionOnGround + Random.insideUnitSphere * 1f);
		positionOnGround2 = Dew.GetValidAgentDestination_LinearSweep(positionOnGround, positionOnGround2);
		_indices.RemoveAt(index);
		return positionOnGround2;
	}

	private void Routine()
	{
		List<MonsterPool.SpawnRuleEntry> filteredEntries = SingletonDewNetworkBehaviour<Room>.instance.monsters.defaultRule.pool.GetFilteredEntries();
		int count = filteredEntries.Count;
		List<MonsterPool.SpawnRuleEntry> list = new List<MonsterPool.SpawnRuleEntry>();
		for (int i = 0; i < count; i++)
		{
			if (!(filteredEntries[i].monster.asset is Mon_Sky_StarSeed))
			{
				list.Add(filteredEntries[i]);
			}
		}
		filteredEntries.Clear();
		count = list.Count;
		int num = Random.Range(dreamStatues.x, dreamStatues.y + 1);
		for (int j = 0; j < num; j++)
		{
			Monster asset = list[Random.Range(0, count)].monster.asset;
			Vector3 pos = CalSpawnPosition();
			Monster monster = SpawnEntity(asset, pos, Quaternion.Euler(0f, Random.Range(0f, 360f), 0f), DewPlayer.environment, NetworkedManagerBase<GameManager>.instance.ambientLevel, (Monster b) =>
			{
				b.Visual.skipSpawning = true;
				b.AI.disableAI = true;
				b.Sound.voiceStart = null;
				b.Sound.voiceIdle = null;
				b.Sound.voiceDeath = null;
				b.populationCost = 0f;
			});
			monster.InvalidatePoolReuseEverywhere();
			RpcModifiyStatueEntity(monster);
			CreateStatusEffect<Se_PureDream_Statue>(monster, new CastInfo(monster));
		}
	}

	[ClientRpc]
	private void RpcModifiyStatueEntity(Entity b)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteNetworkBehaviour((NetworkWriter)(object)val, (NetworkBehaviour)(object)b);
		((NetworkBehaviour)this).SendRPCInternal("System.Void RoomMod_PureDream::RpcModifiyStatueEntity(Entity)", -812774626, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			_indices = null;
		}
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_RpcModifiyStatueEntity__Entity(Entity b)
	{
		List<Renderer> list = new List<Renderer>();
		list.AddRange(((Component)(object)b).GetComponentsInChildren<SkinnedMeshRenderer>());
		foreach (Renderer item in list)
		{
			Material[] sharedMaterials = item.sharedMaterials;
			for (int i = 0; i < sharedMaterials.Length; i++)
			{
				sharedMaterials[i] = matDreamDust;
			}
			item.sharedMaterials = sharedMaterials;
		}
		Animator componentInChildren = ((Component)(object)b).GetComponentInChildren<Animator>();
		if ((Object)(object)componentInChildren != null)
		{
			((Behaviour)(object)componentInChildren).enabled = false;
		}
		b.Visual.model.deathBehavior = EntityVisual.EntityDeathBehavior.HideModel;
	}

	protected static void InvokeUserCode_RpcModifiyStatueEntity__Entity(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcModifiyStatueEntity called on server.");
		}
		else
		{
			((RoomMod_PureDream)(object)obj).UserCode_RpcModifiyStatueEntity__Entity(NetworkReaderExtensions.ReadNetworkBehaviour<Entity>(reader));
		}
	}

	static RoomMod_PureDream()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected Obj, but got Unknown
		RemoteProcedureCalls.RegisterRpc(typeof(RoomMod_PureDream), "System.Void RoomMod_PureDream::RpcModifiyStatueEntity(Entity)", (RemoteCallDelegate)InvokeUserCode_RpcModifiyStatueEntity__Entity);
	}
}
