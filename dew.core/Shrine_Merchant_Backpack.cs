using System.Collections;
using UnityEngine;

public class Shrine_Merchant_Backpack : Shrine, ICustomInteractable
{
	public float spawnSkillRatio;

	public float spawnDelay;

	public string nameRawText => DewLocalization.GetUIValue(((object)this).GetType().Name + "_Name");

	public string interactActionRawText => DewLocalization.GetUIValue("InGame_Tooltip_PickUp");

	public Vector3 worldOffset => new Vector3(0f, 3f, 0f);

	protected override bool OnUse(Entity entity)
	{
		((MonoBehaviour)(object)this).StartCoroutine(Routine());
		return true;
		IEnumerator Routine()
		{
			Vector3 pivot = Dew.GetPositionOnGround(((Component)(object)this).transform.position);
			if (Random.value <= spawnSkillRatio)
			{
				yield return new WaitForSeconds(spawnDelay);
				foreach (DewPlayer gamePlayer in DewPlayer.gamePlayers)
				{
					if (!gamePlayer.hero.IsNullInactiveDeadOrKnockedOut())
					{
						Rarity value = NetworkedManagerBase<LootManager>.instance.SelectSkillRarity();
						Vector3 goodRewardPosition = Dew.GetGoodRewardPosition(pivot);
						NetworkedManagerBase<LootManager>.instance.SelectSkillAndLevel(value, out var skill, out var level);
						level += SingletonDewNetworkBehaviour<Room>.instance.rewards.skillBonusLevel;
						Dew.CreateSkillTrigger(skill, goodRewardPosition, level, gamePlayer);
						yield return null;
					}
				}
			}
			else
			{
				yield return new WaitForSeconds(spawnDelay);
				foreach (DewPlayer gamePlayer2 in DewPlayer.gamePlayers)
				{
					if (!gamePlayer2.hero.IsNullInactiveDeadOrKnockedOut())
					{
						Rarity value2 = NetworkedManagerBase<LootManager>.instance.SelectGemRarity();
						Vector3 goodRewardPosition2 = Dew.GetGoodRewardPosition(pivot);
						NetworkedManagerBase<LootManager>.instance.SelectGemAndQuality(value2, out var gem, out var quality);
						quality += SingletonDewNetworkBehaviour<Room>.instance.rewards.gemBonusQuality;
						Dew.CreateGem(gem, goodRewardPosition2, quality, gamePlayer2);
						yield return null;
					}
				}
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
