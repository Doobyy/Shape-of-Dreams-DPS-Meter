using System;
using Mirror;
using UnityEngine;

public class Se_Star_Husk_I_BossKillPlatinumCoin : StarEffect
{
	public StarScalingValue gainChance;

	public override Type heroType => typeof(Hero_Husk);

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
		if (obj.victim.IsAnyBoss() && !(UnityEngine.Random.value > GetValue(gainChance)))
		{
			player.platinumCoin++;
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)hero != null)
		{
			hero.ClientHeroEvent_OnKillOrAssist -= new Action<EventInfoKill>(ClientHeroEventOnKillOrAssist);
		}
	}

	private void MirrorProcessed()
	{
	}
}
