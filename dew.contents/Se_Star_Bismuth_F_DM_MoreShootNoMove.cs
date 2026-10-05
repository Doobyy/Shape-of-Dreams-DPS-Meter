using System;
using Mirror;
using UnityEngine;

public class Se_Star_Bismuth_F_DM_MoreShootNoMove : StarEffect
{
	public int increased = 4;

	public GameObject fxArtilleryFlavour;

	public override Type heroType => typeof(Hero_Bismuth);

	public override Type skillType => typeof(St_QR_DistortedMind);

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
		if ((UnityEngine.Object)(object)hero != null)
		{
			hero.ActorEvent_OnAbilityInstanceBeforePrepare -= new Action<EventInfoAbilityInstance>(ActorEventOnAbilityInstanceBeforePrepare);
		}
	}

	private void ActorEventOnAbilityInstanceBeforePrepare(EventInfoAbilityInstance obj)
	{
		if (obj.instance is Ai_QR_DistortedMind_Spawner ai_QR_DistortedMind_Spawner)
		{
			ai_QR_DistortedMind_Spawner.channel.canMove = false;
			ai_QR_DistortedMind_Spawner.countOffset += increased;
			ai_QR_DistortedMind_Spawner.onShoot += (Action<int>)((int _) =>
			{
				FxPlayNewNetworked(fxArtilleryFlavour, hero);
			});
		}
	}

	private void MirrorProcessed()
	{
	}
}
