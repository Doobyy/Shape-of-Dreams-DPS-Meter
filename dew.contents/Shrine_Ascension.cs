using System.Collections.Generic;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class Shrine_Ascension : EditSkillShrine
{
	public float addedCostMultiplierPerUse = 0.1f;

	public PerRarityData<float> baseGoldCostByStartRarity;

	public Color indicatorColor;

	public Color backdropColor;

	public override Color GetEditSkillIndicatorColor()
	{
		return indicatorColor;
	}

	public override Color GetEditSkillBackdropColor()
	{
		return backdropColor;
	}

	public override string GetEditSkillIndicatorRawText()
	{
		return string.Format(base.GetEditSkillIndicatorRawText(), maxUseCountPerPlayer - GetUseCountByPlayer(DewPlayer.local));
	}

	public override EditSkillTargetInfo GetTargetInfo(DewPlayer player, HeroSkillLocation loc, SkillTrigger target)
	{
		return new EditSkillTargetInfo
		{
			cost = GetCost(target.rarity, target.owner.owner),
			actionTypeRawText = DewLocalization.GetUIValue("Shrine_Ascension_AscendAction"),
			tooltipRawText = GetTooltip(target.rarity, isMemory: true),
			rejectReasonRawText = GetRejectionReason(target.rarity)
		};
	}

	public override EditSkillTargetInfo GetTargetInfo(DewPlayer player, GemLocation loc, Gem target)
	{
		return new EditSkillTargetInfo
		{
			cost = GetCost(target.rarity, target.owner.owner),
			actionTypeRawText = DewLocalization.GetUIValue("Shrine_Ascension_AscendAction"),
			tooltipRawText = GetTooltip(target.rarity, isMemory: false),
			rejectReasonRawText = GetRejectionReason(target.rarity)
		};
	}

	private string GetRejectionReason(Rarity rarity)
	{
		if (rarity == Rarity.Identity || rarity == Rarity.Character)
		{
			return DewLocalization.GetUIValue("Shrine_Ascension_InvalidTarget");
		}
		return null;
	}

	private string GetTooltip(Rarity currentRarity, bool isMemory)
	{
		if (currentRarity == Rarity.Character || currentRarity == Rarity.Identity)
		{
			return null;
		}
		Rarity nextRarity = GetNextRarity(currentRarity);
		Color color = Color.Lerp(Color.white, Dew.GetRarityColor(nextRarity), 0.65f);
		string uIValue = DewLocalization.GetUIValue(isMemory ? "Shrine_Ascension_MemoryReplace" : "Shrine_Ascension_EssenceReplace");
		string uIValue2 = DewLocalization.GetUIValue(string.Format("InGame_Tooltip_{0}{1}", nextRarity, isMemory ? "Skill" : "Essence"));
		return "<color=" + Dew.GetHex(color) + ">" + string.Format(uIValue, uIValue2) + "</color>";
	}

	private Rarity GetNextRarity(Rarity rarity)
	{
		return rarity switch
		{
			Rarity.Common => Rarity.Rare, 
			Rarity.Rare => Rarity.Epic, 
			Rarity.Epic => Rarity.Legendary, 
			Rarity.Legendary => Rarity.Unique, 
			_ => Rarity.Common, 
		};
	}

	private Cost GetCost(Rarity rarity, DewPlayer player)
	{
		float num = baseGoldCostByStartRarity.Get(rarity);
		num *= 1f + addedCostMultiplierPerUse * (float)GetUseCountByPlayer(player);
		return Cost.Gold(Mathf.RoundToInt(NetworkedManagerBase<GameManager>.instance.GetAdjustedGoldAmount_Cost_Service(num)));
	}

	protected override bool OnActivateEditSkill(DewPlayer player, HeroSkillLocation loc, SkillTrigger target)
	{
		string name = ((object)target).GetType().Name;
		Hero owner = target.owner;
		Rarity nextRarity = GetNextRarity(target.rarity);
		List<string> list = NetworkedManagerBase<LootManager>.instance.poolSkillsByRarity[nextRarity];
		string text = list[Random.Range(0, list.Count)];
		SkillTrigger skillTrigger = Dew.CreateSkillTrigger(DewResources.GetByShortTypeName<SkillTrigger>(text, default(ResourceLoadSettings)), owner.position, target.level);
		HeroSkillLocation skillType = target.skillType;
		target.Destroy();
		owner.Skill.EquipSkill(skillType, skillTrigger);
		RpcShowNotice(player, name, text, skillTrigger.level);
		return true;
	}

	protected override bool OnActivateEditSkill(DewPlayer player, GemLocation loc, Gem target)
	{
		string name = ((object)target).GetType().Name;
		Hero hero = target.owner;
		Rarity nextRarity = GetNextRarity(target.rarity);
		string text = Dew.SelectRandomWeightedInList(NetworkedManagerBase<LootManager>.instance.poolGemsByRarity[nextRarity], (string type) => (!hero.Skill.HasGemOfType(type)) ? 1f : 0f, null);
		Gem gem = Dew.CreateGem(DewResources.GetByShortTypeName<Gem>(text, default(ResourceLoadSettings)), hero.position, target.quality);
		GemLocation location = target.location;
		target.Destroy();
		hero.Skill.EquipGem(location, gem);
		RpcShowNotice(player, name, text, gem.quality);
		return true;
	}

	[ClientRpc]
	private void RpcShowNotice(DewPlayer whom, string fromType, string toType, int level)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteNetworkBehaviour((NetworkWriter)(object)val, (NetworkBehaviour)(object)whom);
		NetworkWriterExtensions.WriteString((NetworkWriter)(object)val, fromType);
		NetworkWriterExtensions.WriteString((NetworkWriter)(object)val, toType);
		NetworkWriterExtensions.WriteInt((NetworkWriter)(object)val, level);
		((NetworkBehaviour)this).SendRPCInternal("System.Void Shrine_Ascension::RpcShowNotice(DewPlayer,System.String,System.String,System.Int32)", -1853925464, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_RpcShowNotice__DewPlayer__String__String__Int32(DewPlayer whom, string fromType, string toType, int level)
	{
		string text = "";
		string text2 = "";
		if (fromType.StartsWith("St_"))
		{
			text = ChatManager.GetColoredSkillName(fromType, level);
		}
		if (fromType.StartsWith("Gem_"))
		{
			text = ChatManager.GetColoredGemName(fromType, level);
		}
		if (toType.StartsWith("St_"))
		{
			text2 = ChatManager.GetColoredSkillName(toType, level);
		}
		if (toType.StartsWith("Gem_"))
		{
			text2 = ChatManager.GetColoredGemName(toType, level);
		}
		NetworkedManagerBase<ChatManager>.instance.ShowMessageLocally(new ChatManager.Message
		{
			type = ChatManager.MessageType.Notice,
			content = "Shrine_Ascension_ChatNotice",
			args = new string[3]
			{
				ChatManager.GetColoredDescribedPlayerName(whom),
				text,
				text2
			},
			itemLevel = level,
			itemType = toType
		});
		if (((NetworkBehaviour)whom).isLocalPlayer && Dew.TryGetTypeFromShortName(toType, out var type))
		{
			if (toType.StartsWith("St_") && DewSave.profileMain.DiscoverSkill(toType))
			{
				AchievementManager.lastGamePlayReward?.discoveredSkills.Add(type);
			}
			if (toType.StartsWith("Gem_") && DewSave.profileMain.DiscoverGem(toType))
			{
				AchievementManager.lastGamePlayReward?.discoveredGems.Add(type);
			}
		}
	}

	protected static void InvokeUserCode_RpcShowNotice__DewPlayer__String__String__Int32(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcShowNotice called on server.");
		}
		else
		{
			((Shrine_Ascension)(object)obj).UserCode_RpcShowNotice__DewPlayer__String__String__Int32(NetworkReaderExtensions.ReadNetworkBehaviour<DewPlayer>(reader), NetworkReaderExtensions.ReadString(reader), NetworkReaderExtensions.ReadString(reader), NetworkReaderExtensions.ReadInt(reader));
		}
	}

	static Shrine_Ascension()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected Obj, but got Unknown
		RemoteProcedureCalls.RegisterRpc(typeof(Shrine_Ascension), "System.Void Shrine_Ascension::RpcShowNotice(DewPlayer,System.String,System.String,System.Int32)", (RemoteCallDelegate)InvokeUserCode_RpcShowNotice__DewPlayer__String__String__Int32);
	}
}
