using Mirror;
using UnityEngine;

public class Se_Star_L_DodgeCooldownOnLowHealth : StarEffect
{
	public StarScalingValue reductionAmount;

	public float healthThreshold;

	private SkillBonus _bonus;

	private float _lastUpdateTime;

	public override bool isMovementSkillType => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer && victim is Hero hero && !hero.Skill.Movement.IsNullOrInactive())
		{
			_bonus = hero.Skill.Movement.AddSkillBonus(new SkillBonus
			{
				ignoreReceiveCooldownReductionFlag = true
			});
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (((NetworkBehaviour)this).isServer && Time.time - _lastUpdateTime > 0.33f)
		{
			_lastUpdateTime = Time.time;
			_bonus.cooldownMultiplier = ((victim.normalizedHealth < healthThreshold) ? (1f - GetValue(reductionAmount)) : 1f);
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
