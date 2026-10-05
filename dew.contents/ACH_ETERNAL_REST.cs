[AchUnlockOnComplete(typeof(St_R_DancingBlades))]
public class ACH_ETERNAL_REST : DewAchievementItem
{
	private const int RequiredKills = 50;

	[AchPersistentVar]
	private int _kills;

	public override int GetMaxProgress()
	{
		return 50;
	}

	public override int GetCurrentProgress()
	{
		return _kills;
	}

	public override void OnStartLocalClient()
	{
		base.OnStartLocalClient();
		AchOnKillLastHit((EventInfoKill k) =>
		{
			if (k.victim is Mon_Ink_GhostBlade)
			{
				Actor actor = k.actor;
				if (actor is AttackProjectile || actor is MeleeAttackInstance)
				{
					_kills++;
					if (_kills >= 50)
					{
						Complete();
					}
				}
			}
		});
	}
}
