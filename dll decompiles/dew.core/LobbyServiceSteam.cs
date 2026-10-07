using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using Steamworks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyServiceSteam : LobbyServiceProvider
{
	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass10_0
	{
		public LobbyServiceSteam _003C_003E4__this;

		public LobbySearchResult result;
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass6_0
	{
		public UniTaskCompletionSource<LobbyCreated_t> source;

		internal void _003CCreateLobby_003Eb__0(LobbyCreated_t t, bool failure)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Invalid comparison between Unknown and I4
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			if ((int)t.m_eResult != 1)
			{
				source.TrySetException((Exception)new SteamException("CreateLobby", t.m_eResult));
			}
			else if (failure)
			{
				source.TrySetException((Exception)new SteamException("CreateLobby"));
			}
			else
			{
				source.TrySetResult(t);
			}
		}
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass7_0
	{
		public LobbyServiceSteam _003C_003E4__this;

		public UniTaskCompletionSource<LobbyEnter_t> source;

		internal bool _003CJoinLobby_003Eb__1()
		{
			return _003C_003E4__this.isRefreshingLobby;
		}

		internal void _003CJoinLobby_003Eb__0(LobbyEnter_t t, bool failure)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			if (t.m_EChatRoomEnterResponse != 1)
			{
				source.TrySetException((Exception)new SteamException("JoinLobby", (object)(EChatRoomEnterResponse)t.m_EChatRoomEnterResponse));
			}
			else if (failure)
			{
				source.TrySetException((Exception)new SteamException("JoinLobby"));
			}
			source.TrySetResult(t);
		}
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass7_1
	{
		public UniTaskCompletionSource<LobbyMatchList_t> s;

		internal void _003CJoinLobby_003Eb__2(LobbyMatchList_t t, bool failure)
		{
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			if (failure)
			{
				s.TrySetException((Exception)new SteamException("GetLobbies"));
			}
			else
			{
				s.TrySetResult(t);
			}
		}
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CCheckLaunchConnectString_003Ed__33 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

		public LobbyServiceSteam _003C_003E4__this;

		private string _003Caddress_003E5__2;

		private Awaiter _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_0138: Unknown result type (might be due to invalid IL or missing references)
			//IL_013d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0144: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
			//IL_0104: Unknown result type (might be due to invalid IL or missing references)
			//IL_0108: Unknown result type (might be due to invalid IL or missing references)
			//IL_010d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0121: Unknown result type (might be due to invalid IL or missing references)
			//IL_0122: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			LobbyServiceSteam lobbyServiceSteam = _003C_003E4__this;
			try
			{
				Awaiter val;
				if (num != 0 && num == 1)
				{
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default;
					num = (_003C_003E1__state = -1);
					goto IL_0153;
				}
				UniTask val2;
				try
				{
					if (num != 0)
					{
						val2 = DewSteam.EnsureReady();
						val = val2.GetAwaiter();
						if (!val.IsCompleted)
						{
							num = (_003C_003E1__state = 0);
							_003C_003Eu__1 = val;
							_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter, _003CCheckLaunchConnectString_003Ed__33>(ref val, ref this);
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
				catch
				{
					goto end_IL_000e;
				}
				string commandLine = default;
				SteamApps.GetLaunchCommandLine(ref commandLine, 1024);
				_003Caddress_003E5__2 = ExtractLaunchAddress(commandLine);
				if (_003Caddress_003E5__2 == null)
				{
					string[] commandLineArgs = Environment.GetCommandLineArgs();
					_003Caddress_003E5__2 = ExtractLaunchAddress(string.Join(" ", commandLineArgs, 1, commandLineArgs.Length - 1));
				}
				if (_003Caddress_003E5__2 != null)
				{
					val2 = UniTask.WaitUntil((Func<bool>)(() => DewSave.profileMainPath != null && SceneManager.GetActiveScene().name == "Title"), (PlayerLoopTiming)8, default(CancellationToken));
					val = val2.GetAwaiter();
					if (!val.IsCompleted)
					{
						num = (_003C_003E1__state = 1);
						_003C_003Eu__1 = val;
						_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter, _003CCheckLaunchConnectString_003Ed__33>(ref val, ref this);
						return;
					}
					goto IL_0153;
				}
				goto end_IL_000e;
				IL_0153:
				val.GetResult();
				lobbyServiceSteam.HandleExternalJoinRequest(_003Caddress_003E5__2);
				end_IL_000e:;
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				_003Caddress_003E5__2 = null;
				_003C_003Et__builder.SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			_003Caddress_003E5__2 = null;
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
	private struct _003CCreateLobby_003Ed__6 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder _003C_003Et__builder;

		public LobbyServiceSteam _003C_003E4__this;

		private _003C_003Ec__DisplayClass6_0 _003C_003E8__1;

		private LobbyCreated_t _003Cresult_003E5__2;

		private Awaiter _003C_003Eu__1;

		private Awaiter<LobbyCreated_t> _003C_003Eu__2;

		private void MoveNext()
		{
			//IL_006f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0074: Unknown result type (might be due to invalid IL or missing references)
			//IL_007b: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00db: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
			//IL_0195: Unknown result type (might be due to invalid IL or missing references)
			//IL_019a: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
			//IL_022c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0231: Unknown result type (might be due to invalid IL or missing references)
			//IL_0238: Unknown result type (might be due to invalid IL or missing references)
			//IL_029c: Unknown result type (might be due to invalid IL or missing references)
			//IL_02a1: Unknown result type (might be due to invalid IL or missing references)
			//IL_02a8: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
			//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
			//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e5: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f5: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f9: Unknown result type (might be due to invalid IL or missing references)
			//IL_01fe: Unknown result type (might be due to invalid IL or missing references)
			//IL_0055: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			//IL_011b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0141: Unknown result type (might be due to invalid IL or missing references)
			//IL_0157: Unknown result type (might be due to invalid IL or missing references)
			//IL_015c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0160: Unknown result type (might be due to invalid IL or missing references)
			//IL_0165: Unknown result type (might be due to invalid IL or missing references)
			//IL_009a: Unknown result type (might be due to invalid IL or missing references)
			//IL_009f: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
			//IL_0212: Unknown result type (might be due to invalid IL or missing references)
			//IL_0213: Unknown result type (might be due to invalid IL or missing references)
			//IL_02ca: Unknown result type (might be due to invalid IL or missing references)
			//IL_02e1: Unknown result type (might be due to invalid IL or missing references)
			//IL_0260: Unknown result type (might be due to invalid IL or missing references)
			//IL_0265: Unknown result type (might be due to invalid IL or missing references)
			//IL_0269: Unknown result type (might be due to invalid IL or missing references)
			//IL_026e: Unknown result type (might be due to invalid IL or missing references)
			//IL_017a: Unknown result type (might be due to invalid IL or missing references)
			//IL_017c: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
			//IL_0282: Unknown result type (might be due to invalid IL or missing references)
			//IL_0283: Unknown result type (might be due to invalid IL or missing references)
			//IL_02f8: Unknown result type (might be due to invalid IL or missing references)
			//IL_02ff: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			LobbyServiceSteam lobbyServiceSteam = _003C_003E4__this;
			try
			{
				UniTask val3;
				Awaiter val;
				Awaiter<LobbyCreated_t> val2;
				LobbyCreated_t result;
				switch (num)
				{
				default:
					_003C_003E8__1 = new _003C_003Ec__DisplayClass6_0();
					val3 = DewSteam.EnsureReady();
					val = val3.GetAwaiter();
					if (!val.IsCompleted)
					{
						num = (_003C_003E1__state = 0);
						_003C_003Eu__1 = val;
						_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter, _003CCreateLobby_003Ed__6>(ref val, ref this);
						return;
					}
					goto IL_008a;
				case 0:
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default;
					num = (_003C_003E1__state = -1);
					goto IL_008a;
				case 1:
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default;
					num = (_003C_003E1__state = -1);
					goto IL_00f1;
				case 2:
					val2 = _003C_003Eu__2;
					_003C_003Eu__2 = default;
					num = (_003C_003E1__state = -1);
					goto IL_01b1;
				case 3:
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default;
					num = (_003C_003E1__state = -1);
					goto IL_0247;
				case 4:
					{
						val = _003C_003Eu__1;
						_003C_003Eu__1 = default;
						num = (_003C_003E1__state = -1);
						goto IL_02b7;
					}
					IL_0247:
					val.GetResult();
					if (!((UnityEngine.Object)(object)NetworkedManagerBase<GameSettingsManager>.instance != null))
					{
						break;
					}
					val3 = NetworkedManagerBase<GameSettingsManager>.instance.Lobby_UpdateGameAttributes();
					val = val3.GetAwaiter();
					if (!val.IsCompleted)
					{
						num = (_003C_003E1__state = 4);
						_003C_003Eu__1 = val;
						_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter, _003CCreateLobby_003Ed__6>(ref val, ref this);
						return;
					}
					goto IL_02b7;
					IL_008a:
					val.GetResult();
					if (lobbyServiceSteam._currentLobby != null)
					{
						val3 = lobbyServiceSteam.LeaveLobby();
						val = val3.GetAwaiter();
						if (!val.IsCompleted)
						{
							num = (_003C_003E1__state = 1);
							_003C_003Eu__1 = val;
							_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter, _003CCreateLobby_003Ed__6>(ref val, ref this);
							return;
						}
						goto IL_00f1;
					}
					goto IL_00f8;
					IL_01b1:
					result = val2.GetResult();
					_003Cresult_003E5__2 = result;
					lobbyServiceSteam._currentLobby = new LobbyInstanceSteam
					{
						isLobbyLeader = true
					};
					lobbyServiceSteam._currentLobby.SetCSteamId((CSteamID)_003Cresult_003E5__2.m_ulSteamIDLobby);
					val3 = lobbyServiceSteam.SetBasicAttributes();
					val = val3.GetAwaiter();
					if (!val.IsCompleted)
					{
						num = (_003C_003E1__state = 3);
						_003C_003Eu__1 = val;
						_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter, _003CCreateLobby_003Ed__6>(ref val, ref this);
						return;
					}
					goto IL_0247;
					IL_02b7:
					val.GetResult();
					break;
					IL_00f1:
					val.GetResult();
					goto IL_00f8;
					IL_00f8:
					ManagerBase<TransitionManager>.instance.UpdateLoadingStatus(LoadingStatus.CreatingLobby);
					_003C_003E8__1.source = new UniTaskCompletionSource<LobbyCreated_t>();
					lobbyServiceSteam._lobbyCreated.Set(SteamMatchmaking.CreateLobby((ELobbyType)2, 1), (APIDispatchDelegate<LobbyCreated_t>)((LobbyCreated_t t, bool failure) =>
					{
						//IL_0000: Unknown result type (might be due to invalid IL or missing references)
						//IL_0001: Unknown result type (might be due to invalid IL or missing references)
						//IL_0007: Invalid comparison between Unknown and I4
						//IL_0014: Unknown result type (might be due to invalid IL or missing references)
						//IL_0015: Unknown result type (might be due to invalid IL or missing references)
						//IL_004c: Unknown result type (might be due to invalid IL or missing references)
						if ((int)t.m_eResult != 1)
						{
							_003C_003E8__1.source.TrySetException((Exception)new SteamException("CreateLobby", t.m_eResult));
						}
						else if (failure)
						{
							_003C_003E8__1.source.TrySetException((Exception)new SteamException("CreateLobby"));
						}
						else
						{
							_003C_003E8__1.source.TrySetResult(t);
						}
					}));
					val2 = UniTaskExtensions.Timeout<LobbyCreated_t>(_003C_003E8__1.source.Task, TimeSpan.FromSeconds(20.0), (DelayType)0, (PlayerLoopTiming)8, (CancellationTokenSource)null).GetAwaiter();
					if (!val2.IsCompleted)
					{
						num = (_003C_003E1__state = 2);
						_003C_003Eu__2 = val2;
						_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter<LobbyCreated_t>, _003CCreateLobby_003Ed__6>(ref val2, ref this);
						return;
					}
					goto IL_01b1;
				}
				lobbyServiceSteam.ApplyLobbyData((CSteamID)_003Cresult_003E5__2.m_ulSteamIDLobby, ref lobbyServiceSteam._currentLobby);
				int numLobbyMembers = SteamMatchmaking.GetNumLobbyMembers(lobbyServiceSteam._currentLobby.GetCSteamId());
				for (int num2 = 0; num2 < numLobbyMembers; num2++)
				{
					lobbyServiceSteam.AddLobbyMember(SteamMatchmaking.GetLobbyMemberByIndex(lobbyServiceSteam._currentLobby.GetCSteamId(), num2));
				}
				UnityEngine.Debug.Log("Created lobby " + lobbyServiceSteam._currentLobby.id);
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				_003C_003E8__1 = null;
				_003C_003Et__builder.SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			_003C_003E8__1 = null;
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
	private struct _003CGetLobbies_003Ed__10 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder _003C_003Et__builder;

		public LobbyServiceSteam _003C_003E4__this;

		public Action<LobbySearchResult> onUpdated;

		private _003C_003Ec__DisplayClass10_0 _003C_003E8__1;

		private Awaiter _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
			//IL_012d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0132: Unknown result type (might be due to invalid IL or missing references)
			//IL_0139: Unknown result type (might be due to invalid IL or missing references)
			//IL_019e: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
			//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
			//IL_0227: Unknown result type (might be due to invalid IL or missing references)
			//IL_022c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0233: Unknown result type (might be due to invalid IL or missing references)
			//IL_0298: Unknown result type (might be due to invalid IL or missing references)
			//IL_029d: Unknown result type (might be due to invalid IL or missing references)
			//IL_02a4: Unknown result type (might be due to invalid IL or missing references)
			//IL_0321: Unknown result type (might be due to invalid IL or missing references)
			//IL_0326: Unknown result type (might be due to invalid IL or missing references)
			//IL_032d: Unknown result type (might be due to invalid IL or missing references)
			//IL_038f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0394: Unknown result type (might be due to invalid IL or missing references)
			//IL_039b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0069: Unknown result type (might be due to invalid IL or missing references)
			//IL_006e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0071: Unknown result type (might be due to invalid IL or missing references)
			//IL_0076: Unknown result type (might be due to invalid IL or missing references)
			//IL_0163: Unknown result type (might be due to invalid IL or missing references)
			//IL_0168: Unknown result type (might be due to invalid IL or missing references)
			//IL_016b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0170: Unknown result type (might be due to invalid IL or missing references)
			//IL_025d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0262: Unknown result type (might be due to invalid IL or missing references)
			//IL_0265: Unknown result type (might be due to invalid IL or missing references)
			//IL_026a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0357: Unknown result type (might be due to invalid IL or missing references)
			//IL_035c: Unknown result type (might be due to invalid IL or missing references)
			//IL_035f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0364: Unknown result type (might be due to invalid IL or missing references)
			//IL_008a: Unknown result type (might be due to invalid IL or missing references)
			//IL_008b: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
			//IL_0184: Unknown result type (might be due to invalid IL or missing references)
			//IL_0185: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f1: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f4: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f9: Unknown result type (might be due to invalid IL or missing references)
			//IL_027e: Unknown result type (might be due to invalid IL or missing references)
			//IL_027f: Unknown result type (might be due to invalid IL or missing references)
			//IL_02e6: Unknown result type (might be due to invalid IL or missing references)
			//IL_02eb: Unknown result type (might be due to invalid IL or missing references)
			//IL_02ee: Unknown result type (might be due to invalid IL or missing references)
			//IL_02f3: Unknown result type (might be due to invalid IL or missing references)
			//IL_0378: Unknown result type (might be due to invalid IL or missing references)
			//IL_0379: Unknown result type (might be due to invalid IL or missing references)
			//IL_0113: Unknown result type (might be due to invalid IL or missing references)
			//IL_0114: Unknown result type (might be due to invalid IL or missing references)
			//IL_020d: Unknown result type (might be due to invalid IL or missing references)
			//IL_020e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0307: Unknown result type (might be due to invalid IL or missing references)
			//IL_0308: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			try
			{
				UniTask val2;
				Awaiter val;
				switch (num)
				{
				default:
					_003C_003E8__1 = new _003C_003Ec__DisplayClass10_0();
					_003C_003E8__1._003C_003E4__this = _003C_003E4__this;
					_003C_003E8__1.result = new LobbySearchResult();
					if (!NeedsToStop())
					{
						val2 = Search((ELobbyDistanceFilter)0, LobbyConnectionQuality.Best);
						val = val2.GetAwaiter();
						if (!val.IsCompleted)
						{
							num = (_003C_003E1__state = 0);
							_003C_003Eu__1 = val;
							_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter, _003CGetLobbies_003Ed__10>(ref val, ref this);
							return;
						}
						goto IL_00bf;
					}
					goto end_IL_0007;
				case 0:
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default;
					num = (_003C_003E1__state = -1);
					goto IL_00bf;
				case 1:
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default;
					num = (_003C_003E1__state = -1);
					goto IL_0148;
				case 2:
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default;
					num = (_003C_003E1__state = -1);
					goto IL_01b9;
				case 3:
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default;
					num = (_003C_003E1__state = -1);
					goto IL_0242;
				case 4:
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default;
					num = (_003C_003E1__state = -1);
					goto IL_02b3;
				case 5:
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default;
					num = (_003C_003E1__state = -1);
					goto IL_033c;
				case 6:
					{
						val = _003C_003Eu__1;
						_003C_003Eu__1 = default;
						num = (_003C_003E1__state = -1);
						break;
					}
					IL_0148:
					val.GetResult();
					if (!NeedsToStop())
					{
						val2 = Search((ELobbyDistanceFilter)1, LobbyConnectionQuality.Good);
						val = val2.GetAwaiter();
						if (!val.IsCompleted)
						{
							num = (_003C_003E1__state = 2);
							_003C_003Eu__1 = val;
							_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter, _003CGetLobbies_003Ed__10>(ref val, ref this);
							return;
						}
						goto IL_01b9;
					}
					goto end_IL_0007;
					IL_0242:
					val.GetResult();
					if (!NeedsToStop())
					{
						val2 = Search((ELobbyDistanceFilter)2, LobbyConnectionQuality.Okay);
						val = val2.GetAwaiter();
						if (!val.IsCompleted)
						{
							num = (_003C_003E1__state = 4);
							_003C_003Eu__1 = val;
							_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter, _003CGetLobbies_003Ed__10>(ref val, ref this);
							return;
						}
						goto IL_02b3;
					}
					goto end_IL_0007;
					IL_00bf:
					val.GetResult();
					onUpdated?.Invoke(_003C_003E8__1.result);
					val2 = UniTask.WaitForSeconds(2f, true, (PlayerLoopTiming)8, default(CancellationToken));
					val = val2.GetAwaiter();
					if (!val.IsCompleted)
					{
						num = (_003C_003E1__state = 1);
						_003C_003Eu__1 = val;
						_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter, _003CGetLobbies_003Ed__10>(ref val, ref this);
						return;
					}
					goto IL_0148;
					IL_01b9:
					val.GetResult();
					onUpdated?.Invoke(_003C_003E8__1.result);
					val2 = UniTask.WaitForSeconds(2f, true, (PlayerLoopTiming)8, default(CancellationToken));
					val = val2.GetAwaiter();
					if (!val.IsCompleted)
					{
						num = (_003C_003E1__state = 3);
						_003C_003Eu__1 = val;
						_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter, _003CGetLobbies_003Ed__10>(ref val, ref this);
						return;
					}
					goto IL_0242;
					IL_033c:
					val.GetResult();
					if (!NeedsToStop())
					{
						val2 = Search((ELobbyDistanceFilter)3, LobbyConnectionQuality.Bad);
						val = val2.GetAwaiter();
						if (!val.IsCompleted)
						{
							num = (_003C_003E1__state = 6);
							_003C_003Eu__1 = val;
							_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter, _003CGetLobbies_003Ed__10>(ref val, ref this);
							return;
						}
						break;
					}
					goto end_IL_0007;
					IL_02b3:
					val.GetResult();
					onUpdated?.Invoke(_003C_003E8__1.result);
					val2 = UniTask.WaitForSeconds(2f, true, (PlayerLoopTiming)8, default(CancellationToken));
					val = val2.GetAwaiter();
					if (!val.IsCompleted)
					{
						num = (_003C_003E1__state = 5);
						_003C_003Eu__1 = val;
						_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter, _003CGetLobbies_003Ed__10>(ref val, ref this);
						return;
					}
					goto IL_033c;
				}
				val.GetResult();
				onUpdated?.Invoke(_003C_003E8__1.result);
				end_IL_0007:;
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				_003C_003E8__1 = null;
				_003C_003Et__builder.SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			_003C_003E8__1 = null;
			_003C_003Et__builder.SetResult();
			static bool NeedsToStop()
			{
				return ManagerBase<TransitionManager>.instance.state == TransitionManager.StateType.Loading;
			}
			[AsyncStateMachine(typeof(_003C_003Ec__DisplayClass10_0._003C_003CGetLobbies_003Eg__Search_007C0_003Ed))]
			UniTask Search(ELobbyDistanceFilter dist, LobbyConnectionQuality quality)
			{
				//IL_0002: Unknown result type (might be due to invalid IL or missing references)
				//IL_0007: Unknown result type (might be due to invalid IL or missing references)
				//IL_0016: Unknown result type (might be due to invalid IL or missing references)
				//IL_0017: Unknown result type (might be due to invalid IL or missing references)
				//IL_0041: Unknown result type (might be due to invalid IL or missing references)
				_003C_003Ec__DisplayClass10_0._003C_003CGetLobbies_003Eg__Search_007C0_003Ed obj = default;
				obj._003C_003Et__builder = AsyncUniTaskMethodBuilder.Create();
				obj._003C_003E4__this = this;
				obj.dist = dist;
				obj.quality = quality;
				obj._003C_003E1__state = -1;
				obj._003C_003Et__builder.Start<_003C_003Ec__DisplayClass10_0._003C_003CGetLobbies_003Eg__Search_007C0_003Ed>(ref obj);
				return obj._003C_003Et__builder.Task;
			}
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
	private struct _003CHandleUserLeavingGame_003Ed__9 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder _003C_003Et__builder;

		private void MoveNext()
		{
			try
			{
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
	private struct _003CJoinGameOfCurrentLobby_003Ed__15 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder _003C_003Et__builder;

		public LobbyServiceSteam _003C_003E4__this;

		private Awaiter _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
			//IL_0084: Unknown result type (might be due to invalid IL or missing references)
			//IL_0089: Unknown result type (might be due to invalid IL or missing references)
			//IL_0090: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			//IL_006a: Unknown result type (might be due to invalid IL or missing references)
			//IL_006b: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			LobbyServiceSteam CS_0024_003C_003E8__locals3 = _003C_003E4__this;
			try
			{
				Awaiter val2;
				if (num == 0 || num != 1)
				{
					UniTask val;
					try
					{
						if (num != 0)
						{
							val = UniTaskExtensions.Timeout(UniTask.WaitWhile((Func<bool>)(() => CS_0024_003C_003E8__locals3._currentLobby != null && string.IsNullOrEmpty(CS_0024_003C_003E8__locals3._currentLobby.gameServerAddress)), (PlayerLoopTiming)8, default(CancellationToken)), TimeSpan.FromSeconds(5.0), (DelayType)2, (PlayerLoopTiming)8, (CancellationTokenSource)null);
							val2 = val.GetAwaiter();
							if (!val2.IsCompleted)
							{
								num = (_003C_003E1__state = 0);
								_003C_003Eu__1 = val2;
								_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter, _003CJoinGameOfCurrentLobby_003Ed__15>(ref val2, ref this);
								return;
							}
						}
						else
						{
							val2 = _003C_003Eu__1;
							_003C_003Eu__1 = default;
							num = (_003C_003E1__state = -1);
						}
						val2.GetResult();
					}
					catch (TimeoutException)
					{
						throw new DewException(DewExceptionType.LobbyTimeout);
					}
					val = ((LobbyServiceProvider)CS_0024_003C_003E8__locals3).JoinGameOfCurrentLobby();
					val2 = val.GetAwaiter();
					if (!val2.IsCompleted)
					{
						num = (_003C_003E1__state = 1);
						_003C_003Eu__1 = val2;
						_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter, _003CJoinGameOfCurrentLobby_003Ed__15>(ref val2, ref this);
						return;
					}
				}
				else
				{
					val2 = _003C_003Eu__1;
					_003C_003Eu__1 = default;
					num = (_003C_003E1__state = -1);
				}
				val2.GetResult();
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
	private struct _003CJoinLobby_003Ed__7 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder _003C_003Et__builder;

		public LobbyServiceSteam _003C_003E4__this;

		public object lobby;

		private _003C_003Ec__DisplayClass7_1 _003C_003E8__1;

		private _003C_003Ec__DisplayClass7_0 _003C_003E8__2;

		private string _003Cstr_003E5__2;

		private CSteamID _003ClobbyId_003E5__3;

		private ulong _003ClobbyIdUint_003E5__4;

		private Awaiter _003C_003Eu__1;

		private Awaiter<LobbyMatchList_t> _003C_003Eu__2;

		private Awaiter<LobbyEnter_t> _003C_003Eu__3;

		private void MoveNext()
		{
			//IL_012c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0131: Unknown result type (might be due to invalid IL or missing references)
			//IL_0139: Unknown result type (might be due to invalid IL or missing references)
			//IL_0236: Unknown result type (might be due to invalid IL or missing references)
			//IL_023b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0243: Unknown result type (might be due to invalid IL or missing references)
			//IL_0349: Unknown result type (might be due to invalid IL or missing references)
			//IL_034e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0356: Unknown result type (might be due to invalid IL or missing references)
			//IL_0254: Unknown result type (might be due to invalid IL or missing references)
			//IL_0367: Unknown result type (might be due to invalid IL or missing references)
			//IL_036c: Unknown result type (might be due to invalid IL or missing references)
			//IL_037e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0384: Unknown result type (might be due to invalid IL or missing references)
			//IL_0395: Unknown result type (might be due to invalid IL or missing references)
			//IL_03ac: Unknown result type (might be due to invalid IL or missing references)
			//IL_0067: Unknown result type (might be due to invalid IL or missing references)
			//IL_006c: Unknown result type (might be due to invalid IL or missing references)
			//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f8: Unknown result type (might be due to invalid IL or missing references)
			//IL_01fd: Unknown result type (might be due to invalid IL or missing references)
			//IL_0201: Unknown result type (might be due to invalid IL or missing references)
			//IL_0206: Unknown result type (might be due to invalid IL or missing references)
			//IL_026e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0273: Unknown result type (might be due to invalid IL or missing references)
			//IL_02ca: Unknown result type (might be due to invalid IL or missing references)
			//IL_02cf: Unknown result type (might be due to invalid IL or missing references)
			//IL_02f5: Unknown result type (might be due to invalid IL or missing references)
			//IL_030b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0310: Unknown result type (might be due to invalid IL or missing references)
			//IL_0314: Unknown result type (might be due to invalid IL or missing references)
			//IL_0319: Unknown result type (might be due to invalid IL or missing references)
			//IL_021b: Unknown result type (might be due to invalid IL or missing references)
			//IL_021d: Unknown result type (might be due to invalid IL or missing references)
			//IL_03c3: Unknown result type (might be due to invalid IL or missing references)
			//IL_03ca: Unknown result type (might be due to invalid IL or missing references)
			//IL_03e5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
			//IL_032e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0330: Unknown result type (might be due to invalid IL or missing references)
			//IL_0111: Unknown result type (might be due to invalid IL or missing references)
			//IL_0113: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			LobbyServiceSteam lobbyServiceSteam = _003C_003E4__this;
			try
			{
				Awaiter val3;
				Awaiter<LobbyMatchList_t> val2;
				Awaiter<LobbyEnter_t> val;
				string text;
				switch (num)
				{
				default:
					_003C_003E8__2 = new _003C_003Ec__DisplayClass7_0();
					_003C_003E8__2._003C_003E4__this = _003C_003E4__this;
					_003Cstr_003E5__2 = lobby.ToString();
					if (ulong.TryParse(_003Cstr_003E5__2, out _003ClobbyIdUint_003E5__4))
					{
						_003ClobbyId_003E5__3 = (CSteamID)_003ClobbyIdUint_003E5__4;
						goto IL_028d;
					}
					if (_003Cstr_003E5__2.Trim().Replace(" ", "").Length == 8)
					{
						_003C_003E8__1 = new _003C_003Ec__DisplayClass7_1();
						if (lobbyServiceSteam.isRefreshingLobby)
						{
							ManagerBase<TransitionManager>.instance.UpdateLoadingStatus(LoadingStatus.WaitingForService);
							UniTask val4 = UniTaskExtensions.Timeout(UniTask.WaitWhile((Func<bool>)(() => _003C_003E8__2._003C_003E4__this.isRefreshingLobby), (PlayerLoopTiming)8, default(CancellationToken)), TimeSpan.FromSeconds(15.0), (DelayType)0, (PlayerLoopTiming)8, (CancellationTokenSource)null);
							val3 = val4.GetAwaiter();
							if (!val3.IsCompleted)
							{
								num = (_003C_003E1__state = 0);
								_003C_003Eu__1 = val3;
								_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter, _003CJoinLobby_003Ed__7>(ref val3, ref this);
								return;
							}
							goto IL_0148;
						}
						goto IL_014f;
					}
					throw new DewException(DewExceptionType.LobbyNotFound, "Code Invalid");
				case 0:
					val3 = _003C_003Eu__1;
					_003C_003Eu__1 = default;
					num = (_003C_003E1__state = -1);
					goto IL_0148;
				case 1:
					val2 = _003C_003Eu__2;
					_003C_003Eu__2 = default;
					num = (_003C_003E1__state = -1);
					goto IL_0252;
				case 2:
					{
						val = _003C_003Eu__3;
						_003C_003Eu__3 = default;
						num = (_003C_003E1__state = -1);
						break;
					}
					IL_028d:
					UnityEngine.Debug.Log("JoinLobby(Steam) - Joining lobby with ID: " + _003ClobbyIdUint_003E5__4);
					ManagerBase<TransitionManager>.instance.UpdateLoadingStatus(LoadingStatus.ConnectingToLobby);
					_003C_003E8__2.source = new UniTaskCompletionSource<LobbyEnter_t>();
					lobbyServiceSteam._lobbyEnter.Set(SteamMatchmaking.JoinLobby(_003ClobbyId_003E5__3), (APIDispatchDelegate<LobbyEnter_t>)((LobbyEnter_t t, bool failure) =>
					{
						//IL_0000: Unknown result type (might be due to invalid IL or missing references)
						//IL_0014: Unknown result type (might be due to invalid IL or missing references)
						//IL_004c: Unknown result type (might be due to invalid IL or missing references)
						if (t.m_EChatRoomEnterResponse != 1)
						{
							_003C_003E8__2.source.TrySetException((Exception)new SteamException("JoinLobby", (object)(EChatRoomEnterResponse)t.m_EChatRoomEnterResponse));
						}
						else if (failure)
						{
							_003C_003E8__2.source.TrySetException((Exception)new SteamException("JoinLobby"));
						}
						_003C_003E8__2.source.TrySetResult(t);
					}));
					val = UniTaskExtensions.Timeout<LobbyEnter_t>(_003C_003E8__2.source.Task, TimeSpan.FromSeconds(20.0), (DelayType)0, (PlayerLoopTiming)8, (CancellationTokenSource)null).GetAwaiter();
					if (!val.IsCompleted)
					{
						num = (_003C_003E1__state = 2);
						_003C_003Eu__3 = val;
						_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter<LobbyEnter_t>, _003CJoinLobby_003Ed__7>(ref val, ref this);
						return;
					}
					break;
					IL_0148:
					val3.GetResult();
					goto IL_014f;
					IL_014f:
					ManagerBase<TransitionManager>.instance.UpdateLoadingStatus(LoadingStatus.GettingLobbyInformation);
					text = _003Cstr_003E5__2.Trim().Replace(" ", "").ToUpper();
					UnityEngine.Debug.Log("JoinLobby(Steam) - Finding lobby with short code: " + text);
					SteamMatchmaking.AddRequestLobbyListStringFilter("shortCode", DewPersistence.ToJson(text), (ELobbyComparison)0);
					SteamMatchmaking.AddRequestLobbyListDistanceFilter((ELobbyDistanceFilter)3);
					_003C_003E8__1.s = new UniTaskCompletionSource<LobbyMatchList_t>();
					lobbyServiceSteam._lobbyMatchList.Set(SteamMatchmaking.RequestLobbyList(), (APIDispatchDelegate<LobbyMatchList_t>)((LobbyMatchList_t t, bool failure) =>
					{
						//IL_0021: Unknown result type (might be due to invalid IL or missing references)
						if (failure)
						{
							_003C_003E8__1.s.TrySetException((Exception)new SteamException("GetLobbies"));
						}
						else
						{
							_003C_003E8__1.s.TrySetResult(t);
						}
					}));
					val2 = UniTaskExtensions.Timeout<LobbyMatchList_t>(_003C_003E8__1.s.Task, TimeSpan.FromSeconds(15.0), (DelayType)0, (PlayerLoopTiming)8, (CancellationTokenSource)null).GetAwaiter();
					if (!val2.IsCompleted)
					{
						num = (_003C_003E1__state = 1);
						_003C_003Eu__2 = val2;
						_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter<LobbyMatchList_t>, _003CJoinLobby_003Ed__7>(ref val2, ref this);
						return;
					}
					goto IL_0252;
					IL_0252:
					if (val2.GetResult().m_nLobbiesMatching == 0)
					{
						throw new DewException(DewExceptionType.LobbyNotFound);
					}
					_003ClobbyId_003E5__3 = SteamMatchmaking.GetLobbyByIndex(0);
					_003C_003E8__1 = null;
					goto IL_028d;
				}
				LobbyEnter_t result = val.GetResult();
				lobbyServiceSteam._currentLobby = new LobbyInstanceSteam();
				lobbyServiceSteam._currentLobby.SetCSteamId((CSteamID)result.m_ulSteamIDLobby);
				lobbyServiceSteam.ApplyLobbyData(lobbyServiceSteam._currentLobby.GetCSteamId(), ref lobbyServiceSteam._currentLobby);
				int numLobbyMembers = SteamMatchmaking.GetNumLobbyMembers(lobbyServiceSteam._currentLobby.GetCSteamId());
				for (int num2 = 0; num2 < numLobbyMembers; num2++)
				{
					lobbyServiceSteam.AddLobbyMember(SteamMatchmaking.GetLobbyMemberByIndex(lobbyServiceSteam._currentLobby.GetCSteamId(), num2));
				}
				UnityEngine.Debug.Log($"JoinLobby(Steam) - Joined {_003ClobbyId_003E5__3}");
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				_003C_003E8__2 = null;
				_003Cstr_003E5__2 = null;
				_003C_003Et__builder.SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			_003C_003E8__2 = null;
			_003Cstr_003E5__2 = null;
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
	private struct _003CLeaveLobby_003Ed__8 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder _003C_003Et__builder;

		public LobbyServiceSteam _003C_003E4__this;

		private void MoveNext()
		{
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			LobbyServiceSteam lobbyServiceSteam = _003C_003E4__this;
			try
			{
				if (lobbyServiceSteam._currentLobby != null)
				{
					ManagerBase<TransitionManager>.instance.UpdateLoadingStatus(LoadingStatus.CleaningUpPreviousLobby);
					SteamMatchmaking.LeaveLobby(lobbyServiceSteam._currentLobby.GetCSteamId());
					UnityEngine.Debug.Log($"Left lobby {lobbyServiceSteam._currentLobby.GetCSteamId()}");
					lobbyServiceSteam._currentLobby = null;
					lobbyServiceSteam.InvokeOnCurrentLobbyChanged();
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
	private struct _003CSetLobbyAttribute_003Ed__11 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder _003C_003Et__builder;

		public LobbyServiceSteam _003C_003E4__this;

		public string key;

		public object value;

		private void MoveNext()
		{
			try
			{
				LobbyServiceSteam lobbyServiceSteam = _003C_003E4__this;
				string key = this.key;
				object value = this.value;
				Dew.Debounce(null, 0.35f, () =>
				{
					//IL_003d: Unknown result type (might be due to invalid IL or missing references)
					//IL_0091: Unknown result type (might be due to invalid IL or missing references)
					//IL_0070: Unknown result type (might be due to invalid IL or missing references)
					if (lobbyServiceSteam._currentLobby != null && lobbyServiceSteam._currentLobby.isLobbyLeader)
					{
						if (key == "allowJoin")
						{
							SteamMatchmaking.SetLobbyJoinable(lobbyServiceSteam._currentLobby.GetCSteamId(), (bool)value);
						}
						if (key == "maxPlayers")
						{
							SteamMatchmaking.SetLobbyMemberLimit(lobbyServiceSteam._currentLobby.GetCSteamId(), (int)value);
						}
						if (!SteamMatchmaking.SetLobbyData(lobbyServiceSteam._currentLobby.GetCSteamId(), key, DewPersistence.ToJson(value)))
						{
							throw new SteamException("SetLobbyAttribute failed");
						}
					}
				}, "SetLobbyAttribute(" + key + ")");
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
	private struct _003CSetLobbyMemberAttribute_003Ed__16 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder _003C_003Et__builder;

		private void MoveNext()
		{
			try
			{
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

	private LobbyInstanceSteam _currentLobby;

	public List<CSteamID> lobbyMembers = new List<CSteamID>();

	public bool disableVersionCheck;

	private CallResult<LobbyCreated_t> _lobbyCreated;

	private CallResult<LobbyEnter_t> _lobbyEnter;

	private CallResult<LobbyMatchList_t> _lobbyMatchList;

	private Callback<LobbyDataUpdate_t> _lobbyDataUpdate;

	private Callback<LobbyChatMsg_t> _lobbyChatMsg;

	private Callback<LobbyChatUpdate_t> _lobbyChatUpdate;

	private Callback<GameLobbyJoinRequested_t> _gameLobbyJoinRequested;

	private Callback<GameRichPresenceJoinRequested_t> _gameRichPresenceJoinRequested;

	private byte[] _receivedMsgBuffer = new byte[1024];

	private byte[] _sentMsgBuffer = new byte[1024];

	public override LobbyInstance currentLobby => _currentLobby;

	private async void Start()
	{
		if (DewBuildProfile.current.platform == PlatformType.STEAM)
		{
			CreateCallbacks();
			UniTaskVoid val = CheckLaunchConnectString();
			val.Forget();
		}
	}

	[AsyncStateMachine(typeof(_003CCreateLobby_003Ed__6))]
	public override UniTask CreateLobby()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		_003CCreateLobby_003Ed__6 obj = default;
		obj._003C_003Et__builder = AsyncUniTaskMethodBuilder.Create();
		obj._003C_003E4__this = this;
		obj._003C_003E1__state = -1;
		obj._003C_003Et__builder.Start<_003CCreateLobby_003Ed__6>(ref obj);
		return obj._003C_003Et__builder.Task;
	}

	[AsyncStateMachine(typeof(_003CJoinLobby_003Ed__7))]
	public override UniTask JoinLobby(object lobby)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		_003CJoinLobby_003Ed__7 obj = default;
		obj._003C_003Et__builder = AsyncUniTaskMethodBuilder.Create();
		obj._003C_003E4__this = this;
		obj.lobby = lobby;
		obj._003C_003E1__state = -1;
		obj._003C_003Et__builder.Start<_003CJoinLobby_003Ed__7>(ref obj);
		return obj._003C_003Et__builder.Task;
	}

	[AsyncStateMachine(typeof(_003CLeaveLobby_003Ed__8))]
	public override UniTask LeaveLobby()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		_003CLeaveLobby_003Ed__8 obj = default;
		obj._003C_003Et__builder = AsyncUniTaskMethodBuilder.Create();
		obj._003C_003E4__this = this;
		obj._003C_003E1__state = -1;
		obj._003C_003Et__builder.Start<_003CLeaveLobby_003Ed__8>(ref obj);
		return obj._003C_003Et__builder.Task;
	}

	[AsyncStateMachine(typeof(_003CHandleUserLeavingGame_003Ed__9))]
	public override UniTask HandleUserLeavingGame(string id)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		_003CHandleUserLeavingGame_003Ed__9 obj = default;
		obj._003C_003Et__builder = AsyncUniTaskMethodBuilder.Create();
		obj._003C_003E1__state = -1;
		obj._003C_003Et__builder.Start<_003CHandleUserLeavingGame_003Ed__9>(ref obj);
		return obj._003C_003Et__builder.Task;
	}

	[AsyncStateMachine(typeof(_003CGetLobbies_003Ed__10))]
	public override UniTask GetLobbies(Action<LobbySearchResult> onUpdated, object continuationToken = null)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		_003CGetLobbies_003Ed__10 obj = default;
		obj._003C_003Et__builder = AsyncUniTaskMethodBuilder.Create();
		obj._003C_003E4__this = this;
		obj.onUpdated = onUpdated;
		obj._003C_003E1__state = -1;
		obj._003C_003Et__builder.Start<_003CGetLobbies_003Ed__10>(ref obj);
		return obj._003C_003Et__builder.Task;
	}

	[AsyncStateMachine(typeof(_003CSetLobbyAttribute_003Ed__11))]
	public override UniTask SetLobbyAttribute(string key, object value, bool isPublic = true)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		_003CSetLobbyAttribute_003Ed__11 obj = default;
		obj._003C_003Et__builder = AsyncUniTaskMethodBuilder.Create();
		obj._003C_003E4__this = this;
		obj.key = key;
		obj.value = value;
		obj._003C_003E1__state = -1;
		obj._003C_003Et__builder.Start<_003CSetLobbyAttribute_003Ed__11>(ref obj);
		return obj._003C_003Et__builder.Task;
	}

	public void ActivateGameOverlayInviteDialog()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		if (_currentLobby != null)
		{
			SteamFriends.ActivateGameOverlayInviteDialog(_currentLobby.GetCSteamId());
		}
	}

	public bool InviteToLobby(CSteamID friend)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		if (_currentLobby == null)
		{
			return false;
		}
		return SteamMatchmaking.InviteUserToLobby(_currentLobby.GetCSteamId(), friend);
	}

	public override string GetGameServerAddress()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		return ((object)SteamUser.GetSteamID()/*cast due to constrained. prefix*/).ToString();
	}

	[AsyncStateMachine(typeof(_003CJoinGameOfCurrentLobby_003Ed__15))]
	public override UniTask JoinGameOfCurrentLobby()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		_003CJoinGameOfCurrentLobby_003Ed__15 obj = default;
		obj._003C_003Et__builder = AsyncUniTaskMethodBuilder.Create();
		obj._003C_003E4__this = this;
		obj._003C_003E1__state = -1;
		obj._003C_003Et__builder.Start<_003CJoinGameOfCurrentLobby_003Ed__15>(ref obj);
		return obj._003C_003Et__builder.Task;
	}

	[AsyncStateMachine(typeof(_003CSetLobbyMemberAttribute_003Ed__16))]
	public override UniTask SetLobbyMemberAttribute(string key, object value, bool isPublic = true)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		_003CSetLobbyMemberAttribute_003Ed__16 obj = default;
		obj._003C_003Et__builder = AsyncUniTaskMethodBuilder.Create();
		obj._003C_003E1__state = -1;
		obj._003C_003Et__builder.Start<_003CSetLobbyMemberAttribute_003Ed__16>(ref obj);
		return obj._003C_003Et__builder.Task;
	}

	private void CreateCallbacks()
	{
		_lobbyCreated = CallResult<LobbyCreated_t>.Create((APIDispatchDelegate<LobbyCreated_t>)null);
		_lobbyEnter = CallResult<LobbyEnter_t>.Create((APIDispatchDelegate<LobbyEnter_t>)null);
		_lobbyMatchList = CallResult<LobbyMatchList_t>.Create((APIDispatchDelegate<LobbyMatchList_t>)null);
		_lobbyDataUpdate = Callback<LobbyDataUpdate_t>.Create((DispatchDelegate<LobbyDataUpdate_t>)LobbyDataUpdateCallback);
		_lobbyChatMsg = Callback<LobbyChatMsg_t>.Create((DispatchDelegate<LobbyChatMsg_t>)LobbyChatMsgCallback);
		_lobbyChatUpdate = Callback<LobbyChatUpdate_t>.Create((DispatchDelegate<LobbyChatUpdate_t>)LobbyChatUpdateCallback);
		if (DewBuildProfile.current.useSteamLobbyAndRelay)
		{
			_gameLobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create((DispatchDelegate<GameLobbyJoinRequested_t>)GameLobbyJoinRequestedCallback);
		}
		_gameRichPresenceJoinRequested = Callback<GameRichPresenceJoinRequested_t>.Create((DispatchDelegate<GameRichPresenceJoinRequested_t>)GameRichPresenceJoinRequestedCallback);
	}

	private void LobbyChatMsgCallback(LobbyChatMsg_t param)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			if (currentLobby != null && !((CSteamID)param.m_ulSteamIDLobby != _currentLobby.GetCSteamId()) && !((CSteamID)param.m_ulSteamIDUser != SteamMatchmaking.GetLobbyOwner(_currentLobby.GetCSteamId())))
			{
				CSteamID val = default;
				EChatEntryType val2 = default;
				int lobbyChatEntry = SteamMatchmaking.GetLobbyChatEntry(_currentLobby.GetCSteamId(), (int)param.m_iChatID, ref val, _receivedMsgBuffer, _receivedMsgBuffer.Length, ref val2);
				Span<byte> val3 = new Span<byte>(_receivedMsgBuffer, 1, lobbyChatEntry - 1);
				string payload = Encoding.Unicode.GetString(Span<byte>.op_Implicit(val3));
				LobbyMessage lobbyMessage = new LobbyMessage
				{
					type = (LobbyMessageType)_receivedMsgBuffer[0],
					payload = payload
				};
				if (lobbyMessage.type != LobbyMessageType.GameServerAddress)
				{
					throw new ArgumentOutOfRangeException();
				}
				_currentLobby.gameServerAddress = lobbyMessage.payload;
			}
		}
		catch (Exception exception)
		{
			UnityEngine.Debug.LogException(exception);
		}
	}

	private void LobbyChatUpdateCallback(LobbyChatUpdate_t param)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Expected I4, but got Unknown
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Invalid comparison between Unknown and I4
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Invalid comparison between Unknown and I4
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		if (currentLobby == null || (CSteamID)param.m_ulSteamIDLobby != _currentLobby.GetCSteamId())
		{
			return;
		}
		CSteamID val = (CSteamID)param.m_ulSteamIDUserChanged;
		EChatMemberStateChange val2 = (EChatMemberStateChange)param.m_rgfChatMemberStateChange;
		UnityEngine.Debug.Log(string.Format("{0} {1} {2}", "LobbyChatUpdateCallback", val2, val));
		switch (val2 - 1)
		{
		default:
			if ((int)val2 != 8)
			{
				if ((int)val2 != 16)
				{
					break;
				}
				RemoveLobbyMember(val);
				return;
			}
			RemoveLobbyMember(val);
			return;
		case 0:
			AddLobbyMember(val);
			return;
		case 1:
			RemoveLobbyMember(val);
			return;
		case 3:
			RemoveLobbyMember(val);
			return;
		case 2:
			break;
		}
		throw new ArgumentOutOfRangeException();
	}

	private void LobbyDataUpdateCallback(LobbyDataUpdate_t param)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		CSteamID val = (CSteamID)param.m_ulSteamIDLobby;
		if (_currentLobby != null && val == _currentLobby.GetCSteamId())
		{
			ApplyLobbyData(val, ref _currentLobby);
		}
	}

	private bool ApplyLobbyData(CSteamID lobbyId, ref LobbyInstanceSteam data)
	{
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			FieldInfo[] fields = typeof(LobbyInstance).GetFields(BindingFlags.Instance | BindingFlags.Public);
			foreach (FieldInfo fieldInfo in fields)
			{
				if (!(fieldInfo.Name == "id") && !(fieldInfo.Name == "maxPlayers") && !(fieldInfo.Name == "isLobbyLeader") && !(fieldInfo.Name == "currentPlayers") && !(fieldInfo.Name == "connectionQuality") && !(fieldInfo.Name == "gameServerAddress"))
				{
					try
					{
						string lobbyData = SteamMatchmaking.GetLobbyData(lobbyId, fieldInfo.Name);
						fieldInfo.SetValue(data, DewPersistence.FromJson(lobbyData, fieldInfo.FieldType));
					}
					catch (Exception)
					{
					}
				}
			}
			data.SetCSteamId(lobbyId);
			data.currentPlayers = SteamMatchmaking.GetNumLobbyMembers(lobbyId);
			data.maxPlayers = Mathf.Clamp(SteamMatchmaking.GetLobbyMemberLimit(lobbyId), 0, 1000);
			if (_currentLobby != null && _currentLobby.GetCSteamId() == lobbyId)
			{
				InvokeOnCurrentLobbyChanged();
			}
			return true;
		}
		catch (Exception)
		{
			return false;
		}
	}

	private void GameLobbyJoinRequestedCallback(GameLobbyJoinRequested_t param)
	{
		HandleExternalJoinRequest(((object)param.m_steamIDLobby/*cast due to constrained. prefix*/).ToString());
	}

	private void GameRichPresenceJoinRequestedCallback(GameRichPresenceJoinRequested_t param)
	{
		HandleExternalJoinRequest(param.m_rgchConnect);
	}

	[AsyncStateMachine(typeof(_003CCheckLaunchConnectString_003Ed__33))]
	private UniTaskVoid CheckLaunchConnectString()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		_003CCheckLaunchConnectString_003Ed__33 obj = default;
		obj._003C_003Et__builder = AsyncUniTaskVoidMethodBuilder.Create();
		obj._003C_003E4__this = this;
		obj._003C_003E1__state = -1;
		obj._003C_003Et__builder.Start<_003CCheckLaunchConnectString_003Ed__33>(ref obj);
		return obj._003C_003Et__builder.Task;
	}

	private static string ExtractLaunchAddress(string commandLine)
	{
		if (string.IsNullOrWhiteSpace(commandLine))
		{
			return null;
		}
		string[] array = commandLine.Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].StartsWith("sod:") && array[i].Length == "sod:".Length + 8)
			{
				return array[i];
			}
		}
		return null;
	}

	private void HandleExternalJoinRequest(string address)
	{
		if (string.IsNullOrEmpty(address))
		{
			return;
		}
		if (address.StartsWith("sod:"))
		{
			address = address.Substring("sod:".Length);
		}
		List<string> list = new List<string> { "Intro", "Title", "Collectables" };
		if (DewSave.profileMainPath != null)
		{
			if (list.Contains(SceneManager.GetActiveScene().name))
			{
				ManagerBase<TransitionManager>.instance.PlayGame(new DewNetworkStartSettings
				{
					networkMode = DewNetworkMode.MultiplayerJoinLobby,
					address = address
				});
			}
			else
			{
				ManagerBase<MessageManager>.instance.ShowMessageLocalized("Message_Steam_CannotAcceptInvitationInsideGame");
			}
		}
	}

	private void AddLobbyMember(CSteamID user)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		if (!lobbyMembers.Contains(user))
		{
			lobbyMembers.Add(user);
		}
		if (currentLobby.isLobbyLeader)
		{
			BroadcastMessageToLobby(new LobbyMessage
			{
				type = LobbyMessageType.GameServerAddress,
				payload = ((object)SteamUser.GetSteamID()/*cast due to constrained. prefix*/).ToString()
			});
		}
	}

	private void BroadcastMessageToLobby(LobbyMessage message)
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		if (_currentLobby != null && _currentLobby.isLobbyLeader)
		{
			_sentMsgBuffer[0] = (byte)message.type;
			Span<byte> val = new Span<byte>(_sentMsgBuffer, 1, _sentMsgBuffer.Length - 1);
			int bytes = Encoding.Unicode.GetBytes(string.op_Implicit(message.payload), val);
			SteamMatchmaking.SendLobbyChatMsg(_currentLobby.GetCSteamId(), _sentMsgBuffer, bytes + 1);
		}
	}

	private void RemoveLobbyMember(CSteamID user)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		if (lobbyMembers.Contains(user))
		{
			lobbyMembers.Remove(user);
		}
	}
}
