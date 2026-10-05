using System;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Shrine_Hatred_Reward : Shrine, IRewardActor
{
	public int bonusSkillLevel = 1;

	public int bonusEssenceQuality = 50;

	public float goldWeight = 1f;

	public float dreamDustWeight = 1f;

	public float skillWeight = 1f;

	public float gemWeight = 1f;

	public float chaosWeight = 1f;

	[Space(16f)]
	public float[] goldMultipliers;

	public float[] dreamDustMultipliers;

	public float currencyDeviation = 0.15f;

	[NonSerialized]
	[SyncVar]
	[SaveVar(SaveVarFlags.Default)]
	public HatredStrengthType type;

	[NonSerialized]
	[SyncVar]
	[SaveVar(SaveVarFlags.Default)]
	public string playerGuid;

	public HatredStrengthType Networktype
	{
		get
		{
			return type;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<HatredStrengthType>(value, ref type, 256uL, (Action<HatredStrengthType, HatredStrengthType>)null);
		}
	}

	public string NetworkplayerGuid
	{
		get
		{
			return playerGuid;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<string>(value, ref playerGuid, 512uL, (Action<string, string>)null);
		}
	}

	public override bool CanInteract(Entity entity)
	{
		if (base.CanInteract(entity) && (UnityEngine.Object)(object)entity.owner != null)
		{
			return entity.owner.guid == playerGuid;
		}
		return false;
	}

	protected override bool OnUse(Entity entity)
	{
		Hero hero;
		if (!((UnityEngine.Object)(object)entity.owner == null) && !(entity.owner.guid != playerGuid))
		{
			hero = entity as Hero;
			if (hero != null)
			{
				if (type == HatredStrengthType.None)
				{
					switch (Dew.SelectRandomWeightedIndexInParams(goldWeight, dreamDustWeight, skillWeight, gemWeight))
					{
					case 0:
						DropGoldReward();
						break;
					case 1:
						DropDreamDustReward();
						break;
					case 2:
						DropSkillReward(isHigh: false, 0, new Rarity[1]);
						break;
					case 3:
						DropGemReward(isHigh: false, 0, new Rarity[1]);
						break;
					}
					return true;
				}
				if (type == HatredStrengthType.Mild)
				{
					switch (Dew.SelectRandomWeightedIndexInParams(goldWeight, dreamDustWeight, skillWeight, gemWeight, chaosWeight))
					{
					case 0:
						DropGoldReward();
						break;
					case 1:
						DropDreamDustReward();
						break;
					case 2:
						DropSkillReward(isHigh: false, bonusSkillLevel, new Rarity[2]
						{
							Rarity.Common,
							Rarity.Rare
						});
						break;
					case 3:
						DropGemReward(isHigh: false, bonusEssenceQuality, new Rarity[2]
						{
							Rarity.Common,
							Rarity.Rare
						});
						break;
					case 4:
						DropChaosReward(isHigh: false, new Rarity[2]
						{
							Rarity.Common,
							Rarity.Rare
						});
						break;
					}
					return true;
				}
				if (type == HatredStrengthType.Potent)
				{
					switch (Dew.SelectRandomWeightedIndexInParams(goldWeight, dreamDustWeight, skillWeight, gemWeight, chaosWeight))
					{
					case 0:
						DropGoldReward();
						break;
					case 1:
						DropDreamDustReward();
						break;
					case 2:
						DropSkillReward(isHigh: true, bonusSkillLevel, new Rarity[2]
						{
							Rarity.Rare,
							Rarity.Epic
						});
						break;
					case 3:
						DropGemReward(isHigh: true, bonusEssenceQuality, new Rarity[2]
						{
							Rarity.Rare,
							Rarity.Epic
						});
						break;
					case 4:
						DropChaosReward(isHigh: true, new Rarity[2]
						{
							Rarity.Rare,
							Rarity.Epic
						});
						break;
					}
					return true;
				}
				switch (Dew.SelectRandomWeightedIndexInParams(goldWeight, dreamDustWeight, skillWeight, gemWeight, chaosWeight))
				{
				case 0:
					DropGoldReward();
					break;
				case 1:
					DropDreamDustReward();
					break;
				case 2:
					DropSkillReward(isHigh: true, bonusSkillLevel, new Rarity[2]
					{
						Rarity.Epic,
						Rarity.Legendary
					});
					break;
				case 3:
					DropGemReward(isHigh: true, bonusEssenceQuality, new Rarity[2]
					{
						Rarity.Epic,
						Rarity.Legendary
					});
					break;
				case 4:
					DropChaosReward(isHigh: true, new Rarity[2]
					{
						Rarity.Epic,
						Rarity.Legendary
					});
					break;
				}
				return true;
			}
		}
		return false;
		void DropChaosReward(bool isHigh, Rarity[] rarities)
		{
			Dew.CreateActor(position, null, null, (Shrine_Chaos chaos) =>
			{
				chaos.playersOverride = new string[1] { hero.owner.guid };
				(float, Rarity)[] array = new (float, Rarity)[rarities.Length];
				for (int i = 0; i < rarities.Length; i++)
				{
					array[i] = ((isHigh ? chaos.highChance : chaos.chance).Get(rarities[i]), rarities[i]);
				}
				Rarity rarity = Dew.SelectRandomWeightedInParams(array);
				chaos.rarity = rarity;
			});
			NetworkedManagerBase<ChatManager>.instance.SendChatMessage((NetworkConnection)(object)(NetworkConnectionToClient)hero.owner, new ChatManager.Message
			{
				type = ChatManager.MessageType.Notice,
				content = "Chat_Hatred_RewardedChaos"
			});
		}
		void DropDreamDustReward()
		{
			float value = NetworkedManagerBase<GameManager>.instance.GetSpecialRewardAmount_DreamDust() * dreamDustMultipliers[Shrine_Hatred.GetIndexOfStrength(type)] * UnityEngine.Random.Range(1f - currencyDeviation, 1f + currencyDeviation);
			NetworkedManagerBase<PickupManager>.instance.DropDreamDust(isGivenByOtherPlayer: false, DewMath.RandomRoundToInt(value), ((Component)(object)this).transform.position, hero);
			NetworkedManagerBase<ChatManager>.instance.SendChatMessage((NetworkConnection)(object)(NetworkConnectionToClient)hero.owner, new ChatManager.Message
			{
				type = ChatManager.MessageType.Notice,
				content = "Chat_Hatred_RewardedDreamDust"
			});
		}
		void DropGemReward(bool isHigh, int addedQuality, Rarity[] rarities)
		{
			(float, Rarity)[] array = new (float, Rarity)[rarities.Length];
			for (int i = 0; i < rarities.Length; i++)
			{
				array[i] = ((isHigh ? NetworkedManagerBase<LootManager>.instance.gemRarityChanceHigh : NetworkedManagerBase<LootManager>.instance.gemRarityChance).Get(rarities[i]), rarities[i]);
			}
			Rarity value = Dew.SelectRandomWeightedInParams(array);
			NetworkedManagerBase<LootManager>.instance.SelectGemAndQuality(value, out var gem, out var quality);
			Dew.CreateGem(gem, position, quality + addedQuality, hero.owner);
			NetworkedManagerBase<ChatManager>.instance.SendChatMessage((NetworkConnection)(object)(NetworkConnectionToClient)hero.owner, new ChatManager.Message
			{
				type = ChatManager.MessageType.Notice,
				content = "Chat_Hatred_RewardedEssence"
			});
		}
		void DropGoldReward()
		{
			float value = NetworkedManagerBase<GameManager>.instance.GetSpecialRewardAmount_Gold() * goldMultipliers[Shrine_Hatred.GetIndexOfStrength(type)] * UnityEngine.Random.Range(1f - currencyDeviation, 1f + currencyDeviation);
			NetworkedManagerBase<PickupManager>.instance.DropGold(isKillGold: false, isGivenByOtherPlayer: false, DewMath.RandomRoundToInt(value), ((Component)(object)this).transform.position, hero);
			NetworkedManagerBase<ChatManager>.instance.SendChatMessage((NetworkConnection)(object)(NetworkConnectionToClient)hero.owner, new ChatManager.Message
			{
				type = ChatManager.MessageType.Notice,
				content = "Chat_Hatred_RewardedGold"
			});
		}
		void DropSkillReward(bool isHigh, int addedLevel, Rarity[] rarities)
		{
			(float, Rarity)[] array = new (float, Rarity)[rarities.Length];
			for (int i = 0; i < rarities.Length; i++)
			{
				array[i] = ((isHigh ? NetworkedManagerBase<LootManager>.instance.skillRarityChanceHigh : NetworkedManagerBase<LootManager>.instance.skillRarityChance).Get(rarities[i]), rarities[i]);
			}
			Rarity value = Dew.SelectRandomWeightedInParams(array);
			NetworkedManagerBase<LootManager>.instance.SelectSkillAndLevel(value, out var skill, out var level);
			Dew.CreateSkillTrigger(skill, position, level + addedLevel, hero.owner);
			NetworkedManagerBase<ChatManager>.instance.SendChatMessage((NetworkConnection)(object)(NetworkConnectionToClient)hero.owner, new ChatManager.Message
			{
				type = ChatManager.MessageType.Notice,
				content = "Chat_Hatred_RewardedMemory"
			});
		}
	}

	private void MirrorProcessed()
	{
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			GeneratedNetworkCode._Write_HatredStrengthType(writer, type);
			NetworkWriterExtensions.WriteString(writer, playerGuid);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x100L) != 0L)
		{
			GeneratedNetworkCode._Write_HatredStrengthType(writer, type);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x200L) != 0L)
		{
			NetworkWriterExtensions.WriteString(writer, playerGuid);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<HatredStrengthType>(ref type, (Action<HatredStrengthType, HatredStrengthType>)null, GeneratedNetworkCode._Read_HatredStrengthType(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<string>(ref playerGuid, (Action<string, string>)null, NetworkReaderExtensions.ReadString(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x100L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<HatredStrengthType>(ref type, (Action<HatredStrengthType, HatredStrengthType>)null, GeneratedNetworkCode._Read_HatredStrengthType(reader));
		}
		if ((num & 0x200L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<string>(ref playerGuid, (Action<string, string>)null, NetworkReaderExtensions.ReadString(reader));
		}
	}
}
