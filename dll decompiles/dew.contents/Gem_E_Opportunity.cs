using UnityEngine;

public class Gem_E_Opportunity : Gem
{
	public ScalingValue reductionAmount;

	public GameObject fxActivate;

	protected override void OnCastComplete(EventInfoCast info)
	{
		base.OnCastComplete(info);
		if (isValid && IsReady() && !((Object)(object)owner.Skill.Movement == null) && owner.Skill.Movement.currentCharges[0] < owner.Skill.Movement.configs[0].maxCharges)
		{
			bool canReceiveCooldownReduction = owner.Skill.Movement.configs[0].canReceiveCooldownReduction;
			owner.Skill.Movement.configs[0].canReceiveCooldownReduction = true;
			ApplyCooldownReduction(owner.Skill.Movement, GetValue(reductionAmount), scaled: false);
			owner.Skill.Movement.configs[0].canReceiveCooldownReduction = canReceiveCooldownReduction;
			StartCooldown();
			NotifyUse();
			FxPlayNewNetworked(fxActivate, owner);
		}
	}

	private void MirrorProcessed()
	{
	}
}
