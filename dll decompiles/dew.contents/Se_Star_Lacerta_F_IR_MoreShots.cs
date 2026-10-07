using System;
using Mirror;
using UnityEngine;

public class Se_Star_Lacerta_F_IR_MoreShots : StarEffect
{
	public float cooldownTimePenalty = 2f;

	public int addedShots = 2;

	public override Type heroType => typeof(Hero_Lacerta);

	public override Type skillType => typeof(St_Q_IncendiaryRounds);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer && !((UnityEngine.Object)(object)skill == null))
		{
			DoSkillBonusAll(new SkillBonus
			{
				cooldownOffset = cooldownTimePenalty
			});
			victim.ActorEvent_OnAbilityInstanceBeforePrepare += new Action<EventInfoAbilityInstance>(ActorEventOnAbilityInstanceBeforePrepare);
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

	private void ActorEventOnAbilityInstanceBeforePrepare(EventInfoAbilityInstance obj)
	{
		if (obj.instance is Se_Q_IncendiaryRounds_EmpowerAttacks se_Q_IncendiaryRounds_EmpowerAttacks)
		{
			se_Q_IncendiaryRounds_EmpowerAttacks.numOfAttacks += addedShots;
		}
	}

	private void MirrorProcessed()
	{
	}
}
