using System;
using Mirror;
using UnityEngine;

public class Se_Star_Husk_F_DE_MarkHitDodgeCDR : StarEffect
{
	public float increasedDuration = 2f;

	public float dodgeReductionAmount = 1f;

	public float darkDamageAmp = 0.3f;

	public override Type heroType => typeof(Hero_Husk);

	public override Type skillType => typeof(St_Q_DeathMark);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			victim.ActorEvent_OnAbilityInstanceBeforePrepare += new Action<EventInfoAbilityInstance>(ActorEventOnAbilityInstanceBeforePrepare);
			victim.ActorEvent_OnAttackEffectTriggered += new Action<EventInfoAttackEffect>(ActorEventOnAttackEffectTriggered);
			victim.dealtDamageProcessor.Add(Processor);
		}
	}

	private void Processor(ref DamageData data, Actor from, Entity to)
	{
		if (!data.IsAmountModifiedBy(this) && victim.CheckEnemyOrNeutral(to) && data.elemental == ElementalType.Dark && to.Status.HasStatusEffect<Se_Q_DeathMark_Marked>())
		{
			data.SetAmountModifiedBy(this);
			data.ApplyAmplification(darkDamageAmp);
			data.SetAttr(DamageAttribute.IsCrit);
		}
	}

	private void ActorEventOnAbilityInstanceBeforePrepare(EventInfoAbilityInstance obj)
	{
		if (obj.instance is Se_Q_DeathMark_Marked se_Q_DeathMark_Marked)
		{
			se_Q_DeathMark_Marked.lingerDuration += increasedDuration;
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)victim != null)
		{
			victim.ActorEvent_OnAbilityInstanceBeforePrepare -= new Action<EventInfoAbilityInstance>(ActorEventOnAbilityInstanceBeforePrepare);
			victim.ActorEvent_OnAttackEffectTriggered -= new Action<EventInfoAttackEffect>(ActorEventOnAttackEffectTriggered);
			victim.dealtDamageProcessor.Remove(Processor);
		}
	}

	private void ActorEventOnAttackEffectTriggered(EventInfoAttackEffect obj)
	{
		if (obj.victim.Status.HasStatusEffect<Se_Q_DeathMark_Marked>())
		{
			SkillTrigger movement = ((Hero)victim).Skill.Movement;
			if (!((UnityEngine.Object)(object)movement == null))
			{
				ApplyCooldownReduction(movement, dodgeReductionAmount, scaled: true, ignoreCanReceiveCooldown: true);
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
