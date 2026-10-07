using System;
using Mirror;
using UnityEngine;

public class Se_Star_Yubar_L_DodgeCooldownWithShorterRange : StarEffect
{
	public float cooldownReduction;

	public StarScalingValue rangeReduction;

	private SkillBonus _bonus;

	public override Type heroType => typeof(Hero_Yubar);

	public override bool isMovementSkillType => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			_bonus = hero.Skill.Movement.AddSkillBonus(new SkillBonus
			{
				ignoreReceiveCooldownReductionFlag = true,
				cooldownOffset = 0f - cooldownReduction
			});
			hero.Skill.Movement.configs[0].castMethod._range *= 1f - GetValue(rangeReduction);
			hero.Skill.Movement.SyncCastMethodChanges(0);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			if (_bonus != null)
			{
				_bonus.Stop();
				_bonus = null;
			}
			if ((UnityEngine.Object)(object)hero != null)
			{
				hero.Skill.Movement.configs[0].castMethod._range /= 1f - GetValue(rangeReduction);
				hero.Skill.Movement.SyncCastMethodChanges(0);
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
