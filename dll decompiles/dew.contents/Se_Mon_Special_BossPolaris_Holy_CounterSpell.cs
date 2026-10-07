using System;
using Mirror;
using UnityEngine;

public class Se_Mon_Special_BossPolaris_Holy_CounterSpell : StatusEffect
{
	public float counterDuration = 3f;

	public GameObject fxCounter;

	private bool _didCounter;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			_didCounter = false;
			DoArmorBoost(500f);
			DoUnstoppable();
			SetTimer(counterDuration);
			victim.EntityEvent_OnTakeDamage += new Action<EventInfoDamage>(EntityEventOnTakeDamage);
			victim.Control.IncrementBlockCounters(Channel.BlockedAction.Everything);
		}
	}

	protected override void OnDestroyActor()
	{
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (bool)(UnityEngine.Object)(object)victim)
		{
			if (!_didCounter && victim is Mon_Special_BossPolaris mon_Special_BossPolaris)
			{
				CreateStatusEffect<Se_Mon_Special_BossPolaris_Holy_Dash>(victim, new CastInfo(victim, mon_Special_BossPolaris.Holy_GetGoodDashPosition()));
			}
			victim.Control.DecrementBlockCounters(Channel.BlockedAction.Everything);
			victim.EntityEvent_OnTakeDamage -= new Action<EventInfoDamage>(EntityEventOnTakeDamage);
		}
	}

	private void EntityEventOnTakeDamage(EventInfoDamage obj)
	{
		Actor actor = obj.actor;
		while ((bool)(UnityEngine.Object)(object)actor)
		{
			if (actor is Gem || actor is StarEffect)
			{
				return;
			}
			if (actor is SkillTrigger)
			{
				break;
			}
			actor = actor.parentActor;
		}
		SkillTrigger st = actor as SkillTrigger;
		if (st != null && !st.owner.IsNullInactiveDeadOrKnockedOut() && st.owner.Skill.CanReplaceSkill(st.skillType))
		{
			FxPlayNetworked(fxCounter, info.caster);
			CreateAbilityInstance(info.caster.agentPosition, null, new CastInfo(info.caster, st.owner), (Ai_Mon_Special_BossPolaris_Holy_CounterSpell_DisarmProjectile ai) =>
			{
				ai.targetSkill = st;
			});
			_didCounter = true;
			victim.Control.StartDaze(1f);
			Destroy();
		}
	}

	private void MirrorProcessed()
	{
	}
}
