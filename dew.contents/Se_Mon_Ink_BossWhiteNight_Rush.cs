using System.Collections;
using Mirror;
using UnityEngine;

public class Se_Mon_Ink_BossWhiteNight_Rush : StatusEffect
{
	public class Ad_Rush
	{
		public float time;
	}

	public float startDelay;

	public float postDelay;

	public float speedAmount;

	public float duration;

	public float dmgInterval;

	public float checkInterval;

	public float rotSmoothTime;

	public Knockback knockback;

	public ScalingValue dmgFactor;

	public DewCollider range;

	public GameObject fxRush;

	public GameObject fxHit;

	public AnimationClip walkAnimation;

	public float rageSlowRadius;

	public float rageSlowAmount;

	public float rageSlowDuration;

	public GameObject fxRage;

	private AnimationClip _originWalkAnimation;

	private bool _isAttacking = true;

	private float _timer;

	private bool _isRage;

	private float _speedAmountPristine;

	private float _baseRotSmoothTime;

	public override bool reuseInRoom => true;

	protected override void Awake()
	{
		base.Awake();
		_speedAmountPristine = speedAmount;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		speedAmount = _speedAmountPristine;
		_isAttacking = true;
		_timer = 0f;
		_isRage = false;
	}

	protected override IEnumerator OnCreateSequenced()
	{
		_originWalkAnimation = victim.Animation.model.runForwardClip;
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		DestroyOnDeath(info.caster);
		_baseRotSmoothTime = info.caster.Control.rotationSmoothTime;
		info.caster.Control.rotationSmoothTime = rotSmoothTime;
		CreateBasicEffect(victim, new UnstoppableEffect(), float.PositiveInfinity).DestroyOnDestroy(this);
		_isRage = ((Mon_Ink_BossWhiteNight)info.caster)._isRage;
		victim.Control.StartDaze(startDelay);
		yield return new SI.WaitForSeconds(startDelay);
		if (_isRage)
		{
			speedAmount *= 1.25f;
			FxPlayNetworked(fxRage, victim);
		}
		DoSpeed(speedAmount);
		float timer = 0f;
		FxPlayNetworked(fxRush, victim);
		victim.Control.StartChannel(new Channel
		{
			blockedActions = (Channel.BlockedAction.Ability | Channel.BlockedAction.Attack),
			duration = duration,
			onCancel = () =>
			{
				FxStopNetworked(fxRush);
				DestroyIfActive();
			},
			onTick = (float t) =>
			{
				range.transform.position = victim.agentPosition;
				range.transform.rotation = ((Component)(object)victim).transform.rotation;
				timer += t;
				if (!(timer <= checkInterval))
				{
					timer = 0f;
					ListReturnHandle<Entity> handle;
					foreach (Entity entity in range.GetEntities(out handle, tvDefaultHarmfulEffectTargets))
					{
						if (entity.TryGetData<Ad_Rush>(out var data))
						{
							if (Time.time - data.time < dmgInterval)
							{
								continue;
							}
							data.time = Time.time;
						}
						else
						{
							entity.AddData(new Ad_Rush
							{
								time = Time.time
							});
						}
						Vector3 normalized = (entity.agentPosition - victim.agentPosition).normalized;
						CreateDamage(DamageData.SourceType.Default, dmgFactor).SetOriginPosition(victim.agentPosition).SetDirection(normalized).Dispatch(entity);
						knockback.ApplyWithDirection(normalized, entity);
						FxPlayNewNetworked(fxHit, entity);
					}
					handle.Return();
				}
			},
			onComplete = () =>
			{
				_isAttacking = false;
				FxStopNetworked(fxRush);
				DestroyIfActive();
			}
		});
		yield return new SI.WaitForSeconds(duration);
		victim.Control.Stop();
		victim.Control.StartDaze(postDelay);
		yield return new SI.WaitForSeconds(postDelay);
		DestroyIfActive();
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (!((NetworkBehaviour)this).isServer || !_isAttacking)
		{
			return;
		}
		_timer += dt;
		Hero closestAliveHero = Dew.GetClosestAliveHero(victim.agentPosition, fallbackToDead: true, victim);
		if ((Object)(object)closestAliveHero == null || _timer <= 0.45f)
		{
			return;
		}
		if (_isRage)
		{
			ListReturnHandle<Entity> handle;
			foreach (Entity item in DewPhysics.OverlapCircleAllEntities(out handle, victim.agentPosition, rageSlowRadius, tvDefaultHarmfulEffectTargets))
			{
				CreateBasicEffect(item, new SlowEffect
				{
					decay = false,
					strength = rageSlowAmount
				}, rageSlowDuration, "slow_whitenightrush");
			}
			handle.Return();
		}
		_timer = 0f;
		victim.Control.MoveToDestination(Dew.GetValidAgentDestination_LinearSweep(victim.agentPosition, closestAliveHero.GetAIAgentPosition(victim)), immediately: true);
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		victim.Animation.ReplaceAnimationLocal(EntityAnimation.ReplaceableAnimationType.RunForward, _originWalkAnimation);
		if (((NetworkBehaviour)this).isServer)
		{
			FxStopNetworked(fxRush);
			FxStopNetworked(fxRage);
			info.caster.Control.rotationSmoothTime = _baseRotSmoothTime;
		}
	}

	private void MirrorProcessed()
	{
	}
}
