using System;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class UI_CreditsView : View
{
	public SafeAction onEnd;

	public CanvasGroup fadeCg;

	public float fadeTransitionTime = 3f;

	public float initDelay = 1f;

	public float duration = 3f;

	public float endDelay = 3f;

	public RectTransform dolly;

	public Transform endCenterTarget;

	public TextMeshProUGUI profileNameText;

	public Volume startBrightnessVolume;

	[Header("Starless Path Settings")]
	public bool forceStarlessPath;

	public DewMusicItem inGameMusic;

	public DewMusicItem starlessPathMusic;

	public GameObject[] starlessPathObjects;

	public GameObject[] normalObjects;

	public Material[] starlessPathMaterials;

	public Material[] normalMaterials;

	public Color[] starlessPathColors;

	public Color[] normalColors;

	private Vector2? _originalAnchorPos;

	private Vector3 _startPos;

	private float _elapsedTime;

	private bool _needToInvokeEnd;

	private float _ffSpeed;

	protected override void OnShow()
	{
		base.OnShow();
		if (!_originalAnchorPos.HasValue)
		{
			_originalAnchorPos = Vector2.zero;
		}
		dolly.anchoredPosition = _originalAnchorPos.Value;
		fadeCg.alpha = 1f;
		_elapsedTime = 0f;
		_startPos = dolly.position;
		_startPos.z = 0f;
		((TMP_Text)profileNameText).text = DewSave.profileMain.name.Trim();
		startBrightnessVolume.weight = 1f;
		_needToInvokeEnd = true;
		bool flag = (bool)(UnityEngine.Object)(object)NetworkedManagerBase<GameResultManager>.instance && NetworkedManagerBase<GameResultManager>.instance.current.result == DewGameResult.ResultType.StarlessPath;
		flag |= forceStarlessPath;
		DewMusicItem dewMusicItem = (flag ? starlessPathMusic : inGameMusic);
		if ((bool)dewMusicItem)
		{
			ManagerBase<MusicManager>.instance.Play(dewMusicItem, isLoop: false);
		}
		UI_CreditsView_ImageEffect[] componentsInChildren = GetComponentsInChildren<UI_CreditsView_ImageEffect>();
		UI_CreditsView_ImageEffect[] array = componentsInChildren;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].RevertMaterial();
		}
		starlessPathObjects.SetActiveAll(flag);
		normalObjects.SetActiveAll(!flag);
		UI_CreditsView_NameSpawner[] componentsInChildren2 = GetComponentsInChildren<UI_CreditsView_NameSpawner>();
		for (int i = 0; i < componentsInChildren2.Length; i++)
		{
			componentsInChildren2[i].UpdateNames();
		}
		if (starlessPathMaterials.Length != normalMaterials.Length)
		{
			throw new InvalidOperationException();
		}
		if (starlessPathColors.Length != normalColors.Length)
		{
			throw new InvalidOperationException();
		}
		Image[] componentsInChildren3 = GetComponentsInChildren<Image>();
		foreach (Image val in componentsInChildren3)
		{
			for (int j = 0; j < starlessPathMaterials.Length; j++)
			{
				Material material = (flag ? normalMaterials[j] : starlessPathMaterials[j]);
				Material material2 = (flag ? starlessPathMaterials[j] : normalMaterials[j]);
				if (((Graphic)val).material == material)
				{
					((Graphic)val).material = material2;
				}
			}
			for (int k = 0; k < starlessPathColors.Length; k++)
			{
				Color b = (flag ? normalColors[k] : starlessPathColors[k]);
				Color color = (flag ? starlessPathColors[k] : normalColors[k]);
				if (IsEqualApproximately(((Graphic)val).color, b))
				{
					((Graphic)val).color = color;
				}
			}
		}
		TextMeshProUGUI[] componentsInChildren4 = GetComponentsInChildren<TextMeshProUGUI>();
		foreach (TextMeshProUGUI val2 in componentsInChildren4)
		{
			for (int l = 0; l < starlessPathMaterials.Length; l++)
			{
				Material material3 = (flag ? normalMaterials[l] : starlessPathMaterials[l]);
				Material material4 = (flag ? starlessPathMaterials[l] : normalMaterials[l]);
				if (((Graphic)val2).material == material3)
				{
					((Graphic)val2).material = material4;
				}
			}
			for (int m = 0; m < starlessPathColors.Length; m++)
			{
				Color b2 = (flag ? normalColors[m] : starlessPathColors[m]);
				Color color2 = (flag ? starlessPathColors[m] : normalColors[m]);
				if (IsEqualApproximately(((Graphic)val2).color, b2))
				{
					((Graphic)val2).color = color2;
				}
			}
		}
		array = componentsInChildren;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].CreateMaterial();
		}
	}

	private bool IsEqualApproximately(Color a, Color b)
	{
		float num = 0.001f;
		if (Mathf.Abs(a.r - b.r) < num && Mathf.Abs(a.g - b.g) < num && Mathf.Abs(a.b - b.b) < num)
		{
			return Mathf.Abs(a.a - b.a) < num;
		}
		return false;
	}

	private Vector3 GetEndPos()
	{
		return _startPos.WithY(dolly.position.y - endCenterTarget.position.y + (float)Screen.height * 0.5f);
	}

	protected override void Update()
	{
		base.Update();
		if (!Application.IsPlaying(this) || !isShowing)
		{
			return;
		}
		bool flag = DewInput.GetButton(MouseButton.Left, checkGameArea: false) || DewInput.GetButton(DewSave.profileMain.controls.skip, checkGameAreaForMouse: false);
		if (!flag)
		{
			_ffSpeed = 4f;
		}
		else
		{
			_ffSpeed += Time.deltaTime * 2f;
		}
		_elapsedTime += (flag ? (Time.unscaledDeltaTime * _ffSpeed) : Time.unscaledDeltaTime);
		float elapsedTime = _elapsedTime;
		if (elapsedTime < fadeTransitionTime)
		{
			fadeCg.alpha = 1f - elapsedTime / fadeTransitionTime;
			startBrightnessVolume.weight = 1f;
			dolly.position = _startPos.Quantitized();
			return;
		}
		fadeCg.alpha = 0f;
		elapsedTime -= fadeTransitionTime;
		if (elapsedTime < initDelay)
		{
			return;
		}
		elapsedTime -= initDelay;
		startBrightnessVolume.weight = Mathf.Clamp01(1f - elapsedTime / 3f);
		float num = elapsedTime / duration;
		if (num < 1f)
		{
			dolly.position = Vector3.Lerp(_startPos, GetEndPos(), num).Quantitized();
			return;
		}
		elapsedTime -= duration;
		dolly.position = GetEndPos();
		if (elapsedTime < endDelay)
		{
			return;
		}
		elapsedTime -= endDelay;
		if (elapsedTime < fadeTransitionTime)
		{
			fadeCg.alpha = elapsedTime / fadeTransitionTime;
			return;
		}
		if (_needToInvokeEnd)
		{
			_needToInvokeEnd = false;
			onEnd?.Invoke();
			if (ManagerBase<TitleManager>.softInstance != null)
			{
				ManagerBase<UIManager>.instance.SetState("Title");
			}
			else if ((UnityEngine.Object)(object)NetworkedManagerBase<GameManager>.softInstance != null)
			{
				ManagerBase<UIManager>.instance.SetState("Result");
			}
		}
		fadeCg.alpha = 1f;
	}

	public void Skip()
	{
		_elapsedTime = Mathf.Max(_elapsedTime, fadeTransitionTime + initDelay + duration + endDelay + fadeTransitionTime);
	}
}
