using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class Shrine_Chaos : EditSkillShrine, IRewardActor
{
	public int numOfChoice = 3;

	public ParticleSystem[] tintedPses;

	public Light[] tintedLights;

	public PerRarityData<float> chance;

	public PerRarityData<float> highChance;

	public PerRarityData<ChaosReward[]> poolByRarity;

	public GameObject destroyEffect;

	public GameObject useEffectOnHero;

	public GameObject useEffectOnLocalHero;

	[SaveVar(SaveVarFlags.Default)]
	public readonly SyncDictionary<string, ChaosReward[]> rewards = new SyncDictionary<string, ChaosReward[]>();

	public Dictionary<string, int> choiceOffset = new Dictionary<string, int>();

	[NonSerialized]
	[SaveVar(SaveVarFlags.Default)]
	[SyncVar]
	public Rarity rarity;

	[NonSerialized]
	[SaveVar(SaveVarFlags.Default)]
	public string[] playersOverride;

	public override bool canInteractWithMouse => true;

	public Rarity Networkrarity
	{
		get
		{
			return rarity;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<Rarity>(value, ref rarity, 256uL, (Action<Rarity, Rarity>)null);
		}
	}

	public override void OnStartClient()
	{
		Color rarityColor = Dew.GetRarityColor(rarity);
		rarityColor = rarityColor.WithV(rarityColor.GetV() - 0.1f);
		ParticleSystem[] array = tintedPses;
		for (int i = 0; i < array.Length; i++)
		{
			DewEffect.TintObject(array[i], rarityColor);
		}
		Light[] array2 = tintedLights;
		for (int i = 0; i < array2.Length; i++)
		{
			DewEffect.TintObject(array2[i], rarityColor);
		}
		base.OnStartClient();
	}

	[Server]
	private void RemovePlayerAndCheckEmpty(DewPlayer player)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Shrine_Chaos::RemovePlayerAndCheckEmpty(DewPlayer)' called when server was not active");
			return;
		}
		((SyncIDictionary<string, ChaosReward[]>)(object)rewards).Remove(player.guid);
		NetworkedManagerBase<ClientEventManager>.instance.InvokeOnChaosUsed(player);
		List<Shrine_HeroSoul> list = Dew.FindAllActorsOfType(out ListReturnHandle<Shrine_HeroSoul> handle);
		string[] array = rewards.Keys.ToArray();
		foreach (string id in array)
		{
			DewPlayer p = DewPlayer.gamePlayers.Find((DewPlayer pl) => pl.guid == id);
			if (p.hero.IsNullInactiveDeadOrKnockedOut() && (UnityEngine.Object)(object)list.Find((Shrine_HeroSoul s) => (UnityEngine.Object)(object)s.targetHero == (UnityEngine.Object)(object)p.hero) == null)
			{
				((SyncIDictionary<string, ChaosReward[]>)(object)rewards).Remove(id);
			}
		}
		handle.Return();
		if (((SyncIDictionary<string, ChaosReward[]>)(object)rewards).Count <= 0)
		{
			FxPlayNetworked(destroyEffect);
			Destroy();
		}
	}

	[Server]
	public static StatBonus GetBonusStat(Hero hero)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'StatBonus Shrine_Chaos::GetBonusStat(Hero)' called when server was not active");
			return null;
		}
		return GetBonusStatusEffect(hero).bonus;
	}

	[Server]
	public static Se_Shrine_Chaos_StatBonus GetBonusStatusEffect(Hero hero)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'Se_Shrine_Chaos_StatBonus Shrine_Chaos::GetBonusStatusEffect(Hero)' called when server was not active");
			return null;
		}
		if (!hero.Status.TryGetStatusEffect<Se_Shrine_Chaos_StatBonus>(out var effect))
		{
			return hero.CreateStatusEffect<Se_Shrine_Chaos_StatBonus>(hero, new CastInfo(hero));
		}
		return effect;
	}

	public void SetRandomRarity(bool isHighQuality, DewRandom random = null)
	{
		if (random == null)
		{
			random = DewRandom.instance;
		}
		float num = random.Value();
		PerRarityData<float> perRarityData = (isHighQuality ? highChance : chance);
		if (num < perRarityData.common)
		{
			Networkrarity = Rarity.Common;
		}
		else if (num < perRarityData.common + perRarityData.rare)
		{
			Networkrarity = Rarity.Rare;
		}
		else if (num < perRarityData.common + perRarityData.rare + perRarityData.epic)
		{
			Networkrarity = Rarity.Epic;
		}
		else
		{
			Networkrarity = Rarity.Legendary;
		}
	}

	public override void OnLateStartServer()
	{
		base.OnLateStartServer();
		if (!isNewInstance)
		{
			return;
		}
		((SyncIDictionary<string, ChaosReward[]>)(object)rewards).Clear();
		ChaosReward[] array = poolByRarity.Get(rarity);
		foreach (DewPlayer gamePlayer in DewPlayer.gamePlayers)
		{
			if (playersOverride == null || playersOverride.Contains(gamePlayer.guid))
			{
				List<int> list = new List<int>();
				for (int i = 0; i < array.Length; i++)
				{
					list.Add(i);
				}
				list.Shuffle();
				int valueOrDefault = CollectionExtensions.GetValueOrDefault<string, int>((IReadOnlyDictionary<string, int>)choiceOffset, gamePlayer.guid, 0);
				ChaosReward[] array2 = new ChaosReward[numOfChoice + valueOrDefault];
				for (int j = 0; j < numOfChoice + valueOrDefault; j++)
				{
					array2[j] = array[list[j]];
					array2[j].rarity = rarity;
				}
				((SyncIDictionary<string, ChaosReward[]>)(object)rewards).Add(gamePlayer.guid, array2);
			}
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
		((NetworkBehaviour)this).SendTargetRPCInternal((NetworkConnection)(object)target, "System.Void Shrine_Chaos::TpcOpenChaos(Mirror.NetworkConnectionToClient)", -1695430507, (NetworkWriter)(object)val, 0);
		NetworkWriterPool.Return(val);
	}

	[Command(requiresAuthority = false)]
	public void CmdChoose(int index, NetworkConnectionToClient sender = null)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteInt((NetworkWriter)(object)val, index);
		((NetworkBehaviour)this).SendCommandInternal("System.Void Shrine_Chaos::CmdChoose(System.Int32,Mirror.NetworkConnectionToClient)", -1112443512, (NetworkWriter)(object)val, 0, false);
		NetworkWriterPool.Return(val);
	}

	[ClientRpc]
	private void RpcShowCenterMessage(DewPlayer player, string message, string formatArg)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteNetworkBehaviour((NetworkWriter)(object)val, (NetworkBehaviour)(object)player);
		NetworkWriterExtensions.WriteString((NetworkWriter)(object)val, message);
		NetworkWriterExtensions.WriteString((NetworkWriter)(object)val, formatArg);
		((NetworkBehaviour)this).SendRPCInternal("System.Void Shrine_Chaos::RpcShowCenterMessage(DewPlayer,System.String,System.String)", 255448538, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[ClientRpc]
	private void RpcPlayLocalEffect(DewPlayer player)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteNetworkBehaviour((NetworkWriter)(object)val, (NetworkBehaviour)(object)player);
		((NetworkBehaviour)this).SendRPCInternal("System.Void Shrine_Chaos::RpcPlayLocalEffect(DewPlayer)", 1762722735, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	public override string GetEditSkillIndicatorRawText()
	{
		ChaosReward chaosReward = ((SyncIDictionary<string, ChaosReward[]>)(object)rewards)[DewPlayer.local.guid][int.Parse(GetCustomData(DewPlayer.local))];
		if (chaosReward.type == ChaosRewardType.UpgradeSkill)
		{
			return DewLocalization.GetUIValue("InGame_SelectSkillToUpgrade");
		}
		if (chaosReward.type == ChaosRewardType.UpgradeGem)
		{
			return DewLocalization.GetUIValue("InGame_SelectGemToUpgrade");
		}
		return null;
	}

	public override Color GetEditSkillIndicatorColor()
	{
		ChaosReward chaosReward = ((SyncIDictionary<string, ChaosReward[]>)(object)rewards)[DewPlayer.local.guid][int.Parse(GetCustomData(DewPlayer.local))];
		if (chaosReward.type == ChaosRewardType.UpgradeSkill)
		{
			return EditSkillShrine.GenericSkillIndicatorColor;
		}
		if (chaosReward.type == ChaosRewardType.UpgradeGem)
		{
			return EditSkillShrine.GenericGemIndicatorColor;
		}
		return default;
	}

	public override EditSkillTargetType GetTargetTypes(DewPlayer player)
	{
		return ((SyncIDictionary<string, ChaosReward[]>)(object)rewards)[player.guid][int.Parse(GetCustomData(player))].type switch
		{
			ChaosRewardType.UpgradeSkill => EditSkillTargetType.Skill, 
			ChaosRewardType.UpgradeGem => EditSkillTargetType.Gem, 
			_ => EditSkillTargetType.None, 
		};
	}

	public override EditSkillTargetInfo GetTargetInfo(DewPlayer player, HeroSkillLocation loc, SkillTrigger target)
	{
		return new EditSkillTargetInfo
		{
			nextLevel = target.level + (int)((SyncIDictionary<string, ChaosReward[]>)(object)rewards)[target.owner.owner.guid][int.Parse(GetCustomData(target.owner.owner))].quantity,
			rejectReasonRawText = ((!target.isLevelUpEnabled) ? DewLocalization.GetUIValue("InGame_Message_CantUpgradeThisSkill") : null)
		};
	}

	public override EditSkillTargetInfo GetTargetInfo(DewPlayer player, GemLocation loc, Gem target)
	{
		return new EditSkillTargetInfo
		{
			nextLevel = target.quality + (int)((SyncIDictionary<string, ChaosReward[]>)(object)rewards)[target.owner.owner.guid][int.Parse(GetCustomData(target.owner.owner))].quantity
		};
	}

	protected override bool OnActivateEditSkill(DewPlayer player, HeroSkillLocation loc, SkillTrigger target)
	{
		if (!int.TryParse(GetCustomData(target.owner.owner), out var result))
		{
			return false;
		}
		ChaosReward[] array = default;
		if (!((SyncIDictionary<string, ChaosReward[]>)(object)rewards).TryGetValue(target.owner.owner.guid, ref array))
		{
			return false;
		}
		if (!array.TryGetValue(result, out var value))
		{
			return false;
		}
		if (value.type != ChaosRewardType.UpgradeSkill)
		{
			return false;
		}
		target.level += Mathf.RoundToInt(value.quantity);
		FxPlayNewNetworked(useEffectOnHero, target.owner);
		RpcPlayLocalEffect(target.owner.owner);
		RemovePlayerAndCheckEmpty(target.owner.owner);
		NetworkedManagerBase<ClientEventManager>.instance.InvokeOnItemUpgraded(target.owner, (NetworkBehaviour)(object)target);
		return true;
	}

	protected override bool OnActivateEditSkill(DewPlayer player, GemLocation loc, Gem target)
	{
		if (!int.TryParse(GetCustomData(target.owner.owner), out var result))
		{
			return false;
		}
		ChaosReward[] array = default;
		if (!((SyncIDictionary<string, ChaosReward[]>)(object)rewards).TryGetValue(target.owner.owner.guid, ref array))
		{
			return false;
		}
		if (!array.TryGetValue(result, out var value))
		{
			return false;
		}
		if (value.type != ChaosRewardType.UpgradeGem)
		{
			return false;
		}
		target.quality += Mathf.RoundToInt(value.quantity);
		FxPlayNewNetworked(useEffectOnHero, target.owner);
		RpcPlayLocalEffect(target.owner.owner);
		RemovePlayerAndCheckEmpty(target.owner.owner);
		NetworkedManagerBase<ClientEventManager>.instance.InvokeOnItemUpgraded(target.owner, (NetworkBehaviour)(object)target);
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
			return ((SyncIDictionary<string, ChaosReward[]>)(object)rewards).ContainsKey(entity.owner.guid);
		}
		return false;
	}

	public override bool ShouldBeSavedWithRoom()
	{
		return true;
	}

	public Shrine_Chaos()
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
			((Shrine_Chaos)(object)obj).UserCode_TpcOpenChaos__NetworkConnectionToClient((NetworkConnectionToClient)(object)NetworkClient.connection);
		}
	}

	protected void UserCode_CmdChoose__Int32__NetworkConnectionToClient(int index, NetworkConnectionToClient sender)
	{
		try
		{
			DewPlayer player = sender.GetPlayer();
			if (((SyncIDictionary<string, ChaosReward[]>)(object)rewards).ContainsKey(player.guid))
			{
				ChaosReward chaosReward = ((SyncIDictionary<string, ChaosReward[]>)(object)rewards)[player.guid][index];
				switch (chaosReward.type)
				{
				case ChaosRewardType.MaxHealth:
					GetBonusStat(player.hero).maxHealthFlat += chaosReward.quantity;
					RpcShowCenterMessage(player, "InGame_Message_Chaos_MaxHealth", chaosReward.quantity.ToString("#,##0"));
					break;
				case ChaosRewardType.AttackDamage:
					GetBonusStat(player.hero).attackDamageFlat += chaosReward.quantity;
					RpcShowCenterMessage(player, "InGame_Message_Chaos_AttackDamage", chaosReward.quantity.ToString("#,##0"));
					break;
				case ChaosRewardType.AttackSpeed:
					GetBonusStat(player.hero).attackSpeedPercentage += chaosReward.quantity;
					RpcShowCenterMessage(player, "InGame_Message_Chaos_AttackSpeed", chaosReward.quantity.ToString("#,##0"));
					break;
				case ChaosRewardType.AbilityPower:
					GetBonusStat(player.hero).abilityPowerFlat += chaosReward.quantity;
					RpcShowCenterMessage(player, "InGame_Message_Chaos_AbilityPower", chaosReward.quantity.ToString("#,##0"));
					break;
				case ChaosRewardType.AbilityHaste:
					GetBonusStat(player.hero).abilityHasteFlat += chaosReward.quantity;
					RpcShowCenterMessage(player, "InGame_Message_Chaos_AbilityHaste", chaosReward.quantity.ToString("#,##0"));
					break;
				case ChaosRewardType.UpgradeGem:
				case ChaosRewardType.UpgradeSkill:
					EnterEditSkill(player, index.ToString());
					return;
				case ChaosRewardType.Armor:
					GetBonusStat(player.hero).armorFlat += chaosReward.quantity;
					RpcShowCenterMessage(player, "InGame_Message_Chaos_Armor", chaosReward.quantity.ToString("#,##0"));
					break;
				default:
					throw new ArgumentOutOfRangeException();
				}
				if (player.hero.Status.TryGetStatusEffect<Se_Shrine_Chaos_StatBonus>(out var effect))
				{
					effect.NotifyUpdate();
				}
				DoPostUseRoutines(player.hero);
				FxPlayNewNetworked(useEffectOnHero, player.hero);
				RpcPlayLocalEffect(player);
				RemovePlayerAndCheckEmpty(player);
			}
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
			((Shrine_Chaos)(object)obj).UserCode_CmdChoose__Int32__NetworkConnectionToClient(NetworkReaderExtensions.ReadInt(reader), senderConnection);
		}
	}

	protected void UserCode_RpcShowCenterMessage__DewPlayer__String__String(DewPlayer player, string message, string formatArg)
	{
		if (((NetworkBehaviour)player).isLocalPlayer)
		{
			InGameUIManager.instance.ShowCenterMessage(CenterMessageType.General, message, new object[1] { formatArg });
		}
	}

	protected static void InvokeUserCode_RpcShowCenterMessage__DewPlayer__String__String(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcShowCenterMessage called on server.");
		}
		else
		{
			((Shrine_Chaos)(object)obj).UserCode_RpcShowCenterMessage__DewPlayer__String__String(NetworkReaderExtensions.ReadNetworkBehaviour<DewPlayer>(reader), NetworkReaderExtensions.ReadString(reader), NetworkReaderExtensions.ReadString(reader));
		}
	}

	protected void UserCode_RpcPlayLocalEffect__DewPlayer(DewPlayer player)
	{
		if (((NetworkBehaviour)player).isLocalPlayer)
		{
			FxPlayNew(useEffectOnLocalHero, player.hero);
		}
	}

	protected static void InvokeUserCode_RpcPlayLocalEffect__DewPlayer(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcPlayLocalEffect called on server.");
		}
		else
		{
			((Shrine_Chaos)(object)obj).UserCode_RpcPlayLocalEffect__DewPlayer(NetworkReaderExtensions.ReadNetworkBehaviour<DewPlayer>(reader));
		}
	}

	static Shrine_Chaos()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected Obj, but got Unknown
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Expected Obj, but got Unknown
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Expected Obj, but got Unknown
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Expected Obj, but got Unknown
		RemoteProcedureCalls.RegisterCommand(typeof(Shrine_Chaos), "System.Void Shrine_Chaos::CmdChoose(System.Int32,Mirror.NetworkConnectionToClient)", (RemoteCallDelegate)InvokeUserCode_CmdChoose__Int32__NetworkConnectionToClient, false);
		RemoteProcedureCalls.RegisterRpc(typeof(Shrine_Chaos), "System.Void Shrine_Chaos::RpcShowCenterMessage(DewPlayer,System.String,System.String)", (RemoteCallDelegate)InvokeUserCode_RpcShowCenterMessage__DewPlayer__String__String);
		RemoteProcedureCalls.RegisterRpc(typeof(Shrine_Chaos), "System.Void Shrine_Chaos::RpcPlayLocalEffect(DewPlayer)", (RemoteCallDelegate)InvokeUserCode_RpcPlayLocalEffect__DewPlayer);
		RemoteProcedureCalls.RegisterRpc(typeof(Shrine_Chaos), "System.Void Shrine_Chaos::TpcOpenChaos(Mirror.NetworkConnectionToClient)", (RemoteCallDelegate)InvokeUserCode_TpcOpenChaos__NetworkConnectionToClient);
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			GeneratedNetworkCode._Write_Rarity(writer, rarity);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x100L) != 0L)
		{
			GeneratedNetworkCode._Write_Rarity(writer, rarity);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<Rarity>(ref rarity, (Action<Rarity, Rarity>)null, GeneratedNetworkCode._Read_Rarity(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x100L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<Rarity>(ref rarity, (Action<Rarity, Rarity>)null, GeneratedNetworkCode._Read_Rarity(reader));
		}
	}
}
