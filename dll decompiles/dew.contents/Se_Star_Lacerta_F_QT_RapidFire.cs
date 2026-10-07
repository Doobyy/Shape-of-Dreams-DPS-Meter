using System;
using Mirror;
using UnityEngine;

public class Se_Star_Lacerta_F_QT_RapidFire : StarEffect
{
	public override Type heroType => typeof(Hero_Lacerta);

	public override Type skillType => typeof(St_R_QuickTrigger);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer && !((UnityEngine.Object)(object)skill == null))
		{
			DoSkillBonusAll(new SkillBonus
			{
				addedCharge = -1,
				ignoreReceiveCooldownReductionFlag = true
			});
			victim.ActorEvent_OnAbilityInstanceBeforePrepare += new Action<EventInfoAbilityInstance>(ActorEventOnAbilityInstanceBeforePrepare);
		}
	}

	private void ActorEventOnAbilityInstanceBeforePrepare(EventInfoAbilityInstance obj)
	{
		if (obj.instance is Ai_R_QuickTrigger ai_R_QuickTrigger)
		{
			ai_R_QuickTrigger.fireCount = 6;
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && !victim.IsNullOrInactive())
		{
			victim.ActorEvent_OnAbilityInstanceBeforePrepare -= new Action<EventInfoAbilityInstance>(ActorEventOnAbilityInstanceBeforePrepare);
		}
	}

	private void MirrorProcessed()
	{
	}
}
