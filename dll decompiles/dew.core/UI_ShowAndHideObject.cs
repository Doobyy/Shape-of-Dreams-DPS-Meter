using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class UI_ShowAndHideObject : MonoBehaviour
{
	private const float ShowDuration = 0.25f;

	private const float HideDuration = 0.25f;

	private CanvasGroup _cg;

	private bool _defaultBlocksRaycast;

	private Vector3? _defaultScale;

	private bool _isShown;

	public CanvasGroup cg
	{
		get
		{
			if ((UnityEngine.Object)(object)_cg == null)
			{
				_cg = GetComponent<CanvasGroup>();
				_defaultBlocksRaycast = _cg.blocksRaycasts;
			}
			return _cg;
		}
	}

	protected virtual void OnEnable()
	{
		_isShown = true;
	}

	public void Show()
	{
		if (_isShown)
		{
			return;
		}
		if (!_defaultScale.HasValue)
		{
			_defaultScale = transform.localScale;
		}
		_isShown = true;
		gameObject.SetActive(value: true);
		cg.alpha = 0f;
		cg.blocksRaycasts = _defaultBlocksRaycast;
		transform.localScale = _defaultScale.Value * 0.7f;
		ShortcutExtensions.DOKill((Component)transform, true);
		TweenSettingsExtensions.SetUpdate<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOScale(transform, _defaultScale.Value, 0.25f), true);
		ShortcutExtensions.DOKill((Component)(object)cg, false);
		TweenSettingsExtensions.SetUpdate<TweenerCore<float, float, FloatOptions>>(DOTweenModuleUI.DOFade(cg, 1f, 0.25f), true);
		try
		{
			OnShow();
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	public void Hide()
	{
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Expected Obj, but got Unknown
		if (!_isShown)
		{
			return;
		}
		if (!_defaultScale.HasValue)
		{
			_defaultScale = transform.localScale;
		}
		_isShown = false;
		cg.alpha = 1f;
		cg.blocksRaycasts = false;
		transform.localScale = _defaultScale.Value;
		ShortcutExtensions.DOKill((Component)transform, true);
		TweenSettingsExtensions.SetUpdate<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOScale(transform, _defaultScale.Value * 0.75f, 0.25f), true);
		TweenSettingsExtensions.SetUpdate<Sequence>(TweenSettingsExtensions.AppendCallback(TweenSettingsExtensions.Append(TweenSettingsExtensions.SetId<Sequence>(DOTween.Sequence(), (object)cg), (Tween)(object)DOTweenModuleUI.DOFade(cg, 0f, 0.25f)), (TweenCallback)(() =>
		{
			gameObject.SetActive(value: false);
		})), true);
		try
		{
			OnHide();
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	protected virtual void OnShow()
	{
	}

	protected virtual void OnHide()
	{
	}
}
