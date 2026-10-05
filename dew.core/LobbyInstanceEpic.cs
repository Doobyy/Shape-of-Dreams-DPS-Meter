using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Epic.OnlineServices;
using Epic.OnlineServices.Lobby;
using EpicTransport;
using UnityEngine;

public class LobbyInstanceEpic : LobbyInstance
{
	public LobbyDetails details;

	public List<EpicLobbyMember> lobbyMembers = new List<EpicLobbyMember>();

	private bool _leavingCleanupInFlight;

	private string _lastPublishedPlatform;

	private bool _platformAttrInFlight;

	public LobbyInstanceEpic ApplyFromHandle(LobbyDetails h)
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_024c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		if ((Handle)(object)h == (Handle)null)
		{
			CleanupOnLobbyLeft("ApplyFromHandle:details==null");
		}
		if ((Handle)(object)details != (Handle)null && details != h)
		{
			details.Release();
		}
		details = h;
		try
		{
			LobbyDetailsCopyInfoOptions val = default;
			LobbyDetailsInfo? val2 = default;
			h.CopyInfo(ref val, ref val2);
			if (val2.HasValue)
			{
				LobbyDetailsInfo value = val2.Value;
				id = Utf8String.op_Implicit(value.LobbyId);
				value = val2.Value;
				maxPlayers = (int)value.MaxMembers;
				value = val2.Value;
				int num;
				if ((Handle)(object)value.LobbyOwnerUserId != (Handle)null && (Handle)(object)EOSSDKComponent.LocalUserProductId != (Handle)null)
				{
					value = val2.Value;
					num = (((Handle)(object)value.LobbyOwnerUserId == (Handle)(object)EOSSDKComponent.LocalUserProductId) ? 1 : 0);
				}
				else
				{
					num = 0;
				}
				isLobbyLeader = (byte)num != 0;
			}
			name = h.GetAttributeString("name").Trim();
			version = h.GetAttributeString("version");
			crossPlayGate = h.GetAttributeString("crossPlayGate");
			country = h.GetAttributeString("country");
			isInviteOnly = h.GetAttributeBool("isInviteOnly");
			shortCode = h.GetAttributeString("shortCode");
			isModded = h.GetAttributeBool("isModded");
			lobbyName = h.GetAttributeString("lobbyName");
			lobbyDescription = h.GetAttributeString("lobbyDescription");
			lobbyTags = h.GetAttributeStringList("lobbyTags");
			difficulty = h.GetAttributeString("difficulty");
			activeLucidDreams = h.GetAttributeStringList("activeLucidDreams");
			bannedGameItems = h.GetAttributeStringList("bannedGameItems");
			allowMidJoins = h.GetAttributeMidJoin("allowMidJoins");
			allowDejavu = h.GetAttributeBool("allowDejavu");
			customData = h.GetAttributeStringDict("customData");
			hasGameStarted = h.GetAttributeBool("hasGameStarted");
			gameStartTimestamp = h.GetAttributeLong("gameStartTimestamp");
			allowJoin = h.GetAttributeBool("allowJoin");
			savedPlayers = h.GetAttributeStringList("savedPlayers");
			hostAddress = h.GetAttributeString("hostAddress");
			gameServerAddress = h.GetAttributeString("hostAddress");
			LobbyDetailsGetMemberCountOptions val3 = default;
			currentPlayers = (int)h.GetMemberCount(ref val3);
			connectionQuality = (LobbyConnectionQuality)h.GetAttributeLong("connectionQuality");
			UpdateMemberList();
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
		return this;
	}

	public bool RefreshDetails()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		if (string.IsNullOrEmpty(id))
		{
			return false;
		}
		CopyLobbyDetailsHandleOptions val = default;
		val.LobbyId = Utf8String.op_Implicit(id);
		val.LocalUserId = EOSSDKComponent.LocalUserProductId;
		CopyLobbyDetailsHandleOptions val2 = val;
		LobbyDetails val3 = default;
		if ((int)EOSSDKComponent.GetLobbyInterface().CopyLobbyDetailsHandle(ref val2, ref val3) != 0 || (Handle)(object)val3 == (Handle)null)
		{
			return false;
		}
		ApplyFromHandle(val3);
		return true;
	}

	public async Task UpdateMemberList()
	{
		lobbyMembers.Clear();
		LobbyDetailsGetMemberCountOptions val = default;
		uint memberCount = details.GetMemberCount(ref val);
		bool flag = false;
		for (uint num = 0u; num < memberCount; num++)
		{
			LobbyDetailsGetMemberByIndexOptions val2 = default;
			val2.MemberIndex = num;
			LobbyDetailsGetMemberByIndexOptions val3 = val2;
			ProductUserId memberByIndex = details.GetMemberByIndex(ref val3);
			if (!((Handle)(object)memberByIndex == (Handle)null))
			{
				if (((object)memberByIndex).ToString() == EOSSDKComponent.LocalUserProductIdString)
				{
					flag = true;
				}
				string memberAttributeString = details.GetMemberAttributeString(memberByIndex, "platform");
				string memberAttributeString2 = details.GetMemberAttributeString(memberByIndex, "sessionID");
				string memberAttributeString3 = details.GetMemberAttributeString(memberByIndex, "platformID");
				lobbyMembers.Add(new EpicLobbyMember
				{
					handle = memberByIndex,
					platform = memberAttributeString,
					sessionID = memberAttributeString2,
					platformID = memberAttributeString3
				});
			}
		}
		if (!flag)
		{
			CleanupOnLobbyLeft("ApplyFromHandle:local-not-member");
		}
		currentPlayers = (int)memberCount;
		await EnsureMyPlatformMemberAttr();
		await EnsureSessionOwner("psn");
	}

	private string ResolveLocalPlatformTag()
	{
		return "steam";
	}

	private async Task EnsureMyPlatformMemberAttr()
	{
		if (ManagerBase<LobbyManager>.instance.service.currentLobby == null)
		{
			return;
		}
		string plat = ResolveLocalPlatformTag();
		if (string.IsNullOrEmpty(plat) || _platformAttrInFlight || _lastPublishedPlatform == plat)
		{
			return;
		}
		_platformAttrInFlight = true;
		try
		{
			await ManagerBase<LobbyManager>.instance.service.SetLobbyMemberAttribute("platform", plat);
			_lastPublishedPlatform = plat;
		}
		catch (Exception)
		{
			_lastPublishedPlatform = null;
		}
		finally
		{
			_platformAttrInFlight = false;
		}
	}

	private async Task EnsureSessionOwner(string platform)
	{
		if (lobbyMembers.Count == 0)
		{
			return;
		}
		string attributeString = details.GetAttributeString("currentSessionOwner" + platform);
		if ((!string.IsNullOrEmpty(attributeString) && IsMember(attributeString)) || !isLobbyLeader)
		{
			return;
		}
		foreach (EpicLobbyMember lobbyMember in lobbyMembers)
		{
			if (lobbyMember.platform == platform)
			{
				await ManagerBase<LobbyManager>.instance.service.SetLobbyAttribute("currentSessionOwner" + platform, ((object)lobbyMember.handle).ToString(), isPublic: false);
				return;
			}
		}
		bool IsMember(string member)
		{
			foreach (EpicLobbyMember lobbyMember2 in lobbyMembers)
			{
				if (((object)lobbyMember2.handle).ToString() == member)
				{
					return true;
				}
			}
			return false;
		}
	}
}
