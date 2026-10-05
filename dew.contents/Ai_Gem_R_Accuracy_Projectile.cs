using System;
using UnityEngine;

public class Ai_Gem_R_Accuracy_Projectile : StandardProjectile
{
	public ScalingValue damageRatio;

	[NonSerialized]
	public FinalDamageData data;

	public override bool reuseInRoom => true;

	protected override void OnEntity(EntityHit hit)
	{
		base.OnEntity(hit);
		float value = GetValue(damageRatio);
		DamageData damageData = new DamageData(data.type, (data.amount + data.discardedAmount) * value, Mathf.Clamp01(data.procCoefficient * value));
		if (data.HasAttr(DamageAttribute.IsCrit))
		{
			damageData.SetAttr(DamageAttribute.IsCrit);
		}
		if (data.HasAttr(DamageAttribute.AreaOfEffect))
		{
			damageData.SetAttr(DamageAttribute.AreaOfEffect);
		}
		if (data.HasAttr(DamageAttribute.DamageOverTime))
		{
			damageData.SetAttr(DamageAttribute.DamageOverTime);
		}
		if (data.elemental.HasValue)
		{
			damageData.SetElemental(data.elemental.Value);
		}
		damageData.SetActor(this);
		damageData.SetDirection(rotation);
		damageData.SetAmountOrigin(data);
		damageData.Dispatch(hit.entity, chain);
		Destroy();
	}

	private void MirrorProcessed()
	{
	}
}
