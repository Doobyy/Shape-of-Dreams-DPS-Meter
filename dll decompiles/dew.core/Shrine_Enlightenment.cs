using System.Collections.Generic;
using Mirror;

public class Shrine_Enlightenment : ChoiceShrine, IRewardActor
{
	public Dictionary<string, int> choiceOffset = new Dictionary<string, int>();

	protected override void OnPopulateChoices(DewPlayer player)
	{
		List<ChoiceShrineItem> list = new List<ChoiceShrineItem>();
		Rarity rarity = NetworkedManagerBase<LootManager>.instance.SelectGemRarity();
		int valueOrDefault = CollectionExtensions.GetValueOrDefault<string, int>((IReadOnlyDictionary<string, int>)choiceOffset, player.guid, 0);
		for (int i = 0; i < itemCount + valueOrDefault; i++)
		{
			AddRandomGemToChoices(rarity, list);
			ChoiceShrineItem value = list[list.Count - 1];
			value.level += SingletonDewNetworkBehaviour<Room>.instance.rewards.gemBonusQuality;
			list[list.Count - 1] = value;
		}
		((SyncIDictionary<string, ChoiceShrineItem[]>)(object)choices)[player.guid] = list.ToArray();
	}

	public static void AddRandomGemToChoices(Rarity rarity, List<ChoiceShrineItem> list)
	{
		int num = 0;
		Gem gem;
		int quality;
		while (true)
		{
			NetworkedManagerBase<LootManager>.instance.SelectGemAndQuality(rarity, out gem, out quality);
			if (num >= 10)
			{
				break;
			}
			bool flag = false;
			for (int i = 0; i < list.Count; i++)
			{
				if (!(list[i].typeName != ((object)gem).GetType().Name))
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				break;
			}
			num++;
		}
		list.Add(new ChoiceShrineItem
		{
			level = quality,
			typeName = ((object)gem).GetType().Name
		});
	}

	private void MirrorProcessed()
	{
	}
}
