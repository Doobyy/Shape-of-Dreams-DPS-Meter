using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class Shrine_MarshOfDestiny_SeedOfTorment : Shrine
{
	public enum PenaltyType
	{
		ArmorFlat,
		MaxHealthPercentage,
		AtkSpdPercentage,
		MovSpdPercentage,
		MirageSkinAnyPercentage,
		MirageSkinSpecificPercentage,
		HeroicBossSpecialSkill,
		RandomCurseEveryTravelThisZone,
		CurseThisZone
	}

	public enum RewardType
	{
		Chaos,
		Memory,
		Essence,
		Gold,
		DreamDust,
		PlatinumCoin
	}

	public enum StrengthType
	{
		Weak,
		Medium,
		Strong,
		VeryStrong
	}

	public struct ChoiceItem
	{
		public StrengthType strength;

		public PenaltyType penalty;

		public string penaltyParameter;

		public bool isTempPenalty;

		public RewardType reward;

		public string rewardParameter;
	}

	[SaveVar(SaveVarFlags.Default)]
	public readonly SyncList<ChoiceItem> choices = new SyncList<ChoiceItem>();

	[Header("Rewards")]
	public float[] currencyRewardMultiplier;

	public float currencyRewardDeviation;

	public int[] platinumCoins;

	[Header("Penalties")]
	public float[] atkSpdPercentages;

	public float[] movSpdPercentages;

	public float[] mirageSkinChancePercentages;

	public float tempPenaltyMultiplier = 2f;

	[Header("Penalties (Adaptive Health Scaling)")]
	public float[] hpAdjustFactors;

	public float armorMultiplier = 0.8f;

	public float desiredMonsterLifetime = 5f;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			if (choices.Count == 0)
			{
				PopulateChoices();
			}
			if (!Rift_RoomExit.instance.IsNullOrInactive())
			{
				Rift_RoomExit.instance.isLocked = isAvailable;
			}
		}
	}

	protected override bool OnUse(Entity entity)
	{
		TpcOpenSelection(((NetworkBehaviour)entity.owner).connectionToClient);
		return false;
	}

	[TargetRpc]
	private void TpcOpenSelection(NetworkConnectionToClient target)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendTargetRPCInternal((NetworkConnection)(object)target, "System.Void Shrine_MarshOfDestiny_SeedOfTorment::TpcOpenSelection(Mirror.NetworkConnectionToClient)", -2088136217, (NetworkWriter)(object)val, 0);
		NetworkWriterPool.Return(val);
	}

	[Command(requiresAuthority = false)]
	public void CmdChoose(int index, NetworkConnectionToClient sender = null)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteInt((NetworkWriter)(object)val, index);
		((NetworkBehaviour)this).SendCommandInternal("System.Void Shrine_MarshOfDestiny_SeedOfTorment::CmdChoose(System.Int32,Mirror.NetworkConnectionToClient)", -1024250162, (NetworkWriter)(object)val, 0, false);
		NetworkWriterPool.Return(val);
	}

	public void ApplyChoiceToGame(ChoiceItem item, DewPlayer player)
	{
		if ((UnityEngine.Object)(object)player != null)
		{
			string text = DewAdvText.Adv_Format(DewAdvText.Adv_LocUI("Shrine_MarshOfDestiny_SeedOfTorment_Broadcast"), DewAdvText.Adv_ColoredDescribedPlayerName(player)) + " <color=#ff9696>" + GetChoiceNameAdv(item) + "</color>";
			NetworkedManagerBase<ChatManager>.instance.BroadcastAdvText("<color=#94aab5><i>" + text + "</i></color>");
			NetworkedManagerBase<ChatManager>.instance.BroadcastAdvText("<color=#94aab5><i>- " + GetChoicePenaltyDescriptionAdv(item) + "</i></color>");
			NetworkedManagerBase<ChatManager>.instance.BroadcastAdvText("<color=#94aab5><i>- " + GetChoiceRewardDescriptionAdv(item) + "</i></color>");
		}
		LucidDream_MarshOfDestiny lucidDream_MarshOfDestiny = Dew.FindActorOfType<LucidDream_MarshOfDestiny>();
		if (lucidDream_MarshOfDestiny.IsNullOrInactive())
		{
			return;
		}
		StatBonus statBonus = (item.isTempPenalty ? lucidDream_MarshOfDestiny.monsterStatBonusThisZone : lucidDream_MarshOfDestiny.monsterStatBonus);
		switch (item.penalty)
		{
		case PenaltyType.ArmorFlat:
			statBonus.armorFlat += DewPersistence.FromJson<float>(item.penaltyParameter);
			break;
		case PenaltyType.MaxHealthPercentage:
			statBonus.maxHealthPercentage += DewPersistence.FromJson<float>(item.penaltyParameter);
			break;
		case PenaltyType.AtkSpdPercentage:
			statBonus.attackSpeedPercentage += DewPersistence.FromJson<float>(item.penaltyParameter);
			break;
		case PenaltyType.MovSpdPercentage:
			statBonus.movementSpeedPercentage += DewPersistence.FromJson<float>(item.penaltyParameter);
			break;
		case PenaltyType.MirageSkinAnyPercentage:
		{
			float num = DewPersistence.FromJson<float>(item.penaltyParameter) * 0.01f;
			if (item.isTempPenalty)
			{
				lucidDream_MarshOfDestiny.addedMirageSkinChanceThisZone += num;
			}
			else
			{
				lucidDream_MarshOfDestiny.addedMirageSkinChance += num;
			}
			break;
		}
		case PenaltyType.MirageSkinSpecificPercentage:
		{
			(string, float) tuple = DewPersistence.FromJson<(string, float)>(item.penaltyParameter);
			MirageSkinEffect byShortTypeName3 = DewResources.GetByShortTypeName<MirageSkinEffect>(tuple.Item1, default(ResourceLoadSettings));
			SyncDictionary<SyncableAssetRef, float> val = (item.isTempPenalty ? lucidDream_MarshOfDestiny.addedMirageSkinChanceThisZoneByType : lucidDream_MarshOfDestiny.addedMirageSkinChanceByType);
			((SyncIDictionary<SyncableAssetRef, float>)(object)val)[(SyncableAssetRef)(UnityEngine.Object)(object)byShortTypeName3] = CollectionExtensions.GetValueOrDefault<SyncableAssetRef, float>((IReadOnlyDictionary<SyncableAssetRef, float>)val, (SyncableAssetRef)(UnityEngine.Object)(object)byShortTypeName3) + tuple.Item2 * 0.01f;
			break;
		}
		case PenaltyType.RandomCurseEveryTravelThisZone:
			lucidDream_MarshOfDestiny.doRandomCurseEveryTravelThisZone = true;
			break;
		case PenaltyType.CurseThisZone:
		{
			CurseStatusEffect byShortTypeName2 = DewResources.GetByShortTypeName<CurseStatusEffect>(item.penaltyParameter, default(ResourceLoadSettings));
			HatredStrengthType curseStrength = HatredStrengthType.Mild;
			if (item.strength == StrengthType.Medium)
			{
				curseStrength = HatredStrengthType.Mild;
			}
			if (item.strength == StrengthType.Strong)
			{
				curseStrength = HatredStrengthType.Potent;
			}
			if (item.strength == StrengthType.VeryStrong)
			{
				curseStrength = HatredStrengthType.Powerful;
			}
			foreach (DewPlayer gamePlayer in DewPlayer.gamePlayers)
			{
				gamePlayer.hero.CreateStatusEffect(byShortTypeName2, gamePlayer.hero, new CastInfo(gamePlayer.hero), (CurseStatusEffect se) =>
				{
					se.currentStrength = curseStrength;
					se.progressType = QuestProgressType.TravelZone;
					se.requiredAmount = 1;
					se.dontRemoveOnKnockOut = true;
				});
			}
			break;
		}
		case PenaltyType.HeroicBossSpecialSkill:
		{
			MiniBossEffect byShortTypeName = DewResources.GetByShortTypeName<MiniBossEffect>(item.penaltyParameter, default(ResourceLoadSettings));
			if (!lucidDream_MarshOfDestiny.heroicBossEffects.Contains((SyncableAssetRef)(UnityEngine.Object)(object)byShortTypeName))
			{
				lucidDream_MarshOfDestiny.heroicBossEffects.Add((SyncableAssetRef)(UnityEngine.Object)(object)byShortTypeName);
			}
			break;
		}
		default:
			throw new ArgumentOutOfRangeException();
		}
		if (item.isTempPenalty)
		{
			lucidDream_MarshOfDestiny.syncedMonsterStatBonusThisZone = (BonusStats)lucidDream_MarshOfDestiny.monsterStatBonusThisZone;
		}
		else
		{
			lucidDream_MarshOfDestiny.syncedMonsterStatBonus = (BonusStats)lucidDream_MarshOfDestiny.monsterStatBonus;
		}
		switch (item.reward)
		{
		case RewardType.Chaos:
			Dew.CreateActor(GetRewardPos(null), null, null, (Shrine_Chaos c) =>
			{
				c.rarity = GetRewardRarity();
			});
			break;
		case RewardType.Memory:
		{
			Rarity value2 = GetRewardRarity();
			{
				foreach (DewPlayer gamePlayer2 in DewPlayer.gamePlayers)
				{
					if (!gamePlayer2.hero.IsNullInactiveDeadOrKnockedOut())
					{
						Vector3 vector2 = GetRewardPos(gamePlayer2.hero);
						NetworkedManagerBase<LootManager>.instance.SelectSkillAndLevel(value2, out var skill, out var level);
						Dew.CreateSkillTrigger(skill, vector2, level, gamePlayer2);
					}
				}
				break;
			}
		}
		case RewardType.Essence:
		{
			Rarity value = GetRewardRarity();
			{
				foreach (DewPlayer gamePlayer3 in DewPlayer.gamePlayers)
				{
					if (!gamePlayer3.hero.IsNullInactiveDeadOrKnockedOut())
					{
						Vector3 vector = GetRewardPos(gamePlayer3.hero);
						NetworkedManagerBase<LootManager>.instance.SelectGemAndQuality(value, out var gem, out var quality);
						Dew.CreateGem(gem, vector, quality, gamePlayer3);
					}
				}
				break;
			}
		}
		case RewardType.Gold:
		{
			int amount2 = DewPersistence.FromJson<int>(item.rewardParameter);
			{
				foreach (DewPlayer gamePlayer4 in DewPlayer.gamePlayers)
				{
					if (!gamePlayer4.hero.IsNullInactiveDeadOrKnockedOut())
					{
						NetworkedManagerBase<PickupManager>.instance.DropGold(isKillGold: false, isGivenByOtherPlayer: false, amount2, position, gamePlayer4.hero);
					}
				}
				break;
			}
		}
		case RewardType.DreamDust:
		{
			int amount = DewPersistence.FromJson<int>(item.rewardParameter);
			{
				foreach (DewPlayer gamePlayer5 in DewPlayer.gamePlayers)
				{
					if (!gamePlayer5.hero.IsNullInactiveDeadOrKnockedOut())
					{
						NetworkedManagerBase<PickupManager>.instance.DropDreamDust(isGivenByOtherPlayer: false, amount, position, gamePlayer5.hero);
					}
				}
				break;
			}
		}
		case RewardType.PlatinumCoin:
		{
			int num2 = DewPersistence.FromJson<int>(item.rewardParameter);
			{
				foreach (DewPlayer gamePlayer6 in DewPlayer.gamePlayers)
				{
					gamePlayer6.platinumCoin += num2;
				}
				break;
			}
		}
		default:
			throw new ArgumentOutOfRangeException();
		}
		Vector3 GetRewardPos(Hero targetHero)
		{
			if ((UnityEngine.Object)(object)targetHero == null)
			{
				return Dew.GetGoodRewardPosition(position, 3f);
			}
			return Dew.GetGoodRewardPosition(position + (targetHero.agentPosition - position).normalized * 2.5f);
		}
		Rarity GetRewardRarity()
		{
			return DewPersistence.FromJson<Rarity>(item.rewardParameter);
		}
	}

	public void PopulateChoices()
	{
		if (!((UnityEngine.Object)(object)Dew.FindActorOfType<LucidDream_MarshOfDestiny>() == null))
		{
			choices.Clear();
			int num = UnityEngine.Random.Range(0, 3);
			AddChoice((UnityEngine.Random.value < 0.5f) ? StrengthType.Strong : StrengthType.VeryStrong, num == 0);
			AddChoice(StrengthType.Medium, num == 1);
			AddChoice(StrengthType.Weak, num == 2);
		}
	}

	public void AddChoice(StrengthType strength, bool isTemporary)
	{
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		ChoiceItem choiceItem = new ChoiceItem
		{
			strength = strength
		};
		List<PenaltyType> list = ((PenaltyType[])Enum.GetValues(typeof(PenaltyType))).ToList();
		if (isTemporary)
		{
			List<PenaltyType> list2 = new List<PenaltyType>
			{
				PenaltyType.ArmorFlat,
				PenaltyType.MaxHealthPercentage,
				PenaltyType.AtkSpdPercentage,
				PenaltyType.MovSpdPercentage,
				PenaltyType.MirageSkinAnyPercentage,
				PenaltyType.MirageSkinSpecificPercentage
			};
			for (int num = list.Count - 1; num >= 0; num--)
			{
				if (!list2.Contains(list[num]))
				{
					list.RemoveAt(num);
				}
			}
		}
		if (strength == StrengthType.Weak || strength == StrengthType.Medium)
		{
			list.Remove(PenaltyType.HeroicBossSpecialSkill);
			list.Remove(PenaltyType.RandomCurseEveryTravelThisZone);
			list.Remove(PenaltyType.CurseThisZone);
		}
		Enumerator<ChoiceItem> enumerator = choices.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				list.Remove(enumerator.Current.penalty);
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
		if (list.Count != 0)
		{
			List<RewardType> list3 = ((RewardType[])Enum.GetValues(typeof(RewardType))).ToList();
			choiceItem.penalty = Dew.SelectRandomWeightedInList(list, AddChoice_GetPenaltyChanceWeight);
			choiceItem.penaltyParameter = AddChoice_GetPenaltyParameter(choiceItem.penalty, strength, isTemporary);
			choiceItem.isTempPenalty = isTemporary;
			choiceItem.reward = list3[UnityEngine.Random.Range(0, list3.Count)];
			choiceItem.rewardParameter = AddChoice_GetRewardParameter(choiceItem.reward, strength);
			choices.Add(choiceItem);
		}
	}

	public string AddChoice_GetRewardParameter(RewardType reward, StrengthType strength)
	{
		switch (reward)
		{
		case RewardType.Chaos:
		case RewardType.Memory:
		case RewardType.Essence:
			return DewPersistence.ToJson(strength switch
			{
				StrengthType.Weak => Rarity.Common, 
				StrengthType.Medium => Rarity.Rare, 
				StrengthType.Strong => Rarity.Epic, 
				StrengthType.VeryStrong => Rarity.Legendary, 
				_ => throw new ArgumentOutOfRangeException("strength", strength, null), 
			});
		case RewardType.Gold:
			return DewPersistence.ToJson((int)(NetworkedManagerBase<GameManager>.instance.GetSpecialRewardAmount_Gold() * (currencyRewardMultiplier.GetClamped((int)strength) * UnityEngine.Random.Range(1f - currencyRewardDeviation, 1f + currencyRewardDeviation))));
		case RewardType.DreamDust:
			return DewPersistence.ToJson((int)(NetworkedManagerBase<GameManager>.instance.GetSpecialRewardAmount_Gold() * (currencyRewardMultiplier.GetClamped((int)strength) * UnityEngine.Random.Range(1f - currencyRewardDeviation, 1f + currencyRewardDeviation))));
		case RewardType.PlatinumCoin:
			return DewPersistence.ToJson(platinumCoins.GetClamped((int)strength));
		default:
			throw new ArgumentOutOfRangeException("reward", reward, null);
		}
	}

	public float AddChoice_GetPenaltyChanceWeight(PenaltyType type)
	{
		LucidDream_MarshOfDestiny instance = Dew.FindActorOfType<LucidDream_MarshOfDestiny>();
		bool flag = instance.addedMirageSkinChance > 1f || ((IEnumerable<KeyValuePair<SyncableAssetRef, float>>)instance.addedMirageSkinChanceByType).Sum((KeyValuePair<SyncableAssetRef, float> p) => p.Value) > 1f;
		if ((type == PenaltyType.MirageSkinAnyPercentage) & flag)
		{
			return 0f;
		}
		if ((type == PenaltyType.MirageSkinSpecificPercentage) & flag)
		{
			return 0f;
		}
		if (type == PenaltyType.HeroicBossSpecialSkill && DewResources.FindAllByType<MiniBossEffect>(default(ResourceLoadSettings)).All((MiniBossEffect eff) => instance.heroicBossEffects.Contains((SyncableAssetRef)(UnityEngine.Object)(object)eff)))
		{
			return 0f;
		}
		if (type == PenaltyType.RandomCurseEveryTravelThisZone && instance.doRandomCurseEveryTravelThisZone)
		{
			return 0f;
		}
		switch (type)
		{
		case PenaltyType.HeroicBossSpecialSkill:
			return 1.25f;
		case PenaltyType.ArmorFlat:
		case PenaltyType.MaxHealthPercentage:
			return 0.55f;
		default:
			return 1f;
		}
	}

	public string AddChoice_GetPenaltyParameter(PenaltyType type, StrengthType strength, bool isTemporary)
	{
		LucidDream_MarshOfDestiny instance = Dew.FindActorOfType<LucidDream_MarshOfDestiny>();
		switch (type)
		{
		case PenaltyType.ArmorFlat:
		case PenaltyType.MaxHealthPercentage:
		{
			float currentKillTimePerMonster = desiredMonsterLifetime;
			if ((UnityEngine.Object)(object)instance != null)
			{
				currentKillTimePerMonster = instance.GetCurrentKillTimePerMonster();
			}
			float num = Mathf.Clamp(desiredMonsterLifetime / currentKillTimePerMonster, 1.4f, 21f);
			float clamped = hpAdjustFactors.GetClamped((int)strength);
			float num2 = (num - 1f) * 100f * clamped;
			if (type == PenaltyType.ArmorFlat)
			{
				num2 *= armorMultiplier;
			}
			num2 = (float)Mathf.RoundToInt(num2 / 5f) * 5f;
			if (isTemporary)
			{
				num2 *= tempPenaltyMultiplier;
			}
			return DewPersistence.ToJson(num2);
		}
		case PenaltyType.AtkSpdPercentage:
		{
			float num3 = atkSpdPercentages.GetClamped((int)strength);
			if (isTemporary)
			{
				num3 *= tempPenaltyMultiplier;
			}
			return DewPersistence.ToJson(num3);
		}
		case PenaltyType.MovSpdPercentage:
		{
			float num5 = movSpdPercentages.GetClamped((int)strength);
			if (isTemporary)
			{
				num5 *= tempPenaltyMultiplier;
			}
			return DewPersistence.ToJson(num5);
		}
		case PenaltyType.MirageSkinAnyPercentage:
		{
			float num6 = mirageSkinChancePercentages.GetClamped((int)strength);
			if (isTemporary)
			{
				num6 *= tempPenaltyMultiplier;
			}
			return DewPersistence.ToJson(num6);
		}
		case PenaltyType.MirageSkinSpecificPercentage:
		{
			float num4 = mirageSkinChancePercentages.GetClamped((int)strength);
			if (isTemporary)
			{
				num4 *= tempPenaltyMultiplier;
			}
			MirageSkinEffect[] array2 = (from se in DewResources.FindAllByType<MirageSkinEffect>(ResourceLoadSettings.Light)
				where se.tier >= 0
				select se).ToArray();
			return DewPersistence.ToJson((((object)array2[UnityEngine.Random.Range(0, array2.Length)]).GetType().Name, num4));
		}
		case PenaltyType.CurseThisZone:
		{
			HatredStrengthType curseStrength = HatredStrengthType.Mild;
			if (strength == StrengthType.Medium)
			{
				curseStrength = HatredStrengthType.Mild;
			}
			if (strength == StrengthType.Strong)
			{
				curseStrength = HatredStrengthType.Potent;
			}
			if (strength == StrengthType.VeryStrong)
			{
				curseStrength = HatredStrengthType.Powerful;
			}
			return ((object)Dew.SelectRandomWeightedInList((from se in DewResources.FindAllByType<CurseStatusEffect>(ResourceLoadSettings.Light)
				where se.availableStrengths.HasFlag(curseStrength)
				select se).ToArray(), (CurseStatusEffect effect) => (!effect.IsViable(NetworkedManagerBase<ActorManager>.instance.allHeroes[0])) ? 0f : effect.chanceWeight, null)).GetType().Name;
		}
		case PenaltyType.HeroicBossSpecialSkill:
		{
			MiniBossEffect[] array = (from eff in DewResources.FindAllByType<MiniBossEffect>(default(ResourceLoadSettings))
				where !instance.heroicBossEffects.Contains((SyncableAssetRef)(UnityEngine.Object)(object)eff)
				select eff).ToArray();
			return ((object)array[UnityEngine.Random.Range(0, array.Length)]).GetType().Name;
		}
		default:
			return null;
		}
	}

	public string GetChoiceNameAdv(ChoiceItem item)
	{
		return DewAdvText.Adv_LocUI($"Shrine_MarshOfDestiny_SeedOfTorment_Choice_{item.penalty}_Name");
	}

	public string GetChoicePenaltyDescriptionAdv(ChoiceItem item)
	{
		string text = DewAdvText.Adv_LocUI(item.isTempPenalty ? $"Shrine_MarshOfDestiny_SeedOfTorment_Choice_{item.penalty}_Temp_Description" : $"Shrine_MarshOfDestiny_SeedOfTorment_Choice_{item.penalty}_Description");
		switch (item.penalty)
		{
		case PenaltyType.ArmorFlat:
			return DewAdvText.Adv_Format(text, ParameterAsFloat());
		case PenaltyType.MaxHealthPercentage:
		case PenaltyType.AtkSpdPercentage:
		case PenaltyType.MovSpdPercentage:
		case PenaltyType.MirageSkinAnyPercentage:
			return DewAdvText.Adv_Format(text, ParameterAsFloat("percent"));
		case PenaltyType.MirageSkinSpecificPercentage:
		{
			(string, float) tuple = DewPersistence.FromJson<(string, float)>(item.penaltyParameter);
			string text4 = DewAdvText.Adv_LocUI(tuple.Item1 + "_Name");
			string text5 = DewAdvText.Adv_Number(tuple.Item2, "percent");
			return DewAdvText.Adv_Format(text, text4, text5);
		}
		case PenaltyType.RandomCurseEveryTravelThisZone:
			return text;
		case PenaltyType.CurseThisZone:
		{
			string curseKey = DewLocalization.GetCurseKey(item.penaltyParameter);
			string text2 = DewAdvText.Adv_LocCurseName(curseKey);
			string text3 = DewAdvText.Adv_LocCurseShortDesc(curseKey);
			return DewAdvText.Adv_Format(text, "<color=#ff9696>" + text2 + "</color> (" + text3 + ")");
		}
		case PenaltyType.HeroicBossSpecialSkill:
			return DewAdvText.Adv_Format(text, "<color=yellow>" + DewAdvText.Adv_LocUI(item.penaltyParameter + "_Name") + "</color>");
		default:
			throw new ArgumentOutOfRangeException();
		}
		string ParameterAsFloat(string format = "#,##0")
		{
			return DewAdvText.Adv_Number(DewPersistence.FromJson<float>(item.penaltyParameter), format);
		}
	}

	public string GetChoiceRewardDescriptionAdv(ChoiceItem item)
	{
		RewardType reward = item.reward;
		if (reward == RewardType.Chaos || reward == RewardType.Memory || reward == RewardType.Essence)
		{
			Rarity rarity = DewPersistence.FromJson<Rarity>(item.rewardParameter);
			string arg = ((item.reward == RewardType.Memory) ? "Skill" : item.reward.ToString());
			string text = DewAdvText.Adv_LocUI($"InGame_Tooltip_{rarity}{arg}");
			return DewAdvText.Adv_Format(DewAdvText.Adv_LocUI("Shrine_MarshOfDestiny_SeedOfTorment_RewardItem"), "<color=" + Dew.GetRarityColorHex(rarity) + ">" + text + "</color>");
		}
		reward = item.reward;
		if (reward == RewardType.Gold || reward == RewardType.DreamDust || reward == RewardType.PlatinumCoin)
		{
			int num = DewPersistence.FromJson<int>(item.rewardParameter);
			string template = DewAdvText.Adv_LocUI($"Currency_Template_{item.reward}");
			string text2 = item.reward switch
			{
				RewardType.Gold => Dew.GetCurrencyColorHex(CurrencyType.Gold), 
				RewardType.DreamDust => Dew.GetCurrencyColorHex(CurrencyType.DreamDust), 
				RewardType.PlatinumCoin => Dew.GetCurrencyColorHex(CurrencyType.PlatinumCoin), 
				_ => throw new ArgumentOutOfRangeException(), 
			};
			string text3 = DewAdvText.Adv_Format(template, "<color=" + text2 + ">" + DewAdvText.Adv_Number(num) + "</color>");
			return DewAdvText.Adv_Format(DewAdvText.Adv_LocUI("Shrine_MarshOfDestiny_SeedOfTorment_RewardItem"), text3);
		}
		throw new ArgumentOutOfRangeException();
	}

	public Shrine_MarshOfDestiny_SeedOfTorment()
	{
		((NetworkBehaviour)this).InitSyncObject((SyncObject)(object)choices);
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_TpcOpenSelection__NetworkConnectionToClient(NetworkConnectionToClient target)
	{
		ManagerBase<FloatingWindowManager>.instance.SetTarget((MonoBehaviour)(object)this);
	}

	protected static void InvokeUserCode_TpcOpenSelection__NetworkConnectionToClient(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TpcOpenSelection called on server.");
		}
		else
		{
			((Shrine_MarshOfDestiny_SeedOfTorment)(object)obj).UserCode_TpcOpenSelection__NetworkConnectionToClient((NetworkConnectionToClient)(object)NetworkClient.connection);
		}
	}

	protected void UserCode_CmdChoose__Int32__NetworkConnectionToClient(int index, NetworkConnectionToClient sender)
	{
		DewPlayer player = sender.GetPlayer();
		Hero hero;
		if (!((UnityEngine.Object)(object)player == null) && index >= 0 && index < choices.Count)
		{
			hero = player.hero;
			if (!((UnityEngine.Object)(object)hero == null) && CanInteract(hero))
			{
				((MonoBehaviour)(object)this).StartCoroutine(Routine());
			}
		}
		IEnumerator Routine()
		{
			NetworkedManagerBase<GameManager>.instance.LockMidRunSave();
			try
			{
				DoPostUseRoutines(hero);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
			yield return new WaitForSeconds(0.65f);
			try
			{
				ApplyChoiceToGame(choices[index], player);
			}
			catch (Exception exception2)
			{
				Debug.LogException(exception2);
			}
			yield return new WaitForSeconds(0.65f);
			if (!Rift_RoomExit.instance.IsNullOrInactive())
			{
				Rift_RoomExit.instance.isLocked = false;
			}
			NetworkedManagerBase<GameManager>.instance.UnlockMidRunSave();
		}
	}

	protected static void InvokeUserCode_CmdChoose__Int32__NetworkConnectionToClient(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdChoose called on client.");
		}
		else
		{
			((Shrine_MarshOfDestiny_SeedOfTorment)(object)obj).UserCode_CmdChoose__Int32__NetworkConnectionToClient(NetworkReaderExtensions.ReadInt(reader), senderConnection);
		}
	}

	static Shrine_MarshOfDestiny_SeedOfTorment()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected Obj, but got Unknown
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Expected Obj, but got Unknown
		RemoteProcedureCalls.RegisterCommand(typeof(Shrine_MarshOfDestiny_SeedOfTorment), "System.Void Shrine_MarshOfDestiny_SeedOfTorment::CmdChoose(System.Int32,Mirror.NetworkConnectionToClient)", (RemoteCallDelegate)InvokeUserCode_CmdChoose__Int32__NetworkConnectionToClient, false);
		RemoteProcedureCalls.RegisterRpc(typeof(Shrine_MarshOfDestiny_SeedOfTorment), "System.Void Shrine_MarshOfDestiny_SeedOfTorment::TpcOpenSelection(Mirror.NetworkConnectionToClient)", (RemoteCallDelegate)InvokeUserCode_TpcOpenSelection__NetworkConnectionToClient);
	}
}
