using System;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Quest_HuntedByObliviax : DewQuest
{
	public GameObject fxStart;

	[SaveVar(SaveVarFlags.Default)]
	[SyncVar(hook = "OnRemainingTurnsChanged")]
	private int _remainingTurns;

	public Action<int, int> _Mirror_SyncVarHookDelegate__remainingTurns;

	public int Network_remainingTurns
	{
		get
		{
			return _remainingTurns;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<int>(value, ref _remainingTurns, 256uL, _Mirror_SyncVarHookDelegate__remainingTurns);
		}
	}

	private void OnRemainingTurnsChanged(int _, int __)
	{
		UpdateQuestText();
	}

	protected override void OnPrepare()
	{
		base.OnPrepare();
		if (isNewInstance)
		{
			Network_remainingTurns = NetworkedManagerBase<ZoneManager>.instance.GetNodeDistance(NetworkedManagerBase<ZoneManager>.instance.currentNodeIndex, NetworkedManagerBase<ZoneManager>.instance.nodes.FindIndex((Predicate<WorldNodeData>)((WorldNodeData w) => w.type == WorldNodeType.ExitBoss))) + 1;
			Network_remainingTurns = Mathf.Clamp(_remainingTurns, 2, 5);
		}
	}

	protected override void OnStepStarted(bool isLoadedFromSave)
	{
		base.OnStepStarted(isLoadedFromSave);
		questTitleRaw = DewLocalization.GetUIValue("Quest_HuntedByObliviax_Title");
		UpdateQuestText();
		if (!isLoadedFromSave)
		{
			FxPlay(fxStart);
		}
		if (((NetworkBehaviour)this).isServer)
		{
			NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnRoomLoaded += new Action<EventInfoLoadRoom>(ClientEventOnRoomLoaded);
		}
	}

	protected override void OnStepEnded(string formerStep)
	{
		base.OnStepEnded(formerStep);
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)NetworkedManagerBase<ZoneManager>.instance != null)
		{
			NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnRoomLoaded -= new Action<EventInfoLoadRoom>(ClientEventOnRoomLoaded);
		}
	}

	private void ClientEventOnRoomLoaded(EventInfoLoadRoom obj)
	{
		if (!obj.isTraveling || obj.isSidetrackTransition)
		{
			return;
		}
		NetworkedManagerBase<ZoneManager>.instance.CallOnReadyAfterTransition(() =>
		{
			Network_remainingTurns = _remainingTurns - 1;
			GameMod_Obliviax gameMod_Obliviax = Dew.FindActorOfType<GameMod_Obliviax>();
			if (NetworkedManagerBase<ZoneManager>.instance.currentNode.type == WorldNodeType.ExitBoss || (NetworkedManagerBase<ZoneManager>.instance.currentNode.type == WorldNodeType.Start && !SingletonDewNetworkBehaviour<Room>.instance.isRevisit))
			{
				if ((UnityEngine.Object)(object)gameMod_Obliviax != null)
				{
					gameMod_Obliviax.immunityTurns = UnityEngine.Random.Range(2, 4);
				}
				CompleteQuest();
			}
			else if (_remainingTurns <= 0)
			{
				if ((UnityEngine.Object)(object)gameMod_Obliviax != null)
				{
					gameMod_Obliviax.immunityTurns = int.MaxValue;
				}
				FailQuest(FailReason.NotSpecified);
				Dew.CreateActor<Ge_Obliviax_InterruptTravelAndKidnap>();
			}
		});
	}

	private void UpdateQuestText()
	{
		if (NetworkedManagerBase<ZoneManager>.instance.currentNode.type != WorldNodeType.ExitBoss)
		{
			if (_remainingTurns <= 0)
			{
				questShortDescriptionRaw = DewLocalization.GetUIValue("Quest_HuntedByObliviax_Description_SheIsComingForYou");
				questDetailedDescriptionRaw = DewLocalization.GetUIValue("Quest_HuntedByObliviax_Description_SheIsComingForYou");
			}
			else
			{
				questShortDescriptionRaw = string.Format(DewLocalization.GetUIValue("Quest_HuntedByObliviax_Description"), _remainingTurns);
				questDetailedDescriptionRaw = string.Format(DewLocalization.GetUIValue("Quest_HuntedByObliviax_Tooltip"), _remainingTurns);
			}
		}
	}

	public Quest_HuntedByObliviax()
	{
		_Mirror_SyncVarHookDelegate__remainingTurns = OnRemainingTurnsChanged;
	}

	private void MirrorProcessed()
	{
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			NetworkWriterExtensions.WriteInt(writer, _remainingTurns);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x100L) != 0L)
		{
			NetworkWriterExtensions.WriteInt(writer, _remainingTurns);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref _remainingTurns, _Mirror_SyncVarHookDelegate__remainingTurns, NetworkReaderExtensions.ReadInt(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x100L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref _remainingTurns, _Mirror_SyncVarHookDelegate__remainingTurns, NetworkReaderExtensions.ReadInt(reader));
		}
	}
}
