using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shrine_Disintegration : Shrine
{
	public float goldMultiplier;

	public float addedMultiplierPerUse;

	public Dictionary<Entity, float> dreamDustRewardees = new Dictionary<Entity, float>();

	public override bool isRegularReward => true;

	protected override bool OnUse(Entity entity)
	{
		int amount = DewMath.RandomRoundToInt(NetworkedManagerBase<GameManager>.instance.GetSpecialRewardAmount_Gold() * goldMultiplier * (1f + addedMultiplierPerUse * (float)(totalUseCount - 1)));
		((MonoBehaviour)(object)this).StartCoroutine(Routine());
		return true;
		IEnumerator Routine()
		{
			for (int i = 0; i < 5; i++)
			{
				if (dreamDustRewardees.TryGetValue(entity, out var value))
				{
					NetworkedManagerBase<PickupManager>.instance.DropDreamDust(isGivenByOtherPlayer: false, DewMath.RandomRoundToInt((float)amount / 5f * value), entity.position, (Hero)entity);
				}
				NetworkedManagerBase<PickupManager>.instance.DropGold(isKillGold: false, isGivenByOtherPlayer: false, DewMath.RandomRoundToInt((float)amount / 5f), entity.position, (Hero)entity);
				yield return new WaitForSeconds(0.25f);
			}
		}
	}

	public override Cost? GetCost(Entity activator)
	{
		return Cost.HealthPercentage(Mathf.RoundToInt(Mathf.Min(90f, 25f + (float)totalUseCount * 15f)));
	}

	private void MirrorProcessed()
	{
	}
}
