using Mirror;
using UnityEngine;

public class RoomMod_MerchantBackpack : RoomModifierBase
{
	protected override void OnCreate()
	{
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		Dew.CallDelayed(() =>
		{
			PropEnt_Merchant_Jonas propEnt_Merchant_Jonas = Dew.FindActorOfType<PropEnt_Merchant_Jonas>();
			if (!((Object)(object)propEnt_Merchant_Jonas == null))
			{
				propEnt_Merchant_Jonas.Destroy();
				PlaceShrine<Shrine_Merchant_Backpack>(new PlaceShrineSettings
				{
					customPosition = propEnt_Merchant_Jonas.position,
					lockedUntilCleared = true,
					removeModifierOnUse = false
				});
			}
		});
	}

	private void MirrorProcessed()
	{
	}
}
