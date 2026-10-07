using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Shrine_Paradox : ChoiceShrine
{
	public Transform[] bobTransforms;

	private Vector3[] _originalPositions;

	protected override void Awake()
	{
		base.Awake();
		_originalPositions = new Vector3[bobTransforms.Length];
		for (int i = 0; i < bobTransforms.Length; i++)
		{
			_originalPositions[i] = bobTransforms[i].localPosition;
		}
	}

	public override void FrameUpdate()
	{
		base.FrameUpdate();
		for (int i = 0; i < bobTransforms.Length; i++)
		{
			Transform transform = bobTransforms[i];
			if (!(transform == null))
			{
				transform.localPosition = _originalPositions[i] + Mathf.Sin(Time.time * 0.5f + 0.5f * (float)i) * 0.3f * Vector3.up;
			}
		}
	}

	protected override void OnPopulateChoices(DewPlayer player)
	{
		List<ChoiceShrineItem> list = new List<ChoiceShrineItem>();
		Rarity value = NetworkedManagerBase<LootManager>.instance.SelectGemRarity();
		int num = 0;
		for (int i = 0; i < itemCount; i++)
		{
			NetworkedManagerBase<LootManager>.instance.SelectGemAndQuality(value, out var gem, out var quality);
			if (num < 10)
			{
				bool flag = false;
				for (int j = 0; j < list.Count; j++)
				{
					if (!(list[j].typeName != ((object)gem).GetType().Name))
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					num++;
					i--;
					continue;
				}
			}
			list.Add(new ChoiceShrineItem
			{
				level = quality + SingletonDewNetworkBehaviour<Room>.instance.rewards.gemBonusQuality,
				typeName = ((object)gem).GetType().Name
			});
		}
		for (int k = 0; k < itemCount; k++)
		{
			NetworkedManagerBase<LootManager>.instance.SelectSkillAndLevel(value, out var skill, out var level);
			if (num < 10)
			{
				bool flag2 = false;
				for (int l = 0; l < list.Count; l++)
				{
					if (!(list[l].typeName != ((object)skill).GetType().Name))
					{
						flag2 = true;
						break;
					}
				}
				if (flag2)
				{
					num++;
					k--;
					continue;
				}
			}
			list.Add(new ChoiceShrineItem
			{
				level = level + SingletonDewNetworkBehaviour<Room>.instance.rewards.gemBonusQuality,
				typeName = ((object)skill).GetType().Name
			});
		}
		((SyncIDictionary<string, ChoiceShrineItem[]>)(object)choices)[player.guid] = list.ToArray();
	}

	private void MirrorProcessed()
	{
	}
}
