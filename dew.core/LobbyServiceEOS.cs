using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using Epic.OnlineServices;
using Epic.OnlineServices.Lobby;
using EpicTransport;
using Mirror;
using Steamworks;
using UnityEngine;

public class LobbyServiceEOS : LobbyServiceProvider
{
	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass32_0
	{
		public UniTaskCompletionSource task;

		public LobbyServiceEOS _003C_003E4__this;

		internal void _003CCreateLobby_003Eb__0(ref CreateLobbyCallbackInfo callback)
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0047: Expected Obj, but got Unknown
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			//IL_0067: Unknown result type (might be due to invalid IL or missing references)
			//IL_0071: Unknown result type (might be due to invalid IL or missing references)
			//IL_0073: Unknown result type (might be due to invalid IL or missing references)
			//IL_0076: Unknown result type (might be due to invalid IL or missing references)
			//IL_0095: Unknown result type (might be due to invalid IL or missing references)
			//IL_0097: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
			//IL_0105: Unknown result type (might be due to invalid IL or missing references)
			//IL_0107: Unknown result type (might be due to invalid IL or missing references)
			//IL_0111: Unknown result type (might be due to invalid IL or missing references)
			//IL_0245: Unknown result type (might be due to invalid IL or missing references)
			//IL_0258: Unknown result type (might be due to invalid IL or missing references)
			//IL_025a: Unknown result type (might be due to invalid IL or missing references)
			//IL_026b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0275: Expected Obj, but got Unknown
			try
			{
				_003C_003Ec__DisplayClass32_1 CS_0024_003C_003E8__locals29 = new _003C_003Ec__DisplayClass32_1();
				CS_0024_003C_003E8__locals29.CS_0024_003C_003E8__locals1 = this;
				if ((int)callback.ResultCode != 0)
				{
					task.TrySetException((Exception)new EOSResultException(callback.ResultCode));
					return;
				}
				CS_0024_003C_003E8__locals29.lobbyReturnData = new List<Attribute>();
				CS_0024_003C_003E8__locals29.modHandle = new LobbyModification();
				AttributeData val = default;
				val.Key = Utf8String.op_Implicit("default");
				val.Value = AttributeDataValue.op_Implicit("default");
				AttributeData value = val;
				UpdateLobbyModificationOptions val2 = default;
				val2.LobbyId = callback.LobbyId;
				val2.LocalUserId = EOSSDKComponent.LocalUserProductId;
				UpdateLobbyModificationOptions val3 = val2;
				Result val4 = EOSSDKComponent.GetLobbyInterface().UpdateLobbyModification(ref val3, ref CS_0024_003C_003E8__locals29.modHandle);
				if ((int)val4 != 0 || (Handle)(object)CS_0024_003C_003E8__locals29.modHandle == (Handle)null)
				{
					UnityEngine.Debug.LogWarning($"CreateLobby: UpdateLobbyModification failed: {val4}");
					task.TrySetException((Exception)new EOSResultException(val4));
					return;
				}
				LobbyModificationAddAttributeOptions val5 = default;
				val5.Attribute = value;
				val5.Visibility = (LobbyAttributeVisibility)0;
				LobbyModificationAddAttributeOptions val6 = val5;
				CS_0024_003C_003E8__locals29.modHandle.AddAttribute(ref val6);
				CS_0024_003C_003E8__locals29._003CCreateLobby_003Eg__AddAttr_007C2("version", _003C_003E4__this.GetInitialAttr_version());
				CS_0024_003C_003E8__locals29._003CCreateLobby_003Eg__AddAttr_007C2("crossPlayGate", _003C_003E4__this.GetInitialAttr_crossPlayGate());
				CS_0024_003C_003E8__locals29._003CCreateLobby_003Eg__AddAttr_007C2("hostAddress", _003C_003E4__this.GetInitialAttr_hostAddress());
				CS_0024_003C_003E8__locals29._003CCreateLobby_003Eg__AddAttr_007C2("hasGameStarted", _003C_003E4__this.GetInitialAttr_hasGameStarted());
				CS_0024_003C_003E8__locals29._003CCreateLobby_003Eg__AddAttr_007C2("allowJoin", _003C_003E4__this.GetInitialAttr_allowJoin());
				CS_0024_003C_003E8__locals29._003CCreateLobby_003Eg__AddAttr_007C2("difficulty", _003C_003E4__this.GetInitialAttr_difficulty());
				CS_0024_003C_003E8__locals29._003CCreateLobby_003Eg__AddAttr_007C2("maxPlayers", _003C_003E4__this.GetInitialAttr_maxPlayers());
				CS_0024_003C_003E8__locals29._003CCreateLobby_003Eg__AddAttr_007C2("name", _003C_003E4__this.GetInitialAttr_name());
				CS_0024_003C_003E8__locals29._003CCreateLobby_003Eg__AddAttr_007C2("isInviteOnly", _003C_003E4__this.GetInitialAttr_isInviteOnly());
				CS_0024_003C_003E8__locals29._003CCreateLobby_003Eg__AddAttr_007C2("isModded", DewMod.isGameplayAltered);
				CS_0024_003C_003E8__locals29._003CCreateLobby_003Eg__AddAttr_007C2("customData", new Dictionary<string, string>());
				CS_0024_003C_003E8__locals29._003CCreateLobby_003Eg__AddAttr_007C2("country", _003C_003E4__this.GetInitialAttr_country());
				CS_0024_003C_003E8__locals29.lobbyId = callback.LobbyId;
				ManagerBase<TransitionManager>.instance.UpdateLoadingStatus(LoadingStatus.PreparingLobby);
				UpdateLobbyOptions val7 = default;
				val7.LobbyModificationHandle = CS_0024_003C_003E8__locals29.modHandle;
				UpdateLobbyOptions val8 = val7;
				EOSSDKComponent.GetLobbyInterface().UpdateLobby(ref val8, (object)null, (OnUpdateLobbyCallback)delegate(ref UpdateLobbyCallbackInfo updateCallback)
				{
					//IL_0001: Unknown result type (might be due to invalid IL or missing references)
					//IL_002b: Unknown result type (might be due to invalid IL or missing references)
					//IL_004a: Unknown result type (might be due to invalid IL or missing references)
					//IL_004b: Unknown result type (might be due to invalid IL or missing references)
					//IL_0055: Unknown result type (might be due to invalid IL or missing references)
					//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
					//IL_0014: Unknown result type (might be due to invalid IL or missing references)
					try
					{
						if ((int)updateCallback.ResultCode != 0)
						{
							CS_0024_003C_003E8__locals29.CS_0024_003C_003E8__locals1.task.TrySetException((Exception)new EOSResultException(updateCallback.ResultCode));
						}
						else
						{
							CopyLobbyDetailsHandleOptions val9 = default;
							val9.LobbyId = CS_0024_003C_003E8__locals29.lobbyId;
							val9.LocalUserId = EOSSDKComponent.LocalUserProductId;
							CopyLobbyDetailsHandleOptions val10 = val9;
							LobbyDetails h = default;
							EOSSDKComponent.GetLobbyInterface().CopyLobbyDetailsHandle(ref val10, ref h);
							LobbyInstanceEpic orCreate = CS_0024_003C_003E8__locals29.CS_0024_003C_003E8__locals1._003C_003E4__this.GetOrCreate(Utf8String.op_Implicit(CS_0024_003C_003E8__locals29.lobbyId));
							orCreate.ApplyFromHandle(h);
							orCreate.isLobbyLeader = true;
							CS_0024_003C_003E8__locals29.CS_0024_003C_003E8__locals1._003C_003E4__this.SetCurrentLobbyData(orCreate);
							CS_0024_003C_003E8__locals29.CS_0024_003C_003E8__locals1._003C_003E4__this.SetLobbyShortId();
							CS_0024_003C_003E8__locals29.CS_0024_003C_003E8__locals1.task.TrySetResult();
						}
					}
					catch (Exception ex2)
					{
						CS_0024_003C_003E8__locals29.CS_0024_003C_003E8__locals1.task.TrySetException(ex2);
					}
				});
				CS_0024_003C_003E8__locals29.modHandle.Release();
			}
			catch (Exception ex)
			{
				task.TrySetException(ex);
			}
		}
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass32_1
	{
		public LobbyModification modHandle;

		public List<Attribute> lobbyReturnData;

		public Utf8String lobbyId;

		public _003C_003Ec__DisplayClass32_0 CS_0024_003C_003E8__locals1;

		internal void _003CCreateLobby_003Eb__3(ref UpdateLobbyCallbackInfo updateCallback)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0055: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			try
			{
				if ((int)updateCallback.ResultCode != 0)
				{
					CS_0024_003C_003E8__locals1.task.TrySetException((Exception)new EOSResultException(updateCallback.ResultCode));
					return;
				}
				CopyLobbyDetailsHandleOptions val = default;
				val.LobbyId = lobbyId;
				val.LocalUserId = EOSSDKComponent.LocalUserProductId;
				CopyLobbyDetailsHandleOptions val2 = val;
				LobbyDetails h = default;
				EOSSDKComponent.GetLobbyInterface().CopyLobbyDetailsHandle(ref val2, ref h);
				LobbyInstanceEpic orCreate = CS_0024_003C_003E8__locals1._003C_003E4__this.GetOrCreate(Utf8String.op_Implicit(lobbyId));
				orCreate.ApplyFromHandle(h);
				orCreate.isLobbyLeader = true;
				CS_0024_003C_003E8__locals1._003C_003E4__this.SetCurrentLobbyData(orCreate);
				CS_0024_003C_003E8__locals1._003C_003E4__this.SetLobbyShortId();
				CS_0024_003C_003E8__locals1.task.TrySetResult();
			}
			catch (Exception ex)
			{
				CS_0024_003C_003E8__locals1.task.TrySetException(ex);
			}
		}
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass33_0
	{
		public UniTaskCompletionSource task;

		public LobbyServiceEOS _003C_003E4__this;

		internal void _003CJoinLobby_003Eb__0(ref JoinLobbyCallbackInfo callback)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			try
			{
				if ((int)callback.ResultCode != 0)
				{
					task.TrySetException((Exception)new EOSResultException(callback.ResultCode));
					return;
				}
				CopyLobbyDetailsHandleOptions val = default;
				val.LobbyId = callback.LobbyId;
				val.LocalUserId = EOSSDKComponent.LocalUserProductId;
				CopyLobbyDetailsHandleOptions val2 = val;
				LobbyDetails h = default;
				EOSSDKComponent.GetLobbyInterface().CopyLobbyDetailsHandle(ref val2, ref h);
				LobbyInstanceEpic orCreate = _003C_003E4__this.GetOrCreate(Utf8String.op_Implicit(callback.LobbyId));
				orCreate.ApplyFromHandle(h);
				orCreate.isLobbyLeader = false;
				_003C_003E4__this.SetCurrentLobbyData(orCreate);
				task.TrySetResult();
			}
			catch (Exception ex)
			{
				task.TrySetException(ex);
			}
		}
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass34_0
	{
		public UniTaskCompletionSource task;

		internal void _003CLeaveLobby_003Eb__0(ref DestroyLobbyCallbackInfo callback)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			if ((int)callback.ResultCode != 0)
			{
				task.TrySetException((Exception)new EOSResultException(callback.ResultCode));
			}
			else
			{
				task.TrySetResult();
			}
		}

		internal void _003CLeaveLobby_003Eb__1(ref LeaveLobbyCallbackInfo callback)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Invalid comparison between Unknown and I4
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			if ((int)callback.ResultCode != 0 && (int)callback.ResultCode != 18)
			{
				task.TrySetException((Exception)new EOSResultException(callback.ResultCode));
			}
			else
			{
				task.TrySetResult();
			}
		}
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass36_0
	{
		public UniTaskCompletionSource<LobbySearchResult> task;

		public LobbySearch search;

		public string localCountry;

		internal void _003CGetLobbies_003Eb__0(ref LobbySearchFindCallbackInfo callback)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0055: Unknown result type (might be due to invalid IL or missing references)
			//IL_0084: Unknown result type (might be due to invalid IL or missing references)
			try
			{
				if ((int)callback.ResultCode != 0)
				{
					task.TrySetException((Exception)new EOSResultException(callback.ResultCode));
					return;
				}
				LobbySearchResult lobbySearchResult = new LobbySearchResult();
				LobbySearchGetSearchResultCountOptions val = default;
				LobbyDetails val3 = default;
				for (int i = 0; i < search.GetSearchResultCount(ref val); i++)
				{
					LobbySearchCopySearchResultByIndexOptions val2 = default;
					val2.LobbyIndex = (uint)i;
					search.CopySearchResultByIndex(ref val2, ref val3);
					LobbyInstanceEpic lobbyInstanceEpic = new LobbyInstanceEpic().ApplyFromHandle(val3);
					lobbyInstanceEpic.connectionQuality = EstimateConnectionQuality(localCountry, lobbyInstanceEpic.country);
					LobbyDetailsGetLobbyOwnerOptions val4 = default;
					if (lobbyInstanceEpic.currentPlayers < lobbyInstanceEpic.maxPlayers && lobbyInstanceEpic.currentPlayers > 0 && lobbyInstanceEpic.name.Length > 0 && (lobbyInstanceEpic.allowMidJoins != AllowMidJoinType.Disallow || !lobbyInstanceEpic.hasGameStarted) && (lobbyInstanceEpic.allowMidJoins != AllowMidJoinType.RejoinOnly || !lobbyInstanceEpic.hasGameStarted || lobbyInstanceEpic.savedPlayers.Contains(DewSave.profileMain.guid)) && (Handle)(object)val3.GetLobbyOwner(ref val4) != (Handle)(object)EOSSDKComponent.LocalUserProductId)
					{
						lobbySearchResult.lobbies.Add(lobbyInstanceEpic);
					}
					else
					{
						val3.Release();
					}
				}
				task.TrySetResult(lobbySearchResult);
			}
			catch (Exception ex)
			{
				task.TrySetException(ex);
			}
			finally
			{
				search.Release();
			}
		}
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass41_0
	{
		public UniTaskCompletionSource<LobbyDetails> task;

		public LobbySearch search;

		internal void _003CGetLobbyById_003Eb__0(ref LobbySearchFindCallbackInfo callback)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			//IL_006b: Unknown result type (might be due to invalid IL or missing references)
			try
			{
				if ((int)callback.ResultCode != 0)
				{
					task.TrySetException((Exception)new EOSResultException(callback.ResultCode));
				}
				LobbySearchGetSearchResultCountOptions val = default;
				if (search.GetSearchResultCount(ref val) == 0)
				{
					task.TrySetException((Exception)new EOSResultException((Result)18));
					return;
				}
				LobbySearchCopySearchResultByIndexOptions val2 = default;
				val2.LobbyIndex = 0u;
				LobbySearchCopySearchResultByIndexOptions val3 = val2;
				LobbyDetails val4 = default;
				search.CopySearchResultByIndex(ref val3, ref val4);
				task.TrySetResult(val4);
			}
			finally
			{
				search.Release();
			}
		}
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CCreateLobby_003Ed__32 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder _003C_003Et__builder;

		public LobbyServiceEOS _003C_003E4__this;

		private _003C_003Ec__DisplayClass32_0 _003C_003E8__1;

		private Awaiter _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_007f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0084: Unknown result type (might be due to invalid IL or missing references)
			//IL_008c: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
			//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e9: Unknown result type (might be due to invalid IL or missing references)
			//IL_0261: Unknown result type (might be due to invalid IL or missing references)
			//IL_0266: Unknown result type (might be due to invalid IL or missing references)
			//IL_026e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			//IL_004f: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
			//IL_0105: Unknown result type (might be due to invalid IL or missing references)
			//IL_0108: Unknown result type (might be due to invalid IL or missing references)
			//IL_0129: Unknown result type (might be due to invalid IL or missing references)
			//IL_0157: Unknown result type (might be due to invalid IL or missing references)
			//IL_0159: Unknown result type (might be due to invalid IL or missing references)
			//IL_0160: Unknown result type (might be due to invalid IL or missing references)
			//IL_016a: Expected Obj, but got Unknown
			//IL_0189: Unknown result type (might be due to invalid IL or missing references)
			//IL_0193: Expected Obj, but got Unknown
			//IL_019e: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			//IL_0066: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
			//IL_0226: Unknown result type (might be due to invalid IL or missing references)
			//IL_022b: Unknown result type (might be due to invalid IL or missing references)
			//IL_022f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0234: Unknown result type (might be due to invalid IL or missing references)
			//IL_0249: Unknown result type (might be due to invalid IL or missing references)
			//IL_024b: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			LobbyServiceEOS lobbyServiceEOS = _003C_003E4__this;
			try
			{
				UniTask val2;
				Awaiter val;
				LobbyPermissionLevel permissionLevel;
				CreateLobbyOptions val3;
				CreateLobbyOptions val4;
				switch (num)
				{
				default:
					_003C_003E8__1 = new _003C_003Ec__DisplayClass32_0();
					_003C_003E8__1._003C_003E4__this = _003C_003E4__this;
					val2 = lobbyServiceEOS.EnsureInitialized();
					val = val2.GetAwaiter();
					if (!val.IsCompleted)
					{
						num = (_003C_003E1__state = 0);
						_003C_003Eu__1 = val;
						_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter, _003CCreateLobby_003Ed__32>(ref val, ref this);
						return;
					}
					goto IL_009b;
				case 0:
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default;
					num = (_003C_003E1__state = -1);
					goto IL_009b;
				case 1:
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default;
					num = (_003C_003E1__state = -1);
					goto IL_00fd;
				case 2:
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default;
					num = (_003C_003E1__state = -1);
					goto IL_01f8;
				case 3:
					{
						val = _003C_003Eu__1;
						_003C_003Eu__1 = default;
						num = (_003C_003E1__state = -1);
						goto IL_027d;
					}
					IL_027d:
					val.GetResult();
					break;
					IL_009b:
					val.GetResult();
					val2 = lobbyServiceEOS.LeaveLobby();
					val = val2.GetAwaiter();
					if (!val.IsCompleted)
					{
						num = (_003C_003E1__state = 1);
						_003C_003Eu__1 = val;
						_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter, _003CCreateLobby_003Ed__32>(ref val, ref this);
						return;
					}
					goto IL_00fd;
					IL_01f8:
					val.GetResult();
					UnityEngine.Debug.Log($"Created EOS Lobby: {lobbyServiceEOS.currentLobby}");
					if (!((UnityEngine.Object)(object)NetworkedManagerBase<GameSettingsManager>.instance != null))
					{
						break;
					}
					val2 = NetworkedManagerBase<GameSettingsManager>.instance.Lobby_UpdateGameAttributes();
					val = val2.GetAwaiter();
					if (!val.IsCompleted)
					{
						num = (_003C_003E1__state = 3);
						_003C_003Eu__1 = val;
						_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter, _003CCreateLobby_003Ed__32>(ref val, ref this);
						return;
					}
					goto IL_027d;
					IL_00fd:
					val.GetResult();
					permissionLevel = (LobbyPermissionLevel)0;
					val3 = default;
					val3.LocalUserId = EOSSDKComponent.LocalUserProductId;
					val3.MaxLobbyMembers = (uint)lobbyServiceEOS.GetInitialAttr_maxPlayers();
					val3.PermissionLevel = permissionLevel;
					val3.PresenceEnabled = false;
					val3.BucketId = Utf8String.op_Implicit("default");
					val3.CrossplayOptOut = !CROSSPLAY;
					val4 = val3;
					_003C_003E8__1.task = new UniTaskCompletionSource();
					ManagerBase<TransitionManager>.instance.UpdateLoadingStatus(LoadingStatus.CreatingLobby);
					EOSSDKComponent.GetLobbyInterface().CreateLobby(ref val4, (object)null, (OnCreateLobbyCallback)delegate(ref CreateLobbyCallbackInfo callback)
					{
						//IL_000e: Unknown result type (might be due to invalid IL or missing references)
						//IL_003d: Unknown result type (might be due to invalid IL or missing references)
						//IL_0047: Expected Obj, but got Unknown
						//IL_0049: Unknown result type (might be due to invalid IL or missing references)
						//IL_0067: Unknown result type (might be due to invalid IL or missing references)
						//IL_0071: Unknown result type (might be due to invalid IL or missing references)
						//IL_0073: Unknown result type (might be due to invalid IL or missing references)
						//IL_0076: Unknown result type (might be due to invalid IL or missing references)
						//IL_0095: Unknown result type (might be due to invalid IL or missing references)
						//IL_0097: Unknown result type (might be due to invalid IL or missing references)
						//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
						//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
						//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
						//IL_001c: Unknown result type (might be due to invalid IL or missing references)
						//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
						//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
						//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
						//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
						//IL_0105: Unknown result type (might be due to invalid IL or missing references)
						//IL_0107: Unknown result type (might be due to invalid IL or missing references)
						//IL_0111: Unknown result type (might be due to invalid IL or missing references)
						//IL_0245: Unknown result type (might be due to invalid IL or missing references)
						//IL_0258: Unknown result type (might be due to invalid IL or missing references)
						//IL_025a: Unknown result type (might be due to invalid IL or missing references)
						//IL_026b: Unknown result type (might be due to invalid IL or missing references)
						//IL_0275: Expected Obj, but got Unknown
						try
						{
							_003C_003Ec__DisplayClass32_1 CS_0024_003C_003E8__locals29 = new _003C_003Ec__DisplayClass32_1();
							CS_0024_003C_003E8__locals29.CS_0024_003C_003E8__locals1 = _003C_003E8__1;
							if ((int)callback.ResultCode != 0)
							{
								_003C_003E8__1.task.TrySetException((Exception)new EOSResultException(callback.ResultCode));
							}
							else
							{
								CS_0024_003C_003E8__locals29.lobbyReturnData = new List<Attribute>();
								CS_0024_003C_003E8__locals29.modHandle = new LobbyModification();
								AttributeData val5 = default;
								val5.Key = Utf8String.op_Implicit("default");
								val5.Value = AttributeDataValue.op_Implicit("default");
								AttributeData value = val5;
								UpdateLobbyModificationOptions val6 = default;
								val6.LobbyId = callback.LobbyId;
								val6.LocalUserId = EOSSDKComponent.LocalUserProductId;
								UpdateLobbyModificationOptions val7 = val6;
								Result val8 = EOSSDKComponent.GetLobbyInterface().UpdateLobbyModification(ref val7, ref CS_0024_003C_003E8__locals29.modHandle);
								if ((int)val8 != 0 || (Handle)(object)CS_0024_003C_003E8__locals29.modHandle == (Handle)null)
								{
									UnityEngine.Debug.LogWarning($"CreateLobby: UpdateLobbyModification failed: {val8}");
									_003C_003E8__1.task.TrySetException((Exception)new EOSResultException(val8));
								}
								else
								{
									LobbyModificationAddAttributeOptions val9 = default;
									val9.Attribute = value;
									val9.Visibility = (LobbyAttributeVisibility)0;
									LobbyModificationAddAttributeOptions val10 = val9;
									CS_0024_003C_003E8__locals29.modHandle.AddAttribute(ref val10);
									CS_0024_003C_003E8__locals29._003CCreateLobby_003Eg__AddAttr_007C2("version", _003C_003E8__1._003C_003E4__this.GetInitialAttr_version());
									CS_0024_003C_003E8__locals29._003CCreateLobby_003Eg__AddAttr_007C2("crossPlayGate", _003C_003E8__1._003C_003E4__this.GetInitialAttr_crossPlayGate());
									CS_0024_003C_003E8__locals29._003CCreateLobby_003Eg__AddAttr_007C2("hostAddress", _003C_003E8__1._003C_003E4__this.GetInitialAttr_hostAddress());
									CS_0024_003C_003E8__locals29._003CCreateLobby_003Eg__AddAttr_007C2("hasGameStarted", _003C_003E8__1._003C_003E4__this.GetInitialAttr_hasGameStarted());
									CS_0024_003C_003E8__locals29._003CCreateLobby_003Eg__AddAttr_007C2("allowJoin", _003C_003E8__1._003C_003E4__this.GetInitialAttr_allowJoin());
									CS_0024_003C_003E8__locals29._003CCreateLobby_003Eg__AddAttr_007C2("difficulty", _003C_003E8__1._003C_003E4__this.GetInitialAttr_difficulty());
									CS_0024_003C_003E8__locals29._003CCreateLobby_003Eg__AddAttr_007C2("maxPlayers", _003C_003E8__1._003C_003E4__this.GetInitialAttr_maxPlayers());
									CS_0024_003C_003E8__locals29._003CCreateLobby_003Eg__AddAttr_007C2("name", _003C_003E8__1._003C_003E4__this.GetInitialAttr_name());
									CS_0024_003C_003E8__locals29._003CCreateLobby_003Eg__AddAttr_007C2("isInviteOnly", _003C_003E8__1._003C_003E4__this.GetInitialAttr_isInviteOnly());
									CS_0024_003C_003E8__locals29._003CCreateLobby_003Eg__AddAttr_007C2("isModded", DewMod.isGameplayAltered);
									CS_0024_003C_003E8__locals29._003CCreateLobby_003Eg__AddAttr_007C2("customData", new Dictionary<string, string>());
									CS_0024_003C_003E8__locals29._003CCreateLobby_003Eg__AddAttr_007C2("country", _003C_003E8__1._003C_003E4__this.GetInitialAttr_country());
									CS_0024_003C_003E8__locals29.lobbyId = callback.LobbyId;
									ManagerBase<TransitionManager>.instance.UpdateLoadingStatus(LoadingStatus.PreparingLobby);
									UpdateLobbyOptions val11 = default;
									val11.LobbyModificationHandle = CS_0024_003C_003E8__locals29.modHandle;
									UpdateLobbyOptions val12 = val11;
									EOSSDKComponent.GetLobbyInterface().UpdateLobby(ref val12, (object)null, (OnUpdateLobbyCallback)delegate(ref UpdateLobbyCallbackInfo updateCallback)
									{
										//IL_0001: Unknown result type (might be due to invalid IL or missing references)
										//IL_002b: Unknown result type (might be due to invalid IL or missing references)
										//IL_004a: Unknown result type (might be due to invalid IL or missing references)
										//IL_004b: Unknown result type (might be due to invalid IL or missing references)
										//IL_0055: Unknown result type (might be due to invalid IL or missing references)
										//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
										//IL_0014: Unknown result type (might be due to invalid IL or missing references)
										try
										{
											if ((int)updateCallback.ResultCode != 0)
											{
												CS_0024_003C_003E8__locals29.CS_0024_003C_003E8__locals1.task.TrySetException((Exception)new EOSResultException(updateCallback.ResultCode));
											}
											else
											{
												CopyLobbyDetailsHandleOptions val13 = default;
												val13.LobbyId = CS_0024_003C_003E8__locals29.lobbyId;
												val13.LocalUserId = EOSSDKComponent.LocalUserProductId;
												CopyLobbyDetailsHandleOptions val14 = val13;
												LobbyDetails h = default;
												EOSSDKComponent.GetLobbyInterface().CopyLobbyDetailsHandle(ref val14, ref h);
												LobbyInstanceEpic orCreate = CS_0024_003C_003E8__locals29.CS_0024_003C_003E8__locals1._003C_003E4__this.GetOrCreate(Utf8String.op_Implicit(CS_0024_003C_003E8__locals29.lobbyId));
												orCreate.ApplyFromHandle(h);
												orCreate.isLobbyLeader = true;
												CS_0024_003C_003E8__locals29.CS_0024_003C_003E8__locals1._003C_003E4__this.SetCurrentLobbyData(orCreate);
												CS_0024_003C_003E8__locals29.CS_0024_003C_003E8__locals1._003C_003E4__this.SetLobbyShortId();
												CS_0024_003C_003E8__locals29.CS_0024_003C_003E8__locals1.task.TrySetResult();
											}
										}
										catch (Exception ex2)
										{
											CS_0024_003C_003E8__locals29.CS_0024_003C_003E8__locals1.task.TrySetException(ex2);
										}
									});
									CS_0024_003C_003E8__locals29.modHandle.Release();
								}
							}
						}
						catch (Exception ex)
						{
							_003C_003E8__1.task.TrySetException(ex);
						}
					});
					val2 = _003C_003E8__1.task.Task;
					val = val2.GetAwaiter();
					if (!val.IsCompleted)
					{
						num = (_003C_003E1__state = 2);
						_003C_003Eu__1 = val;
						_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter, _003CCreateLobby_003Ed__32>(ref val, ref this);
						return;
					}
					goto IL_01f8;
				}
				lobbyServiceEOS.StopAllCoroutines();
				lobbyServiceEOS.StartCoroutine(_003C_003E8__1._003CCreateLobby_003Eg__Heartbeat_007C1());
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
	private struct _003CEnsureInitialized_003Ed__31 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder _003C_003Et__builder;

		private Awaiter _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
			//IL_016b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0170: Unknown result type (might be due to invalid IL or missing references)
			//IL_0177: Unknown result type (might be due to invalid IL or missing references)
			//IL_011d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0133: Unknown result type (might be due to invalid IL or missing references)
			//IL_0138: Unknown result type (might be due to invalid IL or missing references)
			//IL_013b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0140: Unknown result type (might be due to invalid IL or missing references)
			//IL_0154: Unknown result type (might be due to invalid IL or missing references)
			//IL_0155: Unknown result type (might be due to invalid IL or missing references)
			//IL_0077: Unknown result type (might be due to invalid IL or missing references)
			//IL_008d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0092: Unknown result type (might be due to invalid IL or missing references)
			//IL_0095: Unknown result type (might be due to invalid IL or missing references)
			//IL_009a: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
			//IL_00af: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			try
			{
				Awaiter val;
				if (num == 0)
				{
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default;
					num = (_003C_003E1__state = -1);
					goto IL_00e3;
				}
				if (num == 1)
				{
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default;
					num = (_003C_003E1__state = -1);
					goto IL_0186;
				}
				UniTask val2;
				if (EOSSDKComponent.IsConnecting || !EOSSDKComponent.Initialized)
				{
					if (ManagerBase<EOSManager>.softInstance != null)
					{
						if (ManagerBase<EOSManager>.softInstance.status == ServiceStatus.Error)
						{
							ManagerBase<EOSManager>.softInstance.TryInit();
						}
						val2 = UniTaskExtensions.Timeout(UniTask.WaitWhile((Func<bool>)(() => ManagerBase<EOSManager>.softInstance != null && ManagerBase<EOSManager>.softInstance.status == ServiceStatus.Loading), (PlayerLoopTiming)8, default(CancellationToken)), TimeSpan.FromSeconds(15.0), (DelayType)0, (PlayerLoopTiming)8, (CancellationTokenSource)null);
						val = val2.GetAwaiter();
						if (!val.IsCompleted)
						{
							num = (_003C_003E1__state = 0);
							_003C_003Eu__1 = val;
							_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter, _003CEnsureInitialized_003Ed__31>(ref val, ref this);
							return;
						}
						goto IL_00e3;
					}
					goto IL_00ea;
				}
				goto end_IL_0007;
				IL_00ea:
				UnityEngine.Debug.Log("Waiting for EOS");
				val2 = UniTaskExtensions.Timeout(UniTask.WaitWhile((Func<bool>)(() => EOSSDKComponent.IsConnecting), (PlayerLoopTiming)8, default(CancellationToken)), TimeSpan.FromSeconds(10.0), (DelayType)0, (PlayerLoopTiming)8, (CancellationTokenSource)null);
				val = val2.GetAwaiter();
				if (!val.IsCompleted)
				{
					num = (_003C_003E1__state = 1);
					_003C_003Eu__1 = val;
					_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter, _003CEnsureInitialized_003Ed__31>(ref val, ref this);
					return;
				}
				goto IL_0186;
				IL_00e3:
				val.GetResult();
				goto IL_00ea;
				IL_0186:
				val.GetResult();
				if (!EOSSDKComponent.Initialized)
				{
					throw new EOSResultException((Result)int.MaxValue);
				}
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

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CGetLobbies_003Ed__36 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder _003C_003Et__builder;

		public LobbyServiceEOS _003C_003E4__this;

		private _003C_003Ec__DisplayClass36_0 _003C_003E8__1;

		public Action<LobbySearchResult> onUpdated;

		private Awaiter _003C_003Eu__1;

		private Awaiter<LobbySearchResult> _003C_003Eu__2;

		private void MoveNext()
		{
			//IL_0062: Unknown result type (might be due to invalid IL or missing references)
			//IL_0067: Unknown result type (might be due to invalid IL or missing references)
			//IL_006f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0092: Unknown result type (might be due to invalid IL or missing references)
			//IL_009c: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00da: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
			//IL_010b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0115: Unknown result type (might be due to invalid IL or missing references)
			//IL_0129: Unknown result type (might be due to invalid IL or missing references)
			//IL_012b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0134: Unknown result type (might be due to invalid IL or missing references)
			//IL_013e: Unknown result type (might be due to invalid IL or missing references)
			//IL_015d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0167: Unknown result type (might be due to invalid IL or missing references)
			//IL_017b: Unknown result type (might be due to invalid IL or missing references)
			//IL_017d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0186: Unknown result type (might be due to invalid IL or missing references)
			//IL_0190: Unknown result type (might be due to invalid IL or missing references)
			//IL_01af: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
			//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
			//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
			//IL_0212: Unknown result type (might be due to invalid IL or missing references)
			//IL_021c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0230: Unknown result type (might be due to invalid IL or missing references)
			//IL_0232: Unknown result type (might be due to invalid IL or missing references)
			//IL_023d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0247: Expected Obj, but got Unknown
			//IL_0249: Unknown result type (might be due to invalid IL or missing references)
			//IL_0257: Unknown result type (might be due to invalid IL or missing references)
			//IL_0259: Unknown result type (might be due to invalid IL or missing references)
			//IL_026c: Unknown result type (might be due to invalid IL or missing references)
			//IL_035d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0362: Unknown result type (might be due to invalid IL or missing references)
			//IL_036a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			//IL_027d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0282: Unknown result type (might be due to invalid IL or missing references)
			//IL_0291: Unknown result type (might be due to invalid IL or missing references)
			//IL_02a7: Unknown result type (might be due to invalid IL or missing references)
			//IL_02f4: Unknown result type (might be due to invalid IL or missing references)
			//IL_02fe: Expected Obj, but got Unknown
			//IL_0309: Unknown result type (might be due to invalid IL or missing references)
			//IL_031f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0324: Unknown result type (might be due to invalid IL or missing references)
			//IL_0328: Unknown result type (might be due to invalid IL or missing references)
			//IL_032d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0342: Unknown result type (might be due to invalid IL or missing references)
			//IL_0344: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			LobbyServiceEOS lobbyServiceEOS = _003C_003E4__this;
			try
			{
				Awaiter<LobbySearchResult> val;
				Awaiter val3;
				if (num != 0)
				{
					if (num == 1)
					{
						val = _003C_003Eu__2;
						_003C_003Eu__2 = default;
						num = (_003C_003E1__state = -1);
						goto IL_0379;
					}
					_003C_003E8__1 = new _003C_003Ec__DisplayClass36_0();
					UniTask val2 = lobbyServiceEOS.EnsureInitialized();
					val3 = val2.GetAwaiter();
					if (!val3.IsCompleted)
					{
						num = (_003C_003E1__state = 0);
						_003C_003Eu__1 = val3;
						_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter, _003CGetLobbies_003Ed__36>(ref val3, ref this);
						return;
					}
				}
				else
				{
					val3 = _003C_003Eu__1;
					_003C_003Eu__1 = default;
					num = (_003C_003E1__state = -1);
				}
				val3.GetResult();
				uint maxResults = 100u;
				LobbySearchSetParameterOptions[] array = new LobbySearchSetParameterOptions[5];
				LobbySearchSetParameterOptions val4 = default;
				AttributeData value = default;
				value.Key = Utf8String.op_Implicit("version");
				value.Value = AttributeDataValue.op_Implicit(Dew.GetCurrentMultiplayerCompatibilityVersion());
				val4.Parameter = value;
				val4.ComparisonOp = (ComparisonOp)0;
				array[0] = val4;
				val4 = default;
				value = default;
				value.Key = Utf8String.op_Implicit("crossPlayGate");
				value.Value = AttributeDataValue.op_Implicit(GetCrossPlayGate());
				val4.Parameter = value;
				val4.ComparisonOp = (ComparisonOp)0;
				array[1] = val4;
				val4 = default;
				value = default;
				value.Key = Utf8String.op_Implicit("allowJoin");
				value.Value = AttributeDataValue.op_Implicit((bool?)true);
				val4.Parameter = value;
				val4.ComparisonOp = (ComparisonOp)0;
				array[2] = val4;
				val4 = default;
				value = default;
				value.Key = Utf8String.op_Implicit("isInviteOnly");
				value.Value = AttributeDataValue.op_Implicit((bool?)false);
				val4.Parameter = value;
				val4.ComparisonOp = (ComparisonOp)0;
				array[3] = val4;
				val4 = default;
				value = default;
				value.Key = Utf8String.op_Implicit("heartbeat");
				value.Value = AttributeDataValue.op_Implicit((long?)(DateTimeOffset.UtcNow.ToUnixTimeSeconds() - 30));
				val4.Parameter = value;
				val4.ComparisonOp = (ComparisonOp)2;
				array[4] = val4;
				_003C_003E8__1.search = new LobbySearch();
				CreateLobbySearchOptions val5 = default;
				val5.MaxResults = maxResults;
				CreateLobbySearchOptions val6 = val5;
				EOSSDKComponent.GetLobbyInterface().CreateLobbySearch(ref val6, ref _003C_003E8__1.search);
				LobbySearchSetParameterOptions[] array2 = array;
				for (int i = 0; i < array2.Length; i++)
				{
					LobbySearchSetParameterOptions val7 = array2[i];
					_003C_003E8__1.search.SetParameter(ref val7);
				}
				LobbySearchFindOptions val8 = default;
				val8.LocalUserId = EOSSDKComponent.LocalUserProductId;
				_003C_003E8__1.localCountry = lobbyServiceEOS.GetInitialAttr_country();
				_003C_003E8__1.task = new UniTaskCompletionSource<LobbySearchResult>();
				_003C_003E8__1.search.Find(ref val8, (object)null, (LobbySearchOnFindCallback)delegate(ref LobbySearchFindCallbackInfo callback)
				{
					//IL_0001: Unknown result type (might be due to invalid IL or missing references)
					//IL_002c: Unknown result type (might be due to invalid IL or missing references)
					//IL_0032: Unknown result type (might be due to invalid IL or missing references)
					//IL_0033: Unknown result type (might be due to invalid IL or missing references)
					//IL_000f: Unknown result type (might be due to invalid IL or missing references)
					//IL_003d: Unknown result type (might be due to invalid IL or missing references)
					//IL_0055: Unknown result type (might be due to invalid IL or missing references)
					//IL_0084: Unknown result type (might be due to invalid IL or missing references)
					try
					{
						if ((int)callback.ResultCode != 0)
						{
							_003C_003E8__1.task.TrySetException((Exception)new EOSResultException(callback.ResultCode));
						}
						else
						{
							LobbySearchResult lobbySearchResult = new LobbySearchResult();
							LobbySearchGetSearchResultCountOptions val9 = default;
							LobbyDetails val11 = default;
							for (int j = 0; j < _003C_003E8__1.search.GetSearchResultCount(ref val9); j++)
							{
								LobbySearchCopySearchResultByIndexOptions val10 = default;
								val10.LobbyIndex = (uint)j;
								_003C_003E8__1.search.CopySearchResultByIndex(ref val10, ref val11);
								LobbyInstanceEpic lobbyInstanceEpic2 = new LobbyInstanceEpic().ApplyFromHandle(val11);
								lobbyInstanceEpic2.connectionQuality = EstimateConnectionQuality(_003C_003E8__1.localCountry, lobbyInstanceEpic2.country);
								LobbyDetailsGetLobbyOwnerOptions val12 = default;
								if (lobbyInstanceEpic2.currentPlayers < lobbyInstanceEpic2.maxPlayers && lobbyInstanceEpic2.currentPlayers > 0 && lobbyInstanceEpic2.name.Length > 0 && (lobbyInstanceEpic2.allowMidJoins != AllowMidJoinType.Disallow || !lobbyInstanceEpic2.hasGameStarted) && (lobbyInstanceEpic2.allowMidJoins != AllowMidJoinType.RejoinOnly || !lobbyInstanceEpic2.hasGameStarted || lobbyInstanceEpic2.savedPlayers.Contains(DewSave.profileMain.guid)) && (Handle)(object)val11.GetLobbyOwner(ref val12) != (Handle)(object)EOSSDKComponent.LocalUserProductId)
								{
									lobbySearchResult.lobbies.Add(lobbyInstanceEpic2);
								}
								else
								{
									val11.Release();
								}
							}
							_003C_003E8__1.task.TrySetResult(lobbySearchResult);
						}
					}
					catch (Exception ex)
					{
						_003C_003E8__1.task.TrySetException(ex);
					}
					finally
					{
						_003C_003E8__1.search.Release();
					}
				});
				val = UniTaskExtensions.Timeout<LobbySearchResult>(_003C_003E8__1.task.Task, TimeSpan.FromSeconds(15.0), (DelayType)0, (PlayerLoopTiming)8, (CancellationTokenSource)null).GetAwaiter();
				if (!val.IsCompleted)
				{
					num = (_003C_003E1__state = 1);
					_003C_003Eu__2 = val;
					_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter<LobbySearchResult>, _003CGetLobbies_003Ed__36>(ref val, ref this);
					return;
				}
				goto IL_0379;
				IL_0379:
				LobbySearchResult result = val.GetResult();
				List<LobbyInstance>.Enumerator enumerator = lobbyServiceEOS.foundLobbies.lobbies.GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						if (enumerator.Current is LobbyInstanceEpic lobbyInstanceEpic && (Handle)(object)lobbyInstanceEpic.details != (Handle)null)
						{
							lobbyInstanceEpic.details.Release();
							lobbyInstanceEpic.details = null;
						}
					}
				}
				finally
				{
					if (num < 0)
					{
						((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
					}
				}
				onUpdated?.Invoke(result);
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
	private struct _003CGetLobbyById_003Ed__41 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder<LobbyDetails> _003C_003Et__builder;

		public LobbyServiceEOS _003C_003E4__this;

		private _003C_003Ec__DisplayClass41_0 _003C_003E8__1;

		public string id;

		private Awaiter _003C_003Eu__1;

		private Awaiter<LobbyDetails> _003C_003Eu__2;

		private void MoveNext()
		{
			//IL_0062: Unknown result type (might be due to invalid IL or missing references)
			//IL_0067: Unknown result type (might be due to invalid IL or missing references)
			//IL_006f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0093: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
			//IL_022e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0233: Unknown result type (might be due to invalid IL or missing references)
			//IL_023b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_016d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0185: Unknown result type (might be due to invalid IL or missing references)
			//IL_0187: Unknown result type (might be due to invalid IL or missing references)
			//IL_0196: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
			//IL_0104: Unknown result type (might be due to invalid IL or missing references)
			//IL_011f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0129: Unknown result type (might be due to invalid IL or missing references)
			//IL_013d: Unknown result type (might be due to invalid IL or missing references)
			//IL_013f: Unknown result type (might be due to invalid IL or missing references)
			//IL_014e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
			//IL_01de: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e8: Expected Obj, but got Unknown
			//IL_01f3: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f8: Unknown result type (might be due to invalid IL or missing references)
			//IL_01fc: Unknown result type (might be due to invalid IL or missing references)
			//IL_0201: Unknown result type (might be due to invalid IL or missing references)
			//IL_0216: Unknown result type (might be due to invalid IL or missing references)
			//IL_0218: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			LobbyServiceEOS lobbyServiceEOS = _003C_003E4__this;
			LobbyDetails result;
			try
			{
				Awaiter<LobbyDetails> val;
				Awaiter val3;
				if (num != 0)
				{
					if (num == 1)
					{
						val = _003C_003Eu__2;
						_003C_003Eu__2 = default;
						num = (_003C_003E1__state = -1);
						goto IL_024a;
					}
					_003C_003E8__1 = new _003C_003Ec__DisplayClass41_0();
					UniTask val2 = lobbyServiceEOS.EnsureInitialized();
					val3 = val2.GetAwaiter();
					if (!val3.IsCompleted)
					{
						num = (_003C_003E1__state = 0);
						_003C_003Eu__1 = val3;
						_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter, _003CGetLobbyById_003Ed__41>(ref val3, ref this);
						return;
					}
				}
				else
				{
					val3 = _003C_003Eu__1;
					_003C_003Eu__1 = default;
					num = (_003C_003E1__state = -1);
				}
				val3.GetResult();
				ManagerBase<TransitionManager>.instance.UpdateLoadingStatus(LoadingStatus.GettingLobbyInformation);
				CreateLobbySearchOptions val4 = default;
				val4.MaxResults = 1u;
				CreateLobbySearchOptions val5 = val4;
				EOSSDKComponent.GetLobbyInterface().CreateLobbySearch(ref val5, ref _003C_003E8__1.search);
				string text = id.Trim().Replace(" ", "").ToLower();
				if (text.Length == 8)
				{
					UnityEngine.Debug.Log("Starting lobby search via short code: " + text);
					LobbySearchSetParameterOptions val6 = default;
					AttributeData value = default;
					value.Key = Utf8String.op_Implicit("shortCode");
					value.Value = AttributeDataValue.op_Implicit(text);
					val6.Parameter = value;
					val6.ComparisonOp = (ComparisonOp)0;
					LobbySearchSetParameterOptions val7 = val6;
					_003C_003E8__1.search.SetParameter(ref val7);
				}
				else
				{
					UnityEngine.Debug.Log("Starting lobby search via id: " + id);
					LobbySearchSetLobbyIdOptions val8 = default;
					val8.LobbyId = Utf8String.op_Implicit(id);
					LobbySearchSetLobbyIdOptions val9 = val8;
					_003C_003E8__1.search.SetLobbyId(ref val9);
				}
				_003C_003E8__1.task = new UniTaskCompletionSource<LobbyDetails>();
				LobbySearchFindOptions val10 = default;
				val10.LocalUserId = EOSSDKComponent.LocalUserProductId;
				LobbySearchFindOptions val11 = val10;
				_003C_003E8__1.search.Find(ref val11, (object)null, (LobbySearchOnFindCallback)delegate(ref LobbySearchFindCallbackInfo callback)
				{
					//IL_0001: Unknown result type (might be due to invalid IL or missing references)
					//IL_0021: Unknown result type (might be due to invalid IL or missing references)
					//IL_0027: Unknown result type (might be due to invalid IL or missing references)
					//IL_0028: Unknown result type (might be due to invalid IL or missing references)
					//IL_000f: Unknown result type (might be due to invalid IL or missing references)
					//IL_0050: Unknown result type (might be due to invalid IL or missing references)
					//IL_005e: Unknown result type (might be due to invalid IL or missing references)
					//IL_0060: Unknown result type (might be due to invalid IL or missing references)
					//IL_006b: Unknown result type (might be due to invalid IL or missing references)
					try
					{
						if ((int)callback.ResultCode != 0)
						{
							_003C_003E8__1.task.TrySetException((Exception)new EOSResultException(callback.ResultCode));
						}
						LobbySearchGetSearchResultCountOptions val12 = default;
						if (_003C_003E8__1.search.GetSearchResultCount(ref val12) == 0)
						{
							_003C_003E8__1.task.TrySetException((Exception)new EOSResultException((Result)18));
						}
						else
						{
							LobbySearchCopySearchResultByIndexOptions val13 = default;
							val13.LobbyIndex = 0u;
							LobbySearchCopySearchResultByIndexOptions val14 = val13;
							LobbyDetails val15 = default;
							_003C_003E8__1.search.CopySearchResultByIndex(ref val14, ref val15);
							_003C_003E8__1.task.TrySetResult(val15);
						}
					}
					finally
					{
						_003C_003E8__1.search.Release();
					}
				});
				val = _003C_003E8__1.task.Task.GetAwaiter();
				if (!val.IsCompleted)
				{
					num = (_003C_003E1__state = 1);
					_003C_003Eu__2 = val;
					_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter<LobbyDetails>, _003CGetLobbyById_003Ed__41>(ref val, ref this);
					return;
				}
				goto IL_024a;
				IL_024a:
				result = val.GetResult();
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
			_003C_003Et__builder.SetResult(result);
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
	private struct _003CHandleUserLeavingGame_003Ed__35 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder _003C_003Et__builder;

		public string id;

		public LobbyServiceEOS _003C_003E4__this;

		private void MoveNext()
		{
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			//IL_0086: Unknown result type (might be due to invalid IL or missing references)
			//IL_0087: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b6: Expected Obj, but got Unknown
			LobbyServiceEOS lobbyServiceEOS = _003C_003E4__this;
			try
			{
				string id = this.id;
				if (lobbyServiceEOS._currentLobby != null && lobbyServiceEOS._currentLobby.lobbyMembers.Find((EpicLobbyMember e) => ((object)e.handle).ToString() == id) != null)
				{
					KickMemberOptions val = default;
					val.LobbyId = Utf8String.op_Implicit(lobbyServiceEOS._currentLobby.id);
					val.LocalUserId = EOSSDKComponent.LocalUserProductId;
					val.TargetUserId = ProductUserId.FromString(Utf8String.op_Implicit(id));
					KickMemberOptions val2 = val;
					UnityEngine.Debug.Log("Kicking user " + id);
					EOSSDKComponent.GetLobbyInterface().KickMember(ref val2, (object)null, (OnKickMemberCallback)delegate(ref KickMemberCallbackInfo data)
					{
						//IL_0001: Unknown result type (might be due to invalid IL or missing references)
						//IL_0019: Unknown result type (might be due to invalid IL or missing references)
						//IL_001e: Unknown result type (might be due to invalid IL or missing references)
						if ((int)data.ResultCode != 0)
						{
							UnityEngine.Debug.Log("Failed to kick user " + id + " from EOS lobby: " + ((object)data.ResultCode/*cast due to constrained. prefix*/).ToString());
						}
						else
						{
							UnityEngine.Debug.Log("Kicked user " + id + " from EOS lobby");
						}
					});
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
	private struct _003CJoinLobby_003Ed__33 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder _003C_003Et__builder;

		public LobbyServiceEOS _003C_003E4__this;

		public object lobby;

		private _003C_003Ec__DisplayClass33_0 _003C_003E8__1;

		private LobbyDetails _003Cdetails_003E5__2;

		private bool _003CownsDetails_003E5__3;

		private Awaiter<LobbyDetails> _003C_003Eu__1;

		private Awaiter _003C_003Eu__2;

		private void MoveNext()
		{
			//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
			//IL_0135: Unknown result type (might be due to invalid IL or missing references)
			//IL_013a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0142: Unknown result type (might be due to invalid IL or missing references)
			//IL_0197: Unknown result type (might be due to invalid IL or missing references)
			//IL_019c: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
			//IL_0159: Unknown result type (might be due to invalid IL or missing references)
			//IL_015e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0162: Unknown result type (might be due to invalid IL or missing references)
			//IL_0167: Unknown result type (might be due to invalid IL or missing references)
			//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f4: Unknown result type (might be due to invalid IL or missing references)
			//IL_0207: Unknown result type (might be due to invalid IL or missing references)
			//IL_0211: Expected Obj, but got Unknown
			//IL_0225: Unknown result type (might be due to invalid IL or missing references)
			//IL_022f: Expected Obj, but got Unknown
			//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
			//IL_0100: Unknown result type (might be due to invalid IL or missing references)
			//IL_0105: Unknown result type (might be due to invalid IL or missing references)
			//IL_017c: Unknown result type (might be due to invalid IL or missing references)
			//IL_017e: Unknown result type (might be due to invalid IL or missing references)
			//IL_027d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0282: Unknown result type (might be due to invalid IL or missing references)
			//IL_028a: Unknown result type (might be due to invalid IL or missing references)
			//IL_023f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0244: Unknown result type (might be due to invalid IL or missing references)
			//IL_0248: Unknown result type (might be due to invalid IL or missing references)
			//IL_024d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0076: Unknown result type (might be due to invalid IL or missing references)
			//IL_007b: Unknown result type (might be due to invalid IL or missing references)
			//IL_007f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0084: Unknown result type (might be due to invalid IL or missing references)
			//IL_011a: Unknown result type (might be due to invalid IL or missing references)
			//IL_011c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0262: Unknown result type (might be due to invalid IL or missing references)
			//IL_0264: Unknown result type (might be due to invalid IL or missing references)
			//IL_0099: Unknown result type (might be due to invalid IL or missing references)
			//IL_009b: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			LobbyServiceEOS lobbyServiceEOS = _003C_003E4__this;
			try
			{
				Awaiter<LobbyDetails> val2;
				Awaiter val;
				LobbyDetails result;
				UniTask val4;
				JoinLobbyOptions val5;
				JoinLobbyOptions val6;
				switch (num)
				{
				default:
				{
					_003C_003E8__1 = new _003C_003Ec__DisplayClass33_0();
					_003C_003E8__1._003C_003E4__this = _003C_003E4__this;
					_003CownsDetails_003E5__3 = false;
					object obj = lobby;
					LobbyDetails val3 = (LobbyDetails)((obj is LobbyDetails) ? obj : null);
					if (val3 != null)
					{
						_003Cdetails_003E5__2 = val3;
						goto IL_00f6;
					}
					if (lobby is string id)
					{
						val2 = lobbyServiceEOS.GetLobbyById(id).GetAwaiter();
						if (!val2.IsCompleted)
						{
							num = (_003C_003E1__state = 0);
							_003C_003Eu__1 = val2;
							_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter<LobbyDetails>, _003CJoinLobby_003Ed__33>(ref val2, ref this);
							return;
						}
						goto IL_00d0;
					}
					throw new DewException(DewExceptionType.LobbyNotFound);
				}
				case 0:
					val2 = _003C_003Eu__1;
					_003C_003Eu__1 = default;
					num = (_003C_003E1__state = -1);
					goto IL_00d0;
				case 1:
					val = _003C_003Eu__2;
					_003C_003Eu__2 = default;
					num = (_003C_003E1__state = -1);
					goto IL_0151;
				case 2:
					val = _003C_003Eu__2;
					_003C_003Eu__2 = default;
					num = (_003C_003E1__state = -1);
					goto IL_01b3;
				case 3:
					break;
					IL_00d0:
					result = val2.GetResult();
					_003Cdetails_003E5__2 = result;
					_003CownsDetails_003E5__3 = true;
					goto IL_00f6;
					IL_00f6:
					val4 = lobbyServiceEOS.EnsureInitialized();
					val = val4.GetAwaiter();
					if (!val.IsCompleted)
					{
						num = (_003C_003E1__state = 1);
						_003C_003Eu__2 = val;
						_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter, _003CJoinLobby_003Ed__33>(ref val, ref this);
						return;
					}
					goto IL_0151;
					IL_0151:
					val.GetResult();
					val4 = lobbyServiceEOS.LeaveLobby();
					val = val4.GetAwaiter();
					if (!val.IsCompleted)
					{
						num = (_003C_003E1__state = 2);
						_003C_003Eu__2 = val;
						_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter, _003CJoinLobby_003Ed__33>(ref val, ref this);
						return;
					}
					goto IL_01b3;
					IL_01b3:
					val.GetResult();
					val5 = default;
					val5.LobbyDetailsHandle = _003Cdetails_003E5__2;
					val5.LocalUserId = EOSSDKComponent.LocalUserProductId;
					val5.PresenceEnabled = false;
					val5.CrossplayOptOut = !CROSSPLAY;
					val6 = val5;
					ManagerBase<TransitionManager>.instance.UpdateLoadingStatus(LoadingStatus.ConnectingToLobby);
					_003C_003E8__1.task = new UniTaskCompletionSource();
					EOSSDKComponent.GetLobbyInterface().JoinLobby(ref val6, (object)null, (OnJoinLobbyCallback)delegate(ref JoinLobbyCallbackInfo callback)
					{
						//IL_0001: Unknown result type (might be due to invalid IL or missing references)
						//IL_0026: Unknown result type (might be due to invalid IL or missing references)
						//IL_0045: Unknown result type (might be due to invalid IL or missing references)
						//IL_0046: Unknown result type (might be due to invalid IL or missing references)
						//IL_0050: Unknown result type (might be due to invalid IL or missing references)
						//IL_000f: Unknown result type (might be due to invalid IL or missing references)
						try
						{
							if ((int)callback.ResultCode != 0)
							{
								_003C_003E8__1.task.TrySetException((Exception)new EOSResultException(callback.ResultCode));
							}
							else
							{
								CopyLobbyDetailsHandleOptions val7 = default;
								val7.LobbyId = callback.LobbyId;
								val7.LocalUserId = EOSSDKComponent.LocalUserProductId;
								CopyLobbyDetailsHandleOptions val8 = val7;
								LobbyDetails h = default;
								EOSSDKComponent.GetLobbyInterface().CopyLobbyDetailsHandle(ref val8, ref h);
								LobbyInstanceEpic orCreate = _003C_003E8__1._003C_003E4__this.GetOrCreate(Utf8String.op_Implicit(callback.LobbyId));
								orCreate.ApplyFromHandle(h);
								orCreate.isLobbyLeader = false;
								_003C_003E8__1._003C_003E4__this.SetCurrentLobbyData(orCreate);
								_003C_003E8__1.task.TrySetResult();
							}
						}
						catch (Exception ex)
						{
							_003C_003E8__1.task.TrySetException(ex);
						}
					});
					break;
				}
				try
				{
					if (num != 3)
					{
						val4 = _003C_003E8__1.task.Task;
						val = val4.GetAwaiter();
						if (!val.IsCompleted)
						{
							num = (_003C_003E1__state = 3);
							_003C_003Eu__2 = val;
							_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter, _003CJoinLobby_003Ed__33>(ref val, ref this);
							return;
						}
					}
					else
					{
						val = _003C_003Eu__2;
						_003C_003Eu__2 = default;
						num = (_003C_003E1__state = -1);
					}
					val.GetResult();
				}
				finally
				{
					if (num < 0 && _003CownsDetails_003E5__3)
					{
						_003Cdetails_003E5__2.Release();
					}
				}
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				_003C_003E8__1 = null;
				_003Cdetails_003E5__2 = null;
				_003C_003Et__builder.SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			_003C_003E8__1 = null;
			_003Cdetails_003E5__2 = null;
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
	private struct _003CLeaveLobby_003Ed__34 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder _003C_003Et__builder;

		public LobbyServiceEOS _003C_003E4__this;

		private _003C_003Ec__DisplayClass34_0 _003C_003E8__1;

		private Awaiter _003C_003Eu__1;

		private TaskAwaiter _003C_003Eu__2;

		private void MoveNext()
		{
			//IL_0074: Unknown result type (might be due to invalid IL or missing references)
			//IL_0079: Unknown result type (might be due to invalid IL or missing references)
			//IL_0080: Unknown result type (might be due to invalid IL or missing references)
			//IL_0104: Unknown result type (might be due to invalid IL or missing references)
			//IL_010e: Expected Obj, but got Unknown
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			//IL_019d: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
			//IL_01de: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e8: Expected Obj, but got Unknown
			//IL_0132: Unknown result type (might be due to invalid IL or missing references)
			//IL_015b: Unknown result type (might be due to invalid IL or missing references)
			//IL_015d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0173: Unknown result type (might be due to invalid IL or missing references)
			//IL_017d: Expected Obj, but got Unknown
			//IL_0237: Unknown result type (might be due to invalid IL or missing references)
			//IL_023c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0243: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ff: Unknown result type (might be due to invalid IL or missing references)
			//IL_0204: Unknown result type (might be due to invalid IL or missing references)
			//IL_0207: Unknown result type (might be due to invalid IL or missing references)
			//IL_020c: Unknown result type (might be due to invalid IL or missing references)
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			//IL_005b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0220: Unknown result type (might be due to invalid IL or missing references)
			//IL_0221: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			LobbyServiceEOS lobbyServiceEOS = _003C_003E4__this;
			try
			{
				UniTask val2;
				Awaiter val;
				TaskAwaiter taskAwaiter;
				LeaveLobbyOptions val5;
				LeaveLobbyOptions val6;
				switch (num)
				{
				default:
					_003C_003E8__1 = new _003C_003Ec__DisplayClass34_0();
					if (lobbyServiceEOS.currentLobby != null)
					{
						val2 = lobbyServiceEOS.EnsureInitialized();
						val = val2.GetAwaiter();
						if (!val.IsCompleted)
						{
							num = (_003C_003E1__state = 0);
							_003C_003Eu__1 = val;
							_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter, _003CLeaveLobby_003Ed__34>(ref val, ref this);
							return;
						}
						goto IL_008f;
					}
					goto end_IL_000e;
				case 0:
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default;
					num = (_003C_003E1__state = -1);
					goto IL_008f;
				case 1:
					taskAwaiter = _003C_003Eu__2;
					_003C_003Eu__2 = default;
					num = (_003C_003E1__state = -1);
					goto IL_00f7;
				case 2:
					break;
					IL_008f:
					val.GetResult();
					taskAwaiter = lobbyServiceEOS.currentLobby.CleanupOnLobbyLeft("LeaveLobby").GetAwaiter();
					if (!taskAwaiter.IsCompleted)
					{
						num = (_003C_003E1__state = 1);
						_003C_003Eu__2 = taskAwaiter;
						_003C_003Et__builder.AwaitUnsafeOnCompleted<TaskAwaiter, _003CLeaveLobby_003Ed__34>(ref taskAwaiter, ref this);
						return;
					}
					goto IL_00f7;
					IL_00f7:
					taskAwaiter.GetResult();
					_003C_003E8__1.task = new UniTaskCompletionSource();
					if (lobbyServiceEOS.currentLobby.isLobbyLeader)
					{
						UnityEngine.Debug.Log("Deleting EOS lobby");
						ManagerBase<TransitionManager>.instance.UpdateLoadingStatus(LoadingStatus.CleaningUpPreviousLobby);
						DestroyLobbyOptions val3 = default;
						val3.LobbyId = Utf8String.op_Implicit(lobbyServiceEOS.currentLobby.id);
						val3.LocalUserId = EOSSDKComponent.LocalUserProductId;
						DestroyLobbyOptions val4 = val3;
						EOSSDKComponent.GetLobbyInterface().DestroyLobby(ref val4, (object)null, (OnDestroyLobbyCallback)delegate(ref DestroyLobbyCallbackInfo callback)
						{
							//IL_0001: Unknown result type (might be due to invalid IL or missing references)
							//IL_000f: Unknown result type (might be due to invalid IL or missing references)
							if ((int)callback.ResultCode != 0)
							{
								_003C_003E8__1.task.TrySetException((Exception)new EOSResultException(callback.ResultCode));
							}
							else
							{
								_003C_003E8__1.task.TrySetResult();
							}
						});
						lobbyServiceEOS.SetCurrentLobbyData(null);
						break;
					}
					UnityEngine.Debug.Log("Leaving EOS lobby");
					ManagerBase<TransitionManager>.instance.UpdateLoadingStatus(LoadingStatus.LeavingPreviousLobby);
					val5 = default;
					val5.LobbyId = Utf8String.op_Implicit(lobbyServiceEOS.currentLobby.id);
					val5.LocalUserId = EOSSDKComponent.LocalUserProductId;
					val6 = val5;
					EOSSDKComponent.GetLobbyInterface().LeaveLobby(ref val6, (object)null, (OnLeaveLobbyCallback)delegate(ref LeaveLobbyCallbackInfo callback)
					{
						//IL_0001: Unknown result type (might be due to invalid IL or missing references)
						//IL_0009: Unknown result type (might be due to invalid IL or missing references)
						//IL_0010: Invalid comparison between Unknown and I4
						//IL_0019: Unknown result type (might be due to invalid IL or missing references)
						if ((int)callback.ResultCode != 0 && (int)callback.ResultCode != 18)
						{
							_003C_003E8__1.task.TrySetException((Exception)new EOSResultException(callback.ResultCode));
						}
						else
						{
							_003C_003E8__1.task.TrySetResult();
						}
					});
					lobbyServiceEOS.SetCurrentLobbyData(null);
					break;
				}
				try
				{
					if (num != 2)
					{
						val2 = _003C_003E8__1.task.Task;
						val = val2.GetAwaiter();
						if (!val.IsCompleted)
						{
							num = (_003C_003E1__state = 2);
							_003C_003Eu__1 = val;
							_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter, _003CLeaveLobby_003Ed__34>(ref val, ref this);
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
				catch (Exception message)
				{
					UnityEngine.Debug.Log("Leave EOS lobby failed");
					UnityEngine.Debug.Log(message);
				}
				UnityEngine.Debug.Log("EOS Lobby left; LeaveLobby()");
				end_IL_000e:;
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
	private struct _003CRefreshCurrentLobbyFromBackend_003Ed__37 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder _003C_003Et__builder;

		public LobbyServiceEOS _003C_003E4__this;

		private string _003ClobbyId_003E5__2;

		private Awaiter<LobbyDetails> _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_0076: Unknown result type (might be due to invalid IL or missing references)
			//IL_007b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0082: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			//IL_005c: Unknown result type (might be due to invalid IL or missing references)
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			LobbyServiceEOS lobbyServiceEOS = _003C_003E4__this;
			try
			{
				if (num == 0)
				{
					goto IL_0030;
				}
				if (lobbyServiceEOS._currentLobby != null)
				{
					_003ClobbyId_003E5__2 = lobbyServiceEOS._currentLobby.id;
					goto IL_0030;
				}
				goto end_IL_000e;
				IL_0030:
				try
				{
					Awaiter<LobbyDetails> val;
					if (num != 0)
					{
						val = lobbyServiceEOS.GetLobbyById(_003ClobbyId_003E5__2).GetAwaiter();
						if (!val.IsCompleted)
						{
							num = (_003C_003E1__state = 0);
							_003C_003Eu__1 = val;
							_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter<LobbyDetails>, _003CRefreshCurrentLobbyFromBackend_003Ed__37>(ref val, ref this);
							return;
						}
					}
					else
					{
						val = _003C_003Eu__1;
						_003C_003Eu__1 = default;
						num = (_003C_003E1__state = -1);
					}
					LobbyDetails result = val.GetResult();
					if (!((Handle)(object)result == (Handle)null))
					{
						if (lobbyServiceEOS._currentLobby == null || lobbyServiceEOS._currentLobby.id != _003ClobbyId_003E5__2)
						{
							result.Release();
						}
						else
						{
							lobbyServiceEOS._currentLobby.ApplyFromHandle(result);
							lobbyServiceEOS.SetCurrentLobbyData(lobbyServiceEOS._currentLobby);
						}
					}
				}
				catch (Exception ex)
				{
					UnityEngine.Debug.LogWarning("[LobbyServiceEOS] RefreshCurrentLobbyFromBackend failed: " + ex.Message);
				}
				end_IL_000e:;
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				_003ClobbyId_003E5__2 = null;
				_003C_003Et__builder.SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			_003ClobbyId_003E5__2 = null;
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
	private struct _003CSetLobbyAttribute_003Ed__38 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder _003C_003Et__builder;

		public LobbyServiceEOS _003C_003E4__this;

		public string key;

		public object value;

		public bool isPublic;

		private Awaiter _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_019d: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
			//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Expected Obj, but got Unknown
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			//IL_005e: Expected Obj, but got Unknown
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			//IL_0089: Unknown result type (might be due to invalid IL or missing references)
			//IL_008b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0096: Unknown result type (might be due to invalid IL or missing references)
			//IL_009b: Unknown result type (might be due to invalid IL or missing references)
			//IL_009d: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
			//IL_0101: Unknown result type (might be due to invalid IL or missing references)
			//IL_0123: Unknown result type (might be due to invalid IL or missing references)
			//IL_012b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0139: Unknown result type (might be due to invalid IL or missing references)
			//IL_013b: Unknown result type (might be due to invalid IL or missing references)
			//IL_014c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0156: Expected Obj, but got Unknown
			//IL_0162: Unknown result type (might be due to invalid IL or missing references)
			//IL_0167: Unknown result type (might be due to invalid IL or missing references)
			//IL_016b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0170: Unknown result type (might be due to invalid IL or missing references)
			//IL_0185: Unknown result type (might be due to invalid IL or missing references)
			//IL_0187: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			LobbyServiceEOS lobbyServiceEOS = _003C_003E4__this;
			try
			{
				Awaiter val;
				if (num == 0)
				{
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default;
					num = (_003C_003E1__state = -1);
					goto IL_01b9;
				}
				LobbyServiceEOS lobbyServiceEOS2 = _003C_003E4__this;
				if (lobbyServiceEOS.currentLobby == null)
				{
					UnityEngine.Debug.LogWarning("SetLobbyAttribute(" + key + ") skipped - no current lobby");
				}
				else
				{
					UniTaskCompletionSource task = new UniTaskCompletionSource();
					LobbyModification val2 = new LobbyModification();
					UpdateLobbyModificationOptions val3 = default;
					val3.LobbyId = Utf8String.op_Implicit(lobbyServiceEOS.currentLobby.id);
					val3.LocalUserId = EOSSDKComponent.LocalUserProductId;
					UpdateLobbyModificationOptions val4 = val3;
					Result val5 = EOSSDKComponent.GetLobbyInterface().UpdateLobbyModification(ref val4, ref val2);
					if ((int)val5 == 0 && !((Handle)(object)val2 == (Handle)null))
					{
						LobbyModificationAddAttributeOptions val6 = default;
						AttributeData val7 = default;
						val7.Key = Utf8String.op_Implicit(key);
						val7.Value = value.ToAttrDataValue();
						val6.Attribute = val7;
						val6.Visibility = (LobbyAttributeVisibility)(isPublic ? 0 : 0);
						val2.AddAttribute(ref val6);
						UpdateLobbyOptions val8 = default;
						val8.LobbyModificationHandle = val2;
						UpdateLobbyOptions val9 = val8;
						EOSSDKComponent.GetLobbyInterface().UpdateLobby(ref val9, (object)null, (OnUpdateLobbyCallback)delegate(ref UpdateLobbyCallbackInfo callback)
						{
							//IL_0001: Unknown result type (might be due to invalid IL or missing references)
							//IL_0009: Unknown result type (might be due to invalid IL or missing references)
							//IL_0010: Invalid comparison between Unknown and I4
							//IL_0019: Unknown result type (might be due to invalid IL or missing references)
							if ((int)callback.ResultCode != 0 && (int)callback.ResultCode != 20)
							{
								task.TrySetException((Exception)new EOSResultException(callback.ResultCode));
							}
							else if (lobbyServiceEOS2._currentLobby == null)
							{
								task.TrySetResult();
							}
							else
							{
								if (lobbyServiceEOS2._currentLobby.RefreshDetails())
								{
									lobbyServiceEOS2.SetCurrentLobbyData(lobbyServiceEOS2._currentLobby);
								}
								task.TrySetResult();
							}
						});
						val2.Release();
						UniTask task2 = task.Task;
						val = task2.GetAwaiter();
						if (!val.IsCompleted)
						{
							num = (_003C_003E1__state = 0);
							_003C_003Eu__1 = val;
							_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter, _003CSetLobbyAttribute_003Ed__38>(ref val, ref this);
							return;
						}
						goto IL_01b9;
					}
					UnityEngine.Debug.LogWarning($"SetLobbyAttribute({key}) skipped - UpdateLobbyModification failed: {val5}");
				}
				goto end_IL_000e;
				IL_01b9:
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
	private struct _003CSetLobbyMemberAttribute_003Ed__39 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder _003C_003Et__builder;

		public LobbyServiceEOS _003C_003E4__this;

		public string key;

		public object value;

		public bool isPublic;

		private Awaiter _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_0183: Unknown result type (might be due to invalid IL or missing references)
			//IL_0188: Unknown result type (might be due to invalid IL or missing references)
			//IL_0190: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Expected Obj, but got Unknown
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Expected Obj, but got Unknown
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			//IL_006f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0071: Unknown result type (might be due to invalid IL or missing references)
			//IL_007c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0081: Unknown result type (might be due to invalid IL or missing references)
			//IL_0083: Unknown result type (might be due to invalid IL or missing references)
			//IL_009b: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
			//IL_0109: Unknown result type (might be due to invalid IL or missing references)
			//IL_0111: Unknown result type (might be due to invalid IL or missing references)
			//IL_011f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0121: Unknown result type (might be due to invalid IL or missing references)
			//IL_0132: Unknown result type (might be due to invalid IL or missing references)
			//IL_013c: Expected Obj, but got Unknown
			//IL_0148: Unknown result type (might be due to invalid IL or missing references)
			//IL_014d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0151: Unknown result type (might be due to invalid IL or missing references)
			//IL_0156: Unknown result type (might be due to invalid IL or missing references)
			//IL_016b: Unknown result type (might be due to invalid IL or missing references)
			//IL_016d: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			LobbyServiceEOS lobbyServiceEOS = _003C_003E4__this;
			try
			{
				Awaiter val;
				if (num == 0)
				{
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default;
					num = (_003C_003E1__state = -1);
					goto IL_019f;
				}
				LobbyServiceEOS lobbyServiceEOS2 = _003C_003E4__this;
				if (lobbyServiceEOS.currentLobby != null)
				{
					UniTaskCompletionSource task = new UniTaskCompletionSource();
					LobbyModification val2 = new LobbyModification();
					UpdateLobbyModificationOptions val3 = default;
					val3.LobbyId = Utf8String.op_Implicit(lobbyServiceEOS.currentLobby.id);
					val3.LocalUserId = EOSSDKComponent.LocalUserProductId;
					UpdateLobbyModificationOptions val4 = val3;
					Result val5 = EOSSDKComponent.GetLobbyInterface().UpdateLobbyModification(ref val4, ref val2);
					if ((int)val5 == 0 && !((Handle)(object)val2 == (Handle)null))
					{
						LobbyModificationAddMemberAttributeOptions val6 = default;
						AttributeData val7 = default;
						val7.Key = Utf8String.op_Implicit(key);
						val7.Value = value.ToAttrDataValue();
						val6.Attribute = val7;
						val6.Visibility = (LobbyAttributeVisibility)(isPublic ? 0 : 0);
						val2.AddMemberAttribute(ref val6);
						UpdateLobbyOptions val8 = default;
						val8.LobbyModificationHandle = val2;
						UpdateLobbyOptions val9 = val8;
						EOSSDKComponent.GetLobbyInterface().UpdateLobby(ref val9, (object)null, (OnUpdateLobbyCallback)delegate(ref UpdateLobbyCallbackInfo callback)
						{
							//IL_0001: Unknown result type (might be due to invalid IL or missing references)
							//IL_0009: Unknown result type (might be due to invalid IL or missing references)
							//IL_0010: Invalid comparison between Unknown and I4
							//IL_0019: Unknown result type (might be due to invalid IL or missing references)
							if ((int)callback.ResultCode != 0 && (int)callback.ResultCode != 20)
							{
								task.TrySetException((Exception)new EOSResultException(callback.ResultCode));
							}
							else if (lobbyServiceEOS2._currentLobby == null)
							{
								task.TrySetResult();
							}
							else
							{
								if (lobbyServiceEOS2._currentLobby.RefreshDetails())
								{
									lobbyServiceEOS2.SetCurrentLobbyData(lobbyServiceEOS2._currentLobby);
								}
								task.TrySetResult();
							}
						});
						val2.Release();
						UniTask task2 = task.Task;
						val = task2.GetAwaiter();
						if (!val.IsCompleted)
						{
							num = (_003C_003E1__state = 0);
							_003C_003Eu__1 = val;
							_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter, _003CSetLobbyMemberAttribute_003Ed__39>(ref val, ref this);
							return;
						}
						goto IL_019f;
					}
					UnityEngine.Debug.LogWarning($"SetLobbyMemberAttribute({key}) skipped - UpdateLobbyModification failed: {val5}");
				}
				goto end_IL_000e;
				IL_019f:
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
	private struct _003CSetLobbyShortId_003Ed__40 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder _003C_003Et__builder;

		public LobbyServiceEOS _003C_003E4__this;

		private Awaiter _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_0070: Unknown result type (might be due to invalid IL or missing references)
			//IL_0075: Unknown result type (might be due to invalid IL or missing references)
			//IL_007c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_0059: Unknown result type (might be due to invalid IL or missing references)
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			LobbyServiceEOS lobbyServiceEOS = _003C_003E4__this;
			try
			{
				Awaiter val;
				if (num == 0)
				{
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default;
					num = (_003C_003E1__state = -1);
					goto IL_008b;
				}
				if (lobbyServiceEOS._currentLobby != null)
				{
					string value = _003CSetLobbyShortId_003Eg__GenerateHash_007C40_0(lobbyServiceEOS.currentLobby.id);
					UniTask val2 = lobbyServiceEOS.SetLobbyAttribute("shortCode", value);
					val = val2.GetAwaiter();
					if (!val.IsCompleted)
					{
						num = (_003C_003E1__state = 0);
						_003C_003Eu__1 = val;
						_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter, _003CSetLobbyShortId_003Ed__40>(ref val, ref this);
						return;
					}
					goto IL_008b;
				}
				goto end_IL_000e;
				IL_008b:
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

	public const int ShortLobbyIdLength = 8;

	public const string ConnectStringPrefix = "sod:";

	private readonly Dictionary<string, LobbyInstanceEpic> _lobbies = new Dictionary<string, LobbyInstanceEpic>();

	public readonly string[] AttributeKeys = new string[8] { "version", "crossPlayGate", "hasGameStarted", "isInviteOnly", "name", "difficulty", "hostAddress", "shortCode" };

	private LobbyInstanceEpic _currentLobby;

	private ulong _lobbyMemberStatusNotifyId;

	private ulong _lobbyAttributeUpdateNotifyId;

	public static bool CROSSPLAY = true;

	private static readonly Dictionary<string, string> _countryToContinent = BuildCountryToContinentMap();

	private static readonly HashSet<string> _nearContinentPairs = new HashSet<string> { "EU|NA", "NA|EU", "EU|AF", "AF|EU", "EU|AS", "AS|EU", "NA|SA", "SA|NA", "AS|OC", "OC|AS" };

	private string _publishedConnectString;

	public override LobbyInstance currentLobby => _currentLobby;

	private LobbyInstanceEpic GetOrCreate(string lobbyId)
	{
		if (!_lobbies.TryGetValue(lobbyId, out var value))
		{
			value = (_lobbies[lobbyId] = new LobbyInstanceEpic());
		}
		return value;
	}

	public static string GetCrossPlayGate()
	{
		if (!CROSSPLAY || DewMod.isAnyModActive)
		{
			return "STEAM";
		}
		return "ALL";
	}

	public string GetInitialAttr_version()
	{
		return Dew.GetCurrentMultiplayerCompatibilityVersion();
	}

	public string GetInitialAttr_crossPlayGate()
	{
		return GetCrossPlayGate();
	}

	public virtual string GetInitialAttr_hostAddress()
	{
		return EOSSDKComponent.LocalUserProductIdString;
	}

	public bool GetInitialAttr_hasGameStarted()
	{
		return (UnityEngine.Object)(object)NetworkedManagerBase<GameManager>.instance != null;
	}

	public bool GetInitialAttr_allowJoin()
	{
		if (!((UnityEngine.Object)(object)NetworkedManagerBase<GameManager>.softInstance == null))
		{
			if (NetworkedManagerBase<GameSettingsManager>.instance.allowMidJoins != AllowMidJoinType.Disallow)
			{
				return NetworkedManagerBase<GameSettingsManager>.instance.midJoinBanType == MidJoinBanType.None;
			}
			return false;
		}
		return true;
	}

	public bool GetInitialAttr_allowDejavu()
	{
		if (!((UnityEngine.Object)(object)NetworkedManagerBase<GameManager>.softInstance == null))
		{
			return NetworkedManagerBase<GameSettingsManager>.instance.allowDejavu;
		}
		return true;
	}

	public string GetInitialAttr_difficulty()
	{
		if (!((UnityEngine.Object)(object)NetworkedManagerBase<GameManager>.instance != null))
		{
			return "";
		}
		return NetworkedManagerBase<GameManager>.instance.difficulty.name;
	}

	public int GetInitialAttr_maxPlayers()
	{
		return 4;
	}

	public string GetInitialAttr_name()
	{
		if (!string.IsNullOrEmpty(NetworkedManagerBase<GameSettingsManager>.instance.GetLocalPreferredGameSettings().lobbyName))
		{
			return NetworkedManagerBase<GameSettingsManager>.instance.GetLocalPreferredGameSettings().lobbyName;
		}
		return LobbyManager.GetDefaultLobbyName();
	}

	public bool GetInitialAttr_isInviteOnly()
	{
		return DewNetworkManager.startSettings.lobbyType != DewLobbyType.Public;
	}

	public string GetInitialAttr_lucidDreams()
	{
		return NetworkedManagerBase<GameSettingsManager>.instance.GetLocalPreferredGameSettings().lucidDreams.JoinToString(",");
	}

	public string GetInitialAttr_bannedItems()
	{
		return NetworkedManagerBase<GameSettingsManager>.instance.GetLocalPreferredGameSettings().bannedGameItems.JoinToString(",");
	}

	public string GetInitialAttr_tags()
	{
		return NetworkedManagerBase<GameSettingsManager>.instance.GetLocalPreferredGameSettings().tags.JoinToString(",");
	}

	public string GetInitialAttr_description()
	{
		return NetworkedManagerBase<GameSettingsManager>.instance.GetLocalPreferredGameSettings().lobbyDescription;
	}

	public bool GetInitialAttr_isModded()
	{
		return DewMod.isGameplayAltered;
	}

	public string GetInitialAttr_country()
	{
		if (!DewSteam.isInitialized)
		{
			return "";
		}
		return SteamUtils.GetIPCountry();
	}

	public override string GetGameServerAddress()
	{
		return "";
	}

	private async void Start()
	{
		await UniTask.WaitWhile((Func<bool>)(() => !EOSSDKComponent.Initialized), (PlayerLoopTiming)8, default(CancellationToken));
		AddNotifyLobbyMemberStatusReceivedOptions val = default;
		_lobbyMemberStatusNotifyId = EOSSDKComponent.GetLobbyInterface().AddNotifyLobbyMemberStatusReceived(ref val, (object)null, (OnLobbyMemberStatusReceivedCallback)delegate(ref LobbyMemberStatusReceivedCallbackInfo callback)
		{
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Invalid comparison between Unknown and I4
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_006b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0071: Invalid comparison between Unknown and I4
			//IL_0074: Unknown result type (might be due to invalid IL or missing references)
			//IL_007a: Invalid comparison between Unknown and I4
			//IL_007d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0083: Invalid comparison between Unknown and I4
			//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
			if (_currentLobby != null)
			{
				_currentLobby.RefreshDetails();
				_currentLobby.UpdateMemberList();
			}
			if ((int)callback.CurrentStatus == 5)
			{
				LeaveLobby();
			}
			if (_currentLobby != null && _currentLobby.isLobbyLeader && NetworkServer.active && (Handle)(object)callback.TargetUserId != (Handle)(object)EOSSDKComponent.LocalUserProductId && ((int)callback.CurrentStatus == 1 || (int)callback.CurrentStatus == 2 || (int)callback.CurrentStatus == 3))
			{
				string text = ((object)callback.TargetUserId).ToString();
				foreach (KeyValuePair<int, NetworkConnectionToClient> connection in NetworkServer.connections)
				{
					NetworkConnectionToClient value = connection.Value;
					if (value != null && value.address == text)
					{
						UnityEngine.Debug.Log($"[BB EOS] Member {text} status={callback.CurrentStatus}; disconnecting Mirror conn {((NetworkConnection)value).connectionId}");
						((NetworkConnection)value).Disconnect();
						break;
					}
				}
			}
		});
		AddNotifyLobbyUpdateReceivedOptions val2 = default;
		_lobbyAttributeUpdateNotifyId = EOSSDKComponent.GetLobbyInterface().AddNotifyLobbyUpdateReceived(ref val2, (object)null, (OnLobbyUpdateReceivedCallback)delegate
		{
			if (_currentLobby != null && _currentLobby.RefreshDetails())
			{
				SetCurrentLobbyData(_currentLobby);
			}
		});
		UnityEngine.Debug.Log("Added notifications to EOS LobbyInterface");
	}

	private void OnDestroy()
	{
		if (!((UnityEngine.Object)(object)EOSSDKComponent.Instance == null) && !((Handle)(object)EOSSDKComponent.Instance.EOS == (Handle)null))
		{
			EOSSDKComponent.GetLobbyInterface().RemoveNotifyLobbyMemberStatusReceived(_lobbyMemberStatusNotifyId);
			EOSSDKComponent.GetLobbyInterface().RemoveNotifyLobbyUpdateReceived(_lobbyAttributeUpdateNotifyId);
		}
	}

	[AsyncStateMachine(typeof(_003CEnsureInitialized_003Ed__31))]
	private UniTask EnsureInitialized()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		_003CEnsureInitialized_003Ed__31 obj = default;
		obj._003C_003Et__builder = AsyncUniTaskMethodBuilder.Create();
		obj._003C_003E1__state = -1;
		obj._003C_003Et__builder.Start<_003CEnsureInitialized_003Ed__31>(ref obj);
		return obj._003C_003Et__builder.Task;
	}

	[AsyncStateMachine(typeof(_003CCreateLobby_003Ed__32))]
	public override UniTask CreateLobby()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		_003CCreateLobby_003Ed__32 obj = default;
		obj._003C_003Et__builder = AsyncUniTaskMethodBuilder.Create();
		obj._003C_003E4__this = this;
		obj._003C_003E1__state = -1;
		obj._003C_003Et__builder.Start<_003CCreateLobby_003Ed__32>(ref obj);
		return obj._003C_003Et__builder.Task;
	}

	[AsyncStateMachine(typeof(_003CJoinLobby_003Ed__33))]
	public override UniTask JoinLobby(object lobby)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		_003CJoinLobby_003Ed__33 obj = default;
		obj._003C_003Et__builder = AsyncUniTaskMethodBuilder.Create();
		obj._003C_003E4__this = this;
		obj.lobby = lobby;
		obj._003C_003E1__state = -1;
		obj._003C_003Et__builder.Start<_003CJoinLobby_003Ed__33>(ref obj);
		return obj._003C_003Et__builder.Task;
	}

	[AsyncStateMachine(typeof(_003CLeaveLobby_003Ed__34))]
	public override UniTask LeaveLobby()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		_003CLeaveLobby_003Ed__34 obj = default;
		obj._003C_003Et__builder = AsyncUniTaskMethodBuilder.Create();
		obj._003C_003E4__this = this;
		obj._003C_003E1__state = -1;
		obj._003C_003Et__builder.Start<_003CLeaveLobby_003Ed__34>(ref obj);
		return obj._003C_003Et__builder.Task;
	}

	[AsyncStateMachine(typeof(_003CHandleUserLeavingGame_003Ed__35))]
	public override UniTask HandleUserLeavingGame(string id)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		_003CHandleUserLeavingGame_003Ed__35 obj = default;
		obj._003C_003Et__builder = AsyncUniTaskMethodBuilder.Create();
		obj._003C_003E4__this = this;
		obj.id = id;
		obj._003C_003E1__state = -1;
		obj._003C_003Et__builder.Start<_003CHandleUserLeavingGame_003Ed__35>(ref obj);
		return obj._003C_003Et__builder.Task;
	}

	[AsyncStateMachine(typeof(_003CGetLobbies_003Ed__36))]
	public override UniTask GetLobbies(Action<LobbySearchResult> onUpdated, object continuationToken = null)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		_003CGetLobbies_003Ed__36 obj = default;
		obj._003C_003Et__builder = AsyncUniTaskMethodBuilder.Create();
		obj._003C_003E4__this = this;
		obj.onUpdated = onUpdated;
		obj._003C_003E1__state = -1;
		obj._003C_003Et__builder.Start<_003CGetLobbies_003Ed__36>(ref obj);
		return obj._003C_003Et__builder.Task;
	}

	[AsyncStateMachine(typeof(_003CRefreshCurrentLobbyFromBackend_003Ed__37))]
	public override UniTask RefreshCurrentLobbyFromBackend()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		_003CRefreshCurrentLobbyFromBackend_003Ed__37 obj = default;
		obj._003C_003Et__builder = AsyncUniTaskMethodBuilder.Create();
		obj._003C_003E4__this = this;
		obj._003C_003E1__state = -1;
		obj._003C_003Et__builder.Start<_003CRefreshCurrentLobbyFromBackend_003Ed__37>(ref obj);
		return obj._003C_003Et__builder.Task;
	}

	[AsyncStateMachine(typeof(_003CSetLobbyAttribute_003Ed__38))]
	public override UniTask SetLobbyAttribute(string key, object value, bool isPublic = true)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		_003CSetLobbyAttribute_003Ed__38 obj = default;
		obj._003C_003Et__builder = AsyncUniTaskMethodBuilder.Create();
		obj._003C_003E4__this = this;
		obj.key = key;
		obj.value = value;
		obj.isPublic = isPublic;
		obj._003C_003E1__state = -1;
		obj._003C_003Et__builder.Start<_003CSetLobbyAttribute_003Ed__38>(ref obj);
		return obj._003C_003Et__builder.Task;
	}

	[AsyncStateMachine(typeof(_003CSetLobbyMemberAttribute_003Ed__39))]
	public override UniTask SetLobbyMemberAttribute(string key, object value, bool isPublic = true)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		_003CSetLobbyMemberAttribute_003Ed__39 obj = default;
		obj._003C_003Et__builder = AsyncUniTaskMethodBuilder.Create();
		obj._003C_003E4__this = this;
		obj.key = key;
		obj.value = value;
		obj.isPublic = isPublic;
		obj._003C_003E1__state = -1;
		obj._003C_003Et__builder.Start<_003CSetLobbyMemberAttribute_003Ed__39>(ref obj);
		return obj._003C_003Et__builder.Task;
	}

	[AsyncStateMachine(typeof(_003CSetLobbyShortId_003Ed__40))]
	private UniTask SetLobbyShortId()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		_003CSetLobbyShortId_003Ed__40 obj = default;
		obj._003C_003Et__builder = AsyncUniTaskMethodBuilder.Create();
		obj._003C_003E4__this = this;
		obj._003C_003E1__state = -1;
		obj._003C_003Et__builder.Start<_003CSetLobbyShortId_003Ed__40>(ref obj);
		return obj._003C_003Et__builder.Task;
	}

	[AsyncStateMachine(typeof(_003CGetLobbyById_003Ed__41))]
	private UniTask<LobbyDetails> GetLobbyById(string id)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		_003CGetLobbyById_003Ed__41 obj = default;
		obj._003C_003Et__builder = AsyncUniTaskMethodBuilder<LobbyDetails>.Create();
		obj._003C_003E4__this = this;
		obj.id = id;
		obj._003C_003E1__state = -1;
		obj._003C_003Et__builder.Start<_003CGetLobbyById_003Ed__41>(ref obj);
		return obj._003C_003Et__builder.Task;
	}

	private bool TryGetAttribute(List<Attribute> list, string key, out Attribute attr)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		attr = list.Find((Attribute x) =>
		{
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			if (x.Data.HasValue)
			{
				AttributeData value = x.Data.Value;
				return value.Key == Utf8String.op_Implicit(key);
			}
			return false;
		});
		return attr.Data.HasValue;
	}

	private static Dictionary<string, string> BuildCountryToContinentMap()
	{
		Dictionary<string, string> map = new Dictionary<string, string>();
		Add("EU", "AL,AD,AT,BY,BE,BA,BG,HR,CY,CZ,DK,EE,FO,FI,FR,DE,GI,GR,HU,IS,IE,IM,IT,XK,LV,LI,LT,LU,MT,MD,MC,ME,NL,MK,NO,PL,PT,RO,RU,SM,RS,SK,SI,ES,SE,CH,UA,GB,UK,VA,JE,GG");
		Add("NA", "US,CA,MX,GL,BM,BS,CU,JM,HT,DO,PR,GT,BZ,SV,HN,NI,CR,PA,TT,BB,LC,GD,VC,AG,DM,KN,KY,VG,VI,AW,CW,MQ,GP,MS,TC,AI,SX,BQ,PM");
		Add("SA", "BR,AR,CL,CO,PE,VE,EC,BO,PY,UY,GY,SR,GF,FK");
		Add("AS", "CN,JP,KR,KP,TW,HK,MO,MN,IN,PK,BD,LK,NP,BT,MV,AF,IR,IQ,SY,LB,IL,PS,JO,SA,YE,OM,AE,QA,BH,KW,TR,GE,AM,AZ,KZ,KG,TJ,TM,UZ,TH,VN,LA,KH,MM,MY,SG,ID,PH,BN,TL");
		Add("OC", "AU,NZ,FJ,PG,SB,VU,NC,PF,WS,TO,TV,KI,NR,PW,FM,MH,CK,NU,GU,MP,AS");
		Add("AF", "ZA,EG,NG,KE,ET,GH,TZ,UG,DZ,MA,TN,LY,SD,SS,SN,CI,CM,ZW,ZM,MZ,AO,NA,BW,MW,RW,BI,SO,DJ,ER,GA,CG,CD,CF,TD,NE,ML,BF,GN,GW,SL,LR,TG,BJ,MR,GM,CV,ST,GQ,KM,SC,MU,MG,RE,YT,EH,LS,SZ");
		return map;
		void Add(string continent, string codes)
		{
			string[] array = codes.Split(',', StringSplitOptions.None);
			foreach (string key in array)
			{
				map[key] = continent;
			}
		}
	}

	private static LobbyConnectionQuality EstimateConnectionQuality(string localCountry, string hostCountry)
	{
		if (string.IsNullOrEmpty(localCountry) || string.IsNullOrEmpty(hostCountry))
		{
			return LobbyConnectionQuality.Unknown;
		}
		if (localCountry == hostCountry)
		{
			return LobbyConnectionQuality.Best;
		}
		if (!_countryToContinent.TryGetValue(localCountry, out var value) || !_countryToContinent.TryGetValue(hostCountry, out var value2))
		{
			return LobbyConnectionQuality.Unknown;
		}
		if (value == value2)
		{
			return LobbyConnectionQuality.Good;
		}
		if (_nearContinentPairs.Contains(value + "|" + value2))
		{
			return LobbyConnectionQuality.Okay;
		}
		return LobbyConnectionQuality.Bad;
	}

	private void UpdateSteamRichPresenceConnect(LobbyInstanceEpic data)
	{
		if (DewSteam.isInitialized)
		{
			string text = ((data != null && data.allowJoin && !string.IsNullOrEmpty(data.shortCode)) ? ("sod:" + data.shortCode) : null);
			if (!(_publishedConnectString == text))
			{
				_publishedConnectString = text;
				SteamFriends.SetRichPresence("connect", text);
			}
		}
	}

	private void SetCurrentLobbyData(LobbyInstanceEpic data)
	{
		if (_currentLobby == data)
		{
			if (data != null)
			{
				UpdateSteamRichPresenceConnect(data);
				InvokeOnCurrentLobbyChanged();
			}
		}
		else
		{
			_currentLobby = data;
			UpdateSteamRichPresenceConnect(data);
			InvokeOnCurrentLobbyChanged();
		}
	}

	[CompilerGenerated]
	internal static string _003CSetLobbyShortId_003Eg__GenerateHash_007C40_0(string input)
	{
		char[] array = "23456789abcdefghjklmnpqrstuvwxyz".ToCharArray();
		using MD5 mD = MD5.Create();
		byte[] bytes = Encoding.UTF8.GetBytes(input);
		byte[] array2 = mD.ComputeHash(bytes);
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < 8; i++)
		{
			int num = array2[i] % array.Length;
			char value = array[num];
			stringBuilder.Append(value);
		}
		return stringBuilder.ToString();
	}
}
