using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Se_Star_Cetus_F_BA_LightningEcho : StarEffect
{
	public float lightningEchoDelay = 0.3f;

	public float lightningEchoInterval = 0.1f;

	public float lightningEchoDmgMultiplier = 0.4f;

	public float cooldownTimePenalty = 4f;

	public override Type heroType => typeof(Hero_Cetus);

	public override Type skillType => typeof(St_R_BackOff);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			DoSkillBonusAll(new SkillBonus
			{
				cooldownOffset = cooldownTimePenalty
			});
			hero.ActorEvent_OnAbilityInstanceBeforePrepare += new Action<EventInfoAbilityInstance>(ActorEventOnAbilityInstanceBeforePrepare);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (bool)(UnityEngine.Object)(object)hero)
		{
			hero.ActorEvent_OnAbilityInstanceBeforePrepare -= new Action<EventInfoAbilityInstance>(ActorEventOnAbilityInstanceBeforePrepare);
		}
	}

	private void ActorEventOnAbilityInstanceBeforePrepare(EventInfoAbilityInstance obj)
	{
		AbilityInstance instance = obj.instance;
		Ai_R_BackOff_Damage ai = instance as Ai_R_BackOff_Damage;
		if (ai == null)
		{
			return;
		}
		List<Entity> localLightningTargets = new List<Entity>();
		ai.onHit += (Action<Entity>)((Entity entity) =>
		{
			localLightningTargets.Add(entity);
		});
		ai.ClientActorEvent_OnDestroyed += (Action<Actor>)((Actor _) =>
		{
			if (isActive && localLightningTargets.Count != 0)
			{
				float dmgAmount = ai.GetValue(ai.dmgFactor) * lightningEchoDmgMultiplier;
				((MonoBehaviour)(object)this).StartCoroutine(Routine(ai, localLightningTargets, dmgAmount));
				ai.LockDestroyFor(lightningEchoDelay + 0.5f);
			}
		});
	}

	private IEnumerator Routine(AbilityInstance ai, List<Entity> targets, float dmgAmount)
	{
		yield return new WaitForSeconds(lightningEchoDelay);
		foreach (Entity target in targets)
		{
			if (!target.IsNullInactiveDeadOrKnockedOut())
			{
				ai.CreateAbilityInstance(target.position, null, new CastInfo(info.caster, target), (Ai_Star_Cetus_F_BA_LightningEcho_Lightning lightning) =>
				{
					lightning.dmgAmount = dmgAmount;
				});
				yield return new WaitForSeconds(lightningEchoInterval);
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
