using UnityEngine;

public class Rev_ReachNightSky : DewReverieItem
{
	public override int grantedStardust => 50;

	public override void OnStartLocalClient()
	{
		base.OnStartLocalClient();
		AchCompleteWhen(() => (Object)(object)NetworkedManagerBase<ZoneManager>.instance != null && NetworkedManagerBase<ZoneManager>.instance.currentZone != null && NetworkedManagerBase<ZoneManager>.instance.currentZone.name == "Zone_Sky");
	}
}
