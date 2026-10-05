[AchUnlockOnComplete(typeof(LucidDream_PrudentJellyfish))]
public class ACH_WRIST_FRIENDLY_BUILD : DewAchievementItem
{
	private const int RequiredCount = 4;

	private const float MinCooldownTime = 6f;

	public override void OnStartLocalClient()
	{
		base.OnStartLocalClient();
		AchOnGameConcluded((DewGameResult res) =>
		{
			int count;
			if (res.result.IsWin())
			{
				count = 0;
				Check(hero.Skill.Q);
				Check(hero.Skill.W);
				Check(hero.Skill.E);
				Check(hero.Skill.R);
				if (count >= 4)
				{
					Complete();
				}
			}
			void Check(SkillTrigger st)
			{
				if (!st.IsNullOrInactive() && (st.GetMaxCooldownTime(0) >= 6f || st.type == SkillType.Ultimate))
				{
					count++;
				}
			}
		});
	}
}
