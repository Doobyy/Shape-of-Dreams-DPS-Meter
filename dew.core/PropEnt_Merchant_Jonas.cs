using System;
using Mirror;
using UnityEngine;

public class PropEnt_Merchant_Jonas : PropEnt_Merchant_Base
{
	public int skillTypeCount = 3;

	public int gemTypeCount = 3;

	public Vector2 gemQuantity;

	public Vector2 skillQuantity;

	private int _fleeDamageCount;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		EntityEvent_OnTakeDamage += (Action<EventInfoDamage>)((EventInfoDamage obj) =>
		{
			if (!(obj.actor is ElementalStatusEffect))
			{
				_fleeDamageCount++;
			}
		});
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && !((UnityEngine.Object)(object)NetworkedManagerBase<ZoneManager>.instance == null) && !NetworkedManagerBase<ZoneManager>.instance.isInAnyTransition)
		{
			NetworkedManagerBase<ZoneManager>.instance.AddModifier<RoomMod_NoMerchant>(NetworkedManagerBase<ZoneManager>.instance.currentNodeIndex);
		}
	}

	protected override void AIUpdate(ref EntityAIContext context)
	{
		base.AIUpdate(ref context);
		bool flag = _fleeDamageCount >= 60 || Status.missingHealth / Status.maxHealth >= 0.3f;
		if (AI.Helper_CanBeCast<At_Prop_Merchant_Flee>() & flag)
		{
			AI.Helper_CastAbilityAuto<At_Prop_Merchant_Flee>();
		}
	}

	private MerchandiseData GetSkill()
	{
		Rarity value = NetworkedManagerBase<LootManager>.instance.SelectSkillRarity();
		NetworkedManagerBase<LootManager>.instance.SelectSkillAndLevel(value, out var skill, out var num);
		return new MerchandiseData
		{
			type = MerchandiseType.Skill,
			itemName = ((object)skill).GetType().Name,
			level = num,
			count = Mathf.Max(1, Mathf.RoundToInt(UnityEngine.Random.Range(skillQuantity.x, skillQuantity.y)))
		};
	}

	private MerchandiseData GetGem()
	{
		Rarity value = NetworkedManagerBase<LootManager>.instance.SelectGemRarity();
		NetworkedManagerBase<LootManager>.instance.SelectGemAndQuality(value, out var gem, out var quality);
		return new MerchandiseData
		{
			type = MerchandiseType.Gem,
			itemName = ((object)gem).GetType().Name,
			level = quality,
			count = Mathf.Max(1, Mathf.RoundToInt(UnityEngine.Random.Range(gemQuantity.x, gemQuantity.y)))
		};
	}

	private void UpdateItemPrices(MerchandiseData[] arr)
	{
		for (int i = 0; i < arr.Length; i++)
		{
			MerchandiseData merchandiseData = arr[i];
			if (merchandiseData.type == MerchandiseType.Gem)
			{
				Gem byShortTypeName = DewResources.GetByShortTypeName<Gem>(merchandiseData.itemName, default(ResourceLoadSettings));
				merchandiseData.price = Cost.Gold(Gem.GetBuyGold(byShortTypeName.rarity, merchandiseData.level));
			}
			else if (merchandiseData.type == MerchandiseType.Skill)
			{
				SkillTrigger byShortTypeName2 = DewResources.GetByShortTypeName<SkillTrigger>(merchandiseData.itemName, default(ResourceLoadSettings));
				merchandiseData.price = Cost.Gold(SkillTrigger.GetBuyGold(byShortTypeName2.rarity, merchandiseData.level));
			}
			else
			{
				merchandiseData.price = Cost.Gold(99999);
			}
			arr[i] = merchandiseData;
		}
	}

	protected override void OnRefresh(DewPlayer player)
	{
		base.OnRefresh(player);
		PopulatePlayerMerchandises(player);
	}

	private MerchandiseData[] GetBaseSkills()
	{
		MerchandiseData[] array = new MerchandiseData[skillTypeCount];
		for (int i = 0; i < skillTypeCount; i++)
		{
			array[i] = GetSkill();
		}
		return array;
	}

	private MerchandiseData[] GetBaseGems()
	{
		MerchandiseData[] array = new MerchandiseData[gemTypeCount];
		for (int i = 0; i < gemTypeCount; i++)
		{
			array[i] = GetGem();
		}
		return array;
	}

	protected override void OnPopulateMerchandises(DewPlayer player)
	{
		MerchandiseData[] baseSkills = GetBaseSkills();
		MerchandiseData[] baseGems = GetBaseGems();
		MerchandiseData[] array = new MerchandiseData[skillTypeCount + gemTypeCount + player.shopAddedItems * 2];
		Array.Copy(baseSkills, 0, array, 0, baseSkills.Length);
		int num = skillTypeCount;
		int num2 = num + player.shopAddedItems;
		for (int i = num; i < num2; i++)
		{
			array[i] = GetSkill();
		}
		Array.Copy(baseGems, 0, array, skillTypeCount + player.shopAddedItems, baseGems.Length);
		int num3 = skillTypeCount + player.shopAddedItems + gemTypeCount;
		num2 = num3 + player.shopAddedItems;
		for (int j = num3; j < num2; j++)
		{
			array[j] = GetGem();
		}
		UpdateItemPrices(array);
		((SyncIDictionary<string, MerchandiseData[]>)(object)merchandises)[player.guid] = array;
	}

	private void MirrorProcessed()
	{
	}
}
