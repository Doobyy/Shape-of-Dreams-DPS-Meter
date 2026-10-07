using System;
using Mirror;
using UnityEngine;

public class Se_Star_Cetus_F_BI_FastChargeTakeDamageUp : StarEffect
{
	public float chargingSpeedAmp;

	public float takenDamageAmp;

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
		if (obj.instance is Ai_Q_BigBorealChunk_Spawner ai_Q_BigBorealChunk_Spawner)
		{
			ai_Q_BigBorealChunk_Spawner.channelSpeedMultiplier *= 1f + chargingSpeedAmp;
			hero.takenDamageProcessor.Add(Processor);
			ai_Q_BigBorealChunk_Spawner.ClientActorEvent_OnDestroyed += (Action<Actor>)((Actor _) =>
			{
				hero.takenDamageProcessor.Remove(Processor);
			});
		}
		void Processor(ref DamageData data, Actor from, Entity to)
		{
			if (!data.IsAmountModifiedBy(this) && from.firstEntity is Monster)
			{
				data.ApplyAmplification(takenDamageAmp);
				data.SetAmountModifiedBy(this);
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
