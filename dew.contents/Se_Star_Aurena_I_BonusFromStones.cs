using System;
using Mirror;
using UnityEngine;

public class Se_Star_Aurena_I_BonusFromStones : StarEffect
{
	public StarScalingValue amountAmp;

	public override Type heroType => typeof(Hero_Aurena);

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
		if (!hero.IsNullInactiveDeadOrKnockedOut())
		{
			if (obj.victim is PropEnt_Stone_Gold propEnt_Stone_Gold)
			{
				float value = (float)propEnt_Stone_Gold.GetAmount() * GetValue(amountAmp);
				NetworkedManagerBase<PickupManager>.instance.DropGold(isKillGold: false, isGivenByOtherPlayer: false, DewMath.RandomRoundToInt(value), obj.victim.position, hero);
			}
			if (obj.victim is PropEnt_Stone_DreamDust propEnt_Stone_DreamDust)
			{
				float value2 = (float)propEnt_Stone_DreamDust.GetAmount() * GetValue(amountAmp);
				NetworkedManagerBase<PickupManager>.instance.DropDreamDust(isGivenByOtherPlayer: false, DewMath.RandomRoundToInt(value2), obj.victim.position, hero);
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
