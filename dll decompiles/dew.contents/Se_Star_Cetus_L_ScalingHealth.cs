using System;
using Mirror;
using UnityEngine;

public class Se_Star_Cetus_L_ScalingHealth : StarEffect
{
	public StarScalingValue addedHealthPerLevel;

	private StatBonus _bonus;

	public override Type heroType => typeof(Hero_Cetus);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			hero.ClientHeroEvent_OnLevelChanged += new Action<EventInfoHeroLevelUp>(ClientHeroEventOnLevelChanged);
			_bonus = DoStatBonus();
			UpdateBonus();
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (bool)(UnityEngine.Object)(object)hero)
		{
			hero.ClientHeroEvent_OnLevelChanged -= new Action<EventInfoHeroLevelUp>(ClientHeroEventOnLevelChanged);
		}
	}

	private void UpdateBonus()
	{
		_bonus.maxHealthFlat = (float)(hero.level - 1) * GetValue(addedHealthPerLevel);
	}

	private void ClientHeroEventOnLevelChanged(EventInfoHeroLevelUp obj)
	{
		UpdateBonus();
	}

	private void MirrorProcessed()
	{
	}
}
