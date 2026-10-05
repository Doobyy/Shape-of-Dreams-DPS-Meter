using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Shrine_CallOfTheRavenous : Shrine, ICustomInteractable
{
	public int skillCount;

	public int skillBonusLevel;

	public int restoredSkillBonusLevel = 3;

	public GameObject fxModel;

	public GameObject fxInteract;

	public GameObject fxSpawnSkill;

	[SaveVar(SaveVarFlags.Default)]
	private string _droppedItemTypeName;

	[SaveVar(SaveVarFlags.Default)]
	private float _droppedItemChance;

	public bool canAltInteract => true;

	public string nameRawText => DewLocalization.GetUIValue(((object)this).GetType().Name + "_Name");

	public string interactActionRawText => DewLocalization.GetUIValue("InGame_Interact_Destroy");

	public string interactAltActionRawText => DewLocalization.GetUIValue("InGame_Interact_Absorb");

	public void SetItemReward(Type type, float chance)
	{
		_droppedItemTypeName = type?.Name;
		_droppedItemChance = chance;
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			FxPlayNetworked(fxModel);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			FxStopNetworked(fxModel);
		}
	}

	protected override bool OnUse(Entity entity)
	{
		((MonoBehaviour)(object)this).StartCoroutine(Routine());
		SingletonDewNetworkBehaviour<Room>.instance.ClearRoom();
		return true;
		IEnumerator Routine()
		{
			FxStopNetworked(fxModel);
			Vector3 pivot = GetRandomSpawnPosition(entity.position);
			yield return new WaitForSeconds(1.5f);
			foreach (DewPlayer gamePlayer in DewPlayer.gamePlayers)
			{
				if (!gamePlayer.hero.IsNullInactiveDeadOrKnockedOut())
				{
					string guid = gamePlayer.hero.owner.guid;
					Ge_CallOfTheRavenous ge_CallOfTheRavenous = Dew.FindActorOfType<Ge_CallOfTheRavenous>();
					if ((UnityEngine.Object)(object)ge_CallOfTheRavenous != null && ge_CallOfTheRavenous.destroyedSkillData.TryGetValue(guid, out var value))
					{
						SkillSpawnRoutine(pivot, gamePlayer, value);
					}
					else
					{
						SkillSpawnRoutine(pivot, gamePlayer);
					}
				}
			}
			DropUniqueReward(pivot);
			yield return new WaitForSeconds(1f);
		}
	}

	private void SkillSpawnRoutine(Vector3 pivot, DewPlayer player, List<string> skills = null)
	{
		for (int i = 0; i < skillCount; i++)
		{
			if (player.hero.IsNullInactiveDeadOrKnockedOut())
			{
				continue;
			}
			SkillTrigger skill = null;
			Vector3 goodRewardPosition = Dew.GetGoodRewardPosition(pivot);
			Rarity rarity = NetworkedManagerBase<LootManager>.instance.SelectSkillRarity(isHigh: true);
			int level = 0;
			bool num = skills != null && skills.Count > 0;
			int num2 = SingletonDewNetworkBehaviour<Room>.instance.rewards.skillBonusLevel + skillBonusLevel;
			if (num)
			{
				int num3 = skills.FindIndex((string s) => s.Contains("_D_"));
				if (num3 < 0)
				{
					num3 = 0;
				}
				ParseDestroyedSkill(skills[num3], out var typeName, out var level2);
				skill = DewResources.GetByShortTypeName<SkillTrigger>(typeName, default(ResourceLoadSettings));
				skills.RemoveAt(num3);
				int num4 = NetworkedManagerBase<LootManager>.instance.SelectSkillLevel(rarity) + num2;
				level = ((level2 < 0) ? num4 : Mathf.Max(level2 + restoredSkillBonusLevel, num4));
			}
			else
			{
				NetworkedManagerBase<LootManager>.instance.SelectSkillAndLevel(rarity, out skill, out level);
				level += num2;
			}
			SkillTrigger skillTrigger = Dew.CreateSkillTrigger(skill, goodRewardPosition, level, player);
			if (skillTrigger.isCharacterSkill)
			{
				skillTrigger.characterSkillOwner = player.guid;
			}
		}
	}

	private void DropUniqueReward(Vector3 pivot)
	{
		if (string.IsNullOrEmpty(_droppedItemTypeName) || (!Dew.IsSkillIncludedInGame(_droppedItemTypeName) && !Dew.IsGemIncludedInGame(_droppedItemTypeName)))
		{
			return;
		}
		UnityEngine.Object byShortTypeName = DewResources.GetByShortTypeName(_droppedItemTypeName);
		foreach (DewPlayer gamePlayer in DewPlayer.gamePlayers)
		{
			if (!gamePlayer.hero.IsNullInactiveDeadOrKnockedOut() && !(UnityEngine.Random.value > _droppedItemChance))
			{
				Vector3 goodRewardPosition = Dew.GetGoodRewardPosition(pivot + (gamePlayer.hero.position - pivot).normalized * 3f, 1.25f);
				if (byShortTypeName is SkillTrigger skillTrigger)
				{
					NetworkedManagerBase<LootManager>.instance.SelectSkillAndLevel(skillTrigger.rarity, out var _, out var level);
					Dew.CreateSkillTrigger(skillTrigger, goodRewardPosition, level, gamePlayer);
				}
				else if (byShortTypeName is Gem gem)
				{
					int quality = NetworkedManagerBase<LootManager>.instance.SelectGemQuality(gem.rarity);
					Dew.CreateGem(gem, goodRewardPosition, quality, gamePlayer);
				}
			}
		}
	}

	public override void OnInteract(Entity entity, bool alt)
	{
		Ge_CallOfTheRavenous ge;
		if (!alt)
		{
			base.OnInteract(entity, alt);
		}
		else if (((NetworkBehaviour)this).isServer)
		{
			ge = Dew.FindActorOfType<Ge_CallOfTheRavenous>();
			if ((UnityEngine.Object)(object)ge == null)
			{
				ge = Dew.CreateActor<Ge_CallOfTheRavenous>();
			}
			((MonoBehaviour)(object)this).StartCoroutine(Routine());
		}
		IEnumerator Routine()
		{
			MakeUnavailable();
			int num = totalUseCount;
			totalUseCount = num + 1;
			FxPlayNetworked(fxInteract, position, null);
			ge.NetworkenableSpawnScaredMaw = true;
			NetworkedManagerBase<ChatManager>.instance.BroadcastMessage(new ChatManager.Message
			{
				type = ChatManager.MessageType.AdvText,
				content = "<i><color=#c32f83>" + DewAdvText.Adv_LocUI("Chat_Notice_CallOfRavenous") + "</color></i>"
			});
			yield return new WaitForSeconds(1f);
			FxStopNetworked(fxModel);
			DropUniqueReward(position);
			if ((UnityEngine.Object)(object)SingletonDewNetworkBehaviour<Room>.instance != null)
			{
				SingletonDewNetworkBehaviour<Room>.instance.ClearRoom();
			}
		}
	}

	private void ParseDestroyedSkill(string entry, out string typeName, out int level)
	{
		typeName = entry;
		level = -1;
		if (string.IsNullOrEmpty(entry))
		{
			return;
		}
		int num = entry.LastIndexOf(':');
		if (num >= 0)
		{
			typeName = entry.Substring(0, num);
			if (!int.TryParse(entry.Substring(num + 1), out level))
			{
				level = -1;
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
