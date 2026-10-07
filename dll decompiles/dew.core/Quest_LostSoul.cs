using System;
using System.Linq;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Quest_LostSoul : DewQuest
{
	[NonSerialized]
	[SyncVar]
	[SaveVar(SaveVarFlags.Default)]
	public Hero targetHero;

	protected NetworkBehaviourSyncVar ___targetHeroNetId;

	public Hero NetworktargetHero
	{
		get
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			return ((NetworkBehaviour)this).GetSyncVarNetworkBehaviour<Hero>(___targetHeroNetId, ref targetHero);
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter_NetworkBehaviour<Hero>(value, ref targetHero, 256uL, (Action<Hero, Hero>)null, ref ___targetHeroNetId);
		}
	}

	public override string GetInitialStep()
	{
		return GetAppropriateStep();
	}

	private string GetAppropriateStep()
	{
		if (NetworkedManagerBase<ZoneManager>.instance.currentNodeIndex != GetSoulNodeIndex())
		{
			return "MoveToSoul";
		}
		return "SaveSoul";
	}

	private int GetSoulNodeIndex()
	{
		return NetworkedManagerBase<ZoneManager>.instance.nodes.FindIndex((Predicate<WorldNodeData>)((WorldNodeData n) => n.modifiers.Any((ModifierData m) => m.type == "RoomMod_HeroSoul" && m.clientData == NetworktargetHero.owner.guid)));
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		if (NetworktargetHero.IsNullOrInactive())
		{
			if (((NetworkBehaviour)this).isServer)
			{
				DestroyIfActive();
			}
		}
		else if (((NetworkBehaviour)this).isServer)
		{
			if (!NetworktargetHero.isKnockedOut)
			{
				Destroy();
				return;
			}
			NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnRoomLoaded += new Action<EventInfoLoadRoom>(ClientEventOnRoomLoaded);
			NetworktargetHero.Status.GetStatusEffect<Se_HeroKnockedOut>().ClientActorEvent_OnDestroyed += new Action<Actor>(ClientActorEventOnDestroyed);
		}
	}

	private void ClientActorEventOnDestroyed(Actor obj)
	{
		if (NetworktargetHero.IsNullOrInactive())
		{
			Destroy();
		}
		else
		{
			CompleteQuest();
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)NetworkedManagerBase<ZoneManager>.instance != null)
		{
			NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnRoomLoaded -= new Action<EventInfoLoadRoom>(ClientEventOnRoomLoaded);
		}
	}

	private void ClientEventOnRoomLoaded(EventInfoLoadRoom obj)
	{
		string appropriateStep = GetAppropriateStep();
		if (step != appropriateStep)
		{
			EndStep(appropriateStep);
		}
	}

	protected override void OnStepStarted(bool isLoadedFromSave)
	{
		base.OnStepStarted(isLoadedFromSave);
		if (!((UnityEngine.Object)(object)NetworktargetHero == null) && !((UnityEngine.Object)(object)NetworktargetHero.owner == null))
		{
			string describedPlayerName = ChatManager.GetDescribedPlayerName(NetworktargetHero.owner);
			questTitleRaw = string.Format(DewLocalization.GetUIValue("Quest_LostSoul_Title"), describedPlayerName);
			if (step == "SaveSoul")
			{
				questShortDescriptionRaw = string.Format(DewLocalization.GetUIValue("Quest_LostSoul_Description_FindAndSave"), describedPlayerName);
				questDetailedDescriptionRaw = string.Format(DewLocalization.GetUIValue("Quest_LostSoul_Tooltip_FindAndSave"), describedPlayerName);
			}
			else if (step == "MoveToSoul")
			{
				questShortDescriptionRaw = string.Format(DewLocalization.GetUIValue("Quest_LostSoul_Description_Move"), describedPlayerName);
				questDetailedDescriptionRaw = string.Format(DewLocalization.GetUIValue("Quest_LostSoul_Tooltip_Move"), describedPlayerName);
			}
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
			NetworkWriterExtensions.WriteNetworkBehaviour(writer, (NetworkBehaviour)(object)NetworktargetHero);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x100L) != 0L)
		{
			NetworkWriterExtensions.WriteNetworkBehaviour(writer, (NetworkBehaviour)(object)NetworktargetHero);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize_NetworkBehaviour<Hero>(ref targetHero, (Action<Hero, Hero>)null, reader, ref ___targetHeroNetId);
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x100L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize_NetworkBehaviour<Hero>(ref targetHero, (Action<Hero, Hero>)null, reader, ref ___targetHeroNetId);
		}
	}
}
