using System;
using Mirror;
using UnityEngine;

public class Se_Star_Vesper_F_SE_FullOffense : StarEffect
{
	public float hasteAmp = 0.5f;

	public int everyFourAttackStartIndexBonus = 1;

	public override Type heroType => typeof(Hero_Vesper);

	public override Type skillType => typeof(St_R_SanctuaryOfEl);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			victim.ActorEvent_OnAbilityInstanceBeforePrepare += new Action<EventInfoAbilityInstance>(ActorEventOnAbilityInstanceBeforePrepare);
			victim.ActorEvent_OnAbilityInstanceCreated += new Action<EventInfoAbilityInstance>(ActorEventOnAbilityInstanceCreated);
		}
	}

	private void ActorEventOnAbilityInstanceBeforePrepare(EventInfoAbilityInstance obj)
	{
		if (obj.instance is Ai_R_SanctuaryOfEl_Ground ai_R_SanctuaryOfEl_Ground)
		{
			ai_R_SanctuaryOfEl_Ground.startShield = "0";
		}
		if (obj.instance is Se_R_SanctuaryOfEl_Buff se_R_SanctuaryOfEl_Buff)
		{
			se_R_SanctuaryOfEl_Buff.disableUnstoppable = true;
			se_R_SanctuaryOfEl_Buff.armorAmount = "0";
			se_R_SanctuaryOfEl_Buff.hasteAmount *= 1f + hasteAmp;
		}
	}

	private void ActorEventOnAbilityInstanceCreated(EventInfoAbilityInstance obj)
	{
		if (obj.instance is Se_R_SanctuaryOfEl_Buff se_R_SanctuaryOfEl_Buff)
		{
			se_R_SanctuaryOfEl_Buff.DoStatBonus(new StatBonus
			{
				everyFourAttackStartIndexFlat = everyFourAttackStartIndexBonus
			});
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)victim != null)
		{
			victim.ActorEvent_OnAbilityInstanceBeforePrepare -= new Action<EventInfoAbilityInstance>(ActorEventOnAbilityInstanceBeforePrepare);
			victim.ActorEvent_OnAbilityInstanceCreated -= new Action<EventInfoAbilityInstance>(ActorEventOnAbilityInstanceCreated);
		}
	}

	private void MirrorProcessed()
	{
	}
}
