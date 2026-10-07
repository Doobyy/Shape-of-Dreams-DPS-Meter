using Mirror;

public class Se_Star_L_DodgeCooldown : StarEffect
{
	public StarScalingValue reductionAmount;

	private SkillBonus _bonus;

	public override bool isMovementSkillType => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer && victim is Hero hero && !hero.Skill.Movement.IsNullOrInactive())
		{
			_bonus = hero.Skill.Movement.AddSkillBonus(new SkillBonus
			{
				ignoreReceiveCooldownReductionFlag = true,
				cooldownMultiplier = 1f - GetValue(reductionAmount)
			});
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && _bonus != null)
		{
			_bonus.Stop();
			_bonus = null;
		}
	}

	private void MirrorProcessed()
	{
	}
}
