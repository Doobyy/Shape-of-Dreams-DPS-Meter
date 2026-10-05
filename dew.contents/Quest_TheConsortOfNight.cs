using System;
using Mirror;
using UnityEngine;

public class Quest_TheConsortOfNight : DewQuest
{
	private Ge_TheConsortOfNight _ge;

	protected override void OnStepStarted(bool isLoadedFromSave)
	{
		base.OnStepStarted(isLoadedFromSave);
		questTitleRaw = DewLocalization.GetUIValue("Quest_TheConsortOfNight_Title");
		questDetailedDescriptionRaw = DewLocalization.GetUIValue("Quest_TheConsortOfNight_Tooltip");
		questShortDescriptionRaw = DewLocalization.GetUIValue("Quest_TheConsortOfNight_Description");
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		Dew.CallDelayed(() =>
		{
			_ge = Dew.FindActorOfType<Ge_TheConsortOfNight>();
			if ((UnityEngine.Object)(object)_ge == null)
			{
				_ge = Dew.CreateActor<Ge_TheConsortOfNight>();
			}
			_ge.Event_OnCountIncrease += new Action(UpdateProgress);
			UpdateProgress();
		});
		NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnRoomLoaded += new Action<EventInfoLoadRoom>(ClientEventOnRoomLoaded);
		progressType = QuestProgressType.Collect;
		if (isLoadedFromSave)
		{
			return;
		}
		for (int num = 0; num < NetworkedManagerBase<ZoneManager>.instance.nodes.Count; num++)
		{
			WorldNodeData worldNodeData = NetworkedManagerBase<ZoneManager>.instance.nodes[num];
			if (num != NetworkedManagerBase<ZoneManager>.instance.currentNodeIndex && worldNodeData.HasModifier<RoomMod_TheConsortOfNight_Raven>())
			{
				NetworkedManagerBase<ZoneManager>.instance.AddModifier<RoomMod_TheConsortOfNight_Raven_Marker>(num);
			}
		}
	}

	protected override void OnStepEnded(string formerStep)
	{
		base.OnStepEnded(formerStep);
		if (((NetworkBehaviour)this).isServer && !((UnityEngine.Object)(object)NetworkedManagerBase<ZoneManager>.instance == null))
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
			if (NetworkedManagerBase<ZoneManager>.instance.currentNode.type == WorldNodeType.ExitBoss && _ge.count != _ge.countForAction)
			{
				_ge.Networkcount = 0;
				FailQuest(FailReason.NotSpecified);
			}
		});
	}

	private void UpdateProgress()
	{
		if (_ge.count == _ge.countForAction)
		{
			NetworkedManagerBase<ChatManager>.instance.BroadcastMessage(new ChatManager.Message
			{
				type = ChatManager.MessageType.Notice,
				content = "Chat_Notice_ErebosHiddenChallengeActivated"
			});
			CompleteQuest();
		}
		currentProgress = $"{_ge.count} / {_ge.countForAction}";
	}

	private void MirrorProcessed()
	{
	}
}
