public class At_Mon_DarkCave_DarkElemental_Atk : AttackTrigger
{
	public bool spawnAdditionalProjectile;

	public float addProjAngle = 12f;

	public override AbilityInstance OnCastComplete(int configIndex, CastInfo info)
	{
		if (spawnAdditionalProjectile)
		{
			info.angle += addProjAngle;
			base.OnCastComplete(configIndex, info);
			info.angle -= addProjAngle;
			base.OnCastComplete(configIndex, info);
			info.angle -= addProjAngle;
			return base.OnCastComplete(configIndex, info);
		}
		return base.OnCastComplete(configIndex, info);
	}

	private void MirrorProcessed()
	{
	}
}
