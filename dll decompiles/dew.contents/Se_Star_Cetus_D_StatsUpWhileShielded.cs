using System;
using Mirror;

public class Se_Star_Cetus_D_StatsUpWhileShielded : StarEffect
{
	public StarScalingValue addedAp;

	public StarScalingValue addedHaste;

	private StatBonus _bonus;

	private bool _didHaveShield;

	public override Type heroType => typeof(Hero_Cetus);

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
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		bool flag = hero.Status.currentShield > 0f;
		if (_didHaveShield != flag)
		{
			_didHaveShield = flag;
			if (_didHaveShield)
			{
				_bonus.abilityHasteFlat = GetValueInt(addedHaste);
				_bonus.abilityPowerFlat = GetValueInt(addedAp);
			}
			else
			{
				_bonus.abilityHasteFlat = 0f;
				_bonus.abilityPowerFlat = 0f;
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
