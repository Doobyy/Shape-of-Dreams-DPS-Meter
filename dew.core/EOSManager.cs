using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using Epic.OnlineServices;
using EpicTransport;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EOSManager : ManagerBase<EOSManager>
{
	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CEnsureCleanEOSForJoin_003Ed__18 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder _003C_003Et__builder;

		public EOSManager _003C_003E4__this;

		private Awaiter _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_009b: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
			//IL_0062: Unknown result type (might be due to invalid IL or missing references)
			//IL_0067: Unknown result type (might be due to invalid IL or missing references)
			//IL_006b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0070: Unknown result type (might be due to invalid IL or missing references)
			//IL_0084: Unknown result type (might be due to invalid IL or missing references)
			//IL_0085: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			EOSManager CS_0024_003C_003E8__locals2 = _003C_003E4__this;
			try
			{
				Awaiter val;
				if (num == 0)
				{
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default;
					num = (_003C_003E1__state = -1);
					goto IL_00b6;
				}
				if (DewBuildProfile.current.platform != PlatformType.STEAM || !DewBuildProfile.current.useSteamLobbyAndRelay)
				{
					if (p2pDirty)
					{
						UnityEngine.Debug.Log("P2P state dirty — resetting EOS before join");
						CS_0024_003C_003E8__locals2.ResetEOS();
					}
					UniTask val2 = UniTask.WaitWhile((Func<bool>)(() => CS_0024_003C_003E8__locals2.status == ServiceStatus.Loading), (PlayerLoopTiming)8, default(CancellationToken));
					val = val2.GetAwaiter();
					if (!val.IsCompleted)
					{
						num = (_003C_003E1__state = 0);
						_003C_003Eu__1 = val;
						_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter, _003CEnsureCleanEOSForJoin_003Ed__18>(ref val, ref this);
						return;
					}
					goto IL_00b6;
				}
				goto end_IL_000e;
				IL_00b6:
				val.GetResult();
				end_IL_000e:;
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
	private struct _003CResetEOSAfterLobbyLeaveFlush_003Ed__16 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

		public EOSManager _003C_003E4__this;

		private Awaiter _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_009a: Unknown result type (might be due to invalid IL or missing references)
			//IL_009f: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
			//IL_0112: Unknown result type (might be due to invalid IL or missing references)
			//IL_0117: Unknown result type (might be due to invalid IL or missing references)
			//IL_011e: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00db: Unknown result type (might be due to invalid IL or missing references)
			//IL_00df: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			//IL_0067: Unknown result type (might be due to invalid IL or missing references)
			//IL_006c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0080: Unknown result type (might be due to invalid IL or missing references)
			//IL_0081: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			EOSManager eOSManager = _003C_003E4__this;
			try
			{
				if ((uint)num <= 1u)
				{
					goto IL_0027;
				}
				if (!eOSManager._deferredResetInFlight)
				{
					eOSManager._deferredResetInFlight = true;
					goto IL_0027;
				}
				goto end_IL_000e;
				IL_0027:
				try
				{
					Awaiter val;
					UniTask val2;
					if (num != 0)
					{
						if (num == 1)
						{
							val = _003C_003Eu__1;
							_003C_003Eu__1 = default;
							num = (_003C_003E1__state = -1);
							goto IL_012d;
						}
						LobbyServiceProvider lobbyServiceProvider = ((ManagerBase<LobbyManager>.instance != null) ? ManagerBase<LobbyManager>.instance.service : null);
						if (!(lobbyServiceProvider != null) || lobbyServiceProvider.currentLobby == null)
						{
							goto IL_00bc;
						}
						val2 = lobbyServiceProvider.LeaveLobby();
						val = val2.GetAwaiter();
						if (!val.IsCompleted)
						{
							num = (_003C_003E1__state = 0);
							_003C_003Eu__1 = val;
							_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter, _003CResetEOSAfterLobbyLeaveFlush_003Ed__16>(ref val, ref this);
							return;
						}
					}
					else
					{
						val = _003C_003Eu__1;
						_003C_003Eu__1 = default;
						num = (_003C_003E1__state = -1);
					}
					val.GetResult();
					goto IL_00bc;
					IL_00bc:
					val2 = UniTask.Delay(TimeSpan.FromSeconds(1.5), (DelayType)1, (PlayerLoopTiming)8, default(CancellationToken));
					val = val2.GetAwaiter();
					if (!val.IsCompleted)
					{
						num = (_003C_003E1__state = 1);
						_003C_003Eu__1 = val;
						_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter, _003CResetEOSAfterLobbyLeaveFlush_003Ed__16>(ref val, ref this);
						return;
					}
					goto IL_012d;
					IL_012d:
					val.GetResult();
				}
				finally
				{
					if (num < 0)
					{
						eOSManager._deferredResetInFlight = false;
					}
				}
				if (SceneManager.GetActiveScene().name == "Title" && (ManagerBase<LobbyManager>.instance == null || ManagerBase<LobbyManager>.instance.service.currentLobby == null))
				{
					eOSManager.ResetEOS();
				}
				end_IL_000e:;
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

	public static bool ResetEOSOnTitle = true;

	public static bool ForceUseDeviceIdAccessToken = false;

	public static bool p2pDirty;

	private SteamTicketForWebApi _currentSteamTicket;

	private bool _deferredResetInFlight;

	public override bool shouldRegisterUpdates => false;

	public ServiceStatus status { get; private set; }

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	private static void OnInit()
	{
		ResetEOSOnTitle = true;
		ForceUseDeviceIdAccessToken = false;
		p2pDirty = false;
	}

	private void Start()
	{
		if (DewBuildProfile.current.platform != PlatformType.STEAM || !DewBuildProfile.current.useSteamLobbyAndRelay)
		{
			TryInit();
			SceneManager.activeSceneChanged += SceneManagerOnactiveSceneChanged;
		}
	}

	public async void TryInit()
	{
		if (EOSSDKComponent.Initialized)
		{
			status = ServiceStatus.Ready;
			return;
		}
		status = ServiceStatus.Loading;
		try
		{
			if (DewBuildProfile.current.platform == PlatformType.STEAM && !ForceUseDeviceIdAccessToken)
			{
				await UniTask.WaitWhile((Func<bool>)(() => !DewSteam.isInitialized), (PlayerLoopTiming)8, default(CancellationToken));
				_currentSteamTicket?.Dispose();
				_currentSteamTicket = await DewSteam.GetTicketForWebApi("epiconlineservices");
				EOSSDKComponent.SetConnectInterfaceCredentialToken(_currentSteamTicket.ticket);
				EOSSDKComponent.Instance.connectInterfaceCredentialType = (ExternalCredentialType)18;
			}
			else
			{
				EOSSDKComponent.Instance.connectInterfaceCredentialType = (ExternalCredentialType)10;
			}
			UnityEngine.Debug.Log("Initializing EOS with " + ((object)EOSSDKComponent.Instance.connectInterfaceCredentialType/*cast due to constrained. prefix*/).ToString());
			EOSSDKComponent.Initialize();
			await UniTask.WaitWhile((Func<bool>)(() => (UnityEngine.Object)(object)EOSSDKComponent.Instance != null && (!EOSSDKComponent.Initialized || EOSSDKComponent.IsConnecting)), (PlayerLoopTiming)8, default(CancellationToken));
			if ((UnityEngine.Object)(object)EOSSDKComponent.Instance == null || !EOSSDKComponent.Initialized)
			{
				status = ServiceStatus.Error;
			}
			else
			{
				status = ServiceStatus.Ready;
			}
		}
		catch (Exception exception)
		{
			status = ServiceStatus.Error;
			UnityEngine.Debug.LogException(exception);
		}
	}

	private void OnDestroy()
	{
		SceneManager.activeSceneChanged -= SceneManagerOnactiveSceneChanged;
		_currentSteamTicket?.Dispose();
		_currentSteamTicket = null;
	}

	private void SceneManagerOnactiveSceneChanged(Scene from, Scene to)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		if ((DewBuildProfile.current.platform != PlatformType.STEAM || !DewBuildProfile.current.useSteamLobbyAndRelay) && !(to.name != "Title") && ResetEOSOnTitle)
		{
			UniTaskVoid val = ResetEOSAfterLobbyLeaveFlush();
			val.Forget();
		}
	}

	[AsyncStateMachine(typeof(_003CResetEOSAfterLobbyLeaveFlush_003Ed__16))]
	private UniTaskVoid ResetEOSAfterLobbyLeaveFlush()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		_003CResetEOSAfterLobbyLeaveFlush_003Ed__16 obj = default;
		obj._003C_003Et__builder = AsyncUniTaskVoidMethodBuilder.Create();
		obj._003C_003E4__this = this;
		obj._003C_003E1__state = -1;
		obj._003C_003Et__builder.Start<_003CResetEOSAfterLobbyLeaveFlush_003Ed__16>(ref obj);
		return obj._003C_003Et__builder.Task;
	}

	public void ResetEOS()
	{
		status = ServiceStatus.Loading;
		UnityEngine.Debug.Log("Destroying EOS...");
		if ((UnityEngine.Object)(object)EOSSDKComponent.Instance != null)
		{
			try
			{
				if ((Handle)(object)EOSSDKComponent.Instance.EOS != (Handle)null)
				{
					EOSSDKComponent.Instance.EOS.Release();
				}
			}
			catch (Exception ex)
			{
				UnityEngine.Debug.LogWarning("[EOSManager] EOS.Release threw during ResetEOS: " + ex.Message);
			}
			EOSSDKComponent.Instance.EOS = null;
			UnityEngine.Object.DestroyImmediate(((Component)(object)EOSSDKComponent.Instance).gameObject);
		}
		UnityEngine.Debug.Log("Creating EOS...");
		UnityEngine.Object.Instantiate(Resources.Load<GameObject>("EOSSDKComponent"), ManagerBase<GlobalLogicPackage>.instance.transform);
		p2pDirty = false;
		UnityEngine.Debug.Log("Done. Initializing...");
		TryInit();
	}

	[AsyncStateMachine(typeof(_003CEnsureCleanEOSForJoin_003Ed__18))]
	public UniTask EnsureCleanEOSForJoin()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		_003CEnsureCleanEOSForJoin_003Ed__18 obj = default;
		obj._003C_003Et__builder = AsyncUniTaskMethodBuilder.Create();
		obj._003C_003E4__this = this;
		obj._003C_003E1__state = -1;
		obj._003C_003Et__builder.Start<_003CEnsureCleanEOSForJoin_003Ed__18>(ref obj);
		return obj._003C_003Et__builder.Task;
	}
}
