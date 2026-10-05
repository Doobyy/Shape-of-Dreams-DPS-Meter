using Mirror;
using UnityEngine;

public class Treasure_DreamingStarfish : Treasure
{
	public float conversionRatio = 0.33f;

	public int grantedDreamDust => Mathf.RoundToInt(conversionRatio * (float)price);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			NetworkedManagerBase<PickupManager>.instance.DropDreamDust(isGivenByOtherPlayer: false, grantedDreamDust, position, hero);
			Destroy();
		}
	}

	private void MirrorProcessed()
	{
	}
}
