using System;
using Mirror;
using UnityEngine;

public class Se_Star_Lacerta_F_IR_RechargeAmmoAmpBurnDamage : StarEffect
{
	public StarScalingValue damageReduction;

	public int addedFireStacks = 1;

	public int addedNumOfAttacks = 1;

	private Entity _lastHitVictim;

	public override Type heroType => typeof(Hero_Lacerta);

	public override Type skillType => typeof(St_Q_IncendiaryRounds);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			victim.ActorEvent_OnAbilityInstanceCreated += new Action<EventInfoAbilityInstance>(OnAbilityInstanceCreated);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)victim != null)
		{
			victim.ActorEvent_OnAbilityInstanceCreated -= new Action<EventInfoAbilityInstance>(OnAbilityInstanceCreated);
		}
	}

	private void OnAbilityInstanceCreated(EventInfoAbilityInstance obj)
	{
		AbilityInstance instance = obj.instance;
		Ai_Q_IncendiaryRounds_Attack ai = instance as Ai_Q_IncendiaryRounds_Attack;
		if (ai != null)
		{
			float reduction = GetValue(damageReduction);
			ai.dealtDamageProcessor.Add(delegate(ref DamageData data, Actor actor, Entity target)
			{
				data.ApplyReduction(reduction);
				data.SetOverrideElementalStacks(1 + addedFireStacks);
			});
			ai.NetworkattackInstance.ActorEvent_OnAttackHit += new Action<EventInfoAttackHit>(OnHit);
		}
		void OnHit(EventInfoAttackHit hit)
		{
			ai.NetworkattackInstance.ActorEvent_OnAttackHit -= new Action<EventInfoAttackHit>(OnHit);
			if (!((UnityEngine.Object)(object)hit.victim == (UnityEngine.Object)(object)_lastHitVictim))
			{
				_lastHitVictim = hit.victim;
				if (victim.Status.TryGetStatusEffect<Se_Q_IncendiaryRounds_EmpowerAttacks>(out var effect))
				{
					effect.numOfAttacks += addedNumOfAttacks;
				}
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
