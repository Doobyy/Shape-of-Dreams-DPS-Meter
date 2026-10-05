using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using AOT;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using Steamworks;
using UnityEngine;

public static class DewSteam
{
	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CGetTicketForWebApi_003Ed__26 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder<SteamTicketForWebApi> _003C_003Et__builder;

		public string pchIdentity;

		private HAuthTicket _003ChAuthTicket_003E5__2;

		private Awaiter<GetTicketForWebApiResponse_t> _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_00da: Unknown result type (might be due to invalid IL or missing references)
			//IL_00df: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
			//IL_0105: Invalid comparison between Unknown and I4
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_010a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0086: Unknown result type (might be due to invalid IL or missing references)
			//IL_009c: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
			//IL_0119: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
			//IL_0145: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			SteamTicketForWebApi result2;
			try
			{
				try
				{
					Awaiter<GetTicketForWebApiResponse_t> val2;
					if (num != 0)
					{
						if (!isInitialized)
						{
							throw new DewException(DewExceptionType.SteamNotAvailable);
						}
						_003ChAuthTicket_003E5__2 = SteamUser.GetAuthTicketForWebApi(pchIdentity);
						if (_003ChAuthTicket_003E5__2.m_HAuthTicket == 0)
						{
							throw new DewException(DewExceptionType.SteamAuthGetTicketFailed);
						}
						UniTaskCompletionSource<GetTicketForWebApiResponse_t> val = new UniTaskCompletionSource<GetTicketForWebApiResponse_t>();
						_getTicketForWebApis.Add(_003ChAuthTicket_003E5__2.m_HAuthTicket, val);
						UnityEngine.Debug.Log("GetTicketForWebApi(" + pchIdentity + "): Getting ticket");
						val2 = UniTaskExtensions.Timeout<GetTicketForWebApiResponse_t>(val.Task, TimeSpan.FromSeconds(8.0), (DelayType)0, (PlayerLoopTiming)8, (CancellationTokenSource)null).GetAwaiter();
						if (!val2.IsCompleted)
						{
							num = (_003C_003E1__state = 0);
							_003C_003Eu__1 = val2;
							_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter<GetTicketForWebApiResponse_t>, _003CGetTicketForWebApi_003Ed__26>(ref val2, ref this);
							return;
						}
					}
					else
					{
						val2 = _003C_003Eu__1;
						_003C_003Eu__1 = default;
						num = (_003C_003E1__state = -1);
					}
					GetTicketForWebApiResponse_t result = val2.GetResult();
					if ((int)result.m_eResult != 1)
					{
						throw new DewException(DewExceptionType.SteamAuthGetTicketFailed, ((object)result.m_eResult/*cast due to constrained. prefix*/).ToString());
					}
					int cubTicket = result.m_cubTicket;
					if (cubTicket <= 0 || cubTicket > result.m_rgubTicket.Length)
					{
						throw new DewException(DewExceptionType.SteamAuthGetTicketFailed, $"Bad ticket length {cubTicket}");
					}
					byte[] array = new byte[cubTicket];
					Array.Copy(result.m_rgubTicket, 0, array, 0, cubTicket);
					string text = BitConverter.ToString(array).Replace("-", "");
					UnityEngine.Debug.Log(string.Format("{0}({1}): Got ticket, bytes={2}, hexLen={3}", "GetTicketForWebApi", pchIdentity, cubTicket, text.Length));
					result2 = new SteamTicketForWebApi
					{
						handle = _003ChAuthTicket_003E5__2,
						ticket = text
					};
				}
				catch (TimeoutException)
				{
					throw new DewException(DewExceptionType.SteamAuthGetTicketFailed);
				}
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				_003C_003Et__builder.SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			_003C_003Et__builder.SetResult(result2);
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

	public static SafeAction<bool> onGameOverlayShownChanged;

	private static Callback<GameOverlayActivated_t> _onGameOverlayShownChanged;

	private static Callback<GetTicketForWebApiResponse_t> _onGetTicketForWebApiResponse;

	private static Callback<GamepadTextInputDismissed_t> _onGamepadTextInputDismissed;

	private static Dictionary<uint, UniTaskCompletionSource<GetTicketForWebApiResponse_t>> _getTicketForWebApis = new Dictionary<uint, UniTaskCompletionSource<GetTicketForWebApiResponse_t>>();

	private static SteamAPIWarningMessageHook_t m_SteamAPIWarningMessageHook;

	private static Action<string> _currentGamepadInputConfirmAction;

	private static Action _currentGamepadInputCancelAction;

	public static bool isInitialized { get; private set; }

	public static bool isAchievementReady { get; private set; }

	public static CSteamID steamId
	{
		[CompilerGenerated]
		get
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			return field;
		}
		[CompilerGenerated]
		private set
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			field = value;
		}
	}

	public static List<string> installedDLCs { get; set; }

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	private static void OnInit()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Expected Obj, but got Unknown
		onGameOverlayShownChanged = null;
		isInitialized = false;
		isAchievementReady = false;
		CSteamID val = (steamId = default);
		_onGameOverlayShownChanged = null;
		_onGetTicketForWebApiResponse = null;
		_onGamepadTextInputDismissed = null;
		installedDLCs = null;
		if (DewBuildProfile.current.platform != PlatformType.STEAM)
		{
			return;
		}
		Application.quitting += OnQuit;
		if (!Packsize.Test())
		{
			UnityEngine.Debug.LogError("[Steamworks.NET] Packsize Test returned false, the wrong version of Steamworks.NET is being run in this platform.");
		}
		if (!DllCheck.Test())
		{
			UnityEngine.Debug.LogError("[Steamworks.NET] DllCheck Test returned false, One or more of the Steamworks binaries seems to be the wrong version.");
		}
		if (!Environment.GetCommandLineArgs().ToList().Contains("-steamnorestart"))
		{
			try
			{
				if (SteamAPI.RestartAppIfNecessary((AppId_t)2444750u))
				{
					UnityEngine.Debug.Log("Restarting App");
					Application.Quit();
					return;
				}
			}
			catch (DllNotFoundException ex)
			{
				UnityEngine.Debug.LogError("[Steamworks.NET] Could not load [lib]steam_api.dll/so/dylib. It's likely not in the correct location. Refer to the README for more details.\n" + ex);
				Application.Quit();
				return;
			}
		}
		isInitialized = SteamAPI.Init();
		if (!isInitialized)
		{
			UnityEngine.Debug.LogError("[Steamworks.NET] SteamAPI_Init() failed. Refer to Valve's documentation or the comment above this line for more information.");
			return;
		}
		steamId = SteamUser.GetSteamID();
		val = steamId;
		if (val.IsValid())
		{
			CSteamID val3 = steamId;
			val = default;
			if (!(val3 == val))
			{
				UnityEngine.Debug.Log("[Steamworks.NET] Successfully initialized Steam API.");
				_onGetTicketForWebApiResponse = Callback<GetTicketForWebApiResponse_t>.Create((DispatchDelegate<GetTicketForWebApiResponse_t>)OnGetTicketForWebApiResponse_t);
				_onGameOverlayShownChanged = Callback<GameOverlayActivated_t>.Create((DispatchDelegate<GameOverlayActivated_t>)OnGameOverlayActivated);
				_onGamepadTextInputDismissed = Callback<GamepadTextInputDismissed_t>.Create((DispatchDelegate<GamepadTextInputDismissed_t>)OnGamepadTextInputDismissed);
				isAchievementReady = SteamUserStats.RequestCurrentStats();
				if (!isAchievementReady)
				{
					UnityEngine.Debug.LogError("[Steamworks.NET] Achievement is not ready.");
				}
				m_SteamAPIWarningMessageHook = SteamAPIDebugTextHook;
				SteamClient.SetWarningMessageHook(m_SteamAPIWarningMessageHook);
				UpdateInstalledDLCs();
				return;
			}
		}
		UnityEngine.Debug.LogError("[Steamworks.NET] Invalid user SteamID.");
		isInitialized = false;
	}

	private static void UpdateInstalledDLCs()
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		installedDLCs = new List<string>();
		int dLCCount = SteamApps.GetDLCCount();
		AppId_t val = default;
		bool flag = default;
		string text = default;
		for (int i = 0; i < dLCCount; i++)
		{
			if (SteamApps.BGetDLCDataByIndex(i, ref val, ref flag, ref text, 128) && SteamApps.BIsDlcInstalled(val))
			{
				installedDLCs.Add(((object)val/*cast due to constrained. prefix*/).ToString());
			}
		}
		UnityEngine.Debug.Log(string.Format("Installed DLCS({0}/{1}): {2}", installedDLCs.Count, dLCCount, installedDLCs.JoinToString(", ")));
	}

	private static void OnQuit()
	{
		Application.quitting -= OnQuit;
		if (isInitialized)
		{
			SteamAPI.Shutdown();
			isInitialized = false;
		}
	}

	[MonoPInvokeCallback(typeof(SteamAPIWarningMessageHook_t))]
	private static void SteamAPIDebugTextHook(int nSeverity, StringBuilder pchDebugText)
	{
		UnityEngine.Debug.LogWarning(pchDebugText);
	}

	[AsyncStateMachine(typeof(_003CGetTicketForWebApi_003Ed__26))]
	public static UniTask<SteamTicketForWebApi> GetTicketForWebApi(string pchIdentity = "DewOwnership")
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		_003CGetTicketForWebApi_003Ed__26 obj = default;
		obj._003C_003Et__builder = AsyncUniTaskMethodBuilder<SteamTicketForWebApi>.Create();
		obj.pchIdentity = pchIdentity;
		obj._003C_003E1__state = -1;
		obj._003C_003Et__builder.Start<_003CGetTicketForWebApi_003Ed__26>(ref obj);
		return obj._003C_003Et__builder.Task;
	}

	private static void OnGetTicketForWebApiResponse_t(GetTicketForWebApiResponse_t param)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Invalid comparison between Unknown and I4
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		if (_getTicketForWebApis.TryGetValue(param.m_hAuthTicket.m_HAuthTicket, out var value))
		{
			_getTicketForWebApis.Remove(param.m_hAuthTicket.m_HAuthTicket);
			if ((int)param.m_eResult == 1)
			{
				value.TrySetResult(param);
			}
			else
			{
				value.TrySetException((Exception)new SteamException("Could not get ticket for web api", param.m_eResult));
			}
		}
	}

	private static void OnGameOverlayActivated(GameOverlayActivated_t param)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		onGameOverlayShownChanged?.Invoke(param.m_bActive != 0);
	}

	public static void UnsubscribeItem()
	{
	}

	public static UniTask EnsureReady()
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		return UniTaskExtensions.Timeout(UniTask.WaitUntil((Func<bool>)(() => isInitialized), (PlayerLoopTiming)8, default(CancellationToken)), TimeSpan.FromSeconds(5.0), (DelayType)1, (PlayerLoopTiming)8, (CancellationTokenSource)null);
	}

	private static void OnGamepadTextInputDismissed(GamepadTextInputDismissed_t param)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			string obj = default;
			if (param.m_bSubmitted && SteamUtils.GetEnteredGamepadTextInput(ref obj, param.m_unSubmittedText))
			{
				_currentGamepadInputConfirmAction?.Invoke(obj);
			}
			else
			{
				_currentGamepadInputCancelAction?.Invoke();
			}
		}
		catch (Exception exception)
		{
			UnityEngine.Debug.LogException(exception);
		}
		_currentGamepadInputConfirmAction = null;
		_currentGamepadInputCancelAction = null;
	}

	public static bool TryStartGamepadInput(string previousText, string placeholderText, bool isMultiline, bool isPassword, int maxCharacters, Action<string> onConfirm, Action onCancel)
	{
		if (!isInitialized)
		{
			return false;
		}
		if (!SteamUtils.ShowGamepadTextInput((EGamepadTextInputMode)(isPassword ? 1 : 0), (EGamepadTextInputLineMode)(isMultiline ? 1 : 0), placeholderText, (uint)maxCharacters, previousText))
		{
			UnityEngine.Debug.Log("Steam Gamepad Text Input not available");
			return false;
		}
		try
		{
			_currentGamepadInputCancelAction?.Invoke();
		}
		catch (Exception exception)
		{
			UnityEngine.Debug.LogException(exception);
		}
		_currentGamepadInputConfirmAction = onConfirm;
		_currentGamepadInputCancelAction = onCancel;
		ManagerBase<TransitionManager>.instance.onStateChanged += new Action(DismissDialog);
		_currentGamepadInputConfirmAction = (Action<string>)Delegate.Combine(_currentGamepadInputConfirmAction, (Action<string>)((string _) =>
		{
			if (!(ManagerBase<TransitionManager>.instance == null))
			{
				ManagerBase<TransitionManager>.instance.onStateChanged -= new Action(DismissDialog);
			}
		}));
		_currentGamepadInputCancelAction = (Action)Delegate.Combine(_currentGamepadInputCancelAction, (Action)(() =>
		{
			if (!(ManagerBase<TransitionManager>.instance == null))
			{
				ManagerBase<TransitionManager>.instance.onStateChanged -= new Action(DismissDialog);
			}
		}));
		return true;
	}

	private static void DismissDialog()
	{
		if (ManagerBase<TransitionManager>.instance.state == TransitionManager.StateType.Loading)
		{
			SteamUtils.DismissGamepadTextInput();
		}
	}
}
