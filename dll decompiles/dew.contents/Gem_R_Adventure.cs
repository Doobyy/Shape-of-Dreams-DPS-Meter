using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Gem_R_Adventure : Gem
{
	public GameObject fxReward;

	public ScalingValue upgradeGemAmount;

	public ScalingValue goldAmount;

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
		if (!obj.isTraveling || SingletonDewNetworkBehaviour<Room>.instance.isRevisit)
		{
			return;
		}
		NetworkedManagerBase<ZoneManager>.instance.CallOnReadyAfterTransition(() =>
		{
			if (isValid && !owner.IsNullInactiveDeadOrKnockedOut())
			{
				FxPlayNetworked(fxReward, owner);
				if (owner.Skill.TryGetGemLocation(this, out var gemLocation))
				{
					List<Gem> list = new List<Gem>();
					foreach (KeyValuePair<GemLocation, Gem> gem in owner.Skill.gems)
					{
						if (gem.Key.skill == gemLocation.skill && !((UnityEngine.Object)(object)gem.Value == (UnityEngine.Object)(object)this))
						{
							list.Add(gem.Value);
						}
					}
					if (list.Count > 0)
					{
						list[UnityEngine.Random.Range(0, list.Count)].quality += Mathf.RoundToInt(GetValue(upgradeGemAmount));
					}
				}
				NetworkedManagerBase<PickupManager>.instance.DropGold(isKillGold: false, isGivenByOtherPlayer: false, Mathf.RoundToInt(GetValue(goldAmount)), owner.position, owner);
				NotifyUse();
			}
		});
	}

	public override void OnUnequipGem(Hero oldOwner)
	{
		base.OnUnequipGem(oldOwner);
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)NetworkedManagerBase<ZoneManager>.instance != null)
		{
			NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnRoomLoaded -= new Action<EventInfoLoadRoom>(ClientEventOnRoomLoaded);
		}
	}

	private void MirrorProcessed()
	{
	}
}
