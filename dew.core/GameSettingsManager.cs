using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using Mirror;
using UnityEngine;

public class GameSettingsManager : NetworkedManagerBase<GameSettingsManager>
{
	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CLobby_UpdateAttribute_003Ed__85 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder _003C_003Et__builder;

		public string key;

		public GameSettingsManager _003C_003E4__this;

		private Awaiter _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
			//IL_009c: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			GameSettingsManager obj = _003C_003E4__this;
			try
			{
				Awaiter val;
				if (num == 0)
				{
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default;
					num = (_003C_003E1__state = -1);
					goto IL_00f3;
				}
				if (ManagerBase<LobbyManager>.instance.isLobbyLeader)
				{
					FieldInfo field = typeof(GameSettingsManager).GetField(key, BindingFlags.Instance | BindingFlags.Public);
					PropertyInfo property = typeof(GameSettingsManager).GetProperty(key, BindingFlags.Instance | BindingFlags.Public);
					object value;
					if (field != null)
					{
						value = field.GetValue(obj);
					}
					else
					{
						if (!(property != null))
						{
							throw new ArgumentOutOfRangeException(key);
						}
						value = property.GetValue(obj);
					}
					UniTask val2 = ManagerBase<LobbyManager>.instance.service.SetLobbyAttribute(key, value);
					val = val2.GetAwaiter();
					if (!val.IsCompleted)
					{
						num = (_003C_003E1__state = 0);
						_003C_003Eu__1 = val;
						_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter, _003CLobby_UpdateAttribute_003Ed__85>(ref val, ref this);
						return;
					}
					goto IL_00f3;
				}
				goto end_IL_000e;
				IL_00f3:
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
	private struct _003CLobby_UpdateCanJoinAndGameStarted_003Ed__83 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder _003C_003Et__builder;

		public bool? hasGameStartedOverride;

		public GameSettingsManager _003C_003E4__this;

		private bool _003ChasGameStarted_003E5__2;

		private bool _003CallowJoin_003E5__3;

		private int _003Cversion_003E5__4;

		private int _003Cattempt_003E5__5;

		private Awaiter _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_02d4: Unknown result type (might be due to invalid IL or missing references)
			//IL_02d9: Unknown result type (might be due to invalid IL or missing references)
			//IL_02e1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
			//IL_011f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0124: Unknown result type (might be due to invalid IL or missing references)
			//IL_0128: Unknown result type (might be due to invalid IL or missing references)
			//IL_012d: Unknown result type (might be due to invalid IL or missing references)
			//IL_015d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0162: Unknown result type (might be due to invalid IL or missing references)
			//IL_016a: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
			//IL_0296: Unknown result type (might be due to invalid IL or missing references)
			//IL_029b: Unknown result type (might be due to invalid IL or missing references)
			//IL_029f: Unknown result type (might be due to invalid IL or missing references)
			//IL_02a4: Unknown result type (might be due to invalid IL or missing references)
			//IL_0142: Unknown result type (might be due to invalid IL or missing references)
			//IL_0144: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
			//IL_02b9: Unknown result type (might be due to invalid IL or missing references)
			//IL_02bb: Unknown result type (might be due to invalid IL or missing references)
			//IL_03ce: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			GameSettingsManager gameSettingsManager = _003C_003E4__this;
			try
			{
				if ((uint)num <= 1u)
				{
					goto IL_007e;
				}
				if (num != 2)
				{
					_003ChasGameStarted_003E5__2 = hasGameStartedOverride ?? ((UnityEngine.Object)(object)NetworkedManagerBase<GameManager>.instance != null);
					_003CallowJoin_003E5__3 = !_003ChasGameStarted_003E5__2 || gameSettingsManager.allowMidJoins != AllowMidJoinType.Disallow;
					_003Cversion_003E5__4 = ++_canJoinUpdateVersion;
					_003Cattempt_003E5__5 = 1;
					goto IL_007a;
				}
				Awaiter val = _003C_003Eu__1;
				_003C_003Eu__1 = default;
				num = (_003C_003E1__state = -1);
				goto IL_02f0;
				IL_0331:
				if ((bool)(UnityEngine.Object)(object)NetworkedManagerBase<GameManager>.instance && NetworkedManagerBase<GameManager>.instance.playerRejoinData != null && NetworkedManagerBase<GameManager>.instance.playerRejoinData.Count != 0 && ManagerBase<LobbyManager>.instance.isLobbyLeader)
				{
					List<string> list = NetworkedManagerBase<GameManager>.instance.playerRejoinData.Keys.ToList();
					list.Sort();
					if (ManagerBase<LobbyManager>.instance.service.currentLobby.savedPlayers == null || !ManagerBase<LobbyManager>.instance.service.currentLobby.savedPlayers.SequenceEqual(list))
					{
						ManagerBase<LobbyManager>.instance.service.SetLobbyAttribute("savedPlayers", list);
					}
				}
				goto end_IL_000e;
				IL_031a:
				_003Cattempt_003E5__5++;
				goto IL_007a;
				IL_02f0:
				val.GetResult();
				if (_003Cversion_003E5__4 == _canJoinUpdateVersion && ManagerBase<LobbyManager>.instance.service.currentLobby != null)
				{
					goto IL_031a;
				}
				goto end_IL_000e;
				IL_01b5:
				int num2;
				if (num2 != 1)
				{
					goto IL_031a;
				}
				object obj;
				Exception ex = (Exception)obj;
				UniTask val2;
				if (_003Cversion_003E5__4 == _canJoinUpdateVersion && ManagerBase<LobbyManager>.instance.service.currentLobby != null)
				{
					if (_003Cattempt_003E5__5 >= 5)
					{
						UnityEngine.Debug.LogError(string.Format("SetLobbyAttribute({0}={1}) failed after {2} attempts: {3}", "hasGameStarted", _003ChasGameStarted_003E5__2, _003Cattempt_003E5__5, ex.Message));
						goto IL_0331;
					}
					UnityEngine.Debug.LogWarning(string.Format("SetLobbyAttribute({0}={1}) attempt {2} failed: {3} - retrying", "hasGameStarted", _003ChasGameStarted_003E5__2, _003Cattempt_003E5__5, ex.Message));
					val2 = UniTask.Delay(TimeSpan.FromSeconds(2.0), true, (PlayerLoopTiming)8, default(CancellationToken));
					val = val2.GetAwaiter();
					if (!val.IsCompleted)
					{
						num = (_003C_003E1__state = 2);
						_003C_003Eu__1 = val;
						_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter, _003CLobby_UpdateCanJoinAndGameStarted_003Ed__83>(ref val, ref this);
						return;
					}
					goto IL_02f0;
				}
				goto end_IL_000e;
				IL_007e:
				try
				{
					if (num != 0)
					{
						if (num == 1)
						{
							val = _003C_003Eu__1;
							_003C_003Eu__1 = default;
							num = (_003C_003E1__state = -1);
							goto IL_0179;
						}
						val2 = ManagerBase<LobbyManager>.instance.service.SetLobbyAttribute("hasGameStarted", _003ChasGameStarted_003E5__2);
						val = val2.GetAwaiter();
						if (!val.IsCompleted)
						{
							num = (_003C_003E1__state = 0);
							_003C_003Eu__1 = val;
							_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter, _003CLobby_UpdateCanJoinAndGameStarted_003Ed__83>(ref val, ref this);
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
					val2 = ManagerBase<LobbyManager>.instance.service.SetLobbyAttribute("allowJoin", _003CallowJoin_003E5__3);
					val = val2.GetAwaiter();
					if (!val.IsCompleted)
					{
						num = (_003C_003E1__state = 1);
						_003C_003Eu__1 = val;
						_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter, _003CLobby_UpdateCanJoinAndGameStarted_003Ed__83>(ref val, ref this);
						return;
					}
					goto IL_0179;
					IL_0179:
					val.GetResult();
					UnityEngine.Debug.Log(string.Format("Lobby joinability updated: {0}={1}, allowJoin={2}", "hasGameStarted", _003ChasGameStarted_003E5__2, _003CallowJoin_003E5__3));
				}
				catch (Exception ex2)
				{
					obj = ex2;
					num2 = 1;
					goto IL_01b5;
				}
				goto IL_0331;
				IL_007a:
				num2 = 0;
				goto IL_007e;
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
	private struct _003CLobby_UpdateGameAttributes_003Ed__81 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder _003C_003Et__builder;

		public GameSettingsManager _003C_003E4__this;

		private Awaiter _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_0097: Unknown result type (might be due to invalid IL or missing references)
			//IL_009c: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
			//IL_0102: Unknown result type (might be due to invalid IL or missing references)
			//IL_015a: Unknown result type (might be due to invalid IL or missing references)
			//IL_015f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0166: Unknown result type (might be due to invalid IL or missing references)
			//IL_01be: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
			//IL_0222: Unknown result type (might be due to invalid IL or missing references)
			//IL_0227: Unknown result type (might be due to invalid IL or missing references)
			//IL_022e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0286: Unknown result type (might be due to invalid IL or missing references)
			//IL_028b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0292: Unknown result type (might be due to invalid IL or missing references)
			//IL_02ea: Unknown result type (might be due to invalid IL or missing references)
			//IL_02ef: Unknown result type (might be due to invalid IL or missing references)
			//IL_02f6: Unknown result type (might be due to invalid IL or missing references)
			//IL_034e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0353: Unknown result type (might be due to invalid IL or missing references)
			//IL_035a: Unknown result type (might be due to invalid IL or missing references)
			//IL_03b2: Unknown result type (might be due to invalid IL or missing references)
			//IL_03b7: Unknown result type (might be due to invalid IL or missing references)
			//IL_03be: Unknown result type (might be due to invalid IL or missing references)
			//IL_0417: Unknown result type (might be due to invalid IL or missing references)
			//IL_041c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0423: Unknown result type (might be due to invalid IL or missing references)
			//IL_0479: Unknown result type (might be due to invalid IL or missing references)
			//IL_047e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0485: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
			//IL_011e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0123: Unknown result type (might be due to invalid IL or missing references)
			//IL_0127: Unknown result type (might be due to invalid IL or missing references)
			//IL_012c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0182: Unknown result type (might be due to invalid IL or missing references)
			//IL_0187: Unknown result type (might be due to invalid IL or missing references)
			//IL_018b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0190: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e6: Unknown result type (might be due to invalid IL or missing references)
			//IL_01eb: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ef: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f4: Unknown result type (might be due to invalid IL or missing references)
			//IL_024a: Unknown result type (might be due to invalid IL or missing references)
			//IL_024f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0253: Unknown result type (might be due to invalid IL or missing references)
			//IL_0258: Unknown result type (might be due to invalid IL or missing references)
			//IL_02ae: Unknown result type (might be due to invalid IL or missing references)
			//IL_02b3: Unknown result type (might be due to invalid IL or missing references)
			//IL_02b7: Unknown result type (might be due to invalid IL or missing references)
			//IL_02bc: Unknown result type (might be due to invalid IL or missing references)
			//IL_0312: Unknown result type (might be due to invalid IL or missing references)
			//IL_0317: Unknown result type (might be due to invalid IL or missing references)
			//IL_031b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0320: Unknown result type (might be due to invalid IL or missing references)
			//IL_0376: Unknown result type (might be due to invalid IL or missing references)
			//IL_037b: Unknown result type (might be due to invalid IL or missing references)
			//IL_037f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0384: Unknown result type (might be due to invalid IL or missing references)
			//IL_03da: Unknown result type (might be due to invalid IL or missing references)
			//IL_03df: Unknown result type (might be due to invalid IL or missing references)
			//IL_03e3: Unknown result type (might be due to invalid IL or missing references)
			//IL_03e8: Unknown result type (might be due to invalid IL or missing references)
			//IL_043f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0444: Unknown result type (might be due to invalid IL or missing references)
			//IL_0448: Unknown result type (might be due to invalid IL or missing references)
			//IL_044d: Unknown result type (might be due to invalid IL or missing references)
			//IL_005b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			//IL_0069: Unknown result type (might be due to invalid IL or missing references)
			//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
			//IL_0140: Unknown result type (might be due to invalid IL or missing references)
			//IL_0141: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
			//IL_0208: Unknown result type (might be due to invalid IL or missing references)
			//IL_0209: Unknown result type (might be due to invalid IL or missing references)
			//IL_026c: Unknown result type (might be due to invalid IL or missing references)
			//IL_026d: Unknown result type (might be due to invalid IL or missing references)
			//IL_02d0: Unknown result type (might be due to invalid IL or missing references)
			//IL_02d1: Unknown result type (might be due to invalid IL or missing references)
			//IL_0334: Unknown result type (might be due to invalid IL or missing references)
			//IL_0335: Unknown result type (might be due to invalid IL or missing references)
			//IL_0398: Unknown result type (might be due to invalid IL or missing references)
			//IL_0399: Unknown result type (might be due to invalid IL or missing references)
			//IL_03fd: Unknown result type (might be due to invalid IL or missing references)
			//IL_03fe: Unknown result type (might be due to invalid IL or missing references)
			//IL_0462: Unknown result type (might be due to invalid IL or missing references)
			//IL_0463: Unknown result type (might be due to invalid IL or missing references)
			//IL_007d: Unknown result type (might be due to invalid IL or missing references)
			//IL_007e: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			GameSettingsManager gameSettingsManager = _003C_003E4__this;
			try
			{
				UniTask val2;
				Awaiter val;
				switch (num)
				{
				default:
					if (ManagerBase<LobbyManager>.instance.isLobbyLeader)
					{
						val2 = gameSettingsManager.Lobby_UpdateCanJoinAndGameStarted();
						val = val2.GetAwaiter();
						if (!val.IsCompleted)
						{
							num = (_003C_003E1__state = 0);
							_003C_003Eu__1 = val;
							_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter, _003CLobby_UpdateGameAttributes_003Ed__81>(ref val, ref this);
							return;
						}
						goto IL_00b2;
					}
					goto end_IL_000e;
				case 0:
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default;
					num = (_003C_003E1__state = -1);
					goto IL_00b2;
				case 1:
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default;
					num = (_003C_003E1__state = -1);
					goto IL_0111;
				case 2:
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default;
					num = (_003C_003E1__state = -1);
					goto IL_0175;
				case 3:
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default;
					num = (_003C_003E1__state = -1);
					goto IL_01d9;
				case 4:
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default;
					num = (_003C_003E1__state = -1);
					goto IL_023d;
				case 5:
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default;
					num = (_003C_003E1__state = -1);
					goto IL_02a1;
				case 6:
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default;
					num = (_003C_003E1__state = -1);
					goto IL_0305;
				case 7:
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default;
					num = (_003C_003E1__state = -1);
					goto IL_0369;
				case 8:
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default;
					num = (_003C_003E1__state = -1);
					goto IL_03cd;
				case 9:
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default;
					num = (_003C_003E1__state = -1);
					goto IL_0432;
				case 10:
					{
						val = _003C_003Eu__1;
						_003C_003Eu__1 = default;
						num = (_003C_003E1__state = -1);
						break;
					}
					IL_023d:
					val.GetResult();
					val2 = gameSettingsManager.Lobby_UpdateAttribute("lobbyName");
					val = val2.GetAwaiter();
					if (!val.IsCompleted)
					{
						num = (_003C_003E1__state = 5);
						_003C_003Eu__1 = val;
						_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter, _003CLobby_UpdateGameAttributes_003Ed__81>(ref val, ref this);
						return;
					}
					goto IL_02a1;
					IL_0369:
					val.GetResult();
					val2 = gameSettingsManager.Lobby_UpdateAttribute("bannedGameItems");
					val = val2.GetAwaiter();
					if (!val.IsCompleted)
					{
						num = (_003C_003E1__state = 8);
						_003C_003Eu__1 = val;
						_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter, _003CLobby_UpdateGameAttributes_003Ed__81>(ref val, ref this);
						return;
					}
					goto IL_03cd;
					IL_00b2:
					val.GetResult();
					val2 = gameSettingsManager.Lobby_UpdateGameStartTimestamp();
					val = val2.GetAwaiter();
					if (!val.IsCompleted)
					{
						num = (_003C_003E1__state = 1);
						_003C_003Eu__1 = val;
						_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter, _003CLobby_UpdateGameAttributes_003Ed__81>(ref val, ref this);
						return;
					}
					goto IL_0111;
					IL_02a1:
					val.GetResult();
					val2 = gameSettingsManager.Lobby_UpdateAttribute("lobbyDescription");
					val = val2.GetAwaiter();
					if (!val.IsCompleted)
					{
						num = (_003C_003E1__state = 6);
						_003C_003Eu__1 = val;
						_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter, _003CLobby_UpdateGameAttributes_003Ed__81>(ref val, ref this);
						return;
					}
					goto IL_0305;
					IL_0111:
					val.GetResult();
					val2 = gameSettingsManager.Lobby_UpdateAttribute("allowMidJoins");
					val = val2.GetAwaiter();
					if (!val.IsCompleted)
					{
						num = (_003C_003E1__state = 2);
						_003C_003Eu__1 = val;
						_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter, _003CLobby_UpdateGameAttributes_003Ed__81>(ref val, ref this);
						return;
					}
					goto IL_0175;
					IL_0432:
					val.GetResult();
					val2 = gameSettingsManager.Lobby_UpdateAttribute("difficulty");
					val = val2.GetAwaiter();
					if (!val.IsCompleted)
					{
						num = (_003C_003E1__state = 10);
						_003C_003Eu__1 = val;
						_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter, _003CLobby_UpdateGameAttributes_003Ed__81>(ref val, ref this);
						return;
					}
					break;
					IL_0175:
					val.GetResult();
					val2 = gameSettingsManager.Lobby_UpdateAttribute("allowDejavu");
					val = val2.GetAwaiter();
					if (!val.IsCompleted)
					{
						num = (_003C_003E1__state = 3);
						_003C_003Eu__1 = val;
						_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter, _003CLobby_UpdateGameAttributes_003Ed__81>(ref val, ref this);
						return;
					}
					goto IL_01d9;
					IL_0305:
					val.GetResult();
					val2 = gameSettingsManager.Lobby_UpdateAttribute("activeLucidDreams");
					val = val2.GetAwaiter();
					if (!val.IsCompleted)
					{
						num = (_003C_003E1__state = 7);
						_003C_003Eu__1 = val;
						_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter, _003CLobby_UpdateGameAttributes_003Ed__81>(ref val, ref this);
						return;
					}
					goto IL_0369;
					IL_01d9:
					val.GetResult();
					val2 = gameSettingsManager.Lobby_UpdateAttribute("maxPlayers");
					val = val2.GetAwaiter();
					if (!val.IsCompleted)
					{
						num = (_003C_003E1__state = 4);
						_003C_003Eu__1 = val;
						_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter, _003CLobby_UpdateGameAttributes_003Ed__81>(ref val, ref this);
						return;
					}
					goto IL_023d;
					IL_03cd:
					val.GetResult();
					val2 = gameSettingsManager.Lobby_UpdateAttribute("lobbyTags");
					val = val2.GetAwaiter();
					if (!val.IsCompleted)
					{
						num = (_003C_003E1__state = 9);
						_003C_003Eu__1 = val;
						_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter, _003CLobby_UpdateGameAttributes_003Ed__81>(ref val, ref this);
						return;
					}
					goto IL_0432;
				}
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
	private struct _003CLobby_UpdateGameStartTimestamp_003Ed__84 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder _003C_003Et__builder;

		private Awaiter _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_008a: Unknown result type (might be due to invalid IL or missing references)
			//IL_008f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0096: Unknown result type (might be due to invalid IL or missing references)
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			//IL_005f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0073: Unknown result type (might be due to invalid IL or missing references)
			//IL_0074: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			try
			{
				Awaiter val2;
				if (num != 0)
				{
					long num2 = ((!((UnityEngine.Object)(object)NetworkedManagerBase<GameManager>.instance == null)) ? DateTime.UtcNow.AddSeconds(0f - NetworkedManagerBase<GameManager>.instance.elapsedGameTime).ToTimestamp() : 0);
					UniTask val = ManagerBase<LobbyManager>.instance.service.SetLobbyAttribute("gameStartTimestamp", num2);
					val2 = val.GetAwaiter();
					if (!val2.IsCompleted)
					{
						num = (_003C_003E1__state = 0);
						_003C_003Eu__1 = val2;
						_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter, _003CLobby_UpdateGameStartTimestamp_003Ed__84>(ref val2, ref this);
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

	[CompilerGenerated]
	[SyncVar]
	private string gameSettingsSaveKey__BackingField = "default";

	[CompilerGenerated]
	[SyncVar(hook = "OnStateChanged")]
	private GameState state__BackingField;

	public SafeAction ClientEvent_OnStateChanged;

	[SyncVar]
	private AllowMidJoinType _allowMidJoins;

	[SyncVar]
	private bool _allowDejavu;

	[CompilerGenerated]
	[SyncVar]
	private bool enableVotes__BackingField = true;

	[SyncVar(hook = "OnDifficultyChanged")]
	private string _difficulty = "diffNormal";

	public SafeAction<string, string> ClientEvent_OnDifficultyChanged;

	[SaveVar(SaveVarFlags.Default)]
	public readonly SyncList<string> availableLucidDreams = new SyncList<string>();

	public SafeAction ClientEvent_OnAvailableLucidDreamsChanged;

	[SaveVar(SaveVarFlags.Default)]
	public readonly SyncList<string> activeLucidDreams = new SyncList<string>();

	public SafeAction ClientEvent_OnActiveLucidDreamsChanged;

	[SaveVar(SaveVarFlags.Default)]
	public readonly SyncList<string> bannedGameItems = new SyncList<string>();

	public SafeAction ClientEvent_OnBannedItemsChanged;

	[SaveVar(SaveVarFlags.Default)]
	public readonly SyncList<string> unlockedGameItems = new SyncList<string>();

	public SafeAction ClientEvent_OnUnlockedGameItemsChanged;

	[SaveVar(SaveVarFlags.Default)]
	public readonly SyncList<string> lobbyTags = new SyncList<string>();

	public SafeAction ClientEvent_OnLobbyTagsChanged;

	[SyncVar]
	private MidJoinBanType _midJoinBanType;

	[CompilerGenerated]
	[SyncVar]
	[SaveVar(SaveVarFlags.Default)]
	private MidJoinWaitType midJoinWaitType__BackingField;

	[SaveVar(SaveVarFlags.Default)]
	public readonly SyncList<string> addedGameMods = new SyncList<string>();

	[SaveVar(SaveVarFlags.Default)]
	public readonly SyncDictionary<string, string> customData = new SyncDictionary<string, string>();

	public SafeAction<string> ClientEvent_OnCustomDataChanged;

	[SyncVar]
	private int _maxPlayers = 4;

	[SyncVar(hook = "OnLobbyNameChanged")]
	private string _lobbyName = "";

	public SafeAction<string, string> ClientEvent_OnLobbyNameChanged;

	[SyncVar]
	private string _lobbyDescription = "";

	public int localPlayerDejavuCost;

	private static int _canJoinUpdateVersion;

	public Action<GameState, GameState> _Mirror_SyncVarHookDelegate__003Cstate_003Ek__BackingField;

	public Action<string, string> _Mirror_SyncVarHookDelegate__difficulty;

	public Action<string, string> _Mirror_SyncVarHookDelegate__lobbyName;

	[SaveVar(SaveVarFlags.Default)]
	public string gameSettingsSaveKey
	{
		[CompilerGenerated]
		get
		{
			return gameSettingsSaveKey__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003CgameSettingsSaveKey_003Ek__BackingField = value;
		}
	}

	public GameState state
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

	[SaveVar(SaveVarFlags.Default)]
	public AllowMidJoinType allowMidJoins
	{
		get
		{
			return _allowMidJoins;
		}
		set
		{
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			Network_allowMidJoins = value;
			Lobby_UpdateAttribute("allowMidJoins");
			Lobby_UpdateCanJoinAndGameStarted();
		}
	}

	[SaveVar(SaveVarFlags.Default)]
	public bool allowDejavu
	{
		get
		{
			return _allowDejavu;
		}
		set
		{
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			Network_allowDejavu = value;
			Lobby_UpdateAttribute("allowDejavu");
		}
	}

	[SaveVar(SaveVarFlags.Default)]
	public bool enableVotes
	{
		[CompilerGenerated]
		get
		{
			return enableVotes__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003CenableVotes_003Ek__BackingField = value;
		}
	}

	[SaveVar(SaveVarFlags.Default)]
	public string difficulty
	{
		get
		{
			return _difficulty;
		}
		set
		{
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			Network_difficulty = value;
			Lobby_UpdateAttribute("difficulty");
		}
	}

	[SaveVar(SaveVarFlags.Default)]
	public MidJoinBanType midJoinBanType
	{
		get
		{
			return _midJoinBanType;
		}
		set
		{
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			Network_midJoinBanType = value;
			Lobby_UpdateCanJoinAndGameStarted();
		}
	}

	public MidJoinWaitType midJoinWaitType
	{
		[CompilerGenerated]
		get
		{
			return midJoinWaitType__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003CmidJoinWaitType_003Ek__BackingField = value;
		}
	}

	[SaveVar(SaveVarFlags.Default)]
	public int maxPlayers
	{
		get
		{
			return _maxPlayers;
		}
		set
		{
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			Network_maxPlayers = value;
			Lobby_UpdateAttribute("maxPlayers");
		}
	}

	[SaveVar(SaveVarFlags.Default)]
	public string lobbyName
	{
		get
		{
			return _lobbyName;
		}
		set
		{
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			Network_lobbyName = value;
			if (string.IsNullOrWhiteSpace(_lobbyName))
			{
				Network_lobbyName = LobbyManager.GetDefaultLobbyName();
			}
			Lobby_UpdateAttribute("lobbyName");
		}
	}

	[SaveVar(SaveVarFlags.Default)]
	public string lobbyDescription
	{
		get
		{
			return _lobbyDescription;
		}
		set
		{
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			Network_lobbyDescription = value;
			Lobby_UpdateAttribute("lobbyDescription");
		}
	}

	public string Network_003CgameSettingsSaveKey_003Ek__BackingField
	{
		get
		{
			return gameSettingsSaveKey__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<string>(value, ref gameSettingsSaveKey__BackingField, 1uL, (Action<string, string>)null);
		}
	}

	public GameState Network_003Cstate_003Ek__BackingField
	{
		get
		{
			return state__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<GameState>(value, ref state__BackingField, 2uL, _Mirror_SyncVarHookDelegate__003Cstate_003Ek__BackingField);
		}
	}

	public AllowMidJoinType Network_allowMidJoins
	{
		get
		{
			return _allowMidJoins;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<AllowMidJoinType>(value, ref _allowMidJoins, 4uL, (Action<AllowMidJoinType, AllowMidJoinType>)null);
		}
	}

	public bool Network_allowDejavu
	{
		get
		{
			return _allowDejavu;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<bool>(value, ref _allowDejavu, 8uL, (Action<bool, bool>)null);
		}
	}

	public bool Network_003CenableVotes_003Ek__BackingField
	{
		get
		{
			return enableVotes__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<bool>(value, ref enableVotes__BackingField, 16uL, (Action<bool, bool>)null);
		}
	}

	public string Network_difficulty
	{
		get
		{
			return _difficulty;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<string>(value, ref _difficulty, 32uL, _Mirror_SyncVarHookDelegate__difficulty);
		}
	}

	public MidJoinBanType Network_midJoinBanType
	{
		get
		{
			return _midJoinBanType;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<MidJoinBanType>(value, ref _midJoinBanType, 64uL, (Action<MidJoinBanType, MidJoinBanType>)null);
		}
	}

	public MidJoinWaitType Network_003CmidJoinWaitType_003Ek__BackingField
	{
		get
		{
			return midJoinWaitType__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<MidJoinWaitType>(value, ref midJoinWaitType__BackingField, 128uL, (Action<MidJoinWaitType, MidJoinWaitType>)null);
		}
	}

	public int Network_maxPlayers
	{
		get
		{
			return _maxPlayers;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<int>(value, ref _maxPlayers, 256uL, (Action<int, int>)null);
		}
	}

	public string Network_lobbyName
	{
		get
		{
			return _lobbyName;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<string>(value, ref _lobbyName, 512uL, _Mirror_SyncVarHookDelegate__lobbyName);
		}
	}

	public string Network_lobbyDescription
	{
		get
		{
			return _lobbyDescription;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<string>(value, ref _lobbyDescription, 1024uL, (Action<string, string>)null);
		}
	}

	protected override void Awake()
	{
		base.Awake();
		if (!string.IsNullOrEmpty(DewNetworkManager.startSettings.customGameSettingsSaveKey))
		{
			Network_003CgameSettingsSaveKey_003Ek__BackingField = DewNetworkManager.startSettings.customGameSettingsSaveKey;
		}
		activeLucidDreams.Callback += (Operation<string> op, int index, string item, string newItem) =>
		{
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			Dew.Debounce((UnityEngine.Object)(object)this, 0f, () =>
			{
				ClientEvent_OnActiveLucidDreamsChanged?.Invoke();
			}, "GameSettingsManager::activeLucidDreams");
			if (ManagerBase<LobbyManager>.instance.isLobbyLeader)
			{
				Lobby_UpdateAttribute("activeLucidDreams");
			}
		};
		bannedGameItems.Callback += (Operation<string> op, int index, string item, string newItem) =>
		{
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			Dew.Debounce((UnityEngine.Object)(object)this, 0f, () =>
			{
				ClientEvent_OnBannedItemsChanged?.Invoke();
			}, "GameSettingsManager::bannedGameItems");
			if (ManagerBase<LobbyManager>.instance.isLobbyLeader)
			{
				Lobby_UpdateAttribute("bannedGameItems");
			}
		};
		unlockedGameItems.Callback += (Operation<string> op, int index, string item, string newItem) =>
		{
			Dew.Debounce((UnityEngine.Object)(object)this, 0f, () =>
			{
				ClientEvent_OnUnlockedGameItemsChanged?.Invoke();
			}, "GameSettingsManager::unlockedGameItems");
		};
		lobbyTags.Callback += (Operation<string> op, int index, string item, string newItem) =>
		{
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			Dew.Debounce((UnityEngine.Object)(object)this, 0f, () =>
			{
				ClientEvent_OnLobbyTagsChanged?.Invoke();
			}, "GameSettingsManager::lobbyTags");
			if (ManagerBase<LobbyManager>.instance.isLobbyLeader)
			{
				Lobby_UpdateAttribute("lobbyTags");
			}
		};
		availableLucidDreams.Callback += (Operation<string> op, int index, string item, string newItem) =>
		{
			Dew.Debounce((UnityEngine.Object)(object)this, 0.1f, () =>
			{
				ClientEvent_OnAvailableLucidDreamsChanged?.Invoke();
			});
		};
		((SyncIDictionary<string, string>)(object)customData).Callback += (Operation<string, string> op, string key, string item) =>
		{
			Dew.Debounce((UnityEngine.Object)(object)this, 0f, () =>
			{
				ClientEvent_OnCustomDataChanged?.Invoke(key);
			}, "GameSettingsManager::customData::" + key);
		};
	}

	public override void OnStartServer()
	{
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		base.OnStartServer();
		DewPlayer.onHumanPlayerRemoved += new Action<DewPlayer>(OnHumanPlayerRemoved);
		lobbyName = DewNetworkManager.startSettings.lobbyName;
		maxPlayers = DewNetworkManager.startSettings.maxPlayers;
		if (DewNetworkManager.startSettings.continueData == null)
		{
			GetLocalPreferredGameSettings().ApplyToGame();
			addedGameMods.AddRange((IEnumerable<string>)DewNetworkManager.startSettings.addedGameMods);
			foreach (GameModifierBase item in DewResources.FindAllByType<GameModifierBase>(ResourceLoadSettings.Light))
			{
				string name = ((object)item).GetType().Name;
				if (Dew.IsGameModifierIncludedInGame(name) && !Dew.IsExcludedFromPool(name) && !addedGameMods.Contains(name))
				{
					addedGameMods.Add(name);
				}
			}
		}
		ClientEvent_OnUnlockedGameItemsChanged += new Action(ClientEventOnUnlockedGameItemsChangedServer);
		Lobby_UpdateGameAttributes();
		Lobby_UpdateCanJoinAndGameStarted(DewNetworkManager.startSettings.continueData != null);
	}

	public PreferredGameSettings GetLocalPreferredGameSettings()
	{
		return DewSave.profileMain.GetPreferredGameSettings(gameSettingsSaveKey);
	}

	public override void OnStartClient()
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		base.OnStartClient();
		OnDifficultyChanged(null, difficulty);
		ClientEvent_OnActiveLucidDreamsChanged?.Invoke();
		if (DewNetworkManager.startSettings.continueData != null)
		{
			return;
		}
		bool flag = ManagerBase<PlayLobbyManager>.instance != null;
		Enumerator<string> enumerator = addedGameMods.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				string current = enumerator.Current;
				if (string.IsNullOrEmpty(current))
				{
					continue;
				}
				GameModifierBase byShortTypeName = DewResources.GetByShortTypeName<GameModifierBase>(current, default(ResourceLoadSettings));
				if ((UnityEngine.Object)(object)byShortTypeName == null || !flag)
				{
					continue;
				}
				if (((NetworkBehaviour)this).isServer)
				{
					try
					{
						byShortTypeName.OnStartServerLobby();
					}
					catch (Exception exception)
					{
						UnityEngine.Debug.LogException(exception);
					}
				}
				try
				{
					byShortTypeName.OnStartClientLobby();
				}
				catch (Exception exception2)
				{
					UnityEngine.Debug.LogException(exception2);
				}
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
		if (activeLucidDreams.Count > 0)
		{
			Dew.Debounce((UnityEngine.Object)(object)this, 0f, () =>
			{
				ClientEvent_OnActiveLucidDreamsChanged?.Invoke();
			}, "GameSettingsManager::activeLucidDreams");
		}
		if (bannedGameItems.Count > 0)
		{
			Dew.Debounce((UnityEngine.Object)(object)this, 0f, () =>
			{
				ClientEvent_OnBannedItemsChanged?.Invoke();
			}, "GameSettingsManager::bannedGameItems");
		}
		if (unlockedGameItems.Count > 0)
		{
			Dew.Debounce((UnityEngine.Object)(object)this, 0f, () =>
			{
				ClientEvent_OnUnlockedGameItemsChanged?.Invoke();
			}, "GameSettingsManager::unlockedGameItems");
		}
		if (lobbyTags.Count > 0)
		{
			Dew.Debounce((UnityEngine.Object)(object)this, 0f, () =>
			{
				ClientEvent_OnLobbyTagsChanged?.Invoke();
			}, "GameSettingsManager::lobbyTags");
		}
		if (availableLucidDreams.Count > 0)
		{
			Dew.Debounce((UnityEngine.Object)(object)this, 0.1f, () =>
			{
				ClientEvent_OnAvailableLucidDreamsChanged?.Invoke();
			});
		}
		if (((SyncIDictionary<string, string>)(object)customData).Count <= 0)
		{
			return;
		}
		foreach (string key in customData.Keys)
		{
			Dew.Debounce((UnityEngine.Object)(object)this, 0f, () =>
			{
				ClientEvent_OnCustomDataChanged?.Invoke(key);
			}, "GameSettingsManager::customData::" + key);
		}
	}

	public override void OnStopServer()
	{
		base.OnStopServer();
		DewPlayer.onHumanPlayerRemoved -= new Action<DewPlayer>(OnHumanPlayerRemoved);
	}

	[Server]
	public void UpdateAvailableLucidDreams()
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogWarning("[Server] function 'System.Void GameSettingsManager::UpdateAvailableLucidDreams()' called when server was not active");
			return;
		}
		availableLucidDreams.Clear();
		foreach (DewPlayer allHumanPlayer in DewPlayer.allHumanPlayers)
		{
			foreach (string availableLucidDream in allHumanPlayer.availableLucidDreams)
			{
				if (!availableLucidDreams.Contains(availableLucidDream))
				{
					availableLucidDreams.Add(availableLucidDream);
				}
			}
		}
	}

	[Server]
	public void AddLucidDream(string type)
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogWarning("[Server] function 'System.Void GameSettingsManager::AddLucidDream(System.String)' called when server was not active");
		}
		else if (!((UnityEngine.Object)(object)DewResources.GetByShortTypeName<LucidDream>(type, default(ResourceLoadSettings)) == null) && !activeLucidDreams.Contains(type))
		{
			List<string> list = new List<string>((IEnumerable<string>)activeLucidDreams);
			list.Add(type);
			activeLucidDreams.Clear();
			activeLucidDreams.AddRange((IEnumerable<string>)(from l in list
				orderby DewResources.GetByShortTypeName<LucidDream>(l, default(ResourceLoadSettings)).type, l
				select l));
		}
	}

	[Server]
	public void RemoveLucidDream(string type)
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogWarning("[Server] function 'System.Void GameSettingsManager::RemoveLucidDream(System.String)' called when server was not active");
		}
		else
		{
			activeLucidDreams.Remove(type);
		}
	}

	[Server]
	public void ClearLucidDreams()
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogWarning("[Server] function 'System.Void GameSettingsManager::ClearLucidDreams()' called when server was not active");
		}
		else
		{
			activeLucidDreams.Clear();
		}
	}

	[Server]
	public void SetEnableVotes(bool value)
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogWarning("[Server] function 'System.Void GameSettingsManager::SetEnableVotes(System.Boolean)' called when server was not active");
		}
		else
		{
			Network_003CenableVotes_003Ek__BackingField = value;
		}
	}

	public override void LogicUpdate(float dt)
	{
		base.LogicUpdate(dt);
		if (!NetworkServer.active && !((UnityEngine.Object)(object)DewPlayer.local == null) && !DewNetworkManager.instance.didRegisterError && DewPlayer.local.state == PlayerState.InLobby)
		{
			if (midJoinBanType != MidJoinBanType.None)
			{
				DisconnectByMidJoinBan();
			}
			else if (DewPlayer.local.isLoadingForMidJoin && midJoinWaitType != MidJoinWaitType.None)
			{
				DisconnectByMidJoinWaitBlock();
			}
		}
	}

	public void DisconnectByMidJoinBan()
	{
		DewNetworkManager.instance.didRegisterError = true;
		switch (midJoinBanType)
		{
		case MidJoinBanType.GameHasEnded:
			DewSessionError.ShowError(new DewException(DewExceptionType.GameHasEnded));
			break;
		case MidJoinBanType.TooLateToJoin:
			DewSessionError.ShowError(new DewException(DewExceptionType.TooLateToMidJoin));
			break;
		default:
			DewSessionError.ShowError(new DewException(DewExceptionType.Disconnected));
			break;
		}
		DewNetworkManager.instance.EndSession();
	}

	public void DisconnectByMidJoinWaitBlock()
	{
		DewNetworkManager.instance.didRegisterError = true;
		switch (midJoinWaitType)
		{
		case MidJoinWaitType.BeforeBossFight:
			DewSessionError.ShowError(new DewException(DewExceptionType.MidJoinBlockedBeforeBossFight));
			break;
		case MidJoinWaitType.FightingBoss:
			DewSessionError.ShowError(new DewException(DewExceptionType.MidJoinBlockedFightingBoss));
			break;
		case MidJoinWaitType.AfterBossFight:
			DewSessionError.ShowError(new DewException(DewExceptionType.MidJoinBlockedAfterBossFight));
			break;
		default:
			DewSessionError.ShowError(new DewException(DewExceptionType.Disconnected));
			break;
		}
		DewNetworkManager.instance.EndSession();
	}

	internal void UpdateUnlockedGameItems()
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		HashSet<string> hashSet = new HashSet<string>();
		foreach (DewPlayer allHumanPlayer in DewPlayer.allHumanPlayers)
		{
			Enumerator<string> enumerator2 = allHumanPlayer.unlockedGameItems.GetEnumerator();
			try
			{
				while (enumerator2.MoveNext())
				{
					string current = enumerator2.Current;
					hashSet.Add(current);
				}
			}
			finally
			{
				((IDisposable)enumerator2/*cast due to constrained. prefix*/).Dispose();
			}
		}
		unlockedGameItems.Clear();
		unlockedGameItems.AddRange((IEnumerable<string>)hashSet);
	}

	private void OnHumanPlayerRemoved(DewPlayer obj)
	{
		UpdateUnlockedGameItems();
	}

	private void ClientEventOnUnlockedGameItemsChangedServer()
	{
		ValidateBannedItems(printMessage: false);
	}

	public void ValidateBannedItems(bool printMessage)
	{
		//IL_0236: Unknown result type (might be due to invalid IL or missing references)
		//IL_023b: Unknown result type (might be due to invalid IL or missing references)
		for (int num = bannedGameItems.Count - 1; num >= 0; num--)
		{
			string text = bannedGameItems[num];
			DewProfile.UnlockData value;
			DewProfile.UnlockData value2;
			if (!unlockedGameItems.Contains(text))
			{
				bannedGameItems.RemoveAt(num);
			}
			else if (DewSave.profileMain.skills.TryGetValue(text, out value) && value.status != UnlockStatus.Complete)
			{
				bannedGameItems.RemoveAt(num);
			}
			else if (DewSave.profileMain.gems.TryGetValue(text, out value2) && value2.status != UnlockStatus.Complete)
			{
				bannedGameItems.RemoveAt(num);
			}
		}
		int obliterationMinAllowedHeroes = Dew.GetObliterationMinAllowedHeroes();
		int num2 = ((IEnumerable<string>)unlockedGameItems).Count((string i) => i.StartsWith("Hero_")) - ((IEnumerable<string>)bannedGameItems).Count((string i) => i.StartsWith("Hero_"));
		if (num2 < obliterationMinAllowedHeroes)
		{
			if (printMessage)
			{
				ManagerBase<MessageManager>.instance.ShowMessage(new DewMessageSettings
				{
					owner = (UnityEngine.Object)(object)this,
					buttons = DewMessageSettings.ButtonType.Ok,
					rawContent = DewLocalization.GetUIValue("MasteryReward_Traveler") + ": " + string.Format(DewLocalization.GetUIValue("Obliteration_MinRequirement"), obliterationMinAllowedHeroes)
				});
			}
			while (num2 < obliterationMinAllowedHeroes)
			{
				bool flag = false;
				for (int num3 = bannedGameItems.Count - 1; num3 >= 0; num3--)
				{
					if (bannedGameItems[num3].StartsWith("Hero_"))
					{
						bannedGameItems.RemoveAt(num3);
						num2++;
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					break;
				}
			}
		}
		if (NetworkServer.dontListen)
		{
			for (int num4 = bannedGameItems.Count - 1; num4 >= 0; num4--)
			{
				if (bannedGameItems[num4].StartsWith("Hero_"))
				{
					bannedGameItems.RemoveAt(num4);
				}
			}
		}
		int minAllowedItems = Dew.GetObliterationMinAllowedItemsPerRarity();
		Dictionary<string, Rarity> skills = new Dictionary<string, Rarity>();
		Dictionary<string, Rarity> gems = new Dictionary<string, Rarity>();
		Enumerator<string> enumerator = unlockedGameItems.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				string current = enumerator.Current;
				if (current.StartsWith("Gem_"))
				{
					Gem byShortTypeName = DewResources.GetByShortTypeName<Gem>(current, ResourceLoadSettings.Light);
					if (!((UnityEngine.Object)(object)byShortTypeName == null))
					{
						gems[current] = byShortTypeName.rarity;
					}
				}
				else if (current.StartsWith("St_"))
				{
					SkillTrigger byShortTypeName2 = DewResources.GetByShortTypeName<SkillTrigger>(current, ResourceLoadSettings.Light);
					if (!((UnityEngine.Object)(object)byShortTypeName2 == null))
					{
						skills[current] = byShortTypeName2.rarity;
					}
				}
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
		ValidateItemsByRarity(Rarity.Common, isSkill: true);
		ValidateItemsByRarity(Rarity.Common, isSkill: false);
		ValidateItemsByRarity(Rarity.Rare, isSkill: true);
		ValidateItemsByRarity(Rarity.Rare, isSkill: false);
		ValidateItemsByRarity(Rarity.Epic, isSkill: true);
		ValidateItemsByRarity(Rarity.Epic, isSkill: false);
		ValidateItemsByRarity(Rarity.Legendary, isSkill: true);
		ValidateItemsByRarity(Rarity.Legendary, isSkill: false);
		ValidateItemsByRarity(Rarity.Unique, isSkill: true);
		ValidateItemsByRarity(Rarity.Unique, isSkill: false);
		void ValidateItemsByRarity(Rarity r, bool isSkill)
		{
			Dictionary<string, Rarity> list = (isSkill ? skills : gems);
			int num5 = ((IEnumerable<string>)unlockedGameItems).Count((string i) => list.TryGetValue(i, out var value4) && value4 == r);
			int num6 = ((IEnumerable<string>)bannedGameItems).Count((string i) => list.TryGetValue(i, out var value4) && value4 == r);
			int num7 = num5 - num6;
			if (num7 < minAllowedItems)
			{
				while (num7 < minAllowedItems)
				{
					bool flag2 = false;
					for (int num8 = bannedGameItems.Count - 1; num8 >= 0; num8--)
					{
						if (list.TryGetValue(bannedGameItems[num8], out var value3) && value3 == r)
						{
							bannedGameItems.RemoveAt(num8);
							num7++;
							flag2 = true;
							if (printMessage)
							{
								printMessage = false;
								ManagerBase<MessageManager>.instance.ShowMessage(new DewMessageSettings
								{
									owner = (UnityEngine.Object)(object)this,
									buttons = DewMessageSettings.ButtonType.Ok,
									rawContent = DewLocalization.GetUIValue(string.Format("InGame_Tooltip_{0}{1}", r, isSkill ? "Skill" : "Essence")) + ": " + string.Format(DewLocalization.GetUIValue("Obliteration_MinRequirement"), minAllowedItems)
								});
							}
							break;
						}
					}
					if (!flag2)
					{
						break;
					}
				}
			}
		}
	}

	private void OnStateChanged(GameState _, GameState __)
	{
		ClientEvent_OnStateChanged?.Invoke();
	}

	private void OnDifficultyChanged(string oldName, string newName)
	{
		ClientEvent_OnDifficultyChanged?.Invoke(oldName, newName);
	}

	private void OnLobbyNameChanged(string oldName, string newName)
	{
		ClientEvent_OnLobbyNameChanged?.Invoke(oldName, newName);
	}

	[AsyncStateMachine(typeof(_003CLobby_UpdateGameAttributes_003Ed__81))]
	[Server]
	public UniTask Lobby_UpdateGameAttributes()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogWarning("[Server] function 'Cysharp.Threading.Tasks.UniTask GameSettingsManager::Lobby_UpdateGameAttributes()' called when server was not active");
			return default;
		}
		_003CLobby_UpdateGameAttributes_003Ed__81 obj = default;
		obj._003C_003Et__builder = AsyncUniTaskMethodBuilder.Create();
		obj._003C_003E4__this = this;
		obj._003C_003E1__state = -1;
		obj._003C_003Et__builder.Start<_003CLobby_UpdateGameAttributes_003Ed__81>(ref obj);
		return obj._003C_003Et__builder.Task;
	}

	[AsyncStateMachine(typeof(_003CLobby_UpdateCanJoinAndGameStarted_003Ed__83))]
	[Server]
	public UniTask Lobby_UpdateCanJoinAndGameStarted(bool? hasGameStartedOverride = null)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogWarning("[Server] function 'Cysharp.Threading.Tasks.UniTask GameSettingsManager::Lobby_UpdateCanJoinAndGameStarted(System.Nullable`1<System.Boolean>)' called when server was not active");
			return default;
		}
		_003CLobby_UpdateCanJoinAndGameStarted_003Ed__83 obj = default;
		obj._003C_003Et__builder = AsyncUniTaskMethodBuilder.Create();
		obj._003C_003E4__this = this;
		obj.hasGameStartedOverride = hasGameStartedOverride;
		obj._003C_003E1__state = -1;
		obj._003C_003Et__builder.Start<_003CLobby_UpdateCanJoinAndGameStarted_003Ed__83>(ref obj);
		return obj._003C_003Et__builder.Task;
	}

	[AsyncStateMachine(typeof(_003CLobby_UpdateGameStartTimestamp_003Ed__84))]
	[Server]
	public UniTask Lobby_UpdateGameStartTimestamp()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogWarning("[Server] function 'Cysharp.Threading.Tasks.UniTask GameSettingsManager::Lobby_UpdateGameStartTimestamp()' called when server was not active");
			return default;
		}
		_003CLobby_UpdateGameStartTimestamp_003Ed__84 obj = default;
		obj._003C_003Et__builder = AsyncUniTaskMethodBuilder.Create();
		obj._003C_003E1__state = -1;
		obj._003C_003Et__builder.Start<_003CLobby_UpdateGameStartTimestamp_003Ed__84>(ref obj);
		return obj._003C_003Et__builder.Task;
	}

	[AsyncStateMachine(typeof(_003CLobby_UpdateAttribute_003Ed__85))]
	[Server]
	public UniTask Lobby_UpdateAttribute(string key)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogWarning("[Server] function 'Cysharp.Threading.Tasks.UniTask GameSettingsManager::Lobby_UpdateAttribute(System.String)' called when server was not active");
			return default;
		}
		_003CLobby_UpdateAttribute_003Ed__85 obj = default;
		obj._003C_003Et__builder = AsyncUniTaskMethodBuilder.Create();
		obj._003C_003E4__this = this;
		obj.key = key;
		obj._003C_003E1__state = -1;
		obj._003C_003Et__builder.Start<_003CLobby_UpdateAttribute_003Ed__85>(ref obj);
		return obj._003C_003Et__builder.Task;
	}

	public GameSettingsManager()
	{
		((NetworkBehaviour)this).InitSyncObject((SyncObject)(object)availableLucidDreams);
		((NetworkBehaviour)this).InitSyncObject((SyncObject)(object)activeLucidDreams);
		((NetworkBehaviour)this).InitSyncObject((SyncObject)(object)bannedGameItems);
		((NetworkBehaviour)this).InitSyncObject((SyncObject)(object)unlockedGameItems);
		((NetworkBehaviour)this).InitSyncObject((SyncObject)(object)lobbyTags);
		((NetworkBehaviour)this).InitSyncObject((SyncObject)(object)addedGameMods);
		((NetworkBehaviour)this).InitSyncObject((SyncObject)(object)customData);
		_Mirror_SyncVarHookDelegate__003Cstate_003Ek__BackingField = OnStateChanged;
		_Mirror_SyncVarHookDelegate__difficulty = OnDifficultyChanged;
		_Mirror_SyncVarHookDelegate__lobbyName = OnLobbyNameChanged;
	}

	private void MirrorProcessed()
	{
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		((NetworkBehaviour)this).SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			NetworkWriterExtensions.WriteString(writer, gameSettingsSaveKey__BackingField);
			GeneratedNetworkCode._Write_GameState(writer, state__BackingField);
			GeneratedNetworkCode._Write_AllowMidJoinType(writer, _allowMidJoins);
			NetworkWriterExtensions.WriteBool(writer, _allowDejavu);
			NetworkWriterExtensions.WriteBool(writer, enableVotes__BackingField);
			NetworkWriterExtensions.WriteString(writer, _difficulty);
			GeneratedNetworkCode._Write_MidJoinBanType(writer, _midJoinBanType);
			GeneratedNetworkCode._Write_MidJoinWaitType(writer, midJoinWaitType__BackingField);
			NetworkWriterExtensions.WriteInt(writer, _maxPlayers);
			NetworkWriterExtensions.WriteString(writer, _lobbyName);
			NetworkWriterExtensions.WriteString(writer, _lobbyDescription);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 1L) != 0L)
		{
			NetworkWriterExtensions.WriteString(writer, gameSettingsSaveKey__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 2L) != 0L)
		{
			GeneratedNetworkCode._Write_GameState(writer, state__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 4L) != 0L)
		{
			GeneratedNetworkCode._Write_AllowMidJoinType(writer, _allowMidJoins);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 8L) != 0L)
		{
			NetworkWriterExtensions.WriteBool(writer, _allowDejavu);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x10L) != 0L)
		{
			NetworkWriterExtensions.WriteBool(writer, enableVotes__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x20L) != 0L)
		{
			NetworkWriterExtensions.WriteString(writer, _difficulty);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x40L) != 0L)
		{
			GeneratedNetworkCode._Write_MidJoinBanType(writer, _midJoinBanType);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x80L) != 0L)
		{
			GeneratedNetworkCode._Write_MidJoinWaitType(writer, midJoinWaitType__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x100L) != 0L)
		{
			NetworkWriterExtensions.WriteInt(writer, _maxPlayers);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x200L) != 0L)
		{
			NetworkWriterExtensions.WriteString(writer, _lobbyName);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x400L) != 0L)
		{
			NetworkWriterExtensions.WriteString(writer, _lobbyDescription);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		((NetworkBehaviour)this).DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<string>(ref gameSettingsSaveKey__BackingField, (Action<string, string>)null, NetworkReaderExtensions.ReadString(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<GameState>(ref state__BackingField, _Mirror_SyncVarHookDelegate__003Cstate_003Ek__BackingField, GeneratedNetworkCode._Read_GameState(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<AllowMidJoinType>(ref _allowMidJoins, (Action<AllowMidJoinType, AllowMidJoinType>)null, GeneratedNetworkCode._Read_AllowMidJoinType(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref _allowDejavu, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref enableVotes__BackingField, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<string>(ref _difficulty, _Mirror_SyncVarHookDelegate__difficulty, NetworkReaderExtensions.ReadString(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<MidJoinBanType>(ref _midJoinBanType, (Action<MidJoinBanType, MidJoinBanType>)null, GeneratedNetworkCode._Read_MidJoinBanType(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<MidJoinWaitType>(ref midJoinWaitType__BackingField, (Action<MidJoinWaitType, MidJoinWaitType>)null, GeneratedNetworkCode._Read_MidJoinWaitType(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref _maxPlayers, (Action<int, int>)null, NetworkReaderExtensions.ReadInt(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<string>(ref _lobbyName, _Mirror_SyncVarHookDelegate__lobbyName, NetworkReaderExtensions.ReadString(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<string>(ref _lobbyDescription, (Action<string, string>)null, NetworkReaderExtensions.ReadString(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 1L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<string>(ref gameSettingsSaveKey__BackingField, (Action<string, string>)null, NetworkReaderExtensions.ReadString(reader));
		}
		if ((num & 2L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<GameState>(ref state__BackingField, _Mirror_SyncVarHookDelegate__003Cstate_003Ek__BackingField, GeneratedNetworkCode._Read_GameState(reader));
		}
		if ((num & 4L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<AllowMidJoinType>(ref _allowMidJoins, (Action<AllowMidJoinType, AllowMidJoinType>)null, GeneratedNetworkCode._Read_AllowMidJoinType(reader));
		}
		if ((num & 8L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref _allowDejavu, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
		}
		if ((num & 0x10L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref enableVotes__BackingField, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
		}
		if ((num & 0x20L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<string>(ref _difficulty, _Mirror_SyncVarHookDelegate__difficulty, NetworkReaderExtensions.ReadString(reader));
		}
		if ((num & 0x40L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<MidJoinBanType>(ref _midJoinBanType, (Action<MidJoinBanType, MidJoinBanType>)null, GeneratedNetworkCode._Read_MidJoinBanType(reader));
		}
		if ((num & 0x80L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<MidJoinWaitType>(ref midJoinWaitType__BackingField, (Action<MidJoinWaitType, MidJoinWaitType>)null, GeneratedNetworkCode._Read_MidJoinWaitType(reader));
		}
		if ((num & 0x100L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref _maxPlayers, (Action<int, int>)null, NetworkReaderExtensions.ReadInt(reader));
		}
		if ((num & 0x200L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<string>(ref _lobbyName, _Mirror_SyncVarHookDelegate__lobbyName, NetworkReaderExtensions.ReadString(reader));
		}
		if ((num & 0x400L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<string>(ref _lobbyDescription, (Action<string, string>)null, NetworkReaderExtensions.ReadString(reader));
		}
	}
}
