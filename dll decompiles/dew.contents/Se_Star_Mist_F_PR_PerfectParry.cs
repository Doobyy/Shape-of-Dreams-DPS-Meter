using System;
using Mirror;
using UnityEngine;

public class Se_Star_Mist_F_PR_PerfectParry : StarEffect
{
	public float effectiveDurationReduction = 0.4f;

	public float cooldownReduction = 4f;

	private SkillBonus _bonus;

	public override Type heroType => typeof(Hero_Mist);

	public override Type skillType => typeof(St_R_Parry);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			victim.EntityEvent_OnCastCompleteBeforePrepare += new Action<EventInfoCast>(EntityEventOnCastCompleteBeforePrepare);
			if ((UnityEngine.Object)(object)skill != null)
			{
				_bonus = skill.AddSkillBonus(new SkillBonus
				{
					cooldownOffset = 0f - cooldownReduction
				});
			}
		}
	}

	private void EntityEventOnCastCompleteBeforePrepare(EventInfoCast obj)
	{
		if (obj.instance is Se_R_Parry_Start se_R_Parry_Start)
		{
			se_R_Parry_Start.duration *= 1f - effectiveDurationReduction;
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			if ((UnityEngine.Object)(object)victim != null)
			{
				victim.EntityEvent_OnCastCompleteBeforePrepare -= new Action<EventInfoCast>(EntityEventOnCastCompleteBeforePrepare);
			}
			if (_bonus != null)
			{
				_bonus.Stop();
				_bonus = null;
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
