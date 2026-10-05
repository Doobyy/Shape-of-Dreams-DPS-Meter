using System;
using System.Collections;
using UnityEngine;

public class Tut_World : DewInGameTutorialItem
{
	public override void OnStart()
	{
		base.OnStart();
		InGameUIManager instance = InGameUIManager.instance;
		instance.onWorldDisplayedChanged = (Action<WorldDisplayStatus>)Delegate.Combine(instance.onWorldDisplayedChanged, new Action<WorldDisplayStatus>(OnWorldDisplayedChanged));
		NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnRoomLoadStarted += new Action<EventInfoLoadRoom>(ClientEventOnRoomLoadStarted);
	}

	private void OnWorldDisplayedChanged(WorldDisplayStatus obj)
	{
		TutGetCoroutiner().StopAllCoroutines();
		TutGetCoroutiner().StartCoroutine(Routine());
		IEnumerator Routine()
		{
			yield return new WaitForSeconds(0.5f);
			if (InGameUIManager.instance.isWorldDisplayed != WorldDisplayStatus.None)
			{
				int currentIndex = NetworkedManagerBase<ZoneManager>.instance.currentNodeIndex;
				int exitIndex = NetworkedManagerBase<ZoneManager>.instance.nodes.FindIndex((Predicate<WorldNodeData>)((WorldNodeData n) => n.type == WorldNodeType.ExitBoss));
				for (int num = 0; num < NetworkedManagerBase<ZoneManager>.instance.nodes.Count && !NetworkedManagerBase<ZoneManager>.instance.IsNodeConnected(currentIndex, num); num++)
				{
				}
				TutCreateArrow().SetLocalizedText("Tut_World_CurrentRoom").SetBoxPlacement(BoxPlacementMode.AwayFromCenter).SetDuration(6f)
					.FollowUIElement(InGameUIManager.instance.fullWorldMapNodeItems[currentIndex])
					.SetDestroyCondition(() => InGameUIManager.instance.isWorldDisplayed == WorldDisplayStatus.None);
				yield return new WaitForSeconds(2f);
				if (InGameUIManager.instance.isWorldDisplayed != WorldDisplayStatus.None)
				{
					TutCreateArrow().SetLocalizedText("Tut_World_WorldExit").SetBoxPlacement(BoxPlacementMode.AwayFromCenter).SetDuration(8f)
						.FollowUIElement(InGameUIManager.instance.fullWorldMapNodeItems[exitIndex])
						.SetDestroyCondition(() => InGameUIManager.instance.isWorldDisplayed == WorldDisplayStatus.None);
					yield return new WaitForSeconds(5f);
					if (InGameUIManager.instance.isWorldDisplayed == WorldDisplayStatus.Shown)
					{
						TutCreateArrow().SetLocalizedText("Tut_World_ClickOnAdjacentRooms").SetBoxPlacement(BoxPlacementMode.AwayFromCenter).SetDuration(12f)
							.FollowUIElement(InGameUIManager.instance.fullWorldMapNodeItems[currentIndex])
							.SetDestroyCondition(() => InGameUIManager.instance.isWorldDisplayed == WorldDisplayStatus.None);
					}
				}
			}
		}
	}

	private void ClientEventOnRoomLoadStarted(EventInfoLoadRoom _)
	{
		TutComplete();
	}

	public override void OnStop()
	{
		base.OnStop();
		if (InGameUIManager.instance != null)
		{
			InGameUIManager instance = InGameUIManager.instance;
			instance.onWorldDisplayedChanged = (Action<WorldDisplayStatus>)Delegate.Remove(instance.onWorldDisplayedChanged, new Action<WorldDisplayStatus>(OnWorldDisplayedChanged));
		}
		if ((UnityEngine.Object)(object)NetworkedManagerBase<ZoneManager>.instance != null)
		{
			NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnRoomLoadStarted -= new Action<EventInfoLoadRoom>(ClientEventOnRoomLoadStarted);
		}
	}
}
