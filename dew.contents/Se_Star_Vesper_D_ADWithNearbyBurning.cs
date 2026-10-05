using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Se_Star_Vesper_D_ADWithNearbyBurning : StarEffect
{
	public StarScalingValue adPercentage;

	private float _lastCheckTime;

	private StatBonus _bonus;

	public override Type heroType => typeof(Hero_Vesper);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			_bonus = DoStatBonus();
		}
	}

	private bool HasBurningNearby()
	{
		if (victim.IsNullInactiveDeadOrKnockedOut())
		{
			return false;
		}
		if (!LavaLand_Lava.instance.IsNullOrInactive())
		{
			return true;
		}
		if (!Forest_Fireplace.instance.IsNullOrInactive())
		{
			return true;
		}
		List<Entity> list = DewPhysics.OverlapCircleAllEntities(out var handle, hero.agentPosition, 10f);
		bool result = false;
		foreach (Entity item in list)
		{
			if (item.Status.fireStack > 0)
			{
				result = true;
				break;
			}
		}
		handle.Return();
		return result;
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		bool flag = _bonus.attackDamagePercentage > 0f;
		float num = (flag ? 1.4f : 0.7f);
		if (Time.time - _lastCheckTime > num)
		{
			bool flag2 = HasBurningNearby();
			if (flag && !flag2)
			{
				_bonus.attackDamagePercentage = 0f;
			}
			else if (!flag & flag2)
			{
				_bonus.attackDamagePercentage = GetValue(adPercentage);
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
