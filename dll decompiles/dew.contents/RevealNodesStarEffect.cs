public abstract class RevealNodesStarEffect : EveryZoneStarEffect
{
	public StarScalingValue locationCount;

	public override void OnNewZoneReached()
	{
		NetworkedManagerBase<ZoneManager>.instance.CallOnReadyAfterTransition(() =>
		{
			NetworkedManagerBase<ZoneManager>.instance.RevealNodesAndAnnounce(player, GetValueInt(locationCount));
		});
	}

	private void MirrorProcessed()
	{
	}
}
