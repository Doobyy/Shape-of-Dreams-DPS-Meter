using System;
using UnityEngine;

public class DewAudioSource : MonoBehaviour, IEffectComponent, IEffectWithOwnerContext
{
	public const float PitchChangeByTimescaleMultiplier = 0.75f;

	public AudioType type = AudioType.SFX;

	public AudioSpaceType space;

	public DewAudioRollOffType rolloff;

	public DewAudioClip clip;

	public AudioClip rawClip;

	public bool isLoop;

	public bool stopWhenEffectStop;

	public bool forceHighestPriority;

	public float delay;

	public float fadeInTime;

	public float fadeOutTime = 0.1f;

	[Space(20f)]
	public float volumeMultiplier = 1f;

	public float pitchMultiplier = 1f;

	[Space(20f)]
	public bool playOnEnable;

	public bool ignoreTimescale;

	internal int _voiceIndex = -1;

	public bool isPlaying
	{
		get
		{
			DewAudioManager softInstance = ManagerBase<DewAudioManager>.softInstance;
			if (softInstance != null)
			{
				return softInstance.IsPlaying(this);
			}
			return false;
		}
	}

	bool IEffectComponent.isLooping => isLoop;

	private void OnEnable()
	{
		if (playOnEnable)
		{
			Play();
		}
	}

	private void OnDisable()
	{
		ManagerBase<DewAudioManager>.softInstance?.Release(this);
	}

	private void OnDestroy()
	{
		ManagerBase<DewAudioManager>.softInstance?.Release(this);
	}

	void IEffectComponent.Play()
	{
		Play();
	}

	void IEffectComponent.Stop()
	{
		if (stopWhenEffectStop || isLoop)
		{
			Stop();
		}
	}

	public void Play()
	{
		if (enabled && gameObject.activeInHierarchy)
		{
			if (clip == null && (UnityEngine.Object)(object)rawClip == null)
			{
				Debug.LogWarning("Clip not set in DewAudioSource '" + name + "'", this);
			}
			else
			{
				ManagerBase<DewAudioManager>.instance.Play(this);
			}
		}
	}

	public void Stop()
	{
		ManagerBase<DewAudioManager>.softInstance?.Stop(this);
	}

	public void SetOwnerContext(EffectOwnerContext context)
	{
		if (type != AudioType.UI && type != AudioType.Music)
		{
			switch (context)
			{
			case EffectOwnerContext.None:
				type = AudioType.GameOthers;
				break;
			case EffectOwnerContext.Self:
				type = AudioType.GameSelf;
				break;
			case EffectOwnerContext.Boss:
				type = AudioType.GameBoss;
				break;
			case EffectOwnerContext.OtherPlayers:
				type = AudioType.GameOtherPlayers;
				break;
			case EffectOwnerContext.Others:
				type = AudioType.GameOthers;
				break;
			default:
				throw new ArgumentOutOfRangeException("context", context, null);
			}
		}
	}
}
