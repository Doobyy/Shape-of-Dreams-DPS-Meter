using System;

public class At_Mon_Despair_ParalyticFly_Atk : AttackTrigger
{
	[NonSerialized]
	public bool spawnAdditionalProjectile;

	public float addProjAngle = 15f;

	public override AbilityInstance OnCastComplete(int configIndex, CastInfo info)
	{
		if (info.caster.Ability.TryGetAbility<At_Mon_Forest_SpiderSpitter_Melee>(out var trigger))
		{
			trigger.OnCastCompleteSetCharge(0, default);
			trigger.OnCastCompleteSetCooldownTime(0, default);
		}
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
