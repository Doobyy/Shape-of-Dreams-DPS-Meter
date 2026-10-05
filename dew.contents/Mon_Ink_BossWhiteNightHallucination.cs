using Mirror;

public class Mon_Ink_BossWhiteNightHallucination : Monster
{
	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			CreateBasicEffect(this, new InvisibleEffect
			{
				ignoreReveal = true
			}, float.PositiveInfinity);
			CreateBasicEffect(this, new UntargetableEffect(), float.PositiveInfinity);
			CreateBasicEffect(this, new InvulnerableEffect(), float.PositiveInfinity);
			CreateBasicEffect(this, new UncollidableEffect(), float.PositiveInfinity);
			Visual.HideGroundMarker();
		}
	}

	private void MirrorProcessed()
	{
	}
}
