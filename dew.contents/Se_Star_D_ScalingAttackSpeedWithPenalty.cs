using System;
using Mirror;
using UnityEngine;

public class Se_Star_D_ScalingAttackSpeedWithPenalty : StarEffect
{
	public float atkSpdPercentageBasePenalty;

	public StarScalingValue atkSpdPercentagePerLevel;

	private StatBonus _bonus;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			_bonus = DoStatBonus();
			hero.ClientHeroEvent_OnLevelChanged += new Action<EventInfoHeroLevelUp>(ClientHeroEventOnLevelChanged);
			UpdateStatBonus();
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)hero != null)
		{
			hero.ClientHeroEvent_OnLevelChanged -= new Action<EventInfoHeroLevelUp>(ClientHeroEventOnLevelChanged);
		}
	}

	private void ClientHeroEventOnLevelChanged(EventInfoHeroLevelUp obj)
	{
		UpdateStatBonus();
	}

	private void UpdateStatBonus()
	{
		_bonus.attackSpeedPercentage = 0f - atkSpdPercentageBasePenalty + GetValue(atkSpdPercentagePerLevel) * (float)(victim.level - 1);
	}

	private void MirrorProcessed()
	{
	}
}
