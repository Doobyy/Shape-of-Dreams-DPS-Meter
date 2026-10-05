using System;
using Mirror;
using UnityEngine;

public class Se_Star_Vesper_L_EmpowerDodgeShield : StarEffect
{
	public StarScalingValue shieldAmp;

	public StarScalingValue shieldDurationAmp;

	public override Type heroType => typeof(Hero_Vesper);

	public override bool isMovementSkillType => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			hero.ActorEvent_OnAbilityInstanceBeforePrepare += new Action<EventInfoAbilityInstance>(ActorEventOnAbilityInstanceBeforePrepare);
		}
	}

	private void ActorEventOnAbilityInstanceBeforePrepare(EventInfoAbilityInstance obj)
	{
		if (obj.instance is Se_M_Charge se_M_Charge)
		{
			se_M_Charge.gainedShieldMaxHpRatio *= 1f + GetValue(shieldAmp);
			se_M_Charge.shieldDuration *= 1f + GetValue(shieldDurationAmp);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)hero != null)
		{
			hero.ActorEvent_OnAbilityInstanceBeforePrepare -= new Action<EventInfoAbilityInstance>(ActorEventOnAbilityInstanceBeforePrepare);
		}
	}

	private void MirrorProcessed()
	{
	}
}
