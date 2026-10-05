using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Ai_C_BeamOfLight : AbilityInstance
{
	public float targetRadius = 8f;

	public float beamInterval = 0.01666667f;

	public GameObject beamEffect;

	public GameObject targetEffect;

	public GameObject handEffect;

	public GameObject healEffect;

	public LineRenderer lineRenderer;

	public DewAnimationClip canceledClip;

	public float maxDistance;

	public float selfSlow;

	public AbilityTargetValidator hittable;

	public float tickInterval;

	public ScalingValue ticks;

	public ScalingValue tickDamage;

	public ScalingValue tickHeal;

	public float procCoefficient = 0.25f;

	private float _lastTickTime;

	private int _ticksDone;

	private int _maxTicks;

	private bool _atTarget;

	private float _lastBeamInterval;

	private AbilityTrigger _firstTrigger;

	private Channel _beamChannel;

	private ActorRef<StatusEffect> _slow;

	public override bool reuseInRoom => true;

	protected override void OnDisable()
	{
		base.OnDisable();
		_ticksDone = 0;
		_lastTickTime = 0f;
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		if (beamEffect != null)
		{
			beamEffect.transform.position = info.caster.Visual.GetBonePosition((HumanBodyBones)18);
		}
		_atTarget = false;
		_lastBeamInterval = Time.time;
		if (lineRenderer != null)
		{
			lineRenderer.SetPosition(0, info.caster.Visual.GetBonePosition((HumanBodyBones)18));
			lineRenderer.SetPosition(1, info.target.Visual.GetCenterPosition());
		}
		FxPlay(beamEffect);
		FxPlay(targetEffect, info.target);
		FxPlay(handEffect, info.caster);
		if (lineRenderer != null)
		{
			lineRenderer.enabled = true;
		}
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		_firstTrigger = firstTrigger;
		DestroyOnDeath(info.caster);
		_maxTicks = Mathf.RoundToInt(GetValue(ticks));
		_beamChannel = new Channel
		{
			blockedActions = (Channel.BlockedAction.Ability | Channel.BlockedAction.Attack | Channel.BlockedAction.Cancelable),
			duration = tickInterval * (float)(_maxTicks - 1),
			onCancel = () =>
			{
				if (isActive)
				{
					Destroy();
				}
			},
			uncancellableTime = 0.5f
		}.AddValidation(AbilitySelfValidator.Default);
		info.caster.Control.StartChannel(_beamChannel);
		if (selfSlow > 0f)
		{
			_slow = CreateBasicEffect(info.caster, new SlowEffect
			{
				strength = selfSlow
			}, tickInterval * (float)(_maxTicks - 1), "purification");
		}
	}

	protected override void ActiveFrameUpdate()
	{
		base.ActiveFrameUpdate();
		if ((Object)(object)info.target == null)
		{
			return;
		}
		if (Time.time - _lastBeamInterval > beamInterval)
		{
			_atTarget = !_atTarget;
			if (beamEffect != null)
			{
				beamEffect.transform.position = (_atTarget ? info.target.Visual.GetCenterPosition() : info.caster.Visual.GetBonePosition((HumanBodyBones)18));
			}
			_lastBeamInterval = Time.time;
		}
		if (lineRenderer != null)
		{
			lineRenderer.SetPosition(0, info.caster.Visual.GetBonePosition((HumanBodyBones)18));
			lineRenderer.SetPosition(1, info.target.Visual.GetCenterPosition());
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		if ((Object)(object)_firstTrigger != null)
		{
			_firstTrigger.fillAmount = 1f - (float)_ticksDone / (float)_maxTicks;
		}
		if ((Object)(object)info.target != null)
		{
			info.caster.Control.RotateTowards(info.target.position, immediately: false, 0.1f);
		}
		if (Time.time - _lastTickTime < tickInterval)
		{
			return;
		}
		_lastTickTime = Time.time;
		_ticksDone++;
		if (info.target.IsNullInactiveDeadOrKnockedOut() || !hittable.Evaluate(info.caster, info.target))
		{
			if (_ticksDone >= _maxTicks)
			{
				Destroy();
				return;
			}
			List<Entity> list = DewPhysics.OverlapCircleAllEntities(out var handle, info.caster.agentPosition, targetRadius, tvDefaultHarmfulEffectTargets);
			if (list.Count <= 0)
			{
				handle.Return();
				Destroy();
				return;
			}
			float target = (((Object)(object)info.target != null) ? CastInfo.GetAngle(info.target.position - info.caster.position) : info.caster.rotation.eulerAngles.y);
			Entity entity = null;
			float num = float.NegativeInfinity;
			for (int i = 0; i < list.Count; i++)
			{
				float num2 = 0f - Mathf.Abs(Mathf.DeltaAngle(CastInfo.GetAngle(list[i].position - info.caster.position), target)) + ((info.caster.GetRelation(list[i]) == EntityRelation.Ally) ? (-1000f) : 0f);
				if (!(num > num2))
				{
					num = num2;
					entity = list[i];
				}
			}
			if ((Object)(object)entity == null)
			{
				handle.Return();
				Destroy();
				return;
			}
			CastInfo castInfo = info;
			castInfo.target = entity;
			info = castInfo;
			handle.Return();
			FxStopNetworked(targetEffect);
			FxPlayNetworked(targetEffect, info.target);
		}
		Damage(tickDamage, procCoefficient).SetElemental(ElementalType.Light).SetOriginPosition(info.caster.position).SetAttr(DamageAttribute.DamageOverTime)
			.Dispatch(info.target);
		List<Entity> list2 = DewPhysics.OverlapCircleAllEntities(out var handle2, info.caster.agentPosition, 15f, tvDefaultUsefulEffectTargets, new CollisionCheckSettings
		{
			includeUncollidable = true,
			sortComparer = CollisionCheckSettings.Random
		});
		Entity entity2 = info.caster;
		foreach (Entity item in list2)
		{
			if (!item.IsNullInactiveDeadOrKnockedOut() && !(item.normalizedHealth - 0.0001f >= entity2.normalizedHealth))
			{
				entity2 = item;
			}
		}
		if (!entity2.IsNullInactiveDeadOrKnockedOut())
		{
			Heal(tickHeal).Dispatch(entity2);
			FxPlayNewNetworked(healEffect, entity2);
		}
		handle2.Return();
		if (_ticksDone >= _maxTicks)
		{
			Destroy();
		}
		else if (Vector3.Distance(info.caster.position, info.target.position) > maxDistance)
		{
			Destroy();
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && _beamChannel != null)
		{
			_beamChannel.Cancel();
			_beamChannel = null;
		}
		if (((NetworkBehaviour)this).isServer)
		{
			FxStopNetworked(beamEffect);
			FxStopNetworked(targetEffect);
			FxStopNetworked(handEffect);
		}
		if (lineRenderer != null)
		{
			lineRenderer.enabled = false;
		}
		if (((NetworkBehaviour)this).isServer && (Object)(object)info.caster != null)
		{
			info.caster.Animation.StopAbilityAnimation(canceledClip);
			if (!_slow.IsNullOrInactive())
			{
				_slow.Get().Destroy();
			}
		}
		if (((NetworkBehaviour)this).isServer && (Object)(object)_firstTrigger != null)
		{
			_firstTrigger.fillAmount = 0f;
		}
	}

	private void MirrorProcessed()
	{
	}
}
