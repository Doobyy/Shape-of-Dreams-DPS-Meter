public class St_C_SparklingWaterGun : SkillTrigger
{
	protected override bool ShouldTickCooldown(int configIndex)
	{
		if (base.ShouldTickCooldown(configIndex))
		{
			return owner.isInCombat;
		}
		return false;
	}

	private void MirrorProcessed()
	{
	}
}
