using System;
using Mirror;
using UnityEngine;

public class Se_Star_Aurena_F_GB_LessDamageLessSacrifice : StarEffect
{
	public float sacrificeHpReduction = 0.4f;

	public float damageReduction = 0.4f;

	public override Type heroType => typeof(Hero_Aurena);

	public override Type skillType => typeof(St_Q_GoldenBurst);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			if (skill is St_Q_GoldenBurst st_Q_GoldenBurst)
			{
				st_Q_GoldenBurst.NetworksacrificeHpMultiplier = st_Q_GoldenBurst.sacrificeHpMultiplier * (1f - sacrificeHpReduction);
			}
			hero.ActorEvent_OnAbilityInstanceBeforePrepare += new Action<EventInfoAbilityInstance>(ActorEventOnAbilityInstanceBeforePrepare);
		}
	}

	private void ActorEventOnAbilityInstanceBeforePrepare(EventInfoAbilityInstance obj)
	{
		if (obj.instance is Ai_Q_GoldenBurst ai_Q_GoldenBurst)
		{
			ai_Q_GoldenBurst.damageAmount *= 1f - damageReduction;
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			if ((UnityEngine.Object)(object)hero != null)
			{
				hero.ActorEvent_OnAbilityInstanceBeforePrepare -= new Action<EventInfoAbilityInstance>(ActorEventOnAbilityInstanceBeforePrepare);
			}
			if (skill is St_Q_GoldenBurst st_Q_GoldenBurst)
			{
				st_Q_GoldenBurst.NetworksacrificeHpMultiplier = st_Q_GoldenBurst.sacrificeHpMultiplier / (1f - sacrificeHpReduction);
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
