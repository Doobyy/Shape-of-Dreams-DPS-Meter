using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using EpicTransport;
using Mirror;
using Mirror.RemoteCalls;
using Steamworks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Scripting;

[DewResourceLink(ResourceLinkBy.Type)]
public class DewPlayer : DewNetworkBehaviour, ISettingsChangedCallback
{
	private enum Role : byte
	{
		None,
		Environment,
		Creep
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CChangeLobby_Imp_003Ed__355 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder _003C_003Et__builder;

		public string lobbyId;

		private Awaiter _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_0084: Unknown result type (might be due to invalid IL or missing references)
			//IL_0089: Unknown result type (might be due to invalid IL or missing references)
			//IL_0090: Unknown result type (might be due to invalid IL or missing references)
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			//IL_0054: Unknown result type (might be due to invalid IL or missing references)
			//IL_0059: Unknown result type (might be due to invalid IL or missing references)
			//IL_006d: Unknown result type (might be due to invalid IL or missing references)
			//IL_006e: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			try
			{
				Awaiter val;
				if (num == 0)
				{
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default;
					num = (_003C_003E1__state = -1);
					goto IL_009f;
				}
				if (!NetworkServer.active && !ManagerBase<LobbyManager>.instance.isLobbyLeader)
				{
					UnityEngine.Debug.Log("Lobby replaced by host: " + lobbyId);
					UniTask val2 = ManagerBase<LobbyManager>.instance.service.JoinLobby(lobbyId);
					val = val2.GetAwaiter();
					if (!val.IsCompleted)
					{
						num = (_003C_003E1__state = 0);
						_003C_003Eu__1 = val;
						_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter, _003CChangeLobby_Imp_003Ed__355>(ref val, ref this);
						return;
					}
					goto IL_009f;
				}
				goto end_IL_0007;
				IL_009f:
				val.GetResult();
				UnityEngine.Debug.Log("Joined the replaced lobby");
				end_IL_0007:;
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				_003C_003Et__builder.SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			_003C_003Et__builder.SetResult();
		}

		void IAsyncStateMachine.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			this.MoveNext();
		}

		[DebuggerHidden]
		private void SetStateMachine(IAsyncStateMachine stateMachine)
		{
			_003C_003Et__builder.SetStateMachine(stateMachine);
		}

		void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
		{
			//ILSpy generated this explicit interface implementation from .override directive in SetStateMachine
			this.SetStateMachine(stateMachine);
		}
	}

	private static Texture2D DefaultAvatar;

	public static List<DewPlayer> allHumanPlayers;

	public static List<DewPlayer> lobbyPlayers;

	public static List<DewPlayer> gamePlayers;

	public static List<DewPlayer> spectators;

	public static SafeAction<DewPlayer> onHumanPlayerAdded;

	public static SafeAction<DewPlayer> onHumanPlayerRemoved;

	public static SafeAction<DewPlayer> onLobbyPlayerAdded;

	public static SafeAction<DewPlayer> onLobbyPlayerRemoved;

	public static SafeAction<DewPlayer> onGamePlayerAdded;

	public static SafeAction<DewPlayer> onGamePlayerRemoved;

	public static SafeAction<DewPlayer> onSpectatorAdded;

	public static SafeAction<DewPlayer> onSpectatorRemoved;

	[CompilerGenerated]
	[SyncVar(hook = "OnStateChanged")]
	private PlayerState state__BackingField;

	public SafeAction ClientEvent_OnStateChanged;

	public readonly SyncList<DewPlayer> allies = new SyncList<DewPlayer>();

	public readonly SyncList<DewPlayer> enemies = new SyncList<DewPlayer>();

	public readonly SyncList<DewPlayer> neutrals = new SyncList<DewPlayer>();

	[NonSerialized]
	public List<string> availableLucidDreams = new List<string>();

	[SyncVar]
	[SerializeField]
	private Role _role;

	[CompilerGenerated]
	[SyncVar]
	private bool isEveryInfoSet__BackingField;

	[CompilerGenerated]
	[SyncVar(hook = "OnPlayerNameRawChanged")]
	private string playerNameRaw__BackingField = "Dreamer";

	[SyncVar(hook = "OnEquippedNametagChanged")]
	private string _equippedNametag;

	public SafeAction<string, string> ClientEvent_OnEquippedNametagChanged;

	[SyncVar]
	private bool _isHostPlayer;

	[SyncVar]
	private Texture2D _avatar;

	[NonSerialized]
	public List<string> friendSteamIds = new List<string>();

	[CompilerGenerated]
	[SyncVar]
	private int totalMasteryLevel__BackingField;

	[CompilerGenerated]
	[SyncVar]
	private DewProfileStats profileStats__BackingField;

	[CompilerGenerated]
	[SyncVar]
	private bool shareItemsWhenDropped__BackingField = true;

	[CompilerGenerated]
	[SyncVar]
	private bool hasPolarisEndingUnlocked__BackingField;

	private bool _isLocalRequestingOverrides;

	[SyncVar(hook = "OnEosIdChanged")]
	private string _eosId;

	[SyncVar(hook = "OnPlatformIdChanged")]
	private string _platformId;

	[SyncVar(hook = "OnPlatformChanged")]
	private string _platform;

	[SyncVar(hook = "SelectedEntityChanged")]
	private Entity _controllingEntity;

	[CompilerGenerated]
	[SyncVar(hook = "OnHeroChanged")]
	private Hero hero__BackingField;

	public SafeAction<Hero, Hero> ClientEvent_OnHeroChanged;

	[CompilerGenerated]
	[SyncVar(hook = "OnGoldChanged")]
	private int gold__BackingField;

	public SafeAction<int, int> ClientEvent_OnGoldChanged;

	public SafeAction<int> ClientEvent_OnSpendGold;

	public SafeAction<int> ClientEvent_OnEarnGold;

	[CompilerGenerated]
	[SyncVar(hook = "OnDreamDustChanged")]
	private int dreamDust__BackingField;

	public SafeAction<int, int> ClientEvent_OnDreamDustChanged;

	public SafeAction<int> ClientEvent_OnSpendDreamDust;

	public SafeAction<int> ClientEvent_OnEarnDreamDust;

	public SafeAction<int> ClientEvent_OnEarnStardust;

	public SafeAction<int> ClientEvent_OnSpendStardust;

	[CompilerGenerated]
	[SyncVar(hook = "OnPlatinumCoinChanged")]
	private int platinumCoin__BackingField;

	public SafeAction<int, int> ClientEvent_OnPlatinumCoinChanged;

	public SafeAction<int, int, DewPlayer> ClientEvent_OnGiveCurrency;

	[CompilerGenerated]
	[SyncVar]
	private float cleanseRefundMultiplier__BackingField = 0.7f;

	[CompilerGenerated]
	[SyncVar]
	private float dismantleDreamDustMultiplier__BackingField = 1f;

	[CompilerGenerated]
	[SyncVar]
	private float dismantleSkillDreamDustMultiplier__BackingField = 1f;

	[CompilerGenerated]
	[SyncVar]
	private float dismantleGemDreamDustMultiplier__BackingField = 1f;

	[CompilerGenerated]
	[SyncVar]
	private float sellPriceMultiplier__BackingField = 1f;

	[CompilerGenerated]
	[SyncVar]
	private float buyPriceMultiplier__BackingField = 1f;

	[CompilerGenerated]
	[SyncVar]
	private int shopAddedItems__BackingField;

	[CompilerGenerated]
	[SyncVar]
	private float potionDropChanceMultiplier__BackingField = 1f;

	[CompilerGenerated]
	[SyncVar]
	private float doubleChaosChance__BackingField;

	[CompilerGenerated]
	[SyncVar]
	private bool isReadingArtifactStory__BackingField;

	private const float NotifyClientStatusInterval = 0.05f;

	private float _lastNotifyClientStatusTime;

	private Vector3 _lastNotifiedPos;

	private InputMode _lastNotifiedMode;

	private bool _lastNotifiedIsExplicit;

	private Entity _lastNotifiedTargetEnemy;

	private bool _lastNotifiedPlayingCutscene;

	private bool _hasNotifiedAtLeastOnce;

	private int _nextOnScreenTimerId;

	private readonly List<NetworkedOnScreenTimerHandle> _onScreenTimersServer = new List<NetworkedOnScreenTimerHandle>();

	private readonly List<(int, OnScreenTimerHandle, RefValue<float>)> _onScreenTimersLocal = new List<(int, OnScreenTimerHandle, RefValue<float>)>();

	internal SampleCastInfoContext? _currentSampleContext;

	public readonly SyncList<string> ownershipKeys = new SyncList<string>();

	[NonSerialized]
	public List<string> ownedItems = new List<string>();

	[SyncVar]
	public CSteamID steamId;

	[CompilerGenerated]
	[SyncVar]
	private string guid__BackingField;

	[CompilerGenerated]
	[SyncVar(hook = "OnHeroTypeChanged")]
	private string selectedHeroType__BackingField = "Hero_Lacerta";

	[CompilerGenerated]
	[SyncVar(hook = "OnLoadoutChanged")]
	private HeroLoadoutData selectedLoadout__BackingField = new HeroLoadoutData();

	public readonly SyncList<string> selectedAccessories = new SyncList<string>();

	[CompilerGenerated]
	[SyncVar(hook = "OnSkinChanged")]
	private string selectedSkin__BackingField;

	[CompilerGenerated]
	[SyncVar(hook = "OnDejavuItemChanged")]
	private string selectedDejavuItem__BackingField;

	public readonly SyncList<string> unlockedGameItems = new SyncList<string>();

	[CompilerGenerated]
	[SyncVar(hook = "OnIsReadyChanged")]
	private bool isReady__BackingField;

	public SafeAction<string> ClientEvent_OnSelectedHeroTypeChanged;

	public SafeAction<string> ClientEvent_OnSelectedSkinChanged;

	public SafeAction<HeroLoadoutData> ClientEvent_OnSelectedLoadoutChanged;

	public SafeAction ClientEvent_OnSelectedAccessoriesChanged;

	public SafeAction<string> ClientEvent_OnSelectedDejavuItemChanged;

	public SafeAction<bool> ClientEvent_OnIsReadyChanged;

	[CompilerGenerated]
	[SyncVar]
	private bool isLoadingForMidJoin__BackingField;

	[CompilerGenerated]
	[SyncVar]
	private bool isWaitingForContinueMidJoin__BackingField;

	[CompilerGenerated]
	[SyncVar]
	private bool joinedMidGame__BackingField;

	protected NetworkBehaviourSyncVar ____controllingEntityNetId;

	protected NetworkBehaviourSyncVar ____003Chero_003Ek__BackingFieldNetId;

	public Action<PlayerState, PlayerState> _Mirror_SyncVarHookDelegate__003Cstate_003Ek__BackingField;

	public Action<string, string> _Mirror_SyncVarHookDelegate__003CplayerNameRaw_003Ek__BackingField;

	public Action<string, string> _Mirror_SyncVarHookDelegate__equippedNametag;

	public Action<string, string> _Mirror_SyncVarHookDelegate__eosId;

	public Action<string, string> _Mirror_SyncVarHookDelegate__platformId;

	public Action<string, string> _Mirror_SyncVarHookDelegate__platform;

	public Action<Entity, Entity> _Mirror_SyncVarHookDelegate__controllingEntity;

	public Action<Hero, Hero> _Mirror_SyncVarHookDelegate__003Chero_003Ek__BackingField;

	public Action<int, int> _Mirror_SyncVarHookDelegate__003Cgold_003Ek__BackingField;

	public Action<int, int> _Mirror_SyncVarHookDelegate__003CdreamDust_003Ek__BackingField;

	public Action<int, int> _Mirror_SyncVarHookDelegate__003CplatinumCoin_003Ek__BackingField;

	public Action<string, string> _Mirror_SyncVarHookDelegate__003CselectedHeroType_003Ek__BackingField;

	public Action<HeroLoadoutData, HeroLoadoutData> _Mirror_SyncVarHookDelegate__003CselectedLoadout_003Ek__BackingField;

	public Action<string, string> _Mirror_SyncVarHookDelegate__003CselectedSkin_003Ek__BackingField;

	public Action<string, string> _Mirror_SyncVarHookDelegate__003CselectedDejavuItem_003Ek__BackingField;

	public Action<bool, bool> _Mirror_SyncVarHookDelegate__003CisReady_003Ek__BackingField;

	public static DewPlayer creep { get; private set; }

	public static DewPlayer environment { get; private set; }

	public static DewPlayer local { get; private set; }

	public PlayerState state
	{
		[CompilerGenerated]
		get
		{
			return state__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003Cstate_003Ek__BackingField = value;
		}
	}

	public bool isCreepPlayer => (UnityEngine.Object)(object)this == (UnityEngine.Object)(object)creep;

	public bool isEnvironmentPlayer => (UnityEngine.Object)(object)this == (UnityEngine.Object)(object)environment;

	public bool isHumanPlayer
	{
		get
		{
			if (!isCreepPlayer)
			{
				return !isEnvironmentPlayer;
			}
			return false;
		}
	}

	public bool isEveryInfoSet
	{
		[CompilerGenerated]
		get
		{
			return isEveryInfoSet__BackingField;
		}
		[CompilerGenerated]
		private set
		{
			Network_003CisEveryInfoSet_003Ek__BackingField = value;
		}
	}

	public string playerName { get; private set; }

	public string playerNameRaw
	{
		[CompilerGenerated]
		get
		{
			return playerNameRaw__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003CplayerNameRaw_003Ek__BackingField = value;
		}
	}

	public string equippedNametag => _equippedNametag;

	public bool isHostPlayer => _isHostPlayer;

	public Texture2D avatar
	{
		get
		{
			if (!(_avatar == null))
			{
				return _avatar;
			}
			return DefaultAvatar;
		}
	}

	public bool isKicked { get; private set; }

	public int totalMasteryLevel
	{
		[CompilerGenerated]
		get
		{
			return totalMasteryLevel__BackingField;
		}
		[CompilerGenerated]
		private set
		{
			Network_003CtotalMasteryLevel_003Ek__BackingField = value;
		}
	}

	public DewProfileStats profileStats
	{
		[CompilerGenerated]
		get
		{
			return profileStats__BackingField;
		}
		[CompilerGenerated]
		private set
		{
			Network_003CprofileStats_003Ek__BackingField = value;
		}
	}

	public bool shareItemsWhenDropped
	{
		[CompilerGenerated]
		get
		{
			return shareItemsWhenDropped__BackingField;
		}
		[CompilerGenerated]
		private set
		{
			Network_003CshareItemsWhenDropped_003Ek__BackingField = value;
		}
	}

	public bool hasPolarisEndingUnlocked
	{
		[CompilerGenerated]
		get
		{
			return hasPolarisEndingUnlocked__BackingField;
		}
		[CompilerGenerated]
		private set
		{
			Network_003ChasPolarisEndingUnlocked_003Ek__BackingField = value;
		}
	}

	public string EOSID => _eosId;

	public string PlatformID => _platformId;

	public string Platform => _platform;

	public Entity controllingEntity
	{
		get
		{
			return Network_controllingEntity;
		}
		set
		{
			if (!((NetworkBehaviour)this).isServer)
			{
				throw new Exception("Only server can change this.");
			}
			Network_controllingEntity = value;
		}
	}

	public Hero hero
	{
		[CompilerGenerated]
		get
		{
			return Network_003Chero_003Ek__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003Chero_003Ek__BackingField = value;
		}
	}

	[SaveVar(SaveVarFlags.Default)]
	public int gold
	{
		[CompilerGenerated]
		get
		{
			return gold__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003Cgold_003Ek__BackingField = value;
		}
	}

	[SaveVar(SaveVarFlags.Default)]
	public int dreamDust
	{
		[CompilerGenerated]
		get
		{
			return dreamDust__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003CdreamDust_003Ek__BackingField = value;
		}
	}

	[SaveVar(SaveVarFlags.Default)]
	public int platinumCoin
	{
		[CompilerGenerated]
		get
		{
			return platinumCoin__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003CplatinumCoin_003Ek__BackingField = value;
		}
	}

	public float cleanseRefundMultiplier
	{
		[CompilerGenerated]
		get
		{
			return cleanseRefundMultiplier__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003CcleanseRefundMultiplier_003Ek__BackingField = value;
		}
	}

	public float dismantleDreamDustMultiplier
	{
		[CompilerGenerated]
		get
		{
			return dismantleDreamDustMultiplier__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003CdismantleDreamDustMultiplier_003Ek__BackingField = value;
		}
	}

	public float dismantleSkillDreamDustMultiplier
	{
		[CompilerGenerated]
		get
		{
			return dismantleSkillDreamDustMultiplier__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003CdismantleSkillDreamDustMultiplier_003Ek__BackingField = value;
		}
	}

	public float dismantleGemDreamDustMultiplier
	{
		[CompilerGenerated]
		get
		{
			return dismantleGemDreamDustMultiplier__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003CdismantleGemDreamDustMultiplier_003Ek__BackingField = value;
		}
	}

	public float sellPriceMultiplier
	{
		[CompilerGenerated]
		get
		{
			return sellPriceMultiplier__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003CsellPriceMultiplier_003Ek__BackingField = value;
		}
	}

	public float buyPriceMultiplier
	{
		[CompilerGenerated]
		get
		{
			return buyPriceMultiplier__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003CbuyPriceMultiplier_003Ek__BackingField = value;
		}
	}

	public int shopAddedItems
	{
		[CompilerGenerated]
		get
		{
			return shopAddedItems__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003CshopAddedItems_003Ek__BackingField = value;
		}
	}

	public float potionDropChanceMultiplier
	{
		[CompilerGenerated]
		get
		{
			return potionDropChanceMultiplier__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003CpotionDropChanceMultiplier_003Ek__BackingField = value;
		}
	}

	public float doubleChaosChance
	{
		[CompilerGenerated]
		get
		{
			return doubleChaosChance__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003CdoubleChaosChance_003Ek__BackingField = value;
		}
	}

	public bool isReadingArtifactStory
	{
		[CompilerGenerated]
		get
		{
			return isReadingArtifactStory__BackingField;
		}
		[CompilerGenerated]
		internal set
		{
			Network_003CisReadingArtifactStory_003Ek__BackingField = value;
		}
	}

	public bool isPlayingCutscene { get; internal set; }

	public Vector3 cursorWorldPos { get; private set; }

	public InputMode inputMode { get; internal set; }

	public bool isGamepadExplicitAim { get; internal set; }

	public Entity gamepadTargetEnemy { get; internal set; }

	public float monsterKillGoldMultiplier { get; set; } = 1f;

	public bool isSamplingCastInfo => _currentSampleContext.HasValue;

	public string guid
	{
		[CompilerGenerated]
		get
		{
			return guid__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003Cguid_003Ek__BackingField = value;
		}
	}

	public string selectedHeroType
	{
		[CompilerGenerated]
		get
		{
			return selectedHeroType__BackingField;
		}
		[CompilerGenerated]
		private set
		{
			Network_003CselectedHeroType_003Ek__BackingField = value;
		}
	}

	public HeroLoadoutData selectedLoadout
	{
		[CompilerGenerated]
		get
		{
			return selectedLoadout__BackingField;
		}
		[CompilerGenerated]
		private set
		{
			Network_003CselectedLoadout_003Ek__BackingField = value;
		}
	}

	public string selectedSkin
	{
		[CompilerGenerated]
		get
		{
			return selectedSkin__BackingField;
		}
		[CompilerGenerated]
		private set
		{
			Network_003CselectedSkin_003Ek__BackingField = value;
		}
	}

	public string selectedDejavuItem
	{
		[CompilerGenerated]
		get
		{
			return selectedDejavuItem__BackingField;
		}
		[CompilerGenerated]
		private set
		{
			Network_003CselectedDejavuItem_003Ek__BackingField = value;
		}
	}

	public bool isReady
	{
		[CompilerGenerated]
		get
		{
			return isReady__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003CisReady_003Ek__BackingField = value;
		}
	}

	public bool isLoadingForMidJoin
	{
		[CompilerGenerated]
		get
		{
			return isLoadingForMidJoin__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003CisLoadingForMidJoin_003Ek__BackingField = value;
		}
	}

	public bool isWaitingForContinueMidJoin
	{
		[CompilerGenerated]
		get
		{
			return isWaitingForContinueMidJoin__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003CisWaitingForContinueMidJoin_003Ek__BackingField = value;
		}
	}

	public bool joinedMidGame
	{
		[CompilerGenerated]
		get
		{
			return joinedMidGame__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003CjoinedMidGame_003Ek__BackingField = value;
		}
	}

	public PlayerState Network_003Cstate_003Ek__BackingField
	{
		get
		{
			return state__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<PlayerState>(value, ref state__BackingField, 1uL, _Mirror_SyncVarHookDelegate__003Cstate_003Ek__BackingField);
		}
	}

	public Role Network_role
	{
		get
		{
			return _role;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<Role>(value, ref _role, 2uL, (Action<Role, Role>)null);
		}
	}

	public bool Network_003CisEveryInfoSet_003Ek__BackingField
	{
		get
		{
			return isEveryInfoSet__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<bool>(value, ref isEveryInfoSet__BackingField, 4uL, (Action<bool, bool>)null);
		}
	}

	public string Network_003CplayerNameRaw_003Ek__BackingField
	{
		get
		{
			return playerNameRaw__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<string>(value, ref playerNameRaw__BackingField, 8uL, _Mirror_SyncVarHookDelegate__003CplayerNameRaw_003Ek__BackingField);
		}
	}

	public string Network_equippedNametag
	{
		get
		{
			return _equippedNametag;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<string>(value, ref _equippedNametag, 16uL, _Mirror_SyncVarHookDelegate__equippedNametag);
		}
	}

	public bool Network_isHostPlayer
	{
		get
		{
			return _isHostPlayer;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<bool>(value, ref _isHostPlayer, 32uL, (Action<bool, bool>)null);
		}
	}

	public Texture2D Network_avatar
	{
		get
		{
			return _avatar;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<Texture2D>(value, ref _avatar, 64uL, (Action<Texture2D, Texture2D>)null);
		}
	}

	public int Network_003CtotalMasteryLevel_003Ek__BackingField
	{
		get
		{
			return totalMasteryLevel__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<int>(value, ref totalMasteryLevel__BackingField, 128uL, (Action<int, int>)null);
		}
	}

	public DewProfileStats Network_003CprofileStats_003Ek__BackingField
	{
		get
		{
			return profileStats__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<DewProfileStats>(value, ref profileStats__BackingField, 256uL, (Action<DewProfileStats, DewProfileStats>)null);
		}
	}

	public bool Network_003CshareItemsWhenDropped_003Ek__BackingField
	{
		get
		{
			return shareItemsWhenDropped__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<bool>(value, ref shareItemsWhenDropped__BackingField, 512uL, (Action<bool, bool>)null);
		}
	}

	public bool Network_003ChasPolarisEndingUnlocked_003Ek__BackingField
	{
		get
		{
			return hasPolarisEndingUnlocked__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<bool>(value, ref hasPolarisEndingUnlocked__BackingField, 1024uL, (Action<bool, bool>)null);
		}
	}

	public string Network_eosId
	{
		get
		{
			return _eosId;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<string>(value, ref _eosId, 2048uL, _Mirror_SyncVarHookDelegate__eosId);
		}
	}

	public string Network_platformId
	{
		get
		{
			return _platformId;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<string>(value, ref _platformId, 4096uL, _Mirror_SyncVarHookDelegate__platformId);
		}
	}

	public string Network_platform
	{
		get
		{
			return _platform;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<string>(value, ref _platform, 8192uL, _Mirror_SyncVarHookDelegate__platform);
		}
	}

	public Entity Network_controllingEntity
	{
		get
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			return ((NetworkBehaviour)this).GetSyncVarNetworkBehaviour<Entity>(____controllingEntityNetId, ref _controllingEntity);
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter_NetworkBehaviour<Entity>(value, ref _controllingEntity, 16384uL, _Mirror_SyncVarHookDelegate__controllingEntity, ref ____controllingEntityNetId);
		}
	}

	public Hero Network_003Chero_003Ek__BackingField
	{
		get
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			return ((NetworkBehaviour)this).GetSyncVarNetworkBehaviour<Hero>(____003Chero_003Ek__BackingFieldNetId, ref hero__BackingField);
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter_NetworkBehaviour<Hero>(value, ref hero__BackingField, 32768uL, _Mirror_SyncVarHookDelegate__003Chero_003Ek__BackingField, ref ____003Chero_003Ek__BackingFieldNetId);
		}
	}

	public int Network_003Cgold_003Ek__BackingField
	{
		get
		{
			return gold__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<int>(value, ref gold__BackingField, 65536uL, _Mirror_SyncVarHookDelegate__003Cgold_003Ek__BackingField);
		}
	}

	public int Network_003CdreamDust_003Ek__BackingField
	{
		get
		{
			return dreamDust__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<int>(value, ref dreamDust__BackingField, 131072uL, _Mirror_SyncVarHookDelegate__003CdreamDust_003Ek__BackingField);
		}
	}

	public int Network_003CplatinumCoin_003Ek__BackingField
	{
		get
		{
			return platinumCoin__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<int>(value, ref platinumCoin__BackingField, 262144uL, _Mirror_SyncVarHookDelegate__003CplatinumCoin_003Ek__BackingField);
		}
	}

	public float Network_003CcleanseRefundMultiplier_003Ek__BackingField
	{
		get
		{
			return cleanseRefundMultiplier__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref cleanseRefundMultiplier__BackingField, 524288uL, (Action<float, float>)null);
		}
	}

	public float Network_003CdismantleDreamDustMultiplier_003Ek__BackingField
	{
		get
		{
			return dismantleDreamDustMultiplier__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref dismantleDreamDustMultiplier__BackingField, 1048576uL, (Action<float, float>)null);
		}
	}

	public float Network_003CdismantleSkillDreamDustMultiplier_003Ek__BackingField
	{
		get
		{
			return dismantleSkillDreamDustMultiplier__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref dismantleSkillDreamDustMultiplier__BackingField, 2097152uL, (Action<float, float>)null);
		}
	}

	public float Network_003CdismantleGemDreamDustMultiplier_003Ek__BackingField
	{
		get
		{
			return dismantleGemDreamDustMultiplier__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref dismantleGemDreamDustMultiplier__BackingField, 4194304uL, (Action<float, float>)null);
		}
	}

	public float Network_003CsellPriceMultiplier_003Ek__BackingField
	{
		get
		{
			return sellPriceMultiplier__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref sellPriceMultiplier__BackingField, 8388608uL, (Action<float, float>)null);
		}
	}

	public float Network_003CbuyPriceMultiplier_003Ek__BackingField
	{
		get
		{
			return buyPriceMultiplier__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref buyPriceMultiplier__BackingField, 16777216uL, (Action<float, float>)null);
		}
	}

	public int Network_003CshopAddedItems_003Ek__BackingField
	{
		get
		{
			return shopAddedItems__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<int>(value, ref shopAddedItems__BackingField, 33554432uL, (Action<int, int>)null);
		}
	}

	public float Network_003CpotionDropChanceMultiplier_003Ek__BackingField
	{
		get
		{
			return potionDropChanceMultiplier__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref potionDropChanceMultiplier__BackingField, 67108864uL, (Action<float, float>)null);
		}
	}

	public float Network_003CdoubleChaosChance_003Ek__BackingField
	{
		get
		{
			return doubleChaosChance__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref doubleChaosChance__BackingField, 134217728uL, (Action<float, float>)null);
		}
	}

	public bool Network_003CisReadingArtifactStory_003Ek__BackingField
	{
		get
		{
			return isReadingArtifactStory__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<bool>(value, ref isReadingArtifactStory__BackingField, 268435456uL, (Action<bool, bool>)null);
		}
	}

	public CSteamID NetworksteamId
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return steamId;
		}
		[param: In]
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			((NetworkBehaviour)this).GeneratedSyncVarSetter<CSteamID>(value, ref steamId, 536870912uL, (Action<CSteamID, CSteamID>)null);
		}
	}

	public string Network_003Cguid_003Ek__BackingField
	{
		get
		{
			return guid__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<string>(value, ref guid__BackingField, 1073741824uL, (Action<string, string>)null);
		}
	}

	public string Network_003CselectedHeroType_003Ek__BackingField
	{
		get
		{
			return selectedHeroType__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<string>(value, ref selectedHeroType__BackingField, 2147483648uL, _Mirror_SyncVarHookDelegate__003CselectedHeroType_003Ek__BackingField);
		}
	}

	public HeroLoadoutData Network_003CselectedLoadout_003Ek__BackingField
	{
		get
		{
			return selectedLoadout__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<HeroLoadoutData>(value, ref selectedLoadout__BackingField, 4294967296uL, _Mirror_SyncVarHookDelegate__003CselectedLoadout_003Ek__BackingField);
		}
	}

	public string Network_003CselectedSkin_003Ek__BackingField
	{
		get
		{
			return selectedSkin__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<string>(value, ref selectedSkin__BackingField, 8589934592uL, _Mirror_SyncVarHookDelegate__003CselectedSkin_003Ek__BackingField);
		}
	}

	public string Network_003CselectedDejavuItem_003Ek__BackingField
	{
		get
		{
			return selectedDejavuItem__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<string>(value, ref selectedDejavuItem__BackingField, 17179869184uL, _Mirror_SyncVarHookDelegate__003CselectedDejavuItem_003Ek__BackingField);
		}
	}

	public bool Network_003CisReady_003Ek__BackingField
	{
		get
		{
			return isReady__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<bool>(value, ref isReady__BackingField, 34359738368uL, _Mirror_SyncVarHookDelegate__003CisReady_003Ek__BackingField);
		}
	}

	public bool Network_003CisLoadingForMidJoin_003Ek__BackingField
	{
		get
		{
			return isLoadingForMidJoin__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<bool>(value, ref isLoadingForMidJoin__BackingField, 68719476736uL, (Action<bool, bool>)null);
		}
	}

	public bool Network_003CisWaitingForContinueMidJoin_003Ek__BackingField
	{
		get
		{
			return isWaitingForContinueMidJoin__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<bool>(value, ref isWaitingForContinueMidJoin__BackingField, 137438953472uL, (Action<bool, bool>)null);
		}
	}

	public bool Network_003CjoinedMidGame_003Ek__BackingField
	{
		get
		{
			return joinedMidGame__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<bool>(value, ref joinedMidGame__BackingField, 274877906944uL, (Action<bool, bool>)null);
		}
	}

	public static implicit operator NetworkConnectionToClient(DewPlayer p)
	{
		return ((NetworkBehaviour)p).connectionToClient;
	}

	private void OnStateChanged(PlayerState _, PlayerState __)
	{
		if (!isHumanPlayer)
		{
			return;
		}
		ClientEvent_OnStateChanged?.Invoke();
		if (lobbyPlayers.Remove(this))
		{
			onLobbyPlayerRemoved?.Invoke(this);
		}
		if (gamePlayers.Remove(this))
		{
			onGamePlayerRemoved?.Invoke(this);
		}
		if (spectators.Remove(this))
		{
			onSpectatorRemoved?.Invoke(this);
		}
		switch (state)
		{
		case PlayerState.InLobby:
			lobbyPlayers.Add(this);
			lobbyPlayers.Sort((DewPlayer x, DewPlayer y) => ((NetworkBehaviour)x).netId.CompareTo(((NetworkBehaviour)y).netId));
			onLobbyPlayerAdded?.Invoke(this);
			break;
		case PlayerState.Playing:
			gamePlayers.Add(this);
			gamePlayers.Sort((DewPlayer x, DewPlayer y) => ((NetworkBehaviour)x).netId.CompareTo(((NetworkBehaviour)y).netId));
			onGamePlayerAdded?.Invoke(this);
			break;
		case PlayerState.Spectating:
			spectators.Add(this);
			spectators.Sort((DewPlayer x, DewPlayer y) => ((NetworkBehaviour)x).netId.CompareTo(((NetworkBehaviour)y).netId));
			onSpectatorAdded?.Invoke(this);
			break;
		default:
			throw new ArgumentOutOfRangeException();
		case PlayerState.None:
			break;
		}
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	private static void Init()
	{
		allHumanPlayers.Clear();
		lobbyPlayers.Clear();
		gamePlayers.Clear();
		spectators.Clear();
	}

	private void OnEquippedNametagChanged(string old, string newVal)
	{
		ClientEvent_OnEquippedNametagChanged?.Invoke(old, newVal);
	}

	public void CmdUpdateShareItemsWhenDropped()
	{
		CmdUpdateShareItemsWhenDropped_Imp(DewSave.profileMain.gameplay.shareItemsWhenDropped);
	}

	[Command]
	private void CmdUpdateShareItemsWhenDropped_Imp(bool value)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteBool((NetworkWriter)(object)val, value);
		((NetworkBehaviour)this).SendCommandInternal("System.Void DewPlayer::CmdUpdateShareItemsWhenDropped_Imp(System.Boolean)", 431543226, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	public void CmdUpdateHasPolarisEndingUnlocked()
	{
		CmdUpdateHasPolarisEndingUnlocked_Imp(DewSave.profileStats.total.didUnlockPolarisEnding);
	}

	[Command]
	private void CmdUpdateHasPolarisEndingUnlocked_Imp(bool value)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteBool((NetworkWriter)(object)val, value);
		((NetworkBehaviour)this).SendCommandInternal("System.Void DewPlayer::CmdUpdateHasPolarisEndingUnlocked_Imp(System.Boolean)", 966097933, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	protected override void Awake()
	{
		base.Awake();
		if (DefaultAvatar == null)
		{
			DefaultAvatar = Resources.Load<Texture2D>("Sprites/DefaultAvatar");
		}
		allies.Callback += TeamRelationChanged;
		enemies.Callback += TeamRelationChanged;
		neutrals.Callback += TeamRelationChanged;
		Awake_ItemAuth();
	}

	private void TeamRelationChanged(Operation<DewPlayer> op, int itemindex, DewPlayer olditem, DewPlayer newitem)
	{
		DewPlayer dewPlayer = (((UnityEngine.Object)(object)newitem != null) ? newitem : olditem);
		if ((UnityEngine.Object)(object)dewPlayer == null)
		{
			return;
		}
		foreach (Entity allEntity in NetworkedManagerBase<ActorManager>.instance.allEntities)
		{
			if (!((UnityEngine.Object)(object)allEntity.owner != (UnityEngine.Object)(object)dewPlayer))
			{
				NetworkedManagerBase<ClientEventManager>.instance.OnRefreshEntityHealthbar?.Invoke(allEntity);
			}
		}
	}

	public override void OnStartClient()
	{
		base.OnStartClient();
		RevalidateAllItems();
		if (!((NetworkBehaviour)this).isLocalPlayer)
		{
			return;
		}
		OnStartClient_ItemAuth();
		if (!((NetworkBehaviour)this).isServer && ManagerBase<LobbyManager>.instance.service.currentLobby != null && ManagerBase<LobbyManager>.instance.service.currentLobby.isModded)
		{
			CmdRequestOverrides();
		}
		List<string> list = new List<string>();
		foreach (LucidDream item in DewResources.FindAllByType<LucidDream>(default(ResourceLoadSettings)))
		{
			string name = ((object)item).GetType().Name;
			if (Dew.IsLucidDreamIncludedInGame(name) && DewSave.profileMain.lucidDreams[name].status == UnlockStatus.Complete)
			{
				list.Add(name);
			}
		}
		SetUnlockedLucidDreams(list.ToArray());
		if (DewBuildProfile.current.platform == PlatformType.STEAM && DewSteam.isInitialized)
		{
			UpdateLocalPlayerAvatar();
		}
		CmdSetHeroType(NetworkedManagerBase<GameSettingsManager>.instance.GetLocalPreferredGameSettings().hero);
		if (!string.IsNullOrEmpty(DewSave.profileMain.preferredNametag))
		{
			CmdSetNametag_Imp(DewSave.profileMain.preferredNametag);
		}
		CmdSetProfileStats(DewSave.profileStats);
		CmdSetUnlockedGameItems();
		CmdUpdateShareItemsWhenDropped();
		CmdUpdateHasPolarisEndingUnlocked();
		CmdNotifyEveryInfoSet();
	}

	public override void LogicUpdate(float dt)
	{
		LogicUpdate_InGame();
		LogicUpdate_InGame_NetworkedOnScreenTimer();
		LogicUpdate_MidJoin();
		LogicUpdate_Lobby();
	}

	private void UpdateLocalPlayerAvatar()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		if (!((NetworkBehaviour)this).isLocalPlayer || !DewSteam.isInitialized)
		{
			return;
		}
		try
		{
			int mediumFriendAvatar = SteamFriends.GetMediumFriendAvatar(SteamUser.GetSteamID());
			if (mediumFriendAvatar > 0)
			{
				uint num = default;
				uint num2 = default;
				SteamUtils.GetImageSize(mediumFriendAvatar, ref num, ref num2);
				byte[] array = new byte[num * num2 * 4];
				SteamUtils.GetImageRGBA(mediumFriendAvatar, array, array.Length);
				Texture2D texture2D = new Texture2D((int)num, (int)num2, TextureFormat.RGBA32, mipChain: false);
				texture2D.LoadRawTextureData(array);
				texture2D.filterMode = FilterMode.Trilinear;
				texture2D.Apply();
				Color[] pixels = texture2D.GetPixels();
				Color[] array2 = new Color[pixels.Length];
				for (int i = 0; i < num2; i++)
				{
					Array.Copy(pixels, i * num, array2, (num2 - i - 1) * num, num);
				}
				texture2D.SetPixels(array2);
				texture2D.Apply();
				CmdSetPlayerAvatar(texture2D);
			}
		}
		catch (Exception exception)
		{
			UnityEngine.Debug.Log("Failed to update local player avatar.");
			UnityEngine.Debug.LogException(exception);
		}
	}

	[Server]
	public void Kick()
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogWarning("[Server] function 'System.Void DewPlayer::Kick()' called when server was not active");
			return;
		}
		if (((NetworkBehaviour)this).isLocalPlayer)
		{
			throw new InvalidOperationException();
		}
		if (((NetworkBehaviour)this).connectionToClient != null)
		{
			TpcKick();
			isKicked = true;
			((MonoBehaviour)(object)this).StartCoroutine(Routine());
		}
		IEnumerator Routine()
		{
			yield return new WaitForSeconds(0.35f);
			if (((NetworkBehaviour)this).connectionToClient != null)
			{
				((NetworkConnection)((NetworkBehaviour)this).connectionToClient).Disconnect();
			}
		}
	}

	[TargetRpc]
	private void TpcKick()
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendTargetRPCInternal((NetworkConnection)null, "System.Void DewPlayer::TpcKick()", 1044727803, (NetworkWriter)(object)val, 0);
		NetworkWriterPool.Return(val);
	}

	[Command]
	private void CmdSetPlayerAvatar(Texture2D tex)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteTexture2D((NetworkWriter)(object)val, tex);
		((NetworkBehaviour)this).SendCommandInternal("System.Void DewPlayer::CmdSetPlayerAvatar(UnityEngine.Texture2D)", -689222938, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	public void CmdSetNametag(string ntName)
	{
		if (((NetworkBehaviour)this).isLocalPlayer)
		{
			DewSave.profileMain.preferredNametag = ntName;
			CmdSetNametag_Imp(ntName);
		}
	}

	[Command]
	private void CmdSetNametag_Imp(string ntName)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteString((NetworkWriter)(object)val, ntName);
		((NetworkBehaviour)this).SendCommandInternal("System.Void DewPlayer::CmdSetNametag_Imp(System.String)", -443679264, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[Command]
	public void CmdSetDejavuItem(string item)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteString((NetworkWriter)(object)val, item);
		((NetworkBehaviour)this).SendCommandInternal("System.Void DewPlayer::CmdSetDejavuItem(System.String)", 401430204, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[Command]
	private void SetUnlockedLucidDreams(string[] dreams)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		GeneratedNetworkCode._Write_System_002EString_005B_005D((NetworkWriter)(object)val, dreams);
		((NetworkBehaviour)this).SendCommandInternal("System.Void DewPlayer::SetUnlockedLucidDreams(System.String[])", -1108135284, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	public override void OnStart()
	{
		base.OnStart();
		if (((NetworkBehaviour)this).isLocalPlayer)
		{
			local = this;
		}
		if (_role == Role.Creep)
		{
			creep = this;
		}
		else if (_role == Role.Environment)
		{
			environment = this;
		}
		else
		{
			allHumanPlayers.Add(this);
			allHumanPlayers.Sort((DewPlayer x, DewPlayer y) => ((NetworkBehaviour)x).netId.CompareTo(((NetworkBehaviour)y).netId));
			onHumanPlayerAdded?.Invoke(this);
			UpdateGameObjectName();
		}
		Transform transform = ManagerBase<NetworkLogicPackage>.instance.transform.Find("Players");
		((Component)(object)this).transform.parent = transform.transform;
		if (((NetworkBehaviour)this).isServer && isHumanPlayer)
		{
			if (((NetworkBehaviour)this).isLocalPlayer)
			{
				Network_isHostPlayer = true;
			}
			if (state == PlayerState.None)
			{
				Network_003Cstate_003Ek__BackingField = ((!((UnityEngine.Object)(object)TestMapGameManager.instance != null)) ? PlayerState.InLobby : PlayerState.Playing);
			}
		}
		OnStart_Lobby();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (_avatar != null)
		{
			UnityEngine.Object.Destroy(_avatar);
		}
	}

	public override void OnStop()
	{
		base.OnStop();
		if ((UnityEngine.Object)(object)environment == (UnityEngine.Object)(object)this)
		{
			environment = null;
		}
		if ((UnityEngine.Object)(object)creep == (UnityEngine.Object)(object)this)
		{
			creep = null;
		}
		if ((UnityEngine.Object)(object)local == (UnityEngine.Object)(object)this)
		{
			local = null;
		}
		if (allHumanPlayers.Remove(this))
		{
			onHumanPlayerRemoved?.Invoke(this);
		}
		if (lobbyPlayers.Remove(this))
		{
			onLobbyPlayerRemoved?.Invoke(this);
		}
		if (gamePlayers.Remove(this))
		{
			onGamePlayerRemoved?.Invoke(this);
		}
		if (spectators.Remove(this))
		{
			onSpectatorRemoved?.Invoke(this);
		}
	}

	public override void OnStopServer()
	{
		base.OnStopServer();
		if ((UnityEngine.Object)(object)NetworkedManagerBase<GameSettingsManager>.instance != null)
		{
			availableLucidDreams.Clear();
			NetworkedManagerBase<GameSettingsManager>.instance.UpdateAvailableLucidDreams();
		}
		if (!((UnityEngine.Object)(object)NetworkedManagerBase<ActorManager>.instance != null))
		{
			return;
		}
		Actor[] array = NetworkedManagerBase<ActorManager>.instance.allActors.ToArray();
		foreach (Actor obj in array)
		{
			if (obj is SkillTrigger skillTrigger && ((UnityEngine.Object)(object)skillTrigger.tempOwner == (UnityEngine.Object)(object)this || ((UnityEngine.Object)(object)skillTrigger.handOwner != null && (UnityEngine.Object)(object)skillTrigger.handOwner.owner == (UnityEngine.Object)(object)this)))
			{
				skillTrigger.Destroy();
			}
			if (obj is Gem gem && ((UnityEngine.Object)(object)gem.tempOwner == (UnityEngine.Object)(object)this || ((UnityEngine.Object)(object)gem.handOwner != null && (UnityEngine.Object)(object)gem.handOwner.owner == (UnityEngine.Object)(object)this)))
			{
				gem.Destroy();
			}
		}
	}

	public TeamRelation GetTeamRelation(DewPlayer other)
	{
		if (allies.Contains(other))
		{
			return TeamRelation.Ally;
		}
		if (enemies.Contains(other))
		{
			return TeamRelation.Enemy;
		}
		if (neutrals.Contains(other))
		{
			return TeamRelation.Neutral;
		}
		if ((UnityEngine.Object)(object)this == (UnityEngine.Object)(object)other)
		{
			return TeamRelation.Own;
		}
		if (((UnityEngine.Object)(object)this == (UnityEngine.Object)(object)creep && (UnityEngine.Object)(object)other == (UnityEngine.Object)(object)environment) || ((UnityEngine.Object)(object)this == (UnityEngine.Object)(object)environment && (UnityEngine.Object)(object)other == (UnityEngine.Object)(object)creep))
		{
			return TeamRelation.Ally;
		}
		if ((UnityEngine.Object)(object)this == (UnityEngine.Object)(object)environment || (UnityEngine.Object)(object)other == (UnityEngine.Object)(object)environment)
		{
			return TeamRelation.Neutral;
		}
		if ((UnityEngine.Object)(object)this == (UnityEngine.Object)(object)creep || (UnityEngine.Object)(object)other == (UnityEngine.Object)(object)creep)
		{
			return TeamRelation.Enemy;
		}
		if (isHumanPlayer && other.isHumanPlayer)
		{
			return TeamRelation.Ally;
		}
		return TeamRelation.Neutral;
	}

	public bool CheckEnemyOrNeutral(Entity target)
	{
		TeamRelation teamRelation = GetTeamRelation(target);
		if (teamRelation != TeamRelation.Enemy)
		{
			return teamRelation == TeamRelation.Neutral;
		}
		return true;
	}

	[TargetRpc]
	public void SendLog(string message)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteString((NetworkWriter)(object)val, message);
		((NetworkBehaviour)this).SendTargetRPCInternal((NetworkConnection)null, "System.Void DewPlayer::SendLog(System.String)", -1656474440, (NetworkWriter)(object)val, 0);
		NetworkWriterPool.Return(val);
	}

	[TargetRpc]
	public void SendLogWarning(string message)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteString((NetworkWriter)(object)val, message);
		((NetworkBehaviour)this).SendTargetRPCInternal((NetworkConnection)null, "System.Void DewPlayer::SendLogWarning(System.String)", 286198702, (NetworkWriter)(object)val, 0);
		NetworkWriterPool.Return(val);
	}

	[TargetRpc]
	public void SendLogError(string message)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteString((NetworkWriter)(object)val, message);
		((NetworkBehaviour)this).SendTargetRPCInternal((NetworkConnection)null, "System.Void DewPlayer::SendLogError(System.String)", -1800275454, (NetworkWriter)(object)val, 0);
		NetworkWriterPool.Return(val);
	}

	private void OnPlayerNameRawChanged(string oldName, string newName)
	{
		playerName = DewSafety.FilterProfanityIfEnabled(playerNameRaw);
		UpdateGameObjectName();
	}

	private void UpdateGameObjectName()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		((UnityEngine.Object)(object)this).name = (((NetworkBehaviour)this).isLocalPlayer ? "HumanPlayerLocal" : "HumanPlayer") + ((steamId != default(CSteamID)) ? $"({steamId.m_SteamID})" : "") + " " + playerName;
	}

	[Command]
	public void CmdPing()
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendCommandInternal("System.Void DewPlayer::CmdPing()", 1804192186, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	public void OnSettingsChanged()
	{
		playerName = DewSafety.FilterProfanityIfEnabled(playerNameRaw);
		if ((bool)(UnityEngine.Object)(object)((NetworkBehaviour)this).netIdentity && ((NetworkBehaviour)this).isClient && ((NetworkBehaviour)this).isLocalPlayer)
		{
			CmdUpdateShareItemsWhenDropped();
		}
	}

	[Server]
	public void PurgeRelationsList()
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogWarning("[Server] function 'System.Void DewPlayer::PurgeRelationsList()' called when server was not active");
			return;
		}
		Purge(neutrals);
		Purge(allies);
		Purge(enemies);
		static void Purge(SyncList<DewPlayer> list)
		{
			for (int num = list.Count - 1; num >= 0; num--)
			{
				if ((UnityEngine.Object)(object)list[num] == null || !((NetworkBehaviour)list[num]).isServer)
				{
					list.RemoveAt(num);
				}
			}
		}
	}

	public void CmdRequestOverrides()
	{
		if (!((NetworkBehaviour)this).isServer && ManagerBase<LobbyManager>.instance.service.currentLobby != null && ManagerBase<LobbyManager>.instance.service.currentLobby.isModded)
		{
			_isLocalRequestingOverrides = true;
			UnityEngine.Debug.Log("Joined modded lobby. Requesting overrides...");
			CmdRequestOverrides_Imp();
		}
	}

	[Command]
	private void CmdRequestOverrides_Imp()
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendCommandInternal("System.Void DewPlayer::CmdRequestOverrides_Imp()", -1860235059, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[TargetRpc]
	private void TpcSendOverrides(List<JsonOverrideItem> overrides)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		GeneratedNetworkCode._Write_System_002ECollections_002EGeneric_002EList_00601_003CJsonOverrideItem_003E((NetworkWriter)(object)val, overrides);
		((NetworkBehaviour)this).SendTargetRPCInternal((NetworkConnection)null, "System.Void DewPlayer::TpcSendOverrides(System.Collections.Generic.List`1<JsonOverrideItem>)", -1337204467, (NetworkWriter)(object)val, 0);
		NetworkWriterPool.Return(val);
	}

	public override void OnStartLocalPlayer()
	{
		((NetworkBehaviour)this).OnStartLocalPlayer();
		CmdSetPlayerIds(GetMyEosId(), GetMyPlatformId(), GetMyPlatform());
	}

	[Command]
	private void CmdSetPlayerIds(string eosId, string platformId, string platform)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteString((NetworkWriter)(object)val, eosId);
		NetworkWriterExtensions.WriteString((NetworkWriter)(object)val, platformId);
		NetworkWriterExtensions.WriteString((NetworkWriter)(object)val, platform);
		((NetworkBehaviour)this).SendCommandInternal("System.Void DewPlayer::CmdSetPlayerIds(System.String,System.String,System.String)", -2057442883, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[Server]
	public void SetEosIdServer(string eosId)
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogWarning("[Server] function 'System.Void DewPlayer::SetEosIdServer(System.String)' called when server was not active");
		}
		else if (!string.IsNullOrEmpty(eosId))
		{
			Network_eosId = eosId;
		}
	}

	private void OnEosIdChanged(string oldValue, string newValue)
	{
		RevalidateAllItems();
	}

	private void OnPlatformIdChanged(string oldValue, string newValue)
	{
	}

	private void OnPlatformChanged(string oldValue, string newValue)
	{
	}

	private string GetMyEosId()
	{
		return ((object)EOSSDKComponent.LocalUserProductId)?.ToString();
	}

	private string GetMyPlatformId()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		return ((object)DewSteam.steamId/*cast due to constrained. prefix*/).ToString();
	}

	private string GetMyPlatform()
	{
		return "steam";
	}

	private void OnHeroChanged(Hero oldHero, Hero newHero)
	{
		ClientEvent_OnHeroChanged?.Invoke(oldHero, newHero);
		if ((UnityEngine.Object)(object)oldHero != null)
		{
			oldHero.ClientActorEvent_OnDestroyed -= new Action<Actor>(OnHeroDestroyed);
		}
		if ((UnityEngine.Object)(object)newHero != null)
		{
			newHero.ClientActorEvent_OnDestroyed += new Action<Actor>(OnHeroDestroyed);
		}
		if ((UnityEngine.Object)(object)newHero != null && ((NetworkBehaviour)this).isLocalPlayer)
		{
			CmdUpdateShareItemsWhenDropped();
		}
	}

	private void OnHeroDestroyed(Actor obj)
	{
		if ((UnityEngine.Object)(object)Network_003Chero_003Ek__BackingField == (UnityEngine.Object)(object)obj)
		{
			Network_003Chero_003Ek__BackingField = null;
		}
	}

	private void OnGoldChanged(int oldValue, int newValue)
	{
		ClientEvent_OnGoldChanged?.Invoke(oldValue, newValue);
	}

	private void OnDreamDustChanged(int oldValue, int newValue)
	{
		ClientEvent_OnDreamDustChanged?.Invoke(oldValue, newValue);
	}

	private void OnPlatinumCoinChanged(int oldValue, int newValue)
	{
		ClientEvent_OnPlatinumCoinChanged?.Invoke(oldValue, newValue);
	}

	private void LogicUpdate_InGame()
	{
		if (!((NetworkBehaviour)this).isLocalPlayer || !NetworkClient.ready || (UnityEngine.Object)(object)NetworkedManagerBase<GameManager>.softInstance == null)
		{
			return;
		}
		try
		{
			Vector3 vector = (cursorWorldPos = ((DewInput.currentMode != InputMode.Gamepad) ? ControlManager.GetWorldPositionOnGroundOnCursor() : (((UnityEngine.Object)(object)Network_003Chero_003Ek__BackingField == null) ? Vector3.zero : (ManagerBase<ControlManager>.instance.aimPoint.HasValue ? ManagerBase<ControlManager>.instance.aimPoint.Value : ((!ManagerBase<ControlManager>.instance.isLastMovementDirectionFresh) ? (Network_003Chero_003Ek__BackingField.agentPosition + ((Component)(object)Network_003Chero_003Ek__BackingField).transform.forward * 8f) : (Network_003Chero_003Ek__BackingField.agentPosition + ManagerBase<ControlManager>.instance.lastMovementDirection * 8f))))));
			InputMode currentMode = DewInput.currentMode;
			bool hasValue = ManagerBase<ControlManager>.instance.aimPoint.HasValue;
			Entity targetEnemy = ManagerBase<ControlManager>.instance.targetEnemy;
			bool flag = ManagerBase<CameraManager>.instance.isPlayingCutscene;
			bool num = !_hasNotifiedAtLeastOnce || currentMode != _lastNotifiedMode || hasValue != _lastNotifiedIsExplicit || (UnityEngine.Object)(object)targetEnemy != (UnityEngine.Object)(object)_lastNotifiedTargetEnemy || flag != _lastNotifiedPlayingCutscene || (vector - _lastNotifiedPos).sqrMagnitude > 0.01f;
			bool flag2 = Time.time - _lastNotifyClientStatusTime >= 0.05f;
			if (num || flag2)
			{
				_lastNotifyClientStatusTime = Time.time;
				_lastNotifiedPos = vector;
				_lastNotifiedMode = currentMode;
				_lastNotifiedIsExplicit = hasValue;
				_lastNotifiedTargetEnemy = targetEnemy;
				_lastNotifiedPlayingCutscene = flag;
				_hasNotifiedAtLeastOnce = true;
				CmdNotifyClientStatus(vector, currentMode, hasValue, targetEnemy, flag);
			}
		}
		catch (Exception)
		{
		}
	}

	[Command]
	private void CmdNotifyClientStatus(Vector3 pos, InputMode mode, bool isExplicit, Entity targetEnemy, bool playingCutscene)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteVector3((NetworkWriter)(object)val, pos);
		GeneratedNetworkCode._Write_InputMode((NetworkWriter)(object)val, mode);
		NetworkWriterExtensions.WriteBool((NetworkWriter)(object)val, isExplicit);
		NetworkWriterExtensions.WriteNetworkBehaviour((NetworkWriter)(object)val, (NetworkBehaviour)(object)targetEnemy);
		NetworkWriterExtensions.WriteBool((NetworkWriter)(object)val, playingCutscene);
		((NetworkBehaviour)this).SendCommandInternal("System.Void DewPlayer::CmdNotifyClientStatus(UnityEngine.Vector3,InputMode,System.Boolean,Entity,System.Boolean)", 561369997, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	public TeamRelation GetTeamRelation(Entity other)
	{
		return GetTeamRelation(other.owner);
	}

	private void SelectedEntityChanged(Entity before, Entity after)
	{
		if (!(ManagerBase<ControlManager>.instance == null) && ((NetworkBehaviour)this).isLocalPlayer)
		{
			ManagerBase<ControlManager>.instance.onSelectedEntityChanged?.Invoke(before, after);
		}
	}

	[TargetRpc]
	public void TpcShowWorldPopMessage(WorldMessageSetting message)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		GeneratedNetworkCode._Write_WorldMessageSetting((NetworkWriter)(object)val, message);
		((NetworkBehaviour)this).SendTargetRPCInternal((NetworkConnection)null, "System.Void DewPlayer::TpcShowWorldPopMessage(WorldMessageSetting)", 728588415, (NetworkWriter)(object)val, 0);
		NetworkWriterPool.Return(val);
	}

	[TargetRpc]
	public void TpcShowCenterMessage(CenterMessageType type, string key)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		GeneratedNetworkCode._Write_CenterMessageType((NetworkWriter)(object)val, type);
		NetworkWriterExtensions.WriteString((NetworkWriter)(object)val, key);
		((NetworkBehaviour)this).SendTargetRPCInternal((NetworkConnection)null, "System.Void DewPlayer::TpcShowCenterMessage(CenterMessageType,System.String)", 463458304, (NetworkWriter)(object)val, 0);
		NetworkWriterPool.Return(val);
	}

	[TargetRpc]
	public void TpcNotifyDejavuUse()
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendTargetRPCInternal((NetworkConnection)null, "System.Void DewPlayer::TpcNotifyDejavuUse()", -600899820, (NetworkWriter)(object)val, 0);
		NetworkWriterPool.Return(val);
	}

	[TargetRpc]
	public void TpcShowCenterMessage(CenterMessageType type, string key, string[] localizedFormatArgs)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		GeneratedNetworkCode._Write_CenterMessageType((NetworkWriter)(object)val, type);
		NetworkWriterExtensions.WriteString((NetworkWriter)(object)val, key);
		GeneratedNetworkCode._Write_System_002EString_005B_005D((NetworkWriter)(object)val, localizedFormatArgs);
		((NetworkBehaviour)this).SendTargetRPCInternal((NetworkConnection)null, "System.Void DewPlayer::TpcShowCenterMessage(CenterMessageType,System.String,System.String[])", -657416102, (NetworkWriter)(object)val, 0);
		NetworkWriterPool.Return(val);
	}

	[Server]
	public void GiveStardust(int amount)
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogWarning("[Server] function 'System.Void DewPlayer::GiveStardust(System.Int32)' called when server was not active");
		}
		else
		{
			RpcGiveStardust(amount);
		}
	}

	[Command]
	public void CmdRequestStardust(int amount)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteInt((NetworkWriter)(object)val, amount);
		((NetworkBehaviour)this).SendCommandInternal("System.Void DewPlayer::CmdRequestStardust(System.Int32)", 1004913566, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[Command]
	public void CmdGiveCurrency(int goldAmount, int dreamDustAmount, DewPlayer target)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteInt((NetworkWriter)(object)val, goldAmount);
		NetworkWriterExtensions.WriteInt((NetworkWriter)(object)val, dreamDustAmount);
		NetworkWriterExtensions.WriteNetworkBehaviour((NetworkWriter)(object)val, (NetworkBehaviour)(object)target);
		((NetworkBehaviour)this).SendCommandInternal("System.Void DewPlayer::CmdGiveCurrency(System.Int32,System.Int32,DewPlayer)", -1970337615, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[ClientRpc]
	private void RpcGiveStardust(int amount)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteInt((NetworkWriter)(object)val, amount);
		((NetworkBehaviour)this).SendRPCInternal("System.Void DewPlayer::RpcGiveStardust(System.Int32)", -1224848007, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[Server]
	public void Spend(Cost cost)
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogWarning("[Server] function 'System.Void DewPlayer::Spend(Cost)' called when server was not active");
			return;
		}
		if (cost.gold > 0)
		{
			SpendGold(cost.gold);
		}
		if (cost.dreamDust > 0)
		{
			SpendDreamDust(cost.dreamDust);
		}
		if (cost.healthPercentage > 0)
		{
			NetworkedManagerBase<ActorManager>.instance.serverActor.CreateStatusEffect(Network_003Chero_003Ek__BackingField, default, (Se_HealthCost h) =>
			{
				h.totalAmount = Network_003Chero_003Ek__BackingField.maxHealth * (float)cost.healthPercentage / 100f;
			});
		}
		if (cost.platinumCoin > 0)
		{
			Network_003CplatinumCoin_003Ek__BackingField = Mathf.Max(0, platinumCoin - cost.platinumCoin);
		}
	}

	[Server]
	public void SpendGold(int amount)
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogWarning("[Server] function 'System.Void DewPlayer::SpendGold(System.Int32)' called when server was not active");
			return;
		}
		amount = Mathf.Min(amount, gold);
		if (amount > 0)
		{
			gold -= amount;
			RpcInvokeOnSpendGold(amount);
		}
	}

	[Server]
	public void EarnGold(int amount)
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogWarning("[Server] function 'System.Void DewPlayer::EarnGold(System.Int32)' called when server was not active");
		}
		else if (amount > 0)
		{
			AddGold(amount);
			RpcInvokeOnEarnGold(amount);
		}
	}

	[Server]
	public void AddGold(int amount)
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogWarning("[Server] function 'System.Void DewPlayer::AddGold(System.Int32)' called when server was not active");
		}
		else if (amount > 0)
		{
			long val = (long)gold + (long)amount;
			Network_003Cgold_003Ek__BackingField = (int)Math.Min(val, 2147483647L);
		}
	}

	[Server]
	public void SpendDreamDust(int amount)
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogWarning("[Server] function 'System.Void DewPlayer::SpendDreamDust(System.Int32)' called when server was not active");
			return;
		}
		amount = Mathf.Min(amount, dreamDust);
		if (amount > 0)
		{
			dreamDust -= amount;
			RpcInvokeOnSpendDreamDust(amount);
		}
	}

	[Server]
	public void EarnDreamDust(int amount)
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogWarning("[Server] function 'System.Void DewPlayer::EarnDreamDust(System.Int32)' called when server was not active");
		}
		else if (amount > 0)
		{
			AddDreamDust(amount);
			RpcInvokeOnEarnDreamDust(amount);
		}
	}

	[Server]
	public void AddDreamDust(int amount)
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogWarning("[Server] function 'System.Void DewPlayer::AddDreamDust(System.Int32)' called when server was not active");
		}
		else if (amount > 0)
		{
			long val = (long)dreamDust + (long)amount;
			Network_003CdreamDust_003Ek__BackingField = (int)Math.Min(val, 2147483647L);
		}
	}

	[ClientRpc]
	private void RpcInvokeOnSpendGold(int amount)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteInt((NetworkWriter)(object)val, amount);
		((NetworkBehaviour)this).SendRPCInternal("System.Void DewPlayer::RpcInvokeOnSpendGold(System.Int32)", -900971321, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[ClientRpc]
	public void RpcInvokeOnSpendStardust(int amount)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteInt((NetworkWriter)(object)val, amount);
		((NetworkBehaviour)this).SendRPCInternal("System.Void DewPlayer::RpcInvokeOnSpendStardust(System.Int32)", 129993451, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[ClientRpc]
	private void RpcInvokeOnEarnGold(int amount)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteInt((NetworkWriter)(object)val, amount);
		((NetworkBehaviour)this).SendRPCInternal("System.Void DewPlayer::RpcInvokeOnEarnGold(System.Int32)", -1776477517, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[ClientRpc]
	private void RpcInvokeOnSpendDreamDust(int amount)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteInt((NetworkWriter)(object)val, amount);
		((NetworkBehaviour)this).SendRPCInternal("System.Void DewPlayer::RpcInvokeOnSpendDreamDust(System.Int32)", 257724146, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[ClientRpc]
	private void RpcInvokeOnEarnDreamDust(int amount)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteInt((NetworkWriter)(object)val, amount);
		((NetworkBehaviour)this).SendRPCInternal("System.Void DewPlayer::RpcInvokeOnEarnDreamDust(System.Int32)", 813730950, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[ClientRpc]
	private void RpcInvokeOnGiveCurrency(int goldAmount, int dreamDustAmount, DewPlayer target)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteInt((NetworkWriter)(object)val, goldAmount);
		NetworkWriterExtensions.WriteInt((NetworkWriter)(object)val, dreamDustAmount);
		NetworkWriterExtensions.WriteNetworkBehaviour((NetworkWriter)(object)val, (NetworkBehaviour)(object)target);
		((NetworkBehaviour)this).SendRPCInternal("System.Void DewPlayer::RpcInvokeOnGiveCurrency(System.Int32,System.Int32,DewPlayer)", 1832206767, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	private void LogicUpdate_InGame_NetworkedOnScreenTimer()
	{
		if (!NetworkServer.isLoadingScene && NetworkClient.ready && !((UnityEngine.Object)(object)NetworkedManagerBase<GameManager>.softInstance == null) && ((NetworkBehaviour)this).isServer)
		{
			for (int i = 0; i < _onScreenTimersServer.Count; i++)
			{
				TpcSetNetworkedOnScreenTimerValue(_onScreenTimersServer[i]._id, _onScreenTimersServer[i].valueGetter());
			}
		}
	}

	[Server]
	public void ShowOnScreenTimer(NetworkedOnScreenTimerHandle handle)
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogWarning("[Server] function 'System.Void DewPlayer::ShowOnScreenTimer(NetworkedOnScreenTimerHandle)' called when server was not active");
		}
		else if (!((UnityEngine.Object)(object)NetworkedManagerBase<GameManager>.softInstance == null) && isHumanPlayer)
		{
			int id = (handle._id = _nextOnScreenTimerId++);
			_onScreenTimersServer.Add(handle);
			TpcCreateNetworkedOnScreenTimer(id, handle, handle.valueGetter());
		}
	}

	[Server]
	public void HideOnScreenTimer(NetworkedOnScreenTimerHandle handle)
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogWarning("[Server] function 'System.Void DewPlayer::HideOnScreenTimer(NetworkedOnScreenTimerHandle)' called when server was not active");
			return;
		}
		_onScreenTimersServer.Remove(handle);
		TpcRemoveNetworkedOnScreenTimer(handle._id);
	}

	[TargetRpc]
	private void TpcCreateNetworkedOnScreenTimer(int id, NetworkedOnScreenTimerHandle handle, float defaultValue)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteInt((NetworkWriter)(object)val, id);
		GeneratedNetworkCode._Write_NetworkedOnScreenTimerHandle((NetworkWriter)(object)val, handle);
		NetworkWriterExtensions.WriteFloat((NetworkWriter)(object)val, defaultValue);
		((NetworkBehaviour)this).SendTargetRPCInternal((NetworkConnection)null, "System.Void DewPlayer::TpcCreateNetworkedOnScreenTimer(System.Int32,NetworkedOnScreenTimerHandle,System.Single)", 1344470087, (NetworkWriter)(object)val, 0);
		NetworkWriterPool.Return(val);
	}

	[TargetRpc]
	private void TpcSetNetworkedOnScreenTimerValue(int id, float value)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteInt((NetworkWriter)(object)val, id);
		NetworkWriterExtensions.WriteFloat((NetworkWriter)(object)val, value);
		((NetworkBehaviour)this).SendTargetRPCInternal((NetworkConnection)null, "System.Void DewPlayer::TpcSetNetworkedOnScreenTimerValue(System.Int32,System.Single)", 1995958413, (NetworkWriter)(object)val, 0);
		NetworkWriterPool.Return(val);
	}

	[TargetRpc]
	private void TpcRemoveNetworkedOnScreenTimer(int id)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteInt((NetworkWriter)(object)val, id);
		((NetworkBehaviour)this).SendTargetRPCInternal((NetworkConnection)null, "System.Void DewPlayer::TpcRemoveNetworkedOnScreenTimer(System.Int32)", -2043110025, (NetworkWriter)(object)val, 0);
		NetworkWriterPool.Return(val);
	}

	[Command]
	internal void DispatchSample_Cast(CastInfo info)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		GeneratedNetworkCode._Write_CastInfo((NetworkWriter)(object)val, info);
		((NetworkBehaviour)this).SendCommandInternal("System.Void DewPlayer::DispatchSample_Cast(CastInfo)", -622201125, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[Command]
	internal void DispatchSample_Update(CastInfo info)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		GeneratedNetworkCode._Write_CastInfo((NetworkWriter)(object)val, info);
		((NetworkBehaviour)this).SendCommandInternal("System.Void DewPlayer::DispatchSample_Update(CastInfo)", -682732059, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[Server]
	internal void StartSampleCastInfo(SampleCastInfoContext context)
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogWarning("[Server] function 'System.Void DewPlayer::StartSampleCastInfo(SampleCastInfoContext)' called when server was not active");
			return;
		}
		if (!isHumanPlayer)
		{
			try
			{
				context.cancelCallback();
				return;
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
				return;
			}
		}
		if (_currentSampleContext.HasValue)
		{
			CancelSampleCastInfo();
		}
		_currentSampleContext = context;
		TpcSetSampleContext(context);
	}

	[TargetRpc]
	private void TpcSetSampleContext(SampleCastInfoContext? context)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkWriter)(object)val).WriteSampleCastInfoContext(context);
		((NetworkBehaviour)this).SendTargetRPCInternal((NetworkConnection)null, "System.Void DewPlayer::TpcSetSampleContext(System.Nullable`1<SampleCastInfoContext>)", -560506649, (NetworkWriter)(object)val, 0);
		NetworkWriterPool.Return(val);
	}

	[TargetRpc]
	private void RpcSetCastMethod(CastMethodData method)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkWriter)(object)val).WriteCastMethodData(method);
		((NetworkBehaviour)this).SendTargetRPCInternal((NetworkConnection)null, "System.Void DewPlayer::RpcSetCastMethod(CastMethodData)", -1137566233, (NetworkWriter)(object)val, 0);
		NetworkWriterPool.Return(val);
	}

	[Server]
	internal void UpdateSampleCastInfo(CastMethodData castMethod)
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogWarning("[Server] function 'System.Void DewPlayer::UpdateSampleCastInfo(CastMethodData)' called when server was not active");
		}
		else if (_currentSampleContext.HasValue)
		{
			SampleCastInfoContext value = _currentSampleContext.Value;
			value.castMethod = castMethod;
			_currentSampleContext = value;
			RpcSetCastMethod(castMethod);
		}
	}

	[Server]
	internal void StopSampleCastInfo()
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogWarning("[Server] function 'System.Void DewPlayer::StopSampleCastInfo()' called when server was not active");
			return;
		}
		_currentSampleContext = null;
		TpcSetSampleContext(null);
	}

	private void CancelSampleCastInfo()
	{
		if (_currentSampleContext.HasValue)
		{
			try
			{
				_currentSampleContext.Value.cancelCallback?.Invoke();
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
			StopSampleCastInfo();
		}
	}

	[Command]
	private void PlaceholderFunction(SampleCastInfoContext context)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		GeneratedNetworkCode._Write_SampleCastInfoContext((NetworkWriter)(object)val, context);
		((NetworkBehaviour)this).SendCommandInternal("System.Void DewPlayer::PlaceholderFunction(SampleCastInfoContext)", 899946753, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[Command]
	public void CmdAuthorizeForUse(string ownershipKey)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteString((NetworkWriter)(object)val, ownershipKey);
		((NetworkBehaviour)this).SendCommandInternal("System.Void DewPlayer::CmdAuthorizeForUse(System.String)", 1599954497, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[Conditional("ITEM_AUTH")]
	private static void LogItemAuth(string message)
	{
		UnityEngine.Debug.Log(message);
	}

	private void Awake_ItemAuth()
	{
		ownershipKeys.Callback += OwnershipKeysOnCallback;
	}

	private void OnStartClient_ItemAuth()
	{
		if (!((NetworkBehaviour)this).isLocalPlayer)
		{
			return;
		}
		foreach (KeyValuePair<string, DewProfile.CosmeticsData> item in DewSave.profileMain.accessories.Concat(DewSave.profileMain.emotes).Concat(DewSave.profileMain.nametags).Concat(DewSave.profileMain.skins))
		{
			if (!string.IsNullOrEmpty(item.Value.ownershipKey) && DewItem.IsItemGeneratedFromServer(item.Key))
			{
				CmdAuthorizeForUse(item.Value.ownershipKey);
			}
		}
	}

	public void RevalidateAllItems()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		Enumerator<string> enumerator = ownershipKeys.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				string current = enumerator.Current;
				OwnershipKeysOnCallback((Operation<string>)0, -1, null, current);
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
	}

	private void OwnershipKeysOnCallback(Operation<string> op, int itemindex, string olditem, string newitem)
	{
		if ((steamId.m_SteamID != 0L || !string.IsNullOrEmpty(EOSID)) && !string.IsNullOrEmpty(newitem))
		{
			DecryptedItemData decryptedItemData = DewItem.GetDecryptedItemData(newitem);
			if (decryptedItemData != null && !ownedItems.Contains(decryptedItemData.item) && ValidateOwnershipData(decryptedItemData))
			{
				ownedItems.Add(decryptedItemData.item);
			}
		}
	}

	private bool ValidateOwnershipData(DecryptedItemData data)
	{
		if (string.IsNullOrEmpty(data.owner) || string.IsNullOrEmpty(data.item))
		{
			return false;
		}
		bool num = steamId.m_SteamID != 0L && data.owner == steamId.m_SteamID.ToString();
		bool flag = !string.IsNullOrEmpty(EOSID) && data.owner == "eos|" + EOSID;
		if (!num && !flag)
		{
			return false;
		}
		if (DewSteam.isInitialized && data.IsExpired())
		{
			return false;
		}
		return true;
	}

	public bool IsAllowedToUseItem(string itemName)
	{
		if (string.IsNullOrEmpty(itemName))
		{
			return false;
		}
		if (itemName.StartsWith("Acc_"))
		{
			Accessory byName = DewResources.GetByName<Accessory>(itemName);
			if (byName == null)
			{
				return false;
			}
			if (!byName.generatedFromServer)
			{
				return true;
			}
			return ownedItems.Contains(itemName);
		}
		if (itemName.StartsWith("Nametag_"))
		{
			Nametag byName2 = DewResources.GetByName<Nametag>(itemName);
			if (byName2 == null)
			{
				return false;
			}
			if (!byName2.generatedFromServer)
			{
				return true;
			}
			return ownedItems.Contains(itemName);
		}
		if (itemName.StartsWith("Emote_"))
		{
			Emote byName3 = DewResources.GetByName<Emote>(itemName);
			if (byName3 == null)
			{
				return false;
			}
			if (!byName3.generatedFromServer)
			{
				return true;
			}
			return ownedItems.Contains(itemName);
		}
		if (itemName.StartsWith("Skin_"))
		{
			Skin byName4 = DewResources.GetByName<Skin>(itemName);
			if (byName4 == null)
			{
				return false;
			}
			if (!byName4.generatedFromServer)
			{
				return true;
			}
			return ownedItems.Contains(itemName);
		}
		return false;
	}

	private void OnHeroTypeChanged(string oldVal, string newVal)
	{
		ClientEvent_OnSelectedHeroTypeChanged?.Invoke(newVal);
	}

	private void OnSkinChanged(string oldVal, string newVal)
	{
		ClientEvent_OnSelectedSkinChanged?.Invoke(newVal);
	}

	private void OnIsReadyChanged(bool oldVal, bool newVal)
	{
		ClientEvent_OnIsReadyChanged?.Invoke(newVal);
	}

	private void OnStart_Lobby()
	{
		selectedAccessories.Callback += (Operation<string> op, int index, string item, string newItem) =>
		{
			ClientEvent_OnSelectedAccessoriesChanged?.Invoke();
		};
	}

	private void LogicUpdate_Lobby()
	{
		if (((NetworkBehaviour)this).isServer && !NetworkedManagerBase<GameSettingsManager>.instance.allowDejavu && !string.IsNullOrEmpty(selectedDejavuItem))
		{
			Network_003CselectedDejavuItem_003Ek__BackingField = null;
		}
	}

	[Command]
	public void CmdSetAccessories(List<string> accessories)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		GeneratedNetworkCode._Write_System_002ECollections_002EGeneric_002EList_00601_003CSystem_002EString_003E((NetworkWriter)(object)val, accessories);
		((NetworkBehaviour)this).SendCommandInternal("System.Void DewPlayer::CmdSetAccessories(System.Collections.Generic.List`1<System.String>)", -1792256470, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	private void OnLoadoutChanged(HeroLoadoutData oldVal, HeroLoadoutData newVal)
	{
		ClientEvent_OnSelectedLoadoutChanged?.Invoke(newVal);
	}

	private void OnDejavuItemChanged(string oldVal, string newVal)
	{
		ClientEvent_OnSelectedDejavuItemChanged?.Invoke(newVal);
	}

	public void CmdSetHeroType(string newType)
	{
		if (((NetworkBehaviour)this).isLocalPlayer)
		{
			HeroLoadoutData source = DewSave.profileMain.heroLoadouts[newType][NetworkedManagerBase<GameSettingsManager>.instance.GetLocalPreferredGameSettings().heroSelectedLoadoutIndex[newType]];
			List<string> accessories = DewSave.profileMain.heroEquippedAccs[newType];
			string skin = DewSave.profileMain.heroSelectedSkins[newType];
			source = new HeroLoadoutData(source);
			source.PopulateLevelsByLocalSaveData();
			CmdSetHeroType_Imp(newType, source, accessories, skin);
		}
	}

	[Command]
	private void CmdSetHeroType_Imp(string newType, HeroLoadoutData loadoutData, List<string> accessories, string skin)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteString((NetworkWriter)(object)val, newType);
		GeneratedNetworkCode._Write_HeroLoadoutData((NetworkWriter)(object)val, loadoutData);
		GeneratedNetworkCode._Write_System_002ECollections_002EGeneric_002EList_00601_003CSystem_002EString_003E((NetworkWriter)(object)val, accessories);
		NetworkWriterExtensions.WriteString((NetworkWriter)(object)val, skin);
		((NetworkBehaviour)this).SendCommandInternal("System.Void DewPlayer::CmdSetHeroType_Imp(System.String,HeroLoadoutData,System.Collections.Generic.List`1<System.String>,System.String)", 1394427357, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	public void CmdSetSkin(string skin)
	{
		if (state == PlayerState.InLobby)
		{
			DewSave.profileMain.heroSelectedSkins[selectedHeroType] = skin;
			CmdSetSkin_Imp(skin);
		}
	}

	[Command]
	private void CmdSetSkin_Imp(string skin)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteString((NetworkWriter)(object)val, skin);
		((NetworkBehaviour)this).SendCommandInternal("System.Void DewPlayer::CmdSetSkin_Imp(System.String)", -170852356, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	public void CmdSetHeroLoadoutData(HeroLoadoutData newData)
	{
		if (DewBuildProfile.current.buildType == BuildType.DemoLite)
		{
			CmdSetHeroLoadoutData_Imp(newData);
			return;
		}
		HeroLoadoutData heroLoadoutData = new HeroLoadoutData(newData);
		heroLoadoutData.PopulateLevelsByLocalSaveData();
		CmdSetHeroLoadoutData_Imp(heroLoadoutData);
	}

	[Command]
	private void CmdSetHeroLoadoutData_Imp(HeroLoadoutData newData)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		GeneratedNetworkCode._Write_HeroLoadoutData((NetworkWriter)(object)val, newData);
		((NetworkBehaviour)this).SendCommandInternal("System.Void DewPlayer::CmdSetHeroLoadoutData_Imp(HeroLoadoutData)", -1582242929, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[Command]
	public void CmdSetIsReady(bool newReady)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteBool((NetworkWriter)(object)val, newReady);
		((NetworkBehaviour)this).SendCommandInternal("System.Void DewPlayer::CmdSetIsReady(System.Boolean)", 77796566, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[Command]
	public void CmdRequestToJoinCurrentLobby()
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendCommandInternal("System.Void DewPlayer::CmdRequestToJoinCurrentLobby()", -1426181877, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[TargetRpc]
	public void TpcMakePlayerChangeLobby(string lobbyId)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteString((NetworkWriter)(object)val, lobbyId);
		((NetworkBehaviour)this).SendTargetRPCInternal((NetworkConnection)null, "System.Void DewPlayer::TpcMakePlayerChangeLobby(System.String)", -492420642, (NetworkWriter)(object)val, 0);
		NetworkWriterPool.Return(val);
	}

	[AsyncStateMachine(typeof(_003CChangeLobby_Imp_003Ed__355))]
	private UniTask ChangeLobby_Imp(string lobbyId)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		_003CChangeLobby_Imp_003Ed__355 obj = default;
		obj._003C_003Et__builder = AsyncUniTaskMethodBuilder.Create();
		obj.lobbyId = lobbyId;
		obj._003C_003E1__state = -1;
		obj._003C_003Et__builder.Start<_003CChangeLobby_Imp_003Ed__355>(ref obj);
		return obj._003C_003Et__builder.Task;
	}

	[Command]
	public void CmdSetProfileStats(DewProfileStats stats)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		GeneratedNetworkCode._Write_DewProfileStats((NetworkWriter)(object)val, stats);
		((NetworkBehaviour)this).SendCommandInternal("System.Void DewPlayer::CmdSetProfileStats(DewProfileStats)", -795219884, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	public static string[] GetLocalUnlockedGameItems()
	{
		List<string> list = new List<string>();
		foreach (KeyValuePair<string, DewProfile.UnlockData> hero in DewSave.profileMain.heroes)
		{
			if (hero.Value.isAvailableInGame && Dew.IsHeroIncludedInGame(hero.Key))
			{
				list.Add(hero.Key);
			}
		}
		foreach (KeyValuePair<string, DewProfile.UnlockData> gem in DewSave.profileMain.gems)
		{
			if (gem.Value.isAvailableInGame && Dew.IsGemIncludedInGame(gem.Key))
			{
				Gem byShortTypeName = DewResources.GetByShortTypeName<Gem>(gem.Key, new ResourceLoadSettings
				{
					loadLight = true
				});
				if (!((UnityEngine.Object)(object)byShortTypeName == null) && !byShortTypeName.excludeFromPool)
				{
					list.Add(gem.Key);
				}
			}
		}
		foreach (KeyValuePair<string, DewProfile.UnlockData> skill in DewSave.profileMain.skills)
		{
			if (skill.Value.isAvailableInGame && Dew.IsSkillIncludedInGame(skill.Key))
			{
				SkillTrigger byShortTypeName2 = DewResources.GetByShortTypeName<SkillTrigger>(skill.Key, new ResourceLoadSettings
				{
					loadLight = true
				});
				if (!((UnityEngine.Object)(object)byShortTypeName2 == null) && !byShortTypeName2.excludeFromPool && !byShortTypeName2.isCharacterSkill)
				{
					list.Add(skill.Key);
				}
			}
		}
		return list.ToArray();
	}

	public void CmdSetUnlockedGameItems()
	{
		CmdSetUnlockedGameItems_Imp(GetLocalUnlockedGameItems());
	}

	[Command]
	private void CmdSetUnlockedGameItems_Imp(string[] items)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		GeneratedNetworkCode._Write_System_002EString_005B_005D((NetworkWriter)(object)val, items);
		((NetworkBehaviour)this).SendCommandInternal("System.Void DewPlayer::CmdSetUnlockedGameItems_Imp(System.String[])", 1307791010, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[Command]
	public void CmdNotifyEveryInfoSet()
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendCommandInternal("System.Void DewPlayer::CmdNotifyEveryInfoSet()", -335793366, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[Command]
	private void PlaceholderFunction0(DewProfileStats.HeroData a, DewProfileStats.ItemData b, DewProfileStats.MonsterData c, DewProfileStats.ZoneData d)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		GeneratedNetworkCode._Write_DewProfileStats_002FHeroData((NetworkWriter)(object)val, a);
		GeneratedNetworkCode._Write_DewProfileStats_002FItemData((NetworkWriter)(object)val, b);
		GeneratedNetworkCode._Write_DewProfileStats_002FMonsterData((NetworkWriter)(object)val, c);
		GeneratedNetworkCode._Write_DewProfileStats_002FZoneData((NetworkWriter)(object)val, d);
		((NetworkBehaviour)this).SendCommandInternal("System.Void DewPlayer::PlaceholderFunction0(DewProfileStats/HeroData,DewProfileStats/ItemData,DewProfileStats/MonsterData,DewProfileStats/ZoneData)", 1256734030, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	private void LogicUpdate_MidJoin()
	{
		if (!((NetworkBehaviour)this).isServer || state != PlayerState.InLobby || isLoadingForMidJoin || NetworkedManagerBase<GameSettingsManager>.instance.state != GameState.InGame || NetworkedManagerBase<GameSettingsManager>.instance.allowMidJoins == AllowMidJoinType.Disallow || !NetworkedManagerBase<GameManager>.instance.playerRejoinData.ContainsKey(guid))
		{
			return;
		}
		if (NetworkedManagerBase<GameSettingsManager>.instance.midJoinWaitType != MidJoinWaitType.None)
		{
			if (!isWaitingForContinueMidJoin)
			{
				UnityEngine.Debug.Log(playerName + " has continue data, but mid-join wait flag is set. Making the player wait...");
				Network_003CisWaitingForContinueMidJoin_003Ek__BackingField = true;
			}
		}
		else
		{
			UnityEngine.Debug.Log("Preparing " + playerName + " for mid-join...");
			Network_003CisLoadingForMidJoin_003Ek__BackingField = true;
			Network_003CisWaitingForContinueMidJoin_003Ek__BackingField = false;
			TpcPrepareSceneBeforeMidJoin(SceneManager.GetActiveScene().name);
		}
		DewPersistence.PlayerData playerData = NetworkedManagerBase<GameManager>.instance.playerRejoinData[guid];
		Network_003CselectedHeroType_003Ek__BackingField = playerData.heroType;
		Network_003CselectedLoadout_003Ek__BackingField = new HeroLoadoutData();
	}

	[Command]
	public void CmdRequestMidJoin()
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendCommandInternal("System.Void DewPlayer::CmdRequestMidJoin()", 309437771, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[TargetRpc]
	private void TpcPrepareSceneBeforeMidJoin(string sceneName)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteString((NetworkWriter)(object)val, sceneName);
		((NetworkBehaviour)this).SendTargetRPCInternal((NetworkConnection)null, "System.Void DewPlayer::TpcPrepareSceneBeforeMidJoin(System.String)", 79188935, (NetworkWriter)(object)val, 0);
		NetworkWriterPool.Return(val);
	}

	[Command]
	public void CmdNotifyMidJoinReady(string sceneName)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteString((NetworkWriter)(object)val, sceneName);
		((NetworkBehaviour)this).SendCommandInternal("System.Void DewPlayer::CmdNotifyMidJoinReady(System.String)", -265770720, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[TargetRpc]
	private void TpcNotifyHeroSpawned()
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendTargetRPCInternal((NetworkConnection)null, "System.Void DewPlayer::TpcNotifyHeroSpawned()", 2132407940, (NetworkWriter)(object)val, 0);
		NetworkWriterPool.Return(val);
	}

	public DewPlayer()
	{
		((NetworkBehaviour)this).InitSyncObject((SyncObject)(object)allies);
		((NetworkBehaviour)this).InitSyncObject((SyncObject)(object)enemies);
		((NetworkBehaviour)this).InitSyncObject((SyncObject)(object)neutrals);
		((NetworkBehaviour)this).InitSyncObject((SyncObject)(object)ownershipKeys);
		((NetworkBehaviour)this).InitSyncObject((SyncObject)(object)selectedAccessories);
		((NetworkBehaviour)this).InitSyncObject((SyncObject)(object)unlockedGameItems);
		_Mirror_SyncVarHookDelegate__003Cstate_003Ek__BackingField = OnStateChanged;
		_Mirror_SyncVarHookDelegate__003CplayerNameRaw_003Ek__BackingField = OnPlayerNameRawChanged;
		_Mirror_SyncVarHookDelegate__equippedNametag = OnEquippedNametagChanged;
		_Mirror_SyncVarHookDelegate__eosId = OnEosIdChanged;
		_Mirror_SyncVarHookDelegate__platformId = OnPlatformIdChanged;
		_Mirror_SyncVarHookDelegate__platform = OnPlatformChanged;
		_Mirror_SyncVarHookDelegate__controllingEntity = SelectedEntityChanged;
		_Mirror_SyncVarHookDelegate__003Chero_003Ek__BackingField = OnHeroChanged;
		_Mirror_SyncVarHookDelegate__003Cgold_003Ek__BackingField = OnGoldChanged;
		_Mirror_SyncVarHookDelegate__003CdreamDust_003Ek__BackingField = OnDreamDustChanged;
		_Mirror_SyncVarHookDelegate__003CplatinumCoin_003Ek__BackingField = OnPlatinumCoinChanged;
		_Mirror_SyncVarHookDelegate__003CselectedHeroType_003Ek__BackingField = OnHeroTypeChanged;
		_Mirror_SyncVarHookDelegate__003CselectedLoadout_003Ek__BackingField = OnLoadoutChanged;
		_Mirror_SyncVarHookDelegate__003CselectedSkin_003Ek__BackingField = OnSkinChanged;
		_Mirror_SyncVarHookDelegate__003CselectedDejavuItem_003Ek__BackingField = OnDejavuItemChanged;
		_Mirror_SyncVarHookDelegate__003CisReady_003Ek__BackingField = OnIsReadyChanged;
	}

	static DewPlayer()
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Expected Obj, but got Unknown
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Expected Obj, but got Unknown
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Expected Obj, but got Unknown
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Expected Obj, but got Unknown
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Expected Obj, but got Unknown
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Expected Obj, but got Unknown
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Expected Obj, but got Unknown
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Expected Obj, but got Unknown
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Expected Obj, but got Unknown
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Expected Obj, but got Unknown
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Expected Obj, but got Unknown
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Expected Obj, but got Unknown
		//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d5: Expected Obj, but got Unknown
		//IL_01eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f6: Expected Obj, but got Unknown
		//IL_020c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0217: Expected Obj, but got Unknown
		//IL_022d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0238: Expected Obj, but got Unknown
		//IL_024e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0259: Expected Obj, but got Unknown
		//IL_026f: Unknown result type (might be due to invalid IL or missing references)
		//IL_027a: Expected Obj, but got Unknown
		//IL_0290: Unknown result type (might be due to invalid IL or missing references)
		//IL_029b: Expected Obj, but got Unknown
		//IL_02b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bc: Expected Obj, but got Unknown
		//IL_02d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02dd: Expected Obj, but got Unknown
		//IL_02f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fe: Expected Obj, but got Unknown
		//IL_0314: Unknown result type (might be due to invalid IL or missing references)
		//IL_031f: Expected Obj, but got Unknown
		//IL_0335: Unknown result type (might be due to invalid IL or missing references)
		//IL_0340: Expected Obj, but got Unknown
		//IL_0356: Unknown result type (might be due to invalid IL or missing references)
		//IL_0361: Expected Obj, but got Unknown
		//IL_0377: Unknown result type (might be due to invalid IL or missing references)
		//IL_0382: Expected Obj, but got Unknown
		//IL_0398: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a3: Expected Obj, but got Unknown
		//IL_03b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c4: Expected Obj, but got Unknown
		//IL_03da: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e4: Expected Obj, but got Unknown
		//IL_03fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0404: Expected Obj, but got Unknown
		//IL_041a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0424: Expected Obj, but got Unknown
		//IL_043a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0444: Expected Obj, but got Unknown
		//IL_045a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0464: Expected Obj, but got Unknown
		//IL_047a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0484: Expected Obj, but got Unknown
		//IL_049a: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a4: Expected Obj, but got Unknown
		//IL_04ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c4: Expected Obj, but got Unknown
		//IL_04da: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e4: Expected Obj, but got Unknown
		//IL_04fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0504: Expected Obj, but got Unknown
		//IL_051a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0524: Expected Obj, but got Unknown
		//IL_053a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0544: Expected Obj, but got Unknown
		//IL_055a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0564: Expected Obj, but got Unknown
		//IL_057a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0584: Expected Obj, but got Unknown
		//IL_059a: Unknown result type (might be due to invalid IL or missing references)
		//IL_05a4: Expected Obj, but got Unknown
		//IL_05ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_05c4: Expected Obj, but got Unknown
		//IL_05da: Unknown result type (might be due to invalid IL or missing references)
		//IL_05e4: Expected Obj, but got Unknown
		//IL_05fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0604: Expected Obj, but got Unknown
		//IL_061a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0624: Expected Obj, but got Unknown
		//IL_063a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0644: Expected Obj, but got Unknown
		//IL_065a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0664: Expected Obj, but got Unknown
		//IL_067a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0684: Expected Obj, but got Unknown
		//IL_069a: Unknown result type (might be due to invalid IL or missing references)
		//IL_06a4: Expected Obj, but got Unknown
		//IL_06ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_06c4: Expected Obj, but got Unknown
		allHumanPlayers = new List<DewPlayer>();
		lobbyPlayers = new List<DewPlayer>();
		gamePlayers = new List<DewPlayer>();
		spectators = new List<DewPlayer>();
		RemoteProcedureCalls.RegisterCommand(typeof(DewPlayer), "System.Void DewPlayer::CmdUpdateShareItemsWhenDropped_Imp(System.Boolean)", (RemoteCallDelegate)InvokeUserCode_CmdUpdateShareItemsWhenDropped_Imp__Boolean, true);
		RemoteProcedureCalls.RegisterCommand(typeof(DewPlayer), "System.Void DewPlayer::CmdUpdateHasPolarisEndingUnlocked_Imp(System.Boolean)", (RemoteCallDelegate)InvokeUserCode_CmdUpdateHasPolarisEndingUnlocked_Imp__Boolean, true);
		RemoteProcedureCalls.RegisterCommand(typeof(DewPlayer), "System.Void DewPlayer::CmdSetPlayerAvatar(UnityEngine.Texture2D)", (RemoteCallDelegate)InvokeUserCode_CmdSetPlayerAvatar__Texture2D, true);
		RemoteProcedureCalls.RegisterCommand(typeof(DewPlayer), "System.Void DewPlayer::CmdSetNametag_Imp(System.String)", (RemoteCallDelegate)InvokeUserCode_CmdSetNametag_Imp__String, true);
		RemoteProcedureCalls.RegisterCommand(typeof(DewPlayer), "System.Void DewPlayer::CmdSetDejavuItem(System.String)", (RemoteCallDelegate)InvokeUserCode_CmdSetDejavuItem__String, true);
		RemoteProcedureCalls.RegisterCommand(typeof(DewPlayer), "System.Void DewPlayer::SetUnlockedLucidDreams(System.String[])", (RemoteCallDelegate)InvokeUserCode_SetUnlockedLucidDreams__String_005B_005D, true);
		RemoteProcedureCalls.RegisterCommand(typeof(DewPlayer), "System.Void DewPlayer::CmdPing()", (RemoteCallDelegate)InvokeUserCode_CmdPing, true);
		RemoteProcedureCalls.RegisterCommand(typeof(DewPlayer), "System.Void DewPlayer::CmdRequestOverrides_Imp()", (RemoteCallDelegate)InvokeUserCode_CmdRequestOverrides_Imp, true);
		RemoteProcedureCalls.RegisterCommand(typeof(DewPlayer), "System.Void DewPlayer::CmdSetPlayerIds(System.String,System.String,System.String)", (RemoteCallDelegate)InvokeUserCode_CmdSetPlayerIds__String__String__String, true);
		RemoteProcedureCalls.RegisterCommand(typeof(DewPlayer), "System.Void DewPlayer::CmdNotifyClientStatus(UnityEngine.Vector3,InputMode,System.Boolean,Entity,System.Boolean)", (RemoteCallDelegate)InvokeUserCode_CmdNotifyClientStatus__Vector3__InputMode__Boolean__Entity__Boolean, true);
		RemoteProcedureCalls.RegisterCommand(typeof(DewPlayer), "System.Void DewPlayer::CmdRequestStardust(System.Int32)", (RemoteCallDelegate)InvokeUserCode_CmdRequestStardust__Int32, true);
		RemoteProcedureCalls.RegisterCommand(typeof(DewPlayer), "System.Void DewPlayer::CmdGiveCurrency(System.Int32,System.Int32,DewPlayer)", (RemoteCallDelegate)InvokeUserCode_CmdGiveCurrency__Int32__Int32__DewPlayer, true);
		RemoteProcedureCalls.RegisterCommand(typeof(DewPlayer), "System.Void DewPlayer::DispatchSample_Cast(CastInfo)", (RemoteCallDelegate)InvokeUserCode_DispatchSample_Cast__CastInfo, true);
		RemoteProcedureCalls.RegisterCommand(typeof(DewPlayer), "System.Void DewPlayer::DispatchSample_Update(CastInfo)", (RemoteCallDelegate)InvokeUserCode_DispatchSample_Update__CastInfo, true);
		RemoteProcedureCalls.RegisterCommand(typeof(DewPlayer), "System.Void DewPlayer::PlaceholderFunction(SampleCastInfoContext)", (RemoteCallDelegate)InvokeUserCode_PlaceholderFunction__SampleCastInfoContext, true);
		RemoteProcedureCalls.RegisterCommand(typeof(DewPlayer), "System.Void DewPlayer::CmdAuthorizeForUse(System.String)", (RemoteCallDelegate)InvokeUserCode_CmdAuthorizeForUse__String, true);
		RemoteProcedureCalls.RegisterCommand(typeof(DewPlayer), "System.Void DewPlayer::CmdSetAccessories(System.Collections.Generic.List`1<System.String>)", (RemoteCallDelegate)InvokeUserCode_CmdSetAccessories__List_00601, true);
		RemoteProcedureCalls.RegisterCommand(typeof(DewPlayer), "System.Void DewPlayer::CmdSetHeroType_Imp(System.String,HeroLoadoutData,System.Collections.Generic.List`1<System.String>,System.String)", (RemoteCallDelegate)InvokeUserCode_CmdSetHeroType_Imp__String__HeroLoadoutData__List_00601__String, true);
		RemoteProcedureCalls.RegisterCommand(typeof(DewPlayer), "System.Void DewPlayer::CmdSetSkin_Imp(System.String)", (RemoteCallDelegate)InvokeUserCode_CmdSetSkin_Imp__String, true);
		RemoteProcedureCalls.RegisterCommand(typeof(DewPlayer), "System.Void DewPlayer::CmdSetHeroLoadoutData_Imp(HeroLoadoutData)", (RemoteCallDelegate)InvokeUserCode_CmdSetHeroLoadoutData_Imp__HeroLoadoutData, true);
		RemoteProcedureCalls.RegisterCommand(typeof(DewPlayer), "System.Void DewPlayer::CmdSetIsReady(System.Boolean)", (RemoteCallDelegate)InvokeUserCode_CmdSetIsReady__Boolean, true);
		RemoteProcedureCalls.RegisterCommand(typeof(DewPlayer), "System.Void DewPlayer::CmdRequestToJoinCurrentLobby()", (RemoteCallDelegate)InvokeUserCode_CmdRequestToJoinCurrentLobby, true);
		RemoteProcedureCalls.RegisterCommand(typeof(DewPlayer), "System.Void DewPlayer::CmdSetProfileStats(DewProfileStats)", (RemoteCallDelegate)InvokeUserCode_CmdSetProfileStats__DewProfileStats, true);
		RemoteProcedureCalls.RegisterCommand(typeof(DewPlayer), "System.Void DewPlayer::CmdSetUnlockedGameItems_Imp(System.String[])", (RemoteCallDelegate)InvokeUserCode_CmdSetUnlockedGameItems_Imp__String_005B_005D, true);
		RemoteProcedureCalls.RegisterCommand(typeof(DewPlayer), "System.Void DewPlayer::CmdNotifyEveryInfoSet()", (RemoteCallDelegate)InvokeUserCode_CmdNotifyEveryInfoSet, true);
		RemoteProcedureCalls.RegisterCommand(typeof(DewPlayer), "System.Void DewPlayer::PlaceholderFunction0(DewProfileStats/HeroData,DewProfileStats/ItemData,DewProfileStats/MonsterData,DewProfileStats/ZoneData)", (RemoteCallDelegate)InvokeUserCode_PlaceholderFunction0__HeroData__ItemData__MonsterData__ZoneData, true);
		RemoteProcedureCalls.RegisterCommand(typeof(DewPlayer), "System.Void DewPlayer::CmdRequestMidJoin()", (RemoteCallDelegate)InvokeUserCode_CmdRequestMidJoin, true);
		RemoteProcedureCalls.RegisterCommand(typeof(DewPlayer), "System.Void DewPlayer::CmdNotifyMidJoinReady(System.String)", (RemoteCallDelegate)InvokeUserCode_CmdNotifyMidJoinReady__String, true);
		RemoteProcedureCalls.RegisterRpc(typeof(DewPlayer), "System.Void DewPlayer::RpcGiveStardust(System.Int32)", (RemoteCallDelegate)InvokeUserCode_RpcGiveStardust__Int32);
		RemoteProcedureCalls.RegisterRpc(typeof(DewPlayer), "System.Void DewPlayer::RpcInvokeOnSpendGold(System.Int32)", (RemoteCallDelegate)InvokeUserCode_RpcInvokeOnSpendGold__Int32);
		RemoteProcedureCalls.RegisterRpc(typeof(DewPlayer), "System.Void DewPlayer::RpcInvokeOnSpendStardust(System.Int32)", (RemoteCallDelegate)InvokeUserCode_RpcInvokeOnSpendStardust__Int32);
		RemoteProcedureCalls.RegisterRpc(typeof(DewPlayer), "System.Void DewPlayer::RpcInvokeOnEarnGold(System.Int32)", (RemoteCallDelegate)InvokeUserCode_RpcInvokeOnEarnGold__Int32);
		RemoteProcedureCalls.RegisterRpc(typeof(DewPlayer), "System.Void DewPlayer::RpcInvokeOnSpendDreamDust(System.Int32)", (RemoteCallDelegate)InvokeUserCode_RpcInvokeOnSpendDreamDust__Int32);
		RemoteProcedureCalls.RegisterRpc(typeof(DewPlayer), "System.Void DewPlayer::RpcInvokeOnEarnDreamDust(System.Int32)", (RemoteCallDelegate)InvokeUserCode_RpcInvokeOnEarnDreamDust__Int32);
		RemoteProcedureCalls.RegisterRpc(typeof(DewPlayer), "System.Void DewPlayer::RpcInvokeOnGiveCurrency(System.Int32,System.Int32,DewPlayer)", (RemoteCallDelegate)InvokeUserCode_RpcInvokeOnGiveCurrency__Int32__Int32__DewPlayer);
		RemoteProcedureCalls.RegisterRpc(typeof(DewPlayer), "System.Void DewPlayer::TpcKick()", (RemoteCallDelegate)InvokeUserCode_TpcKick);
		RemoteProcedureCalls.RegisterRpc(typeof(DewPlayer), "System.Void DewPlayer::SendLog(System.String)", (RemoteCallDelegate)InvokeUserCode_SendLog__String);
		RemoteProcedureCalls.RegisterRpc(typeof(DewPlayer), "System.Void DewPlayer::SendLogWarning(System.String)", (RemoteCallDelegate)InvokeUserCode_SendLogWarning__String);
		RemoteProcedureCalls.RegisterRpc(typeof(DewPlayer), "System.Void DewPlayer::SendLogError(System.String)", (RemoteCallDelegate)InvokeUserCode_SendLogError__String);
		RemoteProcedureCalls.RegisterRpc(typeof(DewPlayer), "System.Void DewPlayer::TpcSendOverrides(System.Collections.Generic.List`1<JsonOverrideItem>)", (RemoteCallDelegate)InvokeUserCode_TpcSendOverrides__List_00601);
		RemoteProcedureCalls.RegisterRpc(typeof(DewPlayer), "System.Void DewPlayer::TpcShowWorldPopMessage(WorldMessageSetting)", (RemoteCallDelegate)InvokeUserCode_TpcShowWorldPopMessage__WorldMessageSetting);
		RemoteProcedureCalls.RegisterRpc(typeof(DewPlayer), "System.Void DewPlayer::TpcShowCenterMessage(CenterMessageType,System.String)", (RemoteCallDelegate)InvokeUserCode_TpcShowCenterMessage__CenterMessageType__String);
		RemoteProcedureCalls.RegisterRpc(typeof(DewPlayer), "System.Void DewPlayer::TpcNotifyDejavuUse()", (RemoteCallDelegate)InvokeUserCode_TpcNotifyDejavuUse);
		RemoteProcedureCalls.RegisterRpc(typeof(DewPlayer), "System.Void DewPlayer::TpcShowCenterMessage(CenterMessageType,System.String,System.String[])", (RemoteCallDelegate)InvokeUserCode_TpcShowCenterMessage__CenterMessageType__String__String_005B_005D);
		RemoteProcedureCalls.RegisterRpc(typeof(DewPlayer), "System.Void DewPlayer::TpcCreateNetworkedOnScreenTimer(System.Int32,NetworkedOnScreenTimerHandle,System.Single)", (RemoteCallDelegate)InvokeUserCode_TpcCreateNetworkedOnScreenTimer__Int32__NetworkedOnScreenTimerHandle__Single);
		RemoteProcedureCalls.RegisterRpc(typeof(DewPlayer), "System.Void DewPlayer::TpcSetNetworkedOnScreenTimerValue(System.Int32,System.Single)", (RemoteCallDelegate)InvokeUserCode_TpcSetNetworkedOnScreenTimerValue__Int32__Single);
		RemoteProcedureCalls.RegisterRpc(typeof(DewPlayer), "System.Void DewPlayer::TpcRemoveNetworkedOnScreenTimer(System.Int32)", (RemoteCallDelegate)InvokeUserCode_TpcRemoveNetworkedOnScreenTimer__Int32);
		RemoteProcedureCalls.RegisterRpc(typeof(DewPlayer), "System.Void DewPlayer::TpcSetSampleContext(System.Nullable`1<SampleCastInfoContext>)", (RemoteCallDelegate)InvokeUserCode_TpcSetSampleContext__Nullable_00601);
		RemoteProcedureCalls.RegisterRpc(typeof(DewPlayer), "System.Void DewPlayer::RpcSetCastMethod(CastMethodData)", (RemoteCallDelegate)InvokeUserCode_RpcSetCastMethod__CastMethodData);
		RemoteProcedureCalls.RegisterRpc(typeof(DewPlayer), "System.Void DewPlayer::TpcMakePlayerChangeLobby(System.String)", (RemoteCallDelegate)InvokeUserCode_TpcMakePlayerChangeLobby__String);
		RemoteProcedureCalls.RegisterRpc(typeof(DewPlayer), "System.Void DewPlayer::TpcPrepareSceneBeforeMidJoin(System.String)", (RemoteCallDelegate)InvokeUserCode_TpcPrepareSceneBeforeMidJoin__String);
		RemoteProcedureCalls.RegisterRpc(typeof(DewPlayer), "System.Void DewPlayer::TpcNotifyHeroSpawned()", (RemoteCallDelegate)InvokeUserCode_TpcNotifyHeroSpawned);
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_CmdUpdateShareItemsWhenDropped_Imp__Boolean(bool value)
	{
		Network_003CshareItemsWhenDropped_003Ek__BackingField = value;
	}

	protected static void InvokeUserCode_CmdUpdateShareItemsWhenDropped_Imp__Boolean(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogError("Command CmdUpdateShareItemsWhenDropped_Imp called on client.");
		}
		else
		{
			((DewPlayer)(object)obj).UserCode_CmdUpdateShareItemsWhenDropped_Imp__Boolean(NetworkReaderExtensions.ReadBool(reader));
		}
	}

	protected void UserCode_CmdUpdateHasPolarisEndingUnlocked_Imp__Boolean(bool value)
	{
		Network_003ChasPolarisEndingUnlocked_003Ek__BackingField = value;
	}

	protected static void InvokeUserCode_CmdUpdateHasPolarisEndingUnlocked_Imp__Boolean(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogError("Command CmdUpdateHasPolarisEndingUnlocked_Imp called on client.");
		}
		else
		{
			((DewPlayer)(object)obj).UserCode_CmdUpdateHasPolarisEndingUnlocked_Imp__Boolean(NetworkReaderExtensions.ReadBool(reader));
		}
	}

	protected void UserCode_TpcKick()
	{
		DewNetworkManager.instance.isBeingKicked = true;
		ManagerBase<MessageManager>.instance.ShowMessageLocalized("Title_Message_YouAreKickedFromGame");
		DewNetworkManager.instance.EndSession();
	}

	protected static void InvokeUserCode_TpcKick(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			UnityEngine.Debug.LogError("TargetRPC TpcKick called on server.");
		}
		else
		{
			((DewPlayer)(object)obj).UserCode_TpcKick();
		}
	}

	protected void UserCode_CmdSetPlayerAvatar__Texture2D(Texture2D tex)
	{
		Network_avatar = tex;
	}

	protected static void InvokeUserCode_CmdSetPlayerAvatar__Texture2D(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogError("Command CmdSetPlayerAvatar called on client.");
		}
		else
		{
			((DewPlayer)(object)obj).UserCode_CmdSetPlayerAvatar__Texture2D(NetworkReaderExtensions.ReadTexture2D(reader));
		}
	}

	protected void UserCode_CmdSetNametag_Imp__String(string ntName)
	{
		if (string.IsNullOrEmpty(ntName) || !IsAllowedToUseItem(ntName))
		{
			Network_equippedNametag = null;
		}
		else
		{
			Network_equippedNametag = ntName;
		}
	}

	protected static void InvokeUserCode_CmdSetNametag_Imp__String(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogError("Command CmdSetNametag_Imp called on client.");
		}
		else
		{
			((DewPlayer)(object)obj).UserCode_CmdSetNametag_Imp__String(NetworkReaderExtensions.ReadString(reader));
		}
	}

	protected void UserCode_CmdSetDejavuItem__String(string item)
	{
		if (item == null)
		{
			Network_003CselectedDejavuItem_003Ek__BackingField = null;
		}
		else if (item.StartsWith("St_"))
		{
			if (Dew.IsSkillIncludedInGame(item))
			{
				SkillTrigger byShortTypeName = DewResources.GetByShortTypeName<SkillTrigger>(item, default(ResourceLoadSettings));
				if (!((UnityEngine.Object)(object)byShortTypeName == null) && !byShortTypeName.excludeFromPool)
				{
					Network_003CselectedDejavuItem_003Ek__BackingField = item;
				}
			}
		}
		else if (item.StartsWith("Gem_") && Dew.IsGemIncludedInGame(item))
		{
			Gem byShortTypeName2 = DewResources.GetByShortTypeName<Gem>(item, default(ResourceLoadSettings));
			if (!((UnityEngine.Object)(object)byShortTypeName2 == null) && !byShortTypeName2.excludeFromPool)
			{
				Network_003CselectedDejavuItem_003Ek__BackingField = item;
			}
		}
	}

	protected static void InvokeUserCode_CmdSetDejavuItem__String(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogError("Command CmdSetDejavuItem called on client.");
		}
		else
		{
			((DewPlayer)(object)obj).UserCode_CmdSetDejavuItem__String(NetworkReaderExtensions.ReadString(reader));
		}
	}

	protected void UserCode_SetUnlockedLucidDreams__String_005B_005D(string[] dreams)
	{
		if ((UnityEngine.Object)(object)NetworkedManagerBase<GameSettingsManager>.instance == null || dreams.Length > Dew.allLucidDreams.Count)
		{
			return;
		}
		availableLucidDreams.Clear();
		foreach (string text in dreams)
		{
			if ((UnityEngine.Object)(object)DewResources.GetByShortTypeName<LucidDream>(text, default(ResourceLoadSettings)) != null)
			{
				availableLucidDreams.Add(text);
			}
		}
		NetworkedManagerBase<GameSettingsManager>.instance.UpdateAvailableLucidDreams();
	}

	protected static void InvokeUserCode_SetUnlockedLucidDreams__String_005B_005D(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogError("Command SetUnlockedLucidDreams called on client.");
		}
		else
		{
			((DewPlayer)(object)obj).UserCode_SetUnlockedLucidDreams__String_005B_005D(GeneratedNetworkCode._Read_System_002EString_005B_005D(reader));
		}
	}

	protected void UserCode_SendLog__String(string message)
	{
		UnityEngine.Debug.Log(message);
	}

	protected static void InvokeUserCode_SendLog__String(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			UnityEngine.Debug.LogError("TargetRPC SendLog called on server.");
		}
		else
		{
			((DewPlayer)(object)obj).UserCode_SendLog__String(NetworkReaderExtensions.ReadString(reader));
		}
	}

	protected void UserCode_SendLogWarning__String(string message)
	{
		UnityEngine.Debug.LogWarning(message);
	}

	protected static void InvokeUserCode_SendLogWarning__String(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			UnityEngine.Debug.LogError("TargetRPC SendLogWarning called on server.");
		}
		else
		{
			((DewPlayer)(object)obj).UserCode_SendLogWarning__String(NetworkReaderExtensions.ReadString(reader));
		}
	}

	protected void UserCode_SendLogError__String(string message)
	{
		UnityEngine.Debug.LogError(message);
	}

	protected static void InvokeUserCode_SendLogError__String(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			UnityEngine.Debug.LogError("TargetRPC SendLogError called on server.");
		}
		else
		{
			((DewPlayer)(object)obj).UserCode_SendLogError__String(NetworkReaderExtensions.ReadString(reader));
		}
	}

	protected void UserCode_CmdPing()
	{
		SendLog("Pong!");
	}

	protected static void InvokeUserCode_CmdPing(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogError("Command CmdPing called on client.");
		}
		else
		{
			((DewPlayer)(object)obj).UserCode_CmdPing();
		}
	}

	protected void UserCode_CmdRequestOverrides_Imp()
	{
		TpcSendOverrides(DewMod.allJsonOverrideItemsLocal);
	}

	protected static void InvokeUserCode_CmdRequestOverrides_Imp(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogError("Command CmdRequestOverrides_Imp called on client.");
		}
		else
		{
			((DewPlayer)(object)obj).UserCode_CmdRequestOverrides_Imp();
		}
	}

	protected void UserCode_TpcSendOverrides__List_00601(List<JsonOverrideItem> overrides)
	{
		if (_isLocalRequestingOverrides && (ManagerBase<LobbyManager>.instance.service.currentLobby == null || ManagerBase<LobbyManager>.instance.service.currentLobby.isModded))
		{
			_isLocalRequestingOverrides = false;
			UnityEngine.Debug.Log($"Received overrides from server. Applying {overrides.Count} overrides...");
			Dew.GetCoroutiner().StartCoroutine(Routine());
		}
		IEnumerator Routine()
		{
			ManagerBase<TransitionManager>.instance.SetBusy(value: true);
			ManagerBase<TransitionManager>.instance.UpdateLoadingStatus(string.Format("{0} ({1} overrides)", DewLocalization.GetUIValue("Loading_ApplyingModsFromServer"), overrides.Count));
			yield return null;
			DewMod.StartRegisterJsonOverride();
			for (int i = 0; i < overrides.Count; i++)
			{
				JsonOverrideItem item = overrides[i];
				if (!NetworkClient.active || (UnityEngine.Object)(object)this == null)
				{
					break;
				}
				DewMod.RegisterJsonOverride(item, isFromServer: true);
			}
			DewMod.EndRegisterJsonOverride();
			ManagerBase<TransitionManager>.instance.SetBusy(value: false);
		}
	}

	protected static void InvokeUserCode_TpcSendOverrides__List_00601(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			UnityEngine.Debug.LogError("TargetRPC TpcSendOverrides called on server.");
		}
		else
		{
			((DewPlayer)(object)obj).UserCode_TpcSendOverrides__List_00601(GeneratedNetworkCode._Read_System_002ECollections_002EGeneric_002EList_00601_003CJsonOverrideItem_003E(reader));
		}
	}

	protected void UserCode_CmdSetPlayerIds__String__String__String(string eosId, string platformId, string platform)
	{
		if (string.IsNullOrEmpty(_eosId))
		{
			Network_eosId = eosId;
		}
		Network_platformId = platformId;
		Network_platform = platform;
	}

	protected static void InvokeUserCode_CmdSetPlayerIds__String__String__String(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogError("Command CmdSetPlayerIds called on client.");
		}
		else
		{
			((DewPlayer)(object)obj).UserCode_CmdSetPlayerIds__String__String__String(NetworkReaderExtensions.ReadString(reader), NetworkReaderExtensions.ReadString(reader), NetworkReaderExtensions.ReadString(reader));
		}
	}

	protected void UserCode_CmdNotifyClientStatus__Vector3__InputMode__Boolean__Entity__Boolean(Vector3 pos, InputMode mode, bool isExplicit, Entity targetEnemy, bool playingCutscene)
	{
		cursorWorldPos = pos;
		inputMode = mode;
		isGamepadExplicitAim = isExplicit;
		gamepadTargetEnemy = targetEnemy;
		isPlayingCutscene = playingCutscene;
	}

	protected static void InvokeUserCode_CmdNotifyClientStatus__Vector3__InputMode__Boolean__Entity__Boolean(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogError("Command CmdNotifyClientStatus called on client.");
		}
		else
		{
			((DewPlayer)(object)obj).UserCode_CmdNotifyClientStatus__Vector3__InputMode__Boolean__Entity__Boolean(NetworkReaderExtensions.ReadVector3(reader), GeneratedNetworkCode._Read_InputMode(reader), NetworkReaderExtensions.ReadBool(reader), NetworkReaderExtensions.ReadNetworkBehaviour<Entity>(reader), NetworkReaderExtensions.ReadBool(reader));
		}
	}

	protected void UserCode_TpcShowWorldPopMessage__WorldMessageSetting(WorldMessageSetting message)
	{
		InGameUIManager.instance.ShowWorldPopMessage(message);
	}

	protected static void InvokeUserCode_TpcShowWorldPopMessage__WorldMessageSetting(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			UnityEngine.Debug.LogError("TargetRPC TpcShowWorldPopMessage called on server.");
		}
		else
		{
			((DewPlayer)(object)obj).UserCode_TpcShowWorldPopMessage__WorldMessageSetting(GeneratedNetworkCode._Read_WorldMessageSetting(reader));
		}
	}

	protected void UserCode_TpcShowCenterMessage__CenterMessageType__String(CenterMessageType type, string key)
	{
		InGameUIManager.instance.ShowCenterMessage(type, key);
	}

	protected static void InvokeUserCode_TpcShowCenterMessage__CenterMessageType__String(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			UnityEngine.Debug.LogError("TargetRPC TpcShowCenterMessage called on server.");
		}
		else
		{
			((DewPlayer)(object)obj).UserCode_TpcShowCenterMessage__CenterMessageType__String(GeneratedNetworkCode._Read_CenterMessageType(reader), NetworkReaderExtensions.ReadString(reader));
		}
	}

	protected void UserCode_TpcNotifyDejavuUse()
	{
		if (DewBuildProfile.current.buildType != BuildType.DemoLite)
		{
			NetworkedManagerBase<GameSettingsManager>.instance.GetLocalPreferredGameSettings().dejavuItem = selectedDejavuItem;
			if (!string.IsNullOrEmpty(selectedDejavuItem) && NetworkedManagerBase<GameSettingsManager>.instance.localPlayerDejavuCost > 0)
			{
				DewSave.profileMain.stardust -= NetworkedManagerBase<GameSettingsManager>.instance.localPlayerDejavuCost;
				DewSave.profileMain.spentStardust += NetworkedManagerBase<GameSettingsManager>.instance.localPlayerDejavuCost;
				NetworkedManagerBase<GameSettingsManager>.instance.localPlayerDejavuCost = 0;
				DewSave.profileMain.dejavuCostReductionPeriodTimestamp[selectedDejavuItem] = DateTime.UtcNow.AddHours(24.0).ToTimestamp();
				DewSave.SaveProfileMain();
			}
		}
	}

	protected static void InvokeUserCode_TpcNotifyDejavuUse(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			UnityEngine.Debug.LogError("TargetRPC TpcNotifyDejavuUse called on server.");
		}
		else
		{
			((DewPlayer)(object)obj).UserCode_TpcNotifyDejavuUse();
		}
	}

	protected void UserCode_TpcShowCenterMessage__CenterMessageType__String__String_005B_005D(CenterMessageType type, string key, string[] localizedFormatArgs)
	{
		Dew.ResolveLocalizedFormatArgs(localizedFormatArgs);
		InGameUIManager.instance.ShowCenterMessage(type, key, localizedFormatArgs);
	}

	protected static void InvokeUserCode_TpcShowCenterMessage__CenterMessageType__String__String_005B_005D(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			UnityEngine.Debug.LogError("TargetRPC TpcShowCenterMessage called on server.");
		}
		else
		{
			((DewPlayer)(object)obj).UserCode_TpcShowCenterMessage__CenterMessageType__String__String_005B_005D(GeneratedNetworkCode._Read_CenterMessageType(reader), NetworkReaderExtensions.ReadString(reader), GeneratedNetworkCode._Read_System_002EString_005B_005D(reader));
		}
	}

	protected void UserCode_CmdRequestStardust__Int32(int amount)
	{
		if (amount > 0 && amount <= 100)
		{
			GiveStardust(amount);
		}
	}

	protected static void InvokeUserCode_CmdRequestStardust__Int32(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogError("Command CmdRequestStardust called on client.");
		}
		else
		{
			((DewPlayer)(object)obj).UserCode_CmdRequestStardust__Int32(NetworkReaderExtensions.ReadInt(reader));
		}
	}

	protected void UserCode_CmdGiveCurrency__Int32__Int32__DewPlayer(int goldAmount, int dreamDustAmount, DewPlayer target)
	{
		if ((UnityEngine.Object)(object)target == null || (UnityEngine.Object)(object)target.Network_003Chero_003Ek__BackingField == null)
		{
			return;
		}
		goldAmount = Mathf.Min(goldAmount, gold);
		dreamDustAmount = Mathf.Min(dreamDustAmount, dreamDust);
		if ((goldAmount > 0 || dreamDustAmount > 0) && !((UnityEngine.Object)(object)Network_003Chero_003Ek__BackingField == null))
		{
			gold -= goldAmount;
			dreamDust -= dreamDustAmount;
			Vector3 vector = Network_003Chero_003Ek__BackingField.agentPosition;
			if (Vector3.Distance(vector, target.Network_003Chero_003Ek__BackingField.position) > 10f || Network_003Chero_003Ek__BackingField.isKnockedOut || target.Network_003Chero_003Ek__BackingField.isKnockedOut)
			{
				vector = target.Network_003Chero_003Ek__BackingField.position;
			}
			if (goldAmount > 0)
			{
				NetworkedManagerBase<PickupManager>.instance.DropGold(isKillGold: false, isGivenByOtherPlayer: true, goldAmount, vector, target.Network_003Chero_003Ek__BackingField);
			}
			if (dreamDustAmount > 0)
			{
				NetworkedManagerBase<PickupManager>.instance.DropDreamDust(isGivenByOtherPlayer: true, dreamDustAmount, vector, target.Network_003Chero_003Ek__BackingField);
			}
			RpcInvokeOnGiveCurrency(goldAmount, dreamDustAmount, target);
		}
	}

	protected static void InvokeUserCode_CmdGiveCurrency__Int32__Int32__DewPlayer(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogError("Command CmdGiveCurrency called on client.");
		}
		else
		{
			((DewPlayer)(object)obj).UserCode_CmdGiveCurrency__Int32__Int32__DewPlayer(NetworkReaderExtensions.ReadInt(reader), NetworkReaderExtensions.ReadInt(reader), NetworkReaderExtensions.ReadNetworkBehaviour<DewPlayer>(reader));
		}
	}

	protected void UserCode_RpcGiveStardust__Int32(int amount)
	{
		((MonoBehaviour)(object)this).StartCoroutine(Routine());
		IEnumerator Routine()
		{
			yield return new WaitForSeconds(0.2f);
			int remainingAmount = amount;
			while (remainingAmount > 0)
			{
				int b = Mathf.Max(1, amount / 10);
				b = Mathf.Min(remainingAmount, b);
				remainingAmount -= b;
				ClientEvent_OnEarnStardust?.Invoke(b);
				if (((NetworkBehaviour)this).isLocalPlayer)
				{
					DewSave.profileMain.stardust += b;
				}
				yield return new WaitForSeconds(0.15f);
			}
		}
	}

	protected static void InvokeUserCode_RpcGiveStardust__Int32(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			UnityEngine.Debug.LogError("RPC RpcGiveStardust called on server.");
		}
		else
		{
			((DewPlayer)(object)obj).UserCode_RpcGiveStardust__Int32(NetworkReaderExtensions.ReadInt(reader));
		}
	}

	protected void UserCode_RpcInvokeOnSpendGold__Int32(int amount)
	{
		ClientEvent_OnSpendGold?.Invoke(amount);
	}

	protected static void InvokeUserCode_RpcInvokeOnSpendGold__Int32(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			UnityEngine.Debug.LogError("RPC RpcInvokeOnSpendGold called on server.");
		}
		else
		{
			((DewPlayer)(object)obj).UserCode_RpcInvokeOnSpendGold__Int32(NetworkReaderExtensions.ReadInt(reader));
		}
	}

	protected void UserCode_RpcInvokeOnSpendStardust__Int32(int amount)
	{
		ClientEvent_OnSpendStardust?.Invoke(amount);
	}

	protected static void InvokeUserCode_RpcInvokeOnSpendStardust__Int32(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			UnityEngine.Debug.LogError("RPC RpcInvokeOnSpendStardust called on server.");
		}
		else
		{
			((DewPlayer)(object)obj).UserCode_RpcInvokeOnSpendStardust__Int32(NetworkReaderExtensions.ReadInt(reader));
		}
	}

	protected void UserCode_RpcInvokeOnEarnGold__Int32(int amount)
	{
		ClientEvent_OnEarnGold?.Invoke(amount);
	}

	protected static void InvokeUserCode_RpcInvokeOnEarnGold__Int32(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			UnityEngine.Debug.LogError("RPC RpcInvokeOnEarnGold called on server.");
		}
		else
		{
			((DewPlayer)(object)obj).UserCode_RpcInvokeOnEarnGold__Int32(NetworkReaderExtensions.ReadInt(reader));
		}
	}

	protected void UserCode_RpcInvokeOnSpendDreamDust__Int32(int amount)
	{
		ClientEvent_OnSpendDreamDust?.Invoke(amount);
	}

	protected static void InvokeUserCode_RpcInvokeOnSpendDreamDust__Int32(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			UnityEngine.Debug.LogError("RPC RpcInvokeOnSpendDreamDust called on server.");
		}
		else
		{
			((DewPlayer)(object)obj).UserCode_RpcInvokeOnSpendDreamDust__Int32(NetworkReaderExtensions.ReadInt(reader));
		}
	}

	protected void UserCode_RpcInvokeOnEarnDreamDust__Int32(int amount)
	{
		ClientEvent_OnEarnDreamDust?.Invoke(amount);
	}

	protected static void InvokeUserCode_RpcInvokeOnEarnDreamDust__Int32(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			UnityEngine.Debug.LogError("RPC RpcInvokeOnEarnDreamDust called on server.");
		}
		else
		{
			((DewPlayer)(object)obj).UserCode_RpcInvokeOnEarnDreamDust__Int32(NetworkReaderExtensions.ReadInt(reader));
		}
	}

	protected void UserCode_RpcInvokeOnGiveCurrency__Int32__Int32__DewPlayer(int goldAmount, int dreamDustAmount, DewPlayer target)
	{
		ClientEvent_OnGiveCurrency?.Invoke(goldAmount, dreamDustAmount, target);
	}

	protected static void InvokeUserCode_RpcInvokeOnGiveCurrency__Int32__Int32__DewPlayer(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			UnityEngine.Debug.LogError("RPC RpcInvokeOnGiveCurrency called on server.");
		}
		else
		{
			((DewPlayer)(object)obj).UserCode_RpcInvokeOnGiveCurrency__Int32__Int32__DewPlayer(NetworkReaderExtensions.ReadInt(reader), NetworkReaderExtensions.ReadInt(reader), NetworkReaderExtensions.ReadNetworkBehaviour<DewPlayer>(reader));
		}
	}

	protected void UserCode_TpcCreateNetworkedOnScreenTimer__Int32__NetworkedOnScreenTimerHandle__Single(int id, NetworkedOnScreenTimerHandle handle, float defaultValue)
	{
		RefValue<float> val = new RefValue<float>(defaultValue);
		OnScreenTimerHandle onScreenTimerHandle = new OnScreenTimerHandle
		{
			rawText = ((handle.localeKey != null) ? DewLocalization.GetUIValue(handle.localeKey) : DewLocalization.GetSkillName(handle.skillKey, 0)),
			fillAmountGetter = () => val.value
		};
		_onScreenTimersLocal.Add((id, onScreenTimerHandle, val));
		NetworkedManagerBase<ClientEventManager>.instance.OnShowOnScreenTimer?.Invoke(onScreenTimerHandle);
	}

	protected static void InvokeUserCode_TpcCreateNetworkedOnScreenTimer__Int32__NetworkedOnScreenTimerHandle__Single(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			UnityEngine.Debug.LogError("TargetRPC TpcCreateNetworkedOnScreenTimer called on server.");
		}
		else
		{
			((DewPlayer)(object)obj).UserCode_TpcCreateNetworkedOnScreenTimer__Int32__NetworkedOnScreenTimerHandle__Single(NetworkReaderExtensions.ReadInt(reader), GeneratedNetworkCode._Read_NetworkedOnScreenTimerHandle(reader), NetworkReaderExtensions.ReadFloat(reader));
		}
	}

	protected void UserCode_TpcSetNetworkedOnScreenTimerValue__Int32__Single(int id, float value)
	{
		foreach (var item in _onScreenTimersLocal)
		{
			if (item.Item1 == id)
			{
				item.Item3.value = value;
			}
		}
	}

	protected static void InvokeUserCode_TpcSetNetworkedOnScreenTimerValue__Int32__Single(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			UnityEngine.Debug.LogError("TargetRPC TpcSetNetworkedOnScreenTimerValue called on server.");
		}
		else
		{
			((DewPlayer)(object)obj).UserCode_TpcSetNetworkedOnScreenTimerValue__Int32__Single(NetworkReaderExtensions.ReadInt(reader), NetworkReaderExtensions.ReadFloat(reader));
		}
	}

	protected void UserCode_TpcRemoveNetworkedOnScreenTimer__Int32(int id)
	{
		for (int num = _onScreenTimersLocal.Count - 1; num >= 0; num--)
		{
			if (_onScreenTimersLocal[num].Item1 == id)
			{
				NetworkedManagerBase<ClientEventManager>.instance.OnHideOnScreenTimer?.Invoke(_onScreenTimersLocal[num].Item2);
				_onScreenTimersLocal.RemoveAt(num);
			}
		}
	}

	protected static void InvokeUserCode_TpcRemoveNetworkedOnScreenTimer__Int32(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			UnityEngine.Debug.LogError("TargetRPC TpcRemoveNetworkedOnScreenTimer called on server.");
		}
		else
		{
			((DewPlayer)(object)obj).UserCode_TpcRemoveNetworkedOnScreenTimer__Int32(NetworkReaderExtensions.ReadInt(reader));
		}
	}

	protected void UserCode_DispatchSample_Cast__CastInfo(CastInfo info)
	{
		if (!_currentSampleContext.HasValue)
		{
			return;
		}
		SampleCastInfoContext value = _currentSampleContext.Value;
		value.currentInfo = info;
		_currentSampleContext = value;
		try
		{
			_currentSampleContext.Value.castCallback?.Invoke(info);
		}
		catch (Exception exception)
		{
			UnityEngine.Debug.LogException(exception);
		}
	}

	protected static void InvokeUserCode_DispatchSample_Cast__CastInfo(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogError("Command DispatchSample_Cast called on client.");
		}
		else
		{
			((DewPlayer)(object)obj).UserCode_DispatchSample_Cast__CastInfo(GeneratedNetworkCode._Read_CastInfo(reader));
		}
	}

	protected void UserCode_DispatchSample_Update__CastInfo(CastInfo info)
	{
		if (!_currentSampleContext.HasValue)
		{
			return;
		}
		SampleCastInfoContext value = _currentSampleContext.Value;
		value.currentInfo = info;
		_currentSampleContext = value;
		try
		{
			_currentSampleContext.Value.updateCallback?.Invoke(info);
		}
		catch (Exception exception)
		{
			UnityEngine.Debug.LogException(exception);
		}
	}

	protected static void InvokeUserCode_DispatchSample_Update__CastInfo(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogError("Command DispatchSample_Update called on client.");
		}
		else
		{
			((DewPlayer)(object)obj).UserCode_DispatchSample_Update__CastInfo(GeneratedNetworkCode._Read_CastInfo(reader));
		}
	}

	protected void UserCode_TpcSetSampleContext__Nullable_00601(SampleCastInfoContext? context)
	{
		if (!context.HasValue)
		{
			ManagerBase<ControlManager>.instance.localSampleContext = null;
			return;
		}
		SampleCastInfoContext value = context.Value;
		if (!value.isInitialInfoSet)
		{
			value.currentInfo = ManagerBase<ControlManager>.instance.GetCastInfo(value.castMethod, value.targetValidator);
		}
		if (value.trigger is SkillTrigger skill && Network_003Chero_003Ek__BackingField.Skill.TryGetSkillLocation(skill, out var type) && ManagerBase<ControlManager>.instance._castByKeyInfo.TryGetValue(type, out var value2))
		{
			(value.castKey, _) = value2;
		}
		if ((value.castOnButton == SampleCastInfoContext.CastOnButtonType.ByButton || value.castOnButton == SampleCastInfoContext.CastOnButtonType.ByButtonRelease) && DewInput.currentMode == InputMode.KeyboardAndMouse && DewSave.profileMain.controls.clickToCastInsteadOfHoldToCast)
		{
			value.castOnButton = SampleCastInfoContext.CastOnButtonType.ByButtonPress;
		}
		ManagerBase<ControlManager>.instance.localSampleContext = value;
	}

	protected static void InvokeUserCode_TpcSetSampleContext__Nullable_00601(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			UnityEngine.Debug.LogError("TargetRPC TpcSetSampleContext called on server.");
		}
		else
		{
			((DewPlayer)(object)obj).UserCode_TpcSetSampleContext__Nullable_00601(reader.ReadSampleCastInfoContext());
		}
	}

	protected void UserCode_RpcSetCastMethod__CastMethodData(CastMethodData method)
	{
		if (ManagerBase<ControlManager>.instance.localSampleContext.HasValue)
		{
			SampleCastInfoContext value = ManagerBase<ControlManager>.instance.localSampleContext.Value;
			value.castMethod = method;
			ManagerBase<ControlManager>.instance.localSampleContext = value;
		}
	}

	protected static void InvokeUserCode_RpcSetCastMethod__CastMethodData(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			UnityEngine.Debug.LogError("TargetRPC RpcSetCastMethod called on server.");
		}
		else
		{
			((DewPlayer)(object)obj).UserCode_RpcSetCastMethod__CastMethodData(reader.ReadWriteCastMethodData());
		}
	}

	protected void UserCode_PlaceholderFunction__SampleCastInfoContext(SampleCastInfoContext context)
	{
	}

	protected static void InvokeUserCode_PlaceholderFunction__SampleCastInfoContext(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogError("Command PlaceholderFunction called on client.");
		}
		else
		{
			((DewPlayer)(object)obj).UserCode_PlaceholderFunction__SampleCastInfoContext(GeneratedNetworkCode._Read_SampleCastInfoContext(reader));
		}
	}

	protected void UserCode_CmdAuthorizeForUse__String(string ownershipKey)
	{
		if (ownershipKeys.Count <= 100 && !ownershipKeys.Contains(ownershipKey) && DewItem.GetDecryptedItemData(ownershipKey) != null)
		{
			ownershipKeys.Add(ownershipKey);
		}
	}

	protected static void InvokeUserCode_CmdAuthorizeForUse__String(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogError("Command CmdAuthorizeForUse called on client.");
		}
		else
		{
			((DewPlayer)(object)obj).UserCode_CmdAuthorizeForUse__String(NetworkReaderExtensions.ReadString(reader));
		}
	}

	protected void UserCode_CmdSetAccessories__List_00601(List<string> accessories)
	{
		selectedAccessories.Clear();
		selectedAccessories.AddRange((IEnumerable<string>)accessories);
	}

	protected static void InvokeUserCode_CmdSetAccessories__List_00601(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogError("Command CmdSetAccessories called on client.");
		}
		else
		{
			((DewPlayer)(object)obj).UserCode_CmdSetAccessories__List_00601(GeneratedNetworkCode._Read_System_002ECollections_002EGeneric_002EList_00601_003CSystem_002EString_003E(reader));
		}
	}

	protected void UserCode_CmdSetHeroType_Imp__String__HeroLoadoutData__List_00601__String(string newType, HeroLoadoutData loadoutData, List<string> accessories, string skin)
	{
		if (state == PlayerState.InLobby && !isWaitingForContinueMidJoin && !isLoadingForMidJoin && !((UnityEngine.Object)(object)DewResources.GetByShortTypeName<Hero>(newType, new ResourceLoadSettings
		{
			loadLight = true
		}) == null))
		{
			Network_003CselectedHeroType_003Ek__BackingField = newType;
			Network_003CselectedLoadout_003Ek__BackingField = (loadoutData.IsValidFor(newType) ? loadoutData : new HeroLoadoutData());
			selectedAccessories.Clear();
			selectedAccessories.AddRange((IEnumerable<string>)accessories);
			Skin byName = DewResources.GetByName<Skin>(skin, new ResourceLoadSettings
			{
				loadLight = true
			});
			if (byName == null || !byName.IsValidFor(newType))
			{
				Network_003CselectedSkin_003Ek__BackingField = Skin.GetDefaultSkin(newType);
			}
			else
			{
				Network_003CselectedSkin_003Ek__BackingField = skin;
			}
		}
	}

	protected static void InvokeUserCode_CmdSetHeroType_Imp__String__HeroLoadoutData__List_00601__String(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogError("Command CmdSetHeroType_Imp called on client.");
		}
		else
		{
			((DewPlayer)(object)obj).UserCode_CmdSetHeroType_Imp__String__HeroLoadoutData__List_00601__String(NetworkReaderExtensions.ReadString(reader), GeneratedNetworkCode._Read_HeroLoadoutData(reader), GeneratedNetworkCode._Read_System_002ECollections_002EGeneric_002EList_00601_003CSystem_002EString_003E(reader), NetworkReaderExtensions.ReadString(reader));
		}
	}

	protected void UserCode_CmdSetSkin_Imp__String(string skin)
	{
		if (state == PlayerState.InLobby)
		{
			Skin byName = DewResources.GetByName<Skin>(skin, new ResourceLoadSettings
			{
				loadLight = true
			});
			if (byName == null || !byName.IsValidFor(selectedHeroType))
			{
				Network_003CselectedSkin_003Ek__BackingField = Skin.GetDefaultSkin(selectedHeroType);
			}
			else
			{
				Network_003CselectedSkin_003Ek__BackingField = skin;
			}
		}
	}

	protected static void InvokeUserCode_CmdSetSkin_Imp__String(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogError("Command CmdSetSkin_Imp called on client.");
		}
		else
		{
			((DewPlayer)(object)obj).UserCode_CmdSetSkin_Imp__String(NetworkReaderExtensions.ReadString(reader));
		}
	}

	protected void UserCode_CmdSetHeroLoadoutData_Imp__HeroLoadoutData(HeroLoadoutData newData)
	{
		if (state == PlayerState.InLobby)
		{
			if (!newData.Validate_Imp(selectedHeroType, isRepair: true, checkStarLevels: true, null))
			{
				UnityEngine.Debug.LogWarning("Repaired invalid loadout for " + selectedHeroType + " from " + ((UnityEngine.Object)(object)this).name);
			}
			Network_003CselectedLoadout_003Ek__BackingField = newData;
		}
	}

	protected static void InvokeUserCode_CmdSetHeroLoadoutData_Imp__HeroLoadoutData(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogError("Command CmdSetHeroLoadoutData_Imp called on client.");
		}
		else
		{
			((DewPlayer)(object)obj).UserCode_CmdSetHeroLoadoutData_Imp__HeroLoadoutData(GeneratedNetworkCode._Read_HeroLoadoutData(reader));
		}
	}

	protected void UserCode_CmdSetIsReady__Boolean(bool newReady)
	{
		Network_003CisReady_003Ek__BackingField = newReady;
		if ((UnityEngine.Object)(object)NetworkedManagerBase<ZoneManager>.instance != null && NetworkedManagerBase<ZoneManager>.instance.isVoting)
		{
			NetworkedManagerBase<ZoneManager>.instance.UpdateVoteStatus();
		}
	}

	protected static void InvokeUserCode_CmdSetIsReady__Boolean(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogError("Command CmdSetIsReady called on client.");
		}
		else
		{
			((DewPlayer)(object)obj).UserCode_CmdSetIsReady__Boolean(NetworkReaderExtensions.ReadBool(reader));
		}
	}

	protected void UserCode_CmdRequestToJoinCurrentLobby()
	{
		if (ManagerBase<LobbyManager>.instance.service.currentLobby == null)
		{
			UnityEngine.Debug.Log("Player " + ((NetworkBehaviour)this).connectionToClient.address + " requests to join lobby, but no lobby present");
			return;
		}
		UnityEngine.Debug.Log("Player " + ((NetworkBehaviour)this).connectionToClient.address + " requests to join lobby " + ManagerBase<LobbyManager>.instance.service.currentLobby.id);
		TpcMakePlayerChangeLobby(ManagerBase<LobbyManager>.instance.service.currentLobby.id);
	}

	protected static void InvokeUserCode_CmdRequestToJoinCurrentLobby(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogError("Command CmdRequestToJoinCurrentLobby called on client.");
		}
		else
		{
			((DewPlayer)(object)obj).UserCode_CmdRequestToJoinCurrentLobby();
		}
	}

	protected void UserCode_TpcMakePlayerChangeLobby__String(string lobbyId)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		ChangeLobby_Imp(lobbyId);
	}

	protected static void InvokeUserCode_TpcMakePlayerChangeLobby__String(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			UnityEngine.Debug.LogError("TargetRPC TpcMakePlayerChangeLobby called on server.");
		}
		else
		{
			((DewPlayer)(object)obj).UserCode_TpcMakePlayerChangeLobby__String(NetworkReaderExtensions.ReadString(reader));
		}
	}

	protected void UserCode_CmdSetProfileStats__DewProfileStats(DewProfileStats stats)
	{
		Network_003CprofileStats_003Ek__BackingField = stats;
	}

	protected static void InvokeUserCode_CmdSetProfileStats__DewProfileStats(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogError("Command CmdSetProfileStats called on client.");
		}
		else
		{
			((DewPlayer)(object)obj).UserCode_CmdSetProfileStats__DewProfileStats(GeneratedNetworkCode._Read_DewProfileStats(reader));
		}
	}

	protected void UserCode_CmdSetUnlockedGameItems_Imp__String_005B_005D(string[] items)
	{
		unlockedGameItems.Clear();
		unlockedGameItems.AddRange((IEnumerable<string>)items);
		NetworkedManagerBase<GameSettingsManager>.instance.UpdateUnlockedGameItems();
	}

	protected static void InvokeUserCode_CmdSetUnlockedGameItems_Imp__String_005B_005D(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogError("Command CmdSetUnlockedGameItems_Imp called on client.");
		}
		else
		{
			((DewPlayer)(object)obj).UserCode_CmdSetUnlockedGameItems_Imp__String_005B_005D(GeneratedNetworkCode._Read_System_002EString_005B_005D(reader));
		}
	}

	protected void UserCode_CmdNotifyEveryInfoSet()
	{
		Network_003CisEveryInfoSet_003Ek__BackingField = true;
	}

	protected static void InvokeUserCode_CmdNotifyEveryInfoSet(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogError("Command CmdNotifyEveryInfoSet called on client.");
		}
		else
		{
			((DewPlayer)(object)obj).UserCode_CmdNotifyEveryInfoSet();
		}
	}

	protected void UserCode_PlaceholderFunction0__HeroData__ItemData__MonsterData__ZoneData(DewProfileStats.HeroData a, DewProfileStats.ItemData b, DewProfileStats.MonsterData c, DewProfileStats.ZoneData d)
	{
	}

	protected static void InvokeUserCode_PlaceholderFunction0__HeroData__ItemData__MonsterData__ZoneData(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogError("Command PlaceholderFunction0 called on client.");
		}
		else
		{
			((DewPlayer)(object)obj).UserCode_PlaceholderFunction0__HeroData__ItemData__MonsterData__ZoneData(GeneratedNetworkCode._Read_DewProfileStats_002FHeroData(reader), GeneratedNetworkCode._Read_DewProfileStats_002FItemData(reader), GeneratedNetworkCode._Read_DewProfileStats_002FMonsterData(reader), GeneratedNetworkCode._Read_DewProfileStats_002FZoneData(reader));
		}
	}

	protected void UserCode_CmdRequestMidJoin()
	{
		GameManager.CallOnReady(() =>
		{
			if (!((UnityEngine.Object)(object)this == null) && ((NetworkBehaviour)this).isServer && state == PlayerState.InLobby && NetworkedManagerBase<GameSettingsManager>.instance.state == GameState.InGame)
			{
				Network_003CisLoadingForMidJoin_003Ek__BackingField = true;
				TpcPrepareSceneBeforeMidJoin(SceneManager.GetActiveScene().name);
			}
		});
	}

	protected static void InvokeUserCode_CmdRequestMidJoin(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogError("Command CmdRequestMidJoin called on client.");
		}
		else
		{
			((DewPlayer)(object)obj).UserCode_CmdRequestMidJoin();
		}
	}

	protected void UserCode_TpcPrepareSceneBeforeMidJoin__String(string sceneName)
	{
		((MonoBehaviour)(object)this).StartCoroutine(Routine());
		IEnumerator Routine()
		{
			NetworkClient.isLoadingScene = true;
			ManagerBase<TransitionManager>.instance.FadeOut(showTips: true);
			if (ManagerBase<GameLogicPackage>.instance == null)
			{
				UnityEngine.Debug.Log("Loading GameLogicPackage");
				yield return SceneManager.LoadSceneAsync("PlayGame", new LoadSceneParameters
				{
					loadSceneMode = LoadSceneMode.Single,
					localPhysicsMode = LocalPhysicsMode.None
				});
			}
			else
			{
				UnityEngine.Debug.Log("Server scene has changed while preparing");
			}
			for (int i = 0; i < 5; i++)
			{
				yield return null;
			}
			UnityEngine.Debug.Log("Loading scene: " + sceneName);
			yield return SceneManager.LoadSceneAsync(sceneName, new LoadSceneParameters
			{
				loadSceneMode = LoadSceneMode.Single,
				localPhysicsMode = LocalPhysicsMode.None
			});
			yield return Resources.UnloadUnusedAssets();
			GarbageCollector.CollectIncremental(ulong.MaxValue);
			GC.Collect();
			NetworkClient.isLoadingScene = false;
			NetworkClient.PrepareToSpawnSceneObjects();
			if (!NetworkClient.ready)
			{
				NetworkClient.Ready();
			}
			UnityEngine.Debug.Log("Notified server ready");
			CmdNotifyMidJoinReady(sceneName);
		}
	}

	protected static void InvokeUserCode_TpcPrepareSceneBeforeMidJoin__String(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			UnityEngine.Debug.LogError("TargetRPC TpcPrepareSceneBeforeMidJoin called on server.");
		}
		else
		{
			((DewPlayer)(object)obj).UserCode_TpcPrepareSceneBeforeMidJoin__String(NetworkReaderExtensions.ReadString(reader));
		}
	}

	protected void UserCode_CmdNotifyMidJoinReady__String(string sceneName)
	{
		NetworkedManagerBase<ZoneManager>.instance.CallOnReadyAfterTransition(() =>
		{
			int playerCount;
			float goldSum;
			float dreamDustSum;
			float skillCountSum;
			float skillLevelSum;
			float skillLevelAvg;
			float skillCountAvg;
			float gemQualityAvg;
			float gemCountAvg;
			StatBonus chaosAvg;
			if (!((UnityEngine.Object)(object)this == null) && ((NetworkBehaviour)this).isServer && state == PlayerState.InLobby && NetworkedManagerBase<GameSettingsManager>.instance.state == GameState.InGame)
			{
				UnityEngine.Debug.Log("Received mid-join ready from " + playerName);
				if (sceneName != SceneManager.GetActiveScene().name)
				{
					UnityEngine.Debug.Log("Server scene has changed since " + playerName + " prepared to join");
					UnityEngine.Debug.Log("Directing from " + sceneName + " to " + SceneManager.GetActiveScene().name);
					TpcPrepareSceneBeforeMidJoin(SceneManager.GetActiveScene().name);
				}
				else
				{
					bool flag = NetworkedManagerBase<GameManager>.instance.playerRejoinData.TryGetValue(guid, out var value);
					if (flag)
					{
						UnityEngine.Debug.Log("Applying rejoin data to " + playerName);
						Network_003Cstate_003Ek__BackingField = PlayerState.Playing;
						DewPersistence.ApplyPlayerData(this, value);
					}
					else
					{
						UnityEngine.Debug.Log("Spawning " + playerName + " as a new player");
						Network_003CjoinedMidGame_003Ek__BackingField = true;
						playerCount = 0;
						float num = 0f;
						float num2 = 0f;
						goldSum = 0f;
						dreamDustSum = 0f;
						skillCountSum = 0f;
						skillLevelSum = 0f;
						float num3 = 0f;
						float num4 = 0f;
						StatBonus statBonus = new StatBonus();
						foreach (DewPlayer gamePlayer in gamePlayers)
						{
							if (!((UnityEngine.Object)(object)gamePlayer == (UnityEngine.Object)(object)this) && !gamePlayer.Network_003Chero_003Ek__BackingField.IsNullOrInactive())
							{
								playerCount++;
								num += (float)gamePlayer.Network_003Chero_003Ek__BackingField.Status.level;
								num2 += (float)gamePlayer.Network_003Chero_003Ek__BackingField.exp;
								Check(gamePlayer.Network_003Chero_003Ek__BackingField.Skill.Q);
								Check(gamePlayer.Network_003Chero_003Ek__BackingField.Skill.W);
								Check(gamePlayer.Network_003Chero_003Ek__BackingField.Skill.E);
								Check(gamePlayer.Network_003Chero_003Ek__BackingField.Skill.R);
								Check(gamePlayer.Network_003Chero_003Ek__BackingField.Skill.Identity);
								foreach (Gem value4 in gamePlayer.Network_003Chero_003Ek__BackingField.Skill.gems.Values)
								{
									num3++;
									num4 += (float)value4.quality;
								}
								goldSum += gamePlayer.gold;
								dreamDustSum += gamePlayer.dreamDust;
								if (gamePlayer.Network_003Chero_003Ek__BackingField.Status.TryGetStatusEffect<Se_Shrine_Chaos_StatBonus>(out var effect))
								{
									statBonus.Add(effect.bonus);
								}
							}
						}
						skillLevelAvg = skillLevelSum / skillCountSum;
						skillCountAvg = skillCountSum / (float)playerCount;
						gemQualityAvg = num4 / num3;
						gemCountAvg = num3 / (float)playerCount;
						chaosAvg = new StatBonus();
						chaosAvg.Add(statBonus);
						chaosAvg.Multiply(1f / (float)playerCount);
						Network_003Cstate_003Ek__BackingField = PlayerState.Playing;
						PlayGameManager.instance.SpawnHero(this, DewResources.GetByShortTypeName<Hero>(selectedHeroType, default(ResourceLoadSettings)), selectedLoadout, selectedSkin, ((IEnumerable<string>)selectedAccessories).ToList());
						if (playerCount > 0)
						{
							Network_003Chero_003Ek__BackingField.Status.level = Mathf.RoundToInt(num / (float)playerCount);
							Network_003Chero_003Ek__BackingField.exp = Mathf.RoundToInt(num2 / (float)playerCount);
							((MonoBehaviour)(object)this).StartCoroutine(Routine());
						}
					}
					if (Network_003Chero_003Ek__BackingField.isKnockedOut)
					{
						Network_003Chero_003Ek__BackingField.Control.Teleport(new Vector3(-5000f, -5000f, 0f));
					}
					else
					{
						Se_PortalTransition se_PortalTransition = NetworkedManagerBase<ActorManager>.instance.serverActor.CreateStatusEffect(Network_003Chero_003Ek__BackingField, new CastInfo(Network_003Chero_003Ek__BackingField), (Se_PortalTransition se) =>
						{
							se.playDisappearEffect = false;
						});
						Hero[] array = NetworkedManagerBase<ActorManager>.instance.allHeroes.Where((Hero c) => (UnityEngine.Object)(object)c != (UnityEngine.Object)(object)Network_003Chero_003Ek__BackingField && !c.IsNullInactiveDeadOrKnockedOut()).ToArray();
						if (array.Length == 0)
						{
							array = NetworkedManagerBase<ActorManager>.instance.allHeroes.Where((Hero c) => (UnityEngine.Object)(object)c != (UnityEngine.Object)(object)Network_003Chero_003Ek__BackingField).ToArray();
						}
						Vector3 position = ((array.Length != 0) ? Dew.GetGoodRewardPosition(array[UnityEngine.Random.Range(0, array.Length)].agentPosition, 5f) : Vector3.zero);
						Network_003Chero_003Ek__BackingField.Control.Teleport(position);
						NetworkedManagerBase<ActorManager>.instance.serverActor.CreateBasicEffect(Network_003Chero_003Ek__BackingField, new UntargetableEffect(), 5f);
						NetworkedManagerBase<ActorManager>.instance.serverActor.CreateBasicEffect(Network_003Chero_003Ek__BackingField, new InvisibleEffect
						{
							ignoreReveal = true
						}, 5f);
						NetworkedManagerBase<ActorManager>.instance.serverActor.CreateBasicEffect(Network_003Chero_003Ek__BackingField, new InvulnerableEffect(), 5f);
						if (!flag)
						{
							se_PortalTransition.ClientActorEvent_OnDestroyed += (Action<Actor>)((Actor _) =>
							{
								if (!((UnityEngine.Object)(object)this == null) && !((UnityEngine.Object)(object)Network_003Chero_003Ek__BackingField == null))
								{
									PlayGameManager.instance.DoDejavuSpawn(this);
								}
							});
						}
						se_PortalTransition.SetTimer(1.5f);
					}
					Network_003CisLoadingForMidJoin_003Ek__BackingField = false;
					TpcNotifyHeroSpawned();
					if ((UnityEngine.Object)(object)SingletonDewNetworkBehaviour<Room>.softInstance != null && (UnityEngine.Object)(object)SingletonDewNetworkBehaviour<Room>.softInstance.monsters != null)
					{
						SingletonDewNetworkBehaviour<Room>.softInstance.monsters.ReplayPrewarmTo(((NetworkBehaviour)this).connectionToClient);
					}
				}
			}
			void Check(SkillTrigger s)
			{
				if (!s.IsNullOrInactive())
				{
					skillCountSum++;
					skillLevelSum += s.level;
				}
			}
			IEnumerator Routine()
			{
				yield return new WaitForSeconds(1.5f);
				if (Network_003Chero_003Ek__BackingField.IsNullOrInactive())
				{
					yield break;
				}
				try
				{
					StatBonus newBonus = new StatBonus();
					newBonus.Add(chaosAvg);
					NetworkedManagerBase<ActorManager>.instance.serverActor.CreateStatusEffect(Network_003Chero_003Ek__BackingField, new CastInfo(Network_003Chero_003Ek__BackingField, Network_003Chero_003Ek__BackingField), (Se_Shrine_Chaos_StatBonus se) =>
					{
						se.bonus = newBonus;
					});
				}
				catch (Exception exception)
				{
					UnityEngine.Debug.LogException(exception);
				}
				try
				{
					EarnGold(Mathf.RoundToInt(goldSum / (float)playerCount));
					EarnDreamDust(Mathf.RoundToInt(dreamDustSum / (float)playerCount));
					Network_003CplatinumCoin_003Ek__BackingField = platinumCoin + Mathf.RoundToInt(NetworkedManagerBase<ZoneManager>.instance.currentZoneIndex);
				}
				catch (Exception exception2)
				{
					UnityEngine.Debug.LogException(exception2);
				}
				try
				{
					if ((UnityEngine.Object)(object)Network_003Chero_003Ek__BackingField.Skill.Q != null)
					{
						Network_003Chero_003Ek__BackingField.Skill.Q.level = DewMath.RandomRoundToInt(skillLevelAvg);
					}
					if ((UnityEngine.Object)(object)Network_003Chero_003Ek__BackingField.Skill.R != null)
					{
						Network_003Chero_003Ek__BackingField.Skill.R.level = DewMath.RandomRoundToInt(skillLevelAvg);
					}
					if ((UnityEngine.Object)(object)Network_003Chero_003Ek__BackingField.Skill.Identity != null)
					{
						Network_003Chero_003Ek__BackingField.Skill.Identity.level = DewMath.RandomRoundToInt(skillLevelAvg);
					}
				}
				catch (Exception exception3)
				{
					UnityEngine.Debug.LogException(exception3);
				}
				int level;
				try
				{
					int num5 = Mathf.Clamp(Mathf.CeilToInt(skillCountAvg - 3f), 0, 2);
					for (int num6 = 0; num6 < num5; num6++)
					{
						Rarity value2 = NetworkedManagerBase<LootManager>.instance.SelectSkillRarity();
						NetworkedManagerBase<LootManager>.instance.SelectSkillAndLevel(value2, out var skill, out level);
						SkillTrigger skill2 = Dew.CreateSkillTrigger(skill, Network_003Chero_003Ek__BackingField.position, DewMath.RandomRoundToInt(skillLevelAvg));
						Network_003Chero_003Ek__BackingField.Skill.EquipSkill((num6 == 0) ? HeroSkillLocation.W : HeroSkillLocation.E, skill2);
					}
				}
				catch (Exception exception4)
				{
					UnityEngine.Debug.LogException(exception4);
				}
				try
				{
					int num7 = Mathf.Clamp(Mathf.CeilToInt(gemCountAvg), 0, 8);
					for (int num8 = 0; num8 < num7; num8++)
					{
						Rarity value3 = NetworkedManagerBase<LootManager>.instance.SelectGemRarity();
						NetworkedManagerBase<LootManager>.instance.SelectGemAndQuality(value3, out var gem, out level);
						Gem gem2 = Dew.CreateGem(gem, Network_003Chero_003Ek__BackingField.position, DewMath.RandomRoundToInt(gemQualityAvg / 10f) * 10);
						GemLocation loc = new GemLocation(HeroSkillLocation.Q, num8);
						while (loc.index >= Network_003Chero_003Ek__BackingField.Skill.GetMaxGemCount(loc.skill))
						{
							loc.index -= Network_003Chero_003Ek__BackingField.Skill.GetMaxGemCount(loc.skill);
							loc.skill++;
						}
						Network_003Chero_003Ek__BackingField.Skill.EquipGem(loc, gem2);
					}
				}
				catch (Exception exception5)
				{
					UnityEngine.Debug.LogException(exception5);
				}
			}
		});
	}

	protected static void InvokeUserCode_CmdNotifyMidJoinReady__String(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogError("Command CmdNotifyMidJoinReady called on client.");
		}
		else
		{
			((DewPlayer)(object)obj).UserCode_CmdNotifyMidJoinReady__String(NetworkReaderExtensions.ReadString(reader));
		}
	}

	protected void UserCode_TpcNotifyHeroSpawned()
	{
		((MonoBehaviour)(object)this).StartCoroutine(Routine());
		IEnumerator Routine()
		{
			UnityEngine.Debug.Log("Waiting for hero");
			yield return new WaitWhile(() => (UnityEngine.Object)(object)Network_003Chero_003Ek__BackingField == null);
			ManagerBase<TransitionManager>.instance.FadeIn();
			InGameUIManager.instance.SetState("Playing");
			Dew.CallDelayed(ManagerBase<CameraManager>.instance.SpectateImmediatelyIfDead);
			SingletonDewNetworkBehaviour<Room>.instance.SetCameraAngleIndex_Local(NetworkedManagerBase<ZoneManager>.instance.currentNode.roomRotIndex);
			UnityEngine.Debug.Log("Mid-join complete");
		}
	}

	protected static void InvokeUserCode_TpcNotifyHeroSpawned(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			UnityEngine.Debug.LogError("TargetRPC TpcNotifyHeroSpawned called on server.");
		}
		else
		{
			((DewPlayer)(object)obj).UserCode_TpcNotifyHeroSpawned();
		}
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_05e1: Unknown result type (might be due to invalid IL or missing references)
		((NetworkBehaviour)this).SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			GeneratedNetworkCode._Write_PlayerState(writer, state__BackingField);
			GeneratedNetworkCode._Write_DewPlayer_002FRole(writer, _role);
			NetworkWriterExtensions.WriteBool(writer, isEveryInfoSet__BackingField);
			NetworkWriterExtensions.WriteString(writer, playerNameRaw__BackingField);
			NetworkWriterExtensions.WriteString(writer, _equippedNametag);
			NetworkWriterExtensions.WriteBool(writer, _isHostPlayer);
			NetworkWriterExtensions.WriteTexture2D(writer, _avatar);
			NetworkWriterExtensions.WriteInt(writer, totalMasteryLevel__BackingField);
			GeneratedNetworkCode._Write_DewProfileStats(writer, profileStats__BackingField);
			NetworkWriterExtensions.WriteBool(writer, shareItemsWhenDropped__BackingField);
			NetworkWriterExtensions.WriteBool(writer, hasPolarisEndingUnlocked__BackingField);
			NetworkWriterExtensions.WriteString(writer, _eosId);
			NetworkWriterExtensions.WriteString(writer, _platformId);
			NetworkWriterExtensions.WriteString(writer, _platform);
			NetworkWriterExtensions.WriteNetworkBehaviour(writer, (NetworkBehaviour)(object)Network_controllingEntity);
			NetworkWriterExtensions.WriteNetworkBehaviour(writer, (NetworkBehaviour)(object)Network_003Chero_003Ek__BackingField);
			NetworkWriterExtensions.WriteInt(writer, gold__BackingField);
			NetworkWriterExtensions.WriteInt(writer, dreamDust__BackingField);
			NetworkWriterExtensions.WriteInt(writer, platinumCoin__BackingField);
			NetworkWriterExtensions.WriteFloat(writer, cleanseRefundMultiplier__BackingField);
			NetworkWriterExtensions.WriteFloat(writer, dismantleDreamDustMultiplier__BackingField);
			NetworkWriterExtensions.WriteFloat(writer, dismantleSkillDreamDustMultiplier__BackingField);
			NetworkWriterExtensions.WriteFloat(writer, dismantleGemDreamDustMultiplier__BackingField);
			NetworkWriterExtensions.WriteFloat(writer, sellPriceMultiplier__BackingField);
			NetworkWriterExtensions.WriteFloat(writer, buyPriceMultiplier__BackingField);
			NetworkWriterExtensions.WriteInt(writer, shopAddedItems__BackingField);
			NetworkWriterExtensions.WriteFloat(writer, potionDropChanceMultiplier__BackingField);
			NetworkWriterExtensions.WriteFloat(writer, doubleChaosChance__BackingField);
			NetworkWriterExtensions.WriteBool(writer, isReadingArtifactStory__BackingField);
			GeneratedNetworkCode._Write_Steamworks_002ECSteamID(writer, steamId);
			NetworkWriterExtensions.WriteString(writer, guid__BackingField);
			NetworkWriterExtensions.WriteString(writer, selectedHeroType__BackingField);
			GeneratedNetworkCode._Write_HeroLoadoutData(writer, selectedLoadout__BackingField);
			NetworkWriterExtensions.WriteString(writer, selectedSkin__BackingField);
			NetworkWriterExtensions.WriteString(writer, selectedDejavuItem__BackingField);
			NetworkWriterExtensions.WriteBool(writer, isReady__BackingField);
			NetworkWriterExtensions.WriteBool(writer, isLoadingForMidJoin__BackingField);
			NetworkWriterExtensions.WriteBool(writer, isWaitingForContinueMidJoin__BackingField);
			NetworkWriterExtensions.WriteBool(writer, joinedMidGame__BackingField);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 1L) != 0L)
		{
			GeneratedNetworkCode._Write_PlayerState(writer, state__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 2L) != 0L)
		{
			GeneratedNetworkCode._Write_DewPlayer_002FRole(writer, _role);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 4L) != 0L)
		{
			NetworkWriterExtensions.WriteBool(writer, isEveryInfoSet__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 8L) != 0L)
		{
			NetworkWriterExtensions.WriteString(writer, playerNameRaw__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x10L) != 0L)
		{
			NetworkWriterExtensions.WriteString(writer, _equippedNametag);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x20L) != 0L)
		{
			NetworkWriterExtensions.WriteBool(writer, _isHostPlayer);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x40L) != 0L)
		{
			NetworkWriterExtensions.WriteTexture2D(writer, _avatar);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x80L) != 0L)
		{
			NetworkWriterExtensions.WriteInt(writer, totalMasteryLevel__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x100L) != 0L)
		{
			GeneratedNetworkCode._Write_DewProfileStats(writer, profileStats__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x200L) != 0L)
		{
			NetworkWriterExtensions.WriteBool(writer, shareItemsWhenDropped__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x400L) != 0L)
		{
			NetworkWriterExtensions.WriteBool(writer, hasPolarisEndingUnlocked__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x800L) != 0L)
		{
			NetworkWriterExtensions.WriteString(writer, _eosId);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x1000L) != 0L)
		{
			NetworkWriterExtensions.WriteString(writer, _platformId);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x2000L) != 0L)
		{
			NetworkWriterExtensions.WriteString(writer, _platform);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x4000L) != 0L)
		{
			NetworkWriterExtensions.WriteNetworkBehaviour(writer, (NetworkBehaviour)(object)Network_controllingEntity);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x8000L) != 0L)
		{
			NetworkWriterExtensions.WriteNetworkBehaviour(writer, (NetworkBehaviour)(object)Network_003Chero_003Ek__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x10000L) != 0L)
		{
			NetworkWriterExtensions.WriteInt(writer, gold__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x20000L) != 0L)
		{
			NetworkWriterExtensions.WriteInt(writer, dreamDust__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x40000L) != 0L)
		{
			NetworkWriterExtensions.WriteInt(writer, platinumCoin__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x80000L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, cleanseRefundMultiplier__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x100000L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, dismantleDreamDustMultiplier__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x200000L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, dismantleSkillDreamDustMultiplier__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x400000L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, dismantleGemDreamDustMultiplier__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x800000L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, sellPriceMultiplier__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x1000000L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, buyPriceMultiplier__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x2000000L) != 0L)
		{
			NetworkWriterExtensions.WriteInt(writer, shopAddedItems__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x4000000L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, potionDropChanceMultiplier__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x8000000L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, doubleChaosChance__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x10000000L) != 0L)
		{
			NetworkWriterExtensions.WriteBool(writer, isReadingArtifactStory__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x20000000L) != 0L)
		{
			GeneratedNetworkCode._Write_Steamworks_002ECSteamID(writer, steamId);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x40000000L) != 0L)
		{
			NetworkWriterExtensions.WriteString(writer, guid__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x80000000L) != 0L)
		{
			NetworkWriterExtensions.WriteString(writer, selectedHeroType__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x100000000L) != 0L)
		{
			GeneratedNetworkCode._Write_HeroLoadoutData(writer, selectedLoadout__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x200000000L) != 0L)
		{
			NetworkWriterExtensions.WriteString(writer, selectedSkin__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x400000000L) != 0L)
		{
			NetworkWriterExtensions.WriteString(writer, selectedDejavuItem__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x800000000L) != 0L)
		{
			NetworkWriterExtensions.WriteBool(writer, isReady__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x1000000000L) != 0L)
		{
			NetworkWriterExtensions.WriteBool(writer, isLoadingForMidJoin__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x2000000000L) != 0L)
		{
			NetworkWriterExtensions.WriteBool(writer, isWaitingForContinueMidJoin__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x4000000000L) != 0L)
		{
			NetworkWriterExtensions.WriteBool(writer, joinedMidGame__BackingField);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		//IL_0277: Unknown result type (might be due to invalid IL or missing references)
		//IL_07b4: Unknown result type (might be due to invalid IL or missing references)
		((NetworkBehaviour)this).DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<PlayerState>(ref state__BackingField, _Mirror_SyncVarHookDelegate__003Cstate_003Ek__BackingField, GeneratedNetworkCode._Read_PlayerState(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<Role>(ref _role, (Action<Role, Role>)null, GeneratedNetworkCode._Read_DewPlayer_002FRole(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref isEveryInfoSet__BackingField, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<string>(ref playerNameRaw__BackingField, _Mirror_SyncVarHookDelegate__003CplayerNameRaw_003Ek__BackingField, NetworkReaderExtensions.ReadString(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<string>(ref _equippedNametag, _Mirror_SyncVarHookDelegate__equippedNametag, NetworkReaderExtensions.ReadString(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref _isHostPlayer, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<Texture2D>(ref _avatar, (Action<Texture2D, Texture2D>)null, NetworkReaderExtensions.ReadTexture2D(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref totalMasteryLevel__BackingField, (Action<int, int>)null, NetworkReaderExtensions.ReadInt(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<DewProfileStats>(ref profileStats__BackingField, (Action<DewProfileStats, DewProfileStats>)null, GeneratedNetworkCode._Read_DewProfileStats(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref shareItemsWhenDropped__BackingField, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref hasPolarisEndingUnlocked__BackingField, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<string>(ref _eosId, _Mirror_SyncVarHookDelegate__eosId, NetworkReaderExtensions.ReadString(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<string>(ref _platformId, _Mirror_SyncVarHookDelegate__platformId, NetworkReaderExtensions.ReadString(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<string>(ref _platform, _Mirror_SyncVarHookDelegate__platform, NetworkReaderExtensions.ReadString(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize_NetworkBehaviour<Entity>(ref _controllingEntity, _Mirror_SyncVarHookDelegate__controllingEntity, reader, ref ____controllingEntityNetId);
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize_NetworkBehaviour<Hero>(ref hero__BackingField, _Mirror_SyncVarHookDelegate__003Chero_003Ek__BackingField, reader, ref ____003Chero_003Ek__BackingFieldNetId);
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref gold__BackingField, _Mirror_SyncVarHookDelegate__003Cgold_003Ek__BackingField, NetworkReaderExtensions.ReadInt(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref dreamDust__BackingField, _Mirror_SyncVarHookDelegate__003CdreamDust_003Ek__BackingField, NetworkReaderExtensions.ReadInt(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref platinumCoin__BackingField, _Mirror_SyncVarHookDelegate__003CplatinumCoin_003Ek__BackingField, NetworkReaderExtensions.ReadInt(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref cleanseRefundMultiplier__BackingField, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref dismantleDreamDustMultiplier__BackingField, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref dismantleSkillDreamDustMultiplier__BackingField, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref dismantleGemDreamDustMultiplier__BackingField, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref sellPriceMultiplier__BackingField, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref buyPriceMultiplier__BackingField, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref shopAddedItems__BackingField, (Action<int, int>)null, NetworkReaderExtensions.ReadInt(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref potionDropChanceMultiplier__BackingField, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref doubleChaosChance__BackingField, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref isReadingArtifactStory__BackingField, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<CSteamID>(ref steamId, (Action<CSteamID, CSteamID>)null, GeneratedNetworkCode._Read_Steamworks_002ECSteamID(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<string>(ref guid__BackingField, (Action<string, string>)null, NetworkReaderExtensions.ReadString(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<string>(ref selectedHeroType__BackingField, _Mirror_SyncVarHookDelegate__003CselectedHeroType_003Ek__BackingField, NetworkReaderExtensions.ReadString(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<HeroLoadoutData>(ref selectedLoadout__BackingField, _Mirror_SyncVarHookDelegate__003CselectedLoadout_003Ek__BackingField, GeneratedNetworkCode._Read_HeroLoadoutData(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<string>(ref selectedSkin__BackingField, _Mirror_SyncVarHookDelegate__003CselectedSkin_003Ek__BackingField, NetworkReaderExtensions.ReadString(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<string>(ref selectedDejavuItem__BackingField, _Mirror_SyncVarHookDelegate__003CselectedDejavuItem_003Ek__BackingField, NetworkReaderExtensions.ReadString(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref isReady__BackingField, _Mirror_SyncVarHookDelegate__003CisReady_003Ek__BackingField, NetworkReaderExtensions.ReadBool(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref isLoadingForMidJoin__BackingField, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref isWaitingForContinueMidJoin__BackingField, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref joinedMidGame__BackingField, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 1L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<PlayerState>(ref state__BackingField, _Mirror_SyncVarHookDelegate__003Cstate_003Ek__BackingField, GeneratedNetworkCode._Read_PlayerState(reader));
		}
		if ((num & 2L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<Role>(ref _role, (Action<Role, Role>)null, GeneratedNetworkCode._Read_DewPlayer_002FRole(reader));
		}
		if ((num & 4L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref isEveryInfoSet__BackingField, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
		}
		if ((num & 8L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<string>(ref playerNameRaw__BackingField, _Mirror_SyncVarHookDelegate__003CplayerNameRaw_003Ek__BackingField, NetworkReaderExtensions.ReadString(reader));
		}
		if ((num & 0x10L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<string>(ref _equippedNametag, _Mirror_SyncVarHookDelegate__equippedNametag, NetworkReaderExtensions.ReadString(reader));
		}
		if ((num & 0x20L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref _isHostPlayer, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
		}
		if ((num & 0x40L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<Texture2D>(ref _avatar, (Action<Texture2D, Texture2D>)null, NetworkReaderExtensions.ReadTexture2D(reader));
		}
		if ((num & 0x80L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref totalMasteryLevel__BackingField, (Action<int, int>)null, NetworkReaderExtensions.ReadInt(reader));
		}
		if ((num & 0x100L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<DewProfileStats>(ref profileStats__BackingField, (Action<DewProfileStats, DewProfileStats>)null, GeneratedNetworkCode._Read_DewProfileStats(reader));
		}
		if ((num & 0x200L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref shareItemsWhenDropped__BackingField, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
		}
		if ((num & 0x400L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref hasPolarisEndingUnlocked__BackingField, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
		}
		if ((num & 0x800L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<string>(ref _eosId, _Mirror_SyncVarHookDelegate__eosId, NetworkReaderExtensions.ReadString(reader));
		}
		if ((num & 0x1000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<string>(ref _platformId, _Mirror_SyncVarHookDelegate__platformId, NetworkReaderExtensions.ReadString(reader));
		}
		if ((num & 0x2000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<string>(ref _platform, _Mirror_SyncVarHookDelegate__platform, NetworkReaderExtensions.ReadString(reader));
		}
		if ((num & 0x4000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize_NetworkBehaviour<Entity>(ref _controllingEntity, _Mirror_SyncVarHookDelegate__controllingEntity, reader, ref ____controllingEntityNetId);
		}
		if ((num & 0x8000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize_NetworkBehaviour<Hero>(ref hero__BackingField, _Mirror_SyncVarHookDelegate__003Chero_003Ek__BackingField, reader, ref ____003Chero_003Ek__BackingFieldNetId);
		}
		if ((num & 0x10000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref gold__BackingField, _Mirror_SyncVarHookDelegate__003Cgold_003Ek__BackingField, NetworkReaderExtensions.ReadInt(reader));
		}
		if ((num & 0x20000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref dreamDust__BackingField, _Mirror_SyncVarHookDelegate__003CdreamDust_003Ek__BackingField, NetworkReaderExtensions.ReadInt(reader));
		}
		if ((num & 0x40000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref platinumCoin__BackingField, _Mirror_SyncVarHookDelegate__003CplatinumCoin_003Ek__BackingField, NetworkReaderExtensions.ReadInt(reader));
		}
		if ((num & 0x80000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref cleanseRefundMultiplier__BackingField, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
		if ((num & 0x100000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref dismantleDreamDustMultiplier__BackingField, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
		if ((num & 0x200000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref dismantleSkillDreamDustMultiplier__BackingField, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
		if ((num & 0x400000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref dismantleGemDreamDustMultiplier__BackingField, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
		if ((num & 0x800000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref sellPriceMultiplier__BackingField, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
		if ((num & 0x1000000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref buyPriceMultiplier__BackingField, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
		if ((num & 0x2000000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref shopAddedItems__BackingField, (Action<int, int>)null, NetworkReaderExtensions.ReadInt(reader));
		}
		if ((num & 0x4000000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref potionDropChanceMultiplier__BackingField, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
		if ((num & 0x8000000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref doubleChaosChance__BackingField, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
		if ((num & 0x10000000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref isReadingArtifactStory__BackingField, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
		}
		if ((num & 0x20000000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<CSteamID>(ref steamId, (Action<CSteamID, CSteamID>)null, GeneratedNetworkCode._Read_Steamworks_002ECSteamID(reader));
		}
		if ((num & 0x40000000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<string>(ref guid__BackingField, (Action<string, string>)null, NetworkReaderExtensions.ReadString(reader));
		}
		if ((num & 0x80000000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<string>(ref selectedHeroType__BackingField, _Mirror_SyncVarHookDelegate__003CselectedHeroType_003Ek__BackingField, NetworkReaderExtensions.ReadString(reader));
		}
		if ((num & 0x100000000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<HeroLoadoutData>(ref selectedLoadout__BackingField, _Mirror_SyncVarHookDelegate__003CselectedLoadout_003Ek__BackingField, GeneratedNetworkCode._Read_HeroLoadoutData(reader));
		}
		if ((num & 0x200000000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<string>(ref selectedSkin__BackingField, _Mirror_SyncVarHookDelegate__003CselectedSkin_003Ek__BackingField, NetworkReaderExtensions.ReadString(reader));
		}
		if ((num & 0x400000000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<string>(ref selectedDejavuItem__BackingField, _Mirror_SyncVarHookDelegate__003CselectedDejavuItem_003Ek__BackingField, NetworkReaderExtensions.ReadString(reader));
		}
		if ((num & 0x800000000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref isReady__BackingField, _Mirror_SyncVarHookDelegate__003CisReady_003Ek__BackingField, NetworkReaderExtensions.ReadBool(reader));
		}
		if ((num & 0x1000000000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref isLoadingForMidJoin__BackingField, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
		}
		if ((num & 0x2000000000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref isWaitingForContinueMidJoin__BackingField, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
		}
		if ((num & 0x4000000000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref joinedMidGame__BackingField, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
		}
	}
}
