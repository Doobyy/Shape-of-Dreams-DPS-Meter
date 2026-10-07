public class Ai_Mon_Forest_Treant_PowerBomb : InstantDamageInstance
{
	public Knockback knockback;

	public override bool reuseInRoom => true;

	protected override void OnHit(Entity entity)
	{
		base.OnHit(entity);
		knockback.ApplyWithOrigin(info.caster.position, entity);
	}

	private void MirrorProcessed()
	{
	}
}
