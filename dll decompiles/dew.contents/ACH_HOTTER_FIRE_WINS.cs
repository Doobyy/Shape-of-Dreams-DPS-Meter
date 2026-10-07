[AchUnlockOnComplete(typeof(St_R_BaptismOfSun))]
public class ACH_HOTTER_FIRE_WINS : DewAchievementItem
{
	private const int RequiredFireSkills = 3;

	public override void OnStartLocalClient()
	{
		base.OnStartLocalClient();
		if (((object)DewPlayer.local.hero).GetType() != typeof(Hero_Vesper))
		{
			return;
		}
		AchOnKillOrAssist((EventInfoKill kill) =>
		{
			int count;
			if (kill.victim is Mon_LavaLand_BossInfernus)
			{
				count = 0;
				Check(hero.Skill.Q);
				Check(hero.Skill.W);
				Check(hero.Skill.E);
				Check(hero.Skill.R);
				Check(hero.Skill.Identity);
				if (count >= 3)
				{
					Complete();
				}
			}
			void Check(SkillTrigger st)
			{
				if (!st.IsNullOrInactive() && (st.tags & DescriptionTags.Fire) != 0)
				{
					count++;
				}
			}
		});
	}
}
