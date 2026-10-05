using System;
using Mirror;
using UnityEngine;

public class Se_Star_Lacerta_F_PS_ChargeSpeed : StarEffect
{
	public float speedBonusRatio;

	public override Type heroType => typeof(Hero_Lacerta);

	public override Type skillType => typeof(St_R_PrecisionShot);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			victim.ActorEvent_OnAbilityInstanceBeforePrepare += new Action<EventInfoAbilityInstance>(ActorEventOnAbilityInstanceBeforePrepare);
		}
	}

	private void ActorEventOnAbilityInstanceBeforePrepare(EventInfoAbilityInstance obj)
	{
		if (obj.instance is Ai_R_PrecisionShot { channel: var channel })
		{
			channel.chargeFullDuration /= 1f + speedBonusRatio;
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)victim != null)
		{
			victim.ActorEvent_OnAbilityInstanceBeforePrepare -= new Action<EventInfoAbilityInstance>(ActorEventOnAbilityInstanceBeforePrepare);
		}
	}

	private void MirrorProcessed()
	{
	}
}
