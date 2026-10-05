using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Ai_Mon_Forest_Hound_Charge : AbilityInstance
{
	public float duration;

	public float afterLandDelay = 0.5f;

	public float distance;

	public DewCollider range;

	public AbilityTargetValidator hittable;

	public bool unstoppableWhileCharging;

	public Knockback knockback;

	public ScalingValue dmgFactor;

	public GameObject chargeEffect;

	public EntityAnimation animation;

	public DewAnimationClip clip;

	public float animSpeed;

	public GameObject hitEffect;

	private ActorRef<StatusEffect> _unstoppable;

	private List<Entity> entsHit = new List<Entity>();

	private float _baseDuration;

	private bool _cachedBaseDuration;

	public override bool reuseInRoom => true;

	protected override void Awake()
	{
		base.Awake();
		if (!_cachedBaseDuration)
		{
			_baseDuration = duration;
			_cachedBaseDuration = true;
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		entsHit.Clear();
		_unstoppable = null;
		duration = _baseDuration;
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		animation = info.caster.Animation;
		animation.PlayAbilityAnimation(clip, animSpeed);
		if (unstoppableWhileCharging)
		{
			_unstoppable = CreateBasicEffect(info.caster, new UnstoppableEffect(), float.PositiveInfinity);
		}
		FxPlayNetworked(chargeEffect, info.caster);
		Vector3 destination = info.caster.position + info.forward * distance;
		info.caster.Visual.KnockUp(0.8f, isFriendly: true);
		info.caster.Control.StartDisplacement(new DispByDestination
		{
			affectedByMovementSpeed = true,
			destination = destination,
			duration = duration,
			ease = DewEase.Linear,
			isFriendly = true,
			onFinish = () =>
			{
				info.caster.Control.StartDaze(afterLandDelay);
				Destroy();
			},
			onCancel = () =>
			{
				if (isActive)
				{
					info.caster.Control.StartDaze(afterLandDelay);
					Destroy();
				}
			},
			rotateForward = true,
			canGoOverTerrain = false,
			isCanceledByCC = true
		});
		DestroyOnDeath(info.caster);
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		range.transform.position = info.caster.position;
		range.transform.rotation = info.caster.rotation;
		List<Entity> entities = range.GetEntities(out var handle, hittable, info.caster);
		for (int i = 0; i < entities.Count; i++)
		{
			bool flag = true;
			Entity entity = entities[i];
			for (int j = 0; j < entsHit.Count; j++)
			{
				if ((Object)(object)entity == (Object)(object)entsHit[j])
				{
					flag = false;
				}
			}
			if (flag)
			{
				OnHit(entity);
				entsHit.Add(entity);
			}
		}
		handle.Return();
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			animation.StopAbilityAnimation();
			FxStopNetworked(chargeEffect);
			if (!_unstoppable.IsNullOrInactive())
			{
				_unstoppable.Get().Destroy();
			}
		}
	}

	private void OnHit(Entity entity)
	{
		CreateDamage(DamageData.SourceType.Default, dmgFactor).Dispatch(entity);
		knockback.ApplyWithOrigin(info.caster.position, entity);
		FxPlayNewNetworked(hitEffect, entity);
	}

	private void MirrorProcessed()
	{
	}
}
