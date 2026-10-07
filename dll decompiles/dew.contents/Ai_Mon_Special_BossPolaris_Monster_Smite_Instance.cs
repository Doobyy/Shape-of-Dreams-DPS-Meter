public class Ai_Mon_Special_BossPolaris_Monster_Smite_Instance : InstantDamageInstance
{
	public float stunDuration = 1f;

	protected override void OnHit(Entity entity)
	{
		base.OnHit(entity);
		CreateBasicEffect(entity, new StunEffect(), stunDuration);
		ApplyElemental(ElementalType.Fire, entity);
	}

	protected override void OnAfterDelay()
	{
		base.OnAfterDelay();
		CreateAbilityInstance<Ai_Mon_Special_BossPolaris_Monster_Smite_TickInstance>(position, null, info);
	}

	private void MirrorProcessed()
	{
	}
}
