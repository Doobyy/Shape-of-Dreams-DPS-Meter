using Cinemachine;
using UnityEngine;

public class OverrideCinemachineImpulseByDewSettings : MonoBehaviour, ISettingsChangedCallback
{
	private CinemachineImpulseSource _source;

	private float _initAmplitudeGain;

	private void Awake()
	{
		_source = GetComponent<CinemachineImpulseSource>();
		if ((bool)(Object)(object)_source)
		{
			_initAmplitudeGain = _source.m_ImpulseDefinition.m_AmplitudeGain;
			CinemachineImpulseDefinition impulseDefinition = _source.m_ImpulseDefinition;
			impulseDefinition.m_AmplitudeGain *= DewSave.profileMain.gameplay.screenShakeStrength;
		}
	}

	public void OnSettingsChanged()
	{
		if ((bool)(Object)(object)_source)
		{
			_source.m_ImpulseDefinition.m_AmplitudeGain = _initAmplitudeGain;
			CinemachineImpulseDefinition impulseDefinition = _source.m_ImpulseDefinition;
			impulseDefinition.m_AmplitudeGain *= DewSave.profileMain.gameplay.screenShakeStrength;
		}
	}
}
