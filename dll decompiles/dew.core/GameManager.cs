using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;
using UnityEngine.Serialization;

public class GameManager : NetworkedManagerBase<GameManager>
{
	[FormerlySerializedAs("gss")]
	public DewGameplayExperienceSettings gesTemplate;

	public DewGameplayExperienceSettings ges;

	[SyncVar(hook = "OnDifficultyChanged")]
	private SyncableAssetRef _difficulty;

	public bool disableStuckCheck;

	public SafeAction<DewDifficultySettings, DewDifficultySettings> ClientEvent_OnDifficultyChanged;

	public SafeAction<int, int> ClientEvent_OnAmbientLevelChanged;

	public SafeAction ClientEvent_OnGameConcluded;

	[NonSerialized]
	public string gameOverSubtitleOverride;

	[CompilerGenerated]
	[SyncVar(hook = "OnAmbientLevelChanged")]
	private int ambientLevel__BackingField;

	[CompilerGenerated]
	[SyncVar(hook = "OnIsGameTimePausedChanged")]
	private bool isGameTimePausedByAfk__BackingField;

	[CompilerGenerated]
	[SyncVar(hook = "OnIsGameTimePausedChanged")]
	private bool isGameTimePausedByGame__BackingField;

	private bool _wasGameTimePaused;

	[SyncVar]
	private float _gameTimeBase;

	[SyncVar]
	private float _gameTimeTickStartTime;

	[CompilerGenerated]
	[SyncVar(hook = "OnIsGameConcludedChanged")]
	private bool isGameConcluded__BackingField;

	[CompilerGenerated]
	[SyncVar]
	private float maxAndSpawnedPopulationMultiplier__BackingField = 1f;

	[CompilerGenerated]
	[SyncVar]
	private float spawnedPopulation__BackingField;

	private bool _isSpawnedPopulationDirty;

	[CompilerGenerated]
	[SyncVar]
	private bool isGameOverEnabled__BackingField = true;

	[CompilerGenerated]
	[SyncVar]
	private string runId__BackingField;

	[SaveVar(SaveVarFlags.Default)]
	public Dictionary<string, DewPersistence.PlayerData> playerRejoinData = new Dictionary<string, DewPersistence.PlayerData>();

	private float _gameOverTime;

	private float _lastContinueSaveUnscaledTime = float.NegativeInfinity;

	[SyncVar]
	private int _lockMidRunCounter;

	public Func<float> predictionStrengthOverride;

	[NonSerialized]
	[SyncVar]
	public float gainedExpMultiplier = 1f;

	[NonSerialized]
	[SyncVar]
	public float goldCostMultiplier = 1f;

	[NonSerialized]
	[SyncVar]
	public float goldIncomeMultiplier = 1f;

	private static List<Action> _lazyCalledFunctions;

	public Action<SyncableAssetRef, SyncableAssetRef> _Mirror_SyncVarHookDelegate__difficulty;

	public Action<int, int> _Mirror_SyncVarHookDelegate__003CambientLevel_003Ek__BackingField;

	public Action<bool, bool> _Mirror_SyncVarHookDelegate__003CisGameTimePausedByAfk_003Ek__BackingField;

	public Action<bool, bool> _Mirror_SyncVarHookDelegate__003CisGameTimePausedByGame_003Ek__BackingField;

	public Action<bool, bool> _Mirror_SyncVarHookDelegate__003CisGameConcluded_003Ek__BackingField;

	[SaveVar(SaveVarFlags.Default)]
	public DewDifficultySettings difficulty
	{
		get
		{
			return _difficulty.asset as DewDifficultySettings;
		}
		set
		{
			Network_difficulty = value;
		}
	}

	[SaveVar(SaveVarFlags.Default)]
	public int ambientLevel
	{
		[CompilerGenerated]
		get
		{
			return ambientLevel__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003CambientLevel_003Ek__BackingField = value;
		}
	}

	public bool isGameTimePaused
	{
		get
		{
			if (!isGameTimePausedByAfk)
			{
				return isGameTimePausedByGame;
			}
			return true;
		}
	}

	public bool isGameTimePausedByAfk
	{
		[CompilerGenerated]
		get
		{
			return isGameTimePausedByAfk__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003CisGameTimePausedByAfk_003Ek__BackingField = value;
		}
	}

	public bool isGameTimePausedByGame
	{
		[CompilerGenerated]
		get
		{
			return isGameTimePausedByGame__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003CisGameTimePausedByGame_003Ek__BackingField = value;
		}
	}

	[SaveVar(SaveVarFlags.Default)]
	public float elapsedGameTime
	{
		get
		{
			return _gameTimeBase + ((!isGameTimePaused) ? ((float)NetworkTime.time - _gameTimeTickStartTime) : 0f);
		}
		set
		{
			Network_gameTimeBase = value;
			Network_gameTimeTickStartTime = (float)NetworkTime.time;
		}
	}

	public bool isGameConcluded
	{
		[CompilerGenerated]
		get
		{
			return isGameConcluded__BackingField;
		}
		[CompilerGenerated]
		private set
		{
			Network_003CisGameConcluded_003Ek__BackingField = value;
		}
	}

	public float maxAndSpawnedPopulationMultiplier
	{
		[CompilerGenerated]
		get
		{
			return maxAndSpawnedPopulationMultiplier__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003CmaxAndSpawnedPopulationMultiplier_003Ek__BackingField = value;
		}
	}

	public float spawnedPopulation
	{
		[CompilerGenerated]
		get
		{
			return spawnedPopulation__BackingField;
		}
		[CompilerGenerated]
		internal set
		{
			Network_003CspawnedPopulation_003Ek__BackingField = value;
		}
	}

	public float maxSpawnedPopulation
	{
		get
		{
			float maxGlobalPopulation = ges.maxGlobalPopulation;
			maxGlobalPopulation *= 1f + (ges.maxGlobalPopulationMultiplierPerPlayer - 1f) * GetMultiplayerDifficultyFactor(reduceWhenDead: true) + (ges.maxGlobalPopulationMultiplierByAmbientLevel.Evaluate(ambientLevel - 1) - 1f);
			maxGlobalPopulation *= difficulty.maxPopulationMultiplier;
			if ((UnityEngine.Object)(object)SingletonDewNetworkBehaviour<Room>.softInstance != null)
			{
				maxGlobalPopulation *= SingletonDewNetworkBehaviour<Room>.softInstance.monsters.maxPopulationMultiplier;
			}
			return maxGlobalPopulation * maxAndSpawnedPopulationMultiplier;
		}
	}

	public bool isSpawnOverPopulation => spawnedPopulation >= maxSpawnedPopulation;

	public bool isGameOverEnabled
	{
		[CompilerGenerated]
		get
		{
			return isGameOverEnabled__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003CisGameOverEnabled_003Ek__BackingField = value;
		}
	}

	[SaveVar(SaveVarFlags.Default)]
	public string runId
	{
		[CompilerGenerated]
		get
		{
			return runId__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003CrunId_003Ek__BackingField = value;
		}
	}

	public bool isMidRunSaveLocked => _lockMidRunCounter > 0;

	public SyncableAssetRef Network_difficulty
	{
		get
		{
			return _difficulty;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<SyncableAssetRef>(value, ref _difficulty, 1uL, _Mirror_SyncVarHookDelegate__difficulty);
		}
	}

	public int Network_003CambientLevel_003Ek__BackingField
	{
		get
		{
			return ambientLevel__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<int>(value, ref ambientLevel__BackingField, 2uL, _Mirror_SyncVarHookDelegate__003CambientLevel_003Ek__BackingField);
		}
	}

	public bool Network_003CisGameTimePausedByAfk_003Ek__BackingField
	{
		get
		{
			return isGameTimePausedByAfk__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<bool>(value, ref isGameTimePausedByAfk__BackingField, 4uL, _Mirror_SyncVarHookDelegate__003CisGameTimePausedByAfk_003Ek__BackingField);
		}
	}

	public bool Network_003CisGameTimePausedByGame_003Ek__BackingField
	{
		get
		{
			return isGameTimePausedByGame__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<bool>(value, ref isGameTimePausedByGame__BackingField, 8uL, _Mirror_SyncVarHookDelegate__003CisGameTimePausedByGame_003Ek__BackingField);
		}
	}

	public float Network_gameTimeBase
	{
		get
		{
			return _gameTimeBase;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref _gameTimeBase, 16uL, (Action<float, float>)null);
		}
	}

	public float Network_gameTimeTickStartTime
	{
		get
		{
			return _gameTimeTickStartTime;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref _gameTimeTickStartTime, 32uL, (Action<float, float>)null);
		}
	}

	public bool Network_003CisGameConcluded_003Ek__BackingField
	{
		get
		{
			return isGameConcluded__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<bool>(value, ref isGameConcluded__BackingField, 64uL, _Mirror_SyncVarHookDelegate__003CisGameConcluded_003Ek__BackingField);
		}
	}

	public float Network_003CmaxAndSpawnedPopulationMultiplier_003Ek__BackingField
	{
		get
		{
			return maxAndSpawnedPopulationMultiplier__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref maxAndSpawnedPopulationMultiplier__BackingField, 128uL, (Action<float, float>)null);
		}
	}

	public float Network_003CspawnedPopulation_003Ek__BackingField
	{
		get
		{
			return spawnedPopulation__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref spawnedPopulation__BackingField, 256uL, (Action<float, float>)null);
		}
	}

	public bool Network_003CisGameOverEnabled_003Ek__BackingField
	{
		get
		{
			return isGameOverEnabled__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<bool>(value, ref isGameOverEnabled__BackingField, 512uL, (Action<bool, bool>)null);
		}
	}

	public string Network_003CrunId_003Ek__BackingField
	{
		get
		{
			return runId__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<string>(value, ref runId__BackingField, 1024uL, (Action<string, string>)null);
		}
	}

	public int Network_lockMidRunCounter
	{
		get
		{
			return _lockMidRunCounter;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<int>(value, ref _lockMidRunCounter, 2048uL, (Action<int, int>)null);
		}
	}

	public float NetworkgainedExpMultiplier
	{
		get
		{
			return gainedExpMultiplier;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref gainedExpMultiplier, 4096uL, (Action<float, float>)null);
		}
	}

	public float NetworkgoldCostMultiplier
	{
		get
		{
			return goldCostMultiplier;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref goldCostMultiplier, 8192uL, (Action<float, float>)null);
		}
	}

	public float NetworkgoldIncomeMultiplier
	{
		get
		{
			return goldIncomeMultiplier;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref goldIncomeMultiplier, 16384uL, (Action<float, float>)null);
		}
	}

	private void OnIsGameConcludedChanged(bool oldVal, bool newVal)
	{
		if (newVal)
		{
			ClientEvent_OnGameConcluded?.Invoke();
		}
	}

	[Server]
	public void UpdateSpawnedPopulation()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void GameManager::UpdateSpawnedPopulation()' called when server was not active");
			return;
		}
		_isSpawnedPopulationDirty = false;
		float num = 0f;
		foreach (Entity allEntity in NetworkedManagerBase<ActorManager>.instance.allEntities)
		{
			if (allEntity is Monster { campPosition: null } monster && !monster.IsNullInactiveDeadOrKnockedOut())
			{
				num += monster.populationCost;
			}
		}
		Network_003CspawnedPopulation_003Ek__BackingField = num;
	}

	[Server]
	public void MarkSpawnedPopulationDirty()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void GameManager::MarkSpawnedPopulationDirty()' called when server was not active");
		}
		else
		{
			_isSpawnedPopulationDirty = true;
		}
	}

	public override void FrameUpdate()
	{
		base.FrameUpdate();
		if (((NetworkBehaviour)this).isServer && _isSpawnedPopulationDirty)
		{
			UpdateSpawnedPopulation();
		}
	}

	protected override void Awake()
	{
		base.Awake();
		if (NetworkServer.active)
		{
			difficulty = GetDifficulty();
		}
		ResetGameplayExperienceSettings();
	}

	public void ResetGameplayExperienceSettings()
	{
		if ((UnityEngine.Object)(object)ges != null)
		{
			UnityEngine.Object.DestroyImmediate((UnityEngine.Object)(object)ges);
			ges = null;
		}
		if ((UnityEngine.Object)(object)gesTemplate != null)
		{
			ges = UnityEngine.Object.Instantiate<DewGameplayExperienceSettings>(gesTemplate);
		}
		else
		{
			ges = UnityEngine.Object.Instantiate<DewGameplayExperienceSettings>(Resources.Load<DewGameplayExperienceSettings>("GSS/GSS - Default"));
		}
	}

	protected virtual DewDifficultySettings GetDifficulty()
	{
		return null;
	}

	public virtual IList<string> GetLucidDreams()
	{
		return (IList<string>)NetworkedManagerBase<GameSettingsManager>.instance.activeLucidDreams;
	}

	public override void OnStartServer()
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		base.OnStartServer();
		NetworkedManagerBase<ActorManager>.instance.ClientEvent_OnEntityAdd += (Action<Entity>)((Entity e) =>
		{
			difficulty.ApplyDifficultyModifiers(e);
		});
		EntityAI.DisableAI = false;
		if (ManagerBase<LobbyManager>.instance.isLobbyLeader)
		{
			NetworkedManagerBase<GameSettingsManager>.instance.Lobby_UpdateGameStartTimestamp();
			NetworkedManagerBase<GameSettingsManager>.instance.Lobby_UpdateCanJoinAndGameStarted();
		}
		NetworkedManagerBase<ClientEventManager>.instance.OnHeroKnockedOut += (Action<Hero>)((Hero h) =>
		{
			CollectPlayerRejoinData(h.owner);
		});
		NetworkedManagerBase<GameResultManager>.instance.ClientEvent_OnGameConcluded += (Action<DewGameResult>)((DewGameResult _) =>
		{
			if (DewSave.profileContinue.continueData != null)
			{
				Debug.Log("Game ended. Clearing continue data");
				DewSave.profileContinue.continueData = null;
				DewSave.SaveProfileContinue();
			}
		});
		if (DewNetworkManager.startSettings.continueData == null)
		{
			SetAmbientLevel(1);
			Network_gameTimeBase = 0f;
			Network_gameTimeTickStartTime = (float)NetworkTime.time;
			SetupAIDifficulty();
			Network_003CrunId_003Ek__BackingField = Guid.NewGuid().ToString();
			Enumerator<string> enumerator = NetworkedManagerBase<GameSettingsManager>.instance.addedGameMods.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					string current = enumerator.Current;
					if (!string.IsNullOrEmpty(current))
					{
						GameModifierBase byShortTypeName = DewResources.GetByShortTypeName<GameModifierBase>(current, default(ResourceLoadSettings));
						if (!((UnityEngine.Object)(object)byShortTypeName == null))
						{
							Dew.CreateActor(byShortTypeName, Vector3.zero, Quaternion.identity);
						}
					}
				}
			}
			finally
			{
				((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
			}
			foreach (string lucidDream in GetLucidDreams())
			{
				LucidDream byShortTypeName2 = DewResources.GetByShortTypeName<LucidDream>(lucidDream, default(ResourceLoadSettings));
				if (!((UnityEngine.Object)(object)byShortTypeName2 == null))
				{
					Debug.Log("Creating " + ((object)byShortTypeName2).GetType().Name);
					Dew.CreateActor(byShortTypeName2, Vector3.zero, Quaternion.identity);
				}
			}
			foreach (DewPlayer gamePlayer in DewPlayer.gamePlayers)
			{
				if (DewBuildProfile.current.startGold > 0)
				{
					gamePlayer.EarnGold(DewBuildProfile.current.startGold);
				}
				if (DewBuildProfile.current.startDreamDust > 0)
				{
					gamePlayer.EarnDreamDust(DewBuildProfile.current.startDreamDust);
				}
			}
		}
		((MonoBehaviour)(object)this).StartCoroutine(AfkCheckRoutine());
		IEnumerator AfkCheckRoutine()
		{
			WaitForSeconds waitActive = new WaitForSeconds(0.01f);
			WaitForSeconds waitIdle = new WaitForSeconds(2.5f);
			while (true)
			{
				yield return isGameTimePausedByAfk ? waitActive : waitIdle;
				bool flag = true;
				foreach (DewPlayer gamePlayer2 in DewPlayer.gamePlayers)
				{
					if (!gamePlayer2.hero.IsNullOrInactive() && Time.time - gamePlayer2.hero.Control.lastMoveTime < 120f)
					{
						flag = false;
						break;
					}
				}
				if (isGameTimePausedByAfk != flag)
				{
					Network_003CisGameTimePausedByAfk_003Ek__BackingField = flag;
				}
			}
		}
	}

	public void CollectAllPlayersRejoinData()
	{
		foreach (DewPlayer gamePlayer in DewPlayer.gamePlayers)
		{
			CollectPlayerRejoinData(gamePlayer);
		}
	}

	public void CollectPlayerRejoinData(DewPlayer p)
	{
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			playerRejoinData[p.guid] = DewPersistence.SerializePlayerData(p);
			if (ManagerBase<LobbyManager>.instance.isLobbyLeader)
			{
				List<string> list = playerRejoinData.Keys.ToList();
				list.Sort();
				if (ManagerBase<LobbyManager>.instance.service.currentLobby.savedPlayers == null || !ManagerBase<LobbyManager>.instance.service.currentLobby.savedPlayers.SequenceEqual(list))
				{
					ManagerBase<LobbyManager>.instance.service.SetLobbyAttribute("savedPlayers", list);
				}
			}
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	public override void OnStart()
	{
		base.OnStart();
		CallOnReady(() =>
		{
			NetworkedManagerBase<ConsoleManager>.instance.ExecuteAutoExec(ConsoleManager.AutoExecKey.Game);
			NetworkedManagerBase<ConsoleManager>.instance.ExecuteAutoExec(((NetworkBehaviour)this).isServer ? ConsoleManager.AutoExecKey.GameServer : ConsoleManager.AutoExecKey.GameClient);
		});
		DewResources.AddPreloadRule((MonoBehaviour)(object)this, (PreloadInterface preload) =>
		{
			//IL_007d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0082: Unknown result type (might be due to invalid IL or missing references)
			HashSet<string> hashSet = new HashSet<string>();
			foreach (Entity allEntity in NetworkedManagerBase<ActorManager>.instance.allEntities)
			{
				if ((bool)allEntity.Visual.model && allEntity.Visual.model.name.StartsWith("Skin_"))
				{
					hashSet.Add(Dew.GetOriginalName(allEntity.Visual.model.name));
				}
				if (allEntity is Hero hero)
				{
					Enumerator<string> enumerator2 = hero.accessories.GetEnumerator();
					try
					{
						while (enumerator2.MoveNext())
						{
							string current2 = enumerator2.Current;
							hashSet.Add(current2);
						}
					}
					finally
					{
						((IDisposable)enumerator2/*cast due to constrained. prefix*/).Dispose();
					}
				}
			}
			foreach (string loadedGuid in DewResources.loadedGuids)
			{
				string value2;
				if (DewResources.database.guidToType.TryGetValue(loadedGuid, out var value))
				{
					string name = value.Name;
					if (!name.StartsWith("Gem_") && !name.StartsWith("Mon_") && !name.StartsWith("St_") && !name.StartsWith("Hero_") && !name.StartsWith("At_") && !name.StartsWith("Artifact_") && !name.StartsWith("RoomMod_") && !name.StartsWith("GameMod_") && !name.StartsWith("Ai_") && !name.StartsWith("Se_") && !name.StartsWith("Ge_"))
					{
						preload.AddGuid(loadedGuid);
					}
				}
				else if (DewResources.database.guidToName.TryGetValue(loadedGuid, out value2))
				{
					if (value2.StartsWith("Skin_") || value2.StartsWith("Acc_"))
					{
						if (hashSet.Contains(value2))
						{
							preload.AddGuid(loadedGuid);
						}
					}
					else if (!value2.StartsWith("Nametag_") && !value2.StartsWith("Emote_"))
					{
						preload.AddGuid(loadedGuid);
					}
				}
				else
				{
					preload.AddGuid(loadedGuid);
				}
			}
		});
		DewResources.AddPreloadRule((MonoBehaviour)(object)this, (PreloadInterface preload) =>
		{
			preload.AddType("MockAbilityInstance");
			preload.AddType("Se_GenericEffectContainer");
			preload.AddType("Se_Elm_Fire");
			preload.AddType("Se_Elm_Dark");
			preload.AddType("Se_Elm_Cold");
			preload.AddType("Se_Elm_Light");
			preload.AddType("Se_HealthCost");
			preload.AddType("Se_HunterBuff");
			preload.AddType("Se_InConversation");
			preload.AddType("Se_PortalTransition");
			preload.AddType("Se_BarrierPassThrough");
			preload.AddType("Se_GenericShield_Stacking");
			preload.AddType("Se_GenericShield_OneShot");
			preload.AddType("Se_HeroKnockedOut");
			preload.AddType("Ai_HunterArtillery_Small");
			preload.AddType("Ai_HunterArtillery_Big");
			preload.AddType("Pickup_LargeGoldOrb");
			preload.AddType("Pickup_MediumGoldOrb");
			preload.AddType("Pickup_SmallGoldOrb");
			preload.AddType("Pickup_LargeExpOrb");
			preload.AddType("Pickup_MediumExpOrb");
			preload.AddType("Pickup_SmallExpOrb");
			preload.AddType("Pickup_RegenOrb");
		});
	}

	public override void LogicUpdate(float dt)
	{
		base.LogicUpdate(dt);
		DoLogicUpdate_LazyCall(dt);
		if (((NetworkBehaviour)this).isServer && !isGameConcluded)
		{
			CheckGameOver(dt);
		}
	}

	private void CheckGameOver(float dt)
	{
		if (DewNetworkManager.instance.hasSessionEnded)
		{
			return;
		}
		if (!isGameOverEnabled || NetworkedManagerBase<ActorManager>.instance.allHeroes.Count == 0 || NetworkedManagerBase<ZoneManager>.instance.isInAnyTransition)
		{
			_gameOverTime = 0f;
			return;
		}
		foreach (Hero allHero in NetworkedManagerBase<ActorManager>.instance.allHeroes)
		{
			if (!allHero.isKnockedOut)
			{
				_gameOverTime = 0f;
				return;
			}
		}
		if (DewSave.profileContinue.continueData != null)
		{
			Debug.Log("No alive heroes. Clearing continue data");
			DewSave.profileContinue.continueData = null;
			DewSave.SaveProfileContinue();
		}
		_gameOverTime += dt;
		if (_gameOverTime >= 4f)
		{
			WrapUpAndShowResult(DewGameResult.ResultType.GameOver);
		}
	}

	[Server]
	public void ConcludeUnknownFate()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void GameManager::ConcludeUnknownFate()' called when server was not active");
			return;
		}
		NetworkedManagerBase<ZoneManager>.instance.DoDeadEndTravel();
		WrapUpAndShowResult(DewGameResult.ResultType.UnknownFate);
	}

	[Server]
	public void ConcludePureWhiteDream()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void GameManager::ConcludePureWhiteDream()' called when server was not active");
		}
		else
		{
			WrapUpAndShowResult(DewGameResult.ResultType.PureWhiteDream);
		}
	}

	[Server]
	public void ConcludeStarlessPath()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void GameManager::ConcludeStarlessPath()' called when server was not active");
		}
		else
		{
			WrapUpAndShowResult(DewGameResult.ResultType.StarlessPath);
		}
	}

	public void WrapUpAndShowResult(DewGameResult.ResultType type)
	{
		if (!isGameConcluded)
		{
			Network_003CisGameConcluded_003Ek__BackingField = true;
			Network_003CisGameTimePausedByGame_003Ek__BackingField = true;
			NetworkedManagerBase<GameSettingsManager>.instance.midJoinBanType = MidJoinBanType.GameHasEnded;
			((MonoBehaviour)(object)this).StartCoroutine(Routine());
		}
		IEnumerator Routine()
		{
			if (type != DewGameResult.ResultType.GameOver && type != DewGameResult.ResultType.PureWhiteDream && type != DewGameResult.ResultType.StarlessPath)
			{
				yield return new WaitForSeconds(1.5f);
			}
			NetworkedManagerBase<GameResultManager>.instance.WrapUp(type);
			do
			{
				yield return new WaitForSecondsRealtime(0.25f);
			}
			while (!DewPlayer.gamePlayers.All((DewPlayer player) => player.isReady));
			if (DewPlayer.gamePlayers.Count <= 1 && DewBuildProfile.current.HasFeature(BuildFeatureTag.Booth))
			{
				DewNetworkManager.instance.EndSession();
			}
			else
			{
				DewNetworkManager.instance.RestartSession();
			}
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		_lazyCalledFunctions.Clear();
		foreach (GameObject item in Dew._destroyingGameObject)
		{
			if (item != null)
			{
				UnityEngine.Object.Destroy(item);
			}
		}
		Dew._destroyingGameObject.Clear();
		Dew.FlushPendingDestroys();
		ActorManager.CleanupActorsBeforeShutdown();
		EffectAutoDestroy.DestroyAll();
	}

	private void OnAmbientLevelChanged(int oldVal, int newVal)
	{
		Debug.Log($"Ambient level is now {newVal}");
		ClientEvent_OnAmbientLevelChanged?.Invoke(oldVal, newVal);
	}

	private void OnIsGameTimePausedChanged(bool oldVal, bool newVal)
	{
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		if (((NetworkBehaviour)this).isServer)
		{
			if (isGameTimePaused && !_wasGameTimePaused)
			{
				Network_gameTimeBase = _gameTimeBase + (float)NetworkTime.time - _gameTimeTickStartTime;
			}
			else if (!isGameTimePaused && _wasGameTimePaused)
			{
				Network_gameTimeTickStartTime = (float)NetworkTime.time;
			}
			if (ManagerBase<LobbyManager>.instance.isLobbyLeader)
			{
				long num = DateTime.UtcNow.AddSeconds(0f - elapsedGameTime).ToTimestamp();
				ManagerBase<LobbyManager>.instance.service.SetLobbyAttribute("gameStartTimestamp", num);
			}
		}
		_wasGameTimePaused = isGameTimePaused;
	}

	public virtual void LoadNextZone()
	{
	}

	private void OnDifficultyChanged(SyncableAssetRef old, SyncableAssetRef newVal)
	{
		Debug.Log("Difficulty is at " + newVal.asset.name + ".");
		SetupAIDifficulty();
		ClientEvent_OnDifficultyChanged?.Invoke(old.asset as DewDifficultySettings, newVal.asset as DewDifficultySettings);
	}

	private void SetupAIDifficulty()
	{
		EntityAI.PositionSampleCount = difficulty.positionSampleCount;
		EntityAI.PositionSampleLagBehindFrames = difficulty.positionSampleLagBehindFrames;
		EntityAI.PositionSampleInterval = difficulty.positionSampleInterval;
	}

	[Server]
	public void SetElapsedGameTime(float newTime)
	{
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void GameManager::SetElapsedGameTime(System.Single)' called when server was not active");
			return;
		}
		Network_gameTimeBase = newTime;
		Network_gameTimeTickStartTime = (float)NetworkTime.time;
		if (ManagerBase<LobbyManager>.instance.isLobbyLeader)
		{
			long num = DateTime.UtcNow.AddSeconds(0f - elapsedGameTime).ToTimestamp();
			ManagerBase<LobbyManager>.instance.service.SetLobbyAttribute("gameStartTimestamp", num);
		}
	}

	[ClientRpc]
	public void SetDisconnectedForEveryoneElse()
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendRPCInternal("System.Void GameManager::SetDisconnectedForEveryoneElse()", -1253838744, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[Server]
	public void SetAmbientLevel(int newLevel)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void GameManager::SetAmbientLevel(System.Int32)' called when server was not active");
		}
		else
		{
			Network_003CambientLevel_003Ek__BackingField = Mathf.Max(newLevel, 1);
		}
	}

	public void SaveContinueData(LoadNodeSettings s)
	{
		if (!IsContinueSaveSupported())
		{
			return;
		}
		try
		{
			if (!isGameConcluded && !DewBuildProfile.current.HasFeature(BuildFeatureTag.Booth))
			{
				CollectAllPlayersRejoinData();
				DewSave.profileContinue.continueData = DewPersistence.ToJson(DewPersistence.SerializeGameData(s));
				DewSave.SaveProfileContinue();
				_lastContinueSaveUnscaledTime = Time.unscaledTime;
				Debug.Log("Continue data saved.");
			}
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	public void SaveContinueDataMidRun()
	{
		if (IsContinueSaveSupported() && !isMidRunSaveLocked)
		{
			DewPersistence.RoomData value = NetworkedManagerBase<ZoneManager>.instance.visitedNodesSaveData[NetworkedManagerBase<ZoneManager>.instance.currentNodeIndex];
			NetworkedManagerBase<ZoneManager>.instance.visitedNodesSaveData[NetworkedManagerBase<ZoneManager>.instance.currentNodeIndex] = DewPersistence.SerializeRoomData();
			SaveContinueData(new LoadNodeSettings
			{
				isLoadingFromSave = true,
				from = -1,
				to = NetworkedManagerBase<ZoneManager>.instance.currentNodeIndex,
				advanceTurn = false,
				isSidetrackTransition = false,
				isTravelingZone = false,
				isTravelingRoom = false
			});
			NetworkedManagerBase<ZoneManager>.instance.visitedNodesSaveData[NetworkedManagerBase<ZoneManager>.instance.currentNodeIndex] = value;
		}
	}

	[Server]
	public void LockMidRunSave()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void GameManager::LockMidRunSave()' called when server was not active");
			return;
		}
		if (Time.time - _lastContinueSaveUnscaledTime > 20f && IsEligibleForMidRunSave())
		{
			SaveContinueDataMidRun();
		}
		Network_lockMidRunCounter = _lockMidRunCounter + 1;
		if (_lockMidRunCounter > 20)
		{
			Debug.LogWarning("Lock mid-run save counter exceeded sane max value of 20: " + _lockMidRunCounter);
		}
	}

	[Server]
	public void UnlockMidRunSave()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void GameManager::UnlockMidRunSave()' called when server was not active");
			return;
		}
		Network_lockMidRunCounter = _lockMidRunCounter - 1;
		if (_lockMidRunCounter < 0)
		{
			Debug.LogWarning("Lock mid-run save counter is below 0: " + _lockMidRunCounter);
		}
	}

	public bool IsEligibleForMidRunSave()
	{
		if (IsContinueSaveSupported() && NetworkServer.active && !isGameConcluded && (UnityEngine.Object)(object)SingletonDewNetworkBehaviour<Room>.instance != null && SingletonDewNetworkBehaviour<Room>.instance.didClearRoom && !SingletonDewNetworkBehaviour<Room>.instance.monsters.isDoingHunterWelcomingSpawn)
		{
			return !isMidRunSaveLocked;
		}
		return false;
	}

	public virtual bool IsContinueSaveSupported()
	{
		return false;
	}

	public float GetExpDropFromEntity(Entity ent)
	{
		if (!(ent is Monster monster))
		{
			return 0f;
		}
		float num = 5f * Mathf.Pow(1.2f, Mathf.Min(NetworkedManagerBase<ZoneManager>.instance.currentZoneIndex * 4, 30));
		num *= ges.expMultiplier.Get(monster.type);
		num = GetAdjustedDroppedExperienceAmount(num);
		return num * ges.expGlobalMultiplier * UnityEngine.Random.Range(1f - ges.expDropDeviation, 1f + ges.expDropDeviation);
	}

	public int GetKillGoldAmount(Entity ent)
	{
		if (!(ent is Monster monster))
		{
			return 0;
		}
		float num = ges.killGoldMultiplierByZoneIndex.Evaluate(NetworkedManagerBase<ZoneManager>.instance.currentZoneIndex);
		num *= ges.killGold.Get(monster.type);
		Monster.MonsterType type = monster.type;
		if (type == Monster.MonsterType.Lesser || type == Monster.MonsterType.Normal)
		{
			float num2 = GetAdjustedMonsterSpawnPopulation(1f, ignoreTurnMultiplier: true) * GetAdjustedMonsterWavesMultiplier();
			num = num / num2 * (float)DewPlayer.gamePlayers.Count;
		}
		else
		{
			num *= (float)DewPlayer.gamePlayers.Count;
		}
		num = GetAdjustedGoldAmount_Income(num);
		return DewMath.RandomRoundToInt(num * UnityEngine.Random.Range(1f - ges.killGoldDeviation, 1f + ges.killGoldDeviation));
	}

	public float GetAdjustedGoldAmount(float amount)
	{
		amount *= ges.globalGoldEconomyMultiplierByZoneIndex.Evaluate(NetworkedManagerBase<ZoneManager>.instance.currentZoneIndex);
		return amount;
	}

	public float GetAdjustedGoldAmount_Cost(float amount)
	{
		amount *= goldCostMultiplier;
		return GetAdjustedGoldAmount(amount);
	}

	public float GetAdjustedGoldAmount_Cost_Service(float baseAmount)
	{
		float num = baseAmount;
		num *= ges.generalServiceGoldPriceMultiplier;
		num *= ges.generalServiceGoldPriceMultiplierByZoneIndex.Evaluate(NetworkedManagerBase<ZoneManager>.instance.currentZoneIndex);
		return GetAdjustedGoldAmount_Cost(num);
	}

	public float GetAdjustedGoldAmount_Income(float amount)
	{
		amount *= goldIncomeMultiplier;
		return GetAdjustedGoldAmount(amount);
	}

	public float GetAdjustedMonsterWavesMultiplier()
	{
		return (ges.monsterWavesMultiplierPerPlayer - 1f) * GetMultiplayerDifficultyFactor(reduceWhenDead: true) + 1f;
	}

	public int GetAdjustedMonsterWaves(int original, DewRandom random = null)
	{
		return DewMath.RandomRoundToInt((float)original * GetAdjustedMonsterWavesMultiplier(), random);
	}

	public float GetAdjustedMonsterSpawnPopulation(float original, bool ignoreTurnMultiplier = false, bool ignoreCoopMultiplier = false)
	{
		float num = (ignoreCoopMultiplier ? 1f : (1f + (ges.monsterSpawnPopulationMultiplierPerPlayer - 1f) * GetMultiplayerDifficultyFactor(reduceWhenDead: true)));
		float num2 = (ignoreTurnMultiplier ? 1f : ges.monsterSpawnPopulationMultiplierByTurnIndex.Evaluate(NetworkedManagerBase<ZoneManager>.instance.currentTurnIndex));
		return original * num * num2 * maxAndSpawnedPopulationMultiplier;
	}

	public float GetAdjustedMonsterSpawnDelay(float original)
	{
		float num = ges.monsterSpawnDelayMultiplierByExtraPlayers.Evaluate(GetMultiplayerDifficultyFactor(0f));
		return original * num;
	}

	public float GetAdjustedDroppedExperienceAmount(float original)
	{
		float num = ges.droppedExpMultiplierPerPlayer - 1f;
		num *= GetMultiplayerDifficultyFactor(reduceWhenDead: false);
		return DewMath.RandomRoundToInt(original * (num + 1f) * gainedExpMultiplier);
	}

	public float GetMonsterBonusHealthPercentageByMultiplayer(Monster m)
	{
		float multiplayerDifficultyFactor = GetMultiplayerDifficultyFactor(reduceWhenDead: true);
		switch (m.type)
		{
		case Monster.MonsterType.Lesser:
		case Monster.MonsterType.Normal:
			return multiplayerDifficultyFactor * ges.monsterBonusHealthPercentagePerPlayer;
		case Monster.MonsterType.MiniBoss:
			return multiplayerDifficultyFactor * ges.miniBossBonusHealthPercentagePerPlayer;
		case Monster.MonsterType.Boss:
			return multiplayerDifficultyFactor * ges.bossBonusHealthPercentagePerPlayer;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	public float GetMonsterBonusPowerPercentage(Monster m)
	{
		float multiplayerDifficultyFactor = GetMultiplayerDifficultyFactor(reduceWhenDead: true);
		switch (m.type)
		{
		case Monster.MonsterType.Lesser:
		case Monster.MonsterType.Normal:
			return multiplayerDifficultyFactor * ges.monsterBonusPowerPercentagePerPlayer;
		case Monster.MonsterType.MiniBoss:
			return multiplayerDifficultyFactor * ges.miniBossBonusPowerPercentagePerPlayer;
		case Monster.MonsterType.Boss:
			return multiplayerDifficultyFactor * ges.bossBonusPowerPercentagePerPlayer;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	public int GetGemUpgradeDreamDustCost(Gem gem)
	{
		return GetGemUpgradeDreamDustCost(gem.quality);
	}

	public int GetGemUpgradeDreamDustCost(int quality)
	{
		return Mathf.Max(1, Mathf.RoundToInt(ges.gemUpgradeDreamDustByQuality.Evaluate(quality)));
	}

	public int GetGemUpgradeAddedQuality()
	{
		return ges.gemAddedQualityOnUpgrade;
	}

	public int GetSkillUpgradeDreamDustCost(SkillTrigger skill)
	{
		return GetSkillUpgradeDreamDustCost(skill.level);
	}

	public int GetSkillUpgradeDreamDustCost(int level)
	{
		return Mathf.Max(1, Mathf.RoundToInt(ges.skillUpgradeDreamDustByLevel.Evaluate(level)));
	}

	public float GetPredictionStrength()
	{
		try
		{
			if (predictionStrengthOverride != null)
			{
				return predictionStrengthOverride();
			}
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
		return difficulty.predictionStrengthCurve.Evaluate(UnityEngine.Random.value);
	}

	public float GetMultiplayerDifficultyFactor(bool reduceWhenDead)
	{
		return GetMultiplayerDifficultyFactor(reduceWhenDead ? 0.55f : 1f);
	}

	public float GetMultiplayerDifficultyFactor(float deadContribution)
	{
		int count = DewPlayer.gamePlayers.Count;
		if (deadContribution >= 1f)
		{
			return Mathf.Max((float)count - 1f, 0f);
		}
		int num = 0;
		foreach (DewPlayer gamePlayer in DewPlayer.gamePlayers)
		{
			if (!gamePlayer.hero.IsNullInactiveDeadOrKnockedOut())
			{
				num++;
			}
		}
		return Mathf.Max((float)num + (float)(count - num) * deadContribution - 1f, 0f);
	}

	public float GetSpecialSkillChanceMultiplier()
	{
		return difficulty.specialSkillChanceMultiplier;
	}

	public int GetCleanseSkillMinLevel()
	{
		return ges.skillCleanseMinLevel;
	}

	public int GetCleanseGemMinQuality()
	{
		return ges.gemCleanseMinQuality;
	}

	public int GetCleanseReturnedDreamDust(DewPlayer player, SkillTrigger skill)
	{
		int cleanseSkillMinLevel = GetCleanseSkillMinLevel();
		if (skill.level <= cleanseSkillMinLevel)
		{
			return 0;
		}
		float num = 0f;
		int num2 = 0;
		int num3 = skill.level - 1;
		while (num3 >= cleanseSkillMinLevel && num2++ <= 10000)
		{
			num += (float)GetSkillUpgradeDreamDustCost(num3);
			num3--;
		}
		num *= player.cleanseRefundMultiplier;
		int num4 = Mathf.RoundToInt(num);
		if (num4 < 0)
		{
			num4 = int.MaxValue;
		}
		else if (num4 == 0)
		{
			num4 = 1;
		}
		return num4;
	}

	public int GetCleanseReturnedDreamDust(DewPlayer player, Gem gem)
	{
		int cleanseGemMinQuality = GetCleanseGemMinQuality();
		if (gem.quality <= cleanseGemMinQuality)
		{
			return 0;
		}
		float num = 0f;
		int gemUpgradeAddedQuality = GetGemUpgradeAddedQuality();
		int num2 = gem.quality;
		int num3 = 0;
		while (num2 >= cleanseGemMinQuality + gemUpgradeAddedQuality && num3++ <= 10000)
		{
			num += (float)GetGemUpgradeDreamDustCost(num2 - gemUpgradeAddedQuality);
			num2 -= gemUpgradeAddedQuality;
		}
		int num4 = num2 - cleanseGemMinQuality;
		num += (float)GetGemUpgradeDreamDustCost(cleanseGemMinQuality) * ((float)num4 / (float)gemUpgradeAddedQuality);
		num *= player.cleanseRefundMultiplier;
		int num5 = Mathf.RoundToInt(num);
		if (num5 < 0)
		{
			num5 = int.MaxValue;
		}
		else if (num5 == 0)
		{
			num5 = 1;
		}
		return num5;
	}

	public int GetCleanseGoldCost(SkillTrigger skill)
	{
		return Mathf.RoundToInt(GetAdjustedGoldAmount(ges.skillCleanseCostByLevel.Evaluate(skill.level)));
	}

	public int GetCleanseGoldCost(Gem gem)
	{
		return Mathf.RoundToInt(GetAdjustedGoldAmount(ges.gemCleanseCostByQuality.Evaluate(gem.quality)));
	}

	public float GetGainedSkillHastePerSkillLevel(SkillTrigger skillTrigger)
	{
		if (skillTrigger.type != SkillType.Ultimate)
		{
			return ges.gainedSkillHastePerSkillLevel;
		}
		return 0f;
	}

	public float GetRegularMonsterHealthMultiplierByScaling(float customZoneIndex = float.NaN)
	{
		float scaledZoneIndexForHealth = difficulty.GetScaledZoneIndexForHealth(customZoneIndex);
		return Mathf.Lerp(GetWeakHealthScalingMultiplier_Imp(scaledZoneIndexForHealth), GetStrongHealthScalingMultiplier_Imp(scaledZoneIndexForHealth), 0.15f);
	}

	public float GetRegularMonsterDamageMultiplierByScaling(float customZoneIndex = float.NaN)
	{
		return GetUniversalDamageScalingMultiplier_Imp(difficulty.GetScaledZoneIndexForDamage(customZoneIndex));
	}

	public float GetMiniBossMonsterHealthMultiplierByScaling(float customZoneIndex = float.NaN)
	{
		float scaledZoneIndexForHealth = difficulty.GetScaledZoneIndexForHealth(customZoneIndex);
		return Mathf.Lerp(GetWeakHealthScalingMultiplier_Imp(scaledZoneIndexForHealth), GetStrongHealthScalingMultiplier_Imp(scaledZoneIndexForHealth), 0.7f);
	}

	public float GetMiniBossMonsterDamageMultiplierByScaling(float customZoneIndex = float.NaN)
	{
		return GetUniversalDamageScalingMultiplier_Imp(difficulty.GetScaledZoneIndexForDamage(customZoneIndex));
	}

	public float GetBossMonsterHealthMultiplierByScaling(float customZoneIndex = float.NaN)
	{
		return GetStrongHealthScalingMultiplier_Imp(difficulty.GetScaledZoneIndexForHealth(customZoneIndex));
	}

	public float GetBossMonsterDamageMultiplierByScaling(float customZoneIndex = float.NaN)
	{
		return GetUniversalDamageScalingMultiplier_Imp(difficulty.GetScaledZoneIndexForDamage(customZoneIndex));
	}

	public static float GetStrongHealthScalingMultiplier_Imp(float zi)
	{
		return GetMultiplier_Imp(zi, 0.0566392339721777, 0.209036492019735, 0.407011935005335, 0.999998990190336, 30);
	}

	public static float GetWeakHealthScalingMultiplier_Imp(float zi)
	{
		return GetMultiplier_Imp(zi, -4.10471677947602E-08, 0.481667070394692, 0.19833143379239, 0.999997791915299, 20);
	}

	public static float GetUniversalDamageScalingMultiplier_Imp(float zi)
	{
		return GetMultiplier_Imp(zi, 0.000109450559046085, 0.505623339094561, 0.249726671939049, 0.917160073613465, 8);
	}

	public static float GetMultiplier_Imp(double x, double a, double b, double c, double d, int linearStart)
	{
		if (x <= (double)linearStart)
		{
			return (float)Get(x);
		}
		double num = Get(linearStart + 1) - Get(linearStart);
		return (float)(Get(linearStart) + num * (x - (double)linearStart));
		double Get(double param)
		{
			return a * param * param * param + b * param * param + c * param + d;
		}
	}

	public float GetSpecialRewardAmount_Gold()
	{
		int currentZoneIndex = NetworkedManagerBase<ZoneManager>.instance.currentZoneIndex;
		float num = NetworkedManagerBase<LootManager>.instance.gemQualityMinByZoneIndex.Get(Rarity.Rare).Evaluate(currentZoneIndex);
		float num2 = NetworkedManagerBase<LootManager>.instance.gemQualityMaxByZoneIndex.Get(Rarity.Rare).Evaluate(currentZoneIndex);
		float amount = (float)Gem.GetBuyGold(Rarity.Rare, Mathf.RoundToInt((num + num2) / 2f)) * (1f + (float)currentZoneIndex * 0.075f);
		return GetAdjustedGoldAmount_Income(amount);
	}

	public float GetSpecialRewardAmount_DreamDust()
	{
		int currentZoneIndex = NetworkedManagerBase<ZoneManager>.instance.currentZoneIndex;
		NetworkedManagerBase<LootManager>.instance.gemQualityMinByZoneIndex.Get(Rarity.Rare).Evaluate(currentZoneIndex);
		float f = NetworkedManagerBase<LootManager>.instance.gemQualityMaxByZoneIndex.Get(Rarity.Rare).Evaluate(currentZoneIndex);
		return (float)GetGemUpgradeDreamDustCost(Mathf.RoundToInt(f)) * (1f + (float)NetworkedManagerBase<ZoneManager>.instance.currentZoneIndex * 0.1f) * (1f + (float)currentZoneIndex * 0.125f);
	}

	public virtual bool IsLazyCallReady()
	{
		if (((Behaviour)(object)this).isActiveAndEnabled && ManagerBase<TransitionManager>.instance.state == TransitionManager.StateType.Normal && NetworkClient.active && NetworkClient.ready && !NetworkServer.isLoadingScene && (UnityEngine.Object)(object)DewPlayer.local != null && (UnityEngine.Object)(object)DewPlayer.local.hero != null)
		{
			if (!((UnityEngine.Object)(object)NetworkedManagerBase<ZoneManager>.softInstance == null))
			{
				return !NetworkedManagerBase<ZoneManager>.instance.isInAnyTransition;
			}
			return true;
		}
		return false;
	}

	private void DoLogicUpdate_LazyCall(float dt)
	{
		if (_lazyCalledFunctions.Count <= 0 || !IsLazyCallReady())
		{
			return;
		}
		List<Action> list = new List<Action>(_lazyCalledFunctions);
		_lazyCalledFunctions.Clear();
		foreach (Action item in list)
		{
			try
			{
				item?.Invoke();
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
	}

	public static void CallOnReady(Action action)
	{
		if ((UnityEngine.Object)(object)NetworkedManagerBase<GameManager>.instance != null && NetworkedManagerBase<GameManager>.instance.IsLazyCallReady())
		{
			action?.Invoke();
		}
		else
		{
			_lazyCalledFunctions.Add(action);
		}
	}

	public GameManager()
	{
		_Mirror_SyncVarHookDelegate__difficulty = OnDifficultyChanged;
		_Mirror_SyncVarHookDelegate__003CambientLevel_003Ek__BackingField = OnAmbientLevelChanged;
		_Mirror_SyncVarHookDelegate__003CisGameTimePausedByAfk_003Ek__BackingField = OnIsGameTimePausedChanged;
		_Mirror_SyncVarHookDelegate__003CisGameTimePausedByGame_003Ek__BackingField = OnIsGameTimePausedChanged;
		_Mirror_SyncVarHookDelegate__003CisGameConcluded_003Ek__BackingField = OnIsGameConcludedChanged;
	}

	static GameManager()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected Obj, but got Unknown
		_lazyCalledFunctions = new List<Action>();
		RemoteProcedureCalls.RegisterRpc(typeof(GameManager), "System.Void GameManager::SetDisconnectedForEveryoneElse()", (RemoteCallDelegate)InvokeUserCode_SetDisconnectedForEveryoneElse);
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_SetDisconnectedForEveryoneElse()
	{
		if (!NetworkServer.active)
		{
			DewNetworkManager.instance.didRegisterError = true;
			DewSessionError.ShowError(new DewException(DewExceptionType.Disconnected));
		}
	}

	protected static void InvokeUserCode_SetDisconnectedForEveryoneElse(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC SetDisconnectedForEveryoneElse called on server.");
		}
		else
		{
			((GameManager)(object)obj).UserCode_SetDisconnectedForEveryoneElse();
		}
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		((NetworkBehaviour)this).SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			GeneratedNetworkCode._Write_SyncableAssetRef(writer, _difficulty);
			NetworkWriterExtensions.WriteInt(writer, ambientLevel__BackingField);
			NetworkWriterExtensions.WriteBool(writer, isGameTimePausedByAfk__BackingField);
			NetworkWriterExtensions.WriteBool(writer, isGameTimePausedByGame__BackingField);
			NetworkWriterExtensions.WriteFloat(writer, _gameTimeBase);
			NetworkWriterExtensions.WriteFloat(writer, _gameTimeTickStartTime);
			NetworkWriterExtensions.WriteBool(writer, isGameConcluded__BackingField);
			NetworkWriterExtensions.WriteFloat(writer, maxAndSpawnedPopulationMultiplier__BackingField);
			NetworkWriterExtensions.WriteFloat(writer, spawnedPopulation__BackingField);
			NetworkWriterExtensions.WriteBool(writer, isGameOverEnabled__BackingField);
			NetworkWriterExtensions.WriteString(writer, runId__BackingField);
			NetworkWriterExtensions.WriteInt(writer, _lockMidRunCounter);
			NetworkWriterExtensions.WriteFloat(writer, gainedExpMultiplier);
			NetworkWriterExtensions.WriteFloat(writer, goldCostMultiplier);
			NetworkWriterExtensions.WriteFloat(writer, goldIncomeMultiplier);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 1L) != 0L)
		{
			GeneratedNetworkCode._Write_SyncableAssetRef(writer, _difficulty);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 2L) != 0L)
		{
			NetworkWriterExtensions.WriteInt(writer, ambientLevel__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 4L) != 0L)
		{
			NetworkWriterExtensions.WriteBool(writer, isGameTimePausedByAfk__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 8L) != 0L)
		{
			NetworkWriterExtensions.WriteBool(writer, isGameTimePausedByGame__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x10L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, _gameTimeBase);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x20L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, _gameTimeTickStartTime);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x40L) != 0L)
		{
			NetworkWriterExtensions.WriteBool(writer, isGameConcluded__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x80L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, maxAndSpawnedPopulationMultiplier__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x100L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, spawnedPopulation__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x200L) != 0L)
		{
			NetworkWriterExtensions.WriteBool(writer, isGameOverEnabled__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x400L) != 0L)
		{
			NetworkWriterExtensions.WriteString(writer, runId__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x800L) != 0L)
		{
			NetworkWriterExtensions.WriteInt(writer, _lockMidRunCounter);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x1000L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, gainedExpMultiplier);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x2000L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, goldCostMultiplier);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x4000L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, goldIncomeMultiplier);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		((NetworkBehaviour)this).DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<SyncableAssetRef>(ref _difficulty, _Mirror_SyncVarHookDelegate__difficulty, GeneratedNetworkCode._Read_SyncableAssetRef(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref ambientLevel__BackingField, _Mirror_SyncVarHookDelegate__003CambientLevel_003Ek__BackingField, NetworkReaderExtensions.ReadInt(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref isGameTimePausedByAfk__BackingField, _Mirror_SyncVarHookDelegate__003CisGameTimePausedByAfk_003Ek__BackingField, NetworkReaderExtensions.ReadBool(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref isGameTimePausedByGame__BackingField, _Mirror_SyncVarHookDelegate__003CisGameTimePausedByGame_003Ek__BackingField, NetworkReaderExtensions.ReadBool(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref _gameTimeBase, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref _gameTimeTickStartTime, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref isGameConcluded__BackingField, _Mirror_SyncVarHookDelegate__003CisGameConcluded_003Ek__BackingField, NetworkReaderExtensions.ReadBool(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref maxAndSpawnedPopulationMultiplier__BackingField, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref spawnedPopulation__BackingField, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref isGameOverEnabled__BackingField, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<string>(ref runId__BackingField, (Action<string, string>)null, NetworkReaderExtensions.ReadString(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref _lockMidRunCounter, (Action<int, int>)null, NetworkReaderExtensions.ReadInt(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref gainedExpMultiplier, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref goldCostMultiplier, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref goldIncomeMultiplier, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 1L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<SyncableAssetRef>(ref _difficulty, _Mirror_SyncVarHookDelegate__difficulty, GeneratedNetworkCode._Read_SyncableAssetRef(reader));
		}
		if ((num & 2L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref ambientLevel__BackingField, _Mirror_SyncVarHookDelegate__003CambientLevel_003Ek__BackingField, NetworkReaderExtensions.ReadInt(reader));
		}
		if ((num & 4L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref isGameTimePausedByAfk__BackingField, _Mirror_SyncVarHookDelegate__003CisGameTimePausedByAfk_003Ek__BackingField, NetworkReaderExtensions.ReadBool(reader));
		}
		if ((num & 8L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref isGameTimePausedByGame__BackingField, _Mirror_SyncVarHookDelegate__003CisGameTimePausedByGame_003Ek__BackingField, NetworkReaderExtensions.ReadBool(reader));
		}
		if ((num & 0x10L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref _gameTimeBase, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
		if ((num & 0x20L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref _gameTimeTickStartTime, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
		if ((num & 0x40L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref isGameConcluded__BackingField, _Mirror_SyncVarHookDelegate__003CisGameConcluded_003Ek__BackingField, NetworkReaderExtensions.ReadBool(reader));
		}
		if ((num & 0x80L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref maxAndSpawnedPopulationMultiplier__BackingField, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
		if ((num & 0x100L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref spawnedPopulation__BackingField, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
		if ((num & 0x200L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref isGameOverEnabled__BackingField, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
		}
		if ((num & 0x400L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<string>(ref runId__BackingField, (Action<string, string>)null, NetworkReaderExtensions.ReadString(reader));
		}
		if ((num & 0x800L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref _lockMidRunCounter, (Action<int, int>)null, NetworkReaderExtensions.ReadInt(reader));
		}
		if ((num & 0x1000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref gainedExpMultiplier, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
		if ((num & 0x2000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref goldCostMultiplier, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
		if ((num & 0x4000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref goldIncomeMultiplier, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
	}
}
