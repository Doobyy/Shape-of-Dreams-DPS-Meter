using System;
using Mirror;
using UnityEngine;

public class Se_Star_I_DismantleDustToBonusGold : StarEffect
{
	public StarScalingValue gainGoldRatio;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			((Hero)victim).HeroEvent_OnDismantleItem += new Action<EventInfoDismantle>(DismantleItem);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)victim != null)
		{
			((Hero)victim).HeroEvent_OnDismantleItem -= new Action<EventInfoDismantle>(DismantleItem);
		}
	}

	private void DismantleItem(EventInfoDismantle infoDismantle)
	{
		float value = (float)infoDismantle.dismantleAmount * GetValue(gainGoldRatio);
		NetworkedManagerBase<PickupManager>.instance.DropGold(isKillGold: false, isGivenByOtherPlayer: false, DewMath.RandomRoundToInt(value), infoDismantle.target.position);
	}

	private void MirrorProcessed()
	{
	}
}
