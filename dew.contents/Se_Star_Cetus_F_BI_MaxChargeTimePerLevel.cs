using System;
using Mirror;
using UnityEngine;

public class Se_Star_Cetus_F_BI_MaxChargeTimePerLevel : StarEffect
{
	public float maxChargeTimePerLevel = 0.5f;

	public override Type heroType => typeof(Hero_Cetus);

	public override Type skillType => typeof(St_Q_BigBorealChunk);

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
		if (obj.instance is Ai_Q_BigBorealChunk_Spawner ai_Q_BigBorealChunk_Spawner)
		{
			ai_Q_BigBorealChunk_Spawner.maxChannelTime += (float)(((SkillTrigger)ai_Q_BigBorealChunk_Spawner.firstTrigger).level - 1) * maxChargeTimePerLevel;
		}
	}

	private void MirrorProcessed()
	{
	}
}
