using System;
using Mirror;
using UnityEngine;

public class Se_Curse_FateOfGreed : CurseStatusEffect
{
	public GameObject fxExplode;

	public float[] lostRatio;

	public float cooldownTime;

	private float _lastLostTime;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			victim.EntityEvent_OnTakeDamage += new Action<EventInfoDamage>(CheckLose);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)victim != null)
		{
			victim.EntityEvent_OnTakeDamage -= new Action<EventInfoDamage>(CheckLose);
		}
	}

	private void CheckLose(EventInfoDamage obj)
	{
		if ((UnityEngine.Object)(object)victim.owner == null || (UnityEngine.Object)(object)obj.actor == null)
		{
			return;
		}
		Entity entity = obj.actor.firstEntity;
		if (!((UnityEngine.Object)(object)entity == null) && entity.CheckEnemyOrNeutral(victim) && !(Time.time - _lastLostTime < cooldownTime))
		{
			int num = Mathf.RoundToInt((float)victim.owner.gold * GetValue(lostRatio));
			if (num <= 0 && victim.owner.gold > 0)
			{
				num = 1;
			}
			if (num > 0)
			{
				_lastLostTime = Time.time;
				FxPlayNewNetworked(fxExplode, victim);
				victim.owner.gold -= num;
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
