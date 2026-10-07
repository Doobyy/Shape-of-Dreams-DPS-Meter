using System;
using Mirror;
using UnityEngine;

public class Se_Star_Lacerta_F_QT_AddedCharges : StarEffect
{
	public float cooldownTimePenalty;

	public override Type heroType => typeof(Hero_Lacerta);

	public override Type skillType => typeof(St_R_QuickTrigger);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer && !((UnityEngine.Object)(object)skill == null))
		{
			DoSkillBonusAll(new SkillBonus
			{
				cooldownOffset = cooldownTimePenalty
			});
			skill.configs[0].addedCharges = 999;
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)skill != null)
		{
			skill.configs[0].addedCharges = 1;
		}
	}

	private void MirrorProcessed()
	{
	}
}
