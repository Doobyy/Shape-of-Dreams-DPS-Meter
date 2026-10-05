using System;
using Mirror;
using UnityEngine;

public class Se_Star_Yubar_F_SN_AutoExplosion : StarEffect
{
	public override Type heroType => typeof(Hero_Yubar);

	public override Type skillType => typeof(St_Q_SuperNova);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer && !((UnityEngine.Object)(object)skill == null))
		{
			skill.configs[0].lockCastUntilKilled = false;
			skill.configs[0].lockCooldownUntilKilled = false;
			victim.ActorEvent_OnAbilityInstanceBeforePrepare += new Action<EventInfoAbilityInstance>(ActorEventOnAbilityInstanceBeforePrepare);
		}
	}

	private void ActorEventOnAbilityInstanceBeforePrepare(EventInfoAbilityInstance obj)
	{
		if (obj.instance is Ai_Q_SuperNova ai_Q_SuperNova)
		{
			ai_Q_SuperNova.disableRecastAutoExplode = true;
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			if ((UnityEngine.Object)(object)skill != null)
			{
				skill.configs[0].lockCastUntilKilled = true;
				skill.configs[0].lockCooldownUntilKilled = true;
			}
			if ((UnityEngine.Object)(object)victim != null)
			{
				victim.ActorEvent_OnAbilityInstanceBeforePrepare -= new Action<EventInfoAbilityInstance>(ActorEventOnAbilityInstanceBeforePrepare);
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
