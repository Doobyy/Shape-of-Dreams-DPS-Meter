using Mirror;
using UnityEngine;

public class Shrine_AltarOfCleansing : EditSkillShrine
{
	public override Color GetEditSkillIndicatorColor()
	{
		return new Color(73f / 255f, 1f, 215f / 255f);
	}

	public override string GetEditSkillIndicatorRawText()
	{
		return DewLocalization.GetUIValue("InGame_SelectItemToCleanse");
	}

	public override Color GetEditSkillBackdropColor()
	{
		return new Color(0f, 1f, 190f / 255f);
	}

	public override EditSkillTargetInfo GetTargetInfo(DewPlayer player, GemLocation loc, Gem target)
	{
		return new EditSkillTargetInfo
		{
			nextLevel = NetworkedManagerBase<GameManager>.instance.GetCleanseGemMinQuality(),
			cost = Cost.Gold(NetworkedManagerBase<GameManager>.instance.GetCleanseGoldCost(target)),
			actionTypeRawText = DewLocalization.GetUIValue("InGame_Interact_Cleanse"),
			rejectReasonRawText = ((target.quality <= NetworkedManagerBase<GameManager>.instance.GetCleanseGemMinQuality()) ? DewLocalization.GetUIValue("InGame_Message_GemQualityTooLow") : null),
			tooltipRawText = ((target.quality <= NetworkedManagerBase<GameManager>.instance.GetCleanseGemMinQuality()) ? null : ("<color=#49ffd7>" + string.Format(DewLocalization.GetUIValue("Shrine_AltarOfCleansing_EditSkillTooltip_Essence"), NetworkedManagerBase<GameManager>.instance.GetCleanseGemMinQuality() + "%", NetworkedManagerBase<GameManager>.instance.GetCleanseReturnedDreamDust(target.owner.owner, target).ToString("#,##0")) + "</color>"))
		};
	}

	public override EditSkillTargetInfo GetTargetInfo(DewPlayer player, HeroSkillLocation loc, SkillTrigger target)
	{
		return new EditSkillTargetInfo
		{
			nextLevel = NetworkedManagerBase<GameManager>.instance.GetCleanseSkillMinLevel(),
			cost = Cost.Gold(NetworkedManagerBase<GameManager>.instance.GetCleanseGoldCost(target)),
			actionTypeRawText = DewLocalization.GetUIValue("InGame_Interact_Cleanse"),
			rejectReasonRawText = ((target.level <= NetworkedManagerBase<GameManager>.instance.GetCleanseSkillMinLevel()) ? DewLocalization.GetUIValue("InGame_Message_SkillLevelTooLow") : null),
			tooltipRawText = ((target.level <= NetworkedManagerBase<GameManager>.instance.GetCleanseSkillMinLevel()) ? null : ("<color=#49ffd7>" + string.Format(DewLocalization.GetUIValue("Shrine_AltarOfCleansing_EditSkillTooltip_Memory"), NetworkedManagerBase<GameManager>.instance.GetCleanseSkillMinLevel(), NetworkedManagerBase<GameManager>.instance.GetCleanseReturnedDreamDust(target.owner.owner, target).ToString("#,##0")) + "</color>"))
		};
	}

	protected override bool OnActivateEditSkill(DewPlayer player, HeroSkillLocation loc, SkillTrigger target)
	{
		int cleanseReturnedDreamDust = NetworkedManagerBase<GameManager>.instance.GetCleanseReturnedDreamDust(target.owner.owner, target);
		NetworkedManagerBase<PickupManager>.instance.DropDreamDust(isGivenByOtherPlayer: false, cleanseReturnedDreamDust, position, target.owner);
		NetworkedManagerBase<ClientEventManager>.instance.InvokeOnItemCleansed(target.owner, (NetworkBehaviour)(object)target);
		target.level = NetworkedManagerBase<GameManager>.instance.GetCleanseSkillMinLevel();
		return true;
	}

	protected override bool OnActivateEditSkill(DewPlayer player, GemLocation loc, Gem target)
	{
		int cleanseReturnedDreamDust = NetworkedManagerBase<GameManager>.instance.GetCleanseReturnedDreamDust(target.owner.owner, target);
		NetworkedManagerBase<PickupManager>.instance.DropDreamDust(isGivenByOtherPlayer: false, cleanseReturnedDreamDust, position, target.owner);
		NetworkedManagerBase<ClientEventManager>.instance.InvokeOnItemCleansed(target.owner, (NetworkBehaviour)(object)target);
		target.quality = NetworkedManagerBase<GameManager>.instance.GetCleanseGemMinQuality();
		return true;
	}

	private void MirrorProcessed()
	{
	}
}
