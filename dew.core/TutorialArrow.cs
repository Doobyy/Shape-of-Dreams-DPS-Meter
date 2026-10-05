using System;
using DewInternal;
using DG.Tweening;
using Sirenix.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

[LogicUpdatePriority(3000)]
public class TutorialArrow : LogicBehaviour
{
	private class RefText
	{
		public string text;
	}

	public TextMeshProUGUI textUGUI;

	public float minTime = 3f;

	public float maxTime = 30f;

	public Func<bool> destroyCondition;

	public Action onDestroy;

	public Transform starTransform;

	public Transform boxTransform;

	public UILineRenderer lineRenderer;

	public float boxNormalizedDistance;

	public float boxSmoothTime;

	public float disappearTime = 1.25f;

	public Vector3 boxPunch;

	public float boxPunchDuration;

	private ArrowFollowMode _followMode;

	private Transform _followTarget;

	private Func<Vector3> _followFunc;

	private bool _isDestroyConditionMet;

	private float _creationTime;

	private Vector2[] _lineRendererPoints = new Vector2[2];

	private Vector3 _boxCv;

	private CanvasGroup _cg;

	private BoxPlacementMode _boxMode;

	private Vector2 _boxOffset;

	private Vector2 _boxOffsetCv;

	private Vector3? _customOffset;

	public float elapsedTime => Time.time - _creationTime;

	private void Awake()
	{
		_creationTime = Time.time;
		_cg = GetComponent<CanvasGroup>();
	}

	private void Start()
	{
		UpdatePosition(init: true);
		ShortcutExtensions.DOPunchScale(boxTransform, boxPunch, boxPunchDuration, 10, 1f);
	}

	public override void LogicUpdate(float dt)
	{
		base.LogicUpdate(dt);
		if (!_isDestroyConditionMet && destroyCondition != null && destroyCondition())
		{
			_isDestroyConditionMet = true;
		}
		if ((_isDestroyConditionMet && elapsedTime > minTime) || elapsedTime > maxTime)
		{
			_cg.alpha = Mathf.MoveTowards(_cg.alpha, 0f, dt / disappearTime);
			if (_cg.alpha <= 0.001f)
			{
				UnityEngine.Object.Destroy(gameObject);
			}
		}
	}

	public override void FrameUpdate()
	{
		base.FrameUpdate();
		if (Time.timeScale > 0.0001f)
		{
			UpdatePosition(init: false);
		}
	}

	private void UpdatePosition(bool init)
	{
		switch (_followMode)
		{
		case ArrowFollowMode.None:
			return;
		case ArrowFollowMode.World:
			try
			{
				Vector3 position2;
				if (_followTarget != null)
				{
					position2 = _followTarget.position;
				}
				else
				{
					if (_followFunc == null)
					{
						return;
					}
					position2 = _followFunc();
				}
				starTransform.position = Dew.mainCamera.WorldToScreenPoint(position2);
			}
			catch (Exception)
			{
			}
			break;
		case ArrowFollowMode.Screen:
			try
			{
				Vector3 position;
				if (_followTarget != null)
				{
					position = _followTarget.position;
				}
				else
				{
					if (_followFunc == null)
					{
						return;
					}
					position = _followFunc();
				}
				starTransform.position = position;
			}
			catch (Exception)
			{
			}
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		Vector3 position3 = starTransform.position;
		position3.x /= Screen.width;
		position3.y /= Screen.height;
		Vector2 vector;
		if (_customOffset.HasValue)
		{
			vector = _customOffset.Value;
		}
		else
		{
			switch (_boxMode)
			{
			case BoxPlacementMode.TowardsCenter:
				vector = (Vector2.one * 0.5f - (Vector2)position3).normalized * boxNormalizedDistance;
				break;
			case BoxPlacementMode.AwayFromCenter:
				vector = (Vector2.one * 0.5f - (Vector2)position3).normalized * boxNormalizedDistance;
				vector *= -1f;
				break;
			case BoxPlacementMode.Below:
				vector = Vector2.down * 0.05f;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
		}
		Vector2 vector2 = (Vector2)position3 + vector;
		vector2.x *= Screen.width;
		vector2.y *= Screen.height;
		Vector2 vector3 = Vector2.zero;
		if (ManagerBase<GlobalUIManager>.instance.isTooltipShown)
		{
			Rect screenSpaceRect = ((RectTransform)boxTransform).GetScreenSpaceRect();
			Rect rect = RectExtensions.Expand(ManagerBase<GlobalUIManager>.instance.tooltipScreenSpaceRect, 20f);
			if (rect.Contains(vector2 - screenSpaceRect.size * 0.5f) || rect.Contains(vector2) || rect.Contains(vector2 + screenSpaceRect.size * 0.5f) || rect.Contains(vector2 + new Vector2(0f - screenSpaceRect.size.x, screenSpaceRect.size.y) * 0.5f) || rect.Contains(vector2 + new Vector2(screenSpaceRect.size.x, 0f - screenSpaceRect.size.y) * 0.5f))
			{
				vector3 = Vector2.up * (rect.yMax + screenSpaceRect.size.y * 0.5f - vector2.y);
			}
		}
		if (init)
		{
			_boxOffset = vector3;
		}
		else
		{
			_boxOffset = Vector2.SmoothDamp(_boxOffset, vector3, ref _boxOffsetCv, (_boxOffset.magnitude > vector3.magnitude) ? 0.3f : 0.03f);
		}
		vector2 += _boxOffset;
		if (init)
		{
			boxTransform.position = vector2;
		}
		else
		{
			boxTransform.position = Vector3.SmoothDamp(boxTransform.position, vector2, ref _boxCv, boxSmoothTime);
		}
		Vector2 vector4 = transform.InverseTransformPoint(starTransform.position);
		Vector2 vector5 = transform.InverseTransformPoint(boxTransform.position);
		bool num = _lineRendererPoints[0] != vector4 || _lineRendererPoints[1] != vector5;
		_lineRendererPoints[0] = vector4;
		_lineRendererPoints[1] = vector5;
		lineRenderer.Points = _lineRendererPoints;
		if (num)
		{
			((Graphic)lineRenderer).SetVerticesDirty();
		}
	}

	private void OnDestroy()
	{
		try
		{
			onDestroy?.Invoke();
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	public TutorialArrow SetDuration(float duration)
	{
		minTime = 0f;
		maxTime = duration;
		return this;
	}

	public TutorialArrow SetDuration(float minTime, float maxTime)
	{
		this.minTime = minTime;
		this.maxTime = maxTime;
		return this;
	}

	public TutorialArrow SetRawText(string text, bool processBacktickExpressions)
	{
		if (processBacktickExpressions)
		{
			text = ProcessBacktickedExpressions(text);
		}
		((TMP_Text)textUGUI).text = text;
		return this;
	}

	public TutorialArrow SetBoxPlacement(BoxPlacementMode mode)
	{
		_boxMode = mode;
		return this;
	}

	public TutorialArrow SetBoxPlacementByOffset(Vector2 offset)
	{
		_customOffset = offset;
		return this;
	}

	public TutorialArrow SetLocalizedText(string key, object[] formatArgs = null)
	{
		string text = DewLocalization.GetUIValue(key);
		if (formatArgs != null)
		{
			text = string.Format(text, formatArgs);
		}
		((TMP_Text)textUGUI).text = ProcessBacktickedExpressions(text);
		return this;
	}

	private string ProcessBacktickedExpressions(string input)
	{
		RefText val = new RefText();
		DewLocalizationNodeParser.ParseBacktickedString(input, (string normal) =>
		{
			val.text += normal;
		}, (string tagged) =>
		{
			RefText refText = val;
			refText.text = refText.text + "<" + tagged + ">";
		}, (string backticked) =>
		{
			RefText refText = val;
			refText.text = refText.text + "[" + DewSave.profileMain.controls.GetSettingsValueText(backticked) + "]";
		});
		val.text = val.text.Replace("[", "<color=yellow>").Replace("]", "</color>");
		return val.text;
	}

	public TutorialArrow FollowUIElement(Transform target)
	{
		_followTarget = target;
		_followMode = ArrowFollowMode.Screen;
		return this;
	}

	public TutorialArrow FollowScreenPos(Func<Vector3> func)
	{
		_followFunc = func;
		_followMode = ArrowFollowMode.Screen;
		return this;
	}

	public TutorialArrow FollowWorldTarget(Transform target)
	{
		_followTarget = target;
		_followMode = ArrowFollowMode.World;
		return this;
	}

	public TutorialArrow FollowWorldTarget(Func<Vector3> func)
	{
		_followFunc = func;
		_followMode = ArrowFollowMode.World;
		return this;
	}

	public TutorialArrow SetDestroyCondition(Func<bool> condition)
	{
		destroyCondition = condition;
		return this;
	}

	public TutorialArrow SetOnDestroy(Action func)
	{
		onDestroy = (Action)Delegate.Combine(onDestroy, func);
		return this;
	}

	public TutorialArrow MarkDestroy()
	{
		_isDestroyConditionMet = true;
		return this;
	}
}
