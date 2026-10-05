[AchUnlockOnComplete(typeof(St_E_ChainLightning))]
public class ACH_ELECTRIC_TYPE : DewAchievementItem
{
	private const int DamageThreshold = 1500;

	public override void OnStartLocalClient()
	{
		base.OnStartLocalClient();
		AchOnDealDamage((EventInfoDamage d) =>
		{
			if (d.victim.CheckEnemyOrNeutral(DewPlayer.local.hero) && !(d.damage.amount + d.damage.discardedAmount < 1500f))
			{
				Complete();
			}
		});
	}
}
