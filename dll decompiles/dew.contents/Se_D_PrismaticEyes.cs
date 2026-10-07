using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Se_D_PrismaticEyes : StatusEffect
{
	public ScalingValue hasteAmount;

	private AbilityLockHandle _lock;

	private Hero_Bismuth _hero;

	private AttackTrigger _pendingAttack;

	private CastInfo _pendingCastInfo;

	private Action<Ai_D_PrismaticEyes_Attack> _beforePrepare;

	protected override void OnCreate()
	{
		base.OnCreate();
		_hero = FindFirstAncestorOfType<Hero_Bismuth>();
		if (((NetworkBehaviour)this).isServer)
		{
			_beforePrepare = OnBeforePrepareAttack;
			DoHaste(GetValue(hasteAmount));
			_lock = victim.Ability.GetNewAbilityLockHandle();
			_lock.LockAttackAbilityCast();
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (!((NetworkBehaviour)this).isServer || victim.IsNullInactiveDeadOrKnockedOut() || NetworkedManagerBase<ZoneManager>.instance.isInAnyTransition || ManagerBase<CameraManager>.instance.isPlayingCutscene || ManagerBase<TransitionManager>.instance.state == TransitionManager.StateType.Loading)
		{
			return;
		}
		AttackTrigger attackTrigger = (AttackTrigger)victim.Ability.attackAbility;
		if (attackTrigger.currentConfigCurrentCharge <= 0)
		{
			return;
		}
		float num = attackTrigger.ProcessRange(attackTrigger.currentConfig.castMethod._range) + 1f;
		List<Entity> targetEntities = Hero_Bismuth.GetTargetEntities(out var handle, victim, canBeNeutral: false, num + 1f);
		if (targetEntities.Count > 0)
		{
			attackTrigger.UpdateConfigIndexForCrit();
			attackTrigger.SetChargeAll(0);
			CastInfo pendingCastInfo = new CastInfo(victim, targetEntities[0]);
			_pendingAttack = attackTrigger;
			_pendingCastInfo = pendingCastInfo;
			Ai_D_PrismaticEyes_Attack newInstance = CreateAbilityInstance(victim.position, null, pendingCastInfo, _beforePrepare);
			attackTrigger.CallAttackCompleteRoutines(newInstance, attackTrigger.currentConfigIndex, pendingCastInfo);
			if ((UnityEngine.Object)(object)_hero != null)
			{
				_hero.book.RpcBookCast(targetEntities[0].agentPosition);
			}
		}
		handle.Return();
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && _lock != null)
		{
			_lock.Stop();
			_lock = null;
		}
	}

	private void OnBeforePrepareAttack(Ai_D_PrismaticEyes_Attack ai)
	{
		_pendingAttack.CallAttackCompleteBeforePrepareRoutines(new EventInfoCast
		{
			configIndex = _pendingAttack.currentConfigIndex,
			info = _pendingCastInfo,
			instance = ai,
			trigger = _pendingAttack
		});
		ai.isCrit = _pendingAttack.currentConfigIndex > 0;
	}

	private void MirrorProcessed()
	{
	}
}
