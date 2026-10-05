using Mirror;
using UnityEngine;

public class Se_Star_D_UltimateChargeSpeed : StarEffect
{
	public StarScalingValue chargeSpeedBonusRatio;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		DoAbility((AbilityTrigger at) =>
		{
			if (!(at is SkillTrigger skillTrigger))
			{
				return false;
			}
			if (skillTrigger.type == SkillType.Ultimate)
			{
				return true;
			}
			SkillTrigger byType = DewResources.GetByType<SkillTrigger>(((object)skillTrigger).GetType(), default(ResourceLoadSettings));
			return !((Object)(object)byType == null) && byType.type == SkillType.Ultimate;
		}, (AbilityTrigger at) =>
		{
			SkillTrigger skillTrigger = (SkillTrigger)at;
			SkillBonus newBonus = new SkillBonus
			{
				cooldownMultiplier = 1f / (1f + GetValue(chargeSpeedBonusRatio))
			};
			skillTrigger.AddSkillBonus(newBonus);
			return () =>
			{
				newBonus.Stop();
			};
		});
	}

	private void MirrorProcessed()
	{
	}
}
