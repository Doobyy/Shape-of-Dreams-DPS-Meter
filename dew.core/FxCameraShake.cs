using System;
using System.Collections;
using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class FxCameraShake : MonoBehaviour, IEffectComponent, IEffectWithOwnerContext
{
	public const float Global_AmplitudeMultiplier = 1.25f;

	public const float Global_FrequencyMultiplier = 0.9f;

	public const float Global_TimeMultiplier = 1.25f;

	public SignalSourceAsset signalSource;

	public float delay;

	public float amplitude = 3f;

	public float frequency = 1f;

	public float attackTime;

	public float sustainTime = 0.1f;

	public float decayTime = 0.5f;

	[SerializeField]
	[HideInInspector]
	private CinemachineImpulseSource _impulse;

	private EffectOwnerContext _context;

	public bool isPlaying { get; private set; }

	internal void CreateImpulseSource()
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		if (!((UnityEngine.Object)(object)_impulse != null))
		{
			_impulse = gameObject.AddComponent<CinemachineImpulseSource>();
			_impulse.m_ImpulseDefinition.m_ImpulseType = (ImpulseTypes)3;
			_impulse.m_ImpulseDefinition.m_RawSignal = signalSource;
			_impulse.m_ImpulseDefinition.m_PropagationSpeed = float.PositiveInfinity;
		}
	}

	public void Play()
	{
		if ((UnityEngine.Object)(object)_impulse == null)
		{
			CreateImpulseSource();
		}
		if (delay > 0.0001f)
		{
			StartCoroutine(DelayedRoutine());
		}
		else
		{
			DoShake();
		}
		IEnumerator DelayedRoutine()
		{
			isPlaying = true;
			yield return new WaitForSeconds(delay);
			isPlaying = false;
			DoShake();
		}
	}

	private void DoShake()
	{
		CinemachineImpulseDefinition impulseDefinition = _impulse.m_ImpulseDefinition;
		impulseDefinition.m_AmplitudeGain = amplitude * 1.25f;
		impulseDefinition.m_FrequencyGain = frequency * 0.9f;
		impulseDefinition.m_TimeEnvelope.m_AttackTime = attackTime * 1.25f;
		impulseDefinition.m_TimeEnvelope.m_SustainTime = sustainTime * 1.25f;
		impulseDefinition.m_TimeEnvelope.m_DecayTime = decayTime * 1.25f;
		switch (_context)
		{
		case EffectOwnerContext.OtherPlayers:
			impulseDefinition.m_AmplitudeGain = Mathf.Max(0f, (impulseDefinition.m_AmplitudeGain - 0.6f) * 0.25f);
			impulseDefinition.m_TimeEnvelope.m_AttackTime *= 0.5f;
			impulseDefinition.m_TimeEnvelope.m_SustainTime *= 0.5f;
			impulseDefinition.m_TimeEnvelope.m_DecayTime *= 0.5f;
			break;
		default:
			throw new ArgumentOutOfRangeException();
		case EffectOwnerContext.None:
		case EffectOwnerContext.Self:
		case EffectOwnerContext.Boss:
		case EffectOwnerContext.Others:
			break;
		}
		if (DewInput.currentMode == InputMode.Gamepad && Gamepad.current != null)
		{
			ManagerBase<InputManager>.instance.AddShakeInstance(new ShakeInstance
			{
				position = transform.position,
				frequency = impulseDefinition.m_FrequencyGain,
				amplitude = impulseDefinition.m_AmplitudeGain,
				attackTime = impulseDefinition.m_TimeEnvelope.m_AttackTime,
				sustainTime = impulseDefinition.m_TimeEnvelope.m_SustainTime,
				decayTime = impulseDefinition.m_TimeEnvelope.m_DecayTime,
				startTime = Time.time
			});
		}
		impulseDefinition.m_AmplitudeGain = Mathf.Max(impulseDefinition.m_AmplitudeGain - ShaderManager._tooMuchShakeScore, impulseDefinition.m_AmplitudeGain * 0.3f);
		ShaderManager._tooMuchShakeScore += impulseDefinition.m_AmplitudeGain * (impulseDefinition.m_TimeEnvelope.m_AttackTime * 0.5f + impulseDefinition.m_TimeEnvelope.m_DecayTime * 0.5f + impulseDefinition.m_TimeEnvelope.m_SustainTime);
		if (ManagerBase<CameraManager>.softInstance == null || !ManagerBase<CameraManager>.softInstance.isPlayingCutscene)
		{
			impulseDefinition.m_AmplitudeGain *= DewSave.profileMain.gameplay.screenShakeStrength;
		}
		_impulse.GenerateImpulse();
	}

	public void Stop()
	{
		StopAllCoroutines();
		isPlaying = false;
	}

	public static void CancelAllShakes()
	{
		CinemachineImpulseManager.Instance.Clear();
		ShaderManager._tooMuchShakeScore = 0f;
		if (ManagerBase<InputManager>.softInstance != null)
		{
			ManagerBase<InputManager>.softInstance.ClearShakeInstances();
		}
	}

	public void SetOwnerContext(EffectOwnerContext context)
	{
		_context = context;
	}
}
