using System;
using Mirror;

public class Se_Star_Bismuth_I_TreasureWithHealthPenalty : StarEffect
{
	public float healthPenaltyPercentage = 15f;

	public StarScalingValue goldValue;

	public StarScalingValue dreamDustValue;

	public override Type heroType => typeof(Hero_Bismuth);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		if (isNewInstance)
		{
			NetworkedManagerBase<ZoneManager>.instance.CallOnReadyAfterTransition(() =>
			{
				if (!this.IsNullOrInactive())
				{
					Dew.CreateGem(Dew.GetGoodRewardPosition(hero.agentPosition), 100, player, (Gem_L_MetalCrystal g) =>
					{
						g.NetworkhealthBonusPercentage = (100f / (100f - healthPenaltyPercentage) - 1f) * 100f;
						g.sellGoldOverride = GetValueInt(goldValue);
						g.dismantleDreamDustOverride = GetValueInt(dreamDustValue);
					});
				}
			});
		}
		DoStatBonus(new StatBonus
		{
			maxHealthPercentage = 0f - healthPenaltyPercentage
		});
	}

	private void MirrorProcessed()
	{
	}
}
