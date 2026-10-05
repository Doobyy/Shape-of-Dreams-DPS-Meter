using UnityEngine;

public class Se_Star_I_ShopRefreshToken : EveryZoneStarEffect
{
	public StarScalingValue numOfTokens;

	public int maxTokens;

	public override void OnNewZoneReached()
	{
		NetworkedManagerBase<ZoneManager>.instance.CallOnReadyAfterTransition(() =>
		{
			if (!this.IsNullOrInactive() && player.platinumCoin < maxTokens)
			{
				player.platinumCoin = Mathf.Min(player.platinumCoin + GetValueInt(numOfTokens), maxTokens);
			}
		});
	}

	private void MirrorProcessed()
	{
	}
}
