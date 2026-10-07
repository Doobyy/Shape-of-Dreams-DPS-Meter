using System;
using System.Collections;
using Mirror;
using UnityEngine;

public class Se_Star_Lacerta_F_PS_CooldownReset : StarEffect
{
	public float widthReduction = 0.6f;

	public float killGracePeriod = 0.75f;

	public override Type heroType => typeof(Hero_Lacerta);

	public override Type skillType => typeof(St_R_PrecisionShot);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			victim.ActorEvent_OnAbilityInstanceBeforePrepare += new Action<EventInfoAbilityInstance>(ActorEventOnAbilityInstanceBeforePrepare);
		}
	}

	private void ActorEventOnAbilityInstanceBeforePrepare(EventInfoAbilityInstance obj)
	{
		if (obj.instance is Ai_R_PrecisionShot ai_R_PrecisionShot)
		{
			ai_R_PrecisionShot.channel.castMethod._width *= 1f - widthReduction;
		}
		AbilityInstance instance = obj.instance;
		Ai_R_PrecisionShot_Projectile proj = instance as Ai_R_PrecisionShot_Projectile;
		if (proj == null)
		{
			return;
		}
		proj.collisionRadius *= 1f - widthReduction;
		RefValue<bool> hasUsed = new RefValue<bool>(v: false);
		KillTracker tracker = proj.TrackKills(killGracePeriod, (EventInfoKill _) =>
		{
			if (!hasUsed)
			{
				hasUsed.value = true;
				if ((UnityEngine.Object)(object)proj.firstTrigger != null)
				{
					proj.ResetCooldown(proj.firstTrigger);
				}
			}
		});
		proj.ClientActorEvent_OnDestroyed += (Action<Actor>)((Actor _) =>
		{
			ActorRef<Actor> projRef = proj;
			Dew.GetCoroutiner().StartCoroutine(Routine());
			IEnumerator Routine()
			{
				yield return new WaitForSeconds(killGracePeriod);
				if ((UnityEngine.Object)(object)projRef.Get() != null)
				{
					tracker.Stop();
				}
			}
		});
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)victim != null)
		{
			victim.ActorEvent_OnAbilityInstanceBeforePrepare -= new Action<EventInfoAbilityInstance>(ActorEventOnAbilityInstanceBeforePrepare);
		}
	}

	private void MirrorProcessed()
	{
	}
}
