using System.Runtime.InteropServices;

[AchUnlockOnComplete(typeof(LucidDream_KindArmadillo))]
public class ACH_CLEAR_MIND : DewAchievementItem
{
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	private struct Ad_DidHaveMirageSkin
	{
	}

	private const int MaxKillsCount = 100;

	[AchPersistentVar]
	private int _currentKills;

	public override int GetMaxProgress()
	{
		return 100;
	}

	public override int GetCurrentProgress()
	{
		return _currentKills;
	}

	public override void OnStartLocalClient()
	{
		base.OnStartLocalClient();
		AchOnDealDamage((EventInfoDamage d) =>
		{
			if (DewPlayer.local.hero.CheckEnemyOrNeutral(d.victim) && d.victim.Status.HasStatusEffect<MirageSkinEffect>() && !d.victim.HasData<Ad_DidHaveMirageSkin>())
			{
				d.victim.AddData<Ad_DidHaveMirageSkin>(default);
			}
		});
		AchOnKillOrAssist((EventInfoKill k) =>
		{
			Entity victim = k.victim;
			if (DewPlayer.local.hero.CheckEnemyOrNeutral(victim) && victim.HasData<Ad_DidHaveMirageSkin>())
			{
				_currentKills++;
				if (_currentKills >= 100)
				{
					Complete();
				}
			}
		});
	}
}
