using System;
using Mirror;
using UnityEngine;

public class Se_Star_Nachia_F_MP_NachiaHeal : StarEffect
{
	public float nachiaHealMult = 0.5f;

	public override Type heroType => typeof(Hero_Nachia);

	public override Type skillType => typeof(St_Q_MoonlightPact);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			hero.ActorEvent_OnAbilityInstanceBeforePrepare += new Action<EventInfoAbilityInstance>(ActorEventOnAbilityInstanceBeforePrepare);
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

	private void ActorEventOnAbilityInstanceBeforePrepare(EventInfoAbilityInstance obj)
	{
		if (obj.instance is Ai_Q_MoonlightPact_Land ai_Q_MoonlightPact_Land)
		{
			ai_Q_MoonlightPact_Land.nachiaHealMultiplier = nachiaHealMult;
		}
	}

	private void MirrorProcessed()
	{
	}
}
