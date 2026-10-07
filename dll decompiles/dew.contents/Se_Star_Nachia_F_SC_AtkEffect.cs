using System;
using Mirror;
using UnityEngine;

public class Se_Star_Nachia_F_SC_AtkEffect : StarEffect
{
	public float atkEffectStrength = 0.3f;

	public override Type heroType => typeof(Hero_Nachia);

	public override Type skillType => typeof(St_Q_SylvanCall);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			hero.ActorEvent_OnAbilityInstanceBeforePrepare += new Action<EventInfoAbilityInstance>(ActorEventOnAbilityInstanceBeforePrepare);
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

	private void ActorEventOnAbilityInstanceBeforePrepare(EventInfoAbilityInstance obj)
	{
		if (obj.instance is Ai_Q_SylvanCall_LeafHound_Atk ai_Q_SylvanCall_LeafHound_Atk)
		{
			ai_Q_SylvanCall_LeafHound_Atk.summonerAttackEffect = atkEffectStrength;
		}
	}

	private void MirrorProcessed()
	{
	}
}
