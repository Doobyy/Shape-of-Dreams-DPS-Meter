using System;
using Mirror;
using UnityEngine;

public class Se_Star_Cetus_F_EM_ChaseTarget : StarEffect
{
	public float chasingSpeed = 5f;

	public float chasingTargetFindRadius = 15f;

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
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)victim != null)
		{
			victim.ActorEvent_OnAbilityInstanceBeforePrepare -= new Action<EventInfoAbilityInstance>(ActorEventOnAbilityInstanceBeforePrepare);
		}
	}

	private void ActorEventOnAbilityInstanceBeforePrepare(EventInfoAbilityInstance obj)
	{
		if (obj.instance is Ai_Q_EmbracingTheChill_AreaDoT ai_Q_EmbracingTheChill_AreaDoT)
		{
			ai_Q_EmbracingTheChill_AreaDoT.isChasingTarget = true;
			ai_Q_EmbracingTheChill_AreaDoT.chasingTargetFindRadius = chasingTargetFindRadius;
			ai_Q_EmbracingTheChill_AreaDoT.chasingSpeed = chasingSpeed;
		}
	}

	private void MirrorProcessed()
	{
	}
}
