public class At_Mon_Forest_SpiderSpitter_Melee : AbilityTrigger
{
	public override AbilityInstance OnCastComplete(int configIndex, CastInfo info)
	{
		if (info.caster.Ability.TryGetAbility<At_Mon_Forest_SpiderSpitter_Atk>(out var trigger))
		{
			trigger.OnCastCompleteSetCharge(0, default);
			trigger.OnCastCompleteSetCooldownTime(0, default);
		}
		return base.OnCastComplete(configIndex, info);
	}

	private void MirrorProcessed()
	{
	}
}
