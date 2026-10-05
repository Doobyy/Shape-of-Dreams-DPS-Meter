using UnityEngine;

[AchUnlockOnComplete(typeof(St_L_Blizzard))]
public class ACH_FIGHTING_COLD_WITH_COLD : DewAchievementItem
{
	private const int RequiredCount = 4;

	public override void OnStartLocalClient()
	{
		base.OnStartLocalClient();
		AchOnKillOrAssist((EventInfoKill kill) =>
		{
			int count;
			if (kill.victim is Mon_SnowMountain_BossSkoll)
			{
				count = 0;
				Check(hero.Skill.Q);
				Check(hero.Skill.W);
				Check(hero.Skill.E);
				Check(hero.Skill.R);
				Check(hero.Skill.Identity);
				if (count >= 4)
				{
					Complete();
				}
			}
			void Check(SkillTrigger skill)
			{
				if ((Object)(object)skill != null && skill.tags.HasFlag(DescriptionTags.Cold))
				{
					count++;
				}
			}
		});
	}
}
