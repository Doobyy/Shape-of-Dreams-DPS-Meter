using UnityEngine;

[AchUnlockOnComplete(typeof(St_Q_Discipline))]
public class ACH_PEAK_PERFORMANCE : DewAchievementItem
{
	private const int RequiredMovementSkills = 3;

	public override void OnStartLocalClient()
	{
		base.OnStartLocalClient();
		if (((object)DewPlayer.local.hero).GetType() != typeof(Hero_Vesper))
		{
			return;
		}
		AchCompleteWhen(() =>
		{
			int num = 0;
			if (Check(DewPlayer.local.hero.Skill.Q))
			{
				num++;
			}
			if (Check(DewPlayer.local.hero.Skill.W))
			{
				num++;
			}
			if (Check(DewPlayer.local.hero.Skill.E))
			{
				num++;
			}
			if (Check(DewPlayer.local.hero.Skill.R))
			{
				num++;
			}
			return num >= 3;
		});
		static bool Check(SkillTrigger st)
		{
			if ((Object)(object)st != null)
			{
				if (!st.configs[0].selfValidator.isMovementAbility)
				{
					return (st.tags & DescriptionTags.Mobility) != 0;
				}
				return true;
			}
			return false;
		}
	}
}
