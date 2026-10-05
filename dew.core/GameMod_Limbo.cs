using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class GameMod_Limbo : GameModifierBase
{
	[NonSerialized]
	[SyncVar]
	[SaveVar(SaveVarFlags.Default)]
	public int depth = -1;

	public float atkMovSpdPerDepth = 3f;

	public float atkMovSpdPerDepthMax = 15f;

	public float d1SkillHaste = 50f;

	public float d1HuntSpeedAmp = 0.2f;

	public float d2DamageAmp = 0.15f;

	public float d2Armor = 20f;

	public float d3GoldPriceAmp = 0.25f;

	public float d3ExpReduction = 0.2f;

	public float d3ShieldReduction = 0.35f;

	public float d4MirageSkinChance = 0.2f;

	public float d4ElementalDurationAmp = 0.4f;

	public float d5BossAtkSpd = 20f;

	public float d5BossMovSpd = 20f;

	public static GameMod_Limbo instance
	{
		get
		{
			if ((UnityEngine.Object)(object)softInstance == null)
			{
				softInstance = Dew.FindActorOfType<GameMod_Limbo>();
			}
			return softInstance;
		}
	}

	public static GameMod_Limbo softInstance { get; private set; }

	public int Networkdepth
	{
		get
		{
			return depth;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<int>(value, ref depth, 8uL, (Action<int, int>)null);
		}
	}

	protected override void OnPrepare()
	{
		base.OnPrepare();
		if (isNewInstance)
		{
			Networkdepth = int.Parse(CollectionExtensions.GetValueOrDefault<string, string>((IReadOnlyDictionary<string, string>)NetworkedManagerBase<GameSettingsManager>.instance.customData, "GameMod_Limbo::depth", "1"), CultureInfo.InvariantCulture);
		}
	}

	protected override void OnCreate()
	{
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		base.OnCreate();
		softInstance = this;
		NotifyLimbo();
		NetworkedManagerBase<GameResultManager>.instance.ClientEvent_OnGameConcluded += new Action<DewGameResult>(ClientEventOnGameConcluded);
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		if (depth >= 3)
		{
			GameManager gameManager = NetworkedManagerBase<GameManager>.instance;
			gameManager.NetworkgoldCostMultiplier = gameManager.goldCostMultiplier * (1f + d3GoldPriceAmp);
			GameManager gameManager2 = NetworkedManagerBase<GameManager>.instance;
			gameManager2.NetworkgainedExpMultiplier = gameManager2.gainedExpMultiplier * (1f - d3ExpReduction);
		}
		NetworkedManagerBase<ActorManager>.instance.ClientEvent_OnActorAdd += new Action<Actor>(OnActorAdd);
		Actor[] array = NetworkedManagerBase<ActorManager>.instance.allActors.ToArray();
		foreach (Actor actor in array)
		{
			if (!actor.IsNullOrInactive())
			{
				OnActorAdd(actor);
			}
		}
		if (depth >= 1)
		{
			NetworkedManagerBase<ZoneManager>.instance.hunterSpreadMultiplier *= 1f + d1HuntSpeedAmp;
		}
		if (depth >= 4)
		{
			NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnRoomLoaded += new Action<EventInfoLoadRoom>(AddMirageSkinChanceToCurrentRoom);
		}
		NetworkedManagerBase<ZoneManager>.instance.onWorldGenerated += new Action(OnWorldGenerated);
		if (ManagerBase<LobbyManager>.instance.isLobbyLeader)
		{
			Dictionary<string, string> customData = ManagerBase<LobbyManager>.instance.service.currentLobby.customData;
			customData["GameMod_Limbo::depth"] = depth.ToString(CultureInfo.InvariantCulture);
			ManagerBase<LobbyManager>.instance.service.SetLobbyAttribute("customData", customData);
		}
		NetworkedManagerBase<GameResultManager>.instance.onUpdateGameResult += new Action<DewGameResult>(OnUpdateGameResult);
		Quest_ShapeOfDreams quest_ShapeOfDreams = Dew.FindActorOfType<Quest_ShapeOfDreams>();
		if (!quest_ShapeOfDreams.IsNullOrInactive())
		{
			quest_ShapeOfDreams.Destroy();
		}
		if (isNewInstance && (UnityEngine.Object)(object)NetworkedManagerBase<QuestManager>.instance.activeQuests.Find((DewQuest q) => q is Quest_Limbo) == null)
		{
			NetworkedManagerBase<QuestManager>.instance.StartQuest<Quest_Limbo>();
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (((NetworkBehaviour)this).isServer)
		{
			EnforceGameRules();
		}
	}

	private void OnUpdateGameResult(DewGameResult obj)
	{
		obj.limboDepth = depth;
	}

	private void ClientEventOnGameConcluded(DewGameResult obj)
	{
		if (!obj.result.IsWin())
		{
			return;
		}
		DewGameResult.PlayerData playerData = obj.players.Find((DewGameResult.PlayerData p) => p.isLocalPlayer);
		if (playerData != null)
		{
			string heroType = playerData.heroType;
			if (DewSave.profileStats.heroes.TryGetValue(heroType, out var value) && value.completedLimboDepth < depth)
			{
				value.completedLimboDepth = depth;
				ManagerBase<AchievementManager>.instance.LocalClientEvent_OnLimboDepthComplete?.Invoke(depth, heroType);
				DewSave.profileStats.UpdateTotalData(0L);
				DewSave.SaveProfileStats();
			}
		}
	}

	private void OnWorldGenerated()
	{
		for (int i = 0; i < NetworkedManagerBase<ZoneManager>.instance.nodes.Count; i++)
		{
			WorldNodeData worldNodeData = NetworkedManagerBase<ZoneManager>.instance.nodes[i];
			if (!worldNodeData.HasModifier<RoomMod_Limbo_Decorator>() && worldNodeData.type != WorldNodeType.ExitBoss)
			{
				NetworkedManagerBase<ZoneManager>.instance.AddModifier<RoomMod_Limbo_Decorator>(i);
			}
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if ((UnityEngine.Object)(object)NetworkedManagerBase<GameResultManager>.instance != null)
		{
			NetworkedManagerBase<GameResultManager>.instance.ClientEvent_OnGameConcluded -= new Action<DewGameResult>(ClientEventOnGameConcluded);
			NetworkedManagerBase<GameResultManager>.instance.onUpdateGameResult -= new Action<DewGameResult>(OnUpdateGameResult);
		}
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		if ((UnityEngine.Object)(object)NetworkedManagerBase<ActorManager>.instance != null)
		{
			NetworkedManagerBase<ActorManager>.instance.ClientEvent_OnActorAdd -= new Action<Actor>(OnActorAdd);
		}
		if ((UnityEngine.Object)(object)NetworkedManagerBase<ZoneManager>.instance != null)
		{
			if (depth >= 1)
			{
				NetworkedManagerBase<ZoneManager>.instance.hunterSpreadMultiplier /= 1f + d1HuntSpeedAmp;
			}
			if (depth >= 4)
			{
				NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnRoomLoaded -= new Action<EventInfoLoadRoom>(AddMirageSkinChanceToCurrentRoom);
			}
			NetworkedManagerBase<ZoneManager>.instance.onWorldGenerated -= new Action(OnWorldGenerated);
		}
		if ((UnityEngine.Object)(object)NetworkedManagerBase<GameManager>.instance != null && depth >= 3)
		{
			GameManager gameManager = NetworkedManagerBase<GameManager>.instance;
			gameManager.NetworkgoldCostMultiplier = gameManager.goldCostMultiplier / (1f + d3GoldPriceAmp);
			GameManager gameManager2 = NetworkedManagerBase<GameManager>.instance;
			gameManager2.NetworkgainedExpMultiplier = gameManager2.gainedExpMultiplier / (1f - d3ExpReduction);
		}
	}

	private void AddMirageSkinChanceToCurrentRoom(EventInfoLoadRoom obj)
	{
		SingletonDewNetworkBehaviour<Room>.instance.monsters.addedMirageChance += d4MirageSkinChance;
	}

	private void OnActorAdd(Actor obj)
	{
		Monster m = obj as Monster;
		if (m != null)
		{
			m.Status.AddStatBonus(GetStatBonus(m));
			if (m.IsAnyBoss() && !m.Status.HasStatusEffect<Se_LimboBossDecorator>())
			{
				m.CreateStatusEffect<Se_LimboBossDecorator>(m, new CastInfo(m));
			}
			if (m.IsAnyBoss() && depth >= 6)
			{
				m.ActorEvent_OnDealDamage += (Action<EventInfoDamage>)((EventInfoDamage eventInfoDamage) =>
				{
					if (!(eventInfoDamage.actor is Se_Elm_Fire) && eventInfoDamage.victim is Hero hero2)
					{
						int num = (hero2.IsMeleeHero() ? 30 : 90);
						if (eventInfoDamage.victim.Status.TryGetStatusEffect<Se_MirageSkin_Delusion_Delusional>(out var effect))
						{
							effect.AddStack(num);
							effect.ResetTimer();
							effect.FxPlayNetworked(effect.fxStackUpdated, eventInfoDamage.victim);
						}
						else
						{
							m.CreateStatusEffect<Se_MirageSkin_Delusion_Delusional>(eventInfoDamage.victim, new CastInfo(eventInfoDamage.victim)).SetStack(num);
						}
					}
				});
			}
		}
		ElementalStatusEffect se = obj as ElementalStatusEffect;
		if (se != null)
		{
			Dew.CallDelayed(() =>
			{
				if (depth >= 4)
				{
					se.decayTime *= 1f + d4ElementalDurationAmp;
				}
			});
		}
		if (obj is Quest_ShapeOfDreams)
		{
			obj.Destroy();
		}
		if (!(obj is Hero hero) || depth < 3)
		{
			return;
		}
		hero.takenShieldProcessor.Add(delegate(ref HealData data, Actor from, Entity to)
		{
			if (!data.IsAmountModifiedBy(this))
			{
				data.SetAmountModifiedBy(this);
				data.ApplyReduction(d3ShieldReduction);
			}
		});
	}

	private StatBonus GetStatBonus(Monster m)
	{
		StatBonus statBonus = new StatBonus();
		if (m.IsAnyBoss())
		{
			if (depth >= 5)
			{
				statBonus.attackSpeedPercentage = d5BossAtkSpd;
				statBonus.movementSpeedPercentage = d5BossMovSpd;
			}
		}
		else
		{
			statBonus.attackSpeedPercentage = Mathf.Min(atkMovSpdPerDepth * (float)depth, atkMovSpdPerDepthMax);
			statBonus.movementSpeedPercentage = statBonus.attackSpeedPercentage;
		}
		if (depth >= 1)
		{
			statBonus.abilityHasteFlat = d1SkillHaste;
		}
		if (depth >= 2)
		{
			statBonus.attackDamagePercentage = d2DamageAmp * 100f;
			statBonus.abilityPowerPercentage = d2DamageAmp * 100f;
			statBonus.armorFlat = d2Armor;
		}
		return statBonus;
	}

	public override void OnStartServerLobby()
	{
		base.OnStartServerLobby();
		if (DewNetworkManager.startSettings.continueData == null && Lobby_GetDepth() == -1)
		{
			Lobby_SetDepth(1);
		}
		ManagerBase<PlayLobbyManager>.instance.AddStartGameCondition(() => (Lobby_GetLimboDepthState(Lobby_GetDepth()) == LimboDepthState.Locked) ? Lobby_GetLimboDepthUnavailableReason(Lobby_GetDepth()) : null);
		ManagerBase<PlayLobbyManager>.instance.AddStartGameCondition(() =>
		{
			if (Lobby_GetLimboDepthState(Lobby_GetDepth()) == LimboDepthState.Locked)
			{
				return Lobby_GetLimboDepthUnavailableReason(Lobby_GetDepth());
			}
			int num = ((IEnumerable<string>)NetworkedManagerBase<GameSettingsManager>.instance.activeLucidDreams).Count((string l) =>
			{
				LucidDream byShortTypeName = DewResources.GetByShortTypeName<LucidDream>(l, ResourceLoadSettings.Light);
				return (UnityEngine.Object)(object)byShortTypeName != null && byShortTypeName.type == LucidDreamType.Evil;
			});
			return (num != Lobby_GetDepth()) ? string.Format("{0} ({1}/{2})", DewLocalization.GetUIValue("Limbo_NeedToEnableEvilLucidDreams"), num, Lobby_GetDepth()) : null;
		});
		NetworkedManagerBase<GameSettingsManager>.instance.difficulty = "diffLimbo";
		NetworkedManagerBase<GameSettingsManager>.instance.allowDejavu = false;
		NetworkedManagerBase<GameSettingsManager>.instance.bannedGameItems.Clear();
		if (NetworkedManagerBase<GameSettingsManager>.instance.allowMidJoins == AllowMidJoinType.AllowAll)
		{
			NetworkedManagerBase<GameSettingsManager>.instance.allowMidJoins = AllowMidJoinType.RejoinOnly;
		}
		ManagerBase<PlayLobbyManager>.instance.gameObject.AddComponent<CustomLogicBehavior>().onLogicUpdate = () =>
		{
			EnforceGameRules();
		};
	}

	private void EnforceGameRules()
	{
		if (!NetworkServer.active)
		{
			return;
		}
		if (NetworkedManagerBase<GameSettingsManager>.instance.allowDejavu)
		{
			NetworkedManagerBase<GameSettingsManager>.instance.allowDejavu = false;
			ManagerBase<MessageManager>.instance.ShowMessageLocalized("Limbo_SettingsError_CantUseDejavu");
		}
		if (NetworkedManagerBase<GameSettingsManager>.instance.allowMidJoins == AllowMidJoinType.AllowAll)
		{
			NetworkedManagerBase<GameSettingsManager>.instance.allowMidJoins = AllowMidJoinType.RejoinOnly;
			ManagerBase<MessageManager>.instance.ShowMessageLocalized("Limbo_SettingsError_NewPlayerCantMidJoin");
		}
		if (NetworkedManagerBase<GameSettingsManager>.instance.bannedGameItems.Count > 0)
		{
			NetworkedManagerBase<GameSettingsManager>.instance.bannedGameItems.Clear();
		}
		SyncList<string> activeLucidDreams = NetworkedManagerBase<GameSettingsManager>.instance.activeLucidDreams;
		for (int num = activeLucidDreams.Count - 1; num >= 0; num--)
		{
			LucidDream byShortTypeName = DewResources.GetByShortTypeName<LucidDream>(activeLucidDreams[num], ResourceLoadSettings.Light);
			if (!((UnityEngine.Object)(object)byShortTypeName == null) && byShortTypeName.type != LucidDreamType.Evil)
			{
				activeLucidDreams.RemoveAt(num);
			}
		}
	}

	public override void OnStartClientLobby()
	{
		base.OnStartClientLobby();
		NotifyLimbo();
		if (!IsLimboUnlocked())
		{
			DewNetworkManager.instance.didRegisterError = true;
			ManagerBase<MessageManager>.instance.ShowMessage(new DewMessageSettings
			{
				rawContent = GetInsufficientRequirementMessage(),
				buttons = DewMessageSettings.ButtonType.Ok
			});
			DewNetworkManager.instance.EndSession();
		}
	}

	public static string GetDepthLocalized(int depth)
	{
		return string.Format(DewLocalization.GetUIValue("Limbo_DepthTemplate"), depth);
	}

	public static int Lobby_GetDepth()
	{
		if ((UnityEngine.Object)(object)NetworkedManagerBase<GameSettingsManager>.instance == null)
		{
			return -1;
		}
		return int.Parse(CollectionExtensions.GetValueOrDefault<string, string>((IReadOnlyDictionary<string, string>)NetworkedManagerBase<GameSettingsManager>.instance.customData, "GameMod_Limbo::depth", "-1"), CultureInfo.InvariantCulture);
	}

	public static int Lobby_GetDepth(LobbyInstance lobby)
	{
		if (lobby.customData.TryGetValue("GameMod_Limbo::depth", out var value) && int.TryParse(value, out var result))
		{
			return result;
		}
		return -1;
	}

	public static void Lobby_SetDepth(int depth)
	{
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		if ((UnityEngine.Object)(object)NetworkedManagerBase<GameSettingsManager>.instance == null)
		{
			return;
		}
		((SyncIDictionary<string, string>)(object)NetworkedManagerBase<GameSettingsManager>.instance.customData)["GameMod_Limbo::depth"] = depth.ToString(CultureInfo.InvariantCulture);
		if (ManagerBase<LobbyManager>.instance.isLobbyLeader)
		{
			Dictionary<string, string> dictionary = ManagerBase<LobbyManager>.instance.service.currentLobby.customData;
			if (dictionary == null)
			{
				dictionary = new Dictionary<string, string>();
			}
			dictionary["GameMod_Limbo::depth"] = depth.ToString(CultureInfo.InvariantCulture);
			ManagerBase<LobbyManager>.instance.service.SetLobbyAttribute("customData", dictionary);
		}
	}

	public static LimboDepthState Lobby_GetLimboDepthState(int depth)
	{
		foreach (DewPlayer allHumanPlayer in DewPlayer.allHumanPlayers)
		{
			if (!string.IsNullOrEmpty(allHumanPlayer.selectedHeroType) && allHumanPlayer.isEveryInfoSet && allHumanPlayer.profileStats.heroes.TryGetValue(allHumanPlayer.selectedHeroType, out var value) && depth > value.completedLimboDepth + 1)
			{
				return LimboDepthState.Locked;
			}
		}
		if (DewSave.profileStats.heroes.TryGetValue(DewPlayer.local.selectedHeroType, out var value2) && value2.completedLimboDepth >= depth)
		{
			return LimboDepthState.Completed;
		}
		return LimboDepthState.Available;
	}

	public static string Lobby_GetLimboDepthUnavailableReason(int depth)
	{
		string text = DewLocalization.GetUIValue("Limbo_NeedToCompletePreviousDepth") ?? "";
		string uIValue = DewLocalization.GetUIValue("Limbo_DepthTemplate");
		foreach (DewPlayer allHumanPlayer in DewPlayer.allHumanPlayers)
		{
			if (!string.IsNullOrEmpty(allHumanPlayer.selectedHeroType) && allHumanPlayer.isEveryInfoSet && allHumanPlayer.profileStats.heroes.TryGetValue(allHumanPlayer.selectedHeroType, out var value) && depth > value.completedLimboDepth + 1)
			{
				string text2 = "<color=" + ChatManager.GetPlayerColorHex(allHumanPlayer) + ">" + allHumanPlayer.playerName + " (" + DewLocalization.GetUIValue(allHumanPlayer.selectedHeroType + "_Name") + ")</color>";
				text = text + "\n" + text2 + " - " + string.Format(uIValue, value.completedLimboDepth);
			}
		}
		return text;
	}

	public static string GetInsufficientRequirementMessage()
	{
		string uIValue = DewLocalization.GetUIValue("GameMode_Limbo_RequirementMessage");
		uIValue = string.Format(uIValue, GetRequiredUnlockedEvilLucidDreams());
		return $"{uIValue} ({GetCurrentUnlockedEvilLucidDreams()}/{GetRequiredUnlockedEvilLucidDreams()})";
	}

	public static int GetRequiredUnlockedEvilLucidDreams()
	{
		return 4;
	}

	public static int GetMaxDepths()
	{
		return 6;
	}

	public static int GetCurrentUnlockedEvilLucidDreams()
	{
		int num = 0;
		foreach (Type allLucidDream in Dew.allLucidDreams)
		{
			if (Dew.IsLucidDreamIncludedInGame(allLucidDream.Name))
			{
				LucidDream byType = DewResources.GetByType<LucidDream>(allLucidDream, ResourceLoadSettings.Light);
				if (!((UnityEngine.Object)(object)byType == null) && byType.type == LucidDreamType.Evil && DewSave.profileMain.lucidDreams.TryGetValue(allLucidDream.Name, out var value) && value.isAvailableInGame)
				{
					num++;
				}
			}
		}
		return num;
	}

	public static bool IsLimboUnlocked()
	{
		return GetCurrentUnlockedEvilLucidDreams() >= GetRequiredUnlockedEvilLucidDreams();
	}

	private void NotifyLimbo()
	{
		ILimboNotifyReceiver[] array = Dew.FindInterfacesOfType<ILimboNotifyReceiver>(includeInactive: true);
		foreach (ILimboNotifyReceiver limboNotifyReceiver in array)
		{
			try
			{
				limboNotifyReceiver.OnLimboGame();
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
	}

	private void MirrorProcessed()
	{
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			NetworkWriterExtensions.WriteInt(writer, depth);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 8L) != 0L)
		{
			NetworkWriterExtensions.WriteInt(writer, depth);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref depth, (Action<int, int>)null, NetworkReaderExtensions.ReadInt(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 8L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref depth, (Action<int, int>)null, NetworkReaderExtensions.ReadInt(reader));
		}
	}
}
