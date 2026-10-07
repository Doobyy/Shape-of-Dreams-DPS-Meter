using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Se_Star_Mist_F_LG_ChainAttack : StarEffect
{
	public float addedCooldown = 1.5f;

	public float windowDuration = 3f;

	public float searchRadius = 4f;

	public int stabCount = 3;

	private float _lastCheckTime = float.NegativeInfinity;

	public override Type heroType => typeof(Hero_Mist);

	public override Type skillType => typeof(St_Q_Lunge);

	private bool WindowActive => Time.time < _lastCheckTime;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			DoSkillBonusAll(new SkillBonus
			{
				cooldownOffset = addedCooldown
			});
			hero.ClientHeroEvent_OnSkillUse += new Action<EventInfoSkillUse>(OnSkillUse);
			victim.ActorEvent_OnAbilityInstanceBeforePrepare += new Action<EventInfoAbilityInstance>(OnBeforePrepare);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			if ((UnityEngine.Object)(object)hero != null)
			{
				hero.ClientHeroEvent_OnSkillUse -= new Action<EventInfoSkillUse>(OnSkillUse);
			}
			if ((UnityEngine.Object)(object)victim != null)
			{
				victim.ActorEvent_OnAbilityInstanceBeforePrepare -= new Action<EventInfoAbilityInstance>(OnBeforePrepare);
			}
		}
	}

	private void OnSkillUse(EventInfoSkillUse obj)
	{
		if (obj.type == HeroSkillLocation.Movement)
		{
			_lastCheckTime = Time.time + windowDuration;
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)skill != null)
		{
			skill.fillAmount = (WindowActive ? ((_lastCheckTime - Time.time) / windowDuration) : 0f);
		}
	}

	private void OnBeforePrepare(EventInfoAbilityInstance obj)
	{
		AbilityInstance instance = obj.instance;
		Ai_Q_Lunge ai = instance as Ai_Q_Lunge;
		if (ai != null && WindowActive)
		{
			_lastCheckTime = float.NegativeInfinity;
			ai.ActorEvent_OnDealDamage += new Action<EventInfoDamage>(OnLungeHit);
		}
		void OnLungeHit(EventInfoDamage dmg)
		{
			ai.ActorEvent_OnDealDamage -= new Action<EventInfoDamage>(OnLungeHit);
			SpawnPhantom(dmg.victim.position, dmg.damage.amount, ((Component)(object)ai).transform.forward);
		}
	}

	private void SpawnPhantom(Vector3 center, float dmgAmount, Vector3 lungeDir)
	{
		List<Entity> list = DewPhysics.OverlapCircleAllEntities(out var handle, center, searchRadius, tvDefaultHarmfulEffectTargets, new CollisionCheckSettings
		{
			sortComparer = CollisionCheckSettings.DistanceFromCenter
		});
		List<Entity> targets = new List<Entity>();
		foreach (Entity item in list)
		{
			if (targets.Count >= stabCount)
			{
				break;
			}
			if (!item.IsNullInactiveDeadOrKnockedOut())
			{
				targets.Add(item);
			}
		}
		handle.Return();
		if (targets.Count != 0)
		{
			while (targets.Count < stabCount)
			{
				targets.Add(targets[0]);
			}
			hero.CreateAbilityInstance(center, null, new CastInfo(hero), (Ai_Star_Mist_F_LG_ChainAttack_Phantom ai) =>
			{
				ai.targets = targets;
				ai.orignDmg = dmgAmount;
				ai.lungeDir = lungeDir;
				ai.retargetRadius = searchRadius;
			});
		}
	}

	private void MirrorProcessed()
	{
	}
}
