using System;
using Mirror;
using UnityEngine;

public class Se_Star_Lacerta_I_BonusFromStones : StarEffect
{
	public StarScalingValue bonusGoldAndDreamDust;

	public override Type heroType => typeof(Hero_Lacerta);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			NetworkedManagerBase<ClientEventManager>.instance.OnDeath += new Action<EventInfoKill>(OnDeath);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)NetworkedManagerBase<ClientEventManager>.instance != null)
		{
			NetworkedManagerBase<ClientEventManager>.instance.OnDeath -= new Action<EventInfoKill>(OnDeath);
		}
	}

	private void OnDeath(EventInfoKill obj)
	{
		if (!victim.IsNullInactiveDeadOrKnockedOut() && (obj.victim is PropEnt_Stone_Gold || obj.victim is PropEnt_Stone_DreamDust))
		{
			NetworkedManagerBase<PickupManager>.instance.DropGold(isKillGold: false, isGivenByOtherPlayer: false, GetValueInt(bonusGoldAndDreamDust), obj.victim.position, (Hero)victim);
			NetworkedManagerBase<PickupManager>.instance.DropDreamDust(isGivenByOtherPlayer: false, GetValueInt(bonusGoldAndDreamDust), obj.victim.position, (Hero)victim);
		}
	}

	private void MirrorProcessed()
	{
	}
}
