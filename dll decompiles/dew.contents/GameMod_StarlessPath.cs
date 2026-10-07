using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class GameMod_StarlessPath : GameModifierBase
{
	public float invitationChanceOnJonasFlee = 0.02f;

	public float invitationChanceOnJonasKill = 0.05f;

	[CompilerGenerated]
	[SyncVar]
	private int lastJonasChamberVisitZoneIndex__BackingField = -1;

	[CompilerGenerated]
	[SyncVar]
	private bool didMeetPolaris__BackingField;

	[SaveVar(SaveVarFlags.Default)]
	public int lastJonasChamberVisitZoneIndex
	{
		[CompilerGenerated]
		get
		{
			return lastJonasChamberVisitZoneIndex__BackingField;
		}
		[CompilerGenerated]
		private set
		{
			Network_003ClastJonasChamberVisitZoneIndex_003Ek__BackingField = value;
		}
	}

	[SaveVar(SaveVarFlags.Default)]
	public bool didMeetPolaris
	{
		[CompilerGenerated]
		get
		{
			return didMeetPolaris__BackingField;
		}
		[CompilerGenerated]
		private set
		{
			Network_003CdidMeetPolaris_003Ek__BackingField = value;
		}
	}

	public int Network_003ClastJonasChamberVisitZoneIndex_003Ek__BackingField
	{
		get
		{
			return lastJonasChamberVisitZoneIndex__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<int>(value, ref lastJonasChamberVisitZoneIndex__BackingField, 8uL, (Action<int, int>)null);
		}
	}

	public bool Network_003CdidMeetPolaris_003Ek__BackingField
	{
		get
		{
			return didMeetPolaris__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<bool>(value, ref didMeetPolaris__BackingField, 16uL, (Action<bool, bool>)null);
		}
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			NetworkedManagerBase<ActorManager>.instance.ClientEvent_OnActorAdd += new Action<Actor>(ClientEventOnActorAdd);
			NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnRoomLoaded += new Action<EventInfoLoadRoom>(ClientEventOnRoomLoaded);
		}
	}

	private void ClientEventOnRoomLoaded(EventInfoLoadRoom obj)
	{
		if (((UnityEngine.Object)(object)SingletonDewNetworkBehaviour<Room>.instance).name == "Room_Special_TheChamberOfJonas")
		{
			Network_003ClastJonasChamberVisitZoneIndex_003Ek__BackingField = NetworkedManagerBase<ZoneManager>.instance.currentZoneIndex;
		}
		if (((UnityEngine.Object)(object)SingletonDewNetworkBehaviour<Room>.instance).name == "Room_Special_StarlessPath_BossPolaris")
		{
			Network_003CdidMeetPolaris_003Ek__BackingField = true;
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			if ((bool)(UnityEngine.Object)(object)NetworkedManagerBase<ActorManager>.instance)
			{
				NetworkedManagerBase<ActorManager>.instance.ClientEvent_OnActorAdd -= new Action<Actor>(ClientEventOnActorAdd);
			}
			if ((bool)(UnityEngine.Object)(object)NetworkedManagerBase<ZoneManager>.instance)
			{
				NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnRoomLoaded -= new Action<EventInfoLoadRoom>(ClientEventOnRoomLoaded);
			}
		}
	}

	private bool IsPolarisEndingEligible()
	{
		return DewPlayer.gamePlayers.Any((DewPlayer p) => p.hasPolarisEndingUnlocked);
	}

	private void ClientEventOnActorAdd(Actor obj)
	{
		PropEnt_Merchant_Jonas jonas = obj as PropEnt_Merchant_Jonas;
		if (jonas != null && UnityEngine.Random.value < invitationChanceOnJonasKill && IsPolarisEndingEligible())
		{
			jonas.EntityEvent_OnDeath += (Action<EventInfoKill>)((EventInfoKill _) =>
			{
				Dew.CreateActor<Shrine_InvitationOfJonasMeeting>(jonas.agentPosition, null);
			});
		}
		if (obj is Ai_Prop_Merchant_Flee ai_Prop_Merchant_Flee && UnityEngine.Random.value < invitationChanceOnJonasFlee && IsPolarisEndingEligible())
		{
			Dew.CreateActor<Shrine_InvitationOfJonasMeeting>(ai_Prop_Merchant_Flee.info.caster.agentPosition, null);
		}
	}

	private void MirrorProcessed()
	{
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			NetworkWriterExtensions.WriteInt(writer, lastJonasChamberVisitZoneIndex__BackingField);
			NetworkWriterExtensions.WriteBool(writer, didMeetPolaris__BackingField);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 8L) != 0L)
		{
			NetworkWriterExtensions.WriteInt(writer, lastJonasChamberVisitZoneIndex__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x10L) != 0L)
		{
			NetworkWriterExtensions.WriteBool(writer, didMeetPolaris__BackingField);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref lastJonasChamberVisitZoneIndex__BackingField, (Action<int, int>)null, NetworkReaderExtensions.ReadInt(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref didMeetPolaris__BackingField, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 8L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref lastJonasChamberVisitZoneIndex__BackingField, (Action<int, int>)null, NetworkReaderExtensions.ReadInt(reader));
		}
		if ((num & 0x10L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref didMeetPolaris__BackingField, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
		}
	}
}
