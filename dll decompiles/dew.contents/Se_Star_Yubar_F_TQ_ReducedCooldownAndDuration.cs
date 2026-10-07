using System;
using Mirror;
using UnityEngine;

public class Se_Star_Yubar_F_TQ_ReducedCooldownAndDuration : StarEffect
{
	public float durationReduction;

	public float cooldownReduction;

	public override Type heroType => typeof(Hero_Yubar);

	public override Type skillType => typeof(St_R_Tranquility);

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
		if (obj.instance is Se_R_Tranquility se_R_Tranquility)
		{
			se_R_Tranquility.duration *= 1f - durationReduction;
		}
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
