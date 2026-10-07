using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Sirenix.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GlobalUIManager : ManagerBase<GlobalUIManager>
{
	public struct RectResult
	{
		public Rect rect;

		public float distance;
	}

	public RectTransform tooltipBoxTransform;

	public CanvasGroup tooltipBoxCanvasGroup;

	[NonSerialized]
	public bool isBackDisabled;

	[NonSerialized]
	public IMenuView currentMenuView;

	public TMP_FontAsset fontBody;

	public TMP_FontAsset fontHeading;

	public TMP_FontAsset fontHeadingBold;

	public TMP_FontAsset fontHeadingLight;

	public RectTransform devFloatingTextParent;

	public TextMeshProUGUI devFloatingTextPrefab;

	private readonly List<BackHandler> _backHandlers = new List<BackHandler>();

	private DewInputTrigger it_back;

	private DewInputTrigger it_menu;

	private Dictionary<RectTransform, bool> _isUIElementClickable = new Dictionary<RectTransform, bool>();

	public GameObject fxTick;

	public GameObject focusDisplayObject;

	public Transform focusDisplayAnimationTarget;

	public RectTransform focusDisplayTransform;

	public GameObject focusDisplayBoxObject;

	public GameObject focusDisplayCircleObject;

	private TMP_InputField _targetField;

	internal List<IGamepadFocusable> _allFocusables = new List<IGamepadFocusable>();

	internal List<UI_GamepadFocusableGroup> _allGroups = new List<UI_GamepadFocusableGroup>();

	private DewInputTrigger it_gamepadConfirm;

	private DewInputTrigger it_gamepadUp;

	private DewInputTrigger it_gamepadLeft;

	private DewInputTrigger it_gamepadDown;

	private DewInputTrigger it_gamepadRight;

	public Action<IGamepadFocusable, IGamepadFocusable> onFocusedChanged;

	[NonSerialized]
	public bool disableGamepadUiInputs;

	private List<IGamepadFocusable> _focusedHistory = new List<IGamepadFocusable> { null };

	private List<IGamepadFocusListener> _focusedListeners = new List<IGamepadFocusListener>();

	private int _framesWithoutFocus;

	public GameObject fxHighlightShow;

	public GameObject fxHighlightHide;

	public GameObject tutHighlightObject;

	public RectTransform tutHighlightBox;

	public RectTransform tutHighlightBackdropLeft;

	public RectTransform tutHighlightBackdropRight;

	public RectTransform tutHighlightBackdropTop;

	public RectTransform tutHighlightBackdropBottom;

	public TextMeshProUGUI tutHighlightText;

	private TutorialHighlightSettings _currentHighlight;

	private int _lastHighlightFrame = int.MinValue;

	private float _highlightStartUnscaledTime;

	private Vector2 _anchoredPosCv;

	private Vector2 _sizeDeltaCv;

	private bool _didAddContinueText;

	public bool isTooltipShown => tooltipBoxCanvasGroup.alpha > 0.8f;

	public Rect tooltipScreenSpaceRect { get; private set; }

	public float lastFocusChangeUnscaledTime { get; private set; }

	public IGamepadFocusable focused { get; private set; }

	public bool isTutorialHighlighting => _currentHighlight != null;

	private void Start()
	{
		InitGamepadInput();
		it_back = new DewInputTrigger
		{
			owner = this,
			priority = 0,
			binding = () => DewSave.profileMain.controls.back,
			isValidCheck = () => !isBackDisabled
		};
		it_menu = new DewInputTrigger
		{
			owner = this,
			priority = 0,
			binding = () => DewSave.profileMain.controls.menu,
			isValidCheck = () => currentMenuView is UnityEngine.Object obj && obj != null && (currentMenuView.CanShowMenu() || currentMenuView.IsShowing())
		};
		Start_Tutorial();
	}

	private void OnDestroy()
	{
		OnDestroy_Tutorial();
	}

	public override void FrameUpdate()
	{
		base.FrameUpdate();
		FrameUpdate_Tutorial();
		if (ManagerBase<TransitionManager>.instance.state == TransitionManager.StateType.Loading)
		{
			focusDisplayObject.SetActive(value: false);
			return;
		}
		if (it_menu.down && currentMenuView is UnityEngine.Object obj && obj != null && !isTutorialHighlighting && !ManagerBase<MessageManager>.instance.isShowingMessage && (currentMenuView.CanShowMenu() || currentMenuView.IsShowing()))
		{
			if (currentMenuView.IsShowing())
			{
				GoBack();
			}
			else
			{
				currentMenuView.ShowMenu();
			}
		}
		if (DewInput.currentMode == InputMode.KeyboardAndMouse && !isBackDisabled && it_back.down && _backHandlers.Count > 0)
		{
			GoBack();
		}
		if (DewInput.currentMode == InputMode.Gamepad)
		{
			if (ManagerBase<InputManager>.instance._lastResetInputDevicesFrameCount + 5 < Time.frameCount)
			{
				DoGamepadInputs();
			}
		}
		else
		{
			if (focused == null)
			{
				return;
			}
			IGamepadFocusable arg = focused;
			focused = null;
			try
			{
				onFocusedChanged?.Invoke(arg, null);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
			foreach (IGamepadFocusListener focusedListener in _focusedListeners)
			{
				if (focusedListener.IsValid())
				{
					focusedListener.OnFocusStateChanged(state: false);
				}
			}
			_focusedListeners.Clear();
			focusDisplayObject.SetActive(value: false);
		}
	}

	public override void LogicUpdate(float dt)
	{
		base.LogicUpdate(dt);
		LogicUpdate_Tutorial();
		if (isTooltipShown)
		{
			tooltipScreenSpaceRect = tooltipBoxTransform.GetScreenSpaceRect();
		}
	}

	public void GoBack()
	{
		BackHandler[] array = _backHandlers.ToArray();
		for (int num = array.Length - 1; num >= 0; num--)
		{
			BackHandler backHandler = array[num];
			if (backHandler.owner == null)
			{
				_backHandlers.Remove(backHandler);
			}
			else
			{
				try
				{
					if (backHandler.func())
					{
						break;
					}
				}
				catch (Exception exception)
				{
					Debug.LogException(exception);
					_backHandlers.Remove(backHandler);
				}
			}
		}
	}

	public BackHandler AddBackHandler(MonoBehaviour owner, int priority, Func<bool> callback)
	{
		BackHandler backHandler = new BackHandler
		{
			owner = owner,
			func = callback,
			priority = priority
		};
		int num = -1;
		for (int i = 0; i < _backHandlers.Count && _backHandlers[i].priority <= priority; i++)
		{
			num = i;
		}
		_backHandlers.Insert(num + 1, backHandler);
		for (int num2 = _backHandlers.Count - 1; num2 >= 0; num2--)
		{
			if (_backHandlers[num2].owner == null)
			{
				_backHandlers.RemoveAt(num2);
			}
		}
		return backHandler;
	}

	public void RemoveBackHandler(BackHandler handle)
	{
		_backHandlers.Remove(handle);
	}

	public bool IsUIElementClickable(RectTransform rectTransform)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		if (_isUIElementClickable.TryGetValue(rectTransform, out var value))
		{
			return value;
		}
		List<RaycastResult> list = Dew.RaycastAllUIElementsBelowScreenPoint(rectTransform.GetScreenSpaceRect().center, out var handle);
		int num;
		if (list.Count > 0)
		{
			RaycastResult val = list[0];
			if (!val.gameObject.transform.IsChildOf(rectTransform))
			{
				val = list[0];
				num = ((val.gameObject.transform == rectTransform) ? 1 : 0);
			}
			else
			{
				num = 1;
			}
		}
		else
		{
			num = 0;
		}
		bool flag = (byte)num != 0;
		handle.Return();
		_isUIElementClickable[rectTransform] = flag;
		return flag;
	}

	public void ShowDevText(string rawText, bool log = true)
	{
		if (DewSave.platformSettings.gameplay.enableDeveloperMode)
		{
			if (log)
			{
				Debug.Log(rawText ?? "");
			}
			((TMP_Text)UnityEngine.Object.Instantiate<TextMeshProUGUI>(devFloatingTextPrefab, (Transform)devFloatingTextParent)).text = rawText;
		}
	}

	public void EnforceFontFallbackOrder()
	{
		Enforce(fontBody);
		Enforce(fontHeading);
		Enforce(fontHeadingBold);
		Enforce(fontHeadingLight);
		static void Enforce(TMP_FontAsset fontAsset)
		{
			List<TMP_FontAsset> table = fontAsset.fallbackFontAssetTable.ToList();
			bool flag = DewSave.profileMain.language == "ja-JP";
			int num = table.FindIndex((TMP_FontAsset x) => ((UnityEngine.Object)(object)x).name.Contains("JP"));
			int num2 = table.FindIndex((TMP_FontAsset x) => ((UnityEngine.Object)(object)x).name.Contains("SC"));
			if (num != -1 && num2 != -1)
			{
				if ((flag && num > num2) || (!flag && num2 > num))
				{
					List<TMP_FontAsset> list = table;
					int index = num;
					List<TMP_FontAsset> list2 = table;
					int index2 = num2;
					TMP_FontAsset val = table[num2];
					TMP_FontAsset val2 = table[num];
					TMP_FontAsset val3 = (list[index] = val);
					val3 = (list2[index2] = val2);
				}
				UseFont("simfang", new string[1] { "zh-CN" });
				UseFont("EBGaramond", new string[1] { "ru-RU" });
				if (((UnityEngine.Object)(object)fontAsset).name.Contains("Heading"))
				{
					UseFont("fontBody", new string[2] { "pl-PL", "tr-TR" });
				}
				if (DewSave.profileMain.language.StartsWith("zh-"))
				{
					fontAsset.italicStyle = 0;
				}
				else
				{
					fontAsset.italicStyle = 25;
				}
				fontAsset.fallbackFontAssetTable = table;
			}
			void UseFont(string substr, string[] language)
			{
				int num3 = table.FindIndex((TMP_FontAsset x) => ((UnityEngine.Object)(object)x).name.Contains(substr));
				if (num3 >= 0)
				{
					TMP_FontAsset item = table[num3];
					if (num3 == 0 && !language.Contains(DewSave.profileMain.language))
					{
						table.RemoveAt(num3);
						table.Add(item);
					}
					else if (num3 != 0 && language.Contains(DewSave.profileMain.language))
					{
						table.RemoveAt(num3);
						table.Insert(0, item);
					}
				}
			}
		}
	}

	private void LateUpdate()
	{
		_isUIElementClickable.Clear();
		if (ManagerBase<TransitionManager>.instance.state == TransitionManager.StateType.Loading || (ManagerBase<UIManager>.softInstance != null && !ManagerBase<UIManager>.softInstance.ShouldDoAutoFocus()))
		{
			return;
		}
		if (focused == null && DewInput.currentMode == InputMode.Gamepad)
		{
			_framesWithoutFocus++;
			if (_framesWithoutFocus <= 4 || (_framesWithoutFocus - 5) % 5 != 0 || TryMoveFocusBack())
			{
				return;
			}
			float num = float.NegativeInfinity;
			IGamepadFocusable gamepadFocusable = null;
			for (int i = 0; i < _allFocusables.Count; i++)
			{
				if (_allFocusables[i].IsValid() && _allFocusables[i].CanBeFocused())
				{
					Rect screenSpaceRect = ((RectTransform)((Component)_allFocusables[i]).transform).GetScreenSpaceRect();
					float num2 = screenSpaceRect.center.y - screenSpaceRect.center.x;
					if (!(num2 < num))
					{
						num = num2;
						gamepadFocusable = _allFocusables[i];
					}
				}
			}
			if (gamepadFocusable != null)
			{
				SetFocus(gamepadFocusable);
			}
		}
		else
		{
			_framesWithoutFocus = 0;
		}
	}

	public T GetFocusedComponent<T>() where T : Component
	{
		IGamepadFocusable gamepadFocusable = focused;
		if (gamepadFocusable == null)
		{
			return null;
		}
		return gamepadFocusable.GetTransform().GetComponent<T>();
	}

	private void InitGamepadInput()
	{
		it_gamepadConfirm = new DewInputTrigger
		{
			owner = this,
			binding = () => DewSave.profileMain.controls.confirm,
			priority = -50,
			isValidCheck = () => focused != null
		};
		it_gamepadUp = DpadControl(GamepadButtonEx.DpadUp, GamepadButtonEx.LeftStickUp);
		it_gamepadLeft = DpadControl(GamepadButtonEx.DpadLeft, GamepadButtonEx.LeftStickLeft);
		it_gamepadDown = DpadControl(GamepadButtonEx.DpadDown, GamepadButtonEx.LeftStickDown);
		it_gamepadRight = DpadControl(GamepadButtonEx.DpadRight, GamepadButtonEx.LeftStickRight);
		DewInputTrigger DpadControl(GamepadButtonEx button, GamepadButtonEx button2)
		{
			return new DewInputTrigger
			{
				owner = this,
				priority = -1000,
				binding = () => DewBinding.GamepadOnly(button, button2),
				isValidCheck = ShouldDoDpad
			};
		}
	}

	public void AddGamepadFocusable(IGamepadFocusable focusable)
	{
		if (!focusable.IsValid() || _allFocusables.Contains(focusable))
		{
			return;
		}
		for (int num = _allFocusables.Count - 1; num >= 0; num--)
		{
			if (!_allFocusables[num].IsValid())
			{
				_allFocusables.RemoveAt(num);
			}
		}
		_allFocusables.Add(focusable);
		if (focused == null && focusable.CanBeFocused() && focusable.GetBehavior() == FocusableBehavior.Normal)
		{
			SetFocus(focusable);
		}
	}

	public void RemoveGamepadFocusable(IGamepadFocusable focusable)
	{
		_allFocusables.Remove(focusable);
		if (focused == focusable)
		{
			SetFocus(null);
		}
	}

	public void Unfocus(IGamepadFocusable focusable)
	{
		if (focused == focusable)
		{
			SetFocus(null);
		}
	}

	public void SetFocusOnComponent(Component focusable)
	{
		SetFocus(focusable.GetComponent<IGamepadFocusable>());
	}

	public void SetFocus(IGamepadFocusable focusable)
	{
		if (DewInput.currentMode != InputMode.Gamepad)
		{
			return;
		}
		IGamepadFocusable gamepadFocusable = focused;
		EventSystem.current.SetSelectedGameObject((GameObject)null);
		if (focusable != null && !focusable.IsValid())
		{
			focusable = null;
		}
		if (focused != null || _focusedHistory[0] != null)
		{
			for (int num = _focusedHistory.Count - 1; num >= 0; num--)
			{
				if (_focusedHistory[num] == focused || (_focusedHistory[num] is Component component && component == null))
				{
					_focusedHistory.RemoveAt(num);
				}
			}
			_focusedHistory.Insert(0, focused);
			while (_focusedHistory.Count > 128)
			{
				_focusedHistory.RemoveAt(_focusedHistory.Count - 1);
			}
		}
		focused = focusable;
		if (gamepadFocusable != focused)
		{
			try
			{
				onFocusedChanged?.Invoke(gamepadFocusable, focused);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
		lastFocusChangeUnscaledTime = Time.unscaledTime;
		if (focused == null)
		{
			NotifyNewListeners(null);
			return;
		}
		StopAllCoroutines();
		StartCoroutine(Routine());
		Component component2 = (Component)focused;
		List<IGamepadFocusListener> componentsInParentNonAlloc = component2.GetComponentsInParentNonAlloc(out ListReturnHandle<IGamepadFocusListener> handle);
		NotifyNewListeners(componentsInParentNonAlloc);
		handle.Return();
		if (focused != null && !((UnityEngine.Object)(object)focused.GetTransform().GetComponentInParent<TMP_Dropdown>() == null))
		{
			return;
		}
		ScrollRect componentInParent = component2.GetComponentInParent<ScrollRect>();
		if (!((UnityEngine.Object)(object)componentInParent != null))
		{
			return;
		}
		Canvas.ForceUpdateCanvases();
		Rect screenSpaceRect = componentInParent.viewport.GetScreenSpaceRect();
		Vector2 vector = (Vector2)((Component)(object)componentInParent).transform.InverseTransformPoint(componentInParent.content.position) - (Vector2)((Component)(object)componentInParent).transform.InverseTransformPoint(component2.transform.position);
		float num2 = componentInParent.content.sizeDelta.y - screenSpaceRect.height / componentInParent.viewport.lossyScale.y;
		if (num2 >= 0f)
		{
			componentInParent.content.anchoredPosition = componentInParent.content.anchoredPosition.WithY(Mathf.Clamp(vector.y - screenSpaceRect.height * 0.5f, 0f, num2));
		}
		if (componentInParent.horizontal)
		{
			float num3 = componentInParent.content.rect.width - screenSpaceRect.width / componentInParent.viewport.lossyScale.x;
			if (num3 >= 0f)
			{
				componentInParent.content.anchoredPosition = componentInParent.content.anchoredPosition.WithX(Mathf.Clamp(vector.x + screenSpaceRect.width * 0.5f, 0f - num3, 0f));
			}
		}
		IEnumerator Routine()
		{
			ShortcutExtensions.DOKill((Component)focusDisplayAnimationTarget, true);
			focusDisplayAnimationTarget.localScale = Vector3.zero;
			yield return null;
			focusDisplayAnimationTarget.localScale = Vector3.one * 1.15f;
			TweenSettingsExtensions.SetUpdate<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOScale(focusDisplayAnimationTarget, Vector3.one, 0.17f), true);
		}
	}

	public void ClearFocusWithoutHistory()
	{
		if (focused == null)
		{
			focusDisplayObject.SetActive(value: false);
			return;
		}
		EventSystem current = EventSystem.current;
		if (current != null)
		{
			current.SetSelectedGameObject((GameObject)null);
		}
		IGamepadFocusable arg = focused;
		focused = null;
		lastFocusChangeUnscaledTime = Time.unscaledTime;
		try
		{
			onFocusedChanged?.Invoke(arg, null);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
		foreach (IGamepadFocusListener focusedListener in _focusedListeners)
		{
			if (focusedListener.IsValid())
			{
				focusedListener.OnFocusStateChanged(state: false);
			}
		}
		_focusedListeners.Clear();
		focusDisplayObject.SetActive(value: false);
	}

	public bool MoveFocus(Vector3 direction)
	{
		if (focused == null || !focused.IsValid())
		{
			foreach (IGamepadFocusable allFocusable in _allFocusables)
			{
				if (allFocusable.IsValid() && allFocusable.CanBeFocused())
				{
					SetFocusWithRespectToGroupEnterBehavior(allFocusable);
					return true;
				}
			}
			SetFocus(null);
			return false;
		}
		IGamepadNavigationHint componentInParent = ((Component)focused).GetComponentInParent<IGamepadNavigationHint>(includeInactive: true);
		if (componentInParent != null)
		{
			if (direction == Vector3.up && componentInParent.TryGetUp(out var next))
			{
				if (next != null)
				{
					SetFocus(next);
				}
				return true;
			}
			if (direction == Vector3.left && componentInParent.TryGetLeft(out var next2))
			{
				if (next2 != null)
				{
					SetFocus(next2);
				}
				return true;
			}
			if (direction == Vector3.down && componentInParent.TryGetDown(out var next3))
			{
				if (next3 != null)
				{
					SetFocus(next3);
				}
				return true;
			}
			if (direction == Vector3.right && componentInParent.TryGetRight(out var next4))
			{
				if (next4 != null)
				{
					SetFocus(next4);
				}
				return true;
			}
		}
		if (focused.GetBehavior() == FocusableBehavior.BottomBar && direction == (Vector3)Vector2.up && TryMoveFocusBack())
		{
			return true;
		}
		if (TryGetNextFocusableInSameScrollRect(out var bestInternalTarget))
		{
			SetFocus(bestInternalTarget);
			return true;
		}
		TryGetFocusTargetGeneric(direction, null, out var next5);
		if (next5 != null)
		{
			SetFocus(next5);
			return true;
		}
		return false;
		void SetFocusWithRespectToGroupEnterBehavior(IGamepadFocusable nextTarget)
		{
			if (nextTarget == null || !(nextTarget is Component component) || component == null)
			{
				SetFocus(null);
			}
			else
			{
				UI_GamepadFocusableGroup componentInParent2 = component.GetComponentInParent<UI_GamepadFocusableGroup>();
				if (componentInParent2 == null)
				{
					SetFocus(nextTarget);
				}
				else
				{
					IGamepadFocusable gamepadFocusable = componentInParent2.GetEnterFocusable(direction);
					if (gamepadFocusable == null)
					{
						gamepadFocusable = nextTarget;
					}
					SetFocus(gamepadFocusable);
				}
			}
		}
		bool TryGetNextFocusableInSameScrollRect(out IGamepadFocusable reference)
		{
			reference = null;
			if (!(focused is Component component))
			{
				return false;
			}
			ScrollRect componentInParent2 = component.GetComponentInParent<ScrollRect>();
			if ((UnityEngine.Object)(object)componentInParent2 == null || componentInParent2.content == null)
			{
				return false;
			}
			Vector3 normalized = direction.normalized;
			Vector2 vector = componentInParent2.content.InverseTransformPoint(component.transform.position);
			float num = float.MaxValue;
			for (int i = 0; i < _allFocusables.Count; i++)
			{
				IGamepadFocusable gamepadFocusable = _allFocusables[i];
				if (gamepadFocusable != focused && gamepadFocusable != null && gamepadFocusable.IsValid())
				{
					Component component2 = (Component)gamepadFocusable;
					if (component2.gameObject.activeInHierarchy && component2.transform.IsChildOf(componentInParent2.content))
					{
						Vector2 vector2 = (Vector2)componentInParent2.content.InverseTransformPoint(component2.transform.position) - vector;
						float magnitude = vector2.magnitude;
						if (!(magnitude < 0.001f))
						{
							Vector2 rhs = vector2 / magnitude;
							if (Vector2.Dot(normalized, rhs) > 0.5f && magnitude < num && gamepadFocusable.CanBeFocused())
							{
								num = magnitude;
								reference = gamepadFocusable;
							}
						}
					}
				}
			}
			return reference != null;
		}
	}

	public bool MoveFocus(Vector3 direction, float angleLimit, float normalizedPerpendicularDistLimit, float normalizedDistLimit, Func<IGamepadFocusable, bool> condition = null, Func<IGamepadFocusable, float> customScoreFunc = null)
	{
		return MoveFocus(direction, new Vector2(0f, angleLimit), new Vector2(0f, normalizedPerpendicularDistLimit), new Vector2(0f, normalizedDistLimit), condition, customScoreFunc);
	}

	public bool MoveFocus(Vector3 direction, Vector2 angleLimit, Vector2 normalizedPerpendicularDistLimit, Vector2 normalizedDistLimit, Func<IGamepadFocusable, bool> condition = null, Func<IGamepadFocusable, float> customScoreFunc = null)
	{
		if (focused == null || !focused.IsValid())
		{
			foreach (IGamepadFocusable allFocusable in _allFocusables)
			{
				if (allFocusable.IsValid() && allFocusable.CanBeFocused())
				{
					SetFocus(allFocusable);
					return true;
				}
			}
			SetFocus(null);
			return false;
		}
		if (focused.GetBehavior() == FocusableBehavior.BottomBar && direction == (Vector3)Vector2.up && TryMoveFocusBack())
		{
			return true;
		}
		TryGetFocusTarget(new GetFocusTargetSettings
		{
			direction = direction,
			angleLimit = angleLimit,
			normalizedPerpendicularDistLimit = normalizedPerpendicularDistLimit,
			normalizedDistLimit = normalizedDistLimit,
			condition = condition,
			customScoreFunc = customScoreFunc
		}, out var next);
		if (next != null)
		{
			SetFocus(next);
			return true;
		}
		return false;
	}

	public bool TryMoveFocusBack()
	{
		if (ManagerBase<ControlManager>.softInstance != null)
		{
			return false;
		}
		for (int i = 0; i < _focusedHistory.Count; i++)
		{
			if (_focusedHistory[i] != focused && _focusedHistory[i] != null && _focusedHistory[i].IsValid() && _focusedHistory[i].CanBeFocused())
			{
				IGamepadFocusable focus = _focusedHistory[i];
				_focusedHistory[i] = null;
				SetFocus(focus);
				return true;
			}
		}
		return false;
	}

	private void NotifyNewListeners(List<IGamepadFocusListener> newListeners)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Expected Obj, but got Unknown
		PointerEventData val = new PointerEventData(EventSystem.current);
		foreach (IGamepadFocusListener focusedListener in _focusedListeners)
		{
			try
			{
				if (focusedListener.IsValid() && (newListeners == null || !newListeners.Contains(focusedListener)))
				{
					focusedListener.OnFocusStateChanged(state: false);
				}
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
		if (_focusedHistory[0] is Component component && component != null)
		{
			ListReturnHandle<IPointerExitHandler> handle;
			foreach (IPointerExitHandler item in component.GetComponentsNonAlloc(out handle))
			{
				item.OnPointerExit(val);
			}
			handle.Return();
		}
		if (newListeners != null)
		{
			foreach (IGamepadFocusListener newListener in newListeners)
			{
				try
				{
					if (newListener.IsValid() && !_focusedListeners.Contains(newListener))
					{
						newListener.OnFocusStateChanged(state: true);
					}
				}
				catch (Exception exception2)
				{
					Debug.LogException(exception2);
				}
			}
		}
		if (focused is Component component2 && component2 != null)
		{
			ListReturnHandle<IPointerEnterHandler> handle2;
			foreach (IPointerEnterHandler item2 in component2.GetComponentsNonAlloc(out handle2))
			{
				item2.OnPointerEnter(val);
			}
			handle2.Return();
		}
		_focusedListeners.Clear();
		if (newListeners != null)
		{
			_focusedListeners.AddRange(newListeners);
		}
	}

	private bool ShouldDoDpad()
	{
		if (focused == null)
		{
			return ManagerBase<ControlManager>.softInstance == null;
		}
		return true;
	}

	private void DoGamepadInputs()
	{
		if (focused != null && (!focused.IsValid() || !focused.CanBeFocused()))
		{
			SetFocus(null);
		}
		if (!disableGamepadUiInputs)
		{
			if (focused is IGamepadFocusableOverrideInput gamepadFocusableOverrideInput)
			{
				gamepadFocusableOverrideInput.OnGamepadUpdate();
			}
			bool flag = !ShouldDoDpad();
			if (!flag && it_gamepadUp.downRepeated && (!(focused is IGamepadFocusableOverrideInput gamepadFocusableOverrideInput2) || !gamepadFocusableOverrideInput2.OnGamepadDpadUp()))
			{
				MoveFocus(Vector2.up);
				PlayTickSFX();
			}
			if (!flag && it_gamepadLeft.downRepeated && (!(focused is IGamepadFocusableOverrideInput gamepadFocusableOverrideInput3) || !gamepadFocusableOverrideInput3.OnGamepadDpadLeft()))
			{
				MoveFocus(Vector2.left);
				PlayTickSFX();
			}
			if (!flag && it_gamepadDown.downRepeated && (!(focused is IGamepadFocusableOverrideInput gamepadFocusableOverrideInput4) || !gamepadFocusableOverrideInput4.OnGamepadDpadDown()))
			{
				MoveFocus(Vector2.down);
				PlayTickSFX();
			}
			if (!flag && it_gamepadRight.downRepeated && (!(focused is IGamepadFocusableOverrideInput gamepadFocusableOverrideInput5) || !gamepadFocusableOverrideInput5.OnGamepadDpadRight()))
			{
				MoveFocus(Vector2.right);
				PlayTickSFX();
			}
			if (((focused != null && focused.CanHoldConfirmToRepeat()) ? it_gamepadConfirm.downRepeated : it_gamepadConfirm.down) && focused != null && (!(focused is IGamepadFocusableOverrideInput gamepadFocusableOverrideInput6) || !gamepadFocusableOverrideInput6.OnGamepadConfirm()))
			{
				Component comp = (Component)focused;
				SimulateClickOnUIElement(comp);
			}
			if (it_back.down && (focused == null || !(focused is IGamepadFocusableOverrideInput gamepadFocusableOverrideInput7) || !gamepadFocusableOverrideInput7.OnGamepadBack()))
			{
				GoBack();
			}
		}
		if (focused == null || focused.GetSelectionDisplayType() == SelectionDisplayType.Dont)
		{
			focusDisplayObject.SetActive(value: false);
			return;
		}
		SelectionDisplayType selectionDisplayType = focused.GetSelectionDisplayType();
		focusDisplayObject.SetActive(value: true);
		focusDisplayBoxObject.SetActive(selectionDisplayType == SelectionDisplayType.Box);
		focusDisplayCircleObject.SetActive(selectionDisplayType == SelectionDisplayType.Circle);
		Transform parent = focusDisplayTransform.parent;
		RectTransform parent2 = (RectTransform)((Component)focused).transform;
		int siblingIndex = focusDisplayTransform.GetSiblingIndex();
		focusDisplayTransform.SetParent(parent2, worldPositionStays: true);
		focusDisplayTransform.localScale = Vector3.one;
		focusDisplayTransform.localRotation = Quaternion.identity;
		focusDisplayTransform.anchorMin = Vector2.zero;
		focusDisplayTransform.anchorMax = Vector2.one;
		focusDisplayTransform.sizeDelta = Vector2.zero;
		focusDisplayTransform.anchoredPosition = Vector2.zero;
		focusDisplayTransform.localPosition = focusDisplayTransform.localPosition.WithZ(0f);
		focusDisplayTransform.SetParent(parent, worldPositionStays: true);
		focusDisplayTransform.SetSiblingIndex(siblingIndex);
		focusDisplayTransform.localPosition = focusDisplayTransform.localPosition.WithZ(0f);
	}

	public void SimulateClickOnUIElement(Component comp)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Expected Obj, but got Unknown
		PointerEventData val = new PointerEventData(EventSystem.current)
		{
			button = (InputButton)0
		};
		IPointerDownHandler[] components = comp.GetComponents<IPointerDownHandler>();
		for (int i = 0; i < components.Length; i++)
		{
			components[i].OnPointerDown(val);
		}
		IPointerUpHandler[] components2 = comp.GetComponents<IPointerUpHandler>();
		for (int i = 0; i < components2.Length; i++)
		{
			components2[i].OnPointerUp(val);
		}
		IPointerClickHandler[] components3 = comp.GetComponents<IPointerClickHandler>();
		for (int i = 0; i < components3.Length; i++)
		{
			components3[i].OnPointerClick(val);
		}
	}

	public void SetFocusOnFirstFocusable(GameObject parent)
	{
		if (parent == null)
		{
			return;
		}
		List<IGamepadFocusable> componentsInChildrenNonAlloc = parent.GetComponentsInChildrenNonAlloc(out ListReturnHandle<IGamepadFocusable> handle);
		if (componentsInChildrenNonAlloc.Count > 0)
		{
			componentsInChildrenNonAlloc.Sort((IGamepadFocusable a, IGamepadFocusable b) => ((Component)a).transform.GetSiblingIndex().CompareTo(((Component)b).transform.GetSiblingIndex()));
			foreach (IGamepadFocusable item in componentsInChildrenNonAlloc)
			{
				if (item.CanBeFocused())
				{
					if (focused != item)
					{
						SetFocus(item);
					}
					break;
				}
			}
		}
		handle.Return();
	}

	public void PlayTickSFX()
	{
		DewEffect.Play(fxTick);
	}

	public void PopulateCacheData(ref GetFocusTargetSettings settings, out Action handle)
	{
		UI_GamepadFocusableGroup uI_GamepadFocusableGroup = null;
		if (ManagerBase<GlobalUIManager>.instance.focused is Component component && component != null)
		{
			uI_GamepadFocusableGroup = component.GetComponentInParent<UI_GamepadFocusableGroup>();
		}
		List<(IGamepadFocusable, Rect)> list = DewPool.GetList(out ListReturnHandle<(IGamepadFocusable, Rect)> h0);
		foreach (IGamepadFocusable allFocusable in ManagerBase<GlobalUIManager>.instance._allFocusables)
		{
			if (allFocusable.IsValid() && allFocusable != ManagerBase<GlobalUIManager>.instance.focused && (settings.condition == null || settings.condition(allFocusable)))
			{
				Rect screenSpaceRect = ((RectTransform)((Component)allFocusable).transform).GetScreenSpaceRect();
				DewDebug.DrawRect(screenSpaceRect, Color.blue, 0.5f);
				list.Add((allFocusable, screenSpaceRect));
			}
		}
		List<(UI_GamepadFocusableGroup, Rect)> list2 = DewPool.GetList(out ListReturnHandle<(UI_GamepadFocusableGroup, Rect)> h1);
		if (settings.condition == null && settings.customScoreFunc == null)
		{
			foreach (UI_GamepadFocusableGroup allGroup in _allGroups)
			{
				if (!(uI_GamepadFocusableGroup == allGroup) && (!(uI_GamepadFocusableGroup != null) || !uI_GamepadFocusableGroup.transform.IsSelfOrDescendantOf(allGroup.transform)) && allGroup.isActiveAndEnabled)
				{
					Rect screenSpaceRect2 = ((RectTransform)allGroup.transform).GetScreenSpaceRect();
					DewDebug.DrawRect(screenSpaceRect2, Color.gray, 0.5f);
					DewDebug.DrawRect(RectExtensions.Expand(screenSpaceRect2, 1f), Color.gray, 0.5f);
					DewDebug.DrawRect(RectExtensions.Expand(screenSpaceRect2, 2f), Color.gray, 0.5f);
					list2.Add((allGroup, screenSpaceRect2));
				}
			}
		}
		if (list2.Count > 0)
		{
			for (int num = list.Count - 1; num >= 0; num--)
			{
				if (list[num].Item1 is Component component2)
				{
					UI_GamepadFocusableGroup componentInParent = component2.GetComponentInParent<UI_GamepadFocusableGroup>();
					if (componentInParent != null && componentInParent != uI_GamepadFocusableGroup)
					{
						list.RemoveAt(num);
					}
				}
			}
		}
		settings.cacheData = new GetFocusTargetCacheData
		{
			candidateFocusables = list,
			candidateGroups = list2
		};
		handle = () =>
		{
			if (h0.needToReturn)
			{
				h0.Return();
			}
			if (h1.needToReturn)
			{
				h1.Return();
			}
		};
	}

	public bool TryGetFocusTargetGeneric(Vector3 direction, Func<IGamepadFocusable, bool> condition, out IGamepadFocusable next)
	{
		GetFocusTargetSettings settings = new GetFocusTargetSettings
		{
			direction = direction,
			condition = condition
		};
		PopulateCacheData(ref settings, out var handle);
		settings.angleLimit = new Vector2(0f, 10f);
		settings.normalizedPerpendicularDistLimit = new Vector2(0f, 0.015f);
		settings.normalizedDistLimit = new Vector2(0f, 0.125f);
		bool flag = TryGetFocusTarget(settings, out next);
		if (!flag)
		{
			settings.angleLimit = new Vector2(0f, 30f);
			settings.normalizedPerpendicularDistLimit = new Vector2(0f, 0.015f);
			settings.normalizedDistLimit = new Vector2(0f, 0.3f);
			flag = TryGetFocusTarget(settings, out next);
		}
		if (!flag)
		{
			settings.angleLimit = new Vector2(0f, 45f);
			settings.normalizedPerpendicularDistLimit = new Vector2(0f, 0.35f);
			settings.normalizedDistLimit = new Vector2(0f, float.PositiveInfinity);
			flag = TryGetFocusTarget(settings, out next);
		}
		if (!flag)
		{
			settings.angleLimit = new Vector2(0f, 85f);
			settings.normalizedPerpendicularDistLimit = new Vector2(0f, float.PositiveInfinity);
			settings.normalizedDistLimit = new Vector2(0f, float.PositiveInfinity);
			settings.useCenterOfRect = true;
			TryGetFocusTarget(settings, out next);
		}
		handle();
		return next != null;
	}

	public bool TryGetFocusTarget(GetFocusTargetSettings s, out IGamepadFocusable next)
	{
		Component component = (Component)focused;
		Vector3 normalized = (component.transform.rotation * s.direction).WithZ(0f).normalized;
		Rect screenSpaceRect = ((RectTransform)component.transform).GetScreenSpaceRect();
		Vector2 vector;
		if (normalized.x > 0.5f)
		{
			if (normalized.y > 0.5f)
			{
				vector = new Vector2(screenSpaceRect.xMax + 1f, screenSpaceRect.yMax + 1f);
			}
			else
			{
				vector = ((!(normalized.y < -0.5f)) ? new Vector2(screenSpaceRect.xMax, screenSpaceRect.center.y) : new Vector2(screenSpaceRect.xMax + 1f, screenSpaceRect.yMin - 1f));
			}
		}
		else if (normalized.x < -0.5f)
		{
			if (normalized.y > 0.5f)
			{
				vector = new Vector2(screenSpaceRect.xMin - 1f, screenSpaceRect.yMax + 1f);
			}
			else
			{
				vector = ((!(normalized.y < -0.5f)) ? new Vector2(screenSpaceRect.xMin - 1f, screenSpaceRect.center.y) : new Vector2(screenSpaceRect.xMin - 1f, screenSpaceRect.yMin - 1f));
			}
		}
		else if (normalized.y > 0.5f)
		{
			vector = new Vector2(screenSpaceRect.center.x, screenSpaceRect.yMax + 1f);
		}
		else
		{
			vector = ((!(normalized.y < -0.5f)) ? new Vector2(screenSpaceRect.center.x, screenSpaceRect.center.y) : new Vector2(screenSpaceRect.center.x, screenSpaceRect.yMin - 1f));
		}
		List<float> list = DewPool.GetList(out ListReturnHandle<float> handle);
		Vector2 normalizedPerpendicularDistLimit = s.normalizedPerpendicularDistLimit;
		Vector2 normalizedDistLimit = s.normalizedDistLimit;
		if (Mathf.Abs(normalized.x) > Mathf.Abs(normalized.y))
		{
			normalizedPerpendicularDistLimit *= (float)Screen.height;
			normalizedDistLimit *= (float)Screen.width;
		}
		else
		{
			normalizedPerpendicularDistLimit *= (float)Screen.width;
			normalizedDistLimit *= (float)Screen.height;
		}
		normalizedDistLimit.y = Mathf.Min(normalizedDistLimit.y, 100000f);
		float num = 1f / Mathf.Cos(s.angleLimit.y * ((float)Math.PI / 180f));
		if (num > 9000f)
		{
			num = 9000f;
		}
		if (num < 0.1f)
		{
			num = 0.1f;
		}
		Vector2 vector2 = vector + RotateVector(normalized, 0f - s.angleLimit.y) * normalizedDistLimit.y * num;
		Vector2 vector3 = vector + RotateVector(normalized, s.angleLimit.y) * normalizedDistLimit.y * num;
		Color color = (s.useCenterOfRect ? Color.cyan : Color.magenta);
		Debug.DrawLine(vector, vector2, color, 0.5f);
		Debug.DrawLine(vector, vector3, color, 0.5f);
		Debug.DrawLine(vector3, vector2, color, 0.5f);
		Action handle2 = null;
		if (s.cacheData.candidateFocusables == null)
		{
			PopulateCacheData(ref s, out handle2);
		}
		foreach (var candidateFocusable in s.cacheData.candidateFocusables)
		{
			Rect item = candidateFocusable.Item2;
			if (s.useCenterOfRect)
			{
				if (IsPointInTriangle(item.center, vector, vector2, vector3) && candidateFocusable.Item1.CanBeFocused())
				{
					if (s.customScoreFunc != null)
					{
						list.Add(s.customScoreFunc(candidateFocusable.Item1));
					}
					else
					{
						list.Add(1f / Vector2.Distance(item.center, vector));
					}
					DewDebug.DrawRect(item, Color.green, 0.5f);
				}
				else
				{
					list.Add(-1f);
				}
			}
			else if (IsRectIntersectingTriangle(item, vector, vector2, vector3) && candidateFocusable.Item1.CanBeFocused())
			{
				if (s.customScoreFunc != null)
				{
					list.Add(s.customScoreFunc(candidateFocusable.Item1));
				}
				else
				{
					list.Add(1f / GetClosestPointDistance(item, vector));
				}
				DewDebug.DrawRect(item, Color.green, 0.5f);
			}
			else
			{
				list.Add(-1f);
			}
		}
		foreach (var candidateGroup in s.cacheData.candidateGroups)
		{
			Rect item2 = candidateGroup.Item2;
			if (s.useCenterOfRect)
			{
				if (IsPointInTriangle(item2.center, vector, vector2, vector3) && candidateGroup.Item1.IsValid())
				{
					list.Add(1f / Vector2.Distance(item2.center, vector));
					DewDebug.DrawRect(item2, Color.green, 0.5f);
				}
				else
				{
					list.Add(-1f);
				}
			}
			else if (IsRectIntersectingTriangle(item2, vector, vector2, vector3) && candidateGroup.Item1.IsValid())
			{
				list.Add(1f / GetClosestPointDistance(item2, vector));
				DewDebug.DrawRect(item2, Color.green, 0.5f);
			}
			else
			{
				list.Add(-1f);
			}
		}
		next = null;
		float num2 = float.NegativeInfinity;
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i] < 0f || list[i] < num2)
			{
				continue;
			}
			if (i >= s.cacheData.candidateFocusables.Count)
			{
				IGamepadFocusable enterFocusable = s.cacheData.candidateGroups[i - s.cacheData.candidateFocusables.Count].Item1.GetEnterFocusable(s.direction);
				if (enterFocusable != null)
				{
					next = enterFocusable;
					num2 = list[i];
				}
			}
			else
			{
				next = s.cacheData.candidateFocusables[i].Item1;
				num2 = list[i];
			}
		}
		handle2?.Invoke();
		handle.Return();
		return next != null;
	}

	public static List<RectResult> GetIntersectingRects(List<Rect> rects, Vector2 origin, Vector2 direction, float angle, float radius)
	{
		List<RectResult> list = new List<RectResult>();
		Vector2 b = origin + RotateVector(direction, (0f - angle) / 2f) * radius;
		Vector2 c = origin + RotateVector(direction, angle / 2f) * radius;
		foreach (Rect rect in rects)
		{
			if (IsRectIntersectingTriangle(rect, origin, b, c))
			{
				float closestPointDistance = GetClosestPointDistance(rect, origin);
				list.Add(new RectResult
				{
					rect = rect,
					distance = closestPointDistance
				});
			}
		}
		return list;
	}

	private static bool IsRectIntersectingTriangle(Rect rect, Vector2 a, Vector2 b, Vector2 c)
	{
		if (rect.Contains(a) || rect.Contains(b) || rect.Contains(c))
		{
			return true;
		}
		Vector2[] array = new Vector2[4]
		{
			new Vector2(rect.xMin, rect.yMin),
			new Vector2(rect.xMax, rect.yMin),
			new Vector2(rect.xMax, rect.yMax),
			new Vector2(rect.xMin, rect.yMax)
		};
		for (int i = 0; i < array.Length; i++)
		{
			if (IsPointInTriangle(array[i], a, b, c))
			{
				return true;
			}
		}
		if (LineIntersectsRect(a, b, rect) || LineIntersectsRect(b, c, rect) || LineIntersectsRect(c, a, rect))
		{
			return true;
		}
		return false;
	}

	private static bool IsPointInTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
	{
		float num = Sign(p, a, b);
		float num2 = Sign(p, b, c);
		float num3 = Sign(p, c, a);
		bool flag = num < 0f || num2 < 0f || num3 < 0f;
		bool flag2 = num > 0f || num2 > 0f || num3 > 0f;
		return !(flag & flag2);
	}

	private static float Sign(Vector2 p1, Vector2 p2, Vector2 p3)
	{
		return (p1.x - p3.x) * (p2.y - p3.y) - (p2.x - p3.x) * (p1.y - p3.y);
	}

	private static bool LineIntersectsRect(Vector2 p1, Vector2 p2, Rect r)
	{
		if (!LineIntersectsLine(p1, p2, new Vector2(r.xMin, r.yMin), new Vector2(r.xMax, r.yMin)) && !LineIntersectsLine(p1, p2, new Vector2(r.xMax, r.yMin), new Vector2(r.xMax, r.yMax)) && !LineIntersectsLine(p1, p2, new Vector2(r.xMax, r.yMax), new Vector2(r.xMin, r.yMax)))
		{
			return LineIntersectsLine(p1, p2, new Vector2(r.xMin, r.yMax), new Vector2(r.xMin, r.yMin));
		}
		return true;
	}

	private static bool LineIntersectsLine(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2)
	{
		Vector2 vector = a2 - a1;
		Vector2 vector2 = b2 - b1;
		float num = vector.x * vector2.y - vector.y * vector2.x;
		if (num == 0f)
		{
			return false;
		}
		Vector2 vector3 = b1 - a1;
		float num2 = (vector3.x * vector2.y - vector3.y * vector2.x) / num;
		if (num2 < 0f || num2 > 1f)
		{
			return false;
		}
		float num3 = (vector3.x * vector.y - vector3.y * vector.x) / num;
		if (num3 < 0f || num3 > 1f)
		{
			return false;
		}
		return true;
	}

	private static float GetClosestPointDistance(Rect rect, Vector2 origin)
	{
		return Vector2.Distance(b: new Vector2(Mathf.Clamp(origin.x, rect.xMin, rect.xMax), Mathf.Clamp(origin.y, rect.yMin, rect.yMax)), a: origin);
	}

	private static Vector2 RotateVector(Vector2 vector, float degrees)
	{
		float f = degrees * ((float)Math.PI / 180f);
		float num = Mathf.Sin(f);
		float num2 = Mathf.Cos(f);
		return new Vector2(vector.x * num2 - vector.y * num, vector.x * num + vector.y * num2);
	}

	public IEnumerator HighlightForTutorial(TutorialHighlightSettings s)
	{
		if (!(s.target == null))
		{
			_highlightStartUnscaledTime = Time.unscaledTime;
			_currentHighlight = s;
			_didAddContinueText = false;
			((TMP_Text)tutHighlightText).text = _currentHighlight.rawText + "<size=80%>\n<color=#ccc> ";
			_anchoredPosCv = Vector2.zero;
			_sizeDeltaCv = Vector2.zero;
			UpdateCurrentHighlight(Time.frameCount - _lastHighlightFrame > 2);
			DewEffect.Play(fxHighlightShow);
			yield return new WaitWhile(() => _currentHighlight == s);
		}
	}

	public void ClickHighlightForTutorial()
	{
		if (_currentHighlight != null && !(Time.unscaledTime - _highlightStartUnscaledTime < 0.75f))
		{
			UnhighlightForTutorial();
			DewEffect.Play(fxHighlightHide);
		}
	}

	public void UnhighlightForTutorial()
	{
		if (_currentHighlight != null)
		{
			_currentHighlight = null;
			tutHighlightObject.SetActive(value: false);
		}
	}

	private void UnhighlightForTutorial_Imp(Scene arg0, LoadSceneMode arg1)
	{
		UnhighlightForTutorial();
	}

	private void Start_Tutorial()
	{
		SceneManager.sceneLoaded += UnhighlightForTutorial_Imp;
		AddBackHandler(this, 99999, () => isTutorialHighlighting);
	}

	private void OnDestroy_Tutorial()
	{
		SceneManager.sceneLoaded -= UnhighlightForTutorial_Imp;
	}

	private void FrameUpdate_Tutorial()
	{
		if (_currentHighlight != null)
		{
			_lastHighlightFrame = Time.frameCount;
			UpdateCurrentHighlight(immediately: false);
		}
	}

	private void UpdateCurrentHighlight(bool immediately)
	{
		if (_currentHighlight.target == null)
		{
			UnhighlightForTutorial();
			return;
		}
		if (!_didAddContinueText && Time.unscaledTime - _highlightStartUnscaledTime > 0.75f)
		{
			_didAddContinueText = true;
			if (DewInput.currentMode == InputMode.KeyboardAndMouse)
			{
				((TMP_Text)tutHighlightText).text = ((TMP_Text)tutHighlightText).text.Substring(0, ((TMP_Text)tutHighlightText).text.Length - 1) + DewLocalization.GetUIValue("Generic_TutorialHighlight_Continue_PC");
			}
			else
			{
				string uIValue = DewLocalization.GetUIValue("Generic_TutorialHighlight_Continue_Gamepad");
				uIValue = string.Format(uIValue, GamepadButtonEx.A.GetReadableText());
				((TMP_Text)tutHighlightText).text = ((TMP_Text)tutHighlightText).text.Substring(0, ((TMP_Text)tutHighlightText).text.Length - 1) + uIValue;
			}
		}
		tutHighlightObject.SetActive(value: true);
		float num = 1f / tutHighlightBackdropLeft.lossyScale.x;
		Rect screenSpaceRect = _currentHighlight.target.GetScreenSpaceRect();
		screenSpaceRect.yMax += _currentHighlight.padding.x;
		screenSpaceRect.xMax += _currentHighlight.padding.y;
		screenSpaceRect.yMin -= _currentHighlight.padding.z;
		screenSpaceRect.xMin -= _currentHighlight.padding.w;
		Vector2 target = new Vector2(screenSpaceRect.x * num, screenSpaceRect.y * num);
		Vector2 target2 = new Vector2(screenSpaceRect.width * num, screenSpaceRect.height * num);
		if (immediately)
		{
			tutHighlightBox.anchoredPosition = new Vector2(target.x - 60f, target.y - 60f);
			tutHighlightBox.sizeDelta = new Vector2(target2.x + 120f, target2.y + 120f);
		}
		tutHighlightBox.anchoredPosition = Vector2.SmoothDamp(tutHighlightBox.anchoredPosition, target, ref _anchoredPosCv, 0.1f, float.PositiveInfinity, Time.unscaledDeltaTime);
		tutHighlightBox.sizeDelta = Vector2.SmoothDamp(tutHighlightBox.sizeDelta, target2, ref _sizeDeltaCv, 0.1f, float.PositiveInfinity, Time.unscaledDeltaTime);
		screenSpaceRect = tutHighlightBox.GetScreenSpaceRect();
		Vector2 vector = screenSpaceRect.min * num;
		Vector2 vector2 = screenSpaceRect.max * num;
		Vector2 vector3 = (vector + vector2) * 0.5f;
		float num2 = (float)Screen.height * num;
		float num3 = (float)Screen.width * num;
		tutHighlightBackdropLeft.anchoredPosition = new Vector2(0f, 0f);
		tutHighlightBackdropLeft.sizeDelta = new Vector2(vector.x, num2);
		tutHighlightBackdropRight.anchoredPosition = new Vector2(vector2.x, 0f);
		tutHighlightBackdropRight.sizeDelta = new Vector2(num3 - vector.x, num2);
		tutHighlightBackdropTop.anchoredPosition = new Vector2(vector.x, vector2.y);
		tutHighlightBackdropTop.sizeDelta = new Vector2(vector2.x - vector.x, num2 - vector2.y);
		tutHighlightBackdropBottom.anchoredPosition = new Vector2(vector.x, 0f);
		tutHighlightBackdropBottom.sizeDelta = new Vector2(vector2.x - vector.x, vector.y);
		RectTransform rectTransform = (RectTransform)((TMP_Text)tutHighlightText).transform;
		float num4 = 35f;
		TutorialHighlightTextPlacement textPlacement = _currentHighlight.textPlacement;
		bool flag = textPlacement == TutorialHighlightTextPlacement.Left || textPlacement == TutorialHighlightTextPlacement.Right;
		float num5 = Mathf.Min(vector3.x, num3 - vector3.x, 700f) * 2f - 100f;
		float num6 = Mathf.Min(vector3.y, num2 - vector3.y, 700f) * 2f - 100f;
		if (flag)
		{
			num5 = 850f;
		}
		else
		{
			num6 = 850f;
		}
		switch (_currentHighlight.textPlacement)
		{
		case TutorialHighlightTextPlacement.Left:
			((TMP_Text)tutHighlightText).verticalAlignment = (VerticalAlignmentOptions)512;
			((TMP_Text)tutHighlightText).horizontalAlignment = (HorizontalAlignmentOptions)4;
			rectTransform.sizeDelta = new Vector2(num5, num6);
			rectTransform.anchoredPosition = new Vector2(vector.x - num5 - num4, vector3.y - num6 * 0.5f);
			break;
		case TutorialHighlightTextPlacement.Right:
			((TMP_Text)tutHighlightText).verticalAlignment = (VerticalAlignmentOptions)512;
			((TMP_Text)tutHighlightText).horizontalAlignment = (HorizontalAlignmentOptions)1;
			rectTransform.sizeDelta = new Vector2(num5, num6);
			rectTransform.anchoredPosition = new Vector2(vector2.x + num4, vector3.y - num6 * 0.5f);
			break;
		case TutorialHighlightTextPlacement.Top:
			((TMP_Text)tutHighlightText).verticalAlignment = (VerticalAlignmentOptions)1024;
			((TMP_Text)tutHighlightText).horizontalAlignment = (HorizontalAlignmentOptions)2;
			rectTransform.sizeDelta = new Vector2(num5, num2);
			rectTransform.anchoredPosition = new Vector2(vector3.x - num5 * 0.5f, vector2.y + num4);
			break;
		case TutorialHighlightTextPlacement.Bottom:
			((TMP_Text)tutHighlightText).verticalAlignment = (VerticalAlignmentOptions)256;
			((TMP_Text)tutHighlightText).horizontalAlignment = (HorizontalAlignmentOptions)2;
			rectTransform.sizeDelta = new Vector2(num5, num2);
			rectTransform.anchoredPosition = new Vector2(vector3.x - num5 * 0.5f, vector.y - num4 - num2);
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	private void LogicUpdate_Tutorial()
	{
	}
}
