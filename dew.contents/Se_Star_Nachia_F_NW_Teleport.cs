using System;
using Mirror;
using UnityEngine;

public class Se_Star_Nachia_F_NW_Teleport : StarEffect
{
	public override Type heroType => typeof(Hero_Nachia);

	public override Type skillType => typeof(St_R_NaturesWhisper);

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
		if (obj.instance is Ai_R_NaturesWhisper ai_R_NaturesWhisper)
		{
			ai_R_NaturesWhisper.doTeleport = true;
		}
	}

	private void MirrorProcessed()
	{
	}
}
