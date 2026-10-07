public class Ai_QR_DistortedMind_Damage : InstantDamageInstance
{
	public float stunDuration = 0.5f;

	public override bool reuseInRoom => true;

	protected override void OnHit(Entity entity)
	{
		base.OnHit(entity);
		if (stunDuration > 0f)
		{
			CreateBasicEffect(entity, new StunEffect(), stunDuration);
		}
	}

	private void MirrorProcessed()
	{
	}
}
