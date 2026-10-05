public class Ai_Mon_Ink_Boss_OnDeath : StandardProjectile
{
	protected override void OnPrepare()
	{
		base.OnPrepare();
		SetCustomStartPosition(info.caster.Visual.GetCenterPosition());
	}

	private void MirrorProcessed()
	{
	}
}
