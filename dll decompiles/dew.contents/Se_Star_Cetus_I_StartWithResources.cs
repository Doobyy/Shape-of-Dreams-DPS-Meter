using System;
using Mirror;
using UnityEngine;

public class Se_Star_Cetus_I_StartWithResources : StarEffect
{
	public StarScalingValue addedGold;

	public StarScalingValue addedDreamDust;

	public override Type heroType => typeof(Hero_Cetus);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		NetworkedManagerBase<ZoneManager>.instance.CallOnReadyAfterTransition(() =>
		{
			if (isActive && !((UnityEngine.Object)(object)hero == null))
			{
				NetworkedManagerBase<PickupManager>.instance.DropGold(isKillGold: false, isGivenByOtherPlayer: false, GetValueInt(addedGold), hero.position, hero);
				NetworkedManagerBase<PickupManager>.instance.DropDreamDust(isGivenByOtherPlayer: false, GetValueInt(addedDreamDust), hero.position, hero);
				Destroy();
			}
		});
	}

	private void MirrorProcessed()
	{
	}
}
