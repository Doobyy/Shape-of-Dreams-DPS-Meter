using System;
using System.Collections;
using UnityEngine;

public class Tut_WorldHunt : DewInGameTutorialItem
{
	private bool _didShowHuntHelp;

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
				int num = NetworkedManagerBase<ZoneManager>.instance.hunterStatuses.FindIndex((Predicate<HunterStatus>)((HunterStatus s) => s != HunterStatus.None));
				if (num >= 0)
				{
					_didShowHuntHelp = true;
					TutCreateArrow().SetLocalizedText("Tut_WorldHunt_Hunt").SetBoxPlacement(BoxPlacementMode.AwayFromCenter).SetDuration(10f)
						.FollowUIElement(InGameUIManager.instance.fullWorldMapNodeItems[num])
						.SetDestroyCondition(() => InGameUIManager.instance.isWorldDisplayed == WorldDisplayStatus.None);
				}
			}
		}
	}

	private void ClientEventOnRoomLoadStarted(EventInfoLoadRoom _)
	{
		if (_didShowHuntHelp)
		{
			TutComplete();
		}
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
