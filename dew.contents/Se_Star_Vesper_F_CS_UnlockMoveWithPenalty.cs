using System;
using Mirror;
using UnityEngine;

public class Se_Star_Vesper_F_CS_UnlockMoveWithPenalty : StarEffect
{
	public float damageAmp = 0.25f;

	public override Type heroType => typeof(Hero_Vesper);

	public override Type skillType => typeof(St_Q_CruelSun);

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
		if (obj.instance is Ai_Q_CruelSun ai_Q_CruelSun)
		{
			ai_Q_CruelSun.enableMovement = true;
			ai_Q_CruelSun.disableArmor = true;
		}
		if (obj.instance is Ai_Q_CruelSun_Shockwave ai_Q_CruelSun_Shockwave)
		{
			ai_Q_CruelSun_Shockwave.damageMin *= 1f + damageAmp;
			ai_Q_CruelSun_Shockwave.damageMax *= 1f + damageAmp;
		}
	}

	private void MirrorProcessed()
	{
	}
}
