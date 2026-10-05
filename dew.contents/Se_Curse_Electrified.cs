using System;
using Mirror;
using UnityEngine;

public class Se_Curse_Electrified : CurseStatusEffect
{
	public GameObject stunEffect;

	public float[] stunDurations;

	public float cooldownTime;

	private float _lastStunTime;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			victim.EntityEvent_OnTakeDamage += new Action<EventInfoDamage>(CheckStun);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)victim != null)
		{
			victim.EntityEvent_OnTakeDamage -= new Action<EventInfoDamage>(CheckStun);
		}
	}

	private void CheckStun(EventInfoDamage obj)
	{
		if (!(Time.time - _lastStunTime < cooldownTime))
		{
			_lastStunTime = Time.time;
			CreateBasicEffect(victim, new StunEffect(), GetValue(stunDurations), "hatred_electrify");
			FxPlayNetworked(stunEffect, victim);
		}
	}

	private void MirrorProcessed()
	{
	}
}
