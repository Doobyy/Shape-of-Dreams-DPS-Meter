using System;
using Mirror;
using UnityEngine;

public class Se_Star_Yubar_L_DodgeRange : StarEffect
{
	public StarScalingValue rangeAmp;

	public override Type heroType => typeof(Hero_Yubar);

	public override bool isMovementSkillType => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			hero.Skill.Movement.configs[0].castMethod._range *= 1f + GetValue(rangeAmp);
			hero.Skill.Movement.SyncCastMethodChanges(0);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)hero != null)
		{
			hero.Skill.Movement.configs[0].castMethod._range /= 1f + GetValue(rangeAmp);
			hero.Skill.Movement.SyncCastMethodChanges(0);
		}
	}

	private void MirrorProcessed()
	{
	}
}
