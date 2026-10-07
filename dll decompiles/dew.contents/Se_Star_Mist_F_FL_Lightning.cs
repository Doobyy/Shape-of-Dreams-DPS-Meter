using System;
using Mirror;
using UnityEngine;

public class Se_Star_Mist_F_FL_Lightning : StarEffect
{
	public override Type heroType => typeof(Hero_Mist);

	public override Type skillType => typeof(St_Q_Fleche);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
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
		if (obj.instance is Ai_Q_Fleche_Dash ai_Q_Fleche_Dash)
		{
			ai_Q_Fleche_Dash.NetworkempoweredWithLightning = true;
		}
	}

	private void MirrorProcessed()
	{
	}
}
