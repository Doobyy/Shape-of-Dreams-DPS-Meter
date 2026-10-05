using System;
using Mirror;
using UnityEngine;

public class Se_Star_Mist_F_LG_SingleTarget : StarEffect
{
	public float cooldownReduction = 1f;

	public float damageAmp = 1f;

	public Knockback knockback;

	public override Type heroType => typeof(Hero_Mist);

	public override Type skillType => typeof(St_Q_Lunge);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer && !((UnityEngine.Object)(object)skill == null))
		{
			victim.ActorEvent_OnAbilityInstanceBeforePrepare += new Action<EventInfoAbilityInstance>(ActorEventOnAbilityInstanceBeforePrepare);
			DoSkillBonusAll(new SkillBonus
			{
				cooldownOffset = 0f - cooldownReduction
			});
		}
	}

	private void ActorEventOnAbilityInstanceBeforePrepare(EventInfoAbilityInstance obj)
	{
		if (!(obj.instance is Ai_Q_Lunge ai_Q_Lunge))
		{
			return;
		}
		ai_Q_Lunge.singleTargetOnly = true;
		ai_Q_Lunge.dealtDamageProcessor.Add(delegate(ref DamageData data, Actor actor, Entity target)
		{
			if (actor is Ai_Q_Lunge && !data.IsAmountModifiedBy(this))
			{
				data.SetAmountModifiedBy(this);
				data.ApplyAmplification(damageAmp);
				knockback.ApplyWithOrigin(victim.position, target);
			}
		});
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)victim != null)
		{
			victim.ActorEvent_OnAbilityInstanceBeforePrepare -= new Action<EventInfoAbilityInstance>(ActorEventOnAbilityInstanceBeforePrepare);
		}
	}

	private void MirrorProcessed()
	{
	}
}
