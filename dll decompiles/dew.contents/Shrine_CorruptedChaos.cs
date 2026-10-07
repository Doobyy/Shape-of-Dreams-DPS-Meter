using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class Shrine_CorruptedChaos : EditSkillShrine, IRewardActor
{
	[Serializable]
	public class PoolItem
	{
		public CorruptedChaosRewardType type;

		public float weight;
	}

	public List<PoolItem> pool = new List<PoolItem>();

	[SaveVar(SaveVarFlags.Default)]
	public readonly SyncDictionary<string, CorruptedChaosRewardType[]> rewards = new SyncDictionary<string, CorruptedChaosRewardType[]>();

	public Dictionary<string, int> choiceOffset = new Dictionary<string, int>();

	[NonSerialized]
	[SaveVar(SaveVarFlags.Default)]
	public string[] playersOverride;

	public int numOfChoice = 3;

	public GameObject fxDestroy;

	public int addedEssenceSlotMax = 4;

	[Server]
	private void RemovePlayerAndCheckEmpty(DewPlayer player)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Shrine_CorruptedChaos::RemovePlayerAndCheckEmpty(DewPlayer)' called when server was not active");
			return;
		}
		((SyncIDictionary<string, CorruptedChaosRewardType[]>)(object)rewards).Remove(player.guid);
		NetworkedManagerBase<ClientEventManager>.instance.InvokeOnChaosUsed(player);
		List<Shrine_HeroSoul> list = Dew.FindAllActorsOfType(out ListReturnHandle<Shrine_HeroSoul> handle);
		string[] array = rewards.Keys.ToArray();
		foreach (string id in array)
		{
			DewPlayer p = DewPlayer.gamePlayers.Find((DewPlayer pl) => pl.guid == id);
			if (p.hero.IsNullInactiveDeadOrKnockedOut() && (UnityEngine.Object)(object)list.Find((Shrine_HeroSoul s) => (UnityEngine.Object)(object)s.targetHero == (UnityEngine.Object)(object)p.hero) == null)
			{
				((SyncIDictionary<string, CorruptedChaosRewardType[]>)(object)rewards).Remove(id);
			}
		}
		handle.Return();
		if (((SyncIDictionary<string, CorruptedChaosRewardType[]>)(object)rewards).Count <= 0)
		{
			FxPlayNetworked(fxDestroy);
			Destroy();
		}
	}

	public override void OnLateStartServer()
	{
		base.OnLateStartServer();
		if (!isNewInstance)
		{
			return;
		}
		((SyncIDictionary<string, CorruptedChaosRewardType[]>)(object)rewards).Clear();
		foreach (DewPlayer gamePlayer in DewPlayer.gamePlayers)
		{
			if (playersOverride != null && !playersOverride.Contains(gamePlayer.guid))
			{
				continue;
			}
			List<int> list = new List<int>();
			for (int i = 0; i < pool.Count; i++)
			{
				if (pool[i].type != CorruptedChaosRewardType.AttackRange || !gamePlayer.hero.IsMeleeHero())
				{
					list.Add(i);
				}
			}
			int valueOrDefault = CollectionExtensions.GetValueOrDefault<string, int>((IReadOnlyDictionary<string, int>)choiceOffset, gamePlayer.guid, 0);
			CorruptedChaosRewardType[] array = new CorruptedChaosRewardType[numOfChoice + valueOrDefault];
			for (int j = 0; j < numOfChoice + valueOrDefault; j++)
			{
				if (list.Count == 0)
				{
					break;
				}
				int num = Dew.SelectRandomWeightedInList(list, (int ii) => pool[ii].weight, null);
				list.Remove(num);
				array[j] = pool[num].type;
			}
			((SyncIDictionary<string, CorruptedChaosRewardType[]>)(object)rewards).Add(gamePlayer.guid, array);
		}
	}

	protected override bool OnUse(Entity entity)
	{
		TpcOpenChaos(((NetworkBehaviour)entity.owner).connectionToClient);
		return false;
	}

	[TargetRpc]
	private void TpcOpenChaos(NetworkConnectionToClient target)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendTargetRPCInternal((NetworkConnection)(object)target, "System.Void Shrine_CorruptedChaos::TpcOpenChaos(Mirror.NetworkConnectionToClient)", -1887346759, (NetworkWriter)(object)val, 0);
		NetworkWriterPool.Return(val);
	}

	[TargetRpc]
	private void TpcCloseFloatingWindow(NetworkConnectionToClient conn)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendTargetRPCInternal((NetworkConnection)(object)conn, "System.Void Shrine_CorruptedChaos::TpcCloseFloatingWindow(Mirror.NetworkConnectionToClient)", 387478481, (NetworkWriter)(object)val, 0);
		NetworkWriterPool.Return(val);
	}

	[Command(requiresAuthority = false)]
	public void CmdChoose(int index, NetworkConnectionToClient sender = null)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteInt((NetworkWriter)(object)val, index);
		((NetworkBehaviour)this).SendCommandInternal("System.Void Shrine_CorruptedChaos::CmdChoose(System.Int32,Mirror.NetworkConnectionToClient)", 1629056940, (NetworkWriter)(object)val, 0, false);
		NetworkWriterPool.Return(val);
	}

	public override string GetEditSkillIndicatorRawText()
	{
		return string.Format(DewLocalization.GetUIValue("Shrine_CorruptedChaos_AddedEssenceSlot_EditSkill"), addedEssenceSlotMax);
	}

	public override EditSkillTargetType GetTargetTypes(DewPlayer player)
	{
		return EditSkillTargetType.Skill | EditSkillTargetType.SkillEmptySlot;
	}

	public override EditSkillTargetInfo GetTargetInfo(DewPlayer player, HeroSkillLocation loc, SkillTrigger target)
	{
		if (((SyncIDictionary<string, CorruptedChaosRewardType[]>)(object)rewards)[player.guid][int.Parse(GetCustomData(player))] != CorruptedChaosRewardType.AddedEssenceSlot)
		{
			return default;
		}
		int maxGemCount = player.hero.Skill.GetMaxGemCount(loc);
		if (maxGemCount >= 4)
		{
			return new EditSkillTargetInfo
			{
				rejectReasonRawText = DewLocalization.GetUIValue("Shrine_CorruptedChaos_AddedEssenceSlot_MaxReached"),
				actionTypeRawText = DewLocalization.GetUIValue("Shrine_CorruptedChaos_AddedEssenceSlot_ActionVerb")
			};
		}
		return new EditSkillTargetInfo
		{
			tooltipRawText = string.Format("{0} {1}<sprite=0>{2}", DewLocalization.GetUIValue("Shrine_CorruptedChaos_AddedEssenceSlot_Tooltip"), maxGemCount, maxGemCount + 1),
			actionTypeRawText = DewLocalization.GetUIValue("Shrine_CorruptedChaos_AddedEssenceSlot_ActionVerb")
		};
	}

	protected override bool OnActivateEditSkill(DewPlayer player, HeroSkillLocation loc, SkillTrigger target)
	{
		if (!int.TryParse(GetCustomData(player), out var result))
		{
			return false;
		}
		CorruptedChaosRewardType[] array = default;
		if (!((SyncIDictionary<string, CorruptedChaosRewardType[]>)(object)rewards).TryGetValue(player.guid, ref array))
		{
			return false;
		}
		if (!array.TryGetValue(result, out var value))
		{
			return false;
		}
		if (value != CorruptedChaosRewardType.AddedEssenceSlot)
		{
			return false;
		}
		if (player.hero.Skill.GetMaxGemCount(loc) >= addedEssenceSlotMax)
		{
			return false;
		}
		Shrine_Chaos.GetBonusStatusEffect(player.hero).AddEssenceSlotBonus(loc);
		RemovePlayerAndCheckEmpty(player);
		return true;
	}

	public override bool CanInteract(Entity entity)
	{
		if ((UnityEngine.Object)(object)entity.owner == null)
		{
			return false;
		}
		if (base.CanInteract(entity))
		{
			return ((SyncIDictionary<string, CorruptedChaosRewardType[]>)(object)rewards).ContainsKey(entity.owner.guid);
		}
		return false;
	}

	public override bool ShouldBeSavedWithRoom()
	{
		return true;
	}

	public Shrine_CorruptedChaos()
	{
		((NetworkBehaviour)this).InitSyncObject((SyncObject)(object)rewards);
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_TpcOpenChaos__NetworkConnectionToClient(NetworkConnectionToClient target)
	{
		ManagerBase<FloatingWindowManager>.instance.SetTarget((MonoBehaviour)(object)this);
	}

	protected static void InvokeUserCode_TpcOpenChaos__NetworkConnectionToClient(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TpcOpenChaos called on server.");
		}
		else
		{
			((Shrine_CorruptedChaos)(object)obj).UserCode_TpcOpenChaos__NetworkConnectionToClient((NetworkConnectionToClient)(object)NetworkClient.connection);
		}
	}

	protected void UserCode_TpcCloseFloatingWindow__NetworkConnectionToClient(NetworkConnectionToClient conn)
	{
		ManagerBase<FloatingWindowManager>.instance.ClearTarget();
	}

	protected static void InvokeUserCode_TpcCloseFloatingWindow__NetworkConnectionToClient(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TpcCloseFloatingWindow called on server.");
		}
		else
		{
			((Shrine_CorruptedChaos)(object)obj).UserCode_TpcCloseFloatingWindow__NetworkConnectionToClient((NetworkConnectionToClient)(object)NetworkClient.connection);
		}
	}

	protected void UserCode_CmdChoose__Int32__NetworkConnectionToClient(int index, NetworkConnectionToClient sender)
	{
		try
		{
			DewPlayer player = sender.GetPlayer();
			if (!((SyncIDictionary<string, CorruptedChaosRewardType[]>)(object)rewards).ContainsKey(player.guid))
			{
				return;
			}
			CorruptedChaosRewardType corruptedChaosRewardType = ((SyncIDictionary<string, CorruptedChaosRewardType[]>)(object)rewards)[player.guid][index];
			if (corruptedChaosRewardType == CorruptedChaosRewardType.AddedEssenceSlot)
			{
				TpcCloseFloatingWindow(sender);
				EnterEditSkill(player, index.ToString(CultureInfo.InvariantCulture));
				return;
			}
			Se_Shrine_Chaos_StatBonus bonusStatusEffect = Shrine_Chaos.GetBonusStatusEffect(player.hero);
			if (corruptedChaosRewardType == CorruptedChaosRewardType.EveryFourAttackBonus && bonusStatusEffect.corruptedBonus.everyFourAttackStartIndexFlat >= 3)
			{
				player.TpcShowCenterMessage(CenterMessageType.Error, "Shrine_CorruptedChaos_EveryFourAttackBonus_MinReached");
				return;
			}
			bonusStatusEffect.AddCorruptedBonus(corruptedChaosRewardType);
			DoPostUseRoutines(player.hero);
			RemovePlayerAndCheckEmpty(player);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception, (UnityEngine.Object)(object)this);
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
			((Shrine_CorruptedChaos)(object)obj).UserCode_CmdChoose__Int32__NetworkConnectionToClient(NetworkReaderExtensions.ReadInt(reader), senderConnection);
		}
	}

	static Shrine_CorruptedChaos()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected Obj, but got Unknown
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Expected Obj, but got Unknown
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Expected Obj, but got Unknown
		RemoteProcedureCalls.RegisterCommand(typeof(Shrine_CorruptedChaos), "System.Void Shrine_CorruptedChaos::CmdChoose(System.Int32,Mirror.NetworkConnectionToClient)", (RemoteCallDelegate)InvokeUserCode_CmdChoose__Int32__NetworkConnectionToClient, false);
		RemoteProcedureCalls.RegisterRpc(typeof(Shrine_CorruptedChaos), "System.Void Shrine_CorruptedChaos::TpcOpenChaos(Mirror.NetworkConnectionToClient)", (RemoteCallDelegate)InvokeUserCode_TpcOpenChaos__NetworkConnectionToClient);
		RemoteProcedureCalls.RegisterRpc(typeof(Shrine_CorruptedChaos), "System.Void Shrine_CorruptedChaos::TpcCloseFloatingWindow(Mirror.NetworkConnectionToClient)", (RemoteCallDelegate)InvokeUserCode_TpcCloseFloatingWindow__NetworkConnectionToClient);
	}
}
