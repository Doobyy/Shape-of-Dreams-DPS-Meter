using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : ManagerBase<AudioManager>
{
	private const int CriticalPriority = 0;

	private const int DynamicPriorityLowerBound = 2;

	private const int DynamicPriorityUpperBound = 255;

	private const int GameEnvironmentPriority = 256;

	private const int CramNumOfPriorityUsed = 10;

	private const float CrammedAudioSourcesRatio = 0.25f;

	private const int NonAdvancedPriotizationPriority = 1;

	public AudioMixer mixer;

	public AudioMixerGroup master;

	public AudioMixerGroup music;

	public AudioMixerGroup ui;

	public AudioMixerGroup sfx;

	public AudioMixerGroup gameEnvironment;

	public AudioMixerGroup gameSelf;

	public AudioMixerGroup gameBoss;

	public AudioMixerGroup gameOtherPlayers;

	public AudioMixerGroup gameOthers;

	public AnimationCurve defaultCurve;

	public AnimationCurve loudCurve;

	private readonly AudioSource[] _sources = new AudioSource[244];

	private int _nextIndex;

	private Dictionary<AudioClip, AudioSource> _uiSoundPlayers = new Dictionary<AudioClip, AudioSource>();

	public override bool shouldRegisterUpdates => false;

	private void Start()
	{
		UpdateMixerAttenuations();
	}

	private void OnApplicationFocus(bool hasFocus)
	{
		UpdateMixerAttenuations();
	}

	public void UpdateMixerAttenuations()
	{
		bool isFocused = Application.isFocused;
		BackgroundAudioBehavior backgroundAudioBehavior = DewSave.profileMain.audio.backgroundAudioBehavior;
		mixer.SetFloat("MasterVolume", GetAttenuation((backgroundAudioBehavior == BackgroundAudioBehavior.Mute && !isFocused) ? 0f : DewSave.profileMain.audio.masterVolume));
		mixer.SetFloat("MusicVolume", GetAttenuation((backgroundAudioBehavior == BackgroundAudioBehavior.MuteMusic && !isFocused) ? 0f : DewSave.profileMain.audio.musicVolume));
		mixer.SetFloat("SFXVolume", GetAttenuation(DewSave.profileMain.audio.sfxVolume));
		mixer.SetFloat("UIVolume", GetAttenuation(DewSave.profileMain.audio.uiVolume));
		mixer.SetFloat("EnvVolume", GetAttenuation(DewSave.profileMain.audio.envVolume));
	}

	public void SetMasterVolumeMultiplier(float multiplier)
	{
		mixer.SetFloat("MasterVolume", GetAttenuation(DewSave.profileMain.audio.masterVolume * multiplier));
	}

	public void FadeInMasterVolume()
	{
		StopAllCoroutines();
		StartCoroutine(MasterVolumeFadeRoutine(0f, 1f, 1f));
	}

	public void FadeOutMasterVolume()
	{
		StopAllCoroutines();
		StartCoroutine(MasterVolumeFadeRoutine(1f, 0f, 1f));
	}

	private IEnumerator MasterVolumeFadeRoutine(float from, float to, float duration)
	{
		for (float t = 0f; t <= duration; t += Time.unscaledDeltaTime)
		{
			float t2 = t / duration;
			SetMasterVolumeMultiplier(Mathf.Lerp(from, to, t2));
			yield return null;
		}
		SetMasterVolumeMultiplier(to);
	}

	private float GetAttenuation(float volume)
	{
		volume = Mathf.Clamp(volume, 0.0001f, 1f);
		return Mathf.Log10(volume) * 20f;
	}

	public static bool NeedsSetupEverytime(AudioType type)
	{
		switch (type)
		{
		case AudioType.UI:
		case AudioType.Music:
		case AudioType.SFX:
		case AudioType.GameEnvironment:
			return false;
		case AudioType.GameBoss:
		case AudioType.GameSelf:
		case AudioType.GameOthers:
		case AudioType.GameOtherPlayers:
			return true;
		default:
			throw new ArgumentOutOfRangeException("type", type, null);
		}
	}

	public void SetupAudioSource(AudioSource source, AudioType type, AudioSpaceType space, DewAudioRollOffType rolloff, bool forceCriticalPriority)
	{
		switch (space)
		{
		case AudioSpaceType.Global:
			source.spatialBlend = 0f;
			break;
		case AudioSpaceType.Normal:
			source.spatialBlend = 1f;
			break;
		}
		switch (type)
		{
		case AudioType.UI:
			source.priority = 0;
			source.outputAudioMixerGroup = ui;
			source.bypassReverbZones = true;
			break;
		case AudioType.Music:
			source.priority = 0;
			source.outputAudioMixerGroup = music;
			source.bypassReverbZones = true;
			break;
		case AudioType.SFX:
			source.priority = 0;
			source.outputAudioMixerGroup = sfx;
			break;
		case AudioType.GameBoss:
			source.priority = ((!forceCriticalPriority) ? GetNextDynamicPriority(source) : 0);
			source.outputAudioMixerGroup = gameBoss;
			break;
		case AudioType.GameSelf:
			source.priority = ((!forceCriticalPriority) ? GetNextDynamicPriority(source) : 0);
			source.outputAudioMixerGroup = gameSelf;
			break;
		case AudioType.GameOtherPlayers:
			source.priority = ((!forceCriticalPriority) ? GetNextDynamicPriority(source) : 0);
			source.outputAudioMixerGroup = gameOtherPlayers;
			break;
		case AudioType.GameOthers:
			source.priority = ((!forceCriticalPriority) ? GetNextDynamicPriority(source) : 0);
			source.outputAudioMixerGroup = gameOthers;
			break;
		case AudioType.GameEnvironment:
			source.priority = 256;
			source.outputAudioMixerGroup = gameEnvironment;
			break;
		default:
			throw new ArgumentOutOfRangeException("type", type, null);
		}
		switch (rolloff)
		{
		case DewAudioRollOffType.Default:
			source.rolloffMode = (AudioRolloffMode)2;
			source.SetCustomCurve((AudioSourceCurveType)0, defaultCurve);
			source.maxDistance = 55f;
			break;
		case DewAudioRollOffType.Loud:
			source.rolloffMode = (AudioRolloffMode)2;
			source.SetCustomCurve((AudioSourceCurveType)0, loudCurve);
			source.maxDistance = 80f;
			break;
		case DewAudioRollOffType.Unity:
			source.rolloffMode = (AudioRolloffMode)0;
			source.minDistance = 25f;
			source.maxDistance = 100f;
			break;
		default:
			throw new ArgumentOutOfRangeException("rolloff", rolloff, null);
		}
	}

	private int GetNextDynamicPriority(AudioSource source)
	{
		if (!DewSave.platformSettings.audio.useAdvancedPrioritization)
		{
			return 1;
		}
		if (_nextIndex >= _sources.Length)
		{
			int num = Mathf.RoundToInt((float)_sources.Length * 0.25f);
			for (int i = 0; i < _sources.Length - num; i++)
			{
				if (!((UnityEngine.Object)(object)_sources[i] == null))
				{
					_sources[i].priority = 255;
				}
			}
			for (int j = 0; j < num; j++)
			{
				if (!((UnityEngine.Object)(object)_sources[_sources.Length - num + j] == null))
				{
					float num2 = (float)j / (float)(num - 1);
					_sources[_sources.Length - num + j].priority = 255 - Mathf.RoundToInt(8f * num2) - 1;
				}
			}
			_sources[0] = source;
			_nextIndex = 1;
			return 265;
		}
		int nextIndex = _nextIndex;
		_nextIndex++;
		_sources[nextIndex] = source;
		return 255 - nextIndex + 10;
	}

	public void PlayUISound(AudioClip clip)
	{
		if (!_uiSoundPlayers.ContainsKey(clip))
		{
			AudioSource val = gameObject.AddComponent<AudioSource>();
			SetupAudioSource(val, AudioType.UI, AudioSpaceType.Global, DewAudioRollOffType.Default, forceCriticalPriority: false);
			val.clip = clip;
			_uiSoundPlayers[clip] = val;
		}
		_uiSoundPlayers[clip].Stop();
		_uiSoundPlayers[clip].Play();
	}
}
