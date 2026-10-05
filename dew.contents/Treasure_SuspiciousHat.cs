using Mirror;
using UnityEngine;

public class Treasure_SuspiciousHat : Treasure
{
	public Rarity rarity;

	public bool isRewardSkill => customData.StartsWith("S");

	public int rewardLevel => int.Parse(customData.Substring(1));

	public override void OnAddMerchandise(out Cost mercPrice, out string customData)
	{
		base.OnAddMerchandise(out mercPrice, out customData);
		float num = 1f;
		if (Random.value < 0.5f)
		{
			NetworkedManagerBase<LootManager>.instance.SelectSkillAndLevel(rarity, out var _, out var level);
			customData = "S" + level;
			mercPrice = new Cost
			{
				gold = Mathf.RoundToInt((float)SkillTrigger.GetBuyGold(rarity, level) * num)
			};
		}
		else
		{
			NetworkedManagerBase<LootManager>.instance.SelectGemAndQuality(rarity, out var _, out var quality);
			customData = "G" + quality;
			mercPrice = new Cost
			{
				gold = Mathf.RoundToInt((float)Gem.GetBuyGold(rarity, quality) * num)
			};
		}
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			Vector3 goodRewardPosition = Dew.GetGoodRewardPosition(hero.agentPosition);
			int quality;
			if (isRewardSkill)
			{
				NetworkedManagerBase<LootManager>.instance.SelectSkillAndLevel(rarity, out var skill, out quality);
				Dew.CreateSkillTrigger(skill, goodRewardPosition, rewardLevel, player);
			}
			else
			{
				NetworkedManagerBase<LootManager>.instance.SelectGemAndQuality(rarity, out var gem, out quality);
				Dew.CreateGem(gem, goodRewardPosition, rewardLevel, player);
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
