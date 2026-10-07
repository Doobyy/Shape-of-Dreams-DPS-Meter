using System;
using Mirror;

public class Se_Star_Mist_F_LG_IncreasedCooldownReduction : StarEffect
{
	public float cooldownReductionAmp = 1f;

	public float damageReduction = 0.5f;

	public override Type heroType => typeof(Hero_Mist);

	public override Type skillType => typeof(St_Q_Lunge);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		DoSkill((SkillTrigger s) =>
		{
			((St_Q_Lunge)s).reducedCooldownByMeister *= 1f + cooldownReductionAmp;
			s.ActorEvent_OnAbilityInstanceBeforePrepare += new Action<EventInfoAbilityInstance>(ActorEventOnAbilityInstanceBeforePrepare);
			return () =>
			{
				((St_Q_Lunge)s).reducedCooldownByMeister /= 1f + cooldownReductionAmp;
				s.ActorEvent_OnAbilityInstanceBeforePrepare -= new Action<EventInfoAbilityInstance>(ActorEventOnAbilityInstanceBeforePrepare);
			};
		});
	}

	private void ActorEventOnAbilityInstanceBeforePrepare(EventInfoAbilityInstance obj)
	{
		if (!(obj.instance is Ai_Q_Lunge))
		{
			return;
		}
		obj.instance.dealtDamageProcessor.Add(delegate(ref DamageData data, Actor actor, Entity target)
		{
			if (actor is Ai_Q_Lunge && !data.IsAmountModifiedBy(this))
			{
				data.SetAmountModifiedBy(this);
				data.ApplyReduction(damageReduction);
			}
		});
	}

	private void MirrorProcessed()
	{
	}
}
