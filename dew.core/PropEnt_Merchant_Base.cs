using System;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public abstract class PropEnt_Merchant_Base : PropEntity, IProp, IInteractable
{
	public bool isSellEnabled = true;

	public bool allowRefresh = true;

	[SaveVar(SaveVarFlags.Default)]
	public readonly SyncDictionary<string, MerchandiseData[]> merchandises = new SyncDictionary<string, MerchandiseData[]>();

	public SafeAction<DewPlayer> onMerchandisePopulated;

	public GameObject fxSell;

	public GameObject fxPurchase;

	public GameObject fxPurchaseSkill;

	public GameObject fxPurchaseGem;

	public GameObject fxPurchaseSouvenir;

	public GameObject fxPurchaseTreasure;

	public GameObject[] props;

	int IInteractable.priority => 100;

	bool IProp.isSingleton => true;

	public float focusDistance => 4.5f;

	public virtual Transform interactPivot => ((Component)(object)this).transform;

	public virtual bool canInteractWithMouse => true;

	protected override void Awake()
	{
		base.Awake();
		if (props != null)
		{
			GameObject[] array = props;
			foreach (GameObject gameObject in array)
			{
				gameObject.transform.parent = null;
				gameObject.SetLayerRecursive(0);
				FxPlay(gameObject);
			}
		}
	}

	public virtual bool CanRefresh(DewPlayer player)
	{
		if (allowRefresh)
		{
			return player.platinumCoin > 0;
		}
		return false;
	}

	public virtual Cost GetRefreshCost(DewPlayer player)
	{
		return new Cost
		{
			platinumCoin = 1
		};
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		GameManager.CallOnReady(() =>
		{
			NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnRoomLoaded += new Action<EventInfoLoadRoom>(OnRoomChanged);
		});
		OnCreate_Merchandise();
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if ((UnityEngine.Object)(object)NetworkedManagerBase<ZoneManager>.instance != null)
		{
			NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnRoomLoaded -= new Action<EventInfoLoadRoom>(OnRoomChanged);
		}
		OnDestroyActor_Merchandise();
	}

	private void OnRoomChanged(EventInfoLoadRoom obj)
	{
		if (!obj.isTraveling || ((UnityEngine.Object)(object)this != null && isActive))
		{
			return;
		}
		for (int i = 0; i < props.Length; i++)
		{
			GameObject gameObject = props[i];
			if (gameObject != null)
			{
				UnityEngine.Object.Destroy(gameObject);
				props[i] = null;
			}
		}
		if ((UnityEngine.Object)(object)NetworkedManagerBase<ZoneManager>.instance != null)
		{
			NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnRoomLoaded -= new Action<EventInfoLoadRoom>(OnRoomChanged);
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (props == null)
		{
			return;
		}
		GameObject[] array = props;
		foreach (GameObject gameObject in array)
		{
			if (gameObject != null)
			{
				gameObject.transform.SetPositionAndRotation(((Component)(object)this).transform.position, ((Component)(object)this).transform.rotation);
			}
		}
	}

	public virtual bool CanInteract(Entity entity)
	{
		if (isActive)
		{
			return Status.isAlive;
		}
		return false;
	}

	public virtual void OnInteract(Entity entity, bool alt)
	{
		if (((NetworkBehaviour)entity).isOwned && !alt)
		{
			ManagerBase<FloatingWindowManager>.instance.SetTarget((MonoBehaviour)(object)this);
			if (isSellEnabled)
			{
				ManagerBase<EditSkillManager>.instance.StartSell(this);
			}
		}
	}

	protected virtual void OnRefresh(DewPlayer player)
	{
	}

	protected void SpawnMerchandise(MerchandiseData data, DewPlayer player, Cost finalPrice)
	{
		Vector3 end = agentPosition + (player.hero.agentPosition - agentPosition).normalized * 3f + UnityEngine.Random.insideUnitSphere.Flattened();
		end = Dew.GetValidAgentDestination_LinearSweep(player.hero.agentPosition, end);
		FxPlayNewNetworked(fxPurchase, player.hero);
		if (data.type == MerchandiseType.Skill)
		{
			SkillTrigger skillTrigger = Dew.CreateSkillTrigger(DewResources.GetByShortTypeName<SkillTrigger>(data.itemName, default(ResourceLoadSettings)), end, data.level, player);
			if (finalPrice.gold > 0)
			{
				skillTrigger.maxSellGold = finalPrice.gold;
			}
			NetworkedManagerBase<ClientEventManager>.instance.InvokeOnItemBought(player.hero, (NetworkBehaviour)(object)skillTrigger);
			FxPlayNewNetworked(fxPurchaseSkill, player.hero);
		}
		else if (data.type == MerchandiseType.Gem)
		{
			Gem gem = Dew.CreateGem(DewResources.GetByShortTypeName<Gem>(data.itemName, default(ResourceLoadSettings)), end, data.level, player);
			if (finalPrice.gold > 0)
			{
				gem.maxSellGold = finalPrice.gold;
			}
			NetworkedManagerBase<ClientEventManager>.instance.InvokeOnItemBought(player.hero, (NetworkBehaviour)(object)gem);
			FxPlayNewNetworked(fxPurchaseGem, player.hero);
		}
		else if (data.type == MerchandiseType.Treasure)
		{
			Treasure nb = Dew.InstantiateAndSpawn<Treasure>(DewResources.GetByShortTypeName<Treasure>(data.itemName, default(ResourceLoadSettings)), end, (Quaternion?)null, (Action<Treasure>)((Treasure t) =>
			{
				t.price = finalPrice.gold;
				t.merchant = this;
				t.player = player;
				t.hero = player.hero;
				t.customData = data.customData;
			}));
			NetworkedManagerBase<ClientEventManager>.instance.InvokeOnItemBought(player.hero, (NetworkBehaviour)(object)nb);
			FxPlayNewNetworked(fxPurchaseTreasure, player.hero);
		}
		else
		{
			if (data.type != MerchandiseType.Souvenir)
			{
				return;
			}
			FxPlayNewNetworked(fxPurchaseSouvenir, player.hero);
			Accessory byName = DewResources.GetByName<Accessory>(data.itemName);
			for (int num = player.hero.accessories.Count - 1; num >= 0; num--)
			{
				if (DewResources.GetByName<Accessory>(player.hero.accessories[num]).type == byName.type)
				{
					player.hero.accessories.RemoveAt(num);
				}
			}
			player.hero.accessories.Add(data.itemName);
		}
	}

	protected void OnSell(DewPlayer activator, NetworkBehaviour target)
	{
		FxPlayNewNetworked(fxSell, activator.hero);
		if (target is SkillTrigger skillTrigger)
		{
			if (!((UnityEngine.Object)(object)skillTrigger.owner.owner != (UnityEngine.Object)(object)activator) && !((UnityEngine.Object)(object)activator.hero == null))
			{
				Hero hero = activator.hero;
				if (hero.Skill.TryGetSkillLocation(skillTrigger, out var type) && hero.Skill.CanReplaceSkill(type))
				{
					hero.Skill.UnequipSkill(type, hero.position);
					int sellGold = skillTrigger.GetSellGold();
					skillTrigger.Destroy();
					hero.owner.EarnGold(sellGold);
					NetworkedManagerBase<ClientEventManager>.instance.InvokeOnItemSold(activator.hero, target);
				}
			}
		}
		else if (target is Gem gem && !((UnityEngine.Object)(object)gem.owner.owner != (UnityEngine.Object)(object)activator) && !((UnityEngine.Object)(object)activator.hero == null))
		{
			Hero hero2 = activator.hero;
			if (hero2.Skill.TryGetGemLocation(gem, out var location))
			{
				hero2.Skill.UnequipGem(location, hero2.position);
				int sellGold2 = gem.GetSellGold();
				gem.Destroy();
				hero2.owner.EarnGold(sellGold2);
				NetworkedManagerBase<ClientEventManager>.instance.InvokeOnItemSold(activator.hero, target);
			}
		}
	}

	[Command(requiresAuthority = false)]
	public void CmdSell(NetworkBehaviour target, NetworkConnectionToClient sender = null)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteNetworkBehaviour((NetworkWriter)(object)val, target);
		((NetworkBehaviour)this).SendCommandInternal("System.Void PropEnt_Merchant_Base::CmdSell(Mirror.NetworkBehaviour,Mirror.NetworkConnectionToClient)", -1388222545, (NetworkWriter)(object)val, 0, false);
		NetworkWriterPool.Return(val);
	}

	[Command(requiresAuthority = false)]
	public void CmdPurchase(int index, NetworkConnectionToClient sender = null)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteInt((NetworkWriter)(object)val, index);
		((NetworkBehaviour)this).SendCommandInternal("System.Void PropEnt_Merchant_Base::CmdPurchase(System.Int32,Mirror.NetworkConnectionToClient)", -151629917, (NetworkWriter)(object)val, 0, false);
		NetworkWriterPool.Return(val);
	}

	[Command(requiresAuthority = false)]
	public void CmdRefresh(NetworkConnectionToClient sender = null)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendCommandInternal("System.Void PropEnt_Merchant_Base::CmdRefresh(Mirror.NetworkConnectionToClient)", -700024020, (NetworkWriter)(object)val, 0, false);
		NetworkWriterPool.Return(val);
	}

	private void OnCreate_Merchandise()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		Dew.CallDelayed(() =>
		{
			if (!this.IsNullOrInactive())
			{
				DewPlayer.onGamePlayerAdded += new Action<DewPlayer>(OnGamePlayerAdded);
				foreach (DewPlayer gamePlayer in DewPlayer.gamePlayers)
				{
					if (!((SyncIDictionary<string, MerchandiseData[]>)(object)merchandises).ContainsKey(gamePlayer.guid))
					{
						PopulatePlayerMerchandises(gamePlayer);
					}
				}
				RpcAddPreloadRule();
			}
		});
	}

	[ClientRpc]
	private void RpcAddPreloadRule()
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendRPCInternal("System.Void PropEnt_Merchant_Base::RpcAddPreloadRule()", 1628769885, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	private void OnDestroyActor_Merchandise()
	{
		if (((NetworkBehaviour)this).isServer)
		{
			DewPlayer.onGamePlayerAdded -= new Action<DewPlayer>(OnGamePlayerAdded);
		}
	}

	private void OnGamePlayerAdded(DewPlayer obj)
	{
		if (!((SyncIDictionary<string, MerchandiseData[]>)(object)merchandises).ContainsKey(obj.guid))
		{
			PopulatePlayerMerchandises(obj);
		}
	}

	public void PopulatePlayerMerchandises(DewPlayer player)
	{
		try
		{
			OnPopulateMerchandises(player);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
		onMerchandisePopulated?.Invoke(player);
	}

	protected abstract void OnPopulateMerchandises(DewPlayer player);

	protected PropEnt_Merchant_Base()
	{
		((NetworkBehaviour)this).InitSyncObject((SyncObject)(object)merchandises);
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_CmdSell__NetworkBehaviour__NetworkConnectionToClient(NetworkBehaviour target, NetworkConnectionToClient sender)
	{
		if (!isSellEnabled)
		{
			return;
		}
		try
		{
			OnSell(sender.GetPlayer(), target);
			sender.GetPlayer().hero.Control.UpdateLastMoveTime();
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	protected static void InvokeUserCode_CmdSell__NetworkBehaviour__NetworkConnectionToClient(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSell called on client.");
		}
		else
		{
			((PropEnt_Merchant_Base)(object)obj).UserCode_CmdSell__NetworkBehaviour__NetworkConnectionToClient(NetworkReaderExtensions.ReadNetworkBehaviour(reader), senderConnection);
		}
	}

	protected void UserCode_CmdPurchase__Int32__NetworkConnectionToClient(int index, NetworkConnectionToClient sender)
	{
		try
		{
			DewPlayer player = sender.GetPlayer();
			player.hero.Control.UpdateLastMoveTime();
			MerchandiseData[] array = default;
			if (!((SyncIDictionary<string, MerchandiseData[]>)(object)merchandises).TryGetValue(player.guid, ref array) || index < 0 || index >= array.Length)
			{
				return;
			}
			MerchandiseData data = array[index];
			if (data.count <= 0)
			{
				return;
			}
			Cost finalPrice = data.price.MultiplyGold((this is PropEnt_Merchant_Jonas) ? player.buyPriceMultiplier : 1f);
			if (finalPrice.CanAfford(player.hero) != AffordType.Yes)
			{
				return;
			}
			if (data.type == MerchandiseType.Treasure)
			{
				Treasure byShortTypeName = DewResources.GetByShortTypeName<Treasure>(data.itemName, default(ResourceLoadSettings));
				byShortTypeName.player = player;
				byShortTypeName.hero = player.hero;
				byShortTypeName.merchant = this;
				byShortTypeName.customData = data.customData;
				if (!byShortTypeName.CanBePurchased())
				{
					return;
				}
			}
			MerchandiseData[] array2 = array.ToArray();
			array2[index].count--;
			((SyncIDictionary<string, MerchandiseData[]>)(object)merchandises)[player.guid] = array2;
			if (finalPrice.gold > 0)
			{
				player.SpendGold(finalPrice.gold);
			}
			if (finalPrice.dreamDust > 0)
			{
				player.SpendDreamDust(finalPrice.dreamDust);
			}
			if (finalPrice.stardust > 0)
			{
				player.RpcInvokeOnSpendStardust(finalPrice.stardust);
			}
			SpawnMerchandise(data, player, finalPrice);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception, (UnityEngine.Object)(object)this);
		}
	}

	protected static void InvokeUserCode_CmdPurchase__Int32__NetworkConnectionToClient(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdPurchase called on client.");
		}
		else
		{
			((PropEnt_Merchant_Base)(object)obj).UserCode_CmdPurchase__Int32__NetworkConnectionToClient(NetworkReaderExtensions.ReadInt(reader), senderConnection);
		}
	}

	protected void UserCode_CmdRefresh__NetworkConnectionToClient(NetworkConnectionToClient sender)
	{
		DewPlayer player = sender.GetPlayer();
		if (!((UnityEngine.Object)(object)player == null) && CanRefresh(player))
		{
			Cost refreshCost = GetRefreshCost(player);
			if (refreshCost.CanAfford(player) == AffordType.Yes)
			{
				player.Spend(refreshCost);
				OnRefresh(player);
			}
		}
	}

	protected static void InvokeUserCode_CmdRefresh__NetworkConnectionToClient(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdRefresh called on client.");
		}
		else
		{
			((PropEnt_Merchant_Base)(object)obj).UserCode_CmdRefresh__NetworkConnectionToClient(senderConnection);
		}
	}

	protected void UserCode_RpcAddPreloadRule()
	{
		DewResources.AddPreloadRule((MonoBehaviour)(object)this, (PreloadInterface preload) =>
		{
			if (this.IsNullOrInactive())
			{
				return;
			}
			foreach (KeyValuePair<string, MerchandiseData[]> merchandise in merchandises)
			{
				MerchandiseData[] value = merchandise.Value;
				for (int i = 0; i < value.Length; i++)
				{
					MerchandiseData merchandiseData = value[i];
					if (!string.IsNullOrEmpty(merchandiseData.itemName))
					{
						MerchandiseType type = merchandiseData.type;
						if (type == MerchandiseType.Skill || type == MerchandiseType.Gem || type == MerchandiseType.Treasure)
						{
							preload.AddType(merchandiseData.itemName);
						}
					}
				}
			}
		});
	}

	protected static void InvokeUserCode_RpcAddPreloadRule(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcAddPreloadRule called on server.");
		}
		else
		{
			((PropEnt_Merchant_Base)(object)obj).UserCode_RpcAddPreloadRule();
		}
	}

	static PropEnt_Merchant_Base()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected Obj, but got Unknown
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Expected Obj, but got Unknown
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Expected Obj, but got Unknown
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Expected Obj, but got Unknown
		RemoteProcedureCalls.RegisterCommand(typeof(PropEnt_Merchant_Base), "System.Void PropEnt_Merchant_Base::CmdSell(Mirror.NetworkBehaviour,Mirror.NetworkConnectionToClient)", (RemoteCallDelegate)InvokeUserCode_CmdSell__NetworkBehaviour__NetworkConnectionToClient, false);
		RemoteProcedureCalls.RegisterCommand(typeof(PropEnt_Merchant_Base), "System.Void PropEnt_Merchant_Base::CmdPurchase(System.Int32,Mirror.NetworkConnectionToClient)", (RemoteCallDelegate)InvokeUserCode_CmdPurchase__Int32__NetworkConnectionToClient, false);
		RemoteProcedureCalls.RegisterCommand(typeof(PropEnt_Merchant_Base), "System.Void PropEnt_Merchant_Base::CmdRefresh(Mirror.NetworkConnectionToClient)", (RemoteCallDelegate)InvokeUserCode_CmdRefresh__NetworkConnectionToClient, false);
		RemoteProcedureCalls.RegisterRpc(typeof(PropEnt_Merchant_Base), "System.Void PropEnt_Merchant_Base::RpcAddPreloadRule()", (RemoteCallDelegate)InvokeUserCode_RpcAddPreloadRule);
	}
}
