public class Ai_Mon_Primus_BossPrimusAeron_Rage_MassSilence_Instance : InstantDamageInstance
{
	public override bool reuseInRoom => true;

	protected override void OnHit(Entity entity)
	{
		base.OnHit(entity);
		if (entity.Status.TryGetStatusEffect<Se_MirageSkin_Oblivion_Silenced>(out var effect))
		{
			effect.ResetTimer();
		}
		else
		{
			CreateStatusEffect<Se_MirageSkin_Oblivion_Silenced>(entity);
		}
	}

	private void MirrorProcessed()
	{
	}
}
