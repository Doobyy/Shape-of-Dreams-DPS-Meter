using Mirror;
using UnityEngine;

public class PropEnt_Stone_Gold : PropEntity
{
	public Formula amountByZoneIndex;

	public override bool isRegularReward => true;

	public int GetAmount()
	{
		return DewMath.RandomRoundToInt(amountByZoneIndex.Evaluate(NetworkedManagerBase<ZoneManager>.instance.currentZoneIndex));
	}

	protected override void OnDeath(EventInfoKill info)
	{
		base.OnDeath(info);
		if (((NetworkBehaviour)this).isServer)
		{
			int amount = GetAmount();
			NetworkedManagerBase<PickupManager>.instance.DropGold(isKillGold: false, isGivenByOtherPlayer: false, amount * DewPlayer.gamePlayers.Count, ((Component)(object)this).transform.position);
		}
	}

	private void MirrorProcessed()
	{
	}
}
