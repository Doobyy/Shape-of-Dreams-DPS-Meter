using System;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class PropEnt_Merchant_Smoothie : PropEnt_Merchant_Base, IEnableWandering
{
	public int treasuresCount = 3;

	public int souvenirsCount = 3;

	private List<AssetRef<Treasure>> _treasures;

	private List<string> _waitingForSouvs = new List<string>();

	public bool shouldWanderAround => DewPlayer.gamePlayers.All((DewPlayer p) => p.hero.IsNullInactiveDeadOrKnockedOut() || Vector3.Distance(agentPosition, p.hero.agentPosition) > 7f);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		if (NetworkedManagerBase<ZoneManager>.instance.isCurrentNodeHunted)
		{
			Destroy();
			return;
		}
		_treasures = new List<AssetRef<Treasure>>();
		foreach (Treasure item in DewResources.FindAllByType<Treasure>(ResourceLoadSettings.Light))
		{
			if (Dew.IsTreasureIncludedInGame(((object)item).GetType().Name) && !item.excludeFromPool)
			{
				_treasures.Add(item);
			}
		}
		CreateStatusEffect<Se_OntologicalShield>(this, new CastInfo(this));
	}

	public override void OnInteract(Entity entity, bool alt)
	{
		base.OnInteract(entity, alt);
		if (((NetworkBehaviour)this).isServer)
		{
			Control.Stop();
			Control.RotateTowards(entity, immediately: false);
		}
	}

	protected override void OnRefresh(DewPlayer player)
	{
		base.OnRefresh(player);
		PopulatePlayerMerchandises(player);
	}

	protected override void OnPopulateMerchandises(DewPlayer player)
	{
		List<MerchandiseData> list = new List<MerchandiseData>();
		List<AssetRef<Treasure>> list2 = _treasures.ToList();
		for (int num = list2.Count - 1; num >= 0; num--)
		{
			list2[num].lightAsset.merchant = this;
			list2[num].lightAsset.player = player;
			list2[num].lightAsset.hero = player.hero;
			if (!list2[num].lightAsset.ShouldBeIncludedInPool())
			{
				list2.RemoveAt(num);
			}
		}
		for (int i = 0; i < treasuresCount; i++)
		{
			Treasure treasure;
			if (list2.Count == 0)
			{
				treasure = Dew.SelectRandomWeightedInList(_treasures, (AssetRef<Treasure> tr) => tr.lightAsset.chanceWeight, null);
			}
			else
			{
				treasure = Dew.SelectRandomWeightedInList(list2, (AssetRef<Treasure> tr) => tr.lightAsset.chanceWeight, null);
				list2.Remove(treasure);
			}
			try
			{
				treasure.OnAddMerchandise(out var mercPrice, out var customData);
				list.Add(new MerchandiseData
				{
					type = MerchandiseType.Treasure,
					itemName = ((object)treasure).GetType().Name,
					count = treasure.maxUse,
					price = mercPrice,
					customData = customData
				});
			}
			catch (Exception exception)
			{
				list.Add(new MerchandiseData
				{
					type = MerchandiseType.Empty
				});
				Debug.LogException(exception);
			}
		}
		if (!_waitingForSouvs.Contains(player.guid))
		{
			_waitingForSouvs.Add(player.guid);
		}
		TpcPopulateSouvenirs(player);
		((SyncIDictionary<string, MerchandiseData[]>)(object)merchandises)[player.guid] = list.ToArray();
	}

	[TargetRpc]
	private void TpcPopulateSouvenirs(NetworkConnectionToClient conn)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendTargetRPCInternal((NetworkConnection)(object)conn, "System.Void PropEnt_Merchant_Smoothie::TpcPopulateSouvenirs(Mirror.NetworkConnectionToClient)", 1456601207, (NetworkWriter)(object)val, 0);
		NetworkWriterPool.Return(val);
	}

	[Command(requiresAuthority = false)]
	private void CmdPopulateSouvenirs(List<string> souvs, NetworkConnectionToClient conn = null)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		GeneratedNetworkCode._Write_System_002ECollections_002EGeneric_002EList_00601_003CSystem_002EString_003E((NetworkWriter)(object)val, souvs);
		((NetworkBehaviour)this).SendCommandInternal("System.Void PropEnt_Merchant_Smoothie::CmdPopulateSouvenirs(System.Collections.Generic.List`1<System.String>,Mirror.NetworkConnectionToClient)", 36579240, (NetworkWriter)(object)val, 0, false);
		NetworkWriterPool.Return(val);
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_TpcPopulateSouvenirs__NetworkConnectionToClient(NetworkConnectionToClient conn)
	{
		List<Accessory> list = DewResources.FindAllByNameSubstring<Accessory>("Acc_", ResourceLoadSettings.Light).ToList();
		for (int num = list.Count - 1; num >= 0; num--)
		{
			if (list[num].generatedFromServer || list[num].excludeFromPool || !Dew.IsAccessoryIncludedInGame(list[num].name) || (DewSave.profileMain.accessories.TryGetValue(list[num].name, out var value) && value.isUnlocked))
			{
				list.RemoveAt(num);
			}
		}
		CmdPopulateSouvenirs(list.Select((Accessory s) => s.name).ToList());
	}

	protected static void InvokeUserCode_TpcPopulateSouvenirs__NetworkConnectionToClient(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TpcPopulateSouvenirs called on server.");
		}
		else
		{
			((PropEnt_Merchant_Smoothie)(object)obj).UserCode_TpcPopulateSouvenirs__NetworkConnectionToClient((NetworkConnectionToClient)(object)NetworkClient.connection);
		}
	}

	protected void UserCode_CmdPopulateSouvenirs__List_00601__NetworkConnectionToClient(List<string> souvs, NetworkConnectionToClient conn)
	{
		DewPlayer player = conn.GetPlayer();
		if ((UnityEngine.Object)(object)player == null || !_waitingForSouvs.Contains(player.guid))
		{
			return;
		}
		_waitingForSouvs.Remove(player.guid);
		MerchandiseData[] source = default;
		if (souvs.Count > 500 || !((SyncIDictionary<string, MerchandiseData[]>)(object)merchandises).TryGetValue(player.guid, ref source))
		{
			return;
		}
		for (int num = souvs.Count - 1; num >= 0; num--)
		{
			if (!Dew.IsAccessoryIncludedInGame(souvs[num]))
			{
				souvs.RemoveAt(num);
			}
			else
			{
				Accessory byName = DewResources.GetByName<Accessory>(souvs[num]);
				if (byName == null || byName.generatedFromServer)
				{
					souvs.RemoveAt(num);
				}
			}
		}
		List<MerchandiseData> list = source.ToList();
		for (int i = 0; i < souvenirsCount; i++)
		{
			if (souvs.Count <= 0)
			{
				break;
			}
			int index = UnityEngine.Random.Range(0, souvs.Count);
			list.Add(new MerchandiseData
			{
				type = MerchandiseType.Souvenir,
				count = 1,
				itemName = souvs[index],
				price = Cost.Stardust(200)
			});
			souvs.RemoveAt(index);
		}
		((SyncIDictionary<string, MerchandiseData[]>)(object)merchandises)[player.guid] = list.ToArray();
	}

	protected static void InvokeUserCode_CmdPopulateSouvenirs__List_00601__NetworkConnectionToClient(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdPopulateSouvenirs called on client.");
		}
		else
		{
			((PropEnt_Merchant_Smoothie)(object)obj).UserCode_CmdPopulateSouvenirs__List_00601__NetworkConnectionToClient(GeneratedNetworkCode._Read_System_002ECollections_002EGeneric_002EList_00601_003CSystem_002EString_003E(reader), senderConnection);
		}
	}

	static PropEnt_Merchant_Smoothie()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected Obj, but got Unknown
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Expected Obj, but got Unknown
		RemoteProcedureCalls.RegisterCommand(typeof(PropEnt_Merchant_Smoothie), "System.Void PropEnt_Merchant_Smoothie::CmdPopulateSouvenirs(System.Collections.Generic.List`1<System.String>,Mirror.NetworkConnectionToClient)", (RemoteCallDelegate)InvokeUserCode_CmdPopulateSouvenirs__List_00601__NetworkConnectionToClient, false);
		RemoteProcedureCalls.RegisterRpc(typeof(PropEnt_Merchant_Smoothie), "System.Void PropEnt_Merchant_Smoothie::TpcPopulateSouvenirs(Mirror.NetworkConnectionToClient)", (RemoteCallDelegate)InvokeUserCode_TpcPopulateSouvenirs__NetworkConnectionToClient);
	}
}
