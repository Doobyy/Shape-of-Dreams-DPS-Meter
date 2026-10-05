using System;
using Mirror;
using UnityEngine;

public class Se_Star_Aurena_F_RE_MoreProjMoreSacrifice : StarEffect
{
	public int addedProj = 1;

	public float newSacrificeHpRatio = 0.15f;

	public override Type heroType => typeof(Hero_Aurena);

	public override Type skillType => typeof(St_Q_Reduction);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			if (skill is St_Q_Reduction st_Q_Reduction)
			{
				st_Q_Reduction.NetworksacrificeHpRatioOverride = newSacrificeHpRatio;
			}
			hero.ActorEvent_OnAbilityInstanceBeforePrepare += new Action<EventInfoAbilityInstance>(ActorEventOnAbilityInstanceBeforePrepare);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			if (skill is St_Q_Reduction st_Q_Reduction)
			{
				st_Q_Reduction.NetworksacrificeHpRatioOverride = null;
			}
			if ((UnityEngine.Object)(object)hero != null)
			{
				hero.ActorEvent_OnAbilityInstanceBeforePrepare -= new Action<EventInfoAbilityInstance>(ActorEventOnAbilityInstanceBeforePrepare);
			}
		}
	}

	private void ActorEventOnAbilityInstanceBeforePrepare(EventInfoAbilityInstance obj)
	{
		if (obj.instance is Ai_Q_Reduction_Spawner ai_Q_Reduction_Spawner)
		{
			ai_Q_Reduction_Spawner.shootCount += addedProj;
		}
	}

	private void MirrorProcessed()
	{
	}
}
