using System;
using Mirror;
using UnityEngine;

public class Se_Star_Nachia_L_WaltzDurationWithPenalty : StarEffect
{
	public StarScalingValue durationAmp;

	public int chargePenalty;

	public override Type heroType => typeof(Hero_Nachia);

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
					addedCharge = -chargePenalty,
					ignoreReceiveCooldownReductionFlag = true
				});
			}
			hero.ActorEvent_OnAbilityInstanceBeforePrepare += new Action<EventInfoAbilityInstance>(ActorEventOnAbilityInstanceBeforePrepare);
		}
	}

	private void ActorEventOnAbilityInstanceBeforePrepare(EventInfoAbilityInstance obj)
	{
		if (obj.instance is Se_M_DreamyWaltz_Buff se_M_DreamyWaltz_Buff)
		{
			se_M_DreamyWaltz_Buff.duration *= 1f + GetValue(durationAmp);
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
