using System;
using Mirror;
using UnityEngine;

public class Se_Star_I_GoldRiskAndReturn : StarEffect
{
	public StarScalingValue bonusRatio;

	public float lostGoldRatio;

	public float cooldownTime;

	public GameObject fxExplode;

	private float _lastLostTime;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			player.monsterKillGoldMultiplier += GetValue(bonusRatio);
			victim.EntityEvent_OnTakeDamage += new Action<EventInfoDamage>(OnTakeDamage);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			if ((bool)(UnityEngine.Object)(object)player)
			{
				player.monsterKillGoldMultiplier -= GetValue(bonusRatio);
			}
			if ((bool)(UnityEngine.Object)(object)victim)
			{
				victim.EntityEvent_OnTakeDamage -= new Action<EventInfoDamage>(OnTakeDamage);
			}
		}
	}

	private void OnTakeDamage(EventInfoDamage obj)
	{
		if ((UnityEngine.Object)(object)player == null || (UnityEngine.Object)(object)obj.actor == null)
		{
			return;
		}
		Entity entity = obj.actor.firstEntity;
		if (!((UnityEngine.Object)(object)entity == null) && entity.CheckEnemyOrNeutral(victim) && !(Time.time - _lastLostTime < cooldownTime))
		{
			int num = Mathf.RoundToInt((float)player.gold * lostGoldRatio);
			if (num <= 0 && player.gold > 0)
			{
				num = 1;
			}
			if (num > 0)
			{
				_lastLostTime = Time.time;
				FxPlayNetworked(fxExplode, victim);
				player.gold -= num;
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
