using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TransitionManager : ManagerBase<TransitionManager>
{
	public enum StateType
	{
		Normal,
		Loading
	}

	public SafeAction onStateChanged;

	public Action onFadeIn;

	public Action onFadeOut;

	public float fadeTime;

	public CanvasGroup screenFadeCg;

	public GameObject busyObject;

	public GameObject fxTransition;

	public TextMeshProUGUI tipText;

	public TextMeshProUGUI[] loadingTexts;

	public float newTipCooldownTime = 1f;

	public CanvasGroup whiteFadeCg;

	private float _lastTooltipChangeTime = float.NegativeInfinity;

	private readonly List<string> _remainingTooltips = new List<string>();

	private readonly List<string> _remainingHeroTooltips = new List<string>();

	private string _currentHeroType;

	private bool _isInSceneTransition;

	public StateType state { get; private set; }

	private void Start()
	{
		((Behaviour)(object)tipText).enabled = false;
		screenFadeCg.alpha = 1f;
		((Component)(object)screenFadeCg).gameObject.SetActive(value: true);
		busyObject.SetActive(value: false);
		TextMeshProUGUI[] array = loadingTexts;
		for (int i = 0; i < array.Length; i++)
		{
			((TMP_Text)array[i]).text = DewLocalization.GetUIValue("Loading");
		}
	}

	public void FadeOut(bool showTips)
	{
		if (showTips)
		{
			((Behaviour)(object)tipText).enabled = true;
			TryGetNewTip();
		}
		else
		{
			((Behaviour)(object)tipText).enabled = false;
		}
		if (state == StateType.Loading)
		{
			return;
		}
		state = StateType.Loading;
		onStateChanged?.Invoke();
		fxTransition.gameObject.SetActive(value: true);
		DewEffect.Play(fxTransition);
		TextMeshProUGUI[] array = loadingTexts;
		for (int i = 0; i < array.Length; i++)
		{
			((TMP_Text)array[i]).text = DewLocalization.GetUIValue("Loading");
		}
		try
		{
			onFadeOut?.Invoke();
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	public IEnumerator FadeOutRoutine(bool showTips)
	{
		FadeOut(showTips);
		yield return new WaitForSecondsRealtime(fadeTime);
	}

	public void PlayGame(DewNetworkStartSettings settings)
	{
		DewNetworkManager.startSettings = settings;
		LoadScene("PlayLobby", settings.continueData != null);
	}

	public void LoadScene(string sceneName, bool dontFadeIn = false)
	{
		if (!_isInSceneTransition)
		{
			StartCoroutine(Routine());
		}
		IEnumerator Routine()
		{
			_isInSceneTransition = true;
			FadeOut(showTips: false);
			yield return new WaitForSecondsRealtime(fadeTime);
			SceneManager.LoadScene(sceneName);
			if (!dontFadeIn)
			{
				FadeIn();
			}
			_isInSceneTransition = false;
		}
	}

	private void TryGetNewTip()
	{
		if (!(Time.unscaledTime - _lastTooltipChangeTime < newTipCooldownTime))
		{
			_lastTooltipChangeTime = Time.unscaledTime;
			if ((UnityEngine.Object)(object)DewPlayer.local != null && UnityEngine.Random.value < 0.3f)
			{
				TryGetNewTip_Hero();
			}
			else
			{
				TryGetNewTip_General();
			}
		}
	}

	private void TryGetNewTip_Hero()
	{
		if (DewPlayer.local.selectedHeroType != _currentHeroType || _remainingHeroTooltips.Count == 0)
		{
			_currentHeroType = DewPlayer.local.selectedHeroType;
			_remainingHeroTooltips.Clear();
			_remainingHeroTooltips.AddRange(DewLocalization.data.tips.Keys.Where((string k) => k.StartsWith(_currentHeroType)));
		}
		if (_remainingHeroTooltips.Count == 0)
		{
			TryGetNewTip_General();
			return;
		}
		int index = UnityEngine.Random.Range(0, _remainingHeroTooltips.Count);
		string key = _remainingHeroTooltips[index];
		_remainingHeroTooltips.RemoveAt(index);
		((TMP_Text)tipText).text = DewLocalization.data.tips[key];
	}

	private void TryGetNewTip_General()
	{
		if (_remainingTooltips.Count == 0)
		{
			_remainingTooltips.AddRange(DewLocalization.data.tips.Keys.Where((string k) => !k.StartsWith("Hero_")));
		}
		int index = UnityEngine.Random.Range(0, _remainingTooltips.Count);
		string key = _remainingTooltips[index];
		_remainingTooltips.RemoveAt(index);
		((TMP_Text)tipText).text = DewLocalization.data.tips[key];
	}

	public void FadeIn()
	{
		StartCoroutine(FadeInRoutine());
	}

	public void FadeOut()
	{
		FadeOut(showTips: false);
	}

	public IEnumerator FadeInRoutine()
	{
		if (state != StateType.Normal)
		{
			yield return null;
			yield return null;
			yield return null;
			try
			{
				onFadeIn?.Invoke();
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
			state = StateType.Normal;
			onStateChanged?.Invoke();
			yield return null;
			yield return null;
			yield return null;
			DewEffect.Stop(fxTransition);
			yield return new WaitForSecondsRealtime(fadeTime);
		}
	}

	public override void FrameUpdate()
	{
		base.FrameUpdate();
		screenFadeCg.alpha = Mathf.MoveTowards(screenFadeCg.alpha, (state == StateType.Loading) ? 1 : 0, Time.unscaledDeltaTime / fadeTime);
		screenFadeCg.blocksRaycasts = state == StateType.Loading;
		((Component)(object)screenFadeCg).gameObject.SetActive(screenFadeCg.alpha > 0.001f);
		if (state == StateType.Loading && (Input.GetKeyDown(KeyCode.Mouse0) || Input.GetKeyDown(KeyCode.Space)))
		{
			TryGetNewTip();
		}
	}

	public void UpdateLoadingStatus(LoadingStatus status)
	{
		if (status == LoadingStatus.Empty)
		{
			TextMeshProUGUI[] array = loadingTexts;
			for (int i = 0; i < array.Length; i++)
			{
				((TMP_Text)array[i]).text = "";
			}
		}
		else
		{
			TextMeshProUGUI[] array = loadingTexts;
			for (int i = 0; i < array.Length; i++)
			{
				((TMP_Text)array[i]).text = DewLocalization.GetUIValue("Loading_" + status);
			}
		}
	}

	public void UpdateLoadingStatus(string rawText)
	{
		TextMeshProUGUI[] array = loadingTexts;
		for (int i = 0; i < array.Length; i++)
		{
			((TMP_Text)array[i]).text = rawText;
		}
	}

	public void SetBusy(bool value)
	{
		busyObject.SetActive(value);
		if (!busyObject.activeSelf && state == StateType.Normal)
		{
			UpdateLoadingStatus("");
		}
	}

	public void ResetWhiteFade()
	{
		if (!((UnityEngine.Object)(object)whiteFadeCg == null))
		{
			ShortcutExtensions.DOKill((Component)(object)whiteFadeCg, false);
			whiteFadeCg.alpha = 0f;
			whiteFadeCg.blocksRaycasts = false;
		}
	}
}
