using System;

public class Ai_Gem_R_Ricochet : StandardProjectile
{
	[NonSerialized]
	public bool isHeal;

	[NonSerialized]
	public FinalDamageData originDamage;

	[NonSerialized]
	public FinalHealData originHeal;

	public ScalingValue ricochetRatio;

	public override bool reuseInRoom => true;

	protected override void OnEntity(EntityHit hit)
	{
		base.OnEntity(hit);
		if (isHeal)
		{
			Heal((originHeal.amount + originHeal.discardedAmount) * GetValue(ricochetRatio)).SetAmountOrigin(originHeal).Dispatch(hit.entity, chain);
			return;
		}
		DamageData damageData = MagicDamage((originDamage.amount + originDamage.discardedAmount) * GetValue(ricochetRatio)).SetSourceType(originDamage.type).SetAmountOrigin(originDamage);
		if (originDamage.elemental.HasValue)
		{
			damageData.SetElemental(originDamage.elemental.Value);
		}
		damageData.Dispatch(hit.entity, chain);
	}

	private void MirrorProcessed()
	{
	}
}
