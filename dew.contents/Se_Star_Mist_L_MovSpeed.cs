using System;
using Mirror;

public class Se_Star_Mist_L_MovSpeed : StarEffect
{
	public StarScalingValue movSpeedBonus;

	public float amp = 0.5f;

	public float ampHpThreshold = 0.9f;

	private StatBonus _bonus;

	public override Type heroType => typeof(Hero_Mist);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			_bonus = DoStatBonus();
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (((NetworkBehaviour)this).isServer)
		{
			float num = GetValue(movSpeedBonus);
			if (victim.normalizedHealth > ampHpThreshold)
			{
				num *= 1f + amp;
			}
			if (Math.Abs(_bonus.movementSpeedPercentage - num) > 0.1f)
			{
				_bonus.movementSpeedPercentage = num;
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
