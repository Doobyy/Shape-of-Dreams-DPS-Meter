public class Ai_L_LightExplosion_Instance : InstantDamageInstance
{
	public float stunDuration = 1f;

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
