public class Ai_Mon_Despair_ParalyticFly_Atk : AttackProjectile
{
	public override bool reuseInRoom => true;

	protected override void OnEntity(EntityHit hit)
	{
		base.OnEntity(hit);
		if (hit.entity.Status.TryGetStatusEffect<Se_Mon_Despair_ParalyticFly_Atk_Instance>(out var effect))
		{
			effect.ResetTimer();
		}
		else
		{
			CreateStatusEffect<Se_Mon_Despair_ParalyticFly_Atk_Instance>(hit.entity, info);
		}
	}

	private void MirrorProcessed()
	{
	}
}
