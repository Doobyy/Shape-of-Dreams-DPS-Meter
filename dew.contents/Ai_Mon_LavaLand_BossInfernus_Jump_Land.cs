public class Ai_Mon_LavaLand_BossInfernus_Jump_Land : InstantDamageInstance
{
	public float stunDuration = 1.5f;

	public override bool reuseInRoom => true;

	protected override void OnHit(Entity entity)
	{
		base.OnHit(entity);
		CreateBasicEffect(entity, new StunEffect(), stunDuration, "infernus_landstun", DuplicateEffectBehavior.UsePrevious);
	}

	private void MirrorProcessed()
	{
	}
}
