using System;
using Mirror;

public class Se_Star_Vesper_I_StartGold : StarEffect
{
	public StarScalingValue addedGoldAmount;

	public override Type heroType => typeof(Hero_Vesper);

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
