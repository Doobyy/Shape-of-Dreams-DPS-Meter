using System.Collections;
using Mirror;
using UnityEngine;

public class Gem_R_Wealth : Gem
{
	public GameObject goldSpawnEffect;

	public float gracePeriod = 1.5f;

	public ScalingValue killGold;

	public float initDelay = 0.2f;

	public float interval = 0.2f;

	public int amountPerDrop = 5;

	private KillTracker _tracker;

	public override void OnEquipSkill(SkillTrigger newSkill)
	{
		base.OnEquipSkill(newSkill);
		if (((NetworkBehaviour)this).isServer)
		{
			_tracker = newSkill.TrackKills(gracePeriod, Callback);
		}
	}

	private void Callback(EventInfoKill obj)
	{
		Vector3 pos;
		if (obj.victim is Monster monster)
		{
			pos = monster.position;
			((MonoBehaviour)(object)this).StartCoroutine(DropGoldRoutine());
		}
		IEnumerator DropGoldRoutine()
		{
			int amount = DewMath.RandomRoundToInt(GetValue(killGold));
			int adjustedAmountPerDrop = amountPerDrop;
			if ((float)adjustedAmountPerDrop < (float)amount / 3f)
			{
				adjustedAmountPerDrop = Mathf.RoundToInt((float)amount / 3f);
			}
			yield return new WaitForSeconds(initDelay);
			while (amount > 0 && !((Object)(object)owner == null))
			{
				if (amount > adjustedAmountPerDrop)
				{
					NetworkedManagerBase<PickupManager>.instance.DropGold(isKillGold: false, isGivenByOtherPlayer: false, adjustedAmountPerDrop, pos);
				}
				else
				{
					NetworkedManagerBase<PickupManager>.instance.DropGold(isKillGold: false, isGivenByOtherPlayer: false, amount, pos);
				}
				amount -= adjustedAmountPerDrop;
				FxPlayNewNetworked(goldSpawnEffect, pos, Quaternion.identity);
				NotifyUse();
				yield return new WaitForSeconds(interval);
			}
		}
	}

	public override void OnUnequipSkill(SkillTrigger oldSkill)
	{
		base.OnUnequipSkill(oldSkill);
		if (((NetworkBehaviour)this).isServer)
		{
			_tracker.Stop();
		}
	}

	private void MirrorProcessed()
	{
	}
}
