using System;
using System.Collections;
using Mirror;
using UnityEngine;

public class Tut_Cleanse : DewInGameTutorialItem
{
	private bool _didShowArrow;

	public override void OnStart()
	{
		base.OnStart();
		EditSkillManager instance = ManagerBase<EditSkillManager>.instance;
		instance.onModeChanged = (Action<EditSkillManager.ModeType>)Delegate.Combine(instance.onModeChanged, new Action<EditSkillManager.ModeType>(OnModeChanged));
		NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnRoomLoadStarted += new Action<EventInfoLoadRoom>(ClientEventOnRoomLoadStarted);
		NetworkedManagerBase<ClientEventManager>.instance.OnItemCleansed += new Action<Hero, NetworkBehaviour>(ItemCleansed);
	}

	private void OnModeChanged(EditSkillManager.ModeType obj)
	{
		TutGetCoroutiner().StopAllCoroutines();
		TutGetCoroutiner().StartCoroutine(Routine());
		IEnumerator Routine()
		{
			yield return new WaitForSeconds(0.5f);
			if (ManagerBase<EditSkillManager>.instance.currentProvider is Shrine_AltarOfCleansing && ManagerBase<EditSkillManager>.instance.mode == EditSkillManager.ModeType.EditSkillShrine)
			{
				_didShowArrow = true;
				TutCreateArrow().SetLocalizedText("Tut_Cleanse").SetDuration(8f).SetDestroyCondition(() => ManagerBase<EditSkillManager>.instance.mode != EditSkillManager.ModeType.EditSkillShrine)
					.FollowWorldTarget(((Component)(object)ManagerBase<EditSkillManager>.instance.currentProvider).transform);
			}
		}
	}

	private void ItemCleansed(Hero arg1, NetworkBehaviour arg2)
	{
		if (((NetworkBehaviour)arg1).isOwned)
		{
			TutComplete();
		}
	}

	private void ClientEventOnRoomLoadStarted(EventInfoLoadRoom obj)
	{
		if (_didShowArrow)
		{
			TutComplete();
		}
	}

	public override void OnStop()
	{
		base.OnStop();
		if (ManagerBase<EditSkillManager>.instance != null)
		{
			EditSkillManager instance = ManagerBase<EditSkillManager>.instance;
			instance.onModeChanged = (Action<EditSkillManager.ModeType>)Delegate.Remove(instance.onModeChanged, new Action<EditSkillManager.ModeType>(OnModeChanged));
		}
		if ((UnityEngine.Object)(object)NetworkedManagerBase<ZoneManager>.instance != null)
		{
			NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnRoomLoadStarted -= new Action<EventInfoLoadRoom>(ClientEventOnRoomLoadStarted);
		}
		if ((UnityEngine.Object)(object)NetworkedManagerBase<ClientEventManager>.instance != null)
		{
			NetworkedManagerBase<ClientEventManager>.instance.OnItemCleansed -= new Action<Hero, NetworkBehaviour>(ItemCleansed);
		}
	}
}
