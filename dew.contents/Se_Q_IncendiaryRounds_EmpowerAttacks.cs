using System;
using Mirror;
using UnityEngine;

public class Se_Q_IncendiaryRounds_EmpowerAttacks : StatusEffect
{
	public float duration;

	public int numOfAttacks = 4;

	public ScalingValue hasteAmount;

	public bool speedOnShoot;

	public bool isSpeedDecay;

	public float speedDuration;

	public ScalingValue speedAmount;

	private int _doneAttacks;

	private Action<EventInfoAttackFired> _onAttackFiredCached;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			if ((UnityEngine.Object)(object)victim.Ability.attackAbility != null)
			{
				ResetCooldown(victim.Ability.attackAbility);
			}
			DoHaste(GetValue(hasteAmount));
			victim.EntityEvent_OnAttackFired += new Action<EventInfoAttackFired>(EntityEventOnAttackFired);
			SetTimer(duration);
			ShowOnScreenTimer();
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)victim != null)
		{
			victim.EntityEvent_OnAttackFired -= _onAttackFiredCached;
		}
	}

	private void EntityEventOnAttackFired(EventInfoAttackFired obj)
	{
		_doneAttacks++;
		ResetTimer();
		CreateAbilityInstance(((Component)(object)this).transform.position, Quaternion.identity, new CastInfo(victim), (Ai_Q_IncendiaryRounds_Attack r) =>
		{
			r.NetworkattackInstance = obj.instance;
		});
		if (speedOnShoot)
		{
			CreateBasicEffect(victim, new SpeedEffect
			{
				decay = isSpeedDecay,
				strength = GetValue(speedAmount)
			}, speedDuration, "incendiaryrounds_speed", DuplicateEffectBehavior.UsePrevious);
		}
		if (_doneAttacks >= numOfAttacks)
		{
			Destroy();
		}
	}

	private void MirrorProcessed()
	{
	}
}
