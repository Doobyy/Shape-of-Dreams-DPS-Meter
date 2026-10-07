using System;
using Mirror;
using UnityEngine;

public class Se_Star_Yubar_F_TQ_LowerCooldownNoInvul : StarEffect
{
	public float shieldRatio;

	public float cooldownReduction;

	public override Type heroType => typeof(Hero_Yubar);

	public override Type skillType => typeof(St_R_Tranquility);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer && !((UnityEngine.Object)(object)skill == null))
		{
			victim.ActorEvent_OnAbilityInstanceBeforePrepare += new Action<EventInfoAbilityInstance>(ActorEventOnAbilityInstanceBeforePrepare);
			victim.ActorEvent_OnAbilityInstanceCreated += new Action<EventInfoAbilityInstance>(ActorEventOnAbilityInstanceCreated);
			DoSkillBonusAll(new SkillBonus
			{
				cooldownOffset = 0f - cooldownReduction
			});
		}
	}

	private void ActorEventOnAbilityInstanceBeforePrepare(EventInfoAbilityInstance obj)
	{
		if (obj.instance is Se_R_Tranquility se_R_Tranquility)
		{
			se_R_Tranquility.disableInvul = true;
		}
	}

	private void ActorEventOnAbilityInstanceCreated(EventInfoAbilityInstance obj)
	{
		if (obj.instance is Se_R_Tranquility se_R_Tranquility)
		{
			se_R_Tranquility.GiveShield(victim, victim.maxHealth * shieldRatio, se_R_Tranquility.duration);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			if ((UnityEngine.Object)(object)victim != null)
			{
				victim.ActorEvent_OnAbilityInstanceBeforePrepare -= new Action<EventInfoAbilityInstance>(ActorEventOnAbilityInstanceBeforePrepare);
			}
			if ((UnityEngine.Object)(object)victim != null)
			{
				victim.ActorEvent_OnAbilityInstanceCreated -= new Action<EventInfoAbilityInstance>(ActorEventOnAbilityInstanceCreated);
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
