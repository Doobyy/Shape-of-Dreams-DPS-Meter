public class Ai_MiniBoss_BloodThorn_SubThorn : InstantDamageInstance
{
	public override bool reuseInRoom => true;

	protected override void OnHit(Entity entity)
	{
		base.OnHit(entity);
		if (entity.Status.TryGetStatusEffect<Se_MiniBoss_BloodThorn_Bleeding>(out var effect))
		{
			effect.ResetTimer();
		}
		else
		{
			CreateStatusEffect<Se_MiniBoss_BloodThorn_Bleeding>(entity, new CastInfo(info.caster, entity));
		}
	}

	private void MirrorProcessed()
	{
	}
}
