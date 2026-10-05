using System;
using Mirror;

public class Se_Star_Mist_F_UD_HealLostHealth : StarEffect
{
	public float lostHealthRatio = 0.35f;

	public override Type heroType => typeof(Hero_Mist);

	public override Type skillType => typeof(St_R_UnbreakableDetermination);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			victim.ActorEvent_OnAbilityInstanceCreated += new Action<EventInfoAbilityInstance>(ActorEventOnAbilityInstanceCreated);
		}
	}

	private void ActorEventOnAbilityInstanceCreated(EventInfoAbilityInstance obj)
	{
		if (obj.instance is Se_R_UnbreakableDetermination)
		{
			obj.instance.CreateStatusEffect(hero, new CastInfo(hero, hero), (Se_GenericHealOverTime heal) =>
			{
				heal.tickInterval = 0.1f;
				heal.ticks = 6;
				heal.totalAmount = lostHealthRatio * hero.Status.missingHealth;
			});
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			victim.ActorEvent_OnAbilityInstanceCreated -= new Action<EventInfoAbilityInstance>(ActorEventOnAbilityInstanceCreated);
		}
	}

	private void MirrorProcessed()
	{
	}
}
