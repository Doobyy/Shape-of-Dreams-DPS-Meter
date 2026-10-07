public class Ai_Star_Bismuth_D_BonusAttackEffect_Arrow : StandardProjectile
{
	public override bool reuseInRoom => true;

	protected override void OnEntity(EntityHit hit)
	{
		base.OnEntity(hit);
		TriggerAttackEffects(info.caster, hit.entity, 1f, AttackEffectType.Others);
		Destroy();
	}

	private void MirrorProcessed()
	{
	}
}
