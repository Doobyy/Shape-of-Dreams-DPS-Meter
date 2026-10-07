using System;
using Mirror;

public class Se_Star_Bismuth_I_SecretJournal : StarEffect
{
	public StarScalingValue grantedAdApPercentage;

	public StarScalingValue dodgeAmp;

	public override Type heroType => typeof(Hero_Bismuth);

	public override bool isMovementSkillType => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		NetworkedManagerBase<ZoneManager>.instance.CallOnReadyAfterTransition(() =>
		{
			if (!this.IsNullOrInactive())
			{
				Dew.CreateGem(Dew.GetGoodRewardPosition(hero.agentPosition), 50, player, (Gem_E_OurStory_Unfinished g) =>
				{
					g.NetworkstatPercentage = GetValue(grantedAdApPercentage);
					g.NetworkdodgeEffectAmp = GetValue(dodgeAmp);
				});
				Destroy();
			}
		});
	}

	private void MirrorProcessed()
	{
	}
}
