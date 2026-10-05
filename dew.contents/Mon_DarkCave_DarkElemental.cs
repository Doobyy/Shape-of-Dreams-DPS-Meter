using System;
using Mirror;
using UnityEngine;

public class Mon_DarkCave_DarkElemental : Monster, ISpawnableAsMiniBoss
{
	public float teleportDamageTime;

	public float teleportHpThreshold;

	public float barrageChancePerSecond;

	private float _lastDamagedTime;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			EntityEvent_OnTakeDamage += new Action<EventInfoDamage>(EntityEventOnTakeDamage);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			EntityEvent_OnTakeDamage -= new Action<EventInfoDamage>(EntityEventOnTakeDamage);
		}
	}

	private void EntityEventOnTakeDamage(EventInfoDamage obj)
	{
		if (isActive && !(obj.actor is ElementalStatusEffect))
		{
			_lastDamagedTime = Time.time;
		}
	}

	protected override void AIUpdate(ref EntityAIContext context)
	{
		base.AIUpdate(ref context);
		if (!((UnityEngine.Object)(object)context.targetEnemy == null))
		{
			if (Time.time - _lastDamagedTime < teleportDamageTime && currentHealth / maxHealth < teleportHpThreshold && AI.Helper_CanBeCast<At_Mon_DarkCave_DarkElemental_Teleport>() && AI.Helper_IsTargetInRange<At_Mon_DarkCave_DarkElemental_Teleport>())
			{
				AI.Helper_CastAbilityAuto<At_Mon_DarkCave_DarkElemental_Teleport>();
			}
			else if (UnityEngine.Random.value < barrageChancePerSecond * context.deltaTime && AI.Helper_CanBeCast<At_Mon_DarkCave_DarkElemental_Barrage>() && AI.Helper_IsTargetInRange<At_Mon_DarkCave_DarkElemental_Barrage>())
			{
				AI.Helper_CastAbilityAuto<At_Mon_DarkCave_DarkElemental_Barrage>();
			}
			else
			{
				AI.Helper_ChaseTarget();
			}
		}
	}

	public void OnBeforeSpawnAsMiniBoss()
	{
	}

	public void OnCreateAsMiniBoss()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		ISpawnableAsMiniBoss.GiveGenericMiniBossBonus(this);
		At_Mon_DarkCave_DarkElemental_Atk ability = Ability.GetAbility<At_Mon_DarkCave_DarkElemental_Atk>();
		ability.spawnAdditionalProjectile = true;
		ability.configs[0].castMethod._range += 3f;
		ActorEvent_OnAbilityInstanceBeforePrepare += (Action<EventInfoAbilityInstance>)((EventInfoAbilityInstance info) =>
		{
			if (info.instance is Ai_Mon_DarkCave_DarkElemental_Barrage_Spawner ai_Mon_DarkCave_DarkElemental_Barrage_Spawner)
			{
				ai_Mon_DarkCave_DarkElemental_Barrage_Spawner.shootCount *= 2;
				ai_Mon_DarkCave_DarkElemental_Barrage_Spawner.maxDeviation *= 1.5f;
				ai_Mon_DarkCave_DarkElemental_Barrage_Spawner.interval *= 0.75f;
			}
		});
	}

	private void MirrorProcessed()
	{
	}
}
