using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

[ExecuteAlways]
[RequireComponent(typeof(CanvasGroup))]
[RequireComponent(typeof(Canvas))]
public class View : MonoBehaviour
{
	public enum ShowBehavior
	{
		UseState,
		Manually
	}

	public static List<View> instances = new List<View>();

	public static List<View> shownInstances = new List<View>();

	public static SafeAction onShownInstancesChanged;

	public ShowBehavior showBehavior;

	private bool _show;

	public List<string> showOn = new List<string>();

	public List<string> ignoreOn = new List<string>();

	public GameObject showEffect;

	public GameObject hideEffect;

	public float fadeTime;

	public bool interactableWhenShown = true;

	public bool blockRaycastWhenShown = true;

	public bool disableGameObjectWhenHidden;

	public bool disablesInGamePlayingInput;

	public UnityEvent onShow;

	public UnityEvent onHide;

	private CanvasGroup _canvasGroup;

	private Canvas _canvas;

	private float _appliedAlpha = float.NaN;

	private int _appliedGroupFlags = -1;

	public bool isShowing => _show;

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	private static void OnInit()
	{
		instances = new List<View>();
		shownInstances = new List<View>();
		onShownInstancesChanged = null;
	}

	protected virtual void Awake()
	{
		_canvasGroup = GetComponent<CanvasGroup>();
		_canvas = GetComponent<Canvas>();
		instances.Add(this);
	}

	protected virtual void Start()
	{
		if (Application.IsPlaying(this))
		{
			if (ManagerBase<UIManager>.instance != null)
			{
				ManagerBase<UIManager>.instance.onStateChanged += new Action<string, string>(OnStateChanged);
				UpdateShowState(ManagerBase<UIManager>.instance.state);
			}
			else
			{
				Debug.LogWarning("View '" + name + "' has no UIManager");
			}
			UpdateComponentStatus(_show);
		}
	}

	protected virtual void OnDestroy()
	{
		if (ManagerBase<UIManager>.instance != null)
		{
			ManagerBase<UIManager>.instance.onStateChanged -= new Action<string, string>(OnStateChanged);
		}
		instances.Remove(this);
		if (InGameUIManager.instance != null)
		{
			InGameUIManager.instance.UpdateDisablePlayingInputByView();
		}
		if (shownInstances.Contains(this))
		{
			shownInstances.Remove(this);
			onShownInstancesChanged?.Invoke();
		}
	}

	private void OnStateChanged(string arg1, string arg2)
	{
		UpdateShowState(arg2);
	}

	private void UpdateShowState(string newState)
	{
		if (showBehavior == ShowBehavior.UseState)
		{
			bool show = _show;
			if (!ignoreOn.Contains(newState))
			{
				_show = showOn.Contains(newState);
			}
			if (!show && _show)
			{
				OnShow();
			}
			else if (show && !_show)
			{
				OnHide();
			}
		}
	}

	public void Show()
	{
		if (showBehavior != ShowBehavior.Manually)
		{
			throw new InvalidOperationException("Show Behaviour is not set to ShowBehaviour.Manually");
		}
		if (!_show)
		{
			_show = true;
			OnShow();
			Update();
		}
	}

	public void Hide()
	{
		if (showBehavior != ShowBehavior.Manually)
		{
			throw new InvalidOperationException("Show Behaviour is not set to ShowBehaviour.Manually");
		}
		if (_show)
		{
			_show = false;
			OnHide();
			Update();
		}
	}

	protected virtual void OnShow()
	{
		shownInstances.Add(this);
		for (int num = shownInstances.Count - 1; num >= 0; num--)
		{
			if (shownInstances[num] == null)
			{
				shownInstances.RemoveAt(num);
			}
		}
		shownInstances.Sort((View x, View y) => -string.Compare(GetHierarchyPath(x.transform), GetHierarchyPath(y.transform), StringComparison.Ordinal));
		onShownInstancesChanged?.Invoke();
		gameObject.SetActive(value: true);
		onShow?.Invoke();
		DewEffect.Play(showEffect);
		DewEffect.Stop(hideEffect);
		static string GetHierarchyPath(Transform transform)
		{
			List<int> list = new List<int>();
			Transform transform2 = transform;
			while (transform2 != null)
			{
				list.Add(transform2.GetSiblingIndex());
				transform2 = transform2.parent;
			}
			list.Reverse();
			return string.Join("/", list.Select((int i) => i.ToString("D4")));
		}
	}

	protected virtual void OnHide()
	{
		shownInstances.Remove(this);
		onShownInstancesChanged?.Invoke();
		onHide?.Invoke();
		DewEffect.Play(hideEffect);
		DewEffect.Stop(showEffect);
	}

	protected virtual void Update()
	{
		bool show = _show;
		UpdateComponentStatus(show);
	}

	private void UpdateComponentStatus(bool shouldShow)
	{
		float num = ((!(fadeTime < 0.001f) && Application.IsPlaying(this)) ? Mathf.MoveTowards(float.IsNaN(_appliedAlpha) ? _canvasGroup.alpha : _appliedAlpha, shouldShow ? 1 : 0, Time.unscaledDeltaTime / fadeTime) : ((float)(shouldShow ? 1 : 0)));
		if (num != _appliedAlpha)
		{
			_canvasGroup.alpha = num;
			_appliedAlpha = num;
		}
		int num2 = ((shouldShow && interactableWhenShown) ? 1 : 0) | ((shouldShow && blockRaycastWhenShown) ? 2 : 0);
		if (num2 != _appliedGroupFlags)
		{
			_canvasGroup.interactable = (num2 & 1) != 0;
			_canvasGroup.blocksRaycasts = (num2 & 2) != 0;
			_appliedGroupFlags = num2;
		}
		if ((UnityEngine.Object)(object)_canvas == null)
		{
			_canvas = GetComponent<Canvas>();
		}
		((Behaviour)(object)_canvas).enabled = num > 0.0001f;
		if (Application.IsPlaying(this))
		{
			if (disableGameObjectWhenHidden && !shouldShow && num < 0.0001f)
			{
				gameObject.SetActive(value: false);
			}
			if (InGameUIManager.instance != null)
			{
				InGameUIManager.instance.UpdateDisablePlayingInputByView();
			}
		}
	}

	private static bool IsParentOrSelf(Transform parent, Transform child)
	{
		if (parent == child)
		{
			return true;
		}
		if (child == null)
		{
			return false;
		}
		return IsParentOrSelf(parent, child.parent);
	}

	private static bool HasCommonElement(List<string> a, List<string> b)
	{
		foreach (string item in a)
		{
			foreach (string item2 in b)
			{
				if (item == item2)
				{
					return true;
				}
			}
		}
		return false;
	}

	protected virtual void OnEnable()
	{
	}

	protected virtual void OnDisable()
	{
	}
}
