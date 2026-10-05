using Mirror;
using UnityEngine;

public class Se_M_Charge : StatusEffect
{
	public float speed = 12f;

	public DewEase ease;

	public float minDistance = 2f;

	public float animSpeedMultiplier = 1f;

	public bool rotateForward;

	public DewAnimationClip startAnim;

	public DewAnimationClip endAnim;

	public DewCollider knockbackRange;

	public float knockbackDamageHpRatio;

	public Vector3 colCheckOffset;

	public float colCheckRadius;

	public Knockback knockback;

	public GameObject hitEffect;

	public GameObject hitOnVictimEffect;

	public float knockupAmount;

	public float postDelay;

	public bool resetAtk;

	public float gainedShieldMaxHpRatio = 0.1f;

	public float shieldDuration = 1.5f;

	private bool _didCollide;

	private Vector3 _dir;

	private Quaternion _dirRot;

	private float _baseGainedShieldMaxHpRatio;

	private float _baseShieldDuration;

	public override bool reuseInRoom => true;

	protected override void Awake()
	{
		base.Awake();
		_baseGainedShieldMaxHpRatio = gainedShieldMaxHpRatio;
		_baseShieldDuration = shieldDuration;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		gainedShieldMaxHpRatio = _baseGainedShieldMaxHpRatio;
		shieldDuration = _baseShieldDuration;
		_didCollide = false;
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		_dir = (info.point - info.caster.agentPosition).Flattened().normalized;
		if (_dir.sqrMagnitude < 0.001f)
		{
			_dir = ((Component)(object)info.caster).transform.forward;
		}
		_dirRot = Quaternion.LookRotation(_dir);
		DoUncollidable();
		GiveShield(info.caster, gainedShieldMaxHpRatio * info.caster.maxHealth, shieldDuration);
		CreateBasicEffect(info.caster, new UncollidableEffect(), 0.3f, "ChargeMinUncollidable", DuplicateEffectBehavior.UsePrevious);
		Vector3 validAgentDestination_LinearSweep = Dew.GetValidAgentDestination_LinearSweep(info.caster.agentPosition, info.point);
		Vector3 vector = validAgentDestination_LinearSweep - info.caster.agentPosition;
		float magnitude = vector.magnitude;
		if (magnitude < minDistance)
		{
			vector = vector.normalized * minDistance;
			validAgentDestination_LinearSweep = info.caster.agentPosition + vector;
			validAgentDestination_LinearSweep = Dew.GetValidAgentDestination_LinearSweep(info.caster.agentPosition, validAgentDestination_LinearSweep);
			magnitude = minDistance;
		}
		float num = magnitude / speed;
		info.caster.Control.StartDisplacement(new DispByDestination
		{
			destination = validAgentDestination_LinearSweep,
			duration = num,
			ease = ease,
			isFriendly = true,
			isDodging = true,
			onCancel = () =>
			{
				if (isActive)
				{
					Destroy();
				}
			},
			onFinish = () =>
			{
				if (isActive)
				{
					Destroy();
				}
			},
			rotateForward = rotateForward,
			canGoOverTerrain = false,
			isCanceledByCC = false
		});
		if (!Ai_GenericDodge.ShouldSkipAnimation(info.caster))
		{
			info.caster.Animation.PlayAbilityAnimation(startAnim, animSpeedMultiplier * startAnim.entries[0].duration / num);
		}
		((Component)(object)this).transform.rotation = Quaternion.LookRotation(validAgentDestination_LinearSweep - info.caster.agentPosition).Flattened();
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (((NetworkBehaviour)this).isServer && !_didCollide)
		{
			DoCollisionCheck();
		}
	}

	private void DoCollisionCheck()
	{
		Vector3 center = info.caster.agentPosition + _dirRot * colCheckOffset;
		if (DewPhysics.OverlapCircleAllEntities(out var handle, center, colCheckRadius, tvDefaultHarmfulEffectTargets, new CollisionCheckSettings
		{
			sortComparer = CollisionCheckSettings.Random
		}).Count > 0)
		{
			DoCollision();
		}
		handle.Return();
	}

	private void DoCollision()
	{
		_didCollide = true;
		knockbackRange.transform.SetPositionAndRotation(info.caster.position, _dirRot);
		FxPlayNewNetworked(hitEffect, info.caster.position, info.caster.rotation);
		ListReturnHandle<Entity> handle;
		foreach (Entity entity in knockbackRange.GetEntities(out handle, tvDefaultHarmfulEffectTargets))
		{
			knockback.ApplyWithDirection(_dirRot, entity);
			DefaultDamage(knockbackDamageHpRatio * info.caster.maxHealth).SetDirection(info.caster.rotation).Dispatch(entity);
			FxPlayNewNetworked(hitOnVictimEffect, entity);
			entity.Visual.KnockUp(knockupAmount, isFriendly: false);
		}
		handle.Return();
		info.caster.Control.CancelOngoingDisplacement();
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && !info.caster.IsNullOrInactive())
		{
			if (!_didCollide)
			{
				DoCollisionCheck();
			}
			info.caster.Animation.StopAbilityAnimation(startAnim);
			if (!Ai_GenericDodge.ShouldSkipAnimation(info.caster))
			{
				info.caster.Animation.PlayAbilityAnimation(endAnim);
			}
			if (postDelay > 0f)
			{
				info.caster.Control.StartDaze(postDelay);
			}
			if (resetAtk)
			{
				ResetCooldown(info.caster.Ability.attackAbility);
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
