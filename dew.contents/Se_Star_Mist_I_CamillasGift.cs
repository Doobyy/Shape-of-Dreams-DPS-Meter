using System;
using Mirror;

public class Se_Star_Mist_I_CamillasGift : StarEffect
{
	public StarScalingValue baseValue;

	public StarScalingValue quality;

	public StarScalingValue increasePerWorld;

	public override Type heroType => typeof(Hero_Mist);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		GameManager.CallOnReady(() =>
		{
			Dew.CreateGem(Dew.GetGoodRewardPosition(victim.agentPosition), GetValueInt(quality), victim.owner, (Gem_L_CamillasGift g) =>
			{
				g.sellGoldOverride = GetValueInt(baseValue);
				g.NetworksellGoldIncreasePerWorld = GetValueInt(increasePerWorld);
			});
			Destroy();
		});
	}

	private void MirrorProcessed()
	{
	}
}
