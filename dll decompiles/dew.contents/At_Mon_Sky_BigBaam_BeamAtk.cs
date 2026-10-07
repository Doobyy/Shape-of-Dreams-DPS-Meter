using System;

public class At_Mon_Sky_BigBaam_BeamAtk : AttackTrigger
{
	[NonSerialized]
	public bool spawnAdditionalProjectile;

	public override AbilityInstance OnCastComplete(int configIndex, CastInfo info)
	{
		if (spawnAdditionalProjectile)
		{
			info.angle += 15f;
			base.OnCastComplete(configIndex, info);
			info.angle -= 15f;
			base.OnCastComplete(configIndex, info);
			info.angle -= 15f;
			return base.OnCastComplete(configIndex, info);
		}
		return base.OnCastComplete(configIndex, info);
	}

	private void MirrorProcessed()
	{
	}
}
