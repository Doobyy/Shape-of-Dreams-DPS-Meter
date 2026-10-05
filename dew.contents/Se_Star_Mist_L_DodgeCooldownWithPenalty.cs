using System;
using Mirror;

public class Se_Star_Mist_L_DodgeCooldownWithPenalty : StarEffect
{
	public StarScalingValue reducedCooldown;

	private SkillBonus _bonus;

	public override Type heroType => typeof(Hero_Mist);

	public override bool isMovementSkillType => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			_bonus = new SkillBonus
			{
				addedCharge = -1,
				ignoreReceiveCooldownReductionFlag = true,
				cooldownOffset = 0f - GetValue(reducedCooldown)
			};
			hero.Skill.Movement.AddSkillBonus(_bonus);
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
