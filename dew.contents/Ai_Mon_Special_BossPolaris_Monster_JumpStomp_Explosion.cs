public class Ai_Mon_Special_BossPolaris_Monster_JumpStomp_Explosion : InstantDamageInstance
{
	protected override void OnAfterDelay()
	{
		base.OnAfterDelay();
		CreateAbilityInstance<Ai_Mon_Special_BossPolaris_Monster_JumpStomp_LargerExplosion>(position, null, info);
	}

	private void MirrorProcessed()
	{
	}
}
