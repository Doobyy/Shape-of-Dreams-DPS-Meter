using System;
using Mirror;
using UnityEngine;

public class Se_D_IcyVeins : StackedStatusEffect
{
	public float shieldDuration;

	public ScalingValue shieldPerStack;

	public GameObject fxAddStack;

	private Action<EventInfoAttackEffect> _onAttackEffectTriggeredCached;

	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			SetStack(1);
			DoAttackCritical(null);
			victim.Visual.genericStackIndicatorValue = stack;
			victim.Visual.genericStackIndicatorMax = maxStack;
			victim.EntityEvent_OnAttackEffectTriggered += new Action<EventInfoAttackEffect>(EntityEventOnAttackEffectTriggered);
		}
	}

	private void EntityEventOnAttackEffectTriggered(EventInfoAttackEffect obj)
	{
		if (!obj.chain.DidReact(this) && stack > 0 && (obj.type != AttackEffectType.BasicAttackMain || !(obj.actor.creationTime < creationTime)) && !(obj.actor is Ai_Atk_CetusStaff))
		{
			CreateAbilityInstance(obj.victim.agentPosition, null, new CastInfo(victim), (Ai_D_IcyVeins_Damage ai) =>
			{
				ai.chain = obj.chain.New(this);
				ai.dmgFactor *= (float)stack;
				ai.strengthMultiplier *= obj.strength;
				ai.isCrit = stack >= maxStack / 2;
			});
			GiveShield(victim, GetValue(shieldPerStack) * (float)stack, shieldDuration, isDecay: true, obj.chain.New(this));
			Destroy();
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (bool)(UnityEngine.Object)(object)victim)
		{
			victim.Visual.genericStackIndicatorValue = 0;
			victim.Visual.genericStackIndicatorMax = 0;
			victim.EntityEvent_OnAttackEffectTriggered -= _onAttackEffectTriggeredCached;
		}
	}

	protected override void OnStackChange(int oldStack, int newStack)
	{
		base.OnStackChange(oldStack, newStack);
		if (((NetworkBehaviour)this).isServer)
		{
			victim.Visual.genericStackIndicatorValue = newStack;
		}
	}

	private void MirrorProcessed()
	{
	}
}
