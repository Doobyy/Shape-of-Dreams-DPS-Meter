using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using DG.Tweening;
using EpicTransport;
using Mirror;
using Mirror.FizzySteam;
using Steamworks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Scripting;

public class DewNetworkManager : NetworkManager
{
	public struct ChangeSceneMessage : NetworkMessage
	{
		public string name;
	}

	private struct SetLoadingStatusMessage : NetworkMessage
	{
		public bool isLoading;

		public bool isWhite;
	}

	[StructLayout(LayoutKind.Sequential, Size = 1)]
	private struct SessionEndedMessage : NetworkMessage
	{
	}

	[StructLayout(LayoutKind.Sequential, Size = 1)]
	private struct SessionRestartingMessage : NetworkMessage
	{
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CRestartAsync_003Ed__53 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

		public DewNetworkManager _003C_003E4__this;

		private Awaiter _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_0133: Unknown result type (might be due to invalid IL or missing references)
			//IL_0138: Unknown result type (might be due to invalid IL or missing references)
			//IL_013f: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
			//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
			//IL_0248: Unknown result type (might be due to invalid IL or missing references)
			//IL_024d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0254: Unknown result type (might be due to invalid IL or missing references)
			//IL_0175: Unknown result type (might be due to invalid IL or missing references)
			//IL_017a: Unknown result type (might be due to invalid IL or missing references)
			//IL_017e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0183: Unknown result type (might be due to invalid IL or missing references)
			//IL_0197: Unknown result type (might be due to invalid IL or missing references)
			//IL_0198: Unknown result type (might be due to invalid IL or missing references)
			//IL_020f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0214: Unknown result type (might be due to invalid IL or missing references)
			//IL_0218: Unknown result type (might be due to invalid IL or missing references)
			//IL_021d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0231: Unknown result type (might be due to invalid IL or missing references)
			//IL_0232: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
			//IL_0100: Unknown result type (might be due to invalid IL or missing references)
			//IL_0105: Unknown result type (might be due to invalid IL or missing references)
			//IL_0119: Unknown result type (might be due to invalid IL or missing references)
			//IL_011a: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			DewNetworkManager dewNetworkManager = _003C_003E4__this;
			try
			{
				UniTask val2;
				Awaiter val;
				switch (num)
				{
				default:
					Dew.GetControlPresetWindow()?.Hide();
					DewSave.SaveProfileAll();
					DewSave.SavePlatformSettings();
					switch (startSettings.networkMode)
					{
					case DewNetworkMode.Singleplayer:
						startSettings.networkMode = DewNetworkMode.Singleplayer;
						break;
					case DewNetworkMode.MultiplayerHost:
					case DewNetworkMode.MultiplayerHostRestart:
						startSettings.networkMode = DewNetworkMode.MultiplayerHostRestart;
						break;
					case DewNetworkMode.MultiplayerJoinLobby:
					case DewNetworkMode.MultiplayerJoinRestart:
						startSettings.networkMode = DewNetworkMode.MultiplayerJoinRestart;
						break;
					default:
						throw new ArgumentOutOfRangeException();
					}
					startSettings.continueData = null;
					if ((UnityEngine.Object)(object)NetworkedManagerBase<GameSettingsManager>.instance != null)
					{
						startSettings.addedGameMods = ((IEnumerable<string>)NetworkedManagerBase<GameSettingsManager>.instance.addedGameMods).ToList();
						startSettings.customGameSettingsSaveKey = NetworkedManagerBase<GameSettingsManager>.instance.gameSettingsSaveKey;
					}
					dewNetworkManager._isThisManagerRestarting = true;
					ManagerBase<TransitionManager>.instance.FadeOut(showTips: false);
					val2 = UniTask.WaitForSeconds(1f, true, (PlayerLoopTiming)8, default(CancellationToken));
					val = val2.GetAwaiter();
					if (!val.IsCompleted)
					{
						num = (_003C_003E1__state = 0);
						_003C_003Eu__1 = val;
						_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter, _003CRestartAsync_003Ed__53>(ref val, ref this);
						return;
					}
					goto IL_014e;
				case 0:
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default;
					num = (_003C_003E1__state = -1);
					goto IL_014e;
				case 1:
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default;
					num = (_003C_003E1__state = -1);
					goto IL_01cc;
				case 2:
					{
						val = _003C_003Eu__1;
						_003C_003Eu__1 = default;
						num = (_003C_003E1__state = -1);
						break;
					}
					IL_014e:
					val.GetResult();
					ActorManager.CleanupActorsBeforeShutdown();
					NetworkServer.Shutdown();
					NetworkClient.Shutdown();
					val2 = UniTask.WaitForSeconds(0.25f, true, (PlayerLoopTiming)8, default(CancellationToken));
					val = val2.GetAwaiter();
					if (!val.IsCompleted)
					{
						num = (_003C_003E1__state = 1);
						_003C_003Eu__1 = val;
						_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter, _003CRestartAsync_003Ed__53>(ref val, ref this);
						return;
					}
					goto IL_01cc;
					IL_01cc:
					val.GetResult();
					if (ManagerBase<GameLogicPackage>.instance != null)
					{
						UnityEngine.Object.Destroy(ManagerBase<GameLogicPackage>.instance.gameObject);
					}
					UnityEngine.Object.Destroy(ManagerBase<NetworkLogicPackage>.instance.gameObject);
					val2 = UniTask.WaitForSeconds(0.25f, true, (PlayerLoopTiming)8, default(CancellationToken));
					val = val2.GetAwaiter();
					if (!val.IsCompleted)
					{
						num = (_003C_003E1__state = 2);
						_003C_003Eu__1 = val;
						_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter, _003CRestartAsync_003Ed__53>(ref val, ref this);
						return;
					}
					break;
				}
				val.GetResult();
				SceneManager.LoadScene("PlayLobby");
				ManagerBase<TransitionManager>.instance.FadeIn();
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

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CStartSession_003Ed__60 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

		public DewNetworkManager _003C_003E4__this;

		private Awaiter _003C_003Eu__1;

		private string _003CrestartLobbyId_003E5__2;

		private void MoveNext()
		{
			//IL_0118: Unknown result type (might be due to invalid IL or missing references)
			//IL_011d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0125: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f8: Unknown result type (might be due to invalid IL or missing references)
			//IL_01fd: Unknown result type (might be due to invalid IL or missing references)
			//IL_0205: Unknown result type (might be due to invalid IL or missing references)
			//IL_026d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0272: Unknown result type (might be due to invalid IL or missing references)
			//IL_027a: Unknown result type (might be due to invalid IL or missing references)
			//IL_02d8: Unknown result type (might be due to invalid IL or missing references)
			//IL_02dd: Unknown result type (might be due to invalid IL or missing references)
			//IL_02e5: Unknown result type (might be due to invalid IL or missing references)
			//IL_03f2: Unknown result type (might be due to invalid IL or missing references)
			//IL_03f7: Unknown result type (might be due to invalid IL or missing references)
			//IL_03ff: Unknown result type (might be due to invalid IL or missing references)
			//IL_04e9: Unknown result type (might be due to invalid IL or missing references)
			//IL_04ee: Unknown result type (might be due to invalid IL or missing references)
			//IL_04f6: Unknown result type (might be due to invalid IL or missing references)
			//IL_055a: Unknown result type (might be due to invalid IL or missing references)
			//IL_055f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0567: Unknown result type (might be due to invalid IL or missing references)
			//IL_029a: Unknown result type (might be due to invalid IL or missing references)
			//IL_029f: Unknown result type (might be due to invalid IL or missing references)
			//IL_02a3: Unknown result type (might be due to invalid IL or missing references)
			//IL_02a8: Unknown result type (might be due to invalid IL or missing references)
			//IL_051c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0521: Unknown result type (might be due to invalid IL or missing references)
			//IL_0525: Unknown result type (might be due to invalid IL or missing references)
			//IL_052a: Unknown result type (might be due to invalid IL or missing references)
			//IL_022f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0234: Unknown result type (might be due to invalid IL or missing references)
			//IL_0238: Unknown result type (might be due to invalid IL or missing references)
			//IL_023d: Unknown result type (might be due to invalid IL or missing references)
			//IL_02bd: Unknown result type (might be due to invalid IL or missing references)
			//IL_02bf: Unknown result type (might be due to invalid IL or missing references)
			//IL_053f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0541: Unknown result type (might be due to invalid IL or missing references)
			//IL_00da: Unknown result type (might be due to invalid IL or missing references)
			//IL_00df: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
			//IL_03b4: Unknown result type (might be due to invalid IL or missing references)
			//IL_03b9: Unknown result type (might be due to invalid IL or missing references)
			//IL_03bd: Unknown result type (might be due to invalid IL or missing references)
			//IL_03c2: Unknown result type (might be due to invalid IL or missing references)
			//IL_0252: Unknown result type (might be due to invalid IL or missing references)
			//IL_0254: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
			//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
			//IL_03d7: Unknown result type (might be due to invalid IL or missing references)
			//IL_03d9: Unknown result type (might be due to invalid IL or missing references)
			//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
			//IL_01df: Unknown result type (might be due to invalid IL or missing references)
			//IL_04ab: Unknown result type (might be due to invalid IL or missing references)
			//IL_04b0: Unknown result type (might be due to invalid IL or missing references)
			//IL_04b4: Unknown result type (might be due to invalid IL or missing references)
			//IL_04b9: Unknown result type (might be due to invalid IL or missing references)
			//IL_04ce: Unknown result type (might be due to invalid IL or missing references)
			//IL_04d0: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			DewNetworkManager dewNetworkManager = _003C_003E4__this;
			try
			{
				if ((uint)num > 6u)
				{
					UnityEngine.Debug.Log(string.Format("{0} - StartGame - {1}", "DewNetworkManager", startSettings.networkMode));
				}
				try
				{
					Awaiter val;
					float startTime;
					float timeout;
					float nextLobbyRefresh;
					UniTask val2;
					switch (num)
					{
					default:
						switch (startSettings.networkMode)
						{
						case DewNetworkMode.Singleplayer:
							break;
						case DewNetworkMode.MultiplayerHost:
							goto IL_00a5;
						case DewNetworkMode.MultiplayerJoinLobby:
							goto IL_015c;
						case DewNetworkMode.MultiplayerHostRestart:
							goto IL_0300;
						case DewNetworkMode.MultiplayerJoinRestart:
							goto IL_032f;
						default:
							throw new ArgumentOutOfRangeException();
						}
						NetworkServer.dontListen = true;
						ManagerBase<TransitionManager>.instance.UpdateLoadingStatus(LoadingStatus.CreatingGame);
						((NetworkManager)dewNetworkManager).StartHost();
						break;
					case 0:
						val = _003C_003Eu__1;
						_003C_003Eu__1 = default;
						num = (_003C_003E1__state = -1);
						goto IL_0134;
					case 1:
						val = _003C_003Eu__1;
						_003C_003Eu__1 = default;
						num = (_003C_003E1__state = -1);
						goto IL_0214;
					case 2:
						val = _003C_003Eu__1;
						_003C_003Eu__1 = default;
						num = (_003C_003E1__state = -1);
						goto IL_0289;
					case 3:
						val = _003C_003Eu__1;
						_003C_003Eu__1 = default;
						num = (_003C_003E1__state = -1);
						goto IL_02f4;
					case 4:
						val = _003C_003Eu__1;
						_003C_003Eu__1 = default;
						num = (_003C_003E1__state = -1);
						goto IL_040e;
					case 5:
						val = _003C_003Eu__1;
						_003C_003Eu__1 = default;
						num = (_003C_003E1__state = -1);
						goto IL_0505;
					case 6:
						{
							val = _003C_003Eu__1;
							_003C_003Eu__1 = default;
							num = (_003C_003E1__state = -1);
							goto IL_0576;
						}
						IL_032f:
						if (ManagerBase<LobbyManager>.instance.service.currentLobby == null)
						{
							throw new DewException(DewExceptionType.LobbyNotFound, "Cannot wait for restart");
						}
						startTime = Time.unscaledTime;
						timeout = 30f;
						UnityEngine.Debug.Log(string.Format("{0} - Waiting for host, timeout: {1:0.#} seconds", "DewNetworkManager", timeout));
						ManagerBase<TransitionManager>.instance.UpdateLoadingStatus(LoadingStatus.WaitingForHostPlayer);
						nextLobbyRefresh = Time.unscaledTime + 5f;
						val2 = UniTask.WaitWhile((Func<bool>)(() =>
						{
							//IL_0028: Unknown result type (might be due to invalid IL or missing references)
							if (Time.unscaledTime >= nextLobbyRefresh)
							{
								nextLobbyRefresh = Time.unscaledTime + 5f;
								UniTaskExtensions.Forget(ManagerBase<LobbyManager>.instance.service.RefreshCurrentLobbyFromBackend());
							}
							return ManagerBase<LobbyManager>.instance.service.currentLobby != null && ManagerBase<LobbyManager>.instance.service.currentLobby.hasGameStarted && Time.unscaledTime - startTime < timeout;
						}), (PlayerLoopTiming)8, default(CancellationToken));
						val = val2.GetAwaiter();
						if (!val.IsCompleted)
						{
							num = (_003C_003E1__state = 4);
							_003C_003Eu__1 = val;
							_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter, _003CStartSession_003Ed__60>(ref val, ref this);
							return;
						}
						goto IL_040e;
						IL_0576:
						val.GetResult();
						if (ManagerBase<LobbyManager>.instance.service.currentLobby == null)
						{
							throw new DewException(DewExceptionType.LobbyNotFound, "Lobby rejoin after EOS reset failed");
						}
						_003CrestartLobbyId_003E5__2 = null;
						goto IL_05a1;
						IL_015c:
						if (startSettings.lanMode)
						{
							UnityEngine.Debug.Log("LAN mode, connecting to " + startSettings.address);
							ManagerBase<TransitionManager>.instance.UpdateLoadingStatus(LoadingStatus.ConnectingToGame);
							((NetworkManager)dewNetworkManager).networkAddress = startSettings.address;
							((NetworkManager)dewNetworkManager).StartClient();
							break;
						}
						if (ManagerBase<EOSManager>.instance != null)
						{
							val2 = ManagerBase<EOSManager>.instance.EnsureCleanEOSForJoin();
							val = val2.GetAwaiter();
							if (!val.IsCompleted)
							{
								num = (_003C_003E1__state = 1);
								_003C_003Eu__1 = val;
								_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter, _003CStartSession_003Ed__60>(ref val, ref this);
								return;
							}
							goto IL_0214;
						}
						goto IL_021b;
						IL_0134:
						val.GetResult();
						UnityEngine.Debug.Log("DewNetworkManager - Starting host");
						goto IL_0145;
						IL_040e:
						val.GetResult();
						if (ManagerBase<LobbyManager>.instance.service.currentLobby == null)
						{
							throw new DewException(DewExceptionType.Disconnected);
						}
						if (ManagerBase<LobbyManager>.instance.service.currentLobby.hasGameStarted)
						{
							throw new DewException(DewExceptionType.LobbyTimeout, "Host did not restart in time");
						}
						if (ManagerBase<EOSManager>.instance != null && EOSManager.p2pDirty && ManagerBase<LobbyManager>.instance.service is LobbyServiceEOS)
						{
							_003CrestartLobbyId_003E5__2 = ManagerBase<LobbyManager>.instance.service.currentLobby.id;
							UnityEngine.Debug.Log("DewNetworkManager - P2P state dirty, resetting EOS and rejoining lobby before connect");
							val2 = ManagerBase<EOSManager>.instance.EnsureCleanEOSForJoin();
							val = val2.GetAwaiter();
							if (!val.IsCompleted)
							{
								num = (_003C_003E1__state = 5);
								_003C_003Eu__1 = val;
								_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter, _003CStartSession_003Ed__60>(ref val, ref this);
								return;
							}
							goto IL_0505;
						}
						goto IL_05a1;
						IL_05a1:
						UnityEngine.Debug.Log("DewNetworkManager - Joining the restarted game");
						((NetworkManager)dewNetworkManager).networkAddress = ManagerBase<LobbyManager>.instance.service.currentLobby.gameServerAddress;
						ManagerBase<TransitionManager>.instance.UpdateLoadingStatus(LoadingStatus.ConnectingToGame);
						((NetworkManager)dewNetworkManager).StartClient();
						break;
						IL_0214:
						val.GetResult();
						goto IL_021b;
						IL_021b:
						val2 = ManagerBase<LobbyManager>.instance.service.JoinLobby(startSettings.address);
						val = val2.GetAwaiter();
						if (!val.IsCompleted)
						{
							num = (_003C_003E1__state = 2);
							_003C_003Eu__1 = val;
							_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter, _003CStartSession_003Ed__60>(ref val, ref this);
							return;
						}
						goto IL_0289;
						IL_0145:
						ManagerBase<TransitionManager>.instance.UpdateLoadingStatus(LoadingStatus.CreatingGame);
						((NetworkManager)dewNetworkManager).StartHost();
						break;
						IL_0289:
						val.GetResult();
						val2 = ManagerBase<LobbyManager>.instance.service.JoinGameOfCurrentLobby();
						val = val2.GetAwaiter();
						if (!val.IsCompleted)
						{
							num = (_003C_003E1__state = 3);
							_003C_003Eu__1 = val;
							_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter, _003CStartSession_003Ed__60>(ref val, ref this);
							return;
						}
						goto IL_02f4;
						IL_0300:
						if (!ManagerBase<LobbyManager>.instance.isLobbyLeader)
						{
							throw new DewException(DewExceptionType.LobbyNotFound, "Cannot restart");
						}
						ManagerBase<TransitionManager>.instance.UpdateLoadingStatus(LoadingStatus.CreatingGame);
						((NetworkManager)dewNetworkManager).StartHost();
						break;
						IL_02f4:
						val.GetResult();
						break;
						IL_00a5:
						NetworkServer.dontListen = false;
						NetworkServer.maxConnections = startSettings.maxPlayers;
						if (!startSettings.lanMode)
						{
							UnityEngine.Debug.Log("DewNetworkManager - Creating lobby");
							val2 = ManagerBase<LobbyManager>.instance.service.CreateLobby();
							val = val2.GetAwaiter();
							if (!val.IsCompleted)
							{
								num = (_003C_003E1__state = 0);
								_003C_003Eu__1 = val;
								_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter, _003CStartSession_003Ed__60>(ref val, ref this);
								return;
							}
							goto IL_0134;
						}
						goto IL_0145;
						IL_0505:
						val.GetResult();
						val2 = ManagerBase<LobbyManager>.instance.service.JoinLobby(_003CrestartLobbyId_003E5__2);
						val = val2.GetAwaiter();
						if (!val.IsCompleted)
						{
							num = (_003C_003E1__state = 6);
							_003C_003Eu__1 = val;
							_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter, _003CStartSession_003Ed__60>(ref val, ref this);
							return;
						}
						goto IL_0576;
					}
					if (startSettings.continueData == null)
					{
						ManagerBase<TransitionManager>.instance.FadeIn();
					}
				}
				catch (Exception ex)
				{
					if (!(ex is SteamException) && !(ex is DewException))
					{
						UnityEngine.Debug.LogException(ex);
					}
					dewNetworkManager.didRegisterError = true;
					DewSessionError.ShowError(ex);
					dewNetworkManager.ReturnToTitleImmediately();
				}
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

	public static DewNetworkStartSettings startSettings = new DewNetworkStartSettings();

	public SafeAction<DewNetworkBehaviour> ClientEvent_OnDewNetworkBehaviourStart;

	public SafeAction<DewNetworkBehaviour> ClientEvent_OnDewNetworkBehaviourStop;

	public SafeAction<bool> ClientEvent_OnLoadingStatusChanged;

	public SafeAction ClientEvent_OnSessionEnd;

	private bool _isThisManagerRestarting;

	private bool _didConnect;

	internal Dictionary<NetworkConnectionToClient, DewAuthRequestMessage> _authMessagesFromClients = new Dictionary<NetworkConnectionToClient, DewAuthRequestMessage>();

	public bool hasSessionEnded { get; private set; }

	public bool didRegisterError { get; set; }

	public bool isBeingKicked { get; internal set; }

	public static DewNetworkManager instance
	{
		get
		{
			if ((UnityEngine.Object)(object)softInstance == null)
			{
				softInstance = UnityEngine.Object.FindObjectOfType<DewNetworkManager>();
			}
			if ((UnityEngine.Object)(object)softInstance == null)
			{
				softInstance = UnityEngine.Object.FindObjectOfType<DewNetworkManager>(true);
			}
			return softInstance;
		}
	}

	public static DewNetworkManager softInstance { get; private set; }

	public bool isEndingSession { get; private set; }

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	private static void OnInit()
	{
		startSettings = new DewNetworkStartSettings();
	}

	public override void Awake()
	{
		if (startSettings.lanMode)
		{
			base.transport = (Transport)(object)((Component)this).GetComponent<TelepathyTransport>();
		}
		else if (DewBuildProfile.current.platform == PlatformType.STEAM && DewBuildProfile.current.useSteamLobbyAndRelay)
		{
			base.transport = (Transport)(object)((Component)this).GetComponent<FizzySteamworks>();
		}
		else if (LobbyServiceEOS.CROSSPLAY)
		{
			base.transport = (Transport)(object)((Component)this).GetComponent<EosTransport>();
		}
		EosTransport.SuppressPeerInactivityKick = () => ManagerBase<PlayLobbyManager>.softInstance == null || NetworkClient.isLoadingScene || (ManagerBase<TransitionManager>.softInstance != null && ManagerBase<TransitionManager>.softInstance.state == TransitionManager.StateType.Loading);
		if (!((UnityEngine.Object)(object)instance != null) || !((UnityEngine.Object)(object)instance != (UnityEngine.Object)(object)this))
		{
			softInstance = this;
			((NetworkManager)this).Awake();
			NetworkClient.RegisterHandler<ChangeSceneMessage>((Action<ChangeSceneMessage>)HandleRoomChangedMessage, true);
			NetworkClient.RegisterHandler<SetLoadingStatusMessage>((Action<SetLoadingStatusMessage>)HandleSetLoadingStatusMessage, true);
			NetworkClient.RegisterHandler<SessionEndedMessage>((Action<SessionEndedMessage>)HandleGameEndedMessage, true);
			NetworkClient.RegisterHandler<SessionRestartingMessage>((Action<SessionRestartingMessage>)HandleGameRestartingMessage, true);
			InGameAnalyticsManager.RegisterHandlers();
			DewEffect.RegisterHandlers();
		}
	}

	private void InvokeSessionEndedIfDidnt()
	{
		if (!hasSessionEnded)
		{
			hasSessionEnded = true;
			ClientEvent_OnSessionEnd?.Invoke();
		}
	}

	private void HandleGameEndedMessage(SessionEndedMessage obj)
	{
		((MonoBehaviour)this).StartCoroutine(Routine());
		IEnumerator Routine()
		{
			InvokeSessionEndedIfDidnt();
			ManagerBase<TransitionManager>.instance.ResetWhiteFade();
			ManagerBase<TransitionManager>.instance.FadeOut(showTips: false);
			yield return new WaitForSecondsRealtime(1f);
			ActorManager.CleanupActorsBeforeShutdown();
			NetworkServer.Shutdown();
			NetworkClient.Shutdown();
			if (ManagerBase<GameLogicPackage>.instance != null)
			{
				UnityEngine.Object.Destroy(ManagerBase<GameLogicPackage>.instance.gameObject);
			}
			UnityEngine.Object.Destroy(ManagerBase<NetworkLogicPackage>.instance.gameObject);
			SceneManager.LoadScene("Title");
			ManagerBase<TransitionManager>.instance.FadeIn();
		}
	}

	private void HandleSetLoadingStatusMessage(SetLoadingStatusMessage msg)
	{
		if (InGameUIManager.instance != null)
		{
			if (msg.isLoading)
			{
				InGameUIManager.instance.SetState("Loading");
			}
			else if (!InGameUIManager.instance.IsState("Cutscene"))
			{
				InGameUIManager.instance.SetState("Playing");
			}
		}
		if (msg.isWhite)
		{
			if (hasSessionEnded || isEndingSession)
			{
				ManagerBase<TransitionManager>.instance.ResetWhiteFade();
			}
			else
			{
				ManagerBase<TransitionManager>.instance.whiteFadeCg.blocksRaycasts = msg.isLoading;
				ShortcutExtensions.DOKill((Component)(object)ManagerBase<TransitionManager>.instance.whiteFadeCg, false);
				DOTweenModuleUI.DOFade(ManagerBase<TransitionManager>.instance.whiteFadeCg, msg.isLoading ? 1f : 0f, 1f);
			}
		}
		else if (msg.isLoading)
		{
			ManagerBase<TransitionManager>.instance.FadeOut(showTips: true);
			ManagerBase<TransitionManager>.instance.UpdateLoadingStatus(LoadingStatus.Empty);
		}
		else
		{
			ManagerBase<TransitionManager>.instance.FadeIn();
		}
		try
		{
			ClientEvent_OnLoadingStatusChanged?.Invoke(msg.isLoading);
		}
		catch (Exception exception)
		{
			UnityEngine.Debug.LogException(exception);
		}
	}

	private void HandleRoomChangedMessage(ChangeSceneMessage msg)
	{
		((MonoBehaviour)this).StartCoroutine(Routine());
		IEnumerator Routine()
		{
			if (NetworkServer.active)
			{
				NetworkClient.Ready();
			}
			else
			{
				NetworkClient.isLoadingScene = true;
				yield return SceneManager.LoadSceneAsync(msg.name, new LoadSceneParameters
				{
					loadSceneMode = LoadSceneMode.Single,
					localPhysicsMode = LocalPhysicsMode.None
				});
				yield return Resources.UnloadUnusedAssets();
				GarbageCollector.CollectIncremental(ulong.MaxValue);
				GC.Collect();
				for (int i = 0; i < 10; i++)
				{
					yield return null;
				}
				NetworkClient.isLoadingScene = false;
				NetworkClient.PrepareToSpawnSceneObjects();
				if (!NetworkClient.ready)
				{
					NetworkClient.Ready();
				}
			}
		}
	}

	public override void OnServerAddPlayer(NetworkConnectionToClient conn)
	{
		SpawnPlayerObject(conn);
		NetworkedManagerBase<ChatManager>.instance.BroadcastMessage(new ChatManager.Message
		{
			type = ChatManager.MessageType.Notice,
			content = "Chat_Notice_PlayerJoinedGame",
			args = new string[1] { conn.GetPlayer().playerName }
		});
	}

	public override void OnServerDisconnect(NetworkConnectionToClient conn)
	{
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		DewPlayer player = conn.GetPlayer();
		if ((UnityEngine.Object)(object)player == null)
		{
			return;
		}
		if ((UnityEngine.Object)(object)NetworkedManagerBase<GameManager>.instance != null && player.state == PlayerState.Playing)
		{
			if (!player.hero.IsNullOrInactive())
			{
				player.hero.Skill.DropItemsOnOwnerDisconnect();
			}
			NetworkedManagerBase<GameManager>.instance.CollectPlayerRejoinData(player);
		}
		if (player.isKicked)
		{
			NetworkedManagerBase<ChatManager>.instance.BroadcastMessage(new ChatManager.Message
			{
				type = ChatManager.MessageType.Notice,
				content = "Chat_Notice_PlayerKickedFromGame",
				args = new string[1] { conn.GetPlayer().playerName }
			});
		}
		else
		{
			NetworkedManagerBase<ChatManager>.instance.BroadcastMessage(new ChatManager.Message
			{
				type = ChatManager.MessageType.Notice,
				content = "Chat_Notice_PlayerExitedGame",
				args = new string[1] { conn.GetPlayer().playerName }
			});
		}
		NetworkIdentity[] array = ((NetworkConnection)conn).owned.ToArray();
		foreach (NetworkIdentity val in array)
		{
			if (((Component)(object)val).TryGetComponent(out Actor component))
			{
				component.Destroy();
			}
			else
			{
				NetworkServer.Destroy(((Component)(object)val).gameObject);
			}
		}
		((NetworkConnection)conn).owned.Clear();
		if (!NetworkServer.dontListen && ManagerBase<LobbyManager>.instance.service.currentLobby != null)
		{
			ManagerBase<LobbyManager>.instance.service.HandleUserLeavingGame(conn.address);
		}
		((NetworkManager)this).OnServerDisconnect(conn);
	}

	public override void OnClientConnect()
	{
		((NetworkManager)this).OnClientConnect();
		_didConnect = true;
	}

	public override void OnClientDisconnect()
	{
		((NetworkManager)this).OnClientDisconnect();
		if (ManagerBase<LobbyManager>.instance != null && ManagerBase<LobbyManager>.instance.service is LobbyServiceEOS)
		{
			EOSManager.p2pDirty = true;
		}
		if (_isThisManagerRestarting)
		{
			return;
		}
		if (!didRegisterError && ((UnityEngine.Object)(object)NetworkedManagerBase<GameManager>.instance == null || !NetworkedManagerBase<GameManager>.instance.isGameConcluded) && !isBeingKicked)
		{
			if (_didConnect)
			{
				DewSessionError.ShowError(new DewException(DewExceptionType.Disconnected));
			}
			else
			{
				DewSessionError.ShowError();
			}
			didRegisterError = true;
		}
		InvokeSessionEndedIfDidnt();
		Dew.CallDelayed(() =>
		{
			DewSave.SaveProfileAll();
			DewSave.SavePlatformSettings();
			ReturnToTitleImmediately();
		});
	}

	public override void OnStartHost()
	{
		((NetworkManager)this).OnStartHost();
		if (startSettings.continueData != null)
		{
			((MonoBehaviour)this).StartCoroutine(Routine());
		}
		IEnumerator Routine()
		{
			yield return new WaitWhile(() => (UnityEngine.Object)(object)DewPlayer.local == null);
			SetLoadingStatus(isLoading: true);
			NetworkedManagerBase<GameSettingsManager>.instance.state = GameState.Starting;
			yield return new WaitWhile(() => !DewPlayer.local.isEveryInfoSet);
			yield return LoadSceneAsync("PlayGame");
			NetworkedManagerBase<GameSettingsManager>.instance.state = GameState.InGame;
		}
	}

	public override void OnStartClient()
	{
		((NetworkManager)this).OnStartClient();
		DewResources.ClearAllVariants(repairReferences: true);
		NetworkedManagerBase<ConsoleManager>.instance.ExecuteAutoExec(ConsoleManager.AutoExecKey.Network);
		NetworkedManagerBase<ConsoleManager>.instance.ExecuteAutoExec(NetworkServer.active ? ConsoleManager.AutoExecKey.NetworkServer : ConsoleManager.AutoExecKey.NetworkClient);
	}

	private void StartLobbyTimeoutTimer()
	{
		((MonoBehaviour)this).StartCoroutine(Routine());
		IEnumerator Routine()
		{
			for (int i = 0; i < 10; i++)
			{
				yield return new WaitForSeconds(1f);
				if (NetworkServer.active || NetworkClient.active)
				{
					yield break;
				}
			}
			didRegisterError = true;
			DewSessionError.ShowError(new DewException(DewExceptionType.LobbyTimeout));
			ReturnToTitleImmediately();
		}
	}

	public override void Start()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		if (!((UnityEngine.Object)(object)instance != (UnityEngine.Object)(object)this))
		{
			((NetworkManager)this).Start();
			if (!NetworkServer.active && !NetworkClient.active)
			{
				StartSession();
				LobbyManager lobbyManager = ManagerBase<LobbyManager>.instance;
				lobbyManager.onCurrentLobbyChanged = (Action)Delegate.Combine(lobbyManager.onCurrentLobbyChanged, new Action(OnCurrentLobbyChanged));
			}
		}
	}

	public override void OnDestroy()
	{
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		((NetworkManager)this).OnDestroy();
		startSettings.continueData = null;
		if (ManagerBase<LobbyManager>.instance != null)
		{
			LobbyManager lobbyManager = ManagerBase<LobbyManager>.instance;
			lobbyManager.onCurrentLobbyChanged = (Action)Delegate.Remove(lobbyManager.onCurrentLobbyChanged, new Action(OnCurrentLobbyChanged));
			if (!_isThisManagerRestarting && startSettings.networkMode != DewNetworkMode.Singleplayer && !startSettings.lanMode)
			{
				ManagerBase<LobbyManager>.instance.service.LeaveLobby();
			}
		}
		DewPlayer.onGamePlayerAdded = null;
		DewPlayer.onGamePlayerRemoved = null;
		DewPlayer.onSpectatorAdded = null;
		DewPlayer.onSpectatorRemoved = null;
		DewPlayer.onHumanPlayerAdded = null;
		DewPlayer.onHumanPlayerRemoved = null;
		DewPlayer.onLobbyPlayerAdded = null;
		DewPlayer.onLobbyPlayerRemoved = null;
		DewResources.RepairMissingReferences_Prepare();
		foreach (KeyValuePair<string, SafeAction<GameObject>> item in DewMod.currentJsonOverrideProcessorsFromServer)
		{
			DewResources.ClearVariantsOfAsset(item.Key, null, repairReferences: false);
		}
		DewResources.RepairMissingReferences_Repair();
		DewMod.currentJsonOverrideProcessorsFromServer = new Dictionary<string, SafeAction<GameObject>>();
	}

	private async void OnCurrentLobbyChanged()
	{
		if (NetworkServer.active && !NetworkServer.dontListen && ManagerBase<LobbyManager>.instance.service.currentLobby == null)
		{
			for (int i = 0; i < 3; i++)
			{
				UnityEngine.Debug.Log($"Lobby deleted mid-session; creating a new one... (Try {i + 1})");
				try
				{
					await ManagerBase<LobbyManager>.instance.service.CreateLobby();
					foreach (DewPlayer allHumanPlayer in DewPlayer.allHumanPlayers)
					{
						allHumanPlayer.TpcMakePlayerChangeLobby(ManagerBase<LobbyManager>.instance.service.currentLobby.id);
					}
					return;
				}
				catch (Exception exception)
				{
					UnityEngine.Debug.LogException(exception);
				}
			}
			UnityEngine.Debug.LogWarning("Mid-session create lobby failed.");
		}
		if (!NetworkServer.active && NetworkClient.active && ManagerBase<LobbyManager>.instance.service.currentLobby == null && (UnityEngine.Object)(object)DewPlayer.local != null)
		{
			UnityEngine.Debug.Log("Disconnected from lobby mid-session; requesting to join...");
			DewPlayer.local.CmdRequestToJoinCurrentLobby();
		}
	}

	public override void OnServerConnect(NetworkConnectionToClient conn)
	{
		((NetworkManager)this).OnServerConnect(conn);
		if (NetworkedManagerBase<GameSettingsManager>.instance.state == GameState.Starting)
		{
			((NetworkConnection)conn).Disconnect();
		}
		else if (NetworkedManagerBase<GameSettingsManager>.instance.state == GameState.InGame && NetworkedManagerBase<GameSettingsManager>.instance.allowMidJoins == AllowMidJoinType.Disallow)
		{
			((NetworkConnection)conn).Disconnect();
		}
	}

	private void ReturnToTitleImmediately()
	{
		if (ManagerBase<TransitionManager>.instance != null)
		{
			ManagerBase<TransitionManager>.instance.ResetWhiteFade();
		}
		ActorManager.CleanupActorsBeforeShutdown();
		NetworkServer.Shutdown();
		NetworkClient.Shutdown();
		if (ManagerBase<GameLogicPackage>.instance != null)
		{
			UnityEngine.Object.Destroy(ManagerBase<GameLogicPackage>.instance.gameObject);
		}
		if (ManagerBase<NetworkLogicPackage>.instance != null)
		{
			UnityEngine.Object.Destroy(ManagerBase<NetworkLogicPackage>.instance.gameObject);
		}
		if (SceneManager.GetActiveScene().name != "Title")
		{
			SceneManager.LoadScene("Title");
		}
	}

	public void EndSession()
	{
		if (isEndingSession)
		{
			return;
		}
		isEndingSession = true;
		try
		{
			if ((UnityEngine.Object)(object)NetworkedManagerBase<GameManager>.instance != null && NetworkedManagerBase<GameManager>.instance.IsEligibleForMidRunSave())
			{
				NetworkedManagerBase<GameManager>.instance.SaveContinueDataMidRun();
			}
		}
		catch (Exception exception)
		{
			UnityEngine.Debug.LogException(exception);
		}
		try
		{
			if ((UnityEngine.Object)(object)NetworkedManagerBase<ActorManager>.instance != null && NetworkServer.active)
			{
				foreach (Entity allEntity in NetworkedManagerBase<ActorManager>.instance.allEntities)
				{
					if (!allEntity.IsNullInactiveDeadOrKnockedOut())
					{
						allEntity.CreateBasicEffect(allEntity, new DeathInterruptEffect
						{
							onInterrupt = (EventInfoKill k) =>
							{
								k.victim.Status.SetHealth(1f);
							}
						}, 3600f);
						allEntity.Control.Stop();
						allEntity.Control.CancelOngoingChannels();
						allEntity.Control.CancelOngoingDisplacement();
					}
				}
			}
		}
		catch (Exception exception2)
		{
			UnityEngine.Debug.LogException(exception2);
		}
		try
		{
			if ((UnityEngine.Object)(object)NetworkedManagerBase<GameResultManager>.instance != null && NetworkedManagerBase<GameResultManager>.instance.tracked != null && NetworkServer.active)
			{
				NetworkedManagerBase<GameResultManager>.instance.UpdateAndSendMidGameResultToClients();
			}
		}
		catch (Exception exception3)
		{
			UnityEngine.Debug.LogException(exception3);
		}
		if (NetworkServer.active)
		{
			NetworkServer.SendToAll<SessionEndedMessage>(default(SessionEndedMessage), 0, false);
		}
		else
		{
			HandleGameEndedMessage(default);
		}
	}

	public IEnumerator LoadSceneAsync(string sceneName, List<DewPlayer> players = null, Action onBeforeSpawnObjects = null)
	{
		if (players == null)
		{
			players = DewPlayer.allHumanPlayers;
		}
		foreach (DewPlayer player in players)
		{
			NetworkServer.SetClientNotReady((NetworkConnectionToClient)player);
		}
		NetworkServer.isLoadingScene = true;
		yield return null;
		foreach (DewPlayer player2 in players)
		{
			((NetworkConnection)((NetworkBehaviour)player2).connectionToClient).Send<ChangeSceneMessage>(new ChangeSceneMessage
			{
				name = sceneName
			}, 0);
		}
		yield return Resources.UnloadUnusedAssets();
		GarbageCollector.CollectIncremental(ulong.MaxValue);
		GC.Collect();
		yield return SceneManager.LoadSceneAsync(sceneName, new LoadSceneParameters
		{
			loadSceneMode = LoadSceneMode.Single,
			localPhysicsMode = LocalPhysicsMode.None
		});
		yield return Resources.UnloadUnusedAssets();
		GarbageCollector.CollectIncremental(ulong.MaxValue);
		GC.Collect();
		try
		{
			onBeforeSpawnObjects?.Invoke();
		}
		catch (Exception exception)
		{
			UnityEngine.Debug.LogException(exception);
		}
		NetworkServer.SpawnObjects();
		NetworkServer.isLoadingScene = false;
	}

	[Server]
	public void SetLoadingStatus(bool isLoading, List<DewPlayer> players = null)
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogWarning("[Server] function 'System.Void DewNetworkManager::SetLoadingStatus(System.Boolean,System.Collections.Generic.List`1<DewPlayer>)' called when server was not active");
			return;
		}
		if (players == null)
		{
			players = DewPlayer.allHumanPlayers;
		}
		foreach (DewPlayer player in players)
		{
			((NetworkConnection)((NetworkBehaviour)player).connectionToClient).Send<SetLoadingStatusMessage>(new SetLoadingStatusMessage
			{
				isLoading = isLoading
			}, 0);
		}
	}

	[Server]
	public void SetWhiteLoadingStatus(bool isLoading, List<DewPlayer> players = null)
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogWarning("[Server] function 'System.Void DewNetworkManager::SetWhiteLoadingStatus(System.Boolean,System.Collections.Generic.List`1<DewPlayer>)' called when server was not active");
			return;
		}
		if (players == null)
		{
			players = DewPlayer.allHumanPlayers;
		}
		foreach (DewPlayer player in players)
		{
			((NetworkConnection)((NetworkBehaviour)player).connectionToClient).Send<SetLoadingStatusMessage>(new SetLoadingStatusMessage
			{
				isLoading = isLoading,
				isWhite = true
			}, 0);
		}
	}

	[Server]
	public void RestartSession()
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogWarning("[Server] function 'System.Void DewNetworkManager::RestartSession()' called when server was not active");
		}
		else if (!isEndingSession)
		{
			isEndingSession = true;
			if ((UnityEngine.Object)(object)NetworkedManagerBase<GameManager>.instance != null && NetworkedManagerBase<GameManager>.instance.IsEligibleForMidRunSave())
			{
				NetworkedManagerBase<GameManager>.instance.SaveContinueDataMidRun();
			}
			NetworkServer.SendToAll<SessionRestartingMessage>(default(SessionRestartingMessage), 0, false);
		}
	}

	private void HandleGameRestartingMessage(SessionRestartingMessage obj)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		RestartAsync();
	}

	[AsyncStateMachine(typeof(_003CRestartAsync_003Ed__53))]
	private UniTaskVoid RestartAsync()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		_003CRestartAsync_003Ed__53 obj = default;
		obj._003C_003Et__builder = AsyncUniTaskVoidMethodBuilder.Create();
		obj._003C_003E4__this = this;
		obj._003C_003E1__state = -1;
		obj._003C_003Et__builder.Start<_003CRestartAsync_003Ed__53>(ref obj);
		return obj._003C_003Et__builder.Task;
	}

	public void SpawnPlayerObject(NetworkConnectionToClient conn)
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		GameObject gameObject = UnityEngine.Object.Instantiate(base.playerPrefab);
		DewPlayer component = gameObject.GetComponent<DewPlayer>();
		if (_authMessagesFromClients.TryGetValue(conn, out var value))
		{
			if (ulong.TryParse(value.userId, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
			{
				component.NetworksteamId = new CSteamID(result);
			}
			if (ManagerBase<LobbyManager>.instance.service is LobbyServiceEOS && !string.IsNullOrEmpty(conn.address) && conn.address != "localhost")
			{
				component.SetEosIdServer(conn.address);
			}
			component.guid = value.profileGuid;
			if (string.IsNullOrEmpty(component.guid))
			{
				component.guid = Guid.NewGuid().ToString();
			}
			component.playerNameRaw = value.profileName.Trim();
			if (!DewProfile.ValidateProfileName(component.playerNameRaw))
			{
				component.playerNameRaw = "Dreamer";
			}
			_authMessagesFromClients.Remove(conn);
		}
		else
		{
			UnityEngine.Debug.LogWarning("Unknown player without auth data: " + conn.address);
		}
		NetworkServer.AddPlayerForConnection(conn, gameObject);
	}

	[AsyncStateMachine(typeof(_003CStartSession_003Ed__60))]
	protected virtual UniTaskVoid StartSession()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		_003CStartSession_003Ed__60 obj = default;
		obj._003C_003Et__builder = AsyncUniTaskVoidMethodBuilder.Create();
		obj._003C_003E4__this = this;
		obj._003C_003E1__state = -1;
		obj._003C_003Et__builder.Start<_003CStartSession_003Ed__60>(ref obj);
		return obj._003C_003Et__builder.Task;
	}
}
