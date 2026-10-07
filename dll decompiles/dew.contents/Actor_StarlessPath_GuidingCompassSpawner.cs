using Mirror;
using UnityEngine;

public class Actor_StarlessPath_GuidingCompassSpawner : Actor
{
	public int baseQuality = 50;

	public int qualityGainPerZone = 50;

	public int maxQuality = 250;

	public override bool ShouldBeSavedWithRoom()
	{
		return true;
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer && isNewInstance)
		{
			int quality = Mathf.Clamp(NetworkedManagerBase<ZoneManager>.instance.currentZoneIndex * qualityGainPerZone + baseQuality, 10, maxQuality);
			Dew.CreateGem<Gem_U_GuidingCompass_NotCharged>(position, quality);
		}
	}

	private void MirrorProcessed()
	{
	}
}
