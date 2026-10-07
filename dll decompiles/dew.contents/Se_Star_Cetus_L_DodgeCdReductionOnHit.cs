using System;
using Mirror;
using UnityEngine;

public class Se_Star_Cetus_L_DodgeCdReductionOnHit : StarEffect
{
	public StarScalingValue reductionAmount;

	public float cooldownTime = 0.4f;

	private float _lastActivateTime;

	public override Type heroType => typeof(Hero_Cetus);

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
		if (((NetworkBehaviour)this).isServer && (bool)(UnityEngine.Object)(object)hero)
		{
			hero.EntityEvent_OnTakeDamage -= new Action<EventInfoDamage>(EntityEventOnTakeDamage);
		}
	}

	private void EntityEventOnTakeDamage(EventInfoDamage obj)
	{
		if (!(Time.time - _lastActivateTime < cooldownTime))
		{
			ApplyCooldownReductionByRatio(hero.Skill.Movement, GetValue(reductionAmount), ignoreCanReceiveCooldown: true);
			_lastActivateTime = Time.time;
		}
	}

	private void MirrorProcessed()
	{
	}
}
