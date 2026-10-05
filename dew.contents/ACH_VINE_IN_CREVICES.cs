[AchUnlockOnComplete(typeof(St_D_AstridsMasterpiecePriorite))]
public class ACH_VINE_IN_CREVICES : DewAchievementItem
{
	private const int RequiredMinutes = 2;

	public override void OnStartLocalClient()
	{
		base.OnStartLocalClient();
		if (((object)DewPlayer.local.hero).GetType() != typeof(Hero_Mist))
		{
			return;
		}
		AchOnKillOrAssist((EventInfoKill k) =>
		{
			if (k.victim is Mon_Forest_BossDemon && !(NetworkedManagerBase<GameManager>.instance.elapsedGameTime > 120f))
			{
				Complete();
			}
		});
	}
}
