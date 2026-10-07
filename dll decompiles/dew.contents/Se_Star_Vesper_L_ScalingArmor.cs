using System;
using Mirror;
using UnityEngine;

public class Se_Star_Vesper_L_ScalingArmor : StarEffect
{
	public StarScalingValue bonusPer5Level;

	private StatBonus _bonus;

	public override Type heroType => typeof(Hero_Vesper);

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
		int num = 0;
		if (victim.level >= 5)
		{
			num++;
		}
		if (victim.level >= 10)
		{
			num++;
		}
		if (victim.level >= 15)
		{
			num++;
		}
		if (victim.level >= 20)
		{
			num++;
		}
		num *= GetValueInt(bonusPer5Level);
		_bonus.armorFlat = num;
	}

	private void MirrorProcessed()
	{
	}
}
