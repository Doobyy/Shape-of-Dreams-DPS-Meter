using Mirror;

public class Se_Star_I_StartGold : StarEffect
{
	public StarScalingValue addedGoldAmount;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			player.EarnGold(GetValueInt(addedGoldAmount));
			Destroy();
		}
	}

	private void MirrorProcessed()
	{
	}
}
