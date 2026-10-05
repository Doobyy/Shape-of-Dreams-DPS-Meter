using System;
using Mirror;
using UnityEngine;

public class Se_Star_Mist_F_PR_AllDirections : StarEffect
{
	public float cooldownPenalty = 1.5f;

	public override Type heroType => typeof(Hero_Mist);

	public override Type skillType => typeof(St_R_Parry);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer && !((UnityEngine.Object)(object)skill == null))
		{
			victim.EntityEvent_OnCastCompleteBeforePrepare += new Action<EventInfoCast>(EntityEventOnCastCompleteBeforePrepare);
			DoSkillBonusAll(new SkillBonus
			{
				cooldownOffset = cooldownPenalty
			});
		}
	}

	private void EntityEventOnCastCompleteBeforePrepare(EventInfoCast obj)
	{
		if (obj.instance is Se_R_Parry_Start se_R_Parry_Start)
		{
			se_R_Parry_Start.allowAnyDirection = true;
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)victim != null)
		{
			victim.EntityEvent_OnCastCompleteBeforePrepare -= new Action<EventInfoCast>(EntityEventOnCastCompleteBeforePrepare);
		}
	}

	private void MirrorProcessed()
	{
	}
}
