public class Ai_MirageSkin_Delusion_Missile : StandardProjectile
{
	public override bool reuseInRoom => true;

	protected override void OnComplete()
	{
		base.OnComplete();
		CreateAbilityInstance<Ai_MirageSkin_Delusion_Missile_Explode>(info.point, null, new CastInfo(info.caster));
	}

	private void MirrorProcessed()
	{
	}
}
