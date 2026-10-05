using System;
using Mirror;
using UnityEngine;

public class Se_Star_Bismuth_F_IN_Elementals : StarEffect
{
	public override Type heroType => typeof(Hero_Bismuth);

	public override Type skillType => typeof(St_QR_Innocence);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			hero.ActorEvent_OnAbilityInstanceBeforePrepare += new Action<EventInfoAbilityInstance>(ActorEventOnAbilityInstanceBeforePrepare);
		}
	}

	private void ActorEventOnAbilityInstanceBeforePrepare(EventInfoAbilityInstance obj)
	{
		if (obj.instance is Ai_QR_Innocence_Projectile ai_QR_Innocence_Projectile)
		{
			ai_QR_Innocence_Projectile.Networkelemental = (ElementalType)UnityEngine.Random.Range(0, 4);
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

	private void MirrorProcessed()
	{
	}
}
