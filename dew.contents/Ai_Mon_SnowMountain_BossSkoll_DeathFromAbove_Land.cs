public class Ai_Mon_SnowMountain_BossSkoll_DeathFromAbove_Land : InstantDamageInstance
{
	public float stunDuration = 1.5f;

	public override bool reuseInRoom => true;

	protected override void OnHit(Entity entity)
	{
		base.OnHit(entity);
		CreateBasicEffect(entity, new StunEffect(), stunDuration, "skoll_landstun", DuplicateEffectBehavior.UsePrevious);
	}

	private void MirrorProcessed()
	{
	}
}
