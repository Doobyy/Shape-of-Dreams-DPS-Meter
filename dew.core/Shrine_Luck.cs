using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;

public class Shrine_Luck : ChoiceShrine, IRewardActor, ICustomInteractable
{
	public float prayBasePriceMultiplier = 0.4f;

	public float priceAmpPerPray = 0.4f;

	public int maxPrayCount = 3;

	public GameObject fxPray;

	public readonly SyncDictionary<string, int> currentPrayCount = new SyncDictionary<string, int>();

	public string nameRawText => DewLocalization.GetUIValue("Shrine_Luck_Name");

	public string interactActionRawText => DewLocalization.GetUIValue("InGame_Interact_ShrineActivate");

	public bool canAltInteract => CanPray(DewPlayer.local);

	public string interactAltActionRawText
	{
		get
		{
			if (GetPrayCost(DewPlayer.local).CanAfford(DewPlayer.local) == AffordType.Yes)
			{
				return DewLocalization.GetUIValue("Shrine_Luck_Pray");
			}
			return "<color=#999>" + DewLocalization.GetUIValue("Shrine_Luck_Pray") + "</color>";
		}
	}

	public Cost costAlt => GetPrayCost(DewPlayer.local);

	public bool CanPray(DewPlayer player)
	{
		if (isLocked)
		{
			return false;
		}
		if (!((SyncIDictionary<string, ChoiceShrineItem[]>)(object)choices).ContainsKey(player.guid))
		{
			return false;
		}
		return CollectionExtensions.GetValueOrDefault<string, int>((IReadOnlyDictionary<string, int>)currentPrayCount, player.guid, 0) < maxPrayCount;
	}

	public Cost GetPrayCost(DewPlayer player)
	{
		if (!((SyncIDictionary<string, ChoiceShrineItem[]>)(object)choices).ContainsKey(player.guid))
		{
			return default;
		}
		int valueOrDefault = CollectionExtensions.GetValueOrDefault<string, int>((IReadOnlyDictionary<string, int>)currentPrayCount, player.guid, 0);
		float num = priceAmpPerPray * (float)valueOrDefault;
		ChoiceShrineItem[] array = ((SyncIDictionary<string, ChoiceShrineItem[]>)(object)choices)[player.guid];
		float num2 = 0f;
		ChoiceShrineItem[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			ChoiceShrineItem choiceShrineItem = array2[i];
			if (choiceShrineItem.typeName.StartsWith("St_"))
			{
				Rarity rarity = DewResources.GetByShortTypeName<SkillTrigger>(choiceShrineItem.typeName, ResourceLoadSettings.Light).rarity;
				num2 += (float)SkillTrigger.GetBuyGold(rarity, choiceShrineItem.level);
			}
			else
			{
				Rarity rarity2 = DewResources.GetByShortTypeName<Gem>(choiceShrineItem.typeName, ResourceLoadSettings.Light).rarity;
				num2 += (float)Gem.GetBuyGold(rarity2, choiceShrineItem.level);
			}
		}
		return Cost.Gold(Mathf.RoundToInt(num2 / (float)array.Length * prayBasePriceMultiplier * (1f + num)));
	}

	protected override void OnPopulateChoices(DewPlayer player)
	{
		List<ChoiceShrineItem> list = new List<ChoiceShrineItem>();
		Rarity rarity = NetworkedManagerBase<LootManager>.instance.SelectSkillRarity();
		for (int i = 0; i < itemCount; i++)
		{
			bool flag = Random.value < 0.5f;
			if (flag)
			{
				Shrine_Retrospection.AddRandomSkillToChoices(rarity, list);
			}
			else
			{
				Shrine_Enlightenment.AddRandomGemToChoices(rarity, list);
			}
			ChoiceShrineItem value = list[list.Count - 1];
			value.level += (flag ? SingletonDewNetworkBehaviour<Room>.instance.rewards.skillBonusLevel : SingletonDewNetworkBehaviour<Room>.instance.rewards.gemBonusQuality);
			list[list.Count - 1] = value;
		}
		((SyncIDictionary<string, ChoiceShrineItem[]>)(object)choices)[player.guid] = list.ToArray();
	}

	public override void OnInteract(Entity entity, bool alt)
	{
		if (!alt)
		{
			base.OnInteract(entity, alt);
		}
		else
		{
			if (!((NetworkBehaviour)this).isServer)
			{
				return;
			}
			DewPlayer owner = entity.owner;
			if ((Object)(object)owner == null || !CanPray(owner))
			{
				return;
			}
			Cost prayCost = GetPrayCost(owner);
			if (prayCost.CanAfford(owner) == AffordType.Yes)
			{
				FxPlayNewNetworked(fxPray, entity);
				owner.Spend(prayCost);
				List<ChoiceShrineItem> list = ((SyncIDictionary<string, ChoiceShrineItem[]>)(object)choices)[owner.guid].ToList();
				Rarity rarity = ((!list[0].typeName.StartsWith("St_")) ? DewResources.GetByShortTypeName<Gem>(list[0].typeName, ResourceLoadSettings.Light).rarity : DewResources.GetByShortTypeName<SkillTrigger>(list[0].typeName, ResourceLoadSettings.Light).rarity);
				bool flag = Random.value < 0.5f;
				if (flag)
				{
					Shrine_Retrospection.AddRandomSkillToChoices(rarity, list);
				}
				else
				{
					Shrine_Enlightenment.AddRandomGemToChoices(rarity, list);
				}
				ChoiceShrineItem value = list[list.Count - 1];
				value.level += (flag ? SingletonDewNetworkBehaviour<Room>.instance.rewards.skillBonusLevel : SingletonDewNetworkBehaviour<Room>.instance.rewards.gemBonusQuality);
				list[list.Count - 1] = value;
				((SyncIDictionary<string, ChoiceShrineItem[]>)(object)choices)[owner.guid] = list.ToArray();
				((SyncIDictionary<string, int>)(object)currentPrayCount)[owner.guid] = CollectionExtensions.GetValueOrDefault<string, int>((IReadOnlyDictionary<string, int>)currentPrayCount, owner.guid) + 1;
			}
		}
	}

	public Shrine_Luck()
	{
		((NetworkBehaviour)this).InitSyncObject((SyncObject)(object)currentPrayCount);
	}

	private void MirrorProcessed()
	{
	}
}
