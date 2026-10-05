using System;
using Mirror;
using UnityEngine;

public class Se_Star_Yubar_I_StarCookie : StarEffect
{
	public StarScalingValue starCookieChance;

	public StarScalingValue starCookieOnBossChance;

	public override Type heroType => typeof(Hero_Yubar);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			hero.ClientHeroEvent_OnKillOrAssist += new Action<EventInfoKill>(ClientHeroEventOnKillOrAssist);
		}
	}

	private void ClientHeroEventOnKillOrAssist(EventInfoKill obj)
	{
		StarScalingValue value = (obj.victim.IsAnyBoss() ? starCookieOnBossChance : starCookieChance);
		if (UnityEngine.Random.value < GetValue(value))
		{
			CreateActor<Shrine_StarCookie>(obj.victim.agentPosition, null);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			hero.ClientHeroEvent_OnKillOrAssist -= new Action<EventInfoKill>(ClientHeroEventOnKillOrAssist);
		}
	}

	private void MirrorProcessed()
	{
	}
}
