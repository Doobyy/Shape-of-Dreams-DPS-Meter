using System;
using System.Collections;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public abstract class ChoiceShrine : Shrine
{
	[SaveVar(SaveVarFlags.Default)]
	public readonly SyncDictionary<string, ChoiceShrineItem[]> choices = new SyncDictionary<string, ChoiceShrineItem[]>();

	public SafeAction<DewPlayer> onItemsPopulated;

	public SafeAction ClientEvent_OnChoicesUpdated;

	public int itemCount = 3;

	protected override void Awake()
	{
		base.Awake();
		((SyncIDictionary<string, ChoiceShrineItem[]>)(object)choices).Callback += (Operation<string, ChoiceShrineItem[]> op, string key, ChoiceShrineItem[] item) =>
		{
			Dew.Debounce((UnityEngine.Object)(object)this, 0f, () =>
			{
				ClientEvent_OnChoicesUpdated?.Invoke();
			});
		};
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		Dew.CallDelayed(() =>
		{
			DewPlayer.onGamePlayerAdded += new Action<DewPlayer>(OnGamePlayerAdded);
			foreach (DewPlayer gamePlayer in DewPlayer.gamePlayers)
			{
				if (!((SyncIDictionary<string, ChoiceShrineItem[]>)(object)choices).ContainsKey(gamePlayer.guid))
				{
					PopulatePlayerChoices(gamePlayer);
				}
			}
		});
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			DewPlayer.onGamePlayerAdded -= new Action<DewPlayer>(OnGamePlayerAdded);
		}
	}

	private void OnGamePlayerAdded(DewPlayer obj)
	{
		if (!((SyncIDictionary<string, ChoiceShrineItem[]>)(object)choices).ContainsKey(obj.guid))
		{
			PopulatePlayerChoices(obj);
		}
	}

	public void PopulatePlayerChoices(DewPlayer player)
	{
		try
		{
			OnPopulateChoices(player);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
		onItemsPopulated?.Invoke(player);
	}

	protected abstract void OnPopulateChoices(DewPlayer player);

	protected override bool OnUse(Entity entity)
	{
		TpcOpenSelection(((NetworkBehaviour)entity.owner).connectionToClient);
		return false;
	}

	[TargetRpc]
	private void TpcOpenSelection(NetworkConnectionToClient target)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendTargetRPCInternal((NetworkConnection)(object)target, "System.Void ChoiceShrine::TpcOpenSelection(Mirror.NetworkConnectionToClient)", 1393396993, (NetworkWriter)(object)val, 0);
		NetworkWriterPool.Return(val);
	}

	[Command(requiresAuthority = false)]
	public void CmdChoose(int index, NetworkConnectionToClient sender = null)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteInt((NetworkWriter)(object)val, index);
		((NetworkBehaviour)this).SendCommandInternal("System.Void ChoiceShrine::CmdChoose(System.Int32,Mirror.NetworkConnectionToClient)", -1078923672, (NetworkWriter)(object)val, 0, false);
		NetworkWriterPool.Return(val);
	}

	protected ChoiceShrine()
	{
		((NetworkBehaviour)this).InitSyncObject((SyncObject)(object)choices);
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_TpcOpenSelection__NetworkConnectionToClient(NetworkConnectionToClient target)
	{
		ManagerBase<FloatingWindowManager>.instance.SetTarget((MonoBehaviour)(object)this);
	}

	protected static void InvokeUserCode_TpcOpenSelection__NetworkConnectionToClient(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TpcOpenSelection called on server.");
		}
		else
		{
			((ChoiceShrine)(object)obj).UserCode_TpcOpenSelection__NetworkConnectionToClient((NetworkConnectionToClient)(object)NetworkClient.connection);
		}
	}

	protected void UserCode_CmdChoose__Int32__NetworkConnectionToClient(int index, NetworkConnectionToClient sender)
	{
		DewPlayer player = sender.GetPlayer();
		ChoiceShrineItem[] items = default;
		Hero hero;
		if (!((UnityEngine.Object)(object)player == null) && ((SyncIDictionary<string, ChoiceShrineItem[]>)(object)choices).TryGetValue(player.guid, ref items) && index >= 0 && index < items.Length)
		{
			hero = player.hero;
			if (!((UnityEngine.Object)(object)hero == null) && CanAfford(hero) == AffordType.Yes && CanInteract(hero))
			{
				((MonoBehaviour)(object)this).StartCoroutine(Routine());
			}
		}
		IEnumerator Routine()
		{
			Vector3 pos = GetRandomSpawnPosition(hero.position);
			DoPostUseRoutines(hero);
			yield return new WaitForSeconds(0.5f);
			ChoiceShrineItem choiceShrineItem = items[index];
			UnityEngine.Object byShortTypeName = DewResources.GetByShortTypeName(choiceShrineItem.typeName);
			if (byShortTypeName is SkillTrigger trigger)
			{
				Dew.CreateSkillTrigger(trigger, pos, choiceShrineItem.level, player);
			}
			else if (byShortTypeName is Gem gem)
			{
				Dew.CreateGem(gem, pos, choiceShrineItem.level, player);
			}
		}
	}

	protected static void InvokeUserCode_CmdChoose__Int32__NetworkConnectionToClient(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdChoose called on client.");
		}
		else
		{
			((ChoiceShrine)(object)obj).UserCode_CmdChoose__Int32__NetworkConnectionToClient(NetworkReaderExtensions.ReadInt(reader), senderConnection);
		}
	}

	static ChoiceShrine()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected Obj, but got Unknown
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Expected Obj, but got Unknown
		RemoteProcedureCalls.RegisterCommand(typeof(ChoiceShrine), "System.Void ChoiceShrine::CmdChoose(System.Int32,Mirror.NetworkConnectionToClient)", (RemoteCallDelegate)InvokeUserCode_CmdChoose__Int32__NetworkConnectionToClient, false);
		RemoteProcedureCalls.RegisterRpc(typeof(ChoiceShrine), "System.Void ChoiceShrine::TpcOpenSelection(Mirror.NetworkConnectionToClient)", (RemoteCallDelegate)InvokeUserCode_TpcOpenSelection__NetworkConnectionToClient);
	}
}
