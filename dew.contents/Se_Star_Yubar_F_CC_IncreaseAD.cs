using System;
using Mirror;
using UnityEngine;

public class Se_Star_Yubar_F_CC_IncreaseAD : StarEffect
{
	public override Type heroType => typeof(Hero_Yubar);

	public override Type skillType => typeof(St_R_Cataclysm);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			victim.ActorEvent_OnAbilityInstanceBeforePrepare += new Action<EventInfoAbilityInstance>(ActorEventOnAbilityInstanceBeforePrepare);
			victim.ActorEvent_OnAbilityInstanceCreated += new Action<EventInfoAbilityInstance>(ActorEventOnAbilityInstanceCreated);
		}
	}

	private void ActorEventOnAbilityInstanceCreated(EventInfoAbilityInstance obj)
	{
		if (obj.instance is Se_R_Cataclysm se_R_Cataclysm)
		{
			se_R_Cataclysm.DoHaste(se_R_Cataclysm.GetValue(se_R_Cataclysm.bonusApPercent));
		}
	}

	private void ActorEventOnAbilityInstanceBeforePrepare(EventInfoAbilityInstance obj)
	{
		if (obj.instance is Se_R_Cataclysm se_R_Cataclysm)
		{
			se_R_Cataclysm.increaseAdInstead = true;
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)victim != null)
		{
			victim.ActorEvent_OnAbilityInstanceBeforePrepare -= new Action<EventInfoAbilityInstance>(ActorEventOnAbilityInstanceBeforePrepare);
			victim.ActorEvent_OnAbilityInstanceCreated -= new Action<EventInfoAbilityInstance>(ActorEventOnAbilityInstanceCreated);
		}
	}

	private void MirrorProcessed()
	{
	}
}
