using System;
using Mirror;
using UnityEngine;

public class Se_Star_L_PostHitDamageImmune : StarEffect
{
	public StarScalingValue cooldownTime;

	private bool _isReady;

	private float _lastActivateTime;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			victim.EntityEvent_OnTakeDamage += new Action<EventInfoDamage>(OnTakeDamage);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && !victim.IsNullOrInactive())
		{
			victim.EntityEvent_OnTakeDamage -= new Action<EventInfoDamage>(OnTakeDamage);
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (((NetworkBehaviour)this).isServer && !_isReady && !(Time.time - _lastActivateTime <= GetValue(cooldownTime)))
		{
			_isReady = true;
			showIcon = true;
		}
	}

	private void OnTakeDamage(EventInfoDamage obj)
	{
		if (_isReady)
		{
			showIcon = false;
			_isReady = false;
			_lastActivateTime = Time.time;
			CreateStatusEffect<Se_Star_L_PostHitDamageImmune_Protected>(victim, new CastInfo(victim)).ClientActorEvent_OnDestroyed += (Action<Actor>)((Actor _) =>
			{
				showIcon = false;
			});
		}
	}

	private void MirrorProcessed()
	{
	}
}
