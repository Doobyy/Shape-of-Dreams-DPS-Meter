using System;
using Mirror;

public class Se_Star_Yubar_L_DodgeChargeWithLongerCooldown : StarEffect
{
	public StarScalingValue cooldownTimePenalty;

	private SkillBonus _bonus;

	public override Type heroType => typeof(Hero_Yubar);

	public override bool isMovementSkillType => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			_bonus = ((Hero)victim).Skill.Movement.AddSkillBonus(new SkillBonus
			{
				ignoreReceiveCooldownReductionFlag = true,
				cooldownOffset = GetValue(cooldownTimePenalty),
				addedCharge = 1
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
