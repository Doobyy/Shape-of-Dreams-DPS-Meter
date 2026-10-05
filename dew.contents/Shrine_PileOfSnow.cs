using System.Collections;
using System.Linq;
using UnityEngine;

public class Shrine_PileOfSnow : Shrine, IShrineCustomAction
{
	public float rewardDelay = 0.25f;

	public float goldAmountMultiplier = 0.2f;

	public float dreamDustAmountMultiplier = 0.2f;

	public float damageCurrHpRatio = 0.05f;

	[Header("Weights")]
	public float noRewardWeight = 6f;

	public float coldMemoryWeight = 1f;

	public float coldEssenceWeight = 1f;

	public float goldWeight = 2f;

	public float dreamDustWeight = 2f;

	public float scavengerWeight = 1f;

	public float quadScavengerWeight = 0.3f;

	public float miniBossScavengerWeight = 0.05f;

	protected override bool OnUse(Entity entity)
	{
		((MonoBehaviour)(object)this).StartCoroutine(Routine());
		return true;
		IEnumerator Routine()
		{
			if (!(entity is Hero_Cetus))
			{
				DefaultDamage(entity.Status.currentHealth * damageCurrHpRatio).SetElemental(ElementalType.Cold).SetAttr(DamageAttribute.DamageOverTime).Dispatch(entity);
			}
			yield return new WaitForSeconds(rewardDelay);
			float num = noRewardWeight;
			num += coldMemoryWeight;
			num += coldEssenceWeight;
			num += goldWeight;
			num += dreamDustWeight;
			num += scavengerWeight;
			num += quadScavengerWeight;
			num += miniBossScavengerWeight;
			float num2 = Random.Range(0f, num);
			float num3 = noRewardWeight;
			if (!(num2 < num3))
			{
				if (num2 < (num3 += coldMemoryWeight))
				{
					Rarity rarity = NetworkedManagerBase<LootManager>.instance.SelectSkillRarity();
					NetworkedManagerBase<LootManager>.instance.SelectSkillAndLevel(rarity, out var _, out var level);
					string[] array = NetworkedManagerBase<LootManager>.instance.poolSkillsByRarity[rarity].Intersect(NetworkedManagerBase<LootManager>.instance.poolSkillsByTag[DescriptionTags.Cold]).ToArray();
					Rarity rarity2 = rarity;
					while (array.Length == 0)
					{
						rarity = ((rarity != Rarity.Common) ? (rarity - 1) : Rarity.Legendary);
						if (rarity == rarity2)
						{
							Debug.Log("Couldn't find appropriate cold skill to reward");
							yield break;
						}
						array = NetworkedManagerBase<LootManager>.instance.poolSkillsByRarity[rarity].Intersect(NetworkedManagerBase<LootManager>.instance.poolSkillsByTag[DescriptionTags.Cold]).ToArray();
					}
					foreach (DewPlayer gamePlayer in DewPlayer.gamePlayers)
					{
						if (!gamePlayer.hero.IsNullInactiveDeadOrKnockedOut())
						{
							Vector3 vector = position;
							Dew.CreateSkillTrigger(DewResources.GetByShortTypeName<SkillTrigger>(array[Random.Range(0, array.Length)], default(ResourceLoadSettings)), vector, level, gamePlayer);
						}
					}
				}
				else if (num2 < (num3 += coldEssenceWeight))
				{
					Rarity rarity3 = NetworkedManagerBase<LootManager>.instance.SelectGemRarity();
					NetworkedManagerBase<LootManager>.instance.SelectGemAndQuality(rarity3, out var _, out var quality);
					string[] array2 = NetworkedManagerBase<LootManager>.instance.poolGemsByRarity[rarity3].Intersect(NetworkedManagerBase<LootManager>.instance.poolGemsByTag[DescriptionTags.Cold]).ToArray();
					Rarity rarity4 = rarity3;
					while (array2.Length == 0)
					{
						rarity3 = ((rarity3 != Rarity.Common) ? (rarity3 - 1) : Rarity.Legendary);
						if (rarity3 == rarity4)
						{
							Debug.Log("Couldn't find appropriate cold skill to reward");
							yield break;
						}
						array2 = NetworkedManagerBase<LootManager>.instance.poolGemsByRarity[rarity3].Intersect(NetworkedManagerBase<LootManager>.instance.poolGemsByTag[DescriptionTags.Cold]).ToArray();
					}
					foreach (DewPlayer gamePlayer2 in DewPlayer.gamePlayers)
					{
						if (!gamePlayer2.hero.IsNullInactiveDeadOrKnockedOut())
						{
							Vector3 vector2 = position;
							Dew.CreateGem(DewResources.GetByShortTypeName<Gem>(array2[Random.Range(0, array2.Length)], default(ResourceLoadSettings)), vector2, quality, gamePlayer2);
						}
					}
				}
				else if (num2 < (num3 += goldWeight))
				{
					int amount = DewMath.RandomRoundToInt(NetworkedManagerBase<GameManager>.instance.GetSpecialRewardAmount_Gold() * goldAmountMultiplier);
					NetworkedManagerBase<PickupManager>.instance.DropGold(isKillGold: false, isGivenByOtherPlayer: false, amount, position);
				}
				else if (num2 < (num3 += dreamDustWeight))
				{
					int amount2 = DewMath.RandomRoundToInt(NetworkedManagerBase<GameManager>.instance.GetSpecialRewardAmount_DreamDust() * dreamDustAmountMultiplier);
					NetworkedManagerBase<PickupManager>.instance.DropDreamDust(isGivenByOtherPlayer: false, amount2, position);
				}
				else if (num2 < (num3 += scavengerWeight))
				{
					Quaternion value = Quaternion.LookRotation(entity.agentPosition - position).Flattened();
					Dew.SpawnEntity<Mon_SnowMountain_Scavenger>(position, value, null, DewPlayer.creep, 1);
				}
				else if (num2 < num3 + quadScavengerWeight)
				{
					for (int i = 0; i < 4; i++)
					{
						Vector3 vector3 = position + (Random.insideUnitSphere * 2f).Flattened();
						Quaternion value2 = Quaternion.LookRotation(vector3 - position).Flattened();
						Dew.SpawnEntity<Mon_SnowMountain_Scavenger>(vector3, value2, null, DewPlayer.creep, 1);
					}
				}
				else
				{
					SingletonDewNetworkBehaviour<Room>.instance.monsters.SpawnMiniBoss(new SpawnMonsterSettings
					{
						hero = (entity as Hero),
						spawnPosGetter = () => position,
						spawnRotGetter = () => Quaternion.LookRotation(entity.agentPosition - position).Flattened(),
						initDelayMultiplier = 0f
					}, DewResources.GetByType<Mon_SnowMountain_Scavenger>(default(ResourceLoadSettings)));
				}
			}
			Destroy();
		}
	}

	public string GetRawAction()
	{
		return DewLocalization.GetUIValue("Shrine_PileOfSnow_Dig");
	}

	private void MirrorProcessed()
	{
	}
}
