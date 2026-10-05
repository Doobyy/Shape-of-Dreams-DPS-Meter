using System;
using Mirror;
using UnityEngine;

public class Se_Mon_SnowMountain_SnowWolf_SlowApproach : StatusEffect
{
	public AnimationClip walkAnimation;

	public float walkAnimationSpeed;

	public AnimationClip runAnimation;

	public float runAnimationSpeed;

	public float timeout;

	public float slowAmount = 40f;

	public Vector2 distanceRange;

	public bool endOnDamage;

	public bool resetPounceOnEnd;

	public float speedOnEnd;

	public float speedDuration;

	public bool isSpeedDecay;

	public bool castPounceAtEnd;

	protected override void OnCreate()
	{
		base.OnCreate();
		victim.Animation.ReplaceAnimationLocal(EntityAnimation.ReplaceableAnimationType.RunForward, walkAnimation);
		victim.Animation.model.walkAnimationSpeed = walkAnimationSpeed;
		if (((NetworkBehaviour)this).isServer)
		{
			if (info.target.IsNullInactiveDeadOrKnockedOut())
			{
				Destroy();
				return;
			}
			SetTimer(timeout);
			DoSlow(slowAmount);
			victim.Control.Attack(info.target, doChase: true);
			victim.EntityEvent_OnTakeDamage += new Action<EventInfoDamage>(EntityEventOnTakeDamage);
		}
	}

	private void EntityEventOnTakeDamage(EventInfoDamage obj)
	{
		if (isActive && !(obj.actor is ElementalStatusEffect) && endOnDamage)
		{
			Entity entity = obj.actor.firstEntity;
			if ((UnityEngine.Object)(object)entity != null && (UnityEngine.Object)(object)entity != (UnityEngine.Object)(object)info.target)
			{
				victim.AI.Aggro(entity);
				victim.Control.Attack(entity, doChase: true);
			}
			Destroy();
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if ((UnityEngine.Object)(object)victim != null)
		{
			victim.Animation.ReplaceAnimationLocal(EntityAnimation.ReplaceableAnimationType.RunForward, runAnimation);
			victim.Animation.model.walkAnimationSpeed = runAnimationSpeed;
		}
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)victim != null && victim.isActive)
		{
			victim.EntityEvent_OnTakeDamage -= new Action<EventInfoDamage>(EntityEventOnTakeDamage);
			At_Mon_SnowMountain_SnowWolf_Pounce ability = victim.Ability.GetAbility<At_Mon_SnowMountain_SnowWolf_Pounce>();
			if (resetPounceOnEnd && (UnityEngine.Object)(object)ability != null)
			{
				ResetCooldown(ability);
			}
			if (speedOnEnd > 0f)
			{
				CreateBasicEffect(victim, new SpeedEffect
				{
					decay = isSpeedDecay,
					strength = speedOnEnd
				}, speedDuration, "snowwolf_speed");
			}
			if (castPounceAtEnd && ability.CanBeCast() && (UnityEngine.Object)(object)victim.AI.context.targetEnemy != null && ability.IsTargetInRange(victim.AI.context.targetEnemy, isAI: true))
			{
				victim.Control.Cast(ability, ability.GetPredictedCastInfoToTarget(victim.AI.context.targetEnemy));
			}
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		if (victim.Control.attackTarget.IsNullInactiveDeadOrKnockedOut() || victim.Control.ongoingChannels.Count > 0)
		{
			Destroy();
			return;
		}
		if (info.target.IsNullInactiveDeadOrKnockedOut() || info.target.Status.isUndetectableByNonAllies)
		{
			Destroy();
			return;
		}
		float num = Vector3.Distance(victim.position, info.target.position);
		if (num < distanceRange.x || num > distanceRange.y)
		{
			Destroy();
		}
	}

	private void MirrorProcessed()
	{
	}
}
