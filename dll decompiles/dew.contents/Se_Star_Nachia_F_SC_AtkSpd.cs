using System;
using Mirror;
using UnityEngine;

public class Se_Star_Nachia_F_SC_AtkSpd : StarEffect
{
	public float hasteAmount = 50f;

	public float hasteDuration = 5f;

	public float lifetimePenalty = 3f;

	public override Type heroType => typeof(Hero_Nachia);

	public override Type skillType => typeof(St_Q_SylvanCall);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			hero.ActorEvent_OnSpawnSummon += new Action<EventInfoSummon>(ActorEventOnSpawnSummon);
		}
	}

	private void ActorEventOnSpawnSummon(EventInfoSummon obj)
	{
		if (obj.summon is Sum_Q_SylvanCall_LeafHound sum_Q_SylvanCall_LeafHound)
		{
			sum_Q_SylvanCall_LeafHound.ReduceDuration(lifetimePenalty);
			CreateBasicEffect(sum_Q_SylvanCall_LeafHound, new HasteEffect
			{
				strength = hasteAmount
			}, hasteDuration + sum_Q_SylvanCall_LeafHound.Visual.spawnDuration + sum_Q_SylvanCall_LeafHound.Visual.dazeAfterSpawnDuration);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)hero != null)
		{
			hero.ActorEvent_OnSpawnSummon -= new Action<EventInfoSummon>(ActorEventOnSpawnSummon);
		}
	}

	private void MirrorProcessed()
	{
	}
}
