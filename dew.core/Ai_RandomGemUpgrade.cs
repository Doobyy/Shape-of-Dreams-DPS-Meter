using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Ai_RandomGemUpgrade : StandardProjectile, IOtherPlayersTonedDownDisable
{
	public int addedQuality = 50;

	public int addedLevel = 1;

	public float deviateMag = 3f;

	private Vector3 _deviateVector;

	[NonSerialized]
	public Actor upgradeTarget;

	protected override void OnPrepare()
	{
		base.OnPrepare();
		SetCustomStartPosition(position);
	}

	protected override void OnCreate()
	{
		_deviateVector = UnityEngine.Random.insideUnitSphere * deviateMag;
		if (_deviateVector.y < 0f)
		{
			_deviateVector.y *= -1f;
		}
		base.OnCreate();
	}

	protected override Vector3 PositionSolver(float dt)
	{
		return base.PositionSolver(dt) + Mathf.Sin(normalizedPosition * (float)Math.PI) * _deviateVector;
	}

	protected override void OnComplete()
	{
		base.OnComplete();
		Entity entity = targetEntity;
		Hero h = entity as Hero;
		if (h == null)
		{
			return;
		}
		if (upgradeTarget.IsNullOrInactive())
		{
			if (h.Skill.gems.Count > 0)
			{
				List<Gem> list = new List<Gem>(h.Skill.gems.Values);
				upgradeTarget = Dew.SelectRandomWeightedInList(list, (Gem g) => g.quality, null);
			}
			else
			{
				List<SkillTrigger> list2 = new List<SkillTrigger>();
				foreach (KeyValuePair<int, AbilityTrigger> ability in h.Ability.abilities)
				{
					if (ability.Value is SkillTrigger { isLevelUpEnabled: not false } skillTrigger)
					{
						list2.Add(skillTrigger);
					}
				}
				upgradeTarget = Dew.SelectRandomWeightedInList(list2, (SkillTrigger s) => s.level, null);
			}
		}
		if (upgradeTarget is Gem gem)
		{
			gem.quality += addedQuality;
			NetworkedManagerBase<ClientEventManager>.instance.InvokeOnItemUpgraded(h, (NetworkBehaviour)(object)gem);
			return;
		}
		if (upgradeTarget is SkillTrigger skillTrigger2)
		{
			skillTrigger2.level += addedLevel;
			NetworkedManagerBase<ClientEventManager>.instance.InvokeOnItemUpgraded(h, (NetworkBehaviour)(object)skillTrigger2);
			return;
		}
		if (h.level < h.maxLevel)
		{
			h.ReceiveExperience(h.maxExp);
		}
		if (h.persistentData.GetDataOrDefault("Ai_RandomGemUpgrade", "didReceiveSneeze", defaultValue: false))
		{
			Rarity rarity = NetworkedManagerBase<LootManager>.instance.SelectSkillRarity(isHigh: true);
			if ((int)rarity <= 1)
			{
				rarity = Rarity.Epic;
			}
			Dew.CreateActor(h.agentPosition, null, null, (Shrine_Chaos c) =>
			{
				c.Networkrarity = rarity;
				c.playersOverride = new string[1] { h.owner.guid };
			});
		}
		else
		{
			h.persistentData.SetData("Ai_RandomGemUpgrade", "didReceiveSneeze", value: true);
			NetworkedManagerBase<LootManager>.instance.SelectSkillAndLevel(Rarity.Common, out var _, out var level);
			Dew.CreateSkillTrigger(DewResources.GetByShortTypeName<SkillTrigger>("St_C_Sneeze", default(ResourceLoadSettings)), h.agentPosition, level + 5);
		}
	}

	private void MirrorProcessed()
	{
	}
}
