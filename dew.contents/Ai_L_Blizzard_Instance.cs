public class Ai_L_Blizzard_Instance : InstantDamageInstance
{
	public float shieldDuration = 4f;

	public bool isShieldDecay = true;

	public ScalingValue shieldAmount;

	public override bool reuseInRoom => true;

	protected override void OnAfterDelay()
	{
		base.OnAfterDelay();
		ListReturnHandle<Entity> handle;
		foreach (Entity entity in range.GetEntities(out handle, tvDefaultUsefulEffectTargets))
		{
			GiveShield(entity, GetValue(shieldAmount), shieldDuration, isShieldDecay);
			if (Adaptive_ShouldPlayEffects())
			{
				FxPlayNewNetworked(hitEffect, entity);
			}
		}
		handle.Return();
	}

	private void MirrorProcessed()
	{
	}
}
