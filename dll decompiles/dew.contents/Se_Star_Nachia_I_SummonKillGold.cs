using System;
using Mirror;
using UnityEngine;

public class Se_Star_Nachia_I_SummonKillGold : StarEffect
{
	public float goldChance = 0.25f;

	public StarScalingValue goldAmount;

	public override Type heroType => typeof(Hero_Nachia);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			hero.ClientHeroEvent_OnKillOrAssist += new Action<EventInfoKill>(ClientHeroEventOnKillOrAssist);
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

	private void ClientHeroEventOnKillOrAssist(EventInfoKill obj)
	{
		if (obj.victim is Monster && !(UnityEngine.Random.value > goldChance) && obj.actor.firstEntity is Summon)
		{
			NetworkedManagerBase<PickupManager>.instance.DropGold(isKillGold: false, isGivenByOtherPlayer: false, GetValueInt(goldAmount), obj.victim.agentPosition, hero);
		}
	}

	private void MirrorProcessed()
	{
	}
}
