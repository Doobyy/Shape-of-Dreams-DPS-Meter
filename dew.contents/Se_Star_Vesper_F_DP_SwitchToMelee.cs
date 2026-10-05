using System;
using Mirror;
using UnityEngine;

public class Se_Star_Vesper_F_DP_SwitchToMelee : StarEffect
{
	public float stunDuration = 1f;

	public int addedCharges = 1;

	public float rangeReduction = 0.5f;

	public override Type heroType => typeof(Hero_Vesper);

	public override Type skillType => typeof(St_Q_Discipline);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			if ((UnityEngine.Object)(object)skill != null)
			{
				DoSkillBonusAll(new SkillBonus
				{
					addedCharge = addedCharges
				});
				skill.tags |= DescriptionTags.HardCC;
				skill.configs[0].castMethod._range *= 1f - rangeReduction;
				skill.SyncCastMethodChanges(0);
			}
			victim.ActorEvent_OnAbilityInstanceBeforePrepare += new Action<EventInfoAbilityInstance>(ActorEventOnAbilityInstanceBeforePrepare);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			if ((UnityEngine.Object)(object)skill != null)
			{
				skill.tags &= ~DescriptionTags.HardCC;
				skill.configs[0].castMethod._range /= 1f - rangeReduction;
				skill.SyncCastMethodChanges(0);
			}
			if ((UnityEngine.Object)(object)victim != null)
			{
				victim.ActorEvent_OnAbilityInstanceBeforePrepare -= new Action<EventInfoAbilityInstance>(ActorEventOnAbilityInstanceBeforePrepare);
			}
		}
	}

	private void ActorEventOnAbilityInstanceBeforePrepare(EventInfoAbilityInstance obj)
	{
		if (obj.instance is Ai_Q_Discipline_Stomp ai_Q_Discipline_Stomp)
		{
			ai_Q_Discipline_Stomp.stunDuration = stunDuration;
			ai_Q_Discipline_Stomp.knockupAmount = 1.5f;
		}
	}

	private void MirrorProcessed()
	{
	}
}
