using System;
using Mirror;
using UnityEngine;

public class Se_Star_Nachia_F_SC_MoreSummons : StarEffect
{
	public int addedCharges = 1;

	private int addedMaxSpawn = 1;

	public float lifetimePenalty = 3f;

	public override Type heroType => typeof(Hero_Nachia);

	public override Type skillType => typeof(St_Q_SylvanCall);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			hero.ActorEvent_OnSpawnSummon += new Action<EventInfoSummon>(ActorEventOnSpawnSummon);
			hero.ActorEvent_OnAbilityInstanceBeforePrepare += new Action<EventInfoAbilityInstance>(ActorEventOnAbilityInstanceBeforePrepare);
			if ((UnityEngine.Object)(object)skill != null)
			{
				DoSkillBonusAll(new SkillBonus
				{
					addedCharge = addedCharges
				});
			}
		}
	}

	private void ActorEventOnSpawnSummon(EventInfoSummon obj)
	{
		if (obj.summon is Sum_Q_SylvanCall_LeafHound sum_Q_SylvanCall_LeafHound)
		{
			sum_Q_SylvanCall_LeafHound.ReduceDuration(lifetimePenalty);
		}
	}

	private void ActorEventOnAbilityInstanceBeforePrepare(EventInfoAbilityInstance obj)
	{
		if (obj.instance is Ai_Q_SylvanCall ai_Q_SylvanCall)
		{
			ai_Q_SylvanCall.maxCount += addedMaxSpawn;
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)hero != null)
		{
			hero.ActorEvent_OnSpawnSummon -= new Action<EventInfoSummon>(ActorEventOnSpawnSummon);
			hero.ActorEvent_OnAbilityInstanceBeforePrepare -= new Action<EventInfoAbilityInstance>(ActorEventOnAbilityInstanceBeforePrepare);
		}
	}

	private void MirrorProcessed()
	{
	}
}
