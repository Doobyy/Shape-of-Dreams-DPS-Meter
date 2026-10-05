using System.Collections;
using Mirror;
using UnityEngine;

public class Ai_Mon_Ink_BossDarkMoon_ShortDash : AbilityInstance
{
	public float startDelay;

	public float minDistance;

	public float backDistance;

	public float sideDistance;

	public float curveSize;

	public float speed;

	public float castDuration;

	public float rotDuration;

	public float rageBackDistance;

	public float rageRotSmoothTime;

	public float ragePostDelay;

	public GameObject fxWeapon;

	public GameObject fxTelegraph;

	public GameObject fxDashEffect;

	public DewAnimationClip readyClip;

	public DewAnimationClip endClip;

	private Vector4 _rightTurn;

	private Vector4 _leftTurn;

	private Vector4 _turnDir;

	private Vector3 _targetPos;

	private float _dashDuration;

	private bool _isRage;

	private float _maxDistance;

	private float _baseRotSmoothTime;

	private float _baseCastDuration;

	private float _baseCurveSize;

	protected override void Awake()
	{
		base.Awake();
		_baseCastDuration = castDuration;
		_baseCurveSize = curveSize;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		castDuration = _baseCastDuration;
		curveSize = _baseCurveSize;
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		Vector3 pos;
		Quaternion rot;
		if (((NetworkBehaviour)this).isServer)
		{
			DestroyOnDeath(info.caster);
			CreateBasicEffect(info.caster, new UnstoppableEffect(), float.PositiveInfinity).DestroyOnDestroy(this);
			_baseRotSmoothTime = info.caster.Control.rotationSmoothTime;
			Mon_Ink_BossDarkMoon mon_Ink_BossDarkMoon = (Mon_Ink_BossDarkMoon)info.caster;
			_isRage = mon_Ink_BossDarkMoon._isRage || mon_Ink_BossDarkMoon._isSolo;
			_rightTurn = new Vector4(0.75f, 0.23f, 0.75f, 0.23f);
			_leftTurn = new Vector4(0.23f, 0.75f, 0.23f, 0.75f);
			_maxDistance = castDuration * speed;
			if (_isRage)
			{
				castDuration -= castDuration / 4f;
				_maxDistance = castDuration * speed;
				info.caster.Control.rotationSmoothTime = rageRotSmoothTime;
				info.caster.Animation.PlayAbilityAnimation(readyClip);
				FxPlayNetworked(fxWeapon, info.caster);
				FxApplySpeedMultiplierNetworked(fxTelegraph, 1f / (castDuration + startDelay));
			}
			info.caster.Control.StartChannel(new Channel
			{
				blockedActions = Channel.BlockedAction.Everything,
				duration = startDelay + castDuration,
				isAttack = false,
				onCancel = DestroyIfActive
			});
			CalculateDestination();
			pos = AbilityTrigger.PredictPoint_Simple(info.caster, 1f, info.target, castDuration);
			rot = Quaternion.LookRotation(pos - _targetPos);
			if (_isRage)
			{
				FxPlayNetworked(fxTelegraph, _targetPos, rot);
				info.caster.Control.Rotate(pos - _targetPos, immediately: false, castDuration);
			}
			else
			{
				info.caster.Control.RotateTowards(info.target, immediately: false, _dashDuration);
			}
			info.caster.Control.StartDisplacement(new DispByDestination
			{
				affectedByMovementSpeed = true,
				canGoOverTerrain = true,
				destination = _targetPos,
				duration = _dashDuration,
				ease = DewEase.EaseOutQuint,
				isCanceledByCC = false,
				isFriendly = true,
				curve = _turnDir,
				curveHorizontalDistance = curveSize,
				onCancel = DestroyIfActive,
				onFinish = () =>
				{
					((MonoBehaviour)(object)this).StartCoroutine(Routine());
				},
				rotateForward = false
			});
		}
		IEnumerator Routine()
		{
			float seconds = castDuration - _dashDuration;
			yield return new WaitForSeconds(seconds);
			info.caster.Animation.StopAbilityAnimation();
			if (_isRage)
			{
				FxPlayNetworked(fxDashEffect, _targetPos, rot);
				info.caster.Animation.PlayAbilityAnimation(endClip);
				CreateAbilityInstance<Ai_Mon_Ink_BossDarkMoon_ShortDash_Atk>(info.caster.position, null, new CastInfo(info.caster, CastInfo.GetAngle(pos - _targetPos)));
				info.caster.Control.StartDaze(ragePostDelay);
				yield return new WaitForSeconds(ragePostDelay);
				DestroyIfActive();
			}
			DestroyIfActive();
		}
	}

	private void CalculateDestination()
	{
		if (_isRage)
		{
			Vector3 vector = AbilityTrigger.PredictPoint_Simple(info.caster, 1f, info.target, castDuration);
			Vector3 vector2 = ((Random.value >= 0.5f) ? vector : info.target.GetAIPosition(info.caster));
			Vector3 normalized = (vector2 - info.caster.position).normalized;
			float num = Vector3.Distance(info.caster.position, vector2);
			num = Mathf.Clamp(num + rageBackDistance, minDistance, _maxDistance);
			Vector3 end = info.caster.position + normalized * num;
			end = Dew.GetValidAgentDestination_LinearSweep(info.caster.position, end);
			num = Vector3.Distance(info.caster.position, end);
			float dashDuration = num / speed;
			float t = Mathf.InverseLerp(minDistance, _maxDistance, num);
			float num2 = Mathf.Lerp(0.2f, 1f, t);
			curveSize *= num2;
			_dashDuration = dashDuration;
			_targetPos = end;
			_turnDir = ((Random.value >= 0.5f) ? _leftTurn : _rightTurn);
			return;
		}
		Vector3 normalized2 = (info.caster.position - info.target.GetAIPosition(info.caster)).normalized;
		Vector3 normalized3 = Vector3.Cross(normalized2, Vector3.up).normalized;
		Vector3 end2 = info.caster.position + normalized2 * backDistance;
		end2 = Dew.GetValidAgentDestination_LinearSweep(info.caster.position, end2);
		float num3 = Vector3.Distance(info.caster.position, end2);
		if (backDistance > num3)
		{
			end2 = info.caster.position + -normalized2 * backDistance;
			end2 = Dew.GetValidAgentDestination_LinearSweep(info.caster.position, end2);
		}
		float num4 = Vector3.Dot(((Component)(object)info.caster).transform.right, normalized2);
		if (num4 <= 0f)
		{
			_turnDir = _rightTurn;
			Vector3 end3 = end2 + normalized3 * sideDistance;
			end3 = Dew.GetValidAgentDestination_Closest(info.caster.position, end3);
			if (Vector3.Distance(info.caster.position, end3) <= minDistance || Random.value <= 0.3f)
			{
				_turnDir = _leftTurn;
				end3 = end2 + -normalized3 * sideDistance;
				end3 = Dew.GetValidAgentDestination_Closest(info.caster.position, end3);
			}
			_targetPos = end3;
		}
		else if (num4 > 0f)
		{
			_turnDir = _leftTurn;
			Vector3 end4 = end2 + -normalized3 * sideDistance;
			end4 = Dew.GetValidAgentDestination_Closest(info.caster.position, end4);
			if (Vector3.Distance(info.caster.position, end4) <= minDistance || Random.value <= 0.3f)
			{
				_turnDir = _rightTurn;
				end4 = end2 + normalized3 * sideDistance;
				end4 = Dew.GetValidAgentDestination_Closest(info.caster.position, end4);
			}
			_targetPos = end4;
		}
		if (Vector3.Distance(info.caster.position, _targetPos) <= minDistance)
		{
			_targetPos = info.caster.position + (_targetPos - info.caster.position).normalized * minDistance;
			_targetPos = Dew.GetValidAgentDestination_LinearSweep(info.caster.position, _targetPos);
		}
		float value = Vector3.Distance(info.caster.position, _targetPos);
		float t2 = Mathf.InverseLerp(minDistance, _maxDistance, value);
		float num5 = Mathf.Lerp(0.2f, 1f, t2);
		curveSize *= num5;
		_dashDuration = Vector3.Distance(info.caster.position, _targetPos) / speed;
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			info.caster.Control.rotationSmoothTime = _baseRotSmoothTime;
			FxStopNetworked(fxWeapon);
		}
	}

	private void MirrorProcessed()
	{
	}
}
