[AchUnlockOnComplete(typeof(Gem_E_Protection))]
public class ACH_TEKKAI : DewAchievementItem
{
	private const float RequiredShieldRatio = 3f;

	public override void OnStartLocalClient()
	{
		base.OnStartLocalClient();
		AchCompleteWhen(() => hero.Status.currentShield >= hero.Status.maxHealth * 3f, 0.25f);
	}
}
