using System;
using Mirror;
using UnityEngine;

public class Se_Star_Bismuth_L_DodgeNoDashMoreCharges : StarEffect
{
	public StarScalingValue addedCharges;

	public override Type heroType => typeof(Hero_Bismuth);

	public override bool isMovementSkillType => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			if ((UnityEngine.Object)(object)skill != null)
			{
				DoSkillBonusAll(new SkillBonus
				{
					addedCharge = GetValueInt(addedCharges)
				});
			}
			hero.ActorEvent_OnAbilityInstanceBeforePrepare += new Action<EventInfoAbilityInstance>(ActorEventOnAbilityInstanceBeforePrepare);
		}
	}

	private void ActorEventOnAbilityInstanceBeforePrepare(EventInfoAbilityInstance obj)
	{
		if (obj.instance is Ai_M_Sprint ai_M_Sprint)
		{
			ai_M_Sprint.minDistance = 0f;
			CastInfo castInfo = ai_M_Sprint.info;
			castInfo.point = hero.agentPosition + (castInfo.point - hero.agentPosition).normalized * 0.001f;
			ai_M_Sprint.info = castInfo;
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
