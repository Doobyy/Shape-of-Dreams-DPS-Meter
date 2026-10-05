using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Steamworks;
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

	public static void _Write_QuestProgressType(NetworkWriter writer, QuestProgressType value)
	{
		NetworkWriterExtensions.WriteInt(writer, (int)value);
	}

	public static void _Write_FailReason(NetworkWriter writer, FailReason value)
	{
		NetworkWriterExtensions.WriteInt(writer, (int)value);
	}

	public static void _Write_QuestState(NetworkWriter writer, QuestState value)
	{
		NetworkWriterExtensions.WriteInt(writer, (int)value);
	}

	public static QuestProgressType _Read_QuestProgressType(NetworkReader reader)
	{
		return (QuestProgressType)NetworkReaderExtensions.ReadInt(reader);
	}

	public static FailReason _Read_FailReason(NetworkReader reader)
	{
		return (FailReason)NetworkReaderExtensions.ReadInt(reader);
	}

	public static QuestState _Read_QuestState(NetworkReader reader)
	{
		return (QuestState)NetworkReaderExtensions.ReadInt(reader);
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

	public static MerchandiseData[] _Read_MerchandiseData_005B_005D(NetworkReader reader)
	{
		return NetworkReaderExtensions.ReadArray<MerchandiseData>(reader);
	}

	public static MerchandiseData _Read_MerchandiseData(NetworkReader reader)
	{
		return new MerchandiseData
		{
			type = _Read_MerchandiseType(reader),
			itemName = NetworkReaderExtensions.ReadString(reader),
			level = NetworkReaderExtensions.ReadInt(reader),
			price = _Read_Cost(reader),
			count = NetworkReaderExtensions.ReadInt(reader),
			customData = NetworkReaderExtensions.ReadString(reader)
		};
	}

	public static MerchandiseType _Read_MerchandiseType(NetworkReader reader)
	{
		return (MerchandiseType)NetworkReaderExtensions.ReadInt(reader);
	}

	public static Cost _Read_Cost(NetworkReader reader)
	{
		return new Cost
		{
			gold = NetworkReaderExtensions.ReadInt(reader),
			dreamDust = NetworkReaderExtensions.ReadInt(reader),
			stardust = NetworkReaderExtensions.ReadInt(reader),
			healthPercentage = NetworkReaderExtensions.ReadInt(reader),
			platinumCoin = NetworkReaderExtensions.ReadInt(reader)
		};
	}

	public static void _Write_MerchandiseData_005B_005D(NetworkWriter writer, MerchandiseData[] value)
	{
		NetworkWriterExtensions.WriteArray<MerchandiseData>(writer, value);
	}

	public static void _Write_MerchandiseData(NetworkWriter writer, MerchandiseData value)
	{
		_Write_MerchandiseType(writer, value.type);
		NetworkWriterExtensions.WriteString(writer, value.itemName);
		NetworkWriterExtensions.WriteInt(writer, value.level);
		_Write_Cost(writer, value.price);
		NetworkWriterExtensions.WriteInt(writer, value.count);
		NetworkWriterExtensions.WriteString(writer, value.customData);
	}

	public static void _Write_MerchandiseType(NetworkWriter writer, MerchandiseType value)
	{
		NetworkWriterExtensions.WriteInt(writer, (int)value);
	}

	public static void _Write_Cost(NetworkWriter writer, Cost value)
	{
		NetworkWriterExtensions.WriteInt(writer, value.gold);
		NetworkWriterExtensions.WriteInt(writer, value.dreamDust);
		NetworkWriterExtensions.WriteInt(writer, value.stardust);
		NetworkWriterExtensions.WriteInt(writer, value.healthPercentage);
		NetworkWriterExtensions.WriteInt(writer, value.platinumCoin);
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

	public static void _Write_System_002ECollections_002EGeneric_002EList_00601_003CSystem_002EString_003E(NetworkWriter writer, List<string> value)
	{
		NetworkWriterExtensions.WriteList<string>(writer, value);
	}

	public static List<string> _Read_System_002ECollections_002EGeneric_002EList_00601_003CSystem_002EString_003E(NetworkReader reader)
	{
		return NetworkReaderExtensions.ReadList<string>(reader);
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

	public static void _Write_StatBonus(NetworkWriter writer, StatBonus value)
	{
		if (value == null)
		{
			NetworkWriterExtensions.WriteBool(writer, false);
			return;
		}
		NetworkWriterExtensions.WriteBool(writer, true);
		NetworkWriterExtensions.WriteFloat(writer, value._attackDamageFlat);
		NetworkWriterExtensions.WriteFloat(writer, value._attackDamagePercentage);
		NetworkWriterExtensions.WriteFloat(writer, value._abilityPowerFlat);
		NetworkWriterExtensions.WriteFloat(writer, value._abilityPowerPercentage);
		NetworkWriterExtensions.WriteFloat(writer, value._maxHealthFlat);
		NetworkWriterExtensions.WriteFloat(writer, value._maxHealthPercentage);
		NetworkWriterExtensions.WriteFloat(writer, value._maxManaFlat);
		NetworkWriterExtensions.WriteFloat(writer, value._maxManaPercentage);
		NetworkWriterExtensions.WriteFloat(writer, value._healthRegenFlat);
		NetworkWriterExtensions.WriteFloat(writer, value._healthRegenPercentage);
		NetworkWriterExtensions.WriteFloat(writer, value._manaRegenFlat);
		NetworkWriterExtensions.WriteFloat(writer, value._manaRegenPercentage);
		NetworkWriterExtensions.WriteFloat(writer, value._attackSpeedPercentage);
		NetworkWriterExtensions.WriteFloat(writer, value._critAmpFlat);
		NetworkWriterExtensions.WriteFloat(writer, value._critAmpPercentage);
		NetworkWriterExtensions.WriteFloat(writer, value._critChanceFlat);
		NetworkWriterExtensions.WriteFloat(writer, value._critChancePercentage);
		NetworkWriterExtensions.WriteFloat(writer, value._abilityHasteFlat);
		NetworkWriterExtensions.WriteFloat(writer, value._abilityHastePercentage);
		NetworkWriterExtensions.WriteFloat(writer, value._tenacityFlat);
		NetworkWriterExtensions.WriteFloat(writer, value._tenacityPercentage);
		NetworkWriterExtensions.WriteFloat(writer, value._movementSpeedPercentage);
		NetworkWriterExtensions.WriteFloat(writer, value._fireEffectAmpFlat);
		NetworkWriterExtensions.WriteFloat(writer, value._coldEffectAmpFlat);
		NetworkWriterExtensions.WriteFloat(writer, value._lightEffectAmpFlat);
		NetworkWriterExtensions.WriteFloat(writer, value._darkEffectAmpFlat);
		NetworkWriterExtensions.WriteFloat(writer, value._armorFlat);
		NetworkWriterExtensions.WriteFloat(writer, value._armorPercentage);
		NetworkWriterExtensions.WriteFloat(writer, value._attackRangeFlat);
		NetworkWriterExtensions.WriteFloat(writer, value._attackRangePercentage);
		NetworkWriterExtensions.WriteInt(writer, value._everyFourAttackStartIndexFlat);
		NetworkWriterExtensions.WriteBool(writer, value._isDirty);
	}

	public static StatBonus _Read_StatBonus(NetworkReader reader)
	{
		if (!NetworkReaderExtensions.ReadBool(reader))
		{
			return null;
		}
		StatBonus statBonus = new StatBonus();
		statBonus._attackDamageFlat = NetworkReaderExtensions.ReadFloat(reader);
		statBonus._attackDamagePercentage = NetworkReaderExtensions.ReadFloat(reader);
		statBonus._abilityPowerFlat = NetworkReaderExtensions.ReadFloat(reader);
		statBonus._abilityPowerPercentage = NetworkReaderExtensions.ReadFloat(reader);
		statBonus._maxHealthFlat = NetworkReaderExtensions.ReadFloat(reader);
		statBonus._maxHealthPercentage = NetworkReaderExtensions.ReadFloat(reader);
		statBonus._maxManaFlat = NetworkReaderExtensions.ReadFloat(reader);
		statBonus._maxManaPercentage = NetworkReaderExtensions.ReadFloat(reader);
		statBonus._healthRegenFlat = NetworkReaderExtensions.ReadFloat(reader);
		statBonus._healthRegenPercentage = NetworkReaderExtensions.ReadFloat(reader);
		statBonus._manaRegenFlat = NetworkReaderExtensions.ReadFloat(reader);
		statBonus._manaRegenPercentage = NetworkReaderExtensions.ReadFloat(reader);
		statBonus._attackSpeedPercentage = NetworkReaderExtensions.ReadFloat(reader);
		statBonus._critAmpFlat = NetworkReaderExtensions.ReadFloat(reader);
		statBonus._critAmpPercentage = NetworkReaderExtensions.ReadFloat(reader);
		statBonus._critChanceFlat = NetworkReaderExtensions.ReadFloat(reader);
		statBonus._critChancePercentage = NetworkReaderExtensions.ReadFloat(reader);
		statBonus._abilityHasteFlat = NetworkReaderExtensions.ReadFloat(reader);
		statBonus._abilityHastePercentage = NetworkReaderExtensions.ReadFloat(reader);
		statBonus._tenacityFlat = NetworkReaderExtensions.ReadFloat(reader);
		statBonus._tenacityPercentage = NetworkReaderExtensions.ReadFloat(reader);
		statBonus._movementSpeedPercentage = NetworkReaderExtensions.ReadFloat(reader);
		statBonus._fireEffectAmpFlat = NetworkReaderExtensions.ReadFloat(reader);
		statBonus._coldEffectAmpFlat = NetworkReaderExtensions.ReadFloat(reader);
		statBonus._lightEffectAmpFlat = NetworkReaderExtensions.ReadFloat(reader);
		statBonus._darkEffectAmpFlat = NetworkReaderExtensions.ReadFloat(reader);
		statBonus._armorFlat = NetworkReaderExtensions.ReadFloat(reader);
		statBonus._armorPercentage = NetworkReaderExtensions.ReadFloat(reader);
		statBonus._attackRangeFlat = NetworkReaderExtensions.ReadFloat(reader);
		statBonus._attackRangePercentage = NetworkReaderExtensions.ReadFloat(reader);
		statBonus._everyFourAttackStartIndexFlat = NetworkReaderExtensions.ReadInt(reader);
		statBonus._isDirty = NetworkReaderExtensions.ReadBool(reader);
		return statBonus;
	}

	public static ChaosReward[] _Read_ChaosReward_005B_005D(NetworkReader reader)
	{
		return NetworkReaderExtensions.ReadArray<ChaosReward>(reader);
	}

	public static ChaosReward _Read_ChaosReward(NetworkReader reader)
	{
		return new ChaosReward
		{
			type = _Read_ChaosRewardType(reader),
			quantity = NetworkReaderExtensions.ReadFloat(reader),
			rarity = _Read_Rarity(reader)
		};
	}

	public static ChaosRewardType _Read_ChaosRewardType(NetworkReader reader)
	{
		return (ChaosRewardType)NetworkReaderExtensions.ReadInt(reader);
	}

	public static Rarity _Read_Rarity(NetworkReader reader)
	{
		return (Rarity)NetworkReaderExtensions.ReadByte(reader);
	}

	public static void _Write_ChaosReward_005B_005D(NetworkWriter writer, ChaosReward[] value)
	{
		NetworkWriterExtensions.WriteArray<ChaosReward>(writer, value);
	}

	public static void _Write_ChaosReward(NetworkWriter writer, ChaosReward value)
	{
		_Write_ChaosRewardType(writer, value.type);
		NetworkWriterExtensions.WriteFloat(writer, value.quantity);
		_Write_Rarity(writer, value.rarity);
	}

	public static void _Write_ChaosRewardType(NetworkWriter writer, ChaosRewardType value)
	{
		NetworkWriterExtensions.WriteInt(writer, (int)value);
	}

	public static void _Write_Rarity(NetworkWriter writer, Rarity value)
	{
		NetworkWriterExtensions.WriteByte(writer, (byte)value);
	}

	public static void _Write_SyncableAssetRef(NetworkWriter writer, SyncableAssetRef value)
	{
		NetworkWriterExtensions.WriteString(writer, value.guid);
		NetworkWriterExtensions.WriteString(writer, value.typeName);
		NetworkWriterExtensions.WriteString(writer, value.typeAssemblyQualifiedName);
		NetworkWriterExtensions.WriteBool(writer, value.isMonoBehaviour);
		NetworkWriterExtensions.WriteBool(writer, value.isActor);
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

	public static void _Write_System_002EString_005B_005D(NetworkWriter writer, string[] value)
	{
		NetworkWriterExtensions.WriteArray<string>(writer, value);
	}

	public static string[] _Read_System_002EString_005B_005D(NetworkReader reader)
	{
		return NetworkReaderExtensions.ReadArray<string>(reader);
	}

	public static void _Write_System_002ECollections_002EGeneric_002EList_00601_003CJsonOverrideItem_003E(NetworkWriter writer, List<JsonOverrideItem> value)
	{
		NetworkWriterExtensions.WriteList<JsonOverrideItem>(writer, value);
	}

	public static void _Write_JsonOverrideItem(NetworkWriter writer, JsonOverrideItem value)
	{
		if (value == null)
		{
			NetworkWriterExtensions.WriteBool(writer, false);
			return;
		}
		NetworkWriterExtensions.WriteBool(writer, true);
		NetworkWriterExtensions.WriteString(writer, value.target);
		writer.WriteDictionary0(value.overrides);
	}

	public static List<JsonOverrideItem> _Read_System_002ECollections_002EGeneric_002EList_00601_003CJsonOverrideItem_003E(NetworkReader reader)
	{
		return NetworkReaderExtensions.ReadList<JsonOverrideItem>(reader);
	}

	public static JsonOverrideItem _Read_JsonOverrideItem(NetworkReader reader)
	{
		if (!NetworkReaderExtensions.ReadBool(reader))
		{
			return null;
		}
		JsonOverrideItem jsonOverrideItem = new JsonOverrideItem();
		jsonOverrideItem.target = NetworkReaderExtensions.ReadString(reader);
		jsonOverrideItem.overrides = reader.ReadDictionary0();
		return jsonOverrideItem;
	}

	public static void _Write_InputMode(NetworkWriter writer, InputMode value)
	{
		NetworkWriterExtensions.WriteInt(writer, (int)value);
	}

	public static InputMode _Read_InputMode(NetworkReader reader)
	{
		return (InputMode)NetworkReaderExtensions.ReadInt(reader);
	}

	public static void _Write_WorldMessageSetting(NetworkWriter writer, WorldMessageSetting value)
	{
		if (value == null)
		{
			NetworkWriterExtensions.WriteBool(writer, false);
			return;
		}
		NetworkWriterExtensions.WriteBool(writer, true);
		NetworkWriterExtensions.WriteString(writer, value.rawText);
		NetworkWriterExtensions.WriteVector3(writer, value.worldPos);
		NetworkWriterExtensions.WriteColor(writer, value.color);
		NetworkWriterExtensions.WriteVector2Nullable(writer, value.popOffset);
	}

	public static WorldMessageSetting _Read_WorldMessageSetting(NetworkReader reader)
	{
		if (!NetworkReaderExtensions.ReadBool(reader))
		{
			return null;
		}
		WorldMessageSetting worldMessageSetting = new WorldMessageSetting();
		worldMessageSetting.rawText = NetworkReaderExtensions.ReadString(reader);
		worldMessageSetting.worldPos = NetworkReaderExtensions.ReadVector3(reader);
		worldMessageSetting.color = NetworkReaderExtensions.ReadColor(reader);
		worldMessageSetting.popOffset = NetworkReaderExtensions.ReadVector2Nullable(reader);
		return worldMessageSetting;
	}

	public static void _Write_CenterMessageType(NetworkWriter writer, CenterMessageType value)
	{
		NetworkWriterExtensions.WriteInt(writer, (int)value);
	}

	public static CenterMessageType _Read_CenterMessageType(NetworkReader reader)
	{
		return (CenterMessageType)NetworkReaderExtensions.ReadInt(reader);
	}

	public static void _Write_NetworkedOnScreenTimerHandle(NetworkWriter writer, NetworkedOnScreenTimerHandle value)
	{
		if (value == null)
		{
			NetworkWriterExtensions.WriteBool(writer, false);
			return;
		}
		NetworkWriterExtensions.WriteBool(writer, true);
		NetworkWriterExtensions.WriteString(writer, value.localeKey);
		NetworkWriterExtensions.WriteString(writer, value.skillKey);
		NetworkWriterExtensions.WriteInt(writer, value._id);
	}

	public static NetworkedOnScreenTimerHandle _Read_NetworkedOnScreenTimerHandle(NetworkReader reader)
	{
		if (!NetworkReaderExtensions.ReadBool(reader))
		{
			return null;
		}
		NetworkedOnScreenTimerHandle networkedOnScreenTimerHandle = new NetworkedOnScreenTimerHandle();
		networkedOnScreenTimerHandle.localeKey = NetworkReaderExtensions.ReadString(reader);
		networkedOnScreenTimerHandle.skillKey = NetworkReaderExtensions.ReadString(reader);
		networkedOnScreenTimerHandle._id = NetworkReaderExtensions.ReadInt(reader);
		return networkedOnScreenTimerHandle;
	}

	public static void _Write_SampleCastInfoContext(NetworkWriter writer, SampleCastInfoContext value)
	{
		writer.WriteCastMethodData(value.castMethod);
		_Write_AbilityTargetValidator(writer, value.targetValidator);
		NetworkWriterExtensions.WriteNetworkBehaviour(writer, (NetworkBehaviour)(object)value.trigger);
		NetworkWriterExtensions.WriteBool(writer, value.showCastIndicator);
		NetworkWriterExtensions.WriteBool(writer, value.isInitialInfoSet);
		_Write_CastInfo(writer, value.currentInfo);
		_Write_SampleCastInfoContext_002FCastOnButtonType(writer, value.castOnButton);
		NetworkWriterExtensions.WriteFloat(writer, value.angleSpeedLimit);
	}

	public static void _Write_AbilityTargetValidator(NetworkWriter writer, AbilityTargetValidator value)
	{
		if (value == null)
		{
			NetworkWriterExtensions.WriteBool(writer, false);
			return;
		}
		NetworkWriterExtensions.WriteBool(writer, true);
		_Write_EntityRelation(writer, value.targets);
	}

	public static void _Write_EntityRelation(NetworkWriter writer, EntityRelation value)
	{
		NetworkWriterExtensions.WriteByte(writer, (byte)value);
	}

	public static void _Write_SampleCastInfoContext_002FCastOnButtonType(NetworkWriter writer, SampleCastInfoContext.CastOnButtonType value)
	{
		NetworkWriterExtensions.WriteInt(writer, (int)value);
	}

	public static SampleCastInfoContext _Read_SampleCastInfoContext(NetworkReader reader)
	{
		return new SampleCastInfoContext
		{
			castMethod = reader.ReadWriteCastMethodData(),
			targetValidator = _Read_AbilityTargetValidator(reader),
			trigger = NetworkReaderExtensions.ReadNetworkBehaviour<AbilityTrigger>(reader),
			showCastIndicator = NetworkReaderExtensions.ReadBool(reader),
			isInitialInfoSet = NetworkReaderExtensions.ReadBool(reader),
			currentInfo = _Read_CastInfo(reader),
			castOnButton = _Read_SampleCastInfoContext_002FCastOnButtonType(reader),
			angleSpeedLimit = NetworkReaderExtensions.ReadFloat(reader)
		};
	}

	public static AbilityTargetValidator _Read_AbilityTargetValidator(NetworkReader reader)
	{
		if (!NetworkReaderExtensions.ReadBool(reader))
		{
			return null;
		}
		AbilityTargetValidator abilityTargetValidator = new AbilityTargetValidator();
		abilityTargetValidator.targets = _Read_EntityRelation(reader);
		return abilityTargetValidator;
	}

	public static EntityRelation _Read_EntityRelation(NetworkReader reader)
	{
		return (EntityRelation)NetworkReaderExtensions.ReadByte(reader);
	}

	public static SampleCastInfoContext.CastOnButtonType _Read_SampleCastInfoContext_002FCastOnButtonType(NetworkReader reader)
	{
		return (SampleCastInfoContext.CastOnButtonType)NetworkReaderExtensions.ReadInt(reader);
	}

	public static void _Write_DewProfileStats(NetworkWriter writer, DewProfileStats value)
	{
		if (value == null)
		{
			NetworkWriterExtensions.WriteBool(writer, false);
			return;
		}
		NetworkWriterExtensions.WriteBool(writer, true);
		_Write_DewProfileStats_002FHeroData(writer, value.total);
		writer.WriteDictionary1(value.heroes);
		writer.WriteDictionary2(value.monsters);
		writer.WriteDictionary3(value.zones);
		writer.WriteDictionary4(value.skills);
		writer.WriteDictionary4(value.gems);
		_Write_System_002ECollections_002EGeneric_002EList_00601_003CSystem_002EString_003E(writer, value.recoveredLossPoints);
	}

	public static void _Write_DewProfileStats_002FHeroData(NetworkWriter writer, DewProfileStats.HeroData value)
	{
		if (value == null)
		{
			NetworkWriterExtensions.WriteBool(writer, false);
			return;
		}
		NetworkWriterExtensions.WriteBool(writer, true);
		NetworkWriterExtensions.WriteInt(writer, value.masteryLevel);
		NetworkWriterExtensions.WriteInt(writer, value.completedLimboDepth);
		NetworkWriterExtensions.WriteLong(writer, value.currentMasteryPoints);
		NetworkWriterExtensions.WriteLong(writer, value.totalMasteryPoints);
		NetworkWriterExtensions.WriteBool(writer, value.didUnlockPolarisEnding);
		NetworkWriterExtensions.WriteLong(writer, value.pureWhiteDreams);
		NetworkWriterExtensions.WriteLong(writer, value.pureWhiteDreamsNightmare);
		NetworkWriterExtensions.WriteLong(writer, value.unknownFates);
		NetworkWriterExtensions.WriteLong(writer, value.unknownFatesNightmare);
		NetworkWriterExtensions.WriteLong(writer, value.starlessPaths);
		NetworkWriterExtensions.WriteLong(writer, value.starlessPathsNightmare);
		NetworkWriterExtensions.WriteLong(writer, value.wins);
		NetworkWriterExtensions.WriteLong(writer, value.winsNightmare);
		NetworkWriterExtensions.WriteLong(writer, value.loses);
		NetworkWriterExtensions.WriteLong(writer, value.playCount);
		NetworkWriterExtensions.WriteLong(writer, value.levelUps);
		NetworkWriterExtensions.WriteLong(writer, value.kills);
		NetworkWriterExtensions.WriteLong(writer, value.hunterKills);
		NetworkWriterExtensions.WriteLong(writer, value.heroicBossKills);
		NetworkWriterExtensions.WriteLong(writer, value.miniBossKills);
		NetworkWriterExtensions.WriteDouble(writer, value.playTimeMinutes);
		NetworkWriterExtensions.WriteLong(writer, value.deaths);
		NetworkWriterExtensions.WriteLong(writer, value.visitedLocations);
		NetworkWriterExtensions.WriteLong(writer, value.visitedHunterLocations);
		NetworkWriterExtensions.WriteLong(writer, value.visitedWorlds);
		NetworkWriterExtensions.WriteLong(writer, value.upgradeCount);
		NetworkWriterExtensions.WriteLong(writer, value.dismantleCount);
		NetworkWriterExtensions.WriteLong(writer, value.buyCount);
		NetworkWriterExtensions.WriteLong(writer, value.sellCount);
		NetworkWriterExtensions.WriteDouble(writer, value.spentGold);
		NetworkWriterExtensions.WriteDouble(writer, value.spentDreamDust);
		NetworkWriterExtensions.WriteLong(writer, value.chaosCount);
		NetworkWriterExtensions.WriteDouble(writer, value.damageDealt);
		NetworkWriterExtensions.WriteDouble(writer, value.damageTaken);
		NetworkWriterExtensions.WriteDouble(writer, value.healToSelf);
		NetworkWriterExtensions.WriteDouble(writer, value.healToOthers);
		NetworkWriterExtensions.WriteDouble(writer, value.earnedGold);
		NetworkWriterExtensions.WriteDouble(writer, value.earnedDreamDust);
		NetworkWriterExtensions.WriteLong(writer, value.maxVisitedWorlds);
		NetworkWriterExtensions.WriteDouble(writer, value.maxElapsedGameTimeSeconds);
		NetworkWriterExtensions.WriteDouble(writer, value.maxTotalDamage);
		NetworkWriterExtensions.WriteDouble(writer, value.maxSingleTargetDamage);
		NetworkWriterExtensions.WriteDouble(writer, value.maxEarnedGold);
		NetworkWriterExtensions.WriteDouble(writer, value.maxEarnedDreamDust);
		NetworkWriterExtensions.WriteDouble(writer, value.maxEarnedStardust);
	}

	public static DewProfileStats _Read_DewProfileStats(NetworkReader reader)
	{
		if (!NetworkReaderExtensions.ReadBool(reader))
		{
			return null;
		}
		DewProfileStats dewProfileStats = new DewProfileStats();
		dewProfileStats.total = _Read_DewProfileStats_002FHeroData(reader);
		dewProfileStats.heroes = reader.ReadDictionary1();
		dewProfileStats.monsters = reader.ReadDictionary2();
		dewProfileStats.zones = reader.ReadDictionary3();
		dewProfileStats.skills = reader.ReadDictionary4();
		dewProfileStats.gems = reader.ReadDictionary4();
		dewProfileStats.recoveredLossPoints = _Read_System_002ECollections_002EGeneric_002EList_00601_003CSystem_002EString_003E(reader);
		return dewProfileStats;
	}

	public static DewProfileStats.HeroData _Read_DewProfileStats_002FHeroData(NetworkReader reader)
	{
		if (!NetworkReaderExtensions.ReadBool(reader))
		{
			return null;
		}
		DewProfileStats.HeroData heroData = new DewProfileStats.HeroData();
		heroData.masteryLevel = NetworkReaderExtensions.ReadInt(reader);
		heroData.completedLimboDepth = NetworkReaderExtensions.ReadInt(reader);
		heroData.currentMasteryPoints = NetworkReaderExtensions.ReadLong(reader);
		heroData.totalMasteryPoints = NetworkReaderExtensions.ReadLong(reader);
		heroData.didUnlockPolarisEnding = NetworkReaderExtensions.ReadBool(reader);
		heroData.pureWhiteDreams = NetworkReaderExtensions.ReadLong(reader);
		heroData.pureWhiteDreamsNightmare = NetworkReaderExtensions.ReadLong(reader);
		heroData.unknownFates = NetworkReaderExtensions.ReadLong(reader);
		heroData.unknownFatesNightmare = NetworkReaderExtensions.ReadLong(reader);
		heroData.starlessPaths = NetworkReaderExtensions.ReadLong(reader);
		heroData.starlessPathsNightmare = NetworkReaderExtensions.ReadLong(reader);
		heroData.wins = NetworkReaderExtensions.ReadLong(reader);
		heroData.winsNightmare = NetworkReaderExtensions.ReadLong(reader);
		heroData.loses = NetworkReaderExtensions.ReadLong(reader);
		heroData.playCount = NetworkReaderExtensions.ReadLong(reader);
		heroData.levelUps = NetworkReaderExtensions.ReadLong(reader);
		heroData.kills = NetworkReaderExtensions.ReadLong(reader);
		heroData.hunterKills = NetworkReaderExtensions.ReadLong(reader);
		heroData.heroicBossKills = NetworkReaderExtensions.ReadLong(reader);
		heroData.miniBossKills = NetworkReaderExtensions.ReadLong(reader);
		heroData.playTimeMinutes = NetworkReaderExtensions.ReadDouble(reader);
		heroData.deaths = NetworkReaderExtensions.ReadLong(reader);
		heroData.visitedLocations = NetworkReaderExtensions.ReadLong(reader);
		heroData.visitedHunterLocations = NetworkReaderExtensions.ReadLong(reader);
		heroData.visitedWorlds = NetworkReaderExtensions.ReadLong(reader);
		heroData.upgradeCount = NetworkReaderExtensions.ReadLong(reader);
		heroData.dismantleCount = NetworkReaderExtensions.ReadLong(reader);
		heroData.buyCount = NetworkReaderExtensions.ReadLong(reader);
		heroData.sellCount = NetworkReaderExtensions.ReadLong(reader);
		heroData.spentGold = NetworkReaderExtensions.ReadDouble(reader);
		heroData.spentDreamDust = NetworkReaderExtensions.ReadDouble(reader);
		heroData.chaosCount = NetworkReaderExtensions.ReadLong(reader);
		heroData.damageDealt = NetworkReaderExtensions.ReadDouble(reader);
		heroData.damageTaken = NetworkReaderExtensions.ReadDouble(reader);
		heroData.healToSelf = NetworkReaderExtensions.ReadDouble(reader);
		heroData.healToOthers = NetworkReaderExtensions.ReadDouble(reader);
		heroData.earnedGold = NetworkReaderExtensions.ReadDouble(reader);
		heroData.earnedDreamDust = NetworkReaderExtensions.ReadDouble(reader);
		heroData.maxVisitedWorlds = NetworkReaderExtensions.ReadLong(reader);
		heroData.maxElapsedGameTimeSeconds = NetworkReaderExtensions.ReadDouble(reader);
		heroData.maxTotalDamage = NetworkReaderExtensions.ReadDouble(reader);
		heroData.maxSingleTargetDamage = NetworkReaderExtensions.ReadDouble(reader);
		heroData.maxEarnedGold = NetworkReaderExtensions.ReadDouble(reader);
		heroData.maxEarnedDreamDust = NetworkReaderExtensions.ReadDouble(reader);
		heroData.maxEarnedStardust = NetworkReaderExtensions.ReadDouble(reader);
		return heroData;
	}

	public static void _Write_DewProfileStats_002FItemData(NetworkWriter writer, DewProfileStats.ItemData value)
	{
		if (value == null)
		{
			NetworkWriterExtensions.WriteBool(writer, false);
			return;
		}
		NetworkWriterExtensions.WriteBool(writer, true);
		NetworkWriterExtensions.WriteLong(writer, value.wins);
		NetworkWriterExtensions.WriteLong(writer, value.loses);
		NetworkWriterExtensions.WriteLong(writer, value.playCount);
		NetworkWriterExtensions.WriteLong(writer, value.upgradeCount);
		NetworkWriterExtensions.WriteLong(writer, value.dismantleCount);
		NetworkWriterExtensions.WriteLong(writer, value.buyCount);
		NetworkWriterExtensions.WriteLong(writer, value.sellCount);
	}

	public static void _Write_DewProfileStats_002FMonsterData(NetworkWriter writer, DewProfileStats.MonsterData value)
	{
		if (value == null)
		{
			NetworkWriterExtensions.WriteBool(writer, false);
			return;
		}
		NetworkWriterExtensions.WriteBool(writer, true);
		NetworkWriterExtensions.WriteLong(writer, value.kills);
		NetworkWriterExtensions.WriteLong(writer, value.nightmareKills);
		NetworkWriterExtensions.WriteLong(writer, value.deaths);
	}

	public static void _Write_DewProfileStats_002FZoneData(NetworkWriter writer, DewProfileStats.ZoneData value)
	{
		if (value == null)
		{
			NetworkWriterExtensions.WriteBool(writer, false);
			return;
		}
		NetworkWriterExtensions.WriteBool(writer, true);
		NetworkWriterExtensions.WriteLong(writer, value.kills);
		NetworkWriterExtensions.WriteLong(writer, value.hunterKills);
		NetworkWriterExtensions.WriteLong(writer, value.heroicBossKills);
		NetworkWriterExtensions.WriteLong(writer, value.miniBossKills);
		NetworkWriterExtensions.WriteLong(writer, value.deaths);
		NetworkWriterExtensions.WriteDouble(writer, value.playTimeMinutes);
		NetworkWriterExtensions.WriteLong(writer, value.visited);
		NetworkWriterExtensions.WriteLong(writer, value.visitedLocations);
		NetworkWriterExtensions.WriteLong(writer, value.visitedHunterLocations);
	}

	public static DewProfileStats.ItemData _Read_DewProfileStats_002FItemData(NetworkReader reader)
	{
		if (!NetworkReaderExtensions.ReadBool(reader))
		{
			return null;
		}
		DewProfileStats.ItemData itemData = new DewProfileStats.ItemData();
		itemData.wins = NetworkReaderExtensions.ReadLong(reader);
		itemData.loses = NetworkReaderExtensions.ReadLong(reader);
		itemData.playCount = NetworkReaderExtensions.ReadLong(reader);
		itemData.upgradeCount = NetworkReaderExtensions.ReadLong(reader);
		itemData.dismantleCount = NetworkReaderExtensions.ReadLong(reader);
		itemData.buyCount = NetworkReaderExtensions.ReadLong(reader);
		itemData.sellCount = NetworkReaderExtensions.ReadLong(reader);
		return itemData;
	}

	public static DewProfileStats.MonsterData _Read_DewProfileStats_002FMonsterData(NetworkReader reader)
	{
		if (!NetworkReaderExtensions.ReadBool(reader))
		{
			return null;
		}
		DewProfileStats.MonsterData monsterData = new DewProfileStats.MonsterData();
		monsterData.kills = NetworkReaderExtensions.ReadLong(reader);
		monsterData.nightmareKills = NetworkReaderExtensions.ReadLong(reader);
		monsterData.deaths = NetworkReaderExtensions.ReadLong(reader);
		return monsterData;
	}

	public static DewProfileStats.ZoneData _Read_DewProfileStats_002FZoneData(NetworkReader reader)
	{
		if (!NetworkReaderExtensions.ReadBool(reader))
		{
			return null;
		}
		DewProfileStats.ZoneData zoneData = new DewProfileStats.ZoneData();
		zoneData.kills = NetworkReaderExtensions.ReadLong(reader);
		zoneData.hunterKills = NetworkReaderExtensions.ReadLong(reader);
		zoneData.heroicBossKills = NetworkReaderExtensions.ReadLong(reader);
		zoneData.miniBossKills = NetworkReaderExtensions.ReadLong(reader);
		zoneData.deaths = NetworkReaderExtensions.ReadLong(reader);
		zoneData.playTimeMinutes = NetworkReaderExtensions.ReadDouble(reader);
		zoneData.visited = NetworkReaderExtensions.ReadLong(reader);
		zoneData.visitedLocations = NetworkReaderExtensions.ReadLong(reader);
		zoneData.visitedHunterLocations = NetworkReaderExtensions.ReadLong(reader);
		return zoneData;
	}

	public static void _Write_PlayerState(NetworkWriter writer, PlayerState value)
	{
		NetworkWriterExtensions.WriteInt(writer, (int)value);
	}

	public static void _Write_DewPlayer_002FRole(NetworkWriter writer, DewPlayer.Role value)
	{
		NetworkWriterExtensions.WriteByte(writer, (byte)value);
	}

	public static void _Write_Steamworks_002ECSteamID(NetworkWriter writer, CSteamID value)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		NetworkWriterExtensions.WriteULong(writer, value.m_SteamID);
	}

	public static PlayerState _Read_PlayerState(NetworkReader reader)
	{
		return (PlayerState)NetworkReaderExtensions.ReadInt(reader);
	}

	public static DewPlayer.Role _Read_DewPlayer_002FRole(NetworkReader reader)
	{
		return (DewPlayer.Role)NetworkReaderExtensions.ReadByte(reader);
	}

	public static CSteamID _Read_Steamworks_002ECSteamID(NetworkReader reader)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		return new CSteamID
		{
			m_SteamID = NetworkReaderExtensions.ReadULong(reader)
		};
	}

	public static void _Write_HatredStrengthType(NetworkWriter writer, HatredStrengthType value)
	{
		NetworkWriterExtensions.WriteInt(writer, (int)value);
	}

	public static HatredStrengthType _Read_HatredStrengthType(NetworkReader reader)
	{
		return (HatredStrengthType)NetworkReaderExtensions.ReadInt(reader);
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

	public static SyncedNetworkBehaviour _Read_SyncedNetworkBehaviour(NetworkReader reader)
	{
		return new SyncedNetworkBehaviour
		{
			netId = NetworkReaderExtensions.ReadUInt(reader),
			componentIndex = NetworkReaderExtensions.ReadByte(reader)
		};
	}

	public static void _Write_SyncedNetworkBehaviour(NetworkWriter writer, SyncedNetworkBehaviour value)
	{
		NetworkWriterExtensions.WriteUInt(writer, value.netId);
		NetworkWriterExtensions.WriteByte(writer, value.componentIndex);
	}

	public static void _Write_EntityAnimation_002FReplaceableAnimationType(NetworkWriter writer, EntityAnimation.ReplaceableAnimationType value)
	{
		NetworkWriterExtensions.WriteInt(writer, (int)value);
	}

	public static EntityAnimation.ReplaceableAnimationType _Read_EntityAnimation_002FReplaceableAnimationType(NetworkReader reader)
	{
		return (EntityAnimation.ReplaceableAnimationType)NetworkReaderExtensions.ReadInt(reader);
	}

	public static void _Write_EntityControl_002FBlockableAction(NetworkWriter writer, EntityControl.BlockableAction value)
	{
		NetworkWriterExtensions.WriteInt(writer, (int)value);
	}

	public static EntityControl.BlockableAction _Read_EntityControl_002FBlockableAction(NetworkReader reader)
	{
		return (EntityControl.BlockableAction)NetworkReaderExtensions.ReadInt(reader);
	}

	public static void _Write_EntityControl_002FPositionSyncData(NetworkWriter writer, EntityControl.PositionSyncData value)
	{
		NetworkWriterExtensions.WriteDouble(writer, value.timestamp);
		NetworkWriterExtensions.WriteVector3(writer, value.position);
		NetworkWriterExtensions.WriteVector3(writer, value.velocity);
		NetworkWriterExtensions.WriteFloat(writer, value.desiredAngle);
	}

	public static EntityControl.PositionSyncData _Read_EntityControl_002FPositionSyncData(NetworkReader reader)
	{
		return new EntityControl.PositionSyncData
		{
			timestamp = NetworkReaderExtensions.ReadDouble(reader),
			position = NetworkReaderExtensions.ReadVector3(reader),
			velocity = NetworkReaderExtensions.ReadVector3(reader),
			desiredAngle = NetworkReaderExtensions.ReadFloat(reader)
		};
	}

	public static void _Write_BaseStats(NetworkWriter writer, BaseStats value)
	{
		NetworkWriterExtensions.WriteFloat(writer, value.attackDamage);
		NetworkWriterExtensions.WriteFloat(writer, value.abilityPower);
		NetworkWriterExtensions.WriteFloat(writer, value.maxHealth);
		NetworkWriterExtensions.WriteFloat(writer, value.maxMana);
		NetworkWriterExtensions.WriteFloat(writer, value.healthRegen);
		NetworkWriterExtensions.WriteFloat(writer, value.manaRegen);
		NetworkWriterExtensions.WriteFloat(writer, value.critAmp);
		NetworkWriterExtensions.WriteFloat(writer, value.critChance);
		NetworkWriterExtensions.WriteFloat(writer, value.abilityHaste);
		NetworkWriterExtensions.WriteFloat(writer, value.tenacity);
		NetworkWriterExtensions.WriteFloat(writer, value.fireEffectAmp);
		NetworkWriterExtensions.WriteFloat(writer, value.coldEffectAmp);
		NetworkWriterExtensions.WriteFloat(writer, value.lightEffectAmp);
		NetworkWriterExtensions.WriteFloat(writer, value.darkEffectAmp);
		NetworkWriterExtensions.WriteFloat(writer, value.armor);
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

	public static void _Write_FinalStats(NetworkWriter writer, FinalStats value)
	{
		NetworkWriterExtensions.WriteFloat(writer, value.attackDamage);
		NetworkWriterExtensions.WriteFloat(writer, value.abilityPower);
		NetworkWriterExtensions.WriteFloat(writer, value.maxHealth);
		NetworkWriterExtensions.WriteFloat(writer, value.maxHealthWithoutBonus);
		NetworkWriterExtensions.WriteFloat(writer, value.maxMana);
		NetworkWriterExtensions.WriteFloat(writer, value.healthRegen);
		NetworkWriterExtensions.WriteFloat(writer, value.manaRegen);
		NetworkWriterExtensions.WriteFloat(writer, value.attackSpeedMultiplier);
		NetworkWriterExtensions.WriteFloat(writer, value.critAmp);
		NetworkWriterExtensions.WriteFloat(writer, value.critChance);
		NetworkWriterExtensions.WriteFloat(writer, value.tenacity);
		NetworkWriterExtensions.WriteFloat(writer, value.abilityHaste);
		NetworkWriterExtensions.WriteFloat(writer, value.movementSpeedMultiplier);
		NetworkWriterExtensions.WriteFloat(writer, value.fireEffectAmp);
		NetworkWriterExtensions.WriteFloat(writer, value.coldEffectAmp);
		NetworkWriterExtensions.WriteFloat(writer, value.lightEffectAmp);
		NetworkWriterExtensions.WriteFloat(writer, value.darkEffectAmp);
		NetworkWriterExtensions.WriteFloat(writer, value.armor);
	}

	public static void _Write_BasicEffectMask(NetworkWriter writer, BasicEffectMask value)
	{
		NetworkWriterExtensions.WriteUInt(writer, (uint)value);
	}

	public static BaseStats _Read_BaseStats(NetworkReader reader)
	{
		return new BaseStats
		{
			attackDamage = NetworkReaderExtensions.ReadFloat(reader),
			abilityPower = NetworkReaderExtensions.ReadFloat(reader),
			maxHealth = NetworkReaderExtensions.ReadFloat(reader),
			maxMana = NetworkReaderExtensions.ReadFloat(reader),
			healthRegen = NetworkReaderExtensions.ReadFloat(reader),
			manaRegen = NetworkReaderExtensions.ReadFloat(reader),
			critAmp = NetworkReaderExtensions.ReadFloat(reader),
			critChance = NetworkReaderExtensions.ReadFloat(reader),
			abilityHaste = NetworkReaderExtensions.ReadFloat(reader),
			tenacity = NetworkReaderExtensions.ReadFloat(reader),
			fireEffectAmp = NetworkReaderExtensions.ReadFloat(reader),
			coldEffectAmp = NetworkReaderExtensions.ReadFloat(reader),
			lightEffectAmp = NetworkReaderExtensions.ReadFloat(reader),
			darkEffectAmp = NetworkReaderExtensions.ReadFloat(reader),
			armor = NetworkReaderExtensions.ReadFloat(reader)
		};
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

	public static FinalStats _Read_FinalStats(NetworkReader reader)
	{
		return new FinalStats
		{
			attackDamage = NetworkReaderExtensions.ReadFloat(reader),
			abilityPower = NetworkReaderExtensions.ReadFloat(reader),
			maxHealth = NetworkReaderExtensions.ReadFloat(reader),
			maxHealthWithoutBonus = NetworkReaderExtensions.ReadFloat(reader),
			maxMana = NetworkReaderExtensions.ReadFloat(reader),
			healthRegen = NetworkReaderExtensions.ReadFloat(reader),
			manaRegen = NetworkReaderExtensions.ReadFloat(reader),
			attackSpeedMultiplier = NetworkReaderExtensions.ReadFloat(reader),
			critAmp = NetworkReaderExtensions.ReadFloat(reader),
			critChance = NetworkReaderExtensions.ReadFloat(reader),
			tenacity = NetworkReaderExtensions.ReadFloat(reader),
			abilityHaste = NetworkReaderExtensions.ReadFloat(reader),
			movementSpeedMultiplier = NetworkReaderExtensions.ReadFloat(reader),
			fireEffectAmp = NetworkReaderExtensions.ReadFloat(reader),
			coldEffectAmp = NetworkReaderExtensions.ReadFloat(reader),
			lightEffectAmp = NetworkReaderExtensions.ReadFloat(reader),
			darkEffectAmp = NetworkReaderExtensions.ReadFloat(reader),
			armor = NetworkReaderExtensions.ReadFloat(reader)
		};
	}

	public static BasicEffectMask _Read_BasicEffectMask(NetworkReader reader)
	{
		return (BasicEffectMask)NetworkReaderExtensions.ReadUInt(reader);
	}

	public static void _Write_GibInfo(NetworkWriter writer, GibInfo value)
	{
		NetworkWriterExtensions.WriteVector3(writer, value.velocity);
		NetworkWriterExtensions.WriteVector3(writer, value.normalizedCurrentDamage);
		NetworkWriterExtensions.WriteFloat(writer, value.yVelocity);
	}

	public static GibInfo _Read_GibInfo(NetworkReader reader)
	{
		return new GibInfo
		{
			velocity = NetworkReaderExtensions.ReadVector3(reader),
			normalizedCurrentDamage = NetworkReaderExtensions.ReadVector3(reader),
			yVelocity = NetworkReaderExtensions.ReadFloat(reader)
		};
	}

	public static void _Write_KnockUpStrength(NetworkWriter writer, KnockUpStrength value)
	{
		NetworkWriterExtensions.WriteInt(writer, (int)value);
	}

	public static KnockUpStrength _Read_KnockUpStrength(NetworkReader reader)
	{
		return (KnockUpStrength)NetworkReaderExtensions.ReadInt(reader);
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

	public static void _Write_System_002ECollections_002EGeneric_002EList_00601_003CSystem_002EUInt32_003E(NetworkWriter writer, List<uint> value)
	{
		NetworkWriterExtensions.WriteList<uint>(writer, value);
	}

	public static List<uint> _Read_System_002ECollections_002EGeneric_002EList_00601_003CSystem_002EUInt32_003E(NetworkReader reader)
	{
		return NetworkReaderExtensions.ReadList<uint>(reader);
	}

	public static void _Write_ChatManager_002FMessage(NetworkWriter writer, ChatManager.Message value)
	{
		_Write_ChatManager_002FMessageType(writer, value.type);
		NetworkWriterExtensions.WriteString(writer, value.content);
		_Write_System_002EString_005B_005D(writer, value.args);
		NetworkWriterExtensions.WriteString(writer, value.itemType);
		NetworkWriterExtensions.WriteInt(writer, value.itemLevel);
		_Write_Cost(writer, value.itemPrice);
		NetworkWriterExtensions.WriteString(writer, value.itemCustomData);
	}

	public static void _Write_ChatManager_002FMessageType(NetworkWriter writer, ChatManager.MessageType value)
	{
		NetworkWriterExtensions.WriteByte(writer, (byte)value);
	}

	public static ChatManager.Message _Read_ChatManager_002FMessage(NetworkReader reader)
	{
		return new ChatManager.Message
		{
			type = _Read_ChatManager_002FMessageType(reader),
			content = NetworkReaderExtensions.ReadString(reader),
			args = _Read_System_002EString_005B_005D(reader),
			itemType = NetworkReaderExtensions.ReadString(reader),
			itemLevel = NetworkReaderExtensions.ReadInt(reader),
			itemPrice = _Read_Cost(reader),
			itemCustomData = NetworkReaderExtensions.ReadString(reader)
		};
	}

	public static ChatManager.MessageType _Read_ChatManager_002FMessageType(NetworkReader reader)
	{
		return (ChatManager.MessageType)NetworkReaderExtensions.ReadByte(reader);
	}

	public static void _Write_EventInfoHeal(NetworkWriter writer, EventInfoHeal value)
	{
		NetworkWriterExtensions.WriteNetworkBehaviour(writer, (NetworkBehaviour)(object)value.actor);
		NetworkWriterExtensions.WriteNetworkBehaviour(writer, (NetworkBehaviour)(object)value.target);
		_Write_FinalHealData(writer, value.heal);
		NetworkWriterExtensions.WriteFloat(writer, value.amount);
		NetworkWriterExtensions.WriteFloat(writer, value.discardedAmount);
		NetworkWriterExtensions.WriteBool(writer, value.isCrit);
		NetworkWriterExtensions.WriteBool(writer, value.canMerge);
		_Write_ReactionChain(writer, value.chain);
	}

	public static void _Write_FinalHealData(NetworkWriter writer, FinalHealData value)
	{
		NetworkWriterExtensions.WriteFloat(writer, value.amount);
		NetworkWriterExtensions.WriteFloat(writer, value.discardedAmount);
		NetworkWriterExtensions.WriteBool(writer, value.isCrit);
		_Write_ActorFlags(writer, value._flags);
	}

	public static void _Write_ActorFlags(NetworkWriter writer, ActorFlags value)
	{
	}

	public static void _Write_ReactionChain(NetworkWriter writer, ReactionChain value)
	{
		_Write_System_002ECollections_002EGeneric_002EList_00601_003CActor_003E(writer, value._actors);
	}

	public static void _Write_System_002ECollections_002EGeneric_002EList_00601_003CActor_003E(NetworkWriter writer, List<Actor> value)
	{
		NetworkWriterExtensions.WriteList<Actor>(writer, value);
	}

	public static EventInfoHeal _Read_EventInfoHeal(NetworkReader reader)
	{
		return new EventInfoHeal
		{
			actor = NetworkReaderExtensions.ReadNetworkBehaviour<Actor>(reader),
			target = NetworkReaderExtensions.ReadNetworkBehaviour<Entity>(reader),
			heal = _Read_FinalHealData(reader),
			amount = NetworkReaderExtensions.ReadFloat(reader),
			discardedAmount = NetworkReaderExtensions.ReadFloat(reader),
			isCrit = NetworkReaderExtensions.ReadBool(reader),
			canMerge = NetworkReaderExtensions.ReadBool(reader),
			chain = _Read_ReactionChain(reader)
		};
	}

	public static FinalHealData _Read_FinalHealData(NetworkReader reader)
	{
		return new FinalHealData
		{
			amount = NetworkReaderExtensions.ReadFloat(reader),
			discardedAmount = NetworkReaderExtensions.ReadFloat(reader),
			isCrit = NetworkReaderExtensions.ReadBool(reader),
			_flags = _Read_ActorFlags(reader)
		};
	}

	public static ActorFlags _Read_ActorFlags(NetworkReader reader)
	{
		return default;
	}

	public static ReactionChain _Read_ReactionChain(NetworkReader reader)
	{
		return new ReactionChain
		{
			_actors = _Read_System_002ECollections_002EGeneric_002EList_00601_003CActor_003E(reader)
		};
	}

	public static List<Actor> _Read_System_002ECollections_002EGeneric_002EList_00601_003CActor_003E(NetworkReader reader)
	{
		return NetworkReaderExtensions.ReadList<Actor>(reader);
	}

	public static void _Write_EventInfoDamage(NetworkWriter writer, EventInfoDamage value)
	{
		NetworkWriterExtensions.WriteNetworkBehaviour(writer, (NetworkBehaviour)(object)value.actor);
		NetworkWriterExtensions.WriteNetworkBehaviour(writer, (NetworkBehaviour)(object)value.victim);
		_Write_FinalDamageData(writer, value.damage);
		NetworkWriterExtensions.WriteFloat(writer, value.negatedAmountByShield);
	}

	public static void _Write_FinalDamageData(NetworkWriter writer, FinalDamageData value)
	{
		NetworkWriterExtensions.WriteFloat(writer, value.amount);
		NetworkWriterExtensions.WriteFloat(writer, value.discardedAmount);
		NetworkWriterExtensions.WriteFloat(writer, value.procCoefficient);
		_Write_AttackEffectType(writer, value.attackEffectType);
		NetworkWriterExtensions.WriteFloat(writer, value.attackEffectStrength);
		writer.WriteNullableElementalType(value.elemental);
		NetworkWriterExtensions.WriteVector3Nullable(writer, value.direction);
		_Write_DamageData_002FSourceType(writer, value.type);
		_Write_DamageAttribute(writer, value.attributes);
		NetworkWriterExtensions.WriteIntNullable(writer, value.overrideElementalStacks);
		_Write_ActorFlags(writer, value._modifyFlags);
	}

	public static void _Write_AttackEffectType(NetworkWriter writer, AttackEffectType value)
	{
		NetworkWriterExtensions.WriteInt(writer, (int)value);
	}

	public static void _Write_DamageData_002FSourceType(NetworkWriter writer, DamageData.SourceType value)
	{
		NetworkWriterExtensions.WriteInt(writer, (int)value);
	}

	public static void _Write_DamageAttribute(NetworkWriter writer, DamageAttribute value)
	{
		NetworkWriterExtensions.WriteLong(writer, (long)value);
	}

	public static EventInfoDamage _Read_EventInfoDamage(NetworkReader reader)
	{
		return new EventInfoDamage
		{
			actor = NetworkReaderExtensions.ReadNetworkBehaviour<Actor>(reader),
			victim = NetworkReaderExtensions.ReadNetworkBehaviour<Entity>(reader),
			damage = _Read_FinalDamageData(reader),
			negatedAmountByShield = NetworkReaderExtensions.ReadFloat(reader)
		};
	}

	public static FinalDamageData _Read_FinalDamageData(NetworkReader reader)
	{
		return new FinalDamageData
		{
			amount = NetworkReaderExtensions.ReadFloat(reader),
			discardedAmount = NetworkReaderExtensions.ReadFloat(reader),
			procCoefficient = NetworkReaderExtensions.ReadFloat(reader),
			attackEffectType = _Read_AttackEffectType(reader),
			attackEffectStrength = NetworkReaderExtensions.ReadFloat(reader),
			elemental = reader.ReadNullableElementalType(),
			direction = NetworkReaderExtensions.ReadVector3Nullable(reader),
			type = _Read_DamageData_002FSourceType(reader),
			attributes = _Read_DamageAttribute(reader),
			overrideElementalStacks = NetworkReaderExtensions.ReadIntNullable(reader),
			_modifyFlags = _Read_ActorFlags(reader)
		};
	}

	public static AttackEffectType _Read_AttackEffectType(NetworkReader reader)
	{
		return (AttackEffectType)NetworkReaderExtensions.ReadInt(reader);
	}

	public static DamageData.SourceType _Read_DamageData_002FSourceType(NetworkReader reader)
	{
		return (DamageData.SourceType)NetworkReaderExtensions.ReadInt(reader);
	}

	public static DamageAttribute _Read_DamageAttribute(NetworkReader reader)
	{
		return (DamageAttribute)NetworkReaderExtensions.ReadLong(reader);
	}

	public static void _Write_EventInfoShield(NetworkWriter writer, EventInfoShield value)
	{
		NetworkWriterExtensions.WriteNetworkBehaviour(writer, (NetworkBehaviour)(object)value.statusEffect);
		NetworkWriterExtensions.WriteNetworkBehaviour(writer, (NetworkBehaviour)(object)value.target);
		NetworkWriterExtensions.WriteFloat(writer, value.finalAmount);
		NetworkWriterExtensions.WriteFloat(writer, value.originalAmount);
		NetworkWriterExtensions.WriteFloat(writer, value.discardedAmount);
		_Write_ReactionChain(writer, value.chain);
	}

	public static EventInfoShield _Read_EventInfoShield(NetworkReader reader)
	{
		return new EventInfoShield
		{
			statusEffect = NetworkReaderExtensions.ReadNetworkBehaviour<StatusEffect>(reader),
			target = NetworkReaderExtensions.ReadNetworkBehaviour<Entity>(reader),
			finalAmount = NetworkReaderExtensions.ReadFloat(reader),
			originalAmount = NetworkReaderExtensions.ReadFloat(reader),
			discardedAmount = NetworkReaderExtensions.ReadFloat(reader),
			chain = _Read_ReactionChain(reader)
		};
	}

	public static void _Write_EventInfoSpentMana(NetworkWriter writer, EventInfoSpentMana value)
	{
		NetworkWriterExtensions.WriteNetworkBehaviour(writer, (NetworkBehaviour)(object)value.actor);
		NetworkWriterExtensions.WriteNetworkBehaviour(writer, (NetworkBehaviour)(object)value.entity);
		NetworkWriterExtensions.WriteFloat(writer, value.amount);
	}

	public static EventInfoSpentMana _Read_EventInfoSpentMana(NetworkReader reader)
	{
		return new EventInfoSpentMana
		{
			actor = NetworkReaderExtensions.ReadNetworkBehaviour<Actor>(reader),
			entity = NetworkReaderExtensions.ReadNetworkBehaviour<Entity>(reader),
			amount = NetworkReaderExtensions.ReadFloat(reader)
		};
	}

	public static void _Write_EventInfoAttackMissed(NetworkWriter writer, EventInfoAttackMissed value)
	{
		NetworkWriterExtensions.WriteNetworkBehaviour(writer, (NetworkBehaviour)(object)value.actor);
		NetworkWriterExtensions.WriteNetworkBehaviour(writer, (NetworkBehaviour)(object)value.attacker);
		NetworkWriterExtensions.WriteNetworkBehaviour(writer, (NetworkBehaviour)(object)value.victim);
		NetworkWriterExtensions.WriteBool(writer, value.isCrit);
	}

	public static EventInfoAttackMissed _Read_EventInfoAttackMissed(NetworkReader reader)
	{
		return new EventInfoAttackMissed
		{
			actor = NetworkReaderExtensions.ReadNetworkBehaviour<Actor>(reader),
			attacker = NetworkReaderExtensions.ReadNetworkBehaviour<Entity>(reader),
			victim = NetworkReaderExtensions.ReadNetworkBehaviour<Entity>(reader),
			isCrit = NetworkReaderExtensions.ReadBool(reader)
		};
	}

	public static void _Write_EventInfoDamageNegatedByImmunity(NetworkWriter writer, EventInfoDamageNegatedByImmunity value)
	{
		NetworkWriterExtensions.WriteNetworkBehaviour(writer, (NetworkBehaviour)(object)value.actor);
		writer.WriteBasicEffect(value.effect);
		NetworkWriterExtensions.WriteNetworkBehaviour(writer, (NetworkBehaviour)(object)value.victim);
		_Write_FinalDamageData(writer, value.data);
	}

	public static EventInfoDamageNegatedByImmunity _Read_EventInfoDamageNegatedByImmunity(NetworkReader reader)
	{
		return new EventInfoDamageNegatedByImmunity
		{
			actor = NetworkReaderExtensions.ReadNetworkBehaviour<Actor>(reader),
			effect = reader.ReadBasicEffect(),
			victim = NetworkReaderExtensions.ReadNetworkBehaviour<Entity>(reader),
			data = _Read_FinalDamageData(reader)
		};
	}

	public static void _Write_EventInfoDamageNegatedByShield(NetworkWriter writer, EventInfoDamageNegatedByShield value)
	{
		NetworkWriterExtensions.WriteNetworkBehaviour(writer, (NetworkBehaviour)(object)value.actor);
		NetworkWriterExtensions.WriteNetworkBehaviour(writer, (NetworkBehaviour)(object)value.victim);
		NetworkWriterExtensions.WriteFloat(writer, value.negatedAmount);
		_Write_FinalDamageData(writer, value.damage);
	}

	public static EventInfoDamageNegatedByShield _Read_EventInfoDamageNegatedByShield(NetworkReader reader)
	{
		return new EventInfoDamageNegatedByShield
		{
			actor = NetworkReaderExtensions.ReadNetworkBehaviour<Actor>(reader),
			victim = NetworkReaderExtensions.ReadNetworkBehaviour<Entity>(reader),
			negatedAmount = NetworkReaderExtensions.ReadFloat(reader),
			damage = _Read_FinalDamageData(reader)
		};
	}

	public static void _Write_EventInfoAttackHit(NetworkWriter writer, EventInfoAttackHit value)
	{
		NetworkWriterExtensions.WriteNetworkBehaviour(writer, (NetworkBehaviour)(object)value.actor);
		NetworkWriterExtensions.WriteNetworkBehaviour(writer, (NetworkBehaviour)(object)value.attacker);
		NetworkWriterExtensions.WriteNetworkBehaviour(writer, (NetworkBehaviour)(object)value.victim);
		NetworkWriterExtensions.WriteBool(writer, value.isCrit);
		NetworkWriterExtensions.WriteFloat(writer, value.strength);
	}

	public static EventInfoAttackHit _Read_EventInfoAttackHit(NetworkReader reader)
	{
		return new EventInfoAttackHit
		{
			actor = NetworkReaderExtensions.ReadNetworkBehaviour<Actor>(reader),
			attacker = NetworkReaderExtensions.ReadNetworkBehaviour<Entity>(reader),
			victim = NetworkReaderExtensions.ReadNetworkBehaviour<Entity>(reader),
			isCrit = NetworkReaderExtensions.ReadBool(reader),
			strength = NetworkReaderExtensions.ReadFloat(reader)
		};
	}

	public static void _Write_EventInfoApplyElemental(NetworkWriter writer, EventInfoApplyElemental value)
	{
		NetworkWriterExtensions.WriteNetworkBehaviour(writer, (NetworkBehaviour)(object)value.actor);
		NetworkWriterExtensions.WriteNetworkBehaviour(writer, (NetworkBehaviour)(object)value.victim);
		_Write_ElementalType(writer, value.type);
		NetworkWriterExtensions.WriteInt(writer, value.addedStack);
	}

	public static void _Write_ElementalType(NetworkWriter writer, ElementalType value)
	{
		NetworkWriterExtensions.WriteInt(writer, (int)value);
	}

	public static EventInfoApplyElemental _Read_EventInfoApplyElemental(NetworkReader reader)
	{
		return new EventInfoApplyElemental
		{
			actor = NetworkReaderExtensions.ReadNetworkBehaviour<Actor>(reader),
			victim = NetworkReaderExtensions.ReadNetworkBehaviour<Entity>(reader),
			type = _Read_ElementalType(reader),
			addedStack = NetworkReaderExtensions.ReadInt(reader)
		};
	}

	public static ElementalType _Read_ElementalType(NetworkReader reader)
	{
		return (ElementalType)NetworkReaderExtensions.ReadInt(reader);
	}

	public static void _Write_EventInfoCast(NetworkWriter writer, EventInfoCast value)
	{
		NetworkWriterExtensions.WriteNetworkBehaviour(writer, (NetworkBehaviour)(object)value.instance);
		NetworkWriterExtensions.WriteNetworkBehaviour(writer, (NetworkBehaviour)(object)value.trigger);
		NetworkWriterExtensions.WriteInt(writer, value.configIndex);
		_Write_CastInfo(writer, value.info);
	}

	public static EventInfoCast _Read_EventInfoCast(NetworkReader reader)
	{
		return new EventInfoCast
		{
			instance = NetworkReaderExtensions.ReadNetworkBehaviour<AbilityInstance>(reader),
			trigger = NetworkReaderExtensions.ReadNetworkBehaviour<AbilityTrigger>(reader),
			configIndex = NetworkReaderExtensions.ReadInt(reader),
			info = _Read_CastInfo(reader)
		};
	}

	public static void _Write_System_002EChar_005B_005D(NetworkWriter writer, char[] value)
	{
		NetworkWriterExtensions.WriteArray<char>(writer, value);
	}

	public static char[] _Read_System_002EChar_005B_005D(NetworkReader reader)
	{
		return NetworkReaderExtensions.ReadArray<char>(reader);
	}

	public static void _Write_DewConversationSettings(NetworkWriter writer, DewConversationSettings value)
	{
		if (value == null)
		{
			NetworkWriterExtensions.WriteBool(writer, false);
			return;
		}
		NetworkWriterExtensions.WriteBool(writer, true);
		NetworkWriterExtensions.WriteString(writer, value.startConversationKey);
		NetworkWriterExtensions.WriteNetworkBehaviour(writer, (NetworkBehaviour)(object)value.player);
		_Write_Entity_005B_005D(writer, value.speakers);
		NetworkWriterExtensions.WriteBool(writer, value.rotateTowardsCenter);
		_Write_ConversationVisibility(writer, value.visibility);
		writer.WriteDictionary0(value.variables);
		NetworkWriterExtensions.WriteInt(writer, value._seed);
	}

	public static void _Write_Entity_005B_005D(NetworkWriter writer, Entity[] value)
	{
		NetworkWriterExtensions.WriteArray<Entity>(writer, value);
	}

	public static void _Write_ConversationVisibility(NetworkWriter writer, ConversationVisibility value)
	{
		NetworkWriterExtensions.WriteInt(writer, (int)value);
	}

	public static DewConversationSettings _Read_DewConversationSettings(NetworkReader reader)
	{
		if (!NetworkReaderExtensions.ReadBool(reader))
		{
			return null;
		}
		DewConversationSettings dewConversationSettings = new DewConversationSettings();
		dewConversationSettings.startConversationKey = NetworkReaderExtensions.ReadString(reader);
		dewConversationSettings.player = NetworkReaderExtensions.ReadNetworkBehaviour<DewPlayer>(reader);
		dewConversationSettings.speakers = _Read_Entity_005B_005D(reader);
		dewConversationSettings.rotateTowardsCenter = NetworkReaderExtensions.ReadBool(reader);
		dewConversationSettings.visibility = _Read_ConversationVisibility(reader);
		dewConversationSettings.variables = reader.ReadDictionary0();
		dewConversationSettings._seed = NetworkReaderExtensions.ReadInt(reader);
		return dewConversationSettings;
	}

	public static Entity[] _Read_Entity_005B_005D(NetworkReader reader)
	{
		return NetworkReaderExtensions.ReadArray<Entity>(reader);
	}

	public static ConversationVisibility _Read_ConversationVisibility(NetworkReader reader)
	{
		return (ConversationVisibility)NetworkReaderExtensions.ReadInt(reader);
	}

	public static void _Write_DewGameResult(NetworkWriter writer, DewGameResult value)
	{
		if (value == null)
		{
			NetworkWriterExtensions.WriteBool(writer, false);
			return;
		}
		NetworkWriterExtensions.WriteBool(writer, true);
		NetworkWriterExtensions.WriteString(writer, value.runId);
		_Write_DewGameResult_002FResultType(writer, value.result);
		NetworkWriterExtensions.WriteLong(writer, value.startTimestamp);
		NetworkWriterExtensions.WriteInt(writer, value.elapsedGameTimeSeconds);
		NetworkWriterExtensions.WriteInt(writer, value.visitedWorlds);
		NetworkWriterExtensions.WriteInt(writer, value.visitedLocations);
		NetworkWriterExtensions.WriteString(writer, value.difficulty);
		NetworkWriterExtensions.WriteInt(writer, value.limboDepth);
		_Write_System_002ECollections_002EGeneric_002EList_00601_003CDewGameResult_002FPlayerData_003E(writer, value.players);
	}

	public static void _Write_DewGameResult_002FResultType(NetworkWriter writer, DewGameResult.ResultType value)
	{
		NetworkWriterExtensions.WriteInt(writer, (int)value);
	}

	public static void _Write_System_002ECollections_002EGeneric_002EList_00601_003CDewGameResult_002FPlayerData_003E(NetworkWriter writer, List<DewGameResult.PlayerData> value)
	{
		NetworkWriterExtensions.WriteList<DewGameResult.PlayerData>(writer, value);
	}

	public static void _Write_DewGameResult_002FPlayerData(NetworkWriter writer, DewGameResult.PlayerData value)
	{
		if (value == null)
		{
			NetworkWriterExtensions.WriteBool(writer, false);
			return;
		}
		NetworkWriterExtensions.WriteBool(writer, true);
		NetworkWriterExtensions.WriteBool(writer, value.isLocalPlayer);
		NetworkWriterExtensions.WriteString(writer, value.playerGuid);
		NetworkWriterExtensions.WriteString(writer, value.platform);
		NetworkWriterExtensions.WriteString(writer, value.platformID);
		NetworkWriterExtensions.WriteString(writer, value.playerProfileName);
		NetworkWriterExtensions.WriteString(writer, value.heroType);
		NetworkWriterExtensions.WriteFloat(writer, value.maxHealth);
		NetworkWriterExtensions.WriteFloat(writer, value.attackDamage);
		NetworkWriterExtensions.WriteFloat(writer, value.abilityPower);
		NetworkWriterExtensions.WriteFloat(writer, value.skillHaste);
		NetworkWriterExtensions.WriteFloat(writer, value.attackSpeed);
		NetworkWriterExtensions.WriteFloat(writer, value.fireAmp);
		NetworkWriterExtensions.WriteFloat(writer, value.armor);
		NetworkWriterExtensions.WriteFloat(writer, value.addedHp);
		NetworkWriterExtensions.WriteFloat(writer, value.critChance);
		NetworkWriterExtensions.WriteInt(writer, value.level);
		NetworkWriterExtensions.WriteInt(writer, value.kills);
		NetworkWriterExtensions.WriteInt(writer, value.heroicBossKills);
		NetworkWriterExtensions.WriteInt(writer, value.miniBossKills);
		NetworkWriterExtensions.WriteInt(writer, value.hunterKills);
		NetworkWriterExtensions.WriteInt(writer, value.totalGoldIncome);
		NetworkWriterExtensions.WriteInt(writer, value.totalDreamDustIncome);
		NetworkWriterExtensions.WriteInt(writer, value.totalStardustIncome);
		NetworkWriterExtensions.WriteInt(writer, value.deaths);
		NetworkWriterExtensions.WriteInt(writer, value.combatTime);
		NetworkWriterExtensions.WriteFloat(writer, value.dealtDamageToEnemies);
		NetworkWriterExtensions.WriteFloat(writer, value.maxDealtSingleDamageToEnemy);
		NetworkWriterExtensions.WriteFloat(writer, value.healToSelf);
		NetworkWriterExtensions.WriteFloat(writer, value.healToOthers);
		NetworkWriterExtensions.WriteFloat(writer, value.receivedDamage);
		NetworkWriterExtensions.WriteString(writer, value.causeOfDeathActor);
		NetworkWriterExtensions.WriteString(writer, value.causeOfDeathEntity);
		_Write_HeroLoadoutData(writer, value.loadout);
		writer.WriteDictionary0(value.capturedStarTooltipFields);
		_Write_System_002ECollections_002EGeneric_002EList_00601_003CDewGameResult_002FSkillData_003E(writer, value.skills);
		_Write_System_002ECollections_002EGeneric_002EList_00601_003CDewGameResult_002FGemData_003E(writer, value.gems);
		_Write_System_002ECollections_002EGeneric_002EList_00601_003CSystem_002EInt32_003E(writer, value.maxGemCounts);
	}

	public static void _Write_System_002ECollections_002EGeneric_002EList_00601_003CDewGameResult_002FSkillData_003E(NetworkWriter writer, List<DewGameResult.SkillData> value)
	{
		NetworkWriterExtensions.WriteList<DewGameResult.SkillData>(writer, value);
	}

	public static void _Write_DewGameResult_002FSkillData(NetworkWriter writer, DewGameResult.SkillData value)
	{
		NetworkWriterExtensions.WriteUInt(writer, value.netId);
		_Write_HeroSkillLocation(writer, value.loc);
		_Write_SkillType(writer, value.type);
		NetworkWriterExtensions.WriteString(writer, value.name);
		NetworkWriterExtensions.WriteInt(writer, value.level);
		NetworkWriterExtensions.WriteInt(writer, value.maxCharges);
		NetworkWriterExtensions.WriteFloat(writer, value.cooldownTime);
		writer.WriteDictionary0(value.capturedTooltipFields);
	}

	public static void _Write_System_002ECollections_002EGeneric_002EList_00601_003CDewGameResult_002FGemData_003E(NetworkWriter writer, List<DewGameResult.GemData> value)
	{
		NetworkWriterExtensions.WriteList<DewGameResult.GemData>(writer, value);
	}

	public static void _Write_DewGameResult_002FGemData(NetworkWriter writer, DewGameResult.GemData value)
	{
		NetworkWriterExtensions.WriteUInt(writer, value.netId);
		_Write_GemLocation(writer, value.location);
		NetworkWriterExtensions.WriteString(writer, value.name);
		NetworkWriterExtensions.WriteInt(writer, value.quality);
		writer.WriteDictionary0(value.capturedTooltipFields);
	}

	public static void _Write_System_002ECollections_002EGeneric_002EList_00601_003CSystem_002EInt32_003E(NetworkWriter writer, List<int> value)
	{
		NetworkWriterExtensions.WriteList<int>(writer, value);
	}

	public static DewGameResult _Read_DewGameResult(NetworkReader reader)
	{
		if (!NetworkReaderExtensions.ReadBool(reader))
		{
			return null;
		}
		DewGameResult dewGameResult = new DewGameResult();
		dewGameResult.runId = NetworkReaderExtensions.ReadString(reader);
		dewGameResult.result = _Read_DewGameResult_002FResultType(reader);
		dewGameResult.startTimestamp = NetworkReaderExtensions.ReadLong(reader);
		dewGameResult.elapsedGameTimeSeconds = NetworkReaderExtensions.ReadInt(reader);
		dewGameResult.visitedWorlds = NetworkReaderExtensions.ReadInt(reader);
		dewGameResult.visitedLocations = NetworkReaderExtensions.ReadInt(reader);
		dewGameResult.difficulty = NetworkReaderExtensions.ReadString(reader);
		dewGameResult.limboDepth = NetworkReaderExtensions.ReadInt(reader);
		dewGameResult.players = _Read_System_002ECollections_002EGeneric_002EList_00601_003CDewGameResult_002FPlayerData_003E(reader);
		return dewGameResult;
	}

	public static DewGameResult.ResultType _Read_DewGameResult_002FResultType(NetworkReader reader)
	{
		return (DewGameResult.ResultType)NetworkReaderExtensions.ReadInt(reader);
	}

	public static List<DewGameResult.PlayerData> _Read_System_002ECollections_002EGeneric_002EList_00601_003CDewGameResult_002FPlayerData_003E(NetworkReader reader)
	{
		return NetworkReaderExtensions.ReadList<DewGameResult.PlayerData>(reader);
	}

	public static DewGameResult.PlayerData _Read_DewGameResult_002FPlayerData(NetworkReader reader)
	{
		if (!NetworkReaderExtensions.ReadBool(reader))
		{
			return null;
		}
		DewGameResult.PlayerData playerData = new DewGameResult.PlayerData();
		playerData.isLocalPlayer = NetworkReaderExtensions.ReadBool(reader);
		playerData.playerGuid = NetworkReaderExtensions.ReadString(reader);
		playerData.platform = NetworkReaderExtensions.ReadString(reader);
		playerData.platformID = NetworkReaderExtensions.ReadString(reader);
		playerData.playerProfileName = NetworkReaderExtensions.ReadString(reader);
		playerData.heroType = NetworkReaderExtensions.ReadString(reader);
		playerData.maxHealth = NetworkReaderExtensions.ReadFloat(reader);
		playerData.attackDamage = NetworkReaderExtensions.ReadFloat(reader);
		playerData.abilityPower = NetworkReaderExtensions.ReadFloat(reader);
		playerData.skillHaste = NetworkReaderExtensions.ReadFloat(reader);
		playerData.attackSpeed = NetworkReaderExtensions.ReadFloat(reader);
		playerData.fireAmp = NetworkReaderExtensions.ReadFloat(reader);
		playerData.armor = NetworkReaderExtensions.ReadFloat(reader);
		playerData.addedHp = NetworkReaderExtensions.ReadFloat(reader);
		playerData.critChance = NetworkReaderExtensions.ReadFloat(reader);
		playerData.level = NetworkReaderExtensions.ReadInt(reader);
		playerData.kills = NetworkReaderExtensions.ReadInt(reader);
		playerData.heroicBossKills = NetworkReaderExtensions.ReadInt(reader);
		playerData.miniBossKills = NetworkReaderExtensions.ReadInt(reader);
		playerData.hunterKills = NetworkReaderExtensions.ReadInt(reader);
		playerData.totalGoldIncome = NetworkReaderExtensions.ReadInt(reader);
		playerData.totalDreamDustIncome = NetworkReaderExtensions.ReadInt(reader);
		playerData.totalStardustIncome = NetworkReaderExtensions.ReadInt(reader);
		playerData.deaths = NetworkReaderExtensions.ReadInt(reader);
		playerData.combatTime = NetworkReaderExtensions.ReadInt(reader);
		playerData.dealtDamageToEnemies = NetworkReaderExtensions.ReadFloat(reader);
		playerData.maxDealtSingleDamageToEnemy = NetworkReaderExtensions.ReadFloat(reader);
		playerData.healToSelf = NetworkReaderExtensions.ReadFloat(reader);
		playerData.healToOthers = NetworkReaderExtensions.ReadFloat(reader);
		playerData.receivedDamage = NetworkReaderExtensions.ReadFloat(reader);
		playerData.causeOfDeathActor = NetworkReaderExtensions.ReadString(reader);
		playerData.causeOfDeathEntity = NetworkReaderExtensions.ReadString(reader);
		playerData.loadout = _Read_HeroLoadoutData(reader);
		playerData.capturedStarTooltipFields = reader.ReadDictionary0();
		playerData.skills = _Read_System_002ECollections_002EGeneric_002EList_00601_003CDewGameResult_002FSkillData_003E(reader);
		playerData.gems = _Read_System_002ECollections_002EGeneric_002EList_00601_003CDewGameResult_002FGemData_003E(reader);
		playerData.maxGemCounts = _Read_System_002ECollections_002EGeneric_002EList_00601_003CSystem_002EInt32_003E(reader);
		return playerData;
	}

	public static List<DewGameResult.SkillData> _Read_System_002ECollections_002EGeneric_002EList_00601_003CDewGameResult_002FSkillData_003E(NetworkReader reader)
	{
		return NetworkReaderExtensions.ReadList<DewGameResult.SkillData>(reader);
	}

	public static DewGameResult.SkillData _Read_DewGameResult_002FSkillData(NetworkReader reader)
	{
		return new DewGameResult.SkillData
		{
			netId = NetworkReaderExtensions.ReadUInt(reader),
			loc = _Read_HeroSkillLocation(reader),
			type = _Read_SkillType(reader),
			name = NetworkReaderExtensions.ReadString(reader),
			level = NetworkReaderExtensions.ReadInt(reader),
			maxCharges = NetworkReaderExtensions.ReadInt(reader),
			cooldownTime = NetworkReaderExtensions.ReadFloat(reader),
			capturedTooltipFields = reader.ReadDictionary0()
		};
	}

	public static List<DewGameResult.GemData> _Read_System_002ECollections_002EGeneric_002EList_00601_003CDewGameResult_002FGemData_003E(NetworkReader reader)
	{
		return NetworkReaderExtensions.ReadList<DewGameResult.GemData>(reader);
	}

	public static DewGameResult.GemData _Read_DewGameResult_002FGemData(NetworkReader reader)
	{
		return new DewGameResult.GemData
		{
			netId = NetworkReaderExtensions.ReadUInt(reader),
			location = _Read_GemLocation(reader),
			name = NetworkReaderExtensions.ReadString(reader),
			quality = NetworkReaderExtensions.ReadInt(reader),
			capturedTooltipFields = reader.ReadDictionary0()
		};
	}

	public static List<int> _Read_System_002ECollections_002EGeneric_002EList_00601_003CSystem_002EInt32_003E(NetworkReader reader)
	{
		return NetworkReaderExtensions.ReadList<int>(reader);
	}

	public static void _Write_GameState(NetworkWriter writer, GameState value)
	{
		NetworkWriterExtensions.WriteInt(writer, (int)value);
	}

	public static void _Write_AllowMidJoinType(NetworkWriter writer, AllowMidJoinType value)
	{
		NetworkWriterExtensions.WriteInt(writer, (int)value);
	}

	public static void _Write_MidJoinBanType(NetworkWriter writer, MidJoinBanType value)
	{
		NetworkWriterExtensions.WriteInt(writer, (int)value);
	}

	public static void _Write_MidJoinWaitType(NetworkWriter writer, MidJoinWaitType value)
	{
		NetworkWriterExtensions.WriteInt(writer, (int)value);
	}

	public static GameState _Read_GameState(NetworkReader reader)
	{
		return (GameState)NetworkReaderExtensions.ReadInt(reader);
	}

	public static AllowMidJoinType _Read_AllowMidJoinType(NetworkReader reader)
	{
		return (AllowMidJoinType)NetworkReaderExtensions.ReadInt(reader);
	}

	public static MidJoinBanType _Read_MidJoinBanType(NetworkReader reader)
	{
		return (MidJoinBanType)NetworkReaderExtensions.ReadInt(reader);
	}

	public static MidJoinWaitType _Read_MidJoinWaitType(NetworkReader reader)
	{
		return (MidJoinWaitType)NetworkReaderExtensions.ReadInt(reader);
	}

	public static void _Write_PingManager_002FPing(NetworkWriter writer, PingManager.Ping value)
	{
		NetworkWriterExtensions.WriteNetworkBehaviour(writer, (NetworkBehaviour)(object)value.sender);
		_Write_PingManager_002FPingType(writer, value.type);
		NetworkWriterExtensions.WriteNetworkBehaviour(writer, value.target);
		NetworkWriterExtensions.WriteVector3(writer, value.position);
		NetworkWriterExtensions.WriteInt(writer, value.itemIndex);
	}

	public static void _Write_PingManager_002FPingType(NetworkWriter writer, PingManager.PingType value)
	{
		NetworkWriterExtensions.WriteByte(writer, (byte)value);
	}

	public static PingManager.Ping _Read_PingManager_002FPing(NetworkReader reader)
	{
		return new PingManager.Ping
		{
			sender = NetworkReaderExtensions.ReadNetworkBehaviour<DewPlayer>(reader),
			type = _Read_PingManager_002FPingType(reader),
			target = NetworkReaderExtensions.ReadNetworkBehaviour(reader),
			position = NetworkReaderExtensions.ReadVector3(reader),
			itemIndex = NetworkReaderExtensions.ReadInt(reader)
		};
	}

	public static PingManager.PingType _Read_PingManager_002FPingType(NetworkReader reader)
	{
		return (PingManager.PingType)NetworkReaderExtensions.ReadByte(reader);
	}

	public static WorldNodeData _Read_WorldNodeData(NetworkReader reader)
	{
		return new WorldNodeData
		{
			type = _Read_WorldNodeType(reader),
			status = _Read_WorldNodeStatus(reader),
			room = NetworkReaderExtensions.ReadString(reader),
			roomOverride = NetworkReaderExtensions.ReadString(reader),
			roomRotValue = NetworkReaderExtensions.ReadFloat(reader),
			roomRotIndex = NetworkReaderExtensions.ReadInt(reader),
			modifiers = _Read_System_002ECollections_002EGeneric_002EList_00601_003CModifierData_003E(reader),
			position = NetworkReaderExtensions.ReadVector2(reader)
		};
	}

	public static WorldNodeType _Read_WorldNodeType(NetworkReader reader)
	{
		return (WorldNodeType)NetworkReaderExtensions.ReadInt(reader);
	}

	public static WorldNodeStatus _Read_WorldNodeStatus(NetworkReader reader)
	{
		return (WorldNodeStatus)NetworkReaderExtensions.ReadInt(reader);
	}

	public static List<ModifierData> _Read_System_002ECollections_002EGeneric_002EList_00601_003CModifierData_003E(NetworkReader reader)
	{
		return NetworkReaderExtensions.ReadList<ModifierData>(reader);
	}

	public static ModifierData _Read_ModifierData(NetworkReader reader)
	{
		return new ModifierData
		{
			id = NetworkReaderExtensions.ReadInt(reader),
			type = NetworkReaderExtensions.ReadString(reader),
			clientData = NetworkReaderExtensions.ReadString(reader),
			isForceRevealed = NetworkReaderExtensions.ReadBool(reader)
		};
	}

	public static void _Write_WorldNodeData(NetworkWriter writer, WorldNodeData value)
	{
		_Write_WorldNodeType(writer, value.type);
		_Write_WorldNodeStatus(writer, value.status);
		NetworkWriterExtensions.WriteString(writer, value.room);
		NetworkWriterExtensions.WriteString(writer, value.roomOverride);
		NetworkWriterExtensions.WriteFloat(writer, value.roomRotValue);
		NetworkWriterExtensions.WriteInt(writer, value.roomRotIndex);
		_Write_System_002ECollections_002EGeneric_002EList_00601_003CModifierData_003E(writer, value.modifiers);
		NetworkWriterExtensions.WriteVector2(writer, value.position);
	}

	public static void _Write_WorldNodeType(NetworkWriter writer, WorldNodeType value)
	{
		NetworkWriterExtensions.WriteInt(writer, (int)value);
	}

	public static void _Write_WorldNodeStatus(NetworkWriter writer, WorldNodeStatus value)
	{
		NetworkWriterExtensions.WriteInt(writer, (int)value);
	}

	public static void _Write_System_002ECollections_002EGeneric_002EList_00601_003CModifierData_003E(NetworkWriter writer, List<ModifierData> value)
	{
		NetworkWriterExtensions.WriteList<ModifierData>(writer, value);
	}

	public static void _Write_ModifierData(NetworkWriter writer, ModifierData value)
	{
		NetworkWriterExtensions.WriteInt(writer, value.id);
		NetworkWriterExtensions.WriteString(writer, value.type);
		NetworkWriterExtensions.WriteString(writer, value.clientData);
		NetworkWriterExtensions.WriteBool(writer, value.isForceRevealed);
	}

	public static HunterStatus _Read_HunterStatus(NetworkReader reader)
	{
		return (HunterStatus)NetworkReaderExtensions.ReadInt(reader);
	}

	public static void _Write_HunterStatus(NetworkWriter writer, HunterStatus value)
	{
		NetworkWriterExtensions.WriteInt(writer, (int)value);
	}

	public static void _Write_EventInfoLoadRoom(NetworkWriter writer, EventInfoLoadRoom value)
	{
		NetworkWriterExtensions.WriteInt(writer, value.fromIndex);
		NetworkWriterExtensions.WriteInt(writer, value.toIndex);
		NetworkWriterExtensions.WriteBool(writer, value.isSidetrackTransition);
		NetworkWriterExtensions.WriteBool(writer, value.isLoadingFromSave);
		NetworkWriterExtensions.WriteBool(writer, value.isTraveling);
	}

	public static EventInfoLoadRoom _Read_EventInfoLoadRoom(NetworkReader reader)
	{
		return new EventInfoLoadRoom
		{
			fromIndex = NetworkReaderExtensions.ReadInt(reader),
			toIndex = NetworkReaderExtensions.ReadInt(reader),
			isSidetrackTransition = NetworkReaderExtensions.ReadBool(reader),
			isLoadingFromSave = NetworkReaderExtensions.ReadBool(reader),
			isTraveling = NetworkReaderExtensions.ReadBool(reader)
		};
	}

	public static void _Write_EventInfoLoadZone(NetworkWriter writer, EventInfoLoadZone value)
	{
		NetworkWriterExtensions.WriteString(writer, value.from);
		NetworkWriterExtensions.WriteString(writer, value.to);
		NetworkWriterExtensions.WriteBool(writer, value.isLoadingFromSave);
		NetworkWriterExtensions.WriteBool(writer, value.isTraveling);
	}

	public static EventInfoLoadZone _Read_EventInfoLoadZone(NetworkReader reader)
	{
		return new EventInfoLoadZone
		{
			from = NetworkReaderExtensions.ReadString(reader),
			to = NetworkReaderExtensions.ReadString(reader),
			isLoadingFromSave = NetworkReaderExtensions.ReadBool(reader),
			isTraveling = NetworkReaderExtensions.ReadBool(reader)
		};
	}

	public static void _Write_VoteType(NetworkWriter writer, VoteType value)
	{
		NetworkWriterExtensions.WriteInt(writer, (int)value);
	}

	public static VoteType _Read_VoteType(NetworkReader reader)
	{
		return (VoteType)NetworkReaderExtensions.ReadInt(reader);
	}

	public static void _Write_RoomMonsters_002FPrewarmEntry_005B_005D(NetworkWriter writer, RoomMonsters.PrewarmEntry[] value)
	{
		NetworkWriterExtensions.WriteArray<RoomMonsters.PrewarmEntry>(writer, value);
	}

	public static void _Write_RoomMonsters_002FPrewarmEntry(NetworkWriter writer, RoomMonsters.PrewarmEntry value)
	{
		NetworkWriterExtensions.WriteUInt(writer, value.assetId);
		NetworkWriterExtensions.WriteInt(writer, value.count);
		NetworkWriterExtensions.WriteUInt(writer, value.ownerNetId);
	}

	public static RoomMonsters.PrewarmEntry[] _Read_RoomMonsters_002FPrewarmEntry_005B_005D(NetworkReader reader)
	{
		return NetworkReaderExtensions.ReadArray<RoomMonsters.PrewarmEntry>(reader);
	}

	public static RoomMonsters.PrewarmEntry _Read_RoomMonsters_002FPrewarmEntry(NetworkReader reader)
	{
		return new RoomMonsters.PrewarmEntry
		{
			assetId = NetworkReaderExtensions.ReadUInt(reader),
			count = NetworkReaderExtensions.ReadInt(reader),
			ownerNetId = NetworkReaderExtensions.ReadUInt(reader)
		};
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
		Writer<float?>.write = NetworkReaderWriterExtensions.WriteNullableFloat;
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
		Writer<QuestProgressType>.write = _Write_QuestProgressType;
		Writer<FailReason>.write = _Write_FailReason;
		Writer<QuestState>.write = _Write_QuestState;
		Writer<Projectile.EntityHit>.write = _Write_Projectile_002FEntityHit;
		Writer<Projectile.ProjectileMode>.write = _Write_Projectile_002FProjectileMode;
		Writer<Projectile.StartPositionType>.write = _Write_Projectile_002FStartPositionType;
		Writer<EventInfoKill>.write = _Write_EventInfoKill;
		Writer<Actor>.write = NetworkWriterExtensions.WriteNetworkBehaviour;
		Writer<Monster.MonsterType>.write = _Write_Monster_002FMonsterType;
		Writer<MerchandiseData[]>.write = _Write_MerchandiseData_005B_005D;
		Writer<MerchandiseData>.write = _Write_MerchandiseData;
		Writer<MerchandiseType>.write = _Write_MerchandiseType;
		Writer<Cost>.write = _Write_Cost;
		Writer<float[]>.write = _Write_System_002ESingle_005B_005D;
		Writer<int[]>.write = _Write_System_002EInt32_005B_005D;
		Writer<AbilityTrigger.ConfigSyncData[]>.write = _Write_AbilityTrigger_002FConfigSyncData_005B_005D;
		Writer<AbilityTrigger.ConfigSyncData>.write = _Write_AbilityTrigger_002FConfigSyncData;
		Writer<List<string>>.write = _Write_System_002ECollections_002EGeneric_002EList_00601_003CSystem_002EString_003E;
		Writer<ChoiceShrineItem[]>.write = _Write_ChoiceShrineItem_005B_005D;
		Writer<ChoiceShrineItem>.write = _Write_ChoiceShrineItem;
		Writer<StatBonus>.write = _Write_StatBonus;
		Writer<ChaosReward[]>.write = _Write_ChaosReward_005B_005D;
		Writer<ChaosReward>.write = _Write_ChaosReward;
		Writer<ChaosRewardType>.write = _Write_ChaosRewardType;
		Writer<Rarity>.write = _Write_Rarity;
		Writer<DewPlayer>.write = NetworkWriterExtensions.WriteNetworkBehaviour;
		Writer<SyncableAssetRef>.write = _Write_SyncableAssetRef;
		Writer<HeroLoadoutData>.write = _Write_HeroLoadoutData;
		Writer<List<LoadoutStarItem>>.write = _Write_System_002ECollections_002EGeneric_002EList_00601_003CLoadoutStarItem_003E;
		Writer<LoadoutStarItem>.write = _Write_LoadoutStarItem;
		Writer<string[]>.write = _Write_System_002EString_005B_005D;
		Writer<List<JsonOverrideItem>>.write = _Write_System_002ECollections_002EGeneric_002EList_00601_003CJsonOverrideItem_003E;
		Writer<JsonOverrideItem>.write = _Write_JsonOverrideItem;
		Writer<InputMode>.write = _Write_InputMode;
		Writer<WorldMessageSetting>.write = _Write_WorldMessageSetting;
		Writer<CenterMessageType>.write = _Write_CenterMessageType;
		Writer<NetworkedOnScreenTimerHandle>.write = _Write_NetworkedOnScreenTimerHandle;
		Writer<SampleCastInfoContext>.write = _Write_SampleCastInfoContext;
		Writer<AbilityTargetValidator>.write = _Write_AbilityTargetValidator;
		Writer<EntityRelation>.write = _Write_EntityRelation;
		Writer<AbilityTrigger>.write = NetworkWriterExtensions.WriteNetworkBehaviour;
		Writer<SampleCastInfoContext.CastOnButtonType>.write = _Write_SampleCastInfoContext_002FCastOnButtonType;
		Writer<DewProfileStats>.write = _Write_DewProfileStats;
		Writer<DewProfileStats.HeroData>.write = _Write_DewProfileStats_002FHeroData;
		Writer<DewProfileStats.ItemData>.write = _Write_DewProfileStats_002FItemData;
		Writer<DewProfileStats.MonsterData>.write = _Write_DewProfileStats_002FMonsterData;
		Writer<DewProfileStats.ZoneData>.write = _Write_DewProfileStats_002FZoneData;
		Writer<PlayerState>.write = _Write_PlayerState;
		Writer<DewPlayer.Role>.write = _Write_DewPlayer_002FRole;
		Writer<CSteamID>.write = _Write_Steamworks_002ECSteamID;
		Writer<HatredStrengthType>.write = _Write_HatredStrengthType;
		Writer<EventInfoSkillUse>.write = _Write_EventInfoSkillUse;
		Writer<HeroSkillLocation>.write = _Write_HeroSkillLocation;
		Writer<SkillTrigger>.write = NetworkWriterExtensions.WriteNetworkBehaviour;
		Writer<SkillType>.write = _Write_SkillType;
		Writer<DescriptionTags>.write = _Write_DescriptionTags;
		Writer<SyncedNetworkBehaviour>.write = _Write_SyncedNetworkBehaviour;
		Writer<EntityAnimation.ReplaceableAnimationType>.write = _Write_EntityAnimation_002FReplaceableAnimationType;
		Writer<EntityControl.BlockableAction>.write = _Write_EntityControl_002FBlockableAction;
		Writer<EntityControl.PositionSyncData>.write = _Write_EntityControl_002FPositionSyncData;
		Writer<BaseStats>.write = _Write_BaseStats;
		Writer<BonusStats>.write = _Write_BonusStats;
		Writer<FinalStats>.write = _Write_FinalStats;
		Writer<BasicEffectMask>.write = _Write_BasicEffectMask;
		Writer<GibInfo>.write = _Write_GibInfo;
		Writer<KnockUpStrength>.write = _Write_KnockUpStrength;
		Writer<GemLocation>.write = _Write_GemLocation;
		Writer<Room_Waypoint>.write = NetworkWriterExtensions.WriteNetworkBehaviour;
		Writer<Gem>.write = NetworkWriterExtensions.WriteNetworkBehaviour;
		Writer<List<uint>>.write = _Write_System_002ECollections_002EGeneric_002EList_00601_003CSystem_002EUInt32_003E;
		Writer<ChatManager.Message>.write = _Write_ChatManager_002FMessage;
		Writer<ChatManager.MessageType>.write = _Write_ChatManager_002FMessageType;
		Writer<EventInfoHeal>.write = _Write_EventInfoHeal;
		Writer<FinalHealData>.write = _Write_FinalHealData;
		Writer<ActorFlags>.write = _Write_ActorFlags;
		Writer<ReactionChain>.write = _Write_ReactionChain;
		Writer<List<Actor>>.write = _Write_System_002ECollections_002EGeneric_002EList_00601_003CActor_003E;
		Writer<EventInfoDamage>.write = _Write_EventInfoDamage;
		Writer<FinalDamageData>.write = _Write_FinalDamageData;
		Writer<AttackEffectType>.write = _Write_AttackEffectType;
		Writer<DamageData.SourceType>.write = _Write_DamageData_002FSourceType;
		Writer<DamageAttribute>.write = _Write_DamageAttribute;
		Writer<EventInfoShield>.write = _Write_EventInfoShield;
		Writer<StatusEffect>.write = NetworkWriterExtensions.WriteNetworkBehaviour;
		Writer<EventInfoSpentMana>.write = _Write_EventInfoSpentMana;
		Writer<EventInfoAttackMissed>.write = _Write_EventInfoAttackMissed;
		Writer<EventInfoDamageNegatedByImmunity>.write = _Write_EventInfoDamageNegatedByImmunity;
		Writer<EventInfoDamageNegatedByShield>.write = _Write_EventInfoDamageNegatedByShield;
		Writer<EventInfoAttackHit>.write = _Write_EventInfoAttackHit;
		Writer<Hero>.write = NetworkWriterExtensions.WriteNetworkBehaviour;
		Writer<EventInfoApplyElemental>.write = _Write_EventInfoApplyElemental;
		Writer<ElementalType>.write = _Write_ElementalType;
		Writer<EventInfoCast>.write = _Write_EventInfoCast;
		Writer<AbilityInstance>.write = NetworkWriterExtensions.WriteNetworkBehaviour;
		Writer<char[]>.write = _Write_System_002EChar_005B_005D;
		Writer<DewConversationSettings>.write = _Write_DewConversationSettings;
		Writer<Entity[]>.write = _Write_Entity_005B_005D;
		Writer<ConversationVisibility>.write = _Write_ConversationVisibility;
		Writer<DewGameResult>.write = _Write_DewGameResult;
		Writer<DewGameResult.ResultType>.write = _Write_DewGameResult_002FResultType;
		Writer<List<DewGameResult.PlayerData>>.write = _Write_System_002ECollections_002EGeneric_002EList_00601_003CDewGameResult_002FPlayerData_003E;
		Writer<DewGameResult.PlayerData>.write = _Write_DewGameResult_002FPlayerData;
		Writer<List<DewGameResult.SkillData>>.write = _Write_System_002ECollections_002EGeneric_002EList_00601_003CDewGameResult_002FSkillData_003E;
		Writer<DewGameResult.SkillData>.write = _Write_DewGameResult_002FSkillData;
		Writer<List<DewGameResult.GemData>>.write = _Write_System_002ECollections_002EGeneric_002EList_00601_003CDewGameResult_002FGemData_003E;
		Writer<DewGameResult.GemData>.write = _Write_DewGameResult_002FGemData;
		Writer<List<int>>.write = _Write_System_002ECollections_002EGeneric_002EList_00601_003CSystem_002EInt32_003E;
		Writer<GameState>.write = _Write_GameState;
		Writer<AllowMidJoinType>.write = _Write_AllowMidJoinType;
		Writer<MidJoinBanType>.write = _Write_MidJoinBanType;
		Writer<MidJoinWaitType>.write = _Write_MidJoinWaitType;
		Writer<PingManager.Ping>.write = _Write_PingManager_002FPing;
		Writer<PingManager.PingType>.write = _Write_PingManager_002FPingType;
		Writer<WorldNodeData>.write = _Write_WorldNodeData;
		Writer<WorldNodeType>.write = _Write_WorldNodeType;
		Writer<WorldNodeStatus>.write = _Write_WorldNodeStatus;
		Writer<List<ModifierData>>.write = _Write_System_002ECollections_002EGeneric_002EList_00601_003CModifierData_003E;
		Writer<ModifierData>.write = _Write_ModifierData;
		Writer<HunterStatus>.write = _Write_HunterStatus;
		Writer<EventInfoLoadRoom>.write = _Write_EventInfoLoadRoom;
		Writer<EventInfoLoadZone>.write = _Write_EventInfoLoadZone;
		Writer<VoteType>.write = _Write_VoteType;
		Writer<RoomMonsters.PrewarmEntry[]>.write = _Write_RoomMonsters_002FPrewarmEntry_005B_005D;
		Writer<RoomMonsters.PrewarmEntry>.write = _Write_RoomMonsters_002FPrewarmEntry;
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
		Reader<float?>.read = NetworkReaderWriterExtensions.ReadNullableFloat;
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
		Reader<QuestProgressType>.read = _Read_QuestProgressType;
		Reader<FailReason>.read = _Read_FailReason;
		Reader<QuestState>.read = _Read_QuestState;
		Reader<Projectile.EntityHit>.read = _Read_Projectile_002FEntityHit;
		Reader<Projectile.ProjectileMode>.read = _Read_Projectile_002FProjectileMode;
		Reader<Projectile.StartPositionType>.read = _Read_Projectile_002FStartPositionType;
		Reader<EventInfoKill>.read = _Read_EventInfoKill;
		Reader<Actor>.read = NetworkReaderExtensions.ReadNetworkBehaviour<Actor>;
		Reader<Monster.MonsterType>.read = _Read_Monster_002FMonsterType;
		Reader<MerchandiseData[]>.read = _Read_MerchandiseData_005B_005D;
		Reader<MerchandiseData>.read = _Read_MerchandiseData;
		Reader<MerchandiseType>.read = _Read_MerchandiseType;
		Reader<Cost>.read = _Read_Cost;
		Reader<float[]>.read = _Read_System_002ESingle_005B_005D;
		Reader<int[]>.read = _Read_System_002EInt32_005B_005D;
		Reader<AbilityTrigger.ConfigSyncData[]>.read = _Read_AbilityTrigger_002FConfigSyncData_005B_005D;
		Reader<AbilityTrigger.ConfigSyncData>.read = _Read_AbilityTrigger_002FConfigSyncData;
		Reader<List<string>>.read = _Read_System_002ECollections_002EGeneric_002EList_00601_003CSystem_002EString_003E;
		Reader<ChoiceShrineItem[]>.read = _Read_ChoiceShrineItem_005B_005D;
		Reader<ChoiceShrineItem>.read = _Read_ChoiceShrineItem;
		Reader<StatBonus>.read = _Read_StatBonus;
		Reader<ChaosReward[]>.read = _Read_ChaosReward_005B_005D;
		Reader<ChaosReward>.read = _Read_ChaosReward;
		Reader<ChaosRewardType>.read = _Read_ChaosRewardType;
		Reader<Rarity>.read = _Read_Rarity;
		Reader<DewPlayer>.read = NetworkReaderExtensions.ReadNetworkBehaviour<DewPlayer>;
		Reader<SyncableAssetRef>.read = _Read_SyncableAssetRef;
		Reader<HeroLoadoutData>.read = _Read_HeroLoadoutData;
		Reader<List<LoadoutStarItem>>.read = _Read_System_002ECollections_002EGeneric_002EList_00601_003CLoadoutStarItem_003E;
		Reader<LoadoutStarItem>.read = _Read_LoadoutStarItem;
		Reader<string[]>.read = _Read_System_002EString_005B_005D;
		Reader<List<JsonOverrideItem>>.read = _Read_System_002ECollections_002EGeneric_002EList_00601_003CJsonOverrideItem_003E;
		Reader<JsonOverrideItem>.read = _Read_JsonOverrideItem;
		Reader<InputMode>.read = _Read_InputMode;
		Reader<WorldMessageSetting>.read = _Read_WorldMessageSetting;
		Reader<CenterMessageType>.read = _Read_CenterMessageType;
		Reader<NetworkedOnScreenTimerHandle>.read = _Read_NetworkedOnScreenTimerHandle;
		Reader<SampleCastInfoContext>.read = _Read_SampleCastInfoContext;
		Reader<AbilityTargetValidator>.read = _Read_AbilityTargetValidator;
		Reader<EntityRelation>.read = _Read_EntityRelation;
		Reader<AbilityTrigger>.read = NetworkReaderExtensions.ReadNetworkBehaviour<AbilityTrigger>;
		Reader<SampleCastInfoContext.CastOnButtonType>.read = _Read_SampleCastInfoContext_002FCastOnButtonType;
		Reader<DewProfileStats>.read = _Read_DewProfileStats;
		Reader<DewProfileStats.HeroData>.read = _Read_DewProfileStats_002FHeroData;
		Reader<DewProfileStats.ItemData>.read = _Read_DewProfileStats_002FItemData;
		Reader<DewProfileStats.MonsterData>.read = _Read_DewProfileStats_002FMonsterData;
		Reader<DewProfileStats.ZoneData>.read = _Read_DewProfileStats_002FZoneData;
		Reader<PlayerState>.read = _Read_PlayerState;
		Reader<DewPlayer.Role>.read = _Read_DewPlayer_002FRole;
		Reader<CSteamID>.read = _Read_Steamworks_002ECSteamID;
		Reader<HatredStrengthType>.read = _Read_HatredStrengthType;
		Reader<EventInfoSkillUse>.read = _Read_EventInfoSkillUse;
		Reader<HeroSkillLocation>.read = _Read_HeroSkillLocation;
		Reader<SkillTrigger>.read = NetworkReaderExtensions.ReadNetworkBehaviour<SkillTrigger>;
		Reader<SkillType>.read = _Read_SkillType;
		Reader<DescriptionTags>.read = _Read_DescriptionTags;
		Reader<SyncedNetworkBehaviour>.read = _Read_SyncedNetworkBehaviour;
		Reader<EntityAnimation.ReplaceableAnimationType>.read = _Read_EntityAnimation_002FReplaceableAnimationType;
		Reader<EntityControl.BlockableAction>.read = _Read_EntityControl_002FBlockableAction;
		Reader<EntityControl.PositionSyncData>.read = _Read_EntityControl_002FPositionSyncData;
		Reader<BaseStats>.read = _Read_BaseStats;
		Reader<BonusStats>.read = _Read_BonusStats;
		Reader<FinalStats>.read = _Read_FinalStats;
		Reader<BasicEffectMask>.read = _Read_BasicEffectMask;
		Reader<GibInfo>.read = _Read_GibInfo;
		Reader<KnockUpStrength>.read = _Read_KnockUpStrength;
		Reader<GemLocation>.read = _Read_GemLocation;
		Reader<Room_Waypoint>.read = NetworkReaderExtensions.ReadNetworkBehaviour<Room_Waypoint>;
		Reader<Gem>.read = NetworkReaderExtensions.ReadNetworkBehaviour<Gem>;
		Reader<List<uint>>.read = _Read_System_002ECollections_002EGeneric_002EList_00601_003CSystem_002EUInt32_003E;
		Reader<ChatManager.Message>.read = _Read_ChatManager_002FMessage;
		Reader<ChatManager.MessageType>.read = _Read_ChatManager_002FMessageType;
		Reader<EventInfoHeal>.read = _Read_EventInfoHeal;
		Reader<FinalHealData>.read = _Read_FinalHealData;
		Reader<ActorFlags>.read = _Read_ActorFlags;
		Reader<ReactionChain>.read = _Read_ReactionChain;
		Reader<List<Actor>>.read = _Read_System_002ECollections_002EGeneric_002EList_00601_003CActor_003E;
		Reader<EventInfoDamage>.read = _Read_EventInfoDamage;
		Reader<FinalDamageData>.read = _Read_FinalDamageData;
		Reader<AttackEffectType>.read = _Read_AttackEffectType;
		Reader<DamageData.SourceType>.read = _Read_DamageData_002FSourceType;
		Reader<DamageAttribute>.read = _Read_DamageAttribute;
		Reader<EventInfoShield>.read = _Read_EventInfoShield;
		Reader<StatusEffect>.read = NetworkReaderExtensions.ReadNetworkBehaviour<StatusEffect>;
		Reader<EventInfoSpentMana>.read = _Read_EventInfoSpentMana;
		Reader<EventInfoAttackMissed>.read = _Read_EventInfoAttackMissed;
		Reader<EventInfoDamageNegatedByImmunity>.read = _Read_EventInfoDamageNegatedByImmunity;
		Reader<EventInfoDamageNegatedByShield>.read = _Read_EventInfoDamageNegatedByShield;
		Reader<EventInfoAttackHit>.read = _Read_EventInfoAttackHit;
		Reader<Hero>.read = NetworkReaderExtensions.ReadNetworkBehaviour<Hero>;
		Reader<EventInfoApplyElemental>.read = _Read_EventInfoApplyElemental;
		Reader<ElementalType>.read = _Read_ElementalType;
		Reader<EventInfoCast>.read = _Read_EventInfoCast;
		Reader<AbilityInstance>.read = NetworkReaderExtensions.ReadNetworkBehaviour<AbilityInstance>;
		Reader<char[]>.read = _Read_System_002EChar_005B_005D;
		Reader<DewConversationSettings>.read = _Read_DewConversationSettings;
		Reader<Entity[]>.read = _Read_Entity_005B_005D;
		Reader<ConversationVisibility>.read = _Read_ConversationVisibility;
		Reader<DewGameResult>.read = _Read_DewGameResult;
		Reader<DewGameResult.ResultType>.read = _Read_DewGameResult_002FResultType;
		Reader<List<DewGameResult.PlayerData>>.read = _Read_System_002ECollections_002EGeneric_002EList_00601_003CDewGameResult_002FPlayerData_003E;
		Reader<DewGameResult.PlayerData>.read = _Read_DewGameResult_002FPlayerData;
		Reader<List<DewGameResult.SkillData>>.read = _Read_System_002ECollections_002EGeneric_002EList_00601_003CDewGameResult_002FSkillData_003E;
		Reader<DewGameResult.SkillData>.read = _Read_DewGameResult_002FSkillData;
		Reader<List<DewGameResult.GemData>>.read = _Read_System_002ECollections_002EGeneric_002EList_00601_003CDewGameResult_002FGemData_003E;
		Reader<DewGameResult.GemData>.read = _Read_DewGameResult_002FGemData;
		Reader<List<int>>.read = _Read_System_002ECollections_002EGeneric_002EList_00601_003CSystem_002EInt32_003E;
		Reader<GameState>.read = _Read_GameState;
		Reader<AllowMidJoinType>.read = _Read_AllowMidJoinType;
		Reader<MidJoinBanType>.read = _Read_MidJoinBanType;
		Reader<MidJoinWaitType>.read = _Read_MidJoinWaitType;
		Reader<PingManager.Ping>.read = _Read_PingManager_002FPing;
		Reader<PingManager.PingType>.read = _Read_PingManager_002FPingType;
		Reader<WorldNodeData>.read = _Read_WorldNodeData;
		Reader<WorldNodeType>.read = _Read_WorldNodeType;
		Reader<WorldNodeStatus>.read = _Read_WorldNodeStatus;
		Reader<List<ModifierData>>.read = _Read_System_002ECollections_002EGeneric_002EList_00601_003CModifierData_003E;
		Reader<ModifierData>.read = _Read_ModifierData;
		Reader<HunterStatus>.read = _Read_HunterStatus;
		Reader<EventInfoLoadRoom>.read = _Read_EventInfoLoadRoom;
		Reader<EventInfoLoadZone>.read = _Read_EventInfoLoadZone;
		Reader<VoteType>.read = _Read_VoteType;
		Reader<RoomMonsters.PrewarmEntry[]>.read = _Read_RoomMonsters_002FPrewarmEntry_005B_005D;
		Reader<RoomMonsters.PrewarmEntry>.read = _Read_RoomMonsters_002FPrewarmEntry;
	}
}
