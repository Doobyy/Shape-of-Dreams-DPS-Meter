using System;
using Mirror;
using UnityEngine;

public class Se_Star_L_ScalingMaxHealth : StarEffect
{
	public StarScalingValue healthPerLevel;

	private StatBonus _bonus;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			_bonus = DoStatBonus(new StatBonus());
			hero.ClientHeroEvent_OnLevelChanged += new Action<EventInfoHeroLevelUp>(ClientHeroEventOnLevelChanged);
			UpdateBonus();
		}
	}

	private void UpdateBonus()
	{
		_bonus.maxHealthFlat = (float)(victim.level - 1) * GetValue(healthPerLevel);
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
		UpdateBonus();
	}

	private void MirrorProcessed()
	{
	}
}
