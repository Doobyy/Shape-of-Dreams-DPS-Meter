using System;
using Mirror;
using UnityEngine;

public class Se_Star_Vesper_F_BS_ExplosionOnEnd : StarEffect
{
	public float durationReduceMultiplier = 0.66f;

	public float explosionDamageMultiplier = 0.5f;

	public override Type heroType => typeof(Hero_Vesper);

	public override Type skillType => typeof(St_R_BaptismOfSun);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			victim.ActorEvent_OnAbilityInstanceBeforePrepare += new Action<EventInfoAbilityInstance>(OnBeforePrepare);
			victim.ActorEvent_OnAbilityInstanceCreated += new Action<EventInfoAbilityInstance>(OnCreated);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && !((UnityEngine.Object)(object)victim == null))
		{
			victim.ActorEvent_OnAbilityInstanceBeforePrepare -= new Action<EventInfoAbilityInstance>(OnBeforePrepare);
			victim.ActorEvent_OnAbilityInstanceCreated -= new Action<EventInfoAbilityInstance>(OnCreated);
		}
	}

	private void OnBeforePrepare(EventInfoAbilityInstance obj)
	{
		AbilityInstance instance = obj.instance;
		if (!(instance is Ai_R_BaptismOfSun ai_R_BaptismOfSun))
		{
			if (instance is Se_R_BaptismOfSun_Buff se_R_BaptismOfSun_Buff)
			{
				se_R_BaptismOfSun_Buff.duration *= 1f - durationReduceMultiplier;
			}
		}
		else
		{
			ai_R_BaptismOfSun.dmgFactor *= 1f + explosionDamageMultiplier;
		}
	}

	private void OnCreated(EventInfoAbilityInstance obj)
	{
		if (!((NetworkBehaviour)this).isServer || !(obj.instance is Se_R_BaptismOfSun_Buff se_R_BaptismOfSun_Buff))
		{
			return;
		}
		se_R_BaptismOfSun_Buff.ClientActorEvent_OnDestroyed += (Action<Actor>)((Actor _) =>
		{
			if (!hero.IsNullInactiveDeadOrKnockedOut())
			{
				CreateAbilityInstance(hero.position, null, new CastInfo(hero, hero.position), (Ai_R_BaptismOfSun ai) =>
				{
					ai.skipBuff = true;
				});
			}
		});
	}

	private void MirrorProcessed()
	{
	}
}
