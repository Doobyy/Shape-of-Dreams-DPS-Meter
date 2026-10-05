using System;
using Mirror;
using UnityEngine;

public class Se_Star_Cetus_F_BI_SelfDamageAoeCold : StarEffect
{
	public override Type heroType => typeof(Hero_Cetus);

	public override Type skillType => typeof(St_Q_BigBorealChunk);

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
		AbilityInstance instance = obj.instance;
		Ai_Q_BigBorealChunk_Spawner ai = instance as Ai_Q_BigBorealChunk_Spawner;
		if (ai != null)
		{
			Ai_Star_Cetus_F_BI_SelfDamageAoeCold_AOE aoe = CreateAbilityInstance(victim.position, null, new CastInfo(victim), (Ai_Star_Cetus_F_BI_SelfDamageAoeCold_AOE ai_Star_Cetus_F_BI_SelfDamageAoeCold_AOE) =>
			{
				ai_Star_Cetus_F_BI_SelfDamageAoeCold_AOE.spawner = ai;
			});
			ai.ClientActorEvent_OnDestroyed += (Action<Actor>)((Actor _) =>
			{
				aoe.Destroy();
			});
		}
	}

	private void MirrorProcessed()
	{
	}
}
