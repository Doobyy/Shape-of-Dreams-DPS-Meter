using System;
using Mirror;
using UnityEngine;

public class Se_Star_Aurena_F_CR_GlobalHeal : StarEffect
{
	public float globalHealMultiplier = 0.7f;

	public override Type heroType => typeof(Hero_Aurena);

	public override Type skillType => typeof(St_R_ChainReaction);

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
		if (obj.instance is Ai_R_ChainReaction ai_R_ChainReaction)
		{
			ai_R_ChainReaction.globalTravelerHealMultiplier = globalHealMultiplier;
		}
	}

	private void MirrorProcessed()
	{
	}
}
