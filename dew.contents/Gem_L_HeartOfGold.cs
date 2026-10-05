using UnityEngine;

public class Gem_L_HeartOfGold : Gem
{
	public ScalingValue ampPerHundredGold;

	public ScalingValue spentGoldRatio;

	public GameObject castEffect;

	public GameObject hitEffect;

	public float currentAmp
	{
		get
		{
			if (!((Object)(object)owner != null) || !((Object)(object)owner.owner != null))
			{
				return 0f;
			}
			return (float)owner.owner.gold / 100f * GetValue(ampPerHundredGold);
		}
	}

	protected override void OnCastCompleteBeforePrepare(EventInfoCast info)
	{
		base.OnCastCompleteBeforePrepare(info);
		if (owner.owner.gold <= 0 || !info.trigger.configs[info.configIndex].canConsumeCastBonus)
		{
			return;
		}
		info.instance.dealtDamageProcessor.Add(delegate(ref DamageData data, Actor actor, Entity target)
		{
			if (isValid && owner.CheckEnemyOrNeutral(target))
			{
				FxPlayNewNetworked(hitEffect, target);
				if (!data.IsAmountModifiedBy(this))
				{
					data.SetAttr(DamageAttribute.IsCrit);
					data.ApplyAmplification(currentAmp);
					data.SetAmountModifiedBy(this);
					NotifyUse();
				}
			}
		});
		info.instance.dealtHealProcessor.Add(delegate(ref HealData data, Actor actor, Entity target)
		{
			if (isValid && !owner.CheckEnemyOrNeutral(target))
			{
				FxPlayNewNetworked(hitEffect, target);
				if (!data.IsAmountModifiedBy(this))
				{
					data.SetCrit();
					data.ApplyAmplification(currentAmp);
					data.SetAmountModifiedBy(this);
					NotifyUse();
				}
			}
		});
		NotifyUse();
		owner.owner.SpendGold(Mathf.Max(1, DewMath.RandomRoundToInt((float)owner.owner.gold * GetValue(spentGoldRatio))));
		FxPlayNewNetworked(castEffect, owner);
	}

	private void MirrorProcessed()
	{
	}
}
