using System;
using Mirror;
using UnityEngine;

public class Se_Star_Husk_D_ScalingAdAndAtkSpd : StarEffect
{
	public StarScalingValue adBonus;

	public StarScalingValue atkSpdBonus;

	private StatBonus _bonus;

	public override Type heroType => typeof(Hero_Husk);

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
		float num = 0f;
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
		_bonus.attackDamageFlat = num * GetValue(adBonus);
		_bonus.attackSpeedPercentage = num * GetValue(atkSpdBonus);
	}

	private void MirrorProcessed()
	{
	}
}
