public class Ai_U_HerWorld_Explosion : InstantDamageInstance
{
	public float stunDuration = 2.5f;

	public override bool reuseInRoom => true;

	protected override void OnHit(Entity entity)
	{
		base.OnHit(entity);
		CreateBasicEffect(entity, new StunEffect(), stunDuration);
	}

	private void MirrorProcessed()
	{
	}
}
