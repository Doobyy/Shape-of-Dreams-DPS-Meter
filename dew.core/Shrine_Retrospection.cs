using System.Collections.Generic;
using Mirror;

public class Shrine_Retrospection : ChoiceShrine, IRewardActor
{
	public Dictionary<string, int> choiceOffset = new Dictionary<string, int>();

	protected override void OnPopulateChoices(DewPlayer player)
	{
		List<ChoiceShrineItem> list = new List<ChoiceShrineItem>();
		Rarity rarity = NetworkedManagerBase<LootManager>.instance.SelectSkillRarity();
		int valueOrDefault = CollectionExtensions.GetValueOrDefault<string, int>((IReadOnlyDictionary<string, int>)choiceOffset, player.guid, 0);
		for (int i = 0; i < itemCount + valueOrDefault; i++)
		{
			AddRandomSkillToChoices(rarity, list);
			ChoiceShrineItem value = list[list.Count - 1];
			value.level += SingletonDewNetworkBehaviour<Room>.instance.rewards.skillBonusLevel;
			list[list.Count - 1] = value;
		}
		((SyncIDictionary<string, ChoiceShrineItem[]>)(object)choices)[player.guid] = list.ToArray();
	}

	public static void AddRandomSkillToChoices(Rarity rarity, List<ChoiceShrineItem> list)
	{
		int num = 0;
		SkillTrigger skill;
		int level;
		while (true)
		{
			NetworkedManagerBase<LootManager>.instance.SelectSkillAndLevel(rarity, out skill, out level);
			if (num >= 10)
			{
				break;
			}
			bool flag = false;
			for (int i = 0; i < list.Count; i++)
			{
				if (!(list[i].typeName != ((object)skill).GetType().Name))
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
			level = level,
			typeName = ((object)skill).GetType().Name
		});
	}

	private void MirrorProcessed()
	{
	}
}
