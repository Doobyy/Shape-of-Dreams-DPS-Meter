using Mirror;

public class Shrine_UpgradeWell : EditSkillShrine
{
	public override string GetEditSkillIndicatorRawText()
	{
		return DewLocalization.GetUIValue("InGame_SelectItemToUpgrade");
	}

	public override EditSkillTargetInfo GetTargetInfo(DewPlayer player, HeroSkillLocation loc, SkillTrigger target)
	{
		return new EditSkillTargetInfo
		{
			nextLevel = target.level + 1,
			cost = Cost.DreamDust(NetworkedManagerBase<GameManager>.instance.GetSkillUpgradeDreamDustCost(target)),
			actionTypeRawText = DewLocalization.GetUIValue("InGame_Interact_Upgrade"),
			rejectReasonRawText = ((!target.isLevelUpEnabled) ? DewLocalization.GetUIValue("InGame_Message_CantUpgradeThisSkill") : null)
		};
	}

	public override EditSkillTargetInfo GetTargetInfo(DewPlayer player, GemLocation loc, Gem target)
	{
		return new EditSkillTargetInfo
		{
			nextLevel = target.quality + NetworkedManagerBase<GameManager>.instance.GetGemUpgradeAddedQuality(),
			cost = Cost.DreamDust(NetworkedManagerBase<GameManager>.instance.GetGemUpgradeDreamDustCost(target)),
			actionTypeRawText = DewLocalization.GetUIValue("InGame_Interact_Upgrade")
		};
	}

	protected override bool OnActivateEditSkill(DewPlayer player, HeroSkillLocation loc, SkillTrigger target)
	{
		target.level = GetTargetInfo(player, loc, target).nextLevel.Value;
		NetworkedManagerBase<ClientEventManager>.instance.InvokeOnItemUpgraded(target.owner, (NetworkBehaviour)(object)target);
		return true;
	}

	protected override bool OnActivateEditSkill(DewPlayer player, GemLocation loc, Gem target)
	{
		target.quality = GetTargetInfo(player, loc, target).nextLevel.Value;
		NetworkedManagerBase<ClientEventManager>.instance.InvokeOnItemUpgraded(target.owner, (NetworkBehaviour)(object)target);
		return true;
	}

	private void MirrorProcessed()
	{
	}
}
