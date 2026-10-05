using UnityEngine;

public class Gem_R_Epiphany : Gem
{
	public GameObject fxStart;

	public ScalingValue refundAmount;

	public float refundCooldownRatio => 1f - 1f / (1f + GetValue(refundAmount));

	protected override void OnCastComplete(EventInfoCast info)
	{
		base.OnCastComplete(info);
		if (isValid && IsReady())
		{
			FxPlayNewNetworked(fxStart, owner);
			if (owner.Status.TryGetStatusEffect<Se_Gem_R_Epiphany_ApBuff>(out var effect))
			{
				effect.ResetTimer();
			}
			else
			{
				CreateStatusEffectWithSource<Se_Gem_R_Epiphany_ApBuff>(this, owner, new CastInfo(owner));
			}
			StartCooldown();
			NotifyUse();
			if (skill.type == SkillType.Ultimate)
			{
				ApplyCooldownReductionByRatio(skill, refundCooldownRatio);
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
