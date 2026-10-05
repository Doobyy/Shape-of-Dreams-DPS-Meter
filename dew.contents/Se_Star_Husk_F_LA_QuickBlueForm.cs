using System;
using Mirror;
using UnityEngine;

public class Se_Star_Husk_F_LA_QuickBlueForm : StarEffect
{
	public float castDurationReduction = 0.4f;

	public float cooldownReduction = 0.3f;

	public override Type heroType => typeof(Hero_Husk);

	public override Type skillType => typeof(St_Q_Laceration);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			hero.ActorEvent_OnAbilityInstanceBeforePrepare += new Action<EventInfoAbilityInstance>(ActorEventOnAbilityInstanceBeforePrepare);
			if ((UnityEngine.Object)(object)skill != null)
			{
				DoSkillBonusAll(new SkillBonus
				{
					cooldownMultiplier = 1f - cooldownReduction
				});
			}
		}
	}

	private void ActorEventOnAbilityInstanceBeforePrepare(EventInfoAbilityInstance obj)
	{
		if (obj.instance is Ai_Q_Laceration_Circle ai_Q_Laceration_Circle)
		{
			ai_Q_Laceration_Circle.castDurationReduction = castDurationReduction;
			ai_Q_Laceration_Circle.selfSlowAmount = 0f;
			ai_Q_Laceration_Circle.disableInvul = true;
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)hero != null)
		{
			hero.ActorEvent_OnAbilityInstanceBeforePrepare -= new Action<EventInfoAbilityInstance>(ActorEventOnAbilityInstanceBeforePrepare);
		}
	}

	private void MirrorProcessed()
	{
	}
}
