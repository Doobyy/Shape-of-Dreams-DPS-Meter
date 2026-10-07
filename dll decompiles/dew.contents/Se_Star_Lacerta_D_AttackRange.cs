using System;
using Mirror;
using UnityEngine;

public class Se_Star_Lacerta_D_AttackRange : StarEffect
{
	public StarScalingValue rangeBonus;

	public override Type heroType => typeof(Hero_Lacerta);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			victim.Ability.attackAbility.configs[0].castMethod._range += GetValue(rangeBonus);
			victim.Ability.attackAbility.configs[1].castMethod._range += GetValue(rangeBonus);
			victim.Ability.attackAbility.SyncCastMethodChanges(0);
			victim.Ability.attackAbility.SyncCastMethodChanges(1);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)victim != null)
		{
			victim.Ability.attackAbility.configs[0].castMethod._range -= GetValue(rangeBonus);
			victim.Ability.attackAbility.configs[1].castMethod._range -= GetValue(rangeBonus);
			victim.Ability.attackAbility.SyncCastMethodChanges(0);
			victim.Ability.attackAbility.SyncCastMethodChanges(1);
		}
	}

	private void MirrorProcessed()
	{
	}
}
