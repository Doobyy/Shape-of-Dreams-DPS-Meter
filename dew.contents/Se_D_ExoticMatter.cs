using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Se_D_ExoticMatter : StackedStatusEffect
{
	public float attackCooldownAfterTrigger;

	public DewAnimationClip shootAnim;

	public float directionAtkSphereCastRadius = 1f;

	public int dodgeCastStacks = 2;

	public int ultCastStacks = 3;

	private St_D_ExoticMatter _trigger;

	private Hero _hero;

	protected override void OnCreate()
	{
		base.OnCreate();
		_trigger = (St_D_ExoticMatter)firstTrigger;
		_hero = (Hero)victim;
		if (((NetworkBehaviour)this).isServer)
		{
			_trigger.fillAmount = (float)stack / (float)maxStack;
			_hero.ClientHeroEvent_OnSkillUse += new Action<EventInfoSkillUse>(HeroEventOnSkillUse);
			_hero.EntityEvent_OnAttackFired += new Action<EventInfoAttackFired>(EntityEventOnAttackFired);
			_hero.Visual.genericStackIndicatorMax = maxStack;
		}
	}

	private void EntityEventOnAttackFired(EventInfoAttackFired obj)
	{
		if (stack <= 0)
		{
			return;
		}
		Vector3 point;
		if ((UnityEngine.Object)(object)obj.info.target == null)
		{
			point = obj.info.point;
			float maxDistance = Vector3.Distance(obj.info.point, victim.agentPosition);
			List<Entity> list = DewPhysics.SphereCastAllEntities(out var handle, victim.agentPosition, directionAtkSphereCastRadius, obj.info.forward, maxDistance, tvDefaultHarmfulEffectTargets, new CollisionCheckSettings
			{
				sortComparer = CollisionCheckSettings.DistanceFromCenter
			});
			if (list.Count > 0)
			{
				point = list[0].agentPosition;
			}
			handle.Return();
		}
		else
		{
			point = obj.info.target.agentPosition;
		}
		CreateAbilityInstance(info.caster.Visual.GetBonePosition((HumanBodyBones)18), Quaternion.identity, new CastInfo(info.caster, point), (Ai_D_ExoticMatter_Projectile p) =>
		{
			p.NetworkexplosionStack = stack;
		});
		RemoveStack(stack);
		if (attackCooldownAfterTrigger > 0f)
		{
			((MonoBehaviour)(object)this).StartCoroutine(Routine());
			info.caster.Ability.attackAbility.SetCooldownTimeAll(attackCooldownAfterTrigger, scaled: false);
		}
		IEnumerator Routine()
		{
			yield return null;
			if (!((UnityEngine.Object)(object)info.caster == null) && info.caster.isActive && !((Hero)info.caster).isKnockedOut)
			{
				if (info.caster.Ability.attackAbility is At_Atk_YubarStardust at_Atk_YubarStardust)
				{
					at_Atk_YubarStardust.ClearTarget();
				}
				info.caster.Animation.PlayAbilityAnimation(shootAnim);
			}
		}
	}

	private void HeroEventOnSkillUse(EventInfoSkillUse obj)
	{
		if ((obj.type == HeroSkillLocation.Q || obj.type == HeroSkillLocation.W || obj.type == HeroSkillLocation.E || obj.type == HeroSkillLocation.R || obj.type == HeroSkillLocation.Movement) && stack < maxStack)
		{
			if (obj.skill.type == SkillType.Ultimate)
			{
				AddStack(ultCastStacks);
			}
			else if (obj.type == HeroSkillLocation.Movement)
			{
				AddStack(dodgeCastStacks);
			}
			else
			{
				AddStack();
			}
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)_hero != null)
		{
			_hero.ClientHeroEvent_OnSkillUse -= new Action<EventInfoSkillUse>(HeroEventOnSkillUse);
			_hero.EntityEvent_OnAttackFired -= new Action<EventInfoAttackFired>(EntityEventOnAttackFired);
			_hero.Visual.genericStackIndicatorMax = 0;
			_hero.Visual.genericStackIndicatorValue = 0;
		}
	}

	protected override void OnStackChange(int oldStack, int newStack)
	{
		base.OnStackChange(oldStack, newStack);
		if (((NetworkBehaviour)this).isServer)
		{
			_trigger.fillAmount = (float)newStack / (float)maxStack;
			_hero.Visual.genericStackIndicatorValue = stack;
		}
	}

	private void MirrorProcessed()
	{
	}
}
