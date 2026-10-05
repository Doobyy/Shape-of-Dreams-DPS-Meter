using System;
using Mirror;
using UnityEngine;

public class Se_Star_Cetus_F_EM_InfiniteDuration : StarEffect
{
	public float damageIntervalAmp;

	public float scaleReduction = 0.25f;

	public override Type heroType => typeof(Hero_Cetus);

	public override Type skillType => typeof(St_Q_EmbracingTheChill);

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
		if (((NetworkBehaviour)this).isServer && !((UnityEngine.Object)(object)victim == null))
		{
			victim.ActorEvent_OnAbilityInstanceBeforePrepare -= new Action<EventInfoAbilityInstance>(ActorEventOnAbilityInstanceBeforePrepare);
		}
	}

	private void ActorEventOnAbilityInstanceBeforePrepare(EventInfoAbilityInstance obj)
	{
		if (obj.instance is Ai_Q_EmbracingTheChill_AreaDoT ai_Q_EmbracingTheChill_AreaDoT)
		{
			ai_Q_EmbracingTheChill_AreaDoT.hasInfiniteDuration = true;
			ai_Q_EmbracingTheChill_AreaDoT.cooldownTime *= 1f + damageIntervalAmp;
			ai_Q_EmbracingTheChill_AreaDoT.baseScale = 1f - scaleReduction;
			St_Q_EmbracingTheChill st_Q_EmbracingTheChill = (St_Q_EmbracingTheChill)ai_Q_EmbracingTheChill_AreaDoT.firstTrigger;
			if (!st_Q_EmbracingTheChill.instAi.IsNullOrInactive())
			{
				st_Q_EmbracingTheChill.instAi.Destroy();
			}
			st_Q_EmbracingTheChill.instAi = ai_Q_EmbracingTheChill_AreaDoT;
		}
	}

	private void MirrorProcessed()
	{
	}
}
