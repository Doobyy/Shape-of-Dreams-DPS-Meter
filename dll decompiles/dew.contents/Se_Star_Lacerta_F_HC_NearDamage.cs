using System;
using Mirror;
using UnityEngine;

public class Se_Star_Lacerta_F_HC_NearDamage : StarEffect
{
	public float knockbackAmp = 0.4f;

	public GameObject AdditionalStartFx;

	public override Type heroType => typeof(Hero_Lacerta);

	public override Type skillType => typeof(St_Q_HandCannon);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			victim.ActorEvent_OnAbilityInstanceBeforePrepare += new Action<EventInfoAbilityInstance>(ActorEventOnAbilityInstanceBeforePrepare);
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

	private void ActorEventOnAbilityInstanceBeforePrepare(EventInfoAbilityInstance obj)
	{
		if (obj.instance is Ai_Q_HandCannon ai_Q_HandCannon)
		{
			if (AdditionalStartFx != null)
			{
				FxPlayNewNetworked(AdditionalStartFx, ai_Q_HandCannon.position, ai_Q_HandCannon.rotation);
			}
			ai_Q_HandCannon.alwaysKnockback = true;
			ai_Q_HandCannon.knockback.distance *= 1f + knockbackAmp;
		}
	}

	private void MirrorProcessed()
	{
	}
}
