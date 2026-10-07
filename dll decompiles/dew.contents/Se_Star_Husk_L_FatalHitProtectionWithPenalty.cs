using System;
using Mirror;

public class Se_Star_Husk_L_FatalHitProtectionWithPenalty : StarEffect
{
	public StarScalingValue invulTime;

	public StarScalingValue healthPenalty;

	public override Type heroType => typeof(Hero_Husk);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		DoDeathInterrupt((EventInfoKill _) =>
		{
			victim.Status.SetHealth(victim.Status.maxHealth * 0.25f);
			if (!victim.Status.HasStatusEffect<Se_FatalHitProtection_Invulnerable>())
			{
				CreateStatusEffect(victim, (Se_FatalHitProtection_Invulnerable se) =>
				{
					se.duration = GetValue(invulTime);
					se.timerCustomNameKey = "Se_Star_Husk_L_FatalHitProtectionWithPenalty";
				});
				CreateStatusEffect<Se_Star_Husk_L_FatalHitProtectionWithPenalty_Repaired>(victim).bonus.maxHealthPercentage = 0f - GetValue(healthPenalty);
				Dew.CallDelayed(() =>
				{
					if (!this.IsNullOrInactive())
					{
						Destroy();
					}
				});
			}
		}, 100);
	}

	private void MirrorProcessed()
	{
	}
}
