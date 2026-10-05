using System;
using Mirror;
using UnityEngine;

public class Se_Star_Aurena_F_DT_UnstoppableAndRemoveDelusion : StarEffect
{
	public float unstoppableTime = 1.5f;

	public override Type heroType => typeof(Hero_Aurena);

	public override Type skillType => typeof(St_R_DangerousTheory);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			hero.ActorEvent_OnAbilityInstanceCreated += new Action<EventInfoAbilityInstance>(ActorEventOnAbilityInstanceCreated);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)hero != null)
		{
			hero.ActorEvent_OnAbilityInstanceCreated -= new Action<EventInfoAbilityInstance>(ActorEventOnAbilityInstanceCreated);
		}
	}

	private void ActorEventOnAbilityInstanceCreated(EventInfoAbilityInstance obj)
	{
		if (obj.instance is Se_R_DangerousTheory)
		{
			CreateBasicEffect(hero, new UnstoppableEffect(), unstoppableTime);
			if (hero.Status.TryGetStatusEffect<Se_MirageSkin_Delusion_Delusional>(out var effect))
			{
				effect.Destroy();
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
