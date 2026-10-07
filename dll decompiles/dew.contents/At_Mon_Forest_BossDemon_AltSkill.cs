public class At_Mon_Forest_BossDemon_AltSkill : AbilityTrigger
{
	public override AbilityInstance OnCastComplete(int configIndex, CastInfo info)
	{
		AbilityInstance result = base.OnCastComplete(configIndex, info);
		currentConfigIndex = ((currentConfigIndex == 0) ? 1 : 0);
		SetChargeAll(0);
		return result;
	}

	private void MirrorProcessed()
	{
	}
}
