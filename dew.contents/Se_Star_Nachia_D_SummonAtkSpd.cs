using System;
using Mirror;
using UnityEngine;

public class Se_Star_Nachia_D_SummonAtkSpd : StarEffect
{
	public StarScalingValue hasteAmount;

	public override Type heroType => typeof(Hero_Nachia);

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
		CreateBasicEffect(obj.summon, new HasteEffect
		{
			strength = GetValue(hasteAmount)
		}, float.PositiveInfinity);
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
