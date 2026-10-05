using System;
using Mirror;
using UnityEngine;

public class Se_Star_Cetus_F_BA_ChargeMsNoShield : StarEffect
{
	public float moveSpeedBuffStrength = 30f;

	public override Type heroType => typeof(Hero_Cetus);

	public override Type skillType => typeof(St_R_BackOff);

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
		if (!(obj.instance is Ai_R_BackOff_Spawner ai_R_BackOff_Spawner))
		{
			return;
		}
		ai_R_BackOff_Spawner.disableArmorAndShieldToSelf = true;
		ActorRef<Se_GenericEffectContainer> be = CreateBasicEffect(info.caster, new SpeedEffect
		{
			strength = moveSpeedBuffStrength
		}, 3600f);
		ai_R_BackOff_Spawner.ClientActorEvent_OnDestroyed += (Action<Actor>)((Actor _) =>
		{
			if (!be.IsNullOrInactive())
			{
				be.Get().Destroy();
			}
		});
	}

	private void MirrorProcessed()
	{
	}
}
