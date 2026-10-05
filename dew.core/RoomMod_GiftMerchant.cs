using System;

public class RoomMod_GiftMerchant : SpecialEntityRoomModifier
{
	protected override Type GetEntityType()
	{
		return typeof(PropEnt_Merchant_Smoothie);
	}

	private void MirrorProcessed()
	{
	}
}
