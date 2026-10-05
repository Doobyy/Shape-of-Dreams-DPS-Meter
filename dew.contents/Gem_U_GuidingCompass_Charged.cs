using System;
using System.Collections;
using Mirror;
using UnityEngine;

public class Gem_U_GuidingCompass_Charged : Gem
{
	public GameObject fxQuestActivate;

	public float questActivateDelay = 1f;

	public int riftTravelCooldownAfterQuestActivate = 8;

	[SaveVar(SaveVarFlags.Default)]
	private int _cooldown;

	public override bool isDroppedOnOwnerDisconnect => true;

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			Se_Curse_GuidingCompass_CripplingAnxiety.LiftAllIfCompassMissingDelayed();
		}
	}

	public override void OnEquipGem(Hero newOwner)
	{
		base.OnEquipGem(newOwner);
		if (((NetworkBehaviour)this).isServer)
		{
			NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnRoomLoaded += new Action<EventInfoLoadRoom>(ClientEventOnRoomLoaded);
		}
	}

	private void ClientEventOnRoomLoaded(EventInfoLoadRoom obj)
	{
		if (!obj.isTraveling || obj.isSidetrackTransition || (UnityEngine.Object)(object)NetworkedManagerBase<QuestManager>.instance.activeQuests.Find((DewQuest q) => q is Quest_GuidingCompass) != null || NetworkedManagerBase<ZoneManager>.instance.currentZone.useSpecialGeneration || NetworkedManagerBase<ZoneManager>.instance.currentNode.type == WorldNodeType.ExitBoss || NetworkedManagerBase<ZoneManager>.instance.isSidetracking)
		{
			return;
		}
		GameMod_StarlessPath gameMod_StarlessPath = Dew.FindActorOfType<GameMod_StarlessPath>();
		if (!gameMod_StarlessPath.IsNullOrInactive() && gameMod_StarlessPath.didMeetPolaris)
		{
			return;
		}
		if (_cooldown > 0)
		{
			_cooldown--;
			return;
		}
		NetworkedManagerBase<ZoneManager>.instance.CallOnReadyAfterTransition(() =>
		{
			if (isValid)
			{
				((MonoBehaviour)(object)this).StartCoroutine(Routine());
			}
		});
		IEnumerator Routine()
		{
			FxPlayNetworked(fxQuestActivate, owner);
			yield return new WaitForSeconds(questActivateDelay);
			NetworkedManagerBase<QuestManager>.instance.StartQuest<Quest_GuidingCompass>();
			_cooldown = riftTravelCooldownAfterQuestActivate;
		}
	}

	public override void OnUnequipGem(Hero oldOwner)
	{
		base.OnUnequipGem(oldOwner);
		if (((NetworkBehaviour)this).isServer && (bool)(UnityEngine.Object)(object)NetworkedManagerBase<ZoneManager>.instance)
		{
			NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnRoomLoaded -= new Action<EventInfoLoadRoom>(ClientEventOnRoomLoaded);
		}
	}

	private void MirrorProcessed()
	{
	}
}
