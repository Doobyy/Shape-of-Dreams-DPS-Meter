using System;
using System.Collections;
using Mirror;
using UnityEngine;

public class Tut_ShopSell : DewInGameTutorialItem
{
	private bool _didShowArrow;

	public override void OnStart()
	{
		base.OnStart();
		FloatingWindowManager instance = ManagerBase<FloatingWindowManager>.instance;
		instance.onTargetChanged = (Action<MonoBehaviour>)Delegate.Combine(instance.onTargetChanged, new Action<MonoBehaviour>(OnTargetChanged));
		NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnRoomLoadStarted += new Action<EventInfoLoadRoom>(ClientEventOnRoomLoadStarted);
		NetworkedManagerBase<ClientEventManager>.instance.OnItemSold += new Action<Hero, NetworkBehaviour>(ItemSold);
	}

	private void ItemSold(Hero arg1, NetworkBehaviour arg2)
	{
		if (((NetworkBehaviour)arg1).isOwned)
		{
			TutComplete();
		}
	}

	private void ClientEventOnRoomLoadStarted(EventInfoLoadRoom _)
	{
		if (_didShowArrow)
		{
			TutComplete();
		}
	}

	private void OnTargetChanged(MonoBehaviour obj)
	{
		TutGetCoroutiner().StopAllCoroutines();
		TutGetCoroutiner().StartCoroutine(Routine());
		IEnumerator Routine()
		{
			yield return new WaitForSeconds(1.5f);
			if (ManagerBase<FloatingWindowManager>.instance.currentTarget is PropEnt_Merchant_Base { isSellEnabled: not false })
			{
				Transform transform = null;
				if ((UnityEngine.Object)(object)DewPlayer.local.hero.Skill.W != null)
				{
					transform = ManagerBase<InGameTutorialManager>.instance.refSkillButtonW;
				}
				else if ((UnityEngine.Object)(object)DewPlayer.local.hero.Skill.E != null)
				{
					transform = ManagerBase<InGameTutorialManager>.instance.refSkillButtonE;
				}
				else if ((UnityEngine.Object)(object)DewPlayer.local.hero.Skill.Q != null)
				{
					transform = ManagerBase<InGameTutorialManager>.instance.refSkillButtonQ;
				}
				else if ((UnityEngine.Object)(object)DewPlayer.local.hero.Skill.R != null)
				{
					transform = ManagerBase<InGameTutorialManager>.instance.refSkillButtonR;
				}
				if (!(transform == null))
				{
					_didShowArrow = true;
					TutCreateArrow().SetLocalizedText("Tut_ShopSell").SetDuration(8f).SetDestroyCondition(() => !(ManagerBase<FloatingWindowManager>.instance.currentTarget is PropEnt_Merchant_Base))
						.FollowUIElement(transform);
				}
			}
		}
	}

	public override void OnStop()
	{
		base.OnStop();
		if (ManagerBase<FloatingWindowManager>.instance != null)
		{
			FloatingWindowManager instance = ManagerBase<FloatingWindowManager>.instance;
			instance.onTargetChanged = (Action<MonoBehaviour>)Delegate.Remove(instance.onTargetChanged, new Action<MonoBehaviour>(OnTargetChanged));
		}
		if ((UnityEngine.Object)(object)NetworkedManagerBase<ZoneManager>.instance != null)
		{
			NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnRoomLoadStarted -= new Action<EventInfoLoadRoom>(ClientEventOnRoomLoadStarted);
		}
		if ((UnityEngine.Object)(object)NetworkedManagerBase<ClientEventManager>.instance != null)
		{
			NetworkedManagerBase<ClientEventManager>.instance.OnItemSold -= new Action<Hero, NetworkBehaviour>(ItemSold);
		}
	}
}
