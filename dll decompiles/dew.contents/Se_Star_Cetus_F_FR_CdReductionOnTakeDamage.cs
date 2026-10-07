using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Se_Star_Cetus_F_FR_CdReductionOnTakeDamage : StarEffect
{
	public float reduceRatio = 0.15f;

	public float ultRatio = 0.33f;

	public float checkInterval = 0.1f;

	private float _lastCheckTime;

	public override Type heroType => typeof(Hero_Cetus);

	public override Type skillType => typeof(St_R_FrozenFists);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			hero.EntityEvent_OnTakeDamage += new Action<EventInfoDamage>(EntityEventOnTakeDamage);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && !((UnityEngine.Object)(object)hero == null))
		{
			hero.EntityEvent_OnTakeDamage -= new Action<EventInfoDamage>(EntityEventOnTakeDamage);
		}
	}

	private void EntityEventOnTakeDamage(EventInfoDamage obj)
	{
		if (_lastCheckTime + checkInterval > Time.time)
		{
			return;
		}
		_lastCheckTime = Time.time;
		foreach (KeyValuePair<int, AbilityTrigger> ability in hero.Ability.abilities)
		{
			if (ability.Value is St_R_FrozenFists st_R_FrozenFists)
			{
				float num = reduceRatio;
				if (st_R_FrozenFists.type == SkillType.Ultimate)
				{
					num *= ultRatio;
				}
				ApplyCooldownReductionByRatio(st_R_FrozenFists, num);
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
