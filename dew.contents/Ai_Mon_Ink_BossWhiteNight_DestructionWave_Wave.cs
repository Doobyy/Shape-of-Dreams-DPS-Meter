using System;
using System.Collections;
using Mirror;
using UnityEngine;

public class Ai_Mon_Ink_BossWhiteNight_DestructionWave_Wave : AbilityInstance
{
	public class Ad_DestructionWave
	{
		public float time;
	}

	public float interval;

	public float postDelay;

	public float startDelay;

	public float rotationDelay;

	public float rotationDuration;

	public float maxRotationAngle;

	public DewCollider range;

	public ScalingValue dmgPerAtk;

	public ScalingValue firstDmg;

	public Knockback Knockback;

	public DewAnimationClip startClip;

	public DewAnimationClip loopClip;

	public DewAnimationClip endClip;

	public GameObject fxStart;

	public GameObject fxInstance;

	public GameObject fxHit;

	public GameObject fxTelegraph;

	public GameObject fxEnd;

	[NonSerialized]
	public bool _disableRotation;

	[Space(15f)]
	public float rageStartDelay;

	public float rageRotDuration;

	public GameObject fxRageInstance;

	[Space(10f)]
	public float rageThunderInterval;

	public float rageThunderRadius;

	public Vector2 rageThunderRangePerWave;

	private Channel _channel;

	private Quaternion _finalRot;

	private Quaternion _startRot;

	private float _rangeCheckTimer = float.PositiveInfinity;

	private float _angle;

	private float _elapsedTime;

	private bool _enableRot;

	private bool _enableAtk;

	private bool _isRage;

	private bool _thunderEnable = true;

	private float _rageTimer;

	private Coroutine _rageRoutine;

	private At_Mon_Ink_BossWhiteNight_DestructionWave _trigger;

	private float _pristineMaxRotationAngle;

	private float _pristineStartDelay;

	private float _pristineRotationDuration;

	private Vector3 _pristineRangeLocalScale;

	public override bool reuseInRoom => true;

	protected override void Awake()
	{
		base.Awake();
		_pristineMaxRotationAngle = maxRotationAngle;
		_pristineStartDelay = startDelay;
		_pristineRotationDuration = rotationDuration;
		if (range != null)
		{
			_pristineRangeLocalScale = range.transform.localScale;
		}
	}

	protected override IEnumerator OnCreateSequenced()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		DestroyOnDeath(info.caster);
		CreateBasicEffect(info.caster, new UnstoppableEffect(), float.PositiveInfinity).DestroyOnDestroy(this);
		_trigger = info.caster.Ability.GetAbility<At_Mon_Ink_BossWhiteNight_DestructionWave>();
		_elapsedTime = 0f;
		_channel = info.caster.Control.StartChannel(new Channel
		{
			duration = float.PositiveInfinity,
			blockedActions = Channel.BlockedAction.Everything,
			onCancel = DestroyIfActive
		});
		_isRage = ((Mon_Ink_BossWhiteNight)info.caster)._isRage;
		if (_isRage)
		{
			range.transform.localScale = new Vector3(1.5f, 1f, 2f);
			startDelay = rageStartDelay;
			rotationDuration = rageRotDuration;
			maxRotationAngle *= 2f;
			_rageRoutine = ((MonoBehaviour)(object)this).StartCoroutine(StartCreateRageInstance());
		}
		Hero closestAliveHero = Dew.GetClosestAliveHero(info.caster.position, fallbackToDead: true, info.caster);
		if ((UnityEngine.Object)(object)closestAliveHero != null && ((UnityEngine.Object)(object)_trigger == null || !_trigger.disableRotation))
		{
			if (Vector3.SignedAngle(((Component)(object)info.caster).transform.forward, (closestAliveHero.GetAIAgentPosition(info.caster) - info.caster.agentPosition).normalized, Vector3.up) >= 0f)
			{
				maxRotationAngle = 0f - maxRotationAngle;
				FxPlayNetworked(fxTelegraph, info.caster, info.caster.agentPosition, Quaternion.Euler(0f, 0f, -90f));
			}
			else
			{
				FxPlayNetworked(fxTelegraph, info.caster, info.caster.agentPosition, Quaternion.Euler(0f, 0f, 90f));
			}
			if (Mathf.Abs(maxRotationAngle) % 360f < 0.01f)
			{
				maxRotationAngle -= Mathf.Sign(maxRotationAngle) * 0.001f;
			}
		}
		yield return new SI.WaitForSeconds(startDelay);
		FxPlayNetworked(fxStart, info.caster);
		info.caster.Animation.PlayAbilityAnimation(startClip);
		yield return new SI.WaitForSeconds(0.1f);
		_enableAtk = true;
		if (_isRage)
		{
			FxPlayNetworked(fxRageInstance, info.caster);
		}
		else
		{
			FxPlayNetworked(fxInstance, info.caster);
		}
		info.caster.Animation.PlayAbilityAnimation(loopClip);
		yield return new SI.WaitForSeconds(rotationDelay);
		if ((UnityEngine.Object)(object)_trigger == null || !_trigger.disableRotation)
		{
			_startRot = info.caster.rotation;
			_finalRot = Quaternion.AngleAxis(maxRotationAngle, Vector3.up) * info.caster.rotation;
			_enableRot = true;
		}
		yield return new SI.WaitForSeconds(rotationDuration);
		_enableRot = false;
		_enableAtk = false;
		info.caster.Control.StopOverrideRotation();
		info.caster.Animation.StopAbilityAnimation();
		info.caster.Animation.PlayAbilityAnimation(endClip);
		info.caster.Control.StartDaze(postDelay);
		FxStopNetworked(fxInstance);
		FxStopNetworked(fxRageInstance);
		FxPlayNetworked(fxEnd, info.caster);
		yield return new SI.WaitForSeconds(postDelay);
		Destroy();
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		if (_enableAtk)
		{
			range.transform.position = info.caster.agentPosition;
			range.transform.rotation = info.caster.rotation;
			float num = 0f;
			ListReturnHandle<Entity> handle;
			foreach (Entity entity in range.GetEntities(out handle, tvDefaultHarmfulEffectTargets))
			{
				if (entity.IsNullInactiveDeadOrKnockedOut())
				{
					continue;
				}
				if (entity.TryGetData<Ad_DestructionWave>(out var data))
				{
					if (Time.time - data.time <= interval)
					{
						continue;
					}
					data.time = Time.time;
					num = GetValue(dmgPerAtk);
				}
				else
				{
					entity.AddData(new Ad_DestructionWave
					{
						time = Time.time
					});
					num = GetValue(firstDmg);
				}
				FxPlayNewNetworked(fxHit, entity);
				CreateDamage(DamageData.SourceType.Default, num).SetDirection(((Component)(object)info.caster).transform.forward).SetOriginPosition(info.caster.agentPosition).SetAttr(DamageAttribute.DamageOverTime)
					.SetElemental(ElementalType.Light)
					.Dispatch(entity);
				if (!entity.Status.hasCrowdControlImmunity)
				{
					Knockback.ApplyWithDirection(((Component)(object)info.caster).transform.forward, entity);
				}
			}
			handle.Return();
		}
		_rangeCheckTimer += dt;
		if (_enableRot)
		{
			float value = Mathf.Clamp01(_elapsedTime / rotationDuration);
			Quaternion quaternion = Quaternion.AngleAxis(Mathf.Lerp(0f, maxRotationAngle, EasingFunction.EaseInOutQuad(0f, 1f, value)), Vector3.up) * _startRot;
			_elapsedTime += dt;
			info.caster.Control.Rotate(quaternion, immediately: false);
		}
	}

	private IEnumerator StartCreateRageInstance()
	{
		while (((NetworkBehaviour)this).isServer && _isRage && _thunderEnable)
		{
			for (int i = 0; (float)i < UnityEngine.Random.Range(rageThunderRangePerWave.x, rageThunderRangePerWave.y); i++)
			{
				Vector3 point = info.caster.position + UnityEngine.Random.insideUnitCircle.ToXZ() * UnityEngine.Random.Range(0f, rageThunderRadius);
				CreateAbilityInstance<Ai_Mon_Ink_BossWhiteNight_RageInstance>(point, null, new CastInfo(info.caster, point));
			}
			yield return new WaitForSeconds(rageThunderInterval);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		if ((UnityEngine.Object)(object)_trigger != null)
		{
			_trigger.disableRotation = false;
		}
		foreach (Entity allEntity in NetworkedManagerBase<ActorManager>.instance.allEntities)
		{
			if (allEntity.HasData<Ad_DestructionWave>())
			{
				allEntity.RemoveData<Ad_DestructionWave>();
			}
		}
		FxStopNetworked(fxInstance);
		FxStopNetworked(fxRageInstance);
		if (_channel != null && _channel.isAlive)
		{
			_channel.Cancel();
			_channel = null;
		}
		if (_rageRoutine != null)
		{
			((MonoBehaviour)(object)this).StopCoroutine(_rageRoutine);
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		maxRotationAngle = _pristineMaxRotationAngle;
		startDelay = _pristineStartDelay;
		rotationDuration = _pristineRotationDuration;
		if (range != null)
		{
			range.transform.localScale = _pristineRangeLocalScale;
		}
		_enableAtk = false;
		_enableRot = false;
		_isRage = false;
		_elapsedTime = 0f;
		_rangeCheckTimer = float.PositiveInfinity;
		_rageRoutine = null;
		_channel = null;
	}

	private void MirrorProcessed()
	{
	}
}
