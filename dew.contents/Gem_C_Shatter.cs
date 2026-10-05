using UnityEngine;

public class Gem_C_Shatter : Gem
{
	public ScalingValue dmgAmp;

	public ScalingValue dmgAmpLight;

	public GameObject castEffect;

	public GameObject hitEffect;

	protected override void OnCastCompleteBeforePrepare(EventInfoCast info)
	{
		base.OnCastCompleteBeforePrepare(info);
		if (IsReady() && info.trigger.configs[info.configIndex].canConsumeCastBonus)
		{
			info.instance.dealtDamageProcessor.Add(DamageAmp);
			NotifyUse();
			StartCooldown();
			FxPlayNewNetworked(castEffect, owner);
		}
	}

	private void DamageAmp(ref DamageData data, Actor actor, Entity target)
	{
		if (isValid && owner.CheckEnemyOrNeutral(target))
		{
			FxPlayNewNetworked(hitEffect, target);
			if (!data.IsAmountModifiedBy(this))
			{
				data.SetAttr(DamageAttribute.IsCrit);
				data.ApplyAmplification((data.elemental == ElementalType.Light) ? GetValue(dmgAmpLight) : GetValue(dmgAmp));
				data.SetAmountModifiedBy(this);
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
