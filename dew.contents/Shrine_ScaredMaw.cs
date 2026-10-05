using Mirror;
using UnityEngine;

public class Shrine_ScaredMaw : EditSkillShrine
{
	public int bonusLevel = 3;

	public Color textColor;

	public Color backdropColor;

	public override string GetEditSkillIndicatorRawText()
	{
		return DewLocalization.GetUIValue("InGame_SelectItemToUpgrade");
	}

	public override Color GetEditSkillIndicatorColor()
	{
		return textColor;
	}

	public override Color GetEditSkillBackdropColor()
	{
		return backdropColor;
	}

	public override EditSkillTargetInfo GetTargetInfo(DewPlayer player, HeroSkillLocation loc, SkillTrigger target)
	{
		return new EditSkillTargetInfo
		{
			nextLevel = target.level + bonusLevel,
			actionTypeRawText = DewLocalization.GetUIValue("InGame_Interact_Upgrade"),
			rejectReasonRawText = ((!target.isLevelUpEnabled) ? DewLocalization.GetUIValue("InGame_Message_CantUpgradeThisSkill") : null)
		};
	}

	public override EditSkillTargetInfo GetTargetInfo(DewPlayer player, GemLocation loc, Gem target)
	{
		return new EditSkillTargetInfo
		{
			nextLevel = target.quality + NetworkedManagerBase<GameManager>.instance.ges.gemAddedQualityOnUpgrade * bonusLevel,
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
