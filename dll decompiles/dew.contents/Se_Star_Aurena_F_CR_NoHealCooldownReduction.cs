using System;
using Mirror;
using UnityEngine;

public class Se_Star_Aurena_F_CR_NoHealCooldownReduction : StarEffect
{
	public float cooldownReductionRatio = 0.1f;

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
		if (!(obj.instance is Ai_R_ChainReaction ai_R_ChainReaction))
		{
			return;
		}
		ai_R_ChainReaction.disableHeal = true;
		KillTracker tracker = ai_R_ChainReaction.TrackKills(0.5f, (EventInfoKill k) =>
		{
			if (k.victim is Monster && (UnityEngine.Object)(object)skill != null)
			{
				ApplyCooldownReductionByRatio(skill, cooldownReductionRatio);
			}
		});
		ai_R_ChainReaction.ClientActorEvent_OnDestroyed += (Action<Actor>)((Actor _) =>
		{
			tracker.Stop();
		});
	}

	private void MirrorProcessed()
	{
	}
}
