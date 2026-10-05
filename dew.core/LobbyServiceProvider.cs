using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using Mirror;
using UnityEngine;

public abstract class LobbyServiceProvider : MonoBehaviour
{
	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CJoinGameOfCurrentLobby_003Ed__20 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder _003C_003Et__builder;

		public LobbyServiceProvider _003C_003E4__this;

		private Awaiter _003C_003Eu__1;

		private string _003ChostVersion_003E5__2;

		private void MoveNext()
		{
			//IL_007a: Unknown result type (might be due to invalid IL or missing references)
			//IL_007f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0086: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
			//IL_019f: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
			//IL_0235: Unknown result type (might be due to invalid IL or missing references)
			//IL_023a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0241: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			//IL_0061: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00be: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
			//IL_01fa: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ff: Unknown result type (might be due to invalid IL or missing references)
			//IL_0202: Unknown result type (might be due to invalid IL or missing references)
			//IL_0207: Unknown result type (might be due to invalid IL or missing references)
			//IL_021b: Unknown result type (might be due to invalid IL or missing references)
			//IL_021c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0164: Unknown result type (might be due to invalid IL or missing references)
			//IL_0169: Unknown result type (might be due to invalid IL or missing references)
			//IL_016c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0171: Unknown result type (might be due to invalid IL or missing references)
			//IL_0185: Unknown result type (might be due to invalid IL or missing references)
			//IL_0186: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			LobbyServiceProvider lobbyServiceProvider = _003C_003E4__this;
			try
			{
				Awaiter val;
				switch (num)
				{
				default:
				{
					UniTask val2;
					if (lobbyServiceProvider.currentLobby == null || string.IsNullOrEmpty(lobbyServiceProvider.currentLobby.gameServerAddress))
					{
						val2 = lobbyServiceProvider.LeaveLobby();
						val = val2.GetAwaiter();
						if (!val.IsCompleted)
						{
							num = (_003C_003E1__state = 0);
							_003C_003Eu__1 = val;
							_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter, _003CJoinGameOfCurrentLobby_003Ed__20>(ref val, ref this);
							return;
						}
						goto IL_0095;
					}
					if (!lobbyServiceProvider.currentLobby.allowJoin)
					{
						val2 = lobbyServiceProvider.LeaveLobby();
						val = val2.GetAwaiter();
						if (!val.IsCompleted)
						{
							num = (_003C_003E1__state = 1);
							_003C_003Eu__1 = val;
							_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter, _003CJoinGameOfCurrentLobby_003Ed__20>(ref val, ref this);
							return;
						}
						goto IL_010c;
					}
					if (lobbyServiceProvider.currentLobby.version != Dew.GetCurrentMultiplayerCompatibilityVersion())
					{
						_003ChostVersion_003E5__2 = (string.IsNullOrEmpty(lobbyServiceProvider.currentLobby.version) ? "Unknown" : lobbyServiceProvider.currentLobby.version);
						val2 = lobbyServiceProvider.LeaveLobby();
						val = val2.GetAwaiter();
						if (!val.IsCompleted)
						{
							num = (_003C_003E1__state = 2);
							_003C_003Eu__1 = val;
							_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter, _003CJoinGameOfCurrentLobby_003Ed__20>(ref val, ref this);
							return;
						}
						goto IL_01ba;
					}
					if (lobbyServiceProvider.currentLobby.crossPlayGate != LobbyServiceEOS.GetCrossPlayGate())
					{
						val2 = lobbyServiceProvider.LeaveLobby();
						val = val2.GetAwaiter();
						if (!val.IsCompleted)
						{
							num = (_003C_003E1__state = 3);
							_003C_003Eu__1 = val;
							_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter, _003CJoinGameOfCurrentLobby_003Ed__20>(ref val, ref this);
							return;
						}
						goto IL_0250;
					}
					ManagerBase<TransitionManager>.instance.UpdateLoadingStatus(LoadingStatus.ConnectingToGame);
					((NetworkManager)DewNetworkManager.instance).networkAddress = lobbyServiceProvider.currentLobby.gameServerAddress;
					((NetworkManager)DewNetworkManager.instance).StartClient();
					break;
				}
				case 0:
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default;
					num = (_003C_003E1__state = -1);
					goto IL_0095;
				case 1:
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default;
					num = (_003C_003E1__state = -1);
					goto IL_010c;
				case 2:
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default;
					num = (_003C_003E1__state = -1);
					goto IL_01ba;
				case 3:
					{
						val = _003C_003Eu__1;
						_003C_003Eu__1 = default;
						num = (_003C_003E1__state = -1);
						goto IL_0250;
					}
					IL_0095:
					val.GetResult();
					throw new DewException(DewExceptionType.UnknownLobby);
					IL_0250:
					val.GetResult();
					throw new DewException(DewExceptionType.CrossPlayMismatch);
					IL_010c:
					val.GetResult();
					throw new DewException(DewExceptionType.GameAlreadyStarted);
					IL_01ba:
					val.GetResult();
					throw new DewException(DewExceptionType.VersionMismatch, "Host " + _003ChostVersion_003E5__2 + " != Local " + Dew.GetCurrentMultiplayerCompatibilityVersion());
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

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CRefreshLobbies_003Ed__14 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

		public LobbyServiceProvider _003C_003E4__this;

		public object continuationToken;

		private Awaiter _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_0074: Unknown result type (might be due to invalid IL or missing references)
			//IL_0079: Unknown result type (might be due to invalid IL or missing references)
			//IL_0080: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			LobbyServiceProvider lobbyServiceProvider = _003C_003E4__this;
			try
			{
				if (num == 0)
				{
					goto IL_0026;
				}
				if (!lobbyServiceProvider.isRefreshingLobby)
				{
					lobbyServiceProvider.isRefreshingLobby = true;
					goto IL_0026;
				}
				goto end_IL_000e;
				IL_0026:
				try
				{
					Awaiter val;
					if (num != 0)
					{
						UniTask lobbies = lobbyServiceProvider.GetLobbies(lobbyServiceProvider._003CRefreshLobbies_003Eg__Notify_007C14_0, continuationToken);
						val = lobbies.GetAwaiter();
						if (!val.IsCompleted)
						{
							num = (_003C_003E1__state = 0);
							_003C_003Eu__1 = val;
							_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter, _003CRefreshLobbies_003Ed__14>(ref val, ref this);
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
				}
				catch (Exception e)
				{
					DewSessionError.ShowError(e);
				}
				lobbyServiceProvider.isRefreshingLobby = false;
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
	private struct _003CSetBasicAttributes_003Ed__17 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder _003C_003Et__builder;

		public LobbyServiceProvider _003C_003E4__this;

		private Awaiter _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_0073: Unknown result type (might be due to invalid IL or missing references)
			//IL_0078: Unknown result type (might be due to invalid IL or missing references)
			//IL_007f: Unknown result type (might be due to invalid IL or missing references)
			//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
			//IL_0152: Unknown result type (might be due to invalid IL or missing references)
			//IL_0157: Unknown result type (might be due to invalid IL or missing references)
			//IL_015e: Unknown result type (might be due to invalid IL or missing references)
			//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
			//IL_022a: Unknown result type (might be due to invalid IL or missing references)
			//IL_022f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0236: Unknown result type (might be due to invalid IL or missing references)
			//IL_0290: Unknown result type (might be due to invalid IL or missing references)
			//IL_0295: Unknown result type (might be due to invalid IL or missing references)
			//IL_029c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
			//IL_0117: Unknown result type (might be due to invalid IL or missing references)
			//IL_011c: Unknown result type (might be due to invalid IL or missing references)
			//IL_011f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0124: Unknown result type (might be due to invalid IL or missing references)
			//IL_0181: Unknown result type (might be due to invalid IL or missing references)
			//IL_0186: Unknown result type (might be due to invalid IL or missing references)
			//IL_0189: Unknown result type (might be due to invalid IL or missing references)
			//IL_018e: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ef: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f4: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f7: Unknown result type (might be due to invalid IL or missing references)
			//IL_01fc: Unknown result type (might be due to invalid IL or missing references)
			//IL_0258: Unknown result type (might be due to invalid IL or missing references)
			//IL_025d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0260: Unknown result type (might be due to invalid IL or missing references)
			//IL_0265: Unknown result type (might be due to invalid IL or missing references)
			//IL_0059: Unknown result type (might be due to invalid IL or missing references)
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
			//IL_0138: Unknown result type (might be due to invalid IL or missing references)
			//IL_0139: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
			//IL_0210: Unknown result type (might be due to invalid IL or missing references)
			//IL_0211: Unknown result type (might be due to invalid IL or missing references)
			//IL_0279: Unknown result type (might be due to invalid IL or missing references)
			//IL_027a: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			LobbyServiceProvider lobbyServiceProvider = _003C_003E4__this;
			try
			{
				UniTask val2;
				Awaiter val;
				switch (num)
				{
				default:
					val2 = lobbyServiceProvider.SetLobbyAttribute("version", Dew.GetCurrentMultiplayerCompatibilityVersion());
					val = val2.GetAwaiter();
					if (!val.IsCompleted)
					{
						num = (_003C_003E1__state = 0);
						_003C_003Eu__1 = val;
						_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter, _003CSetBasicAttributes_003Ed__17>(ref val, ref this);
						return;
					}
					goto IL_008e;
				case 0:
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default;
					num = (_003C_003E1__state = -1);
					goto IL_008e;
				case 1:
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default;
					num = (_003C_003E1__state = -1);
					goto IL_00f7;
				case 2:
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default;
					num = (_003C_003E1__state = -1);
					goto IL_016d;
				case 3:
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default;
					num = (_003C_003E1__state = -1);
					goto IL_01d7;
				case 4:
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default;
					num = (_003C_003E1__state = -1);
					goto IL_0245;
				case 5:
					{
						val = _003C_003Eu__1;
						_003C_003Eu__1 = default;
						num = (_003C_003E1__state = -1);
						break;
					}
					IL_016d:
					val.GetResult();
					val2 = lobbyServiceProvider.SetLobbyAttribute("shortCode", lobbyServiceProvider.GenerateRandomShortCode());
					val = val2.GetAwaiter();
					if (!val.IsCompleted)
					{
						num = (_003C_003E1__state = 3);
						_003C_003Eu__1 = val;
						_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter, _003CSetBasicAttributes_003Ed__17>(ref val, ref this);
						return;
					}
					goto IL_01d7;
					IL_008e:
					val.GetResult();
					val2 = lobbyServiceProvider.SetLobbyAttribute("crossPlayGate", LobbyServiceEOS.GetCrossPlayGate());
					val = val2.GetAwaiter();
					if (!val.IsCompleted)
					{
						num = (_003C_003E1__state = 1);
						_003C_003Eu__1 = val;
						_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter, _003CSetBasicAttributes_003Ed__17>(ref val, ref this);
						return;
					}
					goto IL_00f7;
					IL_0245:
					val.GetResult();
					val2 = lobbyServiceProvider.SetLobbyAttribute("customData", new Dictionary<string, string>());
					val = val2.GetAwaiter();
					if (!val.IsCompleted)
					{
						num = (_003C_003E1__state = 5);
						_003C_003Eu__1 = val;
						_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter, _003CSetBasicAttributes_003Ed__17>(ref val, ref this);
						return;
					}
					break;
					IL_00f7:
					val.GetResult();
					val2 = lobbyServiceProvider.SetLobbyAttribute("isInviteOnly", DewNetworkManager.startSettings.lobbyType == DewLobbyType.InviteOnly);
					val = val2.GetAwaiter();
					if (!val.IsCompleted)
					{
						num = (_003C_003E1__state = 2);
						_003C_003Eu__1 = val;
						_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter, _003CSetBasicAttributes_003Ed__17>(ref val, ref this);
						return;
					}
					goto IL_016d;
					IL_01d7:
					val.GetResult();
					val2 = lobbyServiceProvider.SetLobbyAttribute("isModded", DewMod.isGameplayAltered);
					val = val2.GetAwaiter();
					if (!val.IsCompleted)
					{
						num = (_003C_003E1__state = 4);
						_003C_003Eu__1 = val;
						_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter, _003CSetBasicAttributes_003Ed__17>(ref val, ref this);
						return;
					}
					goto IL_0245;
				}
				val.GetResult();
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

	public bool isRefreshingLobby { get; private set; }

	public abstract LobbyInstance currentLobby { get; }

	public LobbySearchResult foundLobbies { get; set; } = new LobbySearchResult
	{
		continuationToken = null,
		lobbies = new List<LobbyInstance>()
	};

	public abstract UniTask CreateLobby();

	public abstract UniTask JoinLobby(object lobby);

	public abstract UniTask LeaveLobby();

	public abstract UniTask HandleUserLeavingGame(string id);

	[AsyncStateMachine(typeof(_003CRefreshLobbies_003Ed__14))]
	public UniTaskVoid RefreshLobbies(object continuationToken = null)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		_003CRefreshLobbies_003Ed__14 obj = default;
		obj._003C_003Et__builder = AsyncUniTaskVoidMethodBuilder.Create();
		obj._003C_003E4__this = this;
		obj.continuationToken = continuationToken;
		obj._003C_003E1__state = -1;
		obj._003C_003Et__builder.Start<_003CRefreshLobbies_003Ed__14>(ref obj);
		return obj._003C_003Et__builder.Task;
	}

	protected void InvokeOnCurrentLobbyChanged()
	{
		try
		{
			ManagerBase<LobbyManager>.instance.onCurrentLobbyChanged?.Invoke();
		}
		catch (Exception exception)
		{
			UnityEngine.Debug.LogException(exception);
		}
	}

	public abstract UniTask GetLobbies(Action<LobbySearchResult> onUpdated, object continuationToken = null);

	[AsyncStateMachine(typeof(_003CSetBasicAttributes_003Ed__17))]
	public UniTask SetBasicAttributes()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		_003CSetBasicAttributes_003Ed__17 obj = default;
		obj._003C_003Et__builder = AsyncUniTaskMethodBuilder.Create();
		obj._003C_003E4__this = this;
		obj._003C_003E1__state = -1;
		obj._003C_003Et__builder.Start<_003CSetBasicAttributes_003Ed__17>(ref obj);
		return obj._003C_003Et__builder.Task;
	}

	public string GenerateRandomShortCode()
	{
		char[] array = new char[8];
		for (int i = 0; i < 8; i++)
		{
			int index = UnityEngine.Random.Range(0, "ABCDEFGHJKLMNPQRSTUVWXYZ23456789".Length);
			array[i] = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"[index];
		}
		return new string(array);
	}

	public abstract string GetGameServerAddress();

	[AsyncStateMachine(typeof(_003CJoinGameOfCurrentLobby_003Ed__20))]
	public virtual UniTask JoinGameOfCurrentLobby()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		_003CJoinGameOfCurrentLobby_003Ed__20 obj = default;
		obj._003C_003Et__builder = AsyncUniTaskMethodBuilder.Create();
		obj._003C_003E4__this = this;
		obj._003C_003E1__state = -1;
		obj._003C_003Et__builder.Start<_003CJoinGameOfCurrentLobby_003Ed__20>(ref obj);
		return obj._003C_003Et__builder.Task;
	}

	public abstract UniTask SetLobbyAttribute(string key, object value, bool isPublic = true);

	public abstract UniTask SetLobbyMemberAttribute(string key, object value, bool isPublic = true);

	public virtual UniTask RefreshCurrentLobbyFromBackend()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		return UniTask.CompletedTask;
	}
}
