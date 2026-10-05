[AchUnlockOnComplete(typeof(Gem_E_Fangs))]
public class ACH_THEYRE_JUST_BIG_CATS : DewAchievementItem
{
	public interface ILightingActor
	{
	}

	private const int RequiredKillCount = 25;

	[AchPersistentVar]
	private int _currentKillCount;

	public override int GetMaxProgress()
	{
		return 25;
	}

	public override int GetCurrentProgress()
	{
		return _currentKillCount;
	}

	public override void OnStartLocalClient()
	{
		base.OnStartLocalClient();
		AchOnKillLastHit((EventInfoKill k) =>
		{
			if (k.victim is Mon_Ink_DivineAnimal && (k.actor is ILightingActor || k.actor is Ai_Q_Fleche_Dash { empoweredWithLightning: not false }))
			{
				_currentKillCount++;
				if (_currentKillCount >= 25)
				{
					Complete();
				}
			}
		});
	}
}
