using System.Collections.Generic;
using UnityEngine;

public class DewAudioManager : ManagerBase<DewAudioManager>
{
	private sealed class Voice
	{
		public AudioSource source;

		public Transform sourceTransform;

		public DewAudioSource owner;

		public Transform ownerTransform;

		public Vector3 lastSyncedPos;

		public bool hasSyncedPos;

		public bool isEmitting;

		public bool needsToPlay;

		public bool isLoop;

		public bool is3D;

		public bool ignoreTimescale;

		public AudioType type;

		public float normalizedFade;

		public float timeStopPitch;

		public float emitStartTime;

		public float clipRandomPitch;

		public float desiredDelay;

		public float lastVolume;

		public float lastPitch;

		public bool didSetup;

		public AudioType lastSetupType;

		public AudioSpaceType lastSetupSpace;

		public DewAudioRollOffType lastSetupRolloff;

		public bool lastSetupForce;
	}

	public const int MaxPoolSize = 64;

	private readonly List<Voice> _voices = new List<Voice>(64);

	private readonly Stack<int> _free = new Stack<int>(64);

	private readonly List<int> _active = new List<int>(64);

	public override bool shouldRegisterUpdates => false;

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void Bootstrap()
	{
		if (!(ManagerBase<DewAudioManager>.softInstance != null))
		{
			GameObject gameObject = new GameObject("DewAudioManager");
			Object.DontDestroyOnLoad(gameObject);
			gameObject.AddComponent<DewAudioManager>();
		}
	}

	public void Play(DewAudioSource src)
	{
		AudioClip val = ((src.clip != null) ? src.clip.GetAudioClip() : src.rawClip);
		if ((Object)(object)val == null)
		{
			return;
		}
		int voiceIndex = src._voiceIndex;
		Voice voice;
		if (voiceIndex >= 0)
		{
			voice = _voices[voiceIndex];
		}
		else
		{
			voiceIndex = AcquireIndex();
			if (voiceIndex < 0)
			{
				return;
			}
			voice = _voices[voiceIndex];
			voice.owner = src;
			src._voiceIndex = voiceIndex;
			_active.Add(voiceIndex);
		}
		voice.ownerTransform = src.transform;
		voice.isLoop = src.isLoop;
		voice.is3D = src.space == AudioSpaceType.Normal;
		voice.type = src.type;
		voice.ignoreTimescale = src.ignoreTimescale;
		voice.source.loop = src.isLoop;
		voice.source.clip = val;
		if (!voice.didSetup || AudioManager.NeedsSetupEverytime(src.type) || voice.lastSetupType != src.type || voice.lastSetupSpace != src.space || voice.lastSetupRolloff != src.rolloff || voice.lastSetupForce != src.forceHighestPriority)
		{
			ManagerBase<AudioManager>.instance.SetupAudioSource(voice.source, src.type, src.space, src.rolloff, src.forceHighestPriority);
			voice.didSetup = true;
			voice.lastSetupType = src.type;
			voice.lastSetupSpace = src.space;
			voice.lastSetupRolloff = src.rolloff;
			voice.lastSetupForce = src.forceHighestPriority;
		}
		if (voice.is3D)
		{
			Vector3 position = voice.ownerTransform.position;
			voice.sourceTransform.position = position;
			voice.lastSyncedPos = position;
			voice.hasSyncedPos = true;
		}
		voice.isEmitting = true;
		voice.needsToPlay = true;
		voice.normalizedFade = 0f;
		voice.timeStopPitch = 1f;
		voice.emitStartTime = (src.ignoreTimescale ? Time.unscaledTime : Time.time);
		voice.clipRandomPitch = ((src.clip != null) ? src.clip.GetPitch() : 1f);
		voice.desiredDelay = src.delay - ((src.clip != null) ? src.clip.timeOffset : 0f);
		voice.lastVolume = float.NaN;
		voice.lastPitch = float.NaN;
		voice.source.Stop();
		TickVoice(voice, 0f);
	}

	public void Stop(DewAudioSource src)
	{
		int voiceIndex = src._voiceIndex;
		if (voiceIndex >= 0)
		{
			Voice voice = _voices[voiceIndex];
			voice.isEmitting = false;
			voice.needsToPlay = false;
			TickVoice(voice, 0f);
		}
	}

	public void Release(DewAudioSource src)
	{
		int voiceIndex = src._voiceIndex;
		if (voiceIndex >= 0)
		{
			Voice voice = _voices[voiceIndex];
			voice.source.Stop();
			voice.source.clip = null;
			voice.owner = null;
			voice.ownerTransform = null;
			src._voiceIndex = -1;
			int num = _active.IndexOf(voiceIndex);
			if (num >= 0)
			{
				_active.RemoveAt(num);
			}
			_free.Push(voiceIndex);
		}
	}

	public bool IsPlaying(DewAudioSource src)
	{
		int voiceIndex = src._voiceIndex;
		if (voiceIndex < 0)
		{
			return false;
		}
		Voice voice = _voices[voiceIndex];
		if (!voice.source.isPlaying)
		{
			if (voice.isEmitting)
			{
				return voice.needsToPlay;
			}
			return false;
		}
		return true;
	}

	private int AcquireIndex()
	{
		if (_free.Count > 0)
		{
			return _free.Pop();
		}
		if (_voices.Count < 64)
		{
			GameObject gameObject = new GameObject("PooledAudioSource");
			gameObject.transform.SetParent(transform, worldPositionStays: false);
			AudioSource val = gameObject.AddComponent<AudioSource>();
			val.playOnAwake = false;
			_voices.Add(new Voice
			{
				source = val,
				sourceTransform = gameObject.transform
			});
			return _voices.Count - 1;
		}
		return StealIndex();
	}

	private int StealIndex()
	{
		int num = -1;
		int num2 = int.MinValue;
		for (int i = 0; i < _active.Count; i++)
		{
			Voice voice = _voices[_active[i]];
			if (!voice.isLoop)
			{
				int priority = voice.source.priority;
				if (priority > 0 && priority > num2)
				{
					num2 = priority;
					num = i;
				}
			}
		}
		if (num < 0)
		{
			return -1;
		}
		int num3 = _active[num];
		Voice voice2 = _voices[num3];
		voice2.source.Stop();
		voice2.source.clip = null;
		if (voice2.owner != null)
		{
			voice2.owner._voiceIndex = -1;
		}
		voice2.owner = null;
		voice2.ownerTransform = null;
		_active.RemoveAt(num);
		return num3;
	}

	private void ReleaseActiveAt(int activeListIndex, int voiceIndex)
	{
		Voice voice = _voices[voiceIndex];
		voice.source.Stop();
		voice.source.clip = null;
		if (voice.owner != null)
		{
			voice.owner._voiceIndex = -1;
		}
		voice.owner = null;
		voice.ownerTransform = null;
		_active.RemoveAt(activeListIndex);
		_free.Push(voiceIndex);
	}

	private bool TickVoice(Voice v, float dt)
	{
		DewAudioSource owner = v.owner;
		if (owner == null)
		{
			return true;
		}
		float num = (v.ignoreTimescale ? Time.unscaledTime : Time.time);
		if (v.isEmitting || v.needsToPlay)
		{
			if (v.normalizedFade < 1f && num - v.emitStartTime >= v.desiredDelay)
			{
				if (owner.fadeInTime <= 0f)
				{
					v.normalizedFade = 1f;
				}
				else
				{
					v.normalizedFade = Mathf.MoveTowards(v.normalizedFade, 1f, dt / owner.fadeInTime);
				}
			}
		}
		else if (v.normalizedFade > 0f)
		{
			if (owner.fadeOutTime <= 0f)
			{
				v.normalizedFade = 0f;
			}
			else
			{
				v.normalizedFade = Mathf.MoveTowards(v.normalizedFade, 0f, dt / owner.fadeOutTime);
			}
		}
		bool flag = !v.needsToPlay && v.source.isPlaying;
		if (v.normalizedFade > 0f && !flag && v.needsToPlay)
		{
			v.source.Play();
			v.needsToPlay = false;
			if (v.desiredDelay < 0f)
			{
				v.source.time = 0f - v.desiredDelay;
			}
			flag = true;
		}
		if ((v.normalizedFade <= 0f) & flag)
		{
			v.source.Stop();
			flag = false;
		}
		if (!flag)
		{
			if (!v.needsToPlay)
			{
				if (v.isEmitting && !v.isLoop)
				{
					v.isEmitting = false;
				}
				if (!v.isEmitting)
				{
					return true;
				}
			}
			return false;
		}
		if (v.is3D && v.ownerTransform != null)
		{
			Vector3 position = v.ownerTransform.position;
			if (!v.hasSyncedPos || position != v.lastSyncedPos)
			{
				v.sourceTransform.position = position;
				v.lastSyncedPos = position;
				v.hasSyncedPos = true;
			}
		}
		if (v.type != AudioType.UI)
		{
			TimescaleManager timescaleManager = NetworkedManagerBase<TimescaleManager>.softInstance;
			if ((Object)(object)timescaleManager != null)
			{
				float num2 = timescaleManager.effectTimescale * 0.75f + 0.25f;
				float num3 = ((v.timeStopPitch < num2) ? 0.75f : 8f);
				v.timeStopPitch = Mathf.MoveTowards(v.timeStopPitch, num2, num3 * dt);
			}
		}
		float num4 = ((owner.clip != null) ? (owner.clip.volume * owner.volumeMultiplier) : owner.volumeMultiplier) * v.normalizedFade;
		if (num4 != v.lastVolume)
		{
			v.source.volume = num4;
			v.lastVolume = num4;
		}
		float num5 = v.clipRandomPitch * owner.pitchMultiplier * v.timeStopPitch;
		if (num5 != v.lastPitch)
		{
			v.source.pitch = num5;
			v.lastPitch = num5;
		}
		return false;
	}

	private void Update()
	{
		if (_active.Count == 0)
		{
			return;
		}
		float deltaTime = Time.deltaTime;
		float unscaledDeltaTime = Time.unscaledDeltaTime;
		for (int num = _active.Count - 1; num >= 0; num--)
		{
			int num2 = _active[num];
			Voice voice = _voices[num2];
			float dt = (voice.ignoreTimescale ? unscaledDeltaTime : deltaTime);
			if (TickVoice(voice, dt))
			{
				ReleaseActiveAt(num, num2);
			}
		}
	}
}
