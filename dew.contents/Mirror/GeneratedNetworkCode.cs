using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Mirror;

[StructLayout(LayoutKind.Auto, CharSet = CharSet.Auto)]
public static class GeneratedNetworkCode
{
	public static TimeSnapshotMessage _Read_Mirror_002ETimeSnapshotMessage(NetworkReader reader)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		return default;
	}

	public static void _Write_Mirror_002ETimeSnapshotMessage(NetworkWriter writer, TimeSnapshotMessage value)
	{
	}

	public static ReadyMessage _Read_Mirror_002EReadyMessage(NetworkReader reader)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		return default;
	}

	public static void _Write_Mirror_002EReadyMessage(NetworkWriter writer, ReadyMessage value)
	{
	}

	public static NotReadyMessage _Read_Mirror_002ENotReadyMessage(NetworkReader reader)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		return default;
	}

	public static void _Write_Mirror_002ENotReadyMessage(NetworkWriter writer, NotReadyMessage value)
	{
	}

	public static AddPlayerMessage _Read_Mirror_002EAddPlayerMessage(NetworkReader reader)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		return default;
	}

	public static void _Write_Mirror_002EAddPlayerMessage(NetworkWriter writer, AddPlayerMessage value)
	{
	}

	public static SceneMessage _Read_Mirror_002ESceneMessage(NetworkReader reader)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		return new SceneMessage
		{
			sceneName = NetworkReaderExtensions.ReadString(reader),
			sceneOperation = _Read_Mirror_002ESceneOperation(reader),
			customHandling = NetworkReaderExtensions.ReadBool(reader)
		};
	}

	public static SceneOperation _Read_Mirror_002ESceneOperation(NetworkReader reader)
	{
		return (SceneOperation)NetworkReaderExtensions.ReadByte(reader);
	}

	public static void _Write_Mirror_002ESceneMessage(NetworkWriter writer, SceneMessage value)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		NetworkWriterExtensions.WriteString(writer, value.sceneName);
		_Write_Mirror_002ESceneOperation(writer, value.sceneOperation);
		NetworkWriterExtensions.WriteBool(writer, value.customHandling);
	}

	public static void _Write_Mirror_002ESceneOperation(NetworkWriter writer, SceneOperation value)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Expected I4, but got Unknown
		NetworkWriterExtensions.WriteByte(writer, (byte)(int)value);
	}

	public static CommandMessage _Read_Mirror_002ECommandMessage(NetworkReader reader)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		return new CommandMessage
		{
			netId = NetworkReaderExtensions.ReadUInt(reader),
			componentIndex = NetworkReaderExtensions.ReadByte(reader),
			functionHash = NetworkReaderExtensions.ReadUShort(reader),
			payload = NetworkReaderExtensions.ReadBytesAndSizeSegment(reader)
		};
	}

	public static void _Write_Mirror_002ECommandMessage(NetworkWriter writer, CommandMessage value)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		NetworkWriterExtensions.WriteUInt(writer, value.netId);
		NetworkWriterExtensions.WriteByte(writer, value.componentIndex);
		NetworkWriterExtensions.WriteUShort(writer, value.functionHash);
		NetworkWriterExtensions.WriteBytesAndSizeSegment(writer, value.payload);
	}

	public static RpcMessage _Read_Mirror_002ERpcMessage(NetworkReader reader)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		return new RpcMessage
		{
			netId = NetworkReaderExtensions.ReadUInt(reader),
			componentIndex = NetworkReaderExtensions.ReadByte(reader),
			functionHash = NetworkReaderExtensions.ReadUShort(reader),
			payload = NetworkReaderExtensions.ReadBytesAndSizeSegment(reader)
		};
	}

	public static void _Write_Mirror_002ERpcMessage(NetworkWriter writer, RpcMessage value)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		NetworkWriterExtensions.WriteUInt(writer, value.netId);
		NetworkWriterExtensions.WriteByte(writer, value.componentIndex);
		NetworkWriterExtensions.WriteUShort(writer, value.functionHash);
		NetworkWriterExtensions.WriteBytesAndSizeSegment(writer, value.payload);
	}

	public static RpcBufferMessage _Read_Mirror_002ERpcBufferMessage(NetworkReader reader)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		return new RpcBufferMessage
		{
			payload = NetworkReaderExtensions.ReadBytesAndSizeSegment(reader)
		};
	}

	public static void _Write_Mirror_002ERpcBufferMessage(NetworkWriter writer, RpcBufferMessage value)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		NetworkWriterExtensions.WriteBytesAndSizeSegment(writer, value.payload);
	}

	public static SpawnMessage _Read_Mirror_002ESpawnMessage(NetworkReader reader)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		return new SpawnMessage
		{
			netId = NetworkReaderExtensions.ReadUInt(reader),
			isLocalPlayer = NetworkReaderExtensions.ReadBool(reader),
			isOwner = NetworkReaderExtensions.ReadBool(reader),
			sceneId = NetworkReaderExtensions.ReadULong(reader),
			assetId = NetworkReaderExtensions.ReadUInt(reader),
			position = NetworkReaderExtensions.ReadVector3(reader),
			rotation = NetworkReaderExtensions.ReadQuaternion(reader),
			scale = NetworkReaderExtensions.ReadVector3(reader),
			payload = NetworkReaderExtensions.ReadBytesAndSizeSegment(reader),
			parentActorNetId = NetworkReaderExtensions.ReadUInt(reader)
		};
	}

	public static void _Write_Mirror_002ESpawnMessage(NetworkWriter writer, SpawnMessage value)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		NetworkWriterExtensions.WriteUInt(writer, value.netId);
		NetworkWriterExtensions.WriteBool(writer, value.isLocalPlayer);
		NetworkWriterExtensions.WriteBool(writer, value.isOwner);
		NetworkWriterExtensions.WriteULong(writer, value.sceneId);
		NetworkWriterExtensions.WriteUInt(writer, value.assetId);
		NetworkWriterExtensions.WriteVector3(writer, value.position);
		NetworkWriterExtensions.WriteQuaternion(writer, value.rotation);
		NetworkWriterExtensions.WriteVector3(writer, value.scale);
		NetworkWriterExtensions.WriteBytesAndSizeSegment(writer, value.payload);
		NetworkWriterExtensions.WriteUInt(writer, value.parentActorNetId);
	}

	public static ChangeOwnerMessage _Read_Mirror_002EChangeOwnerMessage(NetworkReader reader)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		return new ChangeOwnerMessage
		{
			netId = NetworkReaderExtensions.ReadUInt(reader),
			isOwner = NetworkReaderExtensions.ReadBool(reader),
			isLocalPlayer = NetworkReaderExtensions.ReadBool(reader)
		};
	}

	public static void _Write_Mirror_002EChangeOwnerMessage(NetworkWriter writer, ChangeOwnerMessage value)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		NetworkWriterExtensions.WriteUInt(writer, value.netId);
		NetworkWriterExtensions.WriteBool(writer, value.isOwner);
		NetworkWriterExtensions.WriteBool(writer, value.isLocalPlayer);
	}

	public static ObjectSpawnStartedMessage _Read_Mirror_002EObjectSpawnStartedMessage(NetworkReader reader)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		return default;
	}

	public static void _Write_Mirror_002EObjectSpawnStartedMessage(NetworkWriter writer, ObjectSpawnStartedMessage value)
	{
	}

	public static ObjectSpawnFinishedMessage _Read_Mirror_002EObjectSpawnFinishedMessage(NetworkReader reader)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		return default;
	}

	public static void _Write_Mirror_002EObjectSpawnFinishedMessage(NetworkWriter writer, ObjectSpawnFinishedMessage value)
	{
	}

	public static ObjectDestroyMessage _Read_Mirror_002EObjectDestroyMessage(NetworkReader reader)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		return new ObjectDestroyMessage
		{
			netId = NetworkReaderExtensions.ReadUInt(reader)
		};
	}

	public static void _Write_Mirror_002EObjectDestroyMessage(NetworkWriter writer, ObjectDestroyMessage value)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		NetworkWriterExtensions.WriteUInt(writer, value.netId);
	}

	public static ObjectHideMessage _Read_Mirror_002EObjectHideMessage(NetworkReader reader)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		return new ObjectHideMessage
		{
			netId = NetworkReaderExtensions.ReadUInt(reader)
		};
	}

	public static void _Write_Mirror_002EObjectHideMessage(NetworkWriter writer, ObjectHideMessage value)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		NetworkWriterExtensions.WriteUInt(writer, value.netId);
	}

	public static EntityStateMessage _Read_Mirror_002EEntityStateMessage(NetworkReader reader)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		return new EntityStateMessage
		{
			netId = NetworkReaderExtensions.ReadUInt(reader),
			payload = NetworkReaderExtensions.ReadBytesAndSizeSegment(reader)
		};
	}

	public static void _Write_Mirror_002EEntityStateMessage(NetworkWriter writer, EntityStateMessage value)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		NetworkWriterExtensions.WriteUInt(writer, value.netId);
		NetworkWriterExtensions.WriteBytesAndSizeSegment(writer, value.payload);
	}

	public static NetworkPingMessage _Read_Mirror_002ENetworkPingMessage(NetworkReader reader)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		return new NetworkPingMessage
		{
			clientTime = NetworkReaderExtensions.ReadDouble(reader)
		};
	}

	public static void _Write_Mirror_002ENetworkPingMessage(NetworkWriter writer, NetworkPingMessage value)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		NetworkWriterExtensions.WriteDouble(writer, value.clientTime);
	}

	public static NetworkPongMessage _Read_Mirror_002ENetworkPongMessage(NetworkReader reader)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		return new NetworkPongMessage
		{
			clientTime = NetworkReaderExtensions.ReadDouble(reader)
		};
	}

	public static void _Write_Mirror_002ENetworkPongMessage(NetworkWriter writer, NetworkPongMessage value)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		NetworkWriterExtensions.WriteDouble(writer, value.clientTime);
	}

	public static DewEffect.PlayEffectMessage _Read_DewEffect_002FPlayEffectMessage(NetworkReader reader)
	{
		return new DewEffect.PlayEffectMessage
		{
			isNew = NetworkReaderExtensions.ReadBool(reader),
			parent = NetworkReaderExtensions.ReadNetworkIdentity(reader),
			pathId = NetworkReaderExtensions.ReadInt(reader)
		};
	}

	public static void _Write_DewEffect_002FPlayEffectMessage(NetworkWriter writer, DewEffect.PlayEffectMessage value)
	{
		NetworkWriterExtensions.WriteBool(writer, value.isNew);
		NetworkWriterExtensions.WriteNetworkIdentity(writer, value.parent);
		NetworkWriterExtensions.WriteInt(writer, value.pathId);
	}

	public static DewEffect.PlayPositionedEffectMessage _Read_DewEffect_002FPlayPositionedEffectMessage(NetworkReader reader)
	{
		return new DewEffect.PlayPositionedEffectMessage
		{
			isNew = NetworkReaderExtensions.ReadBool(reader),
			isPooled = NetworkReaderExtensions.ReadBool(reader),
			parent = NetworkReaderExtensions.ReadNetworkIdentity(reader),
			pathId = NetworkReaderExtensions.ReadInt(reader),
			position = NetworkReaderExtensions.ReadVector3(reader),
			rotation = NetworkReaderExtensions.ReadQuaternion(reader)
		};
	}

	public static void _Write_DewEffect_002FPlayPositionedEffectMessage(NetworkWriter writer, DewEffect.PlayPositionedEffectMessage value)
	{
		NetworkWriterExtensions.WriteBool(writer, value.isNew);
		NetworkWriterExtensions.WriteBool(writer, value.isPooled);
		NetworkWriterExtensions.WriteNetworkIdentity(writer, value.parent);
		NetworkWriterExtensions.WriteInt(writer, value.pathId);
		NetworkWriterExtensions.WriteVector3(writer, value.position);
		NetworkWriterExtensions.WriteQuaternion(writer, value.rotation);
	}

	public static DewEffect.PlayAttachedEffectMessage _Read_DewEffect_002FPlayAttachedEffectMessage(NetworkReader reader)
	{
		return new DewEffect.PlayAttachedEffectMessage
		{
			isNew = NetworkReaderExtensions.ReadBool(reader),
			isPooled = NetworkReaderExtensions.ReadBool(reader),
			parent = NetworkReaderExtensions.ReadNetworkIdentity(reader),
			pathId = NetworkReaderExtensions.ReadInt(reader),
			isPositioned = NetworkReaderExtensions.ReadBool(reader),
			position = NetworkReaderExtensions.ReadVector3(reader),
			rotation = NetworkReaderExtensions.ReadQuaternion(reader),
			entity = NetworkReaderExtensions.ReadNetworkBehaviour<Entity>(reader)
		};
	}

	public static void _Write_DewEffect_002FPlayAttachedEffectMessage(NetworkWriter writer, DewEffect.PlayAttachedEffectMessage value)
	{
		NetworkWriterExtensions.WriteBool(writer, value.isNew);
		NetworkWriterExtensions.WriteBool(writer, value.isPooled);
		NetworkWriterExtensions.WriteNetworkIdentity(writer, value.parent);
		NetworkWriterExtensions.WriteInt(writer, value.pathId);
		NetworkWriterExtensions.WriteBool(writer, value.isPositioned);
		NetworkWriterExtensions.WriteVector3(writer, value.position);
		NetworkWriterExtensions.WriteQuaternion(writer, value.rotation);
		NetworkWriterExtensions.WriteNetworkBehaviour(writer, (NetworkBehaviour)(object)value.entity);
	}

	public static DewEffect.PlayCastEffectMessage _Read_DewEffect_002FPlayCastEffectMessage(NetworkReader reader)
	{
		return new DewEffect.PlayCastEffectMessage
		{
			parent = NetworkReaderExtensions.ReadNetworkIdentity(reader),
			pathId = NetworkReaderExtensions.ReadInt(reader),
			info = _Read_CastInfo(reader),
			method = _Read_CastMethodType(reader),
			duration = NetworkReaderExtensions.ReadFloat(reader)
		};
	}

	public static CastInfo _Read_CastInfo(NetworkReader reader)
	{
		return new CastInfo
		{
			caster = NetworkReaderExtensions.ReadNetworkBehaviour<Entity>(reader),
			target = NetworkReaderExtensions.ReadNetworkBehaviour<Entity>(reader),
			point = NetworkReaderExtensions.ReadVector3(reader),
			angle = NetworkReaderExtensions.ReadFloat(reader),
			animSelectValue = NetworkReaderExtensions.ReadFloat(reader)
		};
	}

	public static CastMethodType _Read_CastMethodType(NetworkReader reader)
	{
		return (CastMethodType)NetworkReaderExtensions.ReadInt(reader);
	}

	public static void _Write_DewEffect_002FPlayCastEffectMessage(NetworkWriter writer, DewEffect.PlayCastEffectMessage value)
	{
		NetworkWriterExtensions.WriteNetworkIdentity(writer, value.parent);
		NetworkWriterExtensions.WriteInt(writer, value.pathId);
		_Write_CastInfo(writer, value.info);
		_Write_CastMethodType(writer, value.method);
		NetworkWriterExtensions.WriteFloat(writer, value.duration);
	}

	public static void _Write_CastInfo(NetworkWriter writer, CastInfo value)
	{
		NetworkWriterExtensions.WriteNetworkBehaviour(writer, (NetworkBehaviour)(object)value.caster);
		NetworkWriterExtensions.WriteNetworkBehaviour(writer, (NetworkBehaviour)(object)value.target);
		NetworkWriterExtensions.WriteVector3(writer, value.point);
		NetworkWriterExtensions.WriteFloat(writer, value.angle);
		NetworkWriterExtensions.WriteFloat(writer, value.animSelectValue);
	}

	public static void _Write_CastMethodType(NetworkWriter writer, CastMethodType value)
	{
		NetworkWriterExtensions.WriteInt(writer, (int)value);
	}

	public static DewEffect.StopEffectMessage _Read_DewEffect_002FStopEffectMessage(NetworkReader reader)
	{
		return new DewEffect.StopEffectMessage
		{
			parent = NetworkReaderExtensions.ReadNetworkIdentity(reader),
			pathId = NetworkReaderExtensions.ReadInt(reader)
		};
	}

	public static void _Write_DewEffect_002FStopEffectMessage(NetworkWriter writer, DewEffect.StopEffectMessage value)
	{
		NetworkWriterExtensions.WriteNetworkIdentity(writer, value.parent);
		NetworkWriterExtensions.WriteInt(writer, value.pathId);
	}

	public static DewEffect.ApplySpeedMultiplierToEffectMessage _Read_DewEffect_002FApplySpeedMultiplierToEffectMessage(NetworkReader reader)
	{
		return new DewEffect.ApplySpeedMultiplierToEffectMessage
		{
			multiplier = NetworkReaderExtensions.ReadFloat(reader),
			parent = NetworkReaderExtensions.ReadNetworkIdentity(reader),
			pathId = NetworkReaderExtensions.ReadInt(reader)
		};
	}

	public static void _Write_DewEffect_002FApplySpeedMultiplierToEffectMessage(NetworkWriter writer, DewEffect.ApplySpeedMultiplierToEffectMessage value)
	{
		NetworkWriterExtensions.WriteFloat(writer, value.multiplier);
		NetworkWriterExtensions.WriteNetworkIdentity(writer, value.parent);
		NetworkWriterExtensions.WriteInt(writer, value.pathId);
	}

	public static DewAuthRequestMessage _Read_DewAuthRequestMessage(NetworkReader reader)
	{
		return new DewAuthRequestMessage
		{
			userId = NetworkReaderExtensions.ReadString(reader),
			profileGuid = NetworkReaderExtensions.ReadString(reader),
			profileName = NetworkReaderExtensions.ReadString(reader),
			inviteCode = NetworkReaderExtensions.ReadString(reader)
		};
	}

	public static void _Write_DewAuthRequestMessage(NetworkWriter writer, DewAuthRequestMessage value)
	{
		NetworkWriterExtensions.WriteString(writer, value.userId);
		NetworkWriterExtensions.WriteString(writer, value.profileGuid);
		NetworkWriterExtensions.WriteString(writer, value.profileName);
		NetworkWriterExtensions.WriteString(writer, value.inviteCode);
	}

	public static DewAuthResponseMessage _Read_DewAuthResponseMessage(NetworkReader reader)
	{
		return new DewAuthResponseMessage
		{
			isError = NetworkReaderExtensions.ReadBool(reader),
			errorType = _Read_DewExceptionType(reader)
		};
	}

	public static DewExceptionType _Read_DewExceptionType(NetworkReader reader)
	{
		return (DewExceptionType)NetworkReaderExtensions.ReadInt(reader);
	}

	public static void _Write_DewAuthResponseMessage(NetworkWriter writer, DewAuthResponseMessage value)
	{
		NetworkWriterExtensions.WriteBool(writer, value.isError);
		_Write_DewExceptionType(writer, value.errorType);
	}

	public static void _Write_DewExceptionType(NetworkWriter writer, DewExceptionType value)
	{
		NetworkWriterExtensions.WriteInt(writer, (int)value);
	}

	public static DewNetworkManager.ChangeSceneMessage _Read_DewNetworkManager_002FChangeSceneMessage(NetworkReader reader)
	{
		return new DewNetworkManager.ChangeSceneMessage
		{
			name = NetworkReaderExtensions.ReadString(reader)
		};
	}

	public static void _Write_DewNetworkManager_002FChangeSceneMessage(NetworkWriter writer, DewNetworkManager.ChangeSceneMessage value)
	{
		NetworkWriterExtensions.WriteString(writer, value.name);
	}

	public static DewNetworkManager.SetLoadingStatusMessage _Read_DewNetworkManager_002FSetLoadingStatusMessage(NetworkReader reader)
	{
		return new DewNetworkManager.SetLoadingStatusMessage
		{
			isLoading = NetworkReaderExtensions.ReadBool(reader),
			isWhite = NetworkReaderExtensions.ReadBool(reader)
		};
	}

	public static void _Write_DewNetworkManager_002FSetLoadingStatusMessage(NetworkWriter writer, DewNetworkManager.SetLoadingStatusMessage value)
	{
		NetworkWriterExtensions.WriteBool(writer, value.isLoading);
		NetworkWriterExtensions.WriteBool(writer, value.isWhite);
	}

	public static DewNetworkManager.SessionEndedMessage _Read_DewNetworkManager_002FSessionEndedMessage(NetworkReader reader)
	{
		return default;
	}

	public static void _Write_DewNetworkManager_002FSessionEndedMessage(NetworkWriter writer, DewNetworkManager.SessionEndedMessage value)
	{
	}

	public static DewNetworkManager.SessionRestartingMessage _Read_DewNetworkManager_002FSessionRestartingMessage(NetworkReader reader)
	{
		return default;
	}

	public static void _Write_DewNetworkManager_002FSessionRestartingMessage(NetworkWriter writer, DewNetworkManager.SessionRestartingMessage value)
	{
	}

	public static InGameAnalyticsManager.DisableAnalyticsMessage _Read_InGameAnalyticsManager_002FDisableAnalyticsMessage(NetworkReader reader)
	{
		return default;
	}

	public static void _Write_InGameAnalyticsManager_002FDisableAnalyticsMessage(NetworkWriter writer, InGameAnalyticsManager.DisableAnalyticsMessage value)
	{
	}

	public static void _Write_HatredStrengthType(NetworkWriter writer, HatredStrengthType value)
	{
		NetworkWriterExtensions.WriteInt(writer, (int)value);
	}

	public static void _Write_QuestProgressType(NetworkWriter writer, QuestProgressType value)
	{
		NetworkWriterExtensions.WriteInt(writer, (int)value);
	}

	public static HatredStrengthType _Read_HatredStrengthType(NetworkReader reader)
	{
		return (HatredStrengthType)NetworkReaderExtensions.ReadInt(reader);
	}

	public static QuestProgressType _Read_QuestProgressType(NetworkReader reader)
	{
		return (QuestProgressType)NetworkReaderExtensions.ReadInt(reader);
	}

	public static CorruptedChaosRewardType[] _Read_CorruptedChaosRewardType_005B_005D(NetworkReader reader)
	{
		return NetworkReaderExtensions.ReadArray<CorruptedChaosRewardType>(reader);
	}

	public static CorruptedChaosRewardType _Read_CorruptedChaosRewardType(NetworkReader reader)
	{
		return (CorruptedChaosRewardType)NetworkReaderExtensions.ReadInt(reader);
	}

	public static void _Write_CorruptedChaosRewardType_005B_005D(NetworkWriter writer, CorruptedChaosRewardType[] value)
	{
		NetworkWriterExtensions.WriteArray<CorruptedChaosRewardType>(writer, value);
	}

	public static void _Write_CorruptedChaosRewardType(NetworkWriter writer, CorruptedChaosRewardType value)
	{
		NetworkWriterExtensions.WriteInt(writer, (int)value);
	}

	public static void _Write_System_002ESingle_005B_005D(NetworkWriter writer, float[] value)
	{
		NetworkWriterExtensions.WriteArray<float>(writer, value);
	}

	public static void _Write_System_002EInt32_005B_005D(NetworkWriter writer, int[] value)
	{
		NetworkWriterExtensions.WriteArray<int>(writer, value);
	}

	public static float[] _Read_System_002ESingle_005B_005D(NetworkReader reader)
	{
		return NetworkReaderExtensions.ReadArray<float>(reader);
	}

	public static int[] _Read_System_002EInt32_005B_005D(NetworkReader reader)
	{
		return NetworkReaderExtensions.ReadArray<int>(reader);
	}

	public static void _Write_AbilityTrigger_002FConfigSyncData_005B_005D(NetworkWriter writer, AbilityTrigger.ConfigSyncData[] value)
	{
		NetworkWriterExtensions.WriteArray<AbilityTrigger.ConfigSyncData>(writer, value);
	}

	public static void _Write_AbilityTrigger_002FConfigSyncData(NetworkWriter writer, AbilityTrigger.ConfigSyncData value)
	{
		NetworkWriterExtensions.WriteFloat(writer, value.manaCost);
		NetworkWriterExtensions.WriteInt(writer, value.maxCharges);
		NetworkWriterExtensions.WriteInt(writer, value.addedCharges);
		NetworkWriterExtensions.WriteFloat(writer, value.cooldownTime);
		NetworkWriterExtensions.WriteFloat(writer, value.minimumDelay);
	}

	public static AbilityTrigger.ConfigSyncData[] _Read_AbilityTrigger_002FConfigSyncData_005B_005D(NetworkReader reader)
	{
		return NetworkReaderExtensions.ReadArray<AbilityTrigger.ConfigSyncData>(reader);
	}

	public static AbilityTrigger.ConfigSyncData _Read_AbilityTrigger_002FConfigSyncData(NetworkReader reader)
	{
		return new AbilityTrigger.ConfigSyncData
		{
			manaCost = NetworkReaderExtensions.ReadFloat(reader),
			maxCharges = NetworkReaderExtensions.ReadInt(reader),
			addedCharges = NetworkReaderExtensions.ReadInt(reader),
			cooldownTime = NetworkReaderExtensions.ReadFloat(reader),
			minimumDelay = NetworkReaderExtensions.ReadFloat(reader)
		};
	}

	public static void _Write_EventInfoSkillUse(NetworkWriter writer, EventInfoSkillUse value)
	{
		_Write_HeroSkillLocation(writer, value.type);
		NetworkWriterExtensions.WriteNetworkBehaviour(writer, (NetworkBehaviour)(object)value.skill);
	}

	public static void _Write_HeroSkillLocation(NetworkWriter writer, HeroSkillLocation value)
	{
		NetworkWriterExtensions.WriteInt(writer, (int)value);
	}

	public static EventInfoSkillUse _Read_EventInfoSkillUse(NetworkReader reader)
	{
		return new EventInfoSkillUse
		{
			type = _Read_HeroSkillLocation(reader),
			skill = NetworkReaderExtensions.ReadNetworkBehaviour<SkillTrigger>(reader)
		};
	}

	public static HeroSkillLocation _Read_HeroSkillLocation(NetworkReader reader)
	{
		return (HeroSkillLocation)NetworkReaderExtensions.ReadInt(reader);
	}

	public static void _Write_SkillType(NetworkWriter writer, SkillType value)
	{
		NetworkWriterExtensions.WriteInt(writer, (int)value);
	}

	public static void _Write_DescriptionTags(NetworkWriter writer, DescriptionTags value)
	{
		NetworkWriterExtensions.WriteInt(writer, (int)value);
	}

	public static SkillType _Read_SkillType(NetworkReader reader)
	{
		return (SkillType)NetworkReaderExtensions.ReadInt(reader);
	}

	public static DescriptionTags _Read_DescriptionTags(NetworkReader reader)
	{
		return (DescriptionTags)NetworkReaderExtensions.ReadInt(reader);
	}

	public static void _Write_EventInfoKill(NetworkWriter writer, EventInfoKill value)
	{
		NetworkWriterExtensions.WriteNetworkBehaviour(writer, (NetworkBehaviour)(object)value.actor);
		NetworkWriterExtensions.WriteNetworkBehaviour(writer, (NetworkBehaviour)(object)value.victim);
	}

	public static EventInfoKill _Read_EventInfoKill(NetworkReader reader)
	{
		return new EventInfoKill
		{
			actor = NetworkReaderExtensions.ReadNetworkBehaviour<Actor>(reader),
			victim = NetworkReaderExtensions.ReadNetworkBehaviour<Entity>(reader)
		};
	}

	public static void _Write_Monster_002FMonsterType(NetworkWriter writer, Monster.MonsterType value)
	{
		NetworkWriterExtensions.WriteByte(writer, (byte)value);
	}

	public static Monster.MonsterType _Read_Monster_002FMonsterType(NetworkReader reader)
	{
		return (Monster.MonsterType)NetworkReaderExtensions.ReadByte(reader);
	}

	public static void _Write_Projectile_002FEntityHit(NetworkWriter writer, Projectile.EntityHit value)
	{
		NetworkWriterExtensions.WriteNetworkBehaviour(writer, (NetworkBehaviour)(object)value.entity);
		NetworkWriterExtensions.WriteVector3(writer, value.point);
	}

	public static Projectile.EntityHit _Read_Projectile_002FEntityHit(NetworkReader reader)
	{
		return new Projectile.EntityHit
		{
			entity = NetworkReaderExtensions.ReadNetworkBehaviour<Entity>(reader),
			point = NetworkReaderExtensions.ReadVector3(reader)
		};
	}

	public static void _Write_Projectile_002FProjectileMode(NetworkWriter writer, Projectile.ProjectileMode value)
	{
		NetworkWriterExtensions.WriteInt(writer, (int)value);
	}

	public static void _Write_Projectile_002FStartPositionType(NetworkWriter writer, Projectile.StartPositionType value)
	{
		NetworkWriterExtensions.WriteInt(writer, (int)value);
	}

	public static Projectile.ProjectileMode _Read_Projectile_002FProjectileMode(NetworkReader reader)
	{
		return (Projectile.ProjectileMode)NetworkReaderExtensions.ReadInt(reader);
	}

	public static Projectile.StartPositionType _Read_Projectile_002FStartPositionType(NetworkReader reader)
	{
		return (Projectile.StartPositionType)NetworkReaderExtensions.ReadInt(reader);
	}

	public static void _Write_FailReason(NetworkWriter writer, FailReason value)
	{
		NetworkWriterExtensions.WriteInt(writer, (int)value);
	}

	public static void _Write_QuestState(NetworkWriter writer, QuestState value)
	{
		NetworkWriterExtensions.WriteInt(writer, (int)value);
	}

	public static FailReason _Read_FailReason(NetworkReader reader)
	{
		return (FailReason)NetworkReaderExtensions.ReadInt(reader);
	}

	public static QuestState _Read_QuestState(NetworkReader reader)
	{
		return (QuestState)NetworkReaderExtensions.ReadInt(reader);
	}

	public static void _Write_GemLocation(NetworkWriter writer, GemLocation value)
	{
		_Write_HeroSkillLocation(writer, value.skill);
		NetworkWriterExtensions.WriteInt(writer, value.index);
	}

	public static GemLocation _Read_GemLocation(NetworkReader reader)
	{
		return new GemLocation
		{
			skill = _Read_HeroSkillLocation(reader),
			index = NetworkReaderExtensions.ReadInt(reader)
		};
	}

	public static void _Write_StarlessPath_BossPolarisManager_002FState(NetworkWriter writer, StarlessPath_BossPolarisManager.State value)
	{
		NetworkWriterExtensions.WriteInt(writer, (int)value);
	}

	public static StarlessPath_BossPolarisManager.State _Read_StarlessPath_BossPolarisManager_002FState(NetworkReader reader)
	{
		return (StarlessPath_BossPolarisManager.State)NetworkReaderExtensions.ReadInt(reader);
	}

	public static void _Write_HeroLoadoutData(NetworkWriter writer, HeroLoadoutData value)
	{
		if (value == null)
		{
			NetworkWriterExtensions.WriteBool(writer, false);
			return;
		}
		NetworkWriterExtensions.WriteBool(writer, true);
		NetworkWriterExtensions.WriteInt(writer, value.skillQ);
		NetworkWriterExtensions.WriteInt(writer, value.skillR);
		NetworkWriterExtensions.WriteInt(writer, value.skillTrait);
		NetworkWriterExtensions.WriteInt(writer, value.skillMovement);
		_Write_System_002ECollections_002EGeneric_002EList_00601_003CLoadoutStarItem_003E(writer, value.cDestruction);
		_Write_System_002ECollections_002EGeneric_002EList_00601_003CLoadoutStarItem_003E(writer, value.cLife);
		_Write_System_002ECollections_002EGeneric_002EList_00601_003CLoadoutStarItem_003E(writer, value.cImagination);
		_Write_System_002ECollections_002EGeneric_002EList_00601_003CLoadoutStarItem_003E(writer, value.cFlexible);
	}

	public static void _Write_System_002ECollections_002EGeneric_002EList_00601_003CLoadoutStarItem_003E(NetworkWriter writer, List<LoadoutStarItem> value)
	{
		NetworkWriterExtensions.WriteList<LoadoutStarItem>(writer, value);
	}

	public static void _Write_LoadoutStarItem(NetworkWriter writer, LoadoutStarItem value)
	{
		NetworkWriterExtensions.WriteString(writer, value.name);
		NetworkWriterExtensions.WriteInt(writer, value.level);
	}

	public static HeroLoadoutData _Read_HeroLoadoutData(NetworkReader reader)
	{
		if (!NetworkReaderExtensions.ReadBool(reader))
		{
			return null;
		}
		HeroLoadoutData heroLoadoutData = new HeroLoadoutData();
		heroLoadoutData.skillQ = NetworkReaderExtensions.ReadInt(reader);
		heroLoadoutData.skillR = NetworkReaderExtensions.ReadInt(reader);
		heroLoadoutData.skillTrait = NetworkReaderExtensions.ReadInt(reader);
		heroLoadoutData.skillMovement = NetworkReaderExtensions.ReadInt(reader);
		heroLoadoutData.cDestruction = _Read_System_002ECollections_002EGeneric_002EList_00601_003CLoadoutStarItem_003E(reader);
		heroLoadoutData.cLife = _Read_System_002ECollections_002EGeneric_002EList_00601_003CLoadoutStarItem_003E(reader);
		heroLoadoutData.cImagination = _Read_System_002ECollections_002EGeneric_002EList_00601_003CLoadoutStarItem_003E(reader);
		heroLoadoutData.cFlexible = _Read_System_002ECollections_002EGeneric_002EList_00601_003CLoadoutStarItem_003E(reader);
		return heroLoadoutData;
	}

	public static List<LoadoutStarItem> _Read_System_002ECollections_002EGeneric_002EList_00601_003CLoadoutStarItem_003E(NetworkReader reader)
	{
		return NetworkReaderExtensions.ReadList<LoadoutStarItem>(reader);
	}

	public static LoadoutStarItem _Read_LoadoutStarItem(NetworkReader reader)
	{
		return new LoadoutStarItem
		{
			name = NetworkReaderExtensions.ReadString(reader),
			level = NetworkReaderExtensions.ReadInt(reader)
		};
	}

	public static void _Write_Hero_Aurena_002FClawState(NetworkWriter writer, Hero_Aurena.ClawState value)
	{
		NetworkWriterExtensions.WriteInt(writer, (int)value);
	}

	public static Hero_Aurena.ClawState _Read_Hero_Aurena_002FClawState(NetworkReader reader)
	{
		return (Hero_Aurena.ClawState)NetworkReaderExtensions.ReadInt(reader);
	}

	public static void _Write_ElementalType(NetworkWriter writer, ElementalType value)
	{
		NetworkWriterExtensions.WriteInt(writer, (int)value);
	}

	public static ElementalType _Read_ElementalType(NetworkReader reader)
	{
		return (ElementalType)NetworkReaderExtensions.ReadInt(reader);
	}

	public static SyncableAssetRef _Read_SyncableAssetRef(NetworkReader reader)
	{
		return new SyncableAssetRef
		{
			guid = NetworkReaderExtensions.ReadString(reader),
			typeName = NetworkReaderExtensions.ReadString(reader),
			typeAssemblyQualifiedName = NetworkReaderExtensions.ReadString(reader),
			isMonoBehaviour = NetworkReaderExtensions.ReadBool(reader),
			isActor = NetworkReaderExtensions.ReadBool(reader)
		};
	}

	public static void _Write_SyncableAssetRef(NetworkWriter writer, SyncableAssetRef value)
	{
		NetworkWriterExtensions.WriteString(writer, value.guid);
		NetworkWriterExtensions.WriteString(writer, value.typeName);
		NetworkWriterExtensions.WriteString(writer, value.typeAssemblyQualifiedName);
		NetworkWriterExtensions.WriteBool(writer, value.isMonoBehaviour);
		NetworkWriterExtensions.WriteBool(writer, value.isActor);
	}

	public static void _Write_BonusStats(NetworkWriter writer, BonusStats value)
	{
		NetworkWriterExtensions.WriteFloat(writer, value.attackDamageFlat);
		NetworkWriterExtensions.WriteFloat(writer, value.attackDamagePercentage);
		NetworkWriterExtensions.WriteFloat(writer, value.abilityPowerFlat);
		NetworkWriterExtensions.WriteFloat(writer, value.abilityPowerPercentage);
		NetworkWriterExtensions.WriteFloat(writer, value.maxHealthFlat);
		NetworkWriterExtensions.WriteFloat(writer, value.maxHealthPercentage);
		NetworkWriterExtensions.WriteFloat(writer, value.maxManaFlat);
		NetworkWriterExtensions.WriteFloat(writer, value.maxManaPercentage);
		NetworkWriterExtensions.WriteFloat(writer, value.healthRegenFlat);
		NetworkWriterExtensions.WriteFloat(writer, value.healthRegenPercentage);
		NetworkWriterExtensions.WriteFloat(writer, value.manaRegenFlat);
		NetworkWriterExtensions.WriteFloat(writer, value.manaRegenPercentage);
		NetworkWriterExtensions.WriteFloat(writer, value.attackSpeedPercentage);
		NetworkWriterExtensions.WriteFloat(writer, value.critAmpFlat);
		NetworkWriterExtensions.WriteFloat(writer, value.critAmpPercentage);
		NetworkWriterExtensions.WriteFloat(writer, value.critChanceFlat);
		NetworkWriterExtensions.WriteFloat(writer, value.critChancePercentage);
		NetworkWriterExtensions.WriteFloat(writer, value.abilityHasteFlat);
		NetworkWriterExtensions.WriteFloat(writer, value.abilityHastePercentage);
		NetworkWriterExtensions.WriteFloat(writer, value.tenacityFlat);
		NetworkWriterExtensions.WriteFloat(writer, value.tenacityPercentage);
		NetworkWriterExtensions.WriteFloat(writer, value.attackRangeFlat);
		NetworkWriterExtensions.WriteFloat(writer, value.attackRangePercentage);
		NetworkWriterExtensions.WriteFloat(writer, value.movementSpeedPercentage);
		NetworkWriterExtensions.WriteFloat(writer, value.fireEffectAmpFlat);
		NetworkWriterExtensions.WriteFloat(writer, value.coldEffectAmpFlat);
		NetworkWriterExtensions.WriteFloat(writer, value.lightEffectAmpFlat);
		NetworkWriterExtensions.WriteFloat(writer, value.darkEffectAmpFlat);
		NetworkWriterExtensions.WriteFloat(writer, value.armorFlat);
		NetworkWriterExtensions.WriteFloat(writer, value.armorPercentage);
		NetworkWriterExtensions.WriteInt(writer, value.everyFourAttackStartIndexFlat);
	}

	public static BonusStats _Read_BonusStats(NetworkReader reader)
	{
		return new BonusStats
		{
			attackDamageFlat = NetworkReaderExtensions.ReadFloat(reader),
			attackDamagePercentage = NetworkReaderExtensions.ReadFloat(reader),
			abilityPowerFlat = NetworkReaderExtensions.ReadFloat(reader),
			abilityPowerPercentage = NetworkReaderExtensions.ReadFloat(reader),
			maxHealthFlat = NetworkReaderExtensions.ReadFloat(reader),
			maxHealthPercentage = NetworkReaderExtensions.ReadFloat(reader),
			maxManaFlat = NetworkReaderExtensions.ReadFloat(reader),
			maxManaPercentage = NetworkReaderExtensions.ReadFloat(reader),
			healthRegenFlat = NetworkReaderExtensions.ReadFloat(reader),
			healthRegenPercentage = NetworkReaderExtensions.ReadFloat(reader),
			manaRegenFlat = NetworkReaderExtensions.ReadFloat(reader),
			manaRegenPercentage = NetworkReaderExtensions.ReadFloat(reader),
			attackSpeedPercentage = NetworkReaderExtensions.ReadFloat(reader),
			critAmpFlat = NetworkReaderExtensions.ReadFloat(reader),
			critAmpPercentage = NetworkReaderExtensions.ReadFloat(reader),
			critChanceFlat = NetworkReaderExtensions.ReadFloat(reader),
			critChancePercentage = NetworkReaderExtensions.ReadFloat(reader),
			abilityHasteFlat = NetworkReaderExtensions.ReadFloat(reader),
			abilityHastePercentage = NetworkReaderExtensions.ReadFloat(reader),
			tenacityFlat = NetworkReaderExtensions.ReadFloat(reader),
			tenacityPercentage = NetworkReaderExtensions.ReadFloat(reader),
			attackRangeFlat = NetworkReaderExtensions.ReadFloat(reader),
			attackRangePercentage = NetworkReaderExtensions.ReadFloat(reader),
			movementSpeedPercentage = NetworkReaderExtensions.ReadFloat(reader),
			fireEffectAmpFlat = NetworkReaderExtensions.ReadFloat(reader),
			coldEffectAmpFlat = NetworkReaderExtensions.ReadFloat(reader),
			lightEffectAmpFlat = NetworkReaderExtensions.ReadFloat(reader),
			darkEffectAmpFlat = NetworkReaderExtensions.ReadFloat(reader),
			armorFlat = NetworkReaderExtensions.ReadFloat(reader),
			armorPercentage = NetworkReaderExtensions.ReadFloat(reader),
			everyFourAttackStartIndexFlat = NetworkReaderExtensions.ReadInt(reader)
		};
	}

	public static Shrine_MarshOfDestiny_SeedOfTorment.ChoiceItem _Read_Shrine_MarshOfDestiny_SeedOfTorment_002FChoiceItem(NetworkReader reader)
	{
		return new Shrine_MarshOfDestiny_SeedOfTorment.ChoiceItem
		{
			strength = _Read_Shrine_MarshOfDestiny_SeedOfTorment_002FStrengthType(reader),
			penalty = _Read_Shrine_MarshOfDestiny_SeedOfTorment_002FPenaltyType(reader),
			penaltyParameter = NetworkReaderExtensions.ReadString(reader),
			isTempPenalty = NetworkReaderExtensions.ReadBool(reader),
			reward = _Read_Shrine_MarshOfDestiny_SeedOfTorment_002FRewardType(reader),
			rewardParameter = NetworkReaderExtensions.ReadString(reader)
		};
	}

	public static Shrine_MarshOfDestiny_SeedOfTorment.StrengthType _Read_Shrine_MarshOfDestiny_SeedOfTorment_002FStrengthType(NetworkReader reader)
	{
		return (Shrine_MarshOfDestiny_SeedOfTorment.StrengthType)NetworkReaderExtensions.ReadInt(reader);
	}

	public static Shrine_MarshOfDestiny_SeedOfTorment.PenaltyType _Read_Shrine_MarshOfDestiny_SeedOfTorment_002FPenaltyType(NetworkReader reader)
	{
		return (Shrine_MarshOfDestiny_SeedOfTorment.PenaltyType)NetworkReaderExtensions.ReadInt(reader);
	}

	public static Shrine_MarshOfDestiny_SeedOfTorment.RewardType _Read_Shrine_MarshOfDestiny_SeedOfTorment_002FRewardType(NetworkReader reader)
	{
		return (Shrine_MarshOfDestiny_SeedOfTorment.RewardType)NetworkReaderExtensions.ReadInt(reader);
	}

	public static void _Write_Shrine_MarshOfDestiny_SeedOfTorment_002FChoiceItem(NetworkWriter writer, Shrine_MarshOfDestiny_SeedOfTorment.ChoiceItem value)
	{
		_Write_Shrine_MarshOfDestiny_SeedOfTorment_002FStrengthType(writer, value.strength);
		_Write_Shrine_MarshOfDestiny_SeedOfTorment_002FPenaltyType(writer, value.penalty);
		NetworkWriterExtensions.WriteString(writer, value.penaltyParameter);
		NetworkWriterExtensions.WriteBool(writer, value.isTempPenalty);
		_Write_Shrine_MarshOfDestiny_SeedOfTorment_002FRewardType(writer, value.reward);
		NetworkWriterExtensions.WriteString(writer, value.rewardParameter);
	}

	public static void _Write_Shrine_MarshOfDestiny_SeedOfTorment_002FStrengthType(NetworkWriter writer, Shrine_MarshOfDestiny_SeedOfTorment.StrengthType value)
	{
		NetworkWriterExtensions.WriteInt(writer, (int)value);
	}

	public static void _Write_Shrine_MarshOfDestiny_SeedOfTorment_002FPenaltyType(NetworkWriter writer, Shrine_MarshOfDestiny_SeedOfTorment.PenaltyType value)
	{
		NetworkWriterExtensions.WriteInt(writer, (int)value);
	}

	public static void _Write_Shrine_MarshOfDestiny_SeedOfTorment_002FRewardType(NetworkWriter writer, Shrine_MarshOfDestiny_SeedOfTorment.RewardType value)
	{
		NetworkWriterExtensions.WriteInt(writer, (int)value);
	}

	public static void _Write_Ai_Mon_Despair_BossAzurak_StompGlobal_002FWave(NetworkWriter writer, Ai_Mon_Despair_BossAzurak_StompGlobal.Wave value)
	{
		if (value == null)
		{
			NetworkWriterExtensions.WriteBool(writer, false);
			return;
		}
		NetworkWriterExtensions.WriteBool(writer, true);
		_Write_Ai_Mon_Despair_BossAzurak_StompGlobal_002FPattern_005B_005D(writer, value.patterns);
	}

	public static void _Write_Ai_Mon_Despair_BossAzurak_StompGlobal_002FPattern_005B_005D(NetworkWriter writer, Ai_Mon_Despair_BossAzurak_StompGlobal.Pattern[] value)
	{
		NetworkWriterExtensions.WriteArray<Ai_Mon_Despair_BossAzurak_StompGlobal.Pattern>(writer, value);
	}

	public static void _Write_Ai_Mon_Despair_BossAzurak_StompGlobal_002FPattern(NetworkWriter writer, Ai_Mon_Despair_BossAzurak_StompGlobal.Pattern value)
	{
		NetworkWriterExtensions.WriteFloat(writer, value.baseAngle);
		NetworkWriterExtensions.WriteFloat(writer, value.arc);
		NetworkWriterExtensions.WriteFloat(writer, value.innerRad);
		NetworkWriterExtensions.WriteFloat(writer, value.outerRad);
	}

	public static Ai_Mon_Despair_BossAzurak_StompGlobal.Wave _Read_Ai_Mon_Despair_BossAzurak_StompGlobal_002FWave(NetworkReader reader)
	{
		if (!NetworkReaderExtensions.ReadBool(reader))
		{
			return null;
		}
		Ai_Mon_Despair_BossAzurak_StompGlobal.Wave wave = new Ai_Mon_Despair_BossAzurak_StompGlobal.Wave();
		wave.patterns = _Read_Ai_Mon_Despair_BossAzurak_StompGlobal_002FPattern_005B_005D(reader);
		return wave;
	}

	public static Ai_Mon_Despair_BossAzurak_StompGlobal.Pattern[] _Read_Ai_Mon_Despair_BossAzurak_StompGlobal_002FPattern_005B_005D(NetworkReader reader)
	{
		return NetworkReaderExtensions.ReadArray<Ai_Mon_Despair_BossAzurak_StompGlobal.Pattern>(reader);
	}

	public static Ai_Mon_Despair_BossAzurak_StompGlobal.Pattern _Read_Ai_Mon_Despair_BossAzurak_StompGlobal_002FPattern(NetworkReader reader)
	{
		return new Ai_Mon_Despair_BossAzurak_StompGlobal.Pattern
		{
			baseAngle = NetworkReaderExtensions.ReadFloat(reader),
			arc = NetworkReaderExtensions.ReadFloat(reader),
			innerRad = NetworkReaderExtensions.ReadFloat(reader),
			outerRad = NetworkReaderExtensions.ReadFloat(reader)
		};
	}

	public static void _Write_Se_Mon_Primus_BossPrimusAeron_Adaptation_002FStatType(NetworkWriter writer, Se_Mon_Primus_BossPrimusAeron_Adaptation.StatType value)
	{
		NetworkWriterExtensions.WriteInt(writer, (int)value);
	}

	public static Se_Mon_Primus_BossPrimusAeron_Adaptation.StatType _Read_Se_Mon_Primus_BossPrimusAeron_Adaptation_002FStatType(NetworkReader reader)
	{
		return (Se_Mon_Primus_BossPrimusAeron_Adaptation.StatType)NetworkReaderExtensions.ReadInt(reader);
	}

	public static void _Write_Mon_Primus_BossPrimusAeron_002FWeaponType(NetworkWriter writer, Mon_Primus_BossPrimusAeron.WeaponType value)
	{
		NetworkWriterExtensions.WriteInt(writer, (int)value);
	}

	public static void _Write_Mon_Primus_BossPrimusAeron_002FPhaseType(NetworkWriter writer, Mon_Primus_BossPrimusAeron.PhaseType value)
	{
		NetworkWriterExtensions.WriteInt(writer, (int)value);
	}

	public static Mon_Primus_BossPrimusAeron.WeaponType _Read_Mon_Primus_BossPrimusAeron_002FWeaponType(NetworkReader reader)
	{
		return (Mon_Primus_BossPrimusAeron.WeaponType)NetworkReaderExtensions.ReadInt(reader);
	}

	public static Mon_Primus_BossPrimusAeron.PhaseType _Read_Mon_Primus_BossPrimusAeron_002FPhaseType(NetworkReader reader)
	{
		return (Mon_Primus_BossPrimusAeron.PhaseType)NetworkReaderExtensions.ReadInt(reader);
	}

	public static void _Write_Mon_Special_BossPolaris_002FMainPhase(NetworkWriter writer, Mon_Special_BossPolaris.MainPhase value)
	{
		NetworkWriterExtensions.WriteInt(writer, (int)value);
	}

	public static Mon_Special_BossPolaris.MainPhase _Read_Mon_Special_BossPolaris_002FMainPhase(NetworkReader reader)
	{
		return (Mon_Special_BossPolaris.MainPhase)NetworkReaderExtensions.ReadInt(reader);
	}

	public static ChoiceShrineItem[] _Read_ChoiceShrineItem_005B_005D(NetworkReader reader)
	{
		return NetworkReaderExtensions.ReadArray<ChoiceShrineItem>(reader);
	}

	public static ChoiceShrineItem _Read_ChoiceShrineItem(NetworkReader reader)
	{
		return new ChoiceShrineItem
		{
			typeName = NetworkReaderExtensions.ReadString(reader),
			level = NetworkReaderExtensions.ReadInt(reader)
		};
	}

	public static void _Write_ChoiceShrineItem_005B_005D(NetworkWriter writer, ChoiceShrineItem[] value)
	{
		NetworkWriterExtensions.WriteArray<ChoiceShrineItem>(writer, value);
	}

	public static void _Write_ChoiceShrineItem(NetworkWriter writer, ChoiceShrineItem value)
	{
		NetworkWriterExtensions.WriteString(writer, value.typeName);
		NetworkWriterExtensions.WriteInt(writer, value.level);
	}

	public static void _Write_ChaosRewardType(NetworkWriter writer, ChaosRewardType value)
	{
		NetworkWriterExtensions.WriteInt(writer, (int)value);
	}

	public static ChaosRewardType _Read_ChaosRewardType(NetworkReader reader)
	{
		return (ChaosRewardType)NetworkReaderExtensions.ReadInt(reader);
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	public static void InitReadWriters()
	{
		Writer<byte>.write = NetworkWriterExtensions.WriteByte;
		Writer<byte?>.write = NetworkWriterExtensions.WriteByteNullable;
		Writer<sbyte>.write = NetworkWriterExtensions.WriteSByte;
		Writer<sbyte?>.write = NetworkWriterExtensions.WriteSByteNullable;
		Writer<char>.write = NetworkWriterExtensions.WriteChar;
		Writer<char?>.write = NetworkWriterExtensions.WriteCharNullable;
		Writer<bool>.write = NetworkWriterExtensions.WriteBool;
		Writer<bool?>.write = NetworkWriterExtensions.WriteBoolNullable;
		Writer<short>.write = NetworkWriterExtensions.WriteShort;
		Writer<short?>.write = NetworkWriterExtensions.WriteShortNullable;
		Writer<ushort>.write = NetworkWriterExtensions.WriteUShort;
		Writer<ushort?>.write = NetworkWriterExtensions.WriteUShortNullable;
		Writer<int>.write = NetworkWriterExtensions.WriteInt;
		Writer<int?>.write = NetworkWriterExtensions.WriteIntNullable;
		Writer<uint>.write = NetworkWriterExtensions.WriteUInt;
		Writer<uint?>.write = NetworkWriterExtensions.WriteUIntNullable;
		Writer<long>.write = NetworkWriterExtensions.WriteLong;
		Writer<long?>.write = NetworkWriterExtensions.WriteLongNullable;
		Writer<ulong>.write = NetworkWriterExtensions.WriteULong;
		Writer<ulong?>.write = NetworkWriterExtensions.WriteULongNullable;
		Writer<float>.write = NetworkWriterExtensions.WriteFloat;
		Writer<float?>.write = NetworkWriterExtensions.WriteFloatNullable;
		Writer<double>.write = NetworkWriterExtensions.WriteDouble;
		Writer<double?>.write = NetworkWriterExtensions.WriteDoubleNullable;
		Writer<decimal>.write = NetworkWriterExtensions.WriteDecimal;
		Writer<decimal?>.write = NetworkWriterExtensions.WriteDecimalNullable;
		Writer<string>.write = NetworkWriterExtensions.WriteString;
		Writer<ArraySegment<byte>>.write = NetworkWriterExtensions.WriteBytesAndSizeSegment;
		Writer<byte[]>.write = NetworkWriterExtensions.WriteBytesAndSize;
		Writer<Vector2>.write = NetworkWriterExtensions.WriteVector2;
		Writer<Vector2?>.write = NetworkWriterExtensions.WriteVector2Nullable;
		Writer<Vector3>.write = NetworkWriterExtensions.WriteVector3;
		Writer<Vector3?>.write = NetworkWriterExtensions.WriteVector3Nullable;
		Writer<Vector4>.write = NetworkWriterExtensions.WriteVector4;
		Writer<Vector4?>.write = NetworkWriterExtensions.WriteVector4Nullable;
		Writer<Vector2Int>.write = NetworkWriterExtensions.WriteVector2Int;
		Writer<Vector2Int?>.write = NetworkWriterExtensions.WriteVector2IntNullable;
		Writer<Vector3Int>.write = NetworkWriterExtensions.WriteVector3Int;
		Writer<Vector3Int?>.write = NetworkWriterExtensions.WriteVector3IntNullable;
		Writer<Color>.write = NetworkWriterExtensions.WriteColor;
		Writer<Color?>.write = NetworkWriterExtensions.WriteColorNullable;
		Writer<Color32>.write = NetworkWriterExtensions.WriteColor32;
		Writer<Color32?>.write = NetworkWriterExtensions.WriteColor32Nullable;
		Writer<Quaternion>.write = NetworkWriterExtensions.WriteQuaternion;
		Writer<Quaternion?>.write = NetworkWriterExtensions.WriteQuaternionNullable;
		Writer<Rect>.write = NetworkWriterExtensions.WriteRect;
		Writer<Rect?>.write = NetworkWriterExtensions.WriteRectNullable;
		Writer<Plane>.write = NetworkWriterExtensions.WritePlane;
		Writer<Plane?>.write = NetworkWriterExtensions.WritePlaneNullable;
		Writer<Ray>.write = NetworkWriterExtensions.WriteRay;
		Writer<Ray?>.write = NetworkWriterExtensions.WriteRayNullable;
		Writer<Matrix4x4>.write = NetworkWriterExtensions.WriteMatrix4x4;
		Writer<Matrix4x4?>.write = NetworkWriterExtensions.WriteMatrix4x4Nullable;
		Writer<Guid>.write = NetworkWriterExtensions.WriteGuid;
		Writer<Guid?>.write = NetworkWriterExtensions.WriteGuidNullable;
		Writer<NetworkIdentity>.write = NetworkWriterExtensions.WriteNetworkIdentity;
		Writer<NetworkBehaviour>.write = NetworkWriterExtensions.WriteNetworkBehaviour;
		Writer<Transform>.write = NetworkWriterExtensions.WriteTransform;
		Writer<GameObject>.write = NetworkWriterExtensions.WriteGameObject;
		Writer<Uri>.write = NetworkWriterExtensions.WriteUri;
		Writer<Texture2D>.write = NetworkWriterExtensions.WriteTexture2D;
		Writer<Sprite>.write = NetworkWriterExtensions.WriteSprite;
		Writer<DateTime>.write = NetworkWriterExtensions.WriteDateTime;
		Writer<DateTime?>.write = NetworkWriterExtensions.WriteDateTimeNullable;
		Writer<TimeSnapshotMessage>.write = _Write_Mirror_002ETimeSnapshotMessage;
		Writer<ReadyMessage>.write = _Write_Mirror_002EReadyMessage;
		Writer<NotReadyMessage>.write = _Write_Mirror_002ENotReadyMessage;
		Writer<AddPlayerMessage>.write = _Write_Mirror_002EAddPlayerMessage;
		Writer<SceneMessage>.write = _Write_Mirror_002ESceneMessage;
		Writer<SceneOperation>.write = _Write_Mirror_002ESceneOperation;
		Writer<CommandMessage>.write = _Write_Mirror_002ECommandMessage;
		Writer<RpcMessage>.write = _Write_Mirror_002ERpcMessage;
		Writer<RpcBufferMessage>.write = _Write_Mirror_002ERpcBufferMessage;
		Writer<SpawnMessage>.write = _Write_Mirror_002ESpawnMessage;
		Writer<ChangeOwnerMessage>.write = _Write_Mirror_002EChangeOwnerMessage;
		Writer<ObjectSpawnStartedMessage>.write = _Write_Mirror_002EObjectSpawnStartedMessage;
		Writer<ObjectSpawnFinishedMessage>.write = _Write_Mirror_002EObjectSpawnFinishedMessage;
		Writer<ObjectDestroyMessage>.write = _Write_Mirror_002EObjectDestroyMessage;
		Writer<ObjectHideMessage>.write = _Write_Mirror_002EObjectHideMessage;
		Writer<EntityStateMessage>.write = _Write_Mirror_002EEntityStateMessage;
		Writer<NetworkPingMessage>.write = _Write_Mirror_002ENetworkPingMessage;
		Writer<NetworkPongMessage>.write = _Write_Mirror_002ENetworkPongMessage;
		Writer<SampleCastInfoContext?>.write = SampleCastInfoContextSerialization.WriteSampleCastInfoContext;
		Writer<SyncMovementData>.write = SyncMovementDataSerializer.WriteSyncMovementData;
		Writer<CastMethodData>.write = CastMethodDataSerialization.WriteCastMethodData;
		Writer<ElementalType?>.write = ElementalTypeSerialization.WriteNullableElementalType;
		Writer<BasicEffect>.write = NetworkReaderWriterExtensions.WriteBasicEffect;
		Writer<IInteractable>.write = NetworkReaderWriterExtensions.WriteIInteractable;
		Writer<Type>.write = NetworkReaderWriterExtensions.WriteType;
		Writer<DewAnimationClip>.write = NetworkReaderWriterExtensions.WriteDewAnimationClip;
		Writer<DewAudioClip>.write = NetworkReaderWriterExtensions.WriteDewAudioClip;
		Writer<AnimationClip>.write = NetworkReaderWriterExtensions.WriteAnimationClip;
		Writer<DewDifficultySettings>.write = NetworkReaderWriterExtensions.WriteDewDifficultySettings;
		Writer<Zone>.write = NetworkReaderWriterExtensions.WriteZone;
		Writer<Key?>.write = NetworkReaderWriterExtensions.WriteNullableKey;
		Writer<GamepadButton?>.write = NetworkReaderWriterExtensions.WriteNullableGamepadButton;
		Writer<KeyCode?>.write = NetworkReaderWriterExtensions.WriteNullableKeyCode;
		Writer<IItem>.write = NetworkReaderWriterExtensions.WriteIHoldableInHand;
		Writer<Displacement>.write = NetworkReaderWriterExtensions.WriteDisplacement;
		Writer<Dictionary<string, string>>.write = NetworkReaderWriterExtensions.WriteDictionary0;
		Writer<Dictionary<string, DewProfileStats.HeroData>>.write = NetworkReaderWriterExtensions.WriteDictionary1;
		Writer<Dictionary<string, DewProfileStats.MonsterData>>.write = NetworkReaderWriterExtensions.WriteDictionary2;
		Writer<Dictionary<string, DewProfileStats.ZoneData>>.write = NetworkReaderWriterExtensions.WriteDictionary3;
		Writer<Dictionary<string, DewProfileStats.ItemData>>.write = NetworkReaderWriterExtensions.WriteDictionary4;
		Writer<CounterBool>.write = NetworkReaderWriterExtensions.WriteCounterBool;
		Writer<AssetRef<DewDifficultySettings>>.write = NetworkReaderWriterExtensions.WriteAssetRef0;
		Writer<GemLocation?>.write = NetworkReaderWriterExtensions.WriteNullableGemLocation;
		Writer<HeroSkillLocation?>.write = NetworkReaderWriterExtensions.WriteNullableHeroSkillLocation;
		Writer<DewEffect.PlayEffectMessage>.write = _Write_DewEffect_002FPlayEffectMessage;
		Writer<DewEffect.PlayPositionedEffectMessage>.write = _Write_DewEffect_002FPlayPositionedEffectMessage;
		Writer<DewEffect.PlayAttachedEffectMessage>.write = _Write_DewEffect_002FPlayAttachedEffectMessage;
		Writer<Entity>.write = NetworkWriterExtensions.WriteNetworkBehaviour;
		Writer<DewEffect.PlayCastEffectMessage>.write = _Write_DewEffect_002FPlayCastEffectMessage;
		Writer<CastInfo>.write = _Write_CastInfo;
		Writer<CastMethodType>.write = _Write_CastMethodType;
		Writer<DewEffect.StopEffectMessage>.write = _Write_DewEffect_002FStopEffectMessage;
		Writer<DewEffect.ApplySpeedMultiplierToEffectMessage>.write = _Write_DewEffect_002FApplySpeedMultiplierToEffectMessage;
		Writer<DewAuthRequestMessage>.write = _Write_DewAuthRequestMessage;
		Writer<DewAuthResponseMessage>.write = _Write_DewAuthResponseMessage;
		Writer<DewExceptionType>.write = _Write_DewExceptionType;
		Writer<DewNetworkManager.ChangeSceneMessage>.write = _Write_DewNetworkManager_002FChangeSceneMessage;
		Writer<DewNetworkManager.SetLoadingStatusMessage>.write = _Write_DewNetworkManager_002FSetLoadingStatusMessage;
		Writer<DewNetworkManager.SessionEndedMessage>.write = _Write_DewNetworkManager_002FSessionEndedMessage;
		Writer<DewNetworkManager.SessionRestartingMessage>.write = _Write_DewNetworkManager_002FSessionRestartingMessage;
		Writer<InGameAnalyticsManager.DisableAnalyticsMessage>.write = _Write_InGameAnalyticsManager_002FDisableAnalyticsMessage;
		Writer<HatredStrengthType>.write = _Write_HatredStrengthType;
		Writer<QuestProgressType>.write = _Write_QuestProgressType;
		Writer<CorruptedChaosRewardType[]>.write = _Write_CorruptedChaosRewardType_005B_005D;
		Writer<CorruptedChaosRewardType>.write = _Write_CorruptedChaosRewardType;
		Writer<float[]>.write = _Write_System_002ESingle_005B_005D;
		Writer<int[]>.write = _Write_System_002EInt32_005B_005D;
		Writer<AbilityTrigger.ConfigSyncData[]>.write = _Write_AbilityTrigger_002FConfigSyncData_005B_005D;
		Writer<AbilityTrigger.ConfigSyncData>.write = _Write_AbilityTrigger_002FConfigSyncData;
		Writer<EventInfoSkillUse>.write = _Write_EventInfoSkillUse;
		Writer<HeroSkillLocation>.write = _Write_HeroSkillLocation;
		Writer<SkillTrigger>.write = NetworkWriterExtensions.WriteNetworkBehaviour;
		Writer<SkillType>.write = _Write_SkillType;
		Writer<DescriptionTags>.write = _Write_DescriptionTags;
		Writer<EventInfoKill>.write = _Write_EventInfoKill;
		Writer<Actor>.write = NetworkWriterExtensions.WriteNetworkBehaviour;
		Writer<Monster.MonsterType>.write = _Write_Monster_002FMonsterType;
		Writer<Projectile.EntityHit>.write = _Write_Projectile_002FEntityHit;
		Writer<Projectile.ProjectileMode>.write = _Write_Projectile_002FProjectileMode;
		Writer<Projectile.StartPositionType>.write = _Write_Projectile_002FStartPositionType;
		Writer<FailReason>.write = _Write_FailReason;
		Writer<QuestState>.write = _Write_QuestState;
		Writer<GemLocation>.write = _Write_GemLocation;
		Writer<StarlessPath_BossPolarisManager.State>.write = _Write_StarlessPath_BossPolarisManager_002FState;
		Writer<Room_Waypoint>.write = NetworkWriterExtensions.WriteNetworkBehaviour;
		Writer<HeroLoadoutData>.write = _Write_HeroLoadoutData;
		Writer<List<LoadoutStarItem>>.write = _Write_System_002ECollections_002EGeneric_002EList_00601_003CLoadoutStarItem_003E;
		Writer<LoadoutStarItem>.write = _Write_LoadoutStarItem;
		Writer<Hero_Aurena.ClawState>.write = _Write_Hero_Aurena_002FClawState;
		Writer<ElementalType>.write = _Write_ElementalType;
		Writer<Summon>.write = NetworkWriterExtensions.WriteNetworkBehaviour;
		Writer<SyncableAssetRef>.write = _Write_SyncableAssetRef;
		Writer<BonusStats>.write = _Write_BonusStats;
		Writer<Shrine_MarshOfDestiny_SeedOfTorment.ChoiceItem>.write = _Write_Shrine_MarshOfDestiny_SeedOfTorment_002FChoiceItem;
		Writer<Shrine_MarshOfDestiny_SeedOfTorment.StrengthType>.write = _Write_Shrine_MarshOfDestiny_SeedOfTorment_002FStrengthType;
		Writer<Shrine_MarshOfDestiny_SeedOfTorment.PenaltyType>.write = _Write_Shrine_MarshOfDestiny_SeedOfTorment_002FPenaltyType;
		Writer<Shrine_MarshOfDestiny_SeedOfTorment.RewardType>.write = _Write_Shrine_MarshOfDestiny_SeedOfTorment_002FRewardType;
		Writer<Ai_Mon_Despair_BossAzurak_StompGlobal.Wave>.write = _Write_Ai_Mon_Despair_BossAzurak_StompGlobal_002FWave;
		Writer<Ai_Mon_Despair_BossAzurak_StompGlobal.Pattern[]>.write = _Write_Ai_Mon_Despair_BossAzurak_StompGlobal_002FPattern_005B_005D;
		Writer<Ai_Mon_Despair_BossAzurak_StompGlobal.Pattern>.write = _Write_Ai_Mon_Despair_BossAzurak_StompGlobal_002FPattern;
		Writer<Se_Mon_Primus_BossPrimusAeron_Adaptation.StatType>.write = _Write_Se_Mon_Primus_BossPrimusAeron_Adaptation_002FStatType;
		Writer<Mon_Primus_BossPrimusAeron.WeaponType>.write = _Write_Mon_Primus_BossPrimusAeron_002FWeaponType;
		Writer<Mon_Primus_BossPrimusAeron.PhaseType>.write = _Write_Mon_Primus_BossPrimusAeron_002FPhaseType;
		Writer<Mon_Special_BossPolaris.MainPhase>.write = _Write_Mon_Special_BossPolaris_002FMainPhase;
		Writer<ChoiceShrineItem[]>.write = _Write_ChoiceShrineItem_005B_005D;
		Writer<ChoiceShrineItem>.write = _Write_ChoiceShrineItem;
		Writer<DewPlayer>.write = NetworkWriterExtensions.WriteNetworkBehaviour;
		Writer<Hero>.write = NetworkWriterExtensions.WriteNetworkBehaviour;
		Writer<ChaosRewardType>.write = _Write_ChaosRewardType;
		Reader<byte>.read = NetworkReaderExtensions.ReadByte;
		Reader<byte?>.read = NetworkReaderExtensions.ReadByteNullable;
		Reader<sbyte>.read = NetworkReaderExtensions.ReadSByte;
		Reader<sbyte?>.read = NetworkReaderExtensions.ReadSByteNullable;
		Reader<char>.read = NetworkReaderExtensions.ReadChar;
		Reader<char?>.read = NetworkReaderExtensions.ReadCharNullable;
		Reader<bool>.read = NetworkReaderExtensions.ReadBool;
		Reader<bool?>.read = NetworkReaderExtensions.ReadBoolNullable;
		Reader<short>.read = NetworkReaderExtensions.ReadShort;
		Reader<short?>.read = NetworkReaderExtensions.ReadShortNullable;
		Reader<ushort>.read = NetworkReaderExtensions.ReadUShort;
		Reader<ushort?>.read = NetworkReaderExtensions.ReadUShortNullable;
		Reader<int>.read = NetworkReaderExtensions.ReadInt;
		Reader<int?>.read = NetworkReaderExtensions.ReadIntNullable;
		Reader<uint>.read = NetworkReaderExtensions.ReadUInt;
		Reader<uint?>.read = NetworkReaderExtensions.ReadUIntNullable;
		Reader<long>.read = NetworkReaderExtensions.ReadLong;
		Reader<long?>.read = NetworkReaderExtensions.ReadLongNullable;
		Reader<ulong>.read = NetworkReaderExtensions.ReadULong;
		Reader<ulong?>.read = NetworkReaderExtensions.ReadULongNullable;
		Reader<float>.read = NetworkReaderExtensions.ReadFloat;
		Reader<float?>.read = NetworkReaderExtensions.ReadFloatNullable;
		Reader<double>.read = NetworkReaderExtensions.ReadDouble;
		Reader<double?>.read = NetworkReaderExtensions.ReadDoubleNullable;
		Reader<decimal>.read = NetworkReaderExtensions.ReadDecimal;
		Reader<decimal?>.read = NetworkReaderExtensions.ReadDecimalNullable;
		Reader<string>.read = NetworkReaderExtensions.ReadString;
		Reader<byte[]>.read = NetworkReaderExtensions.ReadBytesAndSize;
		Reader<ArraySegment<byte>>.read = NetworkReaderExtensions.ReadBytesAndSizeSegment;
		Reader<Vector2>.read = NetworkReaderExtensions.ReadVector2;
		Reader<Vector2?>.read = NetworkReaderExtensions.ReadVector2Nullable;
		Reader<Vector3>.read = NetworkReaderExtensions.ReadVector3;
		Reader<Vector3?>.read = NetworkReaderExtensions.ReadVector3Nullable;
		Reader<Vector4>.read = NetworkReaderExtensions.ReadVector4;
		Reader<Vector4?>.read = NetworkReaderExtensions.ReadVector4Nullable;
		Reader<Vector2Int>.read = NetworkReaderExtensions.ReadVector2Int;
		Reader<Vector2Int?>.read = NetworkReaderExtensions.ReadVector2IntNullable;
		Reader<Vector3Int>.read = NetworkReaderExtensions.ReadVector3Int;
		Reader<Vector3Int?>.read = NetworkReaderExtensions.ReadVector3IntNullable;
		Reader<Color>.read = NetworkReaderExtensions.ReadColor;
		Reader<Color?>.read = NetworkReaderExtensions.ReadColorNullable;
		Reader<Color32>.read = NetworkReaderExtensions.ReadColor32;
		Reader<Color32?>.read = NetworkReaderExtensions.ReadColor32Nullable;
		Reader<Quaternion>.read = NetworkReaderExtensions.ReadQuaternion;
		Reader<Quaternion?>.read = NetworkReaderExtensions.ReadQuaternionNullable;
		Reader<Rect>.read = NetworkReaderExtensions.ReadRect;
		Reader<Rect?>.read = NetworkReaderExtensions.ReadRectNullable;
		Reader<Plane>.read = NetworkReaderExtensions.ReadPlane;
		Reader<Plane?>.read = NetworkReaderExtensions.ReadPlaneNullable;
		Reader<Ray>.read = NetworkReaderExtensions.ReadRay;
		Reader<Ray?>.read = NetworkReaderExtensions.ReadRayNullable;
		Reader<Matrix4x4>.read = NetworkReaderExtensions.ReadMatrix4x4;
		Reader<Matrix4x4?>.read = NetworkReaderExtensions.ReadMatrix4x4Nullable;
		Reader<Guid>.read = NetworkReaderExtensions.ReadGuid;
		Reader<Guid?>.read = NetworkReaderExtensions.ReadGuidNullable;
		Reader<NetworkIdentity>.read = NetworkReaderExtensions.ReadNetworkIdentity;
		Reader<NetworkBehaviour>.read = NetworkReaderExtensions.ReadNetworkBehaviour;
		Reader<NetworkBehaviourSyncVar>.read = NetworkReaderExtensions.ReadNetworkBehaviourSyncVar;
		Reader<Transform>.read = NetworkReaderExtensions.ReadTransform;
		Reader<GameObject>.read = NetworkReaderExtensions.ReadGameObject;
		Reader<Uri>.read = NetworkReaderExtensions.ReadUri;
		Reader<Texture2D>.read = NetworkReaderExtensions.ReadTexture2D;
		Reader<Sprite>.read = NetworkReaderExtensions.ReadSprite;
		Reader<DateTime>.read = NetworkReaderExtensions.ReadDateTime;
		Reader<DateTime?>.read = NetworkReaderExtensions.ReadDateTimeNullable;
		Reader<TimeSnapshotMessage>.read = _Read_Mirror_002ETimeSnapshotMessage;
		Reader<ReadyMessage>.read = _Read_Mirror_002EReadyMessage;
		Reader<NotReadyMessage>.read = _Read_Mirror_002ENotReadyMessage;
		Reader<AddPlayerMessage>.read = _Read_Mirror_002EAddPlayerMessage;
		Reader<SceneMessage>.read = _Read_Mirror_002ESceneMessage;
		Reader<SceneOperation>.read = _Read_Mirror_002ESceneOperation;
		Reader<CommandMessage>.read = _Read_Mirror_002ECommandMessage;
		Reader<RpcMessage>.read = _Read_Mirror_002ERpcMessage;
		Reader<RpcBufferMessage>.read = _Read_Mirror_002ERpcBufferMessage;
		Reader<SpawnMessage>.read = _Read_Mirror_002ESpawnMessage;
		Reader<ChangeOwnerMessage>.read = _Read_Mirror_002EChangeOwnerMessage;
		Reader<ObjectSpawnStartedMessage>.read = _Read_Mirror_002EObjectSpawnStartedMessage;
		Reader<ObjectSpawnFinishedMessage>.read = _Read_Mirror_002EObjectSpawnFinishedMessage;
		Reader<ObjectDestroyMessage>.read = _Read_Mirror_002EObjectDestroyMessage;
		Reader<ObjectHideMessage>.read = _Read_Mirror_002EObjectHideMessage;
		Reader<EntityStateMessage>.read = _Read_Mirror_002EEntityStateMessage;
		Reader<NetworkPingMessage>.read = _Read_Mirror_002ENetworkPingMessage;
		Reader<NetworkPongMessage>.read = _Read_Mirror_002ENetworkPongMessage;
		Reader<SampleCastInfoContext?>.read = SampleCastInfoContextSerialization.ReadSampleCastInfoContext;
		Reader<SyncMovementData>.read = SyncMovementDataSerializer.ReadSyncMovementData;
		Reader<CastMethodData>.read = CastMethodDataSerialization.ReadWriteCastMethodData;
		Reader<ElementalType?>.read = ElementalTypeSerialization.ReadNullableElementalType;
		Reader<BasicEffect>.read = NetworkReaderWriterExtensions.ReadBasicEffect;
		Reader<IInteractable>.read = NetworkReaderWriterExtensions.ReadIInteractable;
		Reader<Type>.read = NetworkReaderWriterExtensions.ReadType;
		Reader<DewAnimationClip>.read = NetworkReaderWriterExtensions.ReadDewAnimationClip;
		Reader<DewAudioClip>.read = NetworkReaderWriterExtensions.ReadDewAudioClip;
		Reader<AnimationClip>.read = NetworkReaderWriterExtensions.ReadAnimationClip;
		Reader<DewDifficultySettings>.read = NetworkReaderWriterExtensions.ReadDewDifficultySettings;
		Reader<Zone>.read = NetworkReaderWriterExtensions.ReadZone;
		Reader<Key?>.read = NetworkReaderWriterExtensions.ReadNullableKey;
		Reader<GamepadButton?>.read = NetworkReaderWriterExtensions.ReadNullableGamepadButton;
		Reader<KeyCode?>.read = NetworkReaderWriterExtensions.ReadNullableKeyCode;
		Reader<IItem>.read = NetworkReaderWriterExtensions.ReadIHoldableInHand;
		Reader<Displacement>.read = NetworkReaderWriterExtensions.ReadDisplacement;
		Reader<Dictionary<string, string>>.read = NetworkReaderWriterExtensions.ReadDictionary0;
		Reader<Dictionary<string, DewProfileStats.HeroData>>.read = NetworkReaderWriterExtensions.ReadDictionary1;
		Reader<Dictionary<string, DewProfileStats.MonsterData>>.read = NetworkReaderWriterExtensions.ReadDictionary2;
		Reader<Dictionary<string, DewProfileStats.ZoneData>>.read = NetworkReaderWriterExtensions.ReadDictionary3;
		Reader<Dictionary<string, DewProfileStats.ItemData>>.read = NetworkReaderWriterExtensions.ReadDictionary4;
		Reader<CounterBool>.read = NetworkReaderWriterExtensions.ReadCounterBool;
		Reader<AssetRef<DewDifficultySettings>>.read = NetworkReaderWriterExtensions.ReadAssetRef0;
		Reader<GemLocation?>.read = NetworkReaderWriterExtensions.ReadNullableGemLocation;
		Reader<HeroSkillLocation?>.read = NetworkReaderWriterExtensions.ReadNullableHeroSkillLocation;
		Reader<DewEffect.PlayEffectMessage>.read = _Read_DewEffect_002FPlayEffectMessage;
		Reader<DewEffect.PlayPositionedEffectMessage>.read = _Read_DewEffect_002FPlayPositionedEffectMessage;
		Reader<DewEffect.PlayAttachedEffectMessage>.read = _Read_DewEffect_002FPlayAttachedEffectMessage;
		Reader<Entity>.read = NetworkReaderExtensions.ReadNetworkBehaviour<Entity>;
		Reader<DewEffect.PlayCastEffectMessage>.read = _Read_DewEffect_002FPlayCastEffectMessage;
		Reader<CastInfo>.read = _Read_CastInfo;
		Reader<CastMethodType>.read = _Read_CastMethodType;
		Reader<DewEffect.StopEffectMessage>.read = _Read_DewEffect_002FStopEffectMessage;
		Reader<DewEffect.ApplySpeedMultiplierToEffectMessage>.read = _Read_DewEffect_002FApplySpeedMultiplierToEffectMessage;
		Reader<DewAuthRequestMessage>.read = _Read_DewAuthRequestMessage;
		Reader<DewAuthResponseMessage>.read = _Read_DewAuthResponseMessage;
		Reader<DewExceptionType>.read = _Read_DewExceptionType;
		Reader<DewNetworkManager.ChangeSceneMessage>.read = _Read_DewNetworkManager_002FChangeSceneMessage;
		Reader<DewNetworkManager.SetLoadingStatusMessage>.read = _Read_DewNetworkManager_002FSetLoadingStatusMessage;
		Reader<DewNetworkManager.SessionEndedMessage>.read = _Read_DewNetworkManager_002FSessionEndedMessage;
		Reader<DewNetworkManager.SessionRestartingMessage>.read = _Read_DewNetworkManager_002FSessionRestartingMessage;
		Reader<InGameAnalyticsManager.DisableAnalyticsMessage>.read = _Read_InGameAnalyticsManager_002FDisableAnalyticsMessage;
		Reader<HatredStrengthType>.read = _Read_HatredStrengthType;
		Reader<QuestProgressType>.read = _Read_QuestProgressType;
		Reader<CorruptedChaosRewardType[]>.read = _Read_CorruptedChaosRewardType_005B_005D;
		Reader<CorruptedChaosRewardType>.read = _Read_CorruptedChaosRewardType;
		Reader<float[]>.read = _Read_System_002ESingle_005B_005D;
		Reader<int[]>.read = _Read_System_002EInt32_005B_005D;
		Reader<AbilityTrigger.ConfigSyncData[]>.read = _Read_AbilityTrigger_002FConfigSyncData_005B_005D;
		Reader<AbilityTrigger.ConfigSyncData>.read = _Read_AbilityTrigger_002FConfigSyncData;
		Reader<EventInfoSkillUse>.read = _Read_EventInfoSkillUse;
		Reader<HeroSkillLocation>.read = _Read_HeroSkillLocation;
		Reader<SkillTrigger>.read = NetworkReaderExtensions.ReadNetworkBehaviour<SkillTrigger>;
		Reader<SkillType>.read = _Read_SkillType;
		Reader<DescriptionTags>.read = _Read_DescriptionTags;
		Reader<EventInfoKill>.read = _Read_EventInfoKill;
		Reader<Actor>.read = NetworkReaderExtensions.ReadNetworkBehaviour<Actor>;
		Reader<Monster.MonsterType>.read = _Read_Monster_002FMonsterType;
		Reader<Projectile.EntityHit>.read = _Read_Projectile_002FEntityHit;
		Reader<Projectile.ProjectileMode>.read = _Read_Projectile_002FProjectileMode;
		Reader<Projectile.StartPositionType>.read = _Read_Projectile_002FStartPositionType;
		Reader<FailReason>.read = _Read_FailReason;
		Reader<QuestState>.read = _Read_QuestState;
		Reader<GemLocation>.read = _Read_GemLocation;
		Reader<StarlessPath_BossPolarisManager.State>.read = _Read_StarlessPath_BossPolarisManager_002FState;
		Reader<Room_Waypoint>.read = NetworkReaderExtensions.ReadNetworkBehaviour<Room_Waypoint>;
		Reader<HeroLoadoutData>.read = _Read_HeroLoadoutData;
		Reader<List<LoadoutStarItem>>.read = _Read_System_002ECollections_002EGeneric_002EList_00601_003CLoadoutStarItem_003E;
		Reader<LoadoutStarItem>.read = _Read_LoadoutStarItem;
		Reader<Hero_Aurena.ClawState>.read = _Read_Hero_Aurena_002FClawState;
		Reader<ElementalType>.read = _Read_ElementalType;
		Reader<Summon>.read = NetworkReaderExtensions.ReadNetworkBehaviour<Summon>;
		Reader<SyncableAssetRef>.read = _Read_SyncableAssetRef;
		Reader<BonusStats>.read = _Read_BonusStats;
		Reader<Shrine_MarshOfDestiny_SeedOfTorment.ChoiceItem>.read = _Read_Shrine_MarshOfDestiny_SeedOfTorment_002FChoiceItem;
		Reader<Shrine_MarshOfDestiny_SeedOfTorment.StrengthType>.read = _Read_Shrine_MarshOfDestiny_SeedOfTorment_002FStrengthType;
		Reader<Shrine_MarshOfDestiny_SeedOfTorment.PenaltyType>.read = _Read_Shrine_MarshOfDestiny_SeedOfTorment_002FPenaltyType;
		Reader<Shrine_MarshOfDestiny_SeedOfTorment.RewardType>.read = _Read_Shrine_MarshOfDestiny_SeedOfTorment_002FRewardType;
		Reader<Ai_Mon_Despair_BossAzurak_StompGlobal.Wave>.read = _Read_Ai_Mon_Despair_BossAzurak_StompGlobal_002FWave;
		Reader<Ai_Mon_Despair_BossAzurak_StompGlobal.Pattern[]>.read = _Read_Ai_Mon_Despair_BossAzurak_StompGlobal_002FPattern_005B_005D;
		Reader<Ai_Mon_Despair_BossAzurak_StompGlobal.Pattern>.read = _Read_Ai_Mon_Despair_BossAzurak_StompGlobal_002FPattern;
		Reader<Se_Mon_Primus_BossPrimusAeron_Adaptation.StatType>.read = _Read_Se_Mon_Primus_BossPrimusAeron_Adaptation_002FStatType;
		Reader<Mon_Primus_BossPrimusAeron.WeaponType>.read = _Read_Mon_Primus_BossPrimusAeron_002FWeaponType;
		Reader<Mon_Primus_BossPrimusAeron.PhaseType>.read = _Read_Mon_Primus_BossPrimusAeron_002FPhaseType;
		Reader<Mon_Special_BossPolaris.MainPhase>.read = _Read_Mon_Special_BossPolaris_002FMainPhase;
		Reader<ChoiceShrineItem[]>.read = _Read_ChoiceShrineItem_005B_005D;
		Reader<ChoiceShrineItem>.read = _Read_ChoiceShrineItem;
		Reader<DewPlayer>.read = NetworkReaderExtensions.ReadNetworkBehaviour<DewPlayer>;
		Reader<Hero>.read = NetworkReaderExtensions.ReadNetworkBehaviour<Hero>;
		Reader<ChaosRewardType>.read = _Read_ChaosRewardType;
	}
}
