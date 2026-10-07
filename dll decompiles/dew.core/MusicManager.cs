using System;
using System.Collections;
using UnityEngine;

public class MusicManager : ManagerBase<MusicManager>
{
	public float fadeOutTime;

	public float volumeMultiplier;

	private AudioSource _sourceA;

	private AudioSource _sourceB;

	private AudioSource _activeSource;

	private AudioSource _fadingSource;

	private float _currentNormalizedVolume;

	[NonSerialized]
	public AssetRef<DewMusicItem> next;

	[NonSerialized]
	public AssetRef<DewMusicItem> current;

	private bool _isLoop = true;

	private bool _isCrossFading;

	private float _crossFadeDuration;

	private float _crossFadeTimer;

	private float _fadingAssetVolMult;

	public bool isPlaying => _activeSource.isPlaying;

	internal AudioSource _source => _activeSource;

	protected override void Awake()
	{
		base.Awake();
		_sourceA = GetComponent<AudioSource>();
		if ((UnityEngine.Object)(object)_sourceA == null)
		{
			_sourceA = gameObject.AddComponent<AudioSource>();
		}
		_sourceB = gameObject.AddComponent<AudioSource>();
		_sourceB.loop = true;
		_sourceB.bypassReverbZones = true;
		_activeSource = _sourceA;
		_fadingSource = _sourceB;
		DewResources.AddPreloadRule(this, (PreloadInterface preload) =>
		{
			preload.AddAssetRef(next);
			preload.AddAssetRef(current);
		});
	}

	private void Start()
	{
		ManagerBase<AudioManager>.instance.SetupAudioSource(_sourceA, AudioType.Music, AudioSpaceType.Global, DewAudioRollOffType.Unity, forceCriticalPriority: false);
		ManagerBase<AudioManager>.instance.SetupAudioSource(_sourceB, AudioType.Music, AudioSpaceType.Global, DewAudioRollOffType.Unity, forceCriticalPriority: false);
	}

	public void Play(DewMusicItem music, bool isLoop = true)
	{
		_isCrossFading = false;
		_isLoop = isLoop;
		next = music;
	}

	public void Stop()
	{
		_isCrossFading = false;
		next = null;
	}

	public void Pause()
	{
		_sourceA.Pause();
		_sourceB.Pause();
	}

	public void UnPause()
	{
		_sourceA.UnPause();
		_sourceB.UnPause();
	}

	public void DoCrossFade(DewMusicItem next, float fadeTime)
	{
		if (!(next == null) && !((UnityEngine.Object)(object)next.clip == null) && (!_isCrossFading || !(current != null) || !(current.asset == next)))
		{
			StopAllCoroutines();
			_fadingAssetVolMult = _currentNormalizedVolume * ((current == null) ? 1f : current.asset.volumeMultiplier);
			this.next = next;
			current = next;
			AudioSource fadingSource = _fadingSource;
			AudioSource activeSource = _activeSource;
			_activeSource = fadingSource;
			_fadingSource = activeSource;
			_activeSource.clip = next.clip;
			_activeSource.volume = 0f;
			_activeSource.pitch = next.pitch;
			if (_fadingSource.isPlaying)
			{
				_activeSource.time = _fadingSource.time;
			}
			else
			{
				_activeSource.time = next.startTime;
			}
			_activeSource.Play();
			_isCrossFading = true;
			_crossFadeDuration = ((fadeTime > 0f) ? fadeTime : 0.01f);
			_crossFadeTimer = 0f;
		}
	}

	public override void LogicUpdate(float dt)
	{
		base.LogicUpdate(dt);
		if (Time.timeScale < 0.0001f)
		{
			if (isPlaying)
			{
				Pause();
			}
		}
		else if (((UnityEngine.Object)(object)_activeSource.clip != null || (UnityEngine.Object)(object)_fadingSource.clip != null) && !isPlaying)
		{
			UnPause();
		}
		if (_isCrossFading)
		{
			float num = Mathf.Min(dt, 0.1f);
			_crossFadeTimer += num;
			float num2 = Mathf.Clamp01(_crossFadeTimer / _crossFadeDuration);
			float num3 = num2;
			float num4 = 1f - num2;
			_activeSource.volume = num3 * volumeMultiplier * ((current == null) ? 1f : current.asset.volumeMultiplier);
			_fadingSource.volume = num4 * volumeMultiplier * _fadingAssetVolMult;
			if (num2 >= 1f)
			{
				_isCrossFading = false;
				_fadingSource.Stop();
				_currentNormalizedVolume = 1f;
			}
			return;
		}
		if (current != next)
		{
			_currentNormalizedVolume = Mathf.MoveTowards(_currentNormalizedVolume, 0f, 1f / fadeOutTime * dt);
			if (_currentNormalizedVolume < 0.0001f)
			{
				_activeSource.Stop();
				_activeSource.clip = ((next == null) ? null : next.asset.clip);
				current = next;
				StopAllCoroutines();
				if (current != null)
				{
					StartCoroutine(DelayedPlay());
				}
			}
		}
		else if (isPlaying)
		{
			_currentNormalizedVolume = Mathf.MoveTowards(_currentNormalizedVolume, 1f, 1f / ((next == null) ? 1f : next.asset.fadeInTime) * dt);
		}
		float volume = _currentNormalizedVolume * volumeMultiplier * ((current == null) ? 1f : current.asset.volumeMultiplier);
		_activeSource.volume = volume;
		_activeSource.pitch = ((current != null) ? current.asset.pitch : 1f);
		IEnumerator DelayedPlay()
		{
			yield return new WaitForSeconds(current.asset.delay);
			_activeSource.loop = _isLoop;
			_activeSource.Play();
			_activeSource.time = current.asset.startTime;
		}
	}
}
