using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MessageManager : ManagerBase<MessageManager>
{
	private const int CustomButtonStartIndex = 4;

	public TextMeshProUGUI contentText;

	public CanvasGroup canvasGroup;

	public List<GameObject> buttons;

	private DewMessageSettings _currentMessage;

	private Queue<DewMessageSettings> _messageQueue = new Queue<DewMessageSettings>();

	private DewInputTrigger it_left;

	private DewInputTrigger it_up;

	private DewInputTrigger it_right;

	private DewInputTrigger it_down;

	private DewInputTrigger it_confirm;

	private GameObject _selectedButton;

	private int _navigateFrame;

	private const float CustomButtonMinWidth = 750f;

	private const float CustomButtonHeight = 72f;

	private const float CustomButtonsMaxRowWidth = 1500f;

	private RectTransform _buttonGroupTransform;

	private Vector2 _defaultButtonSize;

	private float _defaultButtonSpacing = 20f;

	private HorizontalOrVerticalLayoutGroup _buttonLayout;

	private RectOffset _horizontalPadding;

	private float _horizontalSpacing;

	private TextAnchor _horizontalAlignment;

	private bool _horizontalControlWidth;

	private bool _horizontalControlHeight;

	private bool _horizontalScaleWidth;

	private bool _horizontalScaleHeight;

	private bool _horizontalExpandWidth;

	private bool _horizontalExpandHeight;

	private const float VerticalButtonSpacing = 10f;

	private RectTransform _customButtonsRootTransform;

	private LayoutElement _customButtonsLayoutElement;

	private GridLayoutGroup _customButtonsGridLayout;

	private readonly List<GameObject> _customButtonObjects = new List<GameObject>();

	private BackHandler _backHandler;

	public bool isShowingMessage => _currentMessage != null;

	private void Start()
	{
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0273: Unknown result type (might be due to invalid IL or missing references)
		//IL_0278: Unknown result type (might be due to invalid IL or missing references)
		UpdateVisibility();
		it_left = MessageControl(() => DewSave.profileMain.controls.moveLeft.CloneWith((Key)61));
		it_right = MessageControl(() => DewSave.profileMain.controls.moveLeft.CloneWith((Key)62));
		it_up = MessageControl(() => DewSave.profileMain.controls.moveLeft.CloneWith((Key)63));
		it_down = MessageControl(() => DewSave.profileMain.controls.moveLeft.CloneWith((Key)64));
		it_confirm = MessageControl(() => DewBinding.KeyboardOnly((object)(Key)2, (object)(Key)1));
		foreach (GameObject button in buttons)
		{
			if (!button.TryGetComponent<Button>(out var btn))
			{
				continue;
			}
			UI_PointerEvents uI_PointerEvents = ((Component)(object)btn).gameObject.AddComponent<UI_PointerEvents>();
			uI_PointerEvents.onPointerEnter.AddListener(() =>
			{
				if (_navigateFrame != Time.frameCount)
				{
					SetSelectedButton(null);
					_selectedButton = ((Component)(object)btn).gameObject;
				}
			});
			uI_PointerEvents.onPointerExit.AddListener(() =>
			{
				if (_navigateFrame != Time.frameCount && _selectedButton == ((Component)(object)btn).gameObject)
				{
					_selectedButton = null;
				}
			});
			Button val = btn;
			Navigation navigation = default;
			navigation.mode = (Mode)0;
			((Selectable)val).navigation = navigation;
		}
		if (buttons.Count > 0)
		{
			_buttonGroupTransform = buttons[0].transform.parent as RectTransform;
			_defaultButtonSize = ((RectTransform)buttons[0].transform).sizeDelta;
			LayoutGroup val2 = ((_buttonGroupTransform != null) ? _buttonGroupTransform.GetComponent<LayoutGroup>() : null);
			HorizontalOrVerticalLayoutGroup val3 = (HorizontalOrVerticalLayoutGroup)(object)((val2 is HorizontalOrVerticalLayoutGroup) ? val2 : null);
			if (val3 != null)
			{
				_defaultButtonSpacing = val3.spacing;
			}
			_buttonLayout = (HorizontalOrVerticalLayoutGroup)(object)((val2 is HorizontalOrVerticalLayoutGroup) ? val2 : null);
			if ((UnityEngine.Object)(object)_buttonLayout != null)
			{
				RectOffset padding = ((LayoutGroup)_buttonLayout).padding;
				_horizontalPadding = new RectOffset(padding.left, padding.right, padding.top, padding.bottom);
				_horizontalSpacing = _buttonLayout.spacing;
				_horizontalAlignment = ((LayoutGroup)_buttonLayout).childAlignment;
				_horizontalControlWidth = _buttonLayout.childControlWidth;
				_horizontalControlHeight = _buttonLayout.childControlHeight;
				_horizontalScaleWidth = _buttonLayout.childScaleWidth;
				_horizontalScaleHeight = _buttonLayout.childScaleHeight;
				_horizontalExpandWidth = _buttonLayout.childForceExpandWidth;
				_horizontalExpandHeight = _buttonLayout.childForceExpandHeight;
			}
		}
		DewInputTrigger MessageControl(Func<DewBinding> binding)
		{
			return new DewInputTrigger
			{
				owner = this,
				priority = -100,
				canConsume = true,
				isValidCheck = () => isShowingMessage,
				checkGameAreaForMouse = false,
				binding = binding
			};
		}
	}

	public void ShowMessage(DewMessageSettings msg)
	{
		_messageQueue.Enqueue(msg);
		if (!isShowingMessage)
		{
			DequeueMessage();
		}
	}

	public void ShowMessageLocalized(string id)
	{
		ShowMessage(new DewMessageSettings
		{
			rawContent = DewLocalization.GetUIValue(id)
		});
	}

	private void DequeueMessage()
	{
		if (_messageQueue.Count <= 0)
		{
			return;
		}
		_currentMessage = _messageQueue.Dequeue();
		((TMP_Text)contentText).text = _currentMessage.rawContent;
		ApplyButtonsOrientation(_currentMessage.verticalButtons && !IsUsingCustomButtons());
		if (IsUsingCustomButtons())
		{
			ConfigureCustomButtons();
		}
		else
		{
			HideCustomButtons();
			for (int i = 0; i < buttons.Count; i++)
			{
				int num = Mathf.RoundToInt(Mathf.Pow(2f, i));
				bool flag = ((uint)_currentMessage.buttons & (uint)num) != 0;
				buttons[i].SetActive(flag);
				if (flag && i >= 4)
				{
					string[] customButtonTexts = _currentMessage.customButtonTexts;
					int num2 = i - 4;
					string text = ((customButtonTexts != null && num2 < customButtonTexts.Length) ? customButtonTexts[num2] : "");
					TextMeshProUGUI componentInChildren = buttons[i].GetComponentInChildren<TextMeshProUGUI>(includeInactive: true);
					if ((UnityEngine.Object)(object)componentInChildren != null)
					{
						((TMP_Text)componentInChildren).text = text;
					}
				}
			}
		}
		UpdateVisibility();
		SetSelectedButton(null);
		if (IsUsingCustomButtons() && ManagerBase<GlobalUIManager>.instance != null)
		{
			_backHandler = ManagerBase<GlobalUIManager>.instance.AddBackHandler(this, 10000, () =>
			{
				if (_currentMessage == null)
				{
					return false;
				}
				int num3 = _currentMessage.defaultCustomButtonIndex;
				if (num3 < 0 || num3 >= _currentMessage.customButtons.Count)
				{
					num3 = _currentMessage.customButtons.Count - 1;
				}
				CloseCustomMessage(num3);
				return true;
			});
		}
		Dew.CallDelayed(() =>
		{
			if (DewInput.currentMode == InputMode.KeyboardAndMouse)
			{
				_navigateFrame = Time.frameCount;
				SetSelectedButton(GetDefaultButton());
			}
			else if (ManagerBase<GlobalUIManager>.instance != null && GetDefaultButton() != null)
			{
				ManagerBase<GlobalUIManager>.instance.SetFocus(GetDefaultButton().GetComponent<IGamepadFocusable>());
			}
		});
	}

	private GameObject GetDefaultButton()
	{
		if (IsUsingCustomButtons())
		{
			int defaultCustomButtonIndex = _currentMessage.defaultCustomButtonIndex;
			if (defaultCustomButtonIndex >= 0 && defaultCustomButtonIndex < _customButtonObjects.Count && _customButtonObjects[defaultCustomButtonIndex] != null && _customButtonObjects[defaultCustomButtonIndex].activeInHierarchy)
			{
				return _customButtonObjects[defaultCustomButtonIndex];
			}
			GameObject result = null;
			for (int i = 0; i < _customButtonObjects.Count; i++)
			{
				if (_customButtonObjects[i] != null && _customButtonObjects[i].activeInHierarchy)
				{
					result = _customButtonObjects[i];
				}
			}
			return result;
		}
		GameObject result2 = null;
		for (int j = 0; j < buttons.Count; j++)
		{
			if (buttons[j].activeInHierarchy)
			{
				int num = Mathf.RoundToInt(Mathf.Pow(2f, j));
				if (_currentMessage.defaultButton == (DewMessageSettings.ButtonType)num)
				{
					return buttons[j];
				}
				result2 = buttons[j];
			}
		}
		return result2;
	}

	public void CloseMessageCustom0()
	{
		CloseMessage(DewMessageSettings.ButtonType.Custom0);
	}

	public void CloseMessageCustom1()
	{
		CloseMessage(DewMessageSettings.ButtonType.Custom1);
	}

	public void CloseMessageCustom2()
	{
		CloseMessage(DewMessageSettings.ButtonType.Custom2);
	}

	public void CloseMessageCustom3()
	{
		CloseMessage(DewMessageSettings.ButtonType.Custom3);
	}

	public void CloseMessageOk()
	{
		CloseMessage(DewMessageSettings.ButtonType.Ok);
	}

	public void CloseMessageYes()
	{
		CloseMessage(DewMessageSettings.ButtonType.Yes);
	}

	public void CloseMessageNo()
	{
		CloseMessage(DewMessageSettings.ButtonType.No);
	}

	public void CloseMessageCancel()
	{
		CloseMessage(DewMessageSettings.ButtonType.Cancel);
	}

	public void CloseMessage(DewMessageSettings.ButtonType button)
	{
		if (_currentMessage == null)
		{
			return;
		}
		if (_currentMessage.IsValid())
		{
			try
			{
				_currentMessage.onClose?.Invoke(button);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
		_backHandler?.Remove();
		_backHandler = null;
		for (int i = 0; i < buttons.Count; i++)
		{
			buttons[i].SetActive(value: false);
		}
		HideCustomButtons();
		buttons[0].transform.parent.parent.GetComponent<DewContentSizeFitter>().maxWidth = 2000f;
		_currentMessage = null;
		if (_messageQueue.Count <= 0)
		{
			UpdateVisibility();
		}
		else
		{
			DequeueMessage();
		}
	}

	private void UpdateVisibility()
	{
		if (isShowingMessage)
		{
			((Component)(object)canvasGroup).gameObject.SetActive(value: true);
			canvasGroup.interactable = true;
			canvasGroup.blocksRaycasts = true;
			canvasGroup.alpha = 1f;
		}
		else
		{
			((Component)(object)canvasGroup).gameObject.SetActive(value: false);
			canvasGroup.interactable = false;
			canvasGroup.blocksRaycasts = false;
			canvasGroup.alpha = 0f;
		}
	}

	public override void LogicUpdate(float dt)
	{
		base.LogicUpdate(dt);
		if (isShowingMessage && !_currentMessage.IsValid())
		{
			CloseMessage(DewMessageSettings.ButtonType.Cancel);
		}
	}

	public override void FrameUpdate()
	{
		base.FrameUpdate();
		if (!isShowingMessage)
		{
			return;
		}
		if (IsUsingCustomButtons())
		{
			if (it_left.down)
			{
				HandleGridNavigation(Vector2.left);
			}
			if (it_right.down)
			{
				HandleGridNavigation(Vector2.right);
			}
			if (it_up.down)
			{
				HandleGridNavigation(Vector2.up);
			}
			if (it_down.down)
			{
				HandleGridNavigation(Vector2.down);
			}
		}
		else if (_currentMessage.verticalButtons)
		{
			if (it_up.down)
			{
				HandleNavigation(isPrev: true);
			}
			if (it_down.down)
			{
				HandleNavigation(isPrev: false);
			}
		}
		else
		{
			if (it_left.down)
			{
				HandleNavigation(isPrev: true);
			}
			if (it_right.down)
			{
				HandleNavigation(isPrev: false);
			}
		}
		if (it_confirm.down)
		{
			if (_selectedButton == null)
			{
				_selectedButton = GetDefaultButton();
			}
			if (_selectedButton != null)
			{
				ManagerBase<GlobalUIManager>.instance.SimulateClickOnUIElement((Component)(object)_selectedButton.GetComponent<Button>());
			}
		}
	}

	private void SetSelectedButton(GameObject button)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected Obj, but got Unknown
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Expected Obj, but got Unknown
		if (_selectedButton == button)
		{
			return;
		}
		if (_selectedButton != null)
		{
			PointerEventData val = new PointerEventData(EventSystem.current);
			IPointerExitHandler[] components = _selectedButton.GetComponents<IPointerExitHandler>();
			for (int i = 0; i < components.Length; i++)
			{
				components[i].OnPointerExit(val);
			}
		}
		_selectedButton = button;
		if (_selectedButton != null)
		{
			PointerEventData val2 = new PointerEventData(EventSystem.current);
			IPointerEnterHandler[] components2 = _selectedButton.GetComponents<IPointerEnterHandler>();
			for (int i = 0; i < components2.Length; i++)
			{
				components2[i].OnPointerEnter(val2);
			}
		}
	}

	private void HandleNavigation(bool isPrev)
	{
		_navigateFrame = Time.frameCount;
		if (_selectedButton == null)
		{
			SetSelectedButton(GetDefaultButton());
			return;
		}
		bool vertical = _currentMessage != null && _currentMessage.verticalButtons;
		float num = Pos(_selectedButton);
		float num2 = float.PositiveInfinity;
		GameObject gameObject = null;
		for (int i = 0; i < buttons.Count; i++)
		{
			if (buttons[i] == _selectedButton || !buttons[i].activeInHierarchy)
			{
				continue;
			}
			float num3 = Pos(buttons[i]);
			if ((isPrev && num3 < num) || (!isPrev && num3 > num))
			{
				float num4 = Mathf.Abs(num3 - num);
				if (num4 < num2)
				{
					gameObject = buttons[i];
					num2 = num4;
				}
			}
		}
		if (gameObject != null)
		{
			SetSelectedButton(gameObject);
		}
		float Pos(GameObject go)
		{
			if (!vertical)
			{
				return go.transform.position.x;
			}
			return 0f - go.transform.position.y;
		}
	}

	private void ApplyButtonsOrientation(bool vertical)
	{
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		if (!(_buttonGroupTransform == null) && !((UnityEngine.Object)(object)_buttonLayout == null) && vertical != _buttonLayout is VerticalLayoutGroup)
		{
			UnityEngine.Object.DestroyImmediate((UnityEngine.Object)(object)_buttonLayout);
			if (vertical)
			{
				VerticalLayoutGroup val = _buttonGroupTransform.gameObject.AddComponent<VerticalLayoutGroup>();
				((LayoutGroup)val).padding = new RectOffset(0, 0, 0, 0);
				((HorizontalOrVerticalLayoutGroup)val).spacing = 10f;
				((LayoutGroup)val).childAlignment = (TextAnchor)4;
				((HorizontalOrVerticalLayoutGroup)val).childControlWidth = true;
				((HorizontalOrVerticalLayoutGroup)val).childControlHeight = true;
				((HorizontalOrVerticalLayoutGroup)val).childScaleWidth = true;
				((HorizontalOrVerticalLayoutGroup)val).childScaleHeight = true;
				((HorizontalOrVerticalLayoutGroup)val).childForceExpandWidth = true;
				((HorizontalOrVerticalLayoutGroup)val).childForceExpandHeight = true;
				_buttonLayout = (HorizontalOrVerticalLayoutGroup)(object)val;
			}
			else
			{
				HorizontalLayoutGroup val2 = _buttonGroupTransform.gameObject.AddComponent<HorizontalLayoutGroup>();
				((LayoutGroup)val2).padding = _horizontalPadding;
				((HorizontalOrVerticalLayoutGroup)val2).spacing = _horizontalSpacing;
				((LayoutGroup)val2).childAlignment = _horizontalAlignment;
				((HorizontalOrVerticalLayoutGroup)val2).childControlWidth = _horizontalControlWidth;
				((HorizontalOrVerticalLayoutGroup)val2).childControlHeight = _horizontalControlHeight;
				((HorizontalOrVerticalLayoutGroup)val2).childScaleWidth = _horizontalScaleWidth;
				((HorizontalOrVerticalLayoutGroup)val2).childScaleHeight = _horizontalScaleHeight;
				((HorizontalOrVerticalLayoutGroup)val2).childForceExpandWidth = _horizontalExpandWidth;
				((HorizontalOrVerticalLayoutGroup)val2).childForceExpandHeight = _horizontalExpandHeight;
				_buttonLayout = (HorizontalOrVerticalLayoutGroup)(object)val2;
			}
			LayoutRebuilder.ForceRebuildLayoutImmediate(_buttonGroupTransform);
		}
	}

	private bool IsUsingCustomButtons()
	{
		if (_currentMessage != null && _currentMessage.customButtons != null)
		{
			return _currentMessage.customButtons.Count > 0;
		}
		return false;
	}

	private void ConfigureCustomButtons()
	{
		for (int i = 0; i < buttons.Count; i++)
		{
			buttons[i].SetActive(value: false);
		}
		int count = _currentMessage.customButtons.Count;
		EnsureCustomButtonPool(count);
		int customButtonsColumnCount = _currentMessage.customButtonsColumnCount;
		int num = ((customButtonsColumnCount <= 0) ? 1 : Mathf.Clamp(customButtonsColumnCount, 1, count));
		float max = ((_defaultButtonSize.x > 0f) ? _defaultButtonSize.x : 1500f);
		float num2 = Mathf.Clamp((1500f - _defaultButtonSpacing * (float)Mathf.Max(0, num - 1)) / (float)num, 750f, max);
		UseCustomButtonGridLayout(num, num2, count);
		for (int j = 0; j < _customButtonObjects.Count; j++)
		{
			if (!(_customButtonObjects[j] == null))
			{
				if (j < count)
				{
					ConfigureCustomButton(_customButtonObjects[j], _currentMessage.customButtons[j], j, num2);
					_customButtonObjects[j].SetActive(value: true);
				}
				else
				{
					_customButtonObjects[j].SetActive(value: false);
				}
			}
		}
		if (_customButtonsRootTransform != null && _customButtonsRootTransform.gameObject.activeSelf)
		{
			LayoutRebuilder.ForceRebuildLayoutImmediate(_customButtonsRootTransform);
		}
		if (_buttonGroupTransform != null)
		{
			LayoutRebuilder.ForceRebuildLayoutImmediate(_buttonGroupTransform);
		}
	}

	private void ConfigureCustomButton(GameObject buttonObject, DewMessageSettings.CustomButton customButton, int index, float width)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected Obj, but got Unknown
		if (buttonObject.TryGetComponent<Button>(out var component))
		{
			component.onClick = new ButtonClickedEvent();
			((UnityEvent)(object)component.onClick).AddListener((UnityAction)(() =>
			{
				CloseCustomMessage(index);
			}));
			SetCustomButtonLabel(buttonObject, customButton.label);
			SetCustomButtonSize(buttonObject, width, 72f);
		}
	}

	private void SetCustomButtonLabel(GameObject buttonObject, string label)
	{
		DewLocalizedText componentInChildren = buttonObject.GetComponentInChildren<DewLocalizedText>(includeInactive: true);
		if (componentInChildren != null)
		{
			UnityEngine.Object.Destroy(componentInChildren);
		}
		TMP_Text componentInChildren2 = buttonObject.GetComponentInChildren<TMP_Text>(includeInactive: true);
		if (!((UnityEngine.Object)(object)componentInChildren2 == null))
		{
			componentInChildren2.enableWordWrapping = true;
			componentInChildren2.overflowMode = (TextOverflowModes)0;
			componentInChildren2.text = label;
		}
	}

	private void SetCustomButtonSize(GameObject buttonObject, float width, float height)
	{
		if (!(buttonObject == null))
		{
			RectTransform component = buttonObject.GetComponent<RectTransform>();
			if (!(component == null))
			{
				component.sizeDelta = new Vector2(width, height);
			}
		}
	}

	private void EnsureCustomButtonPool(int count)
	{
		if (buttons.Count == 0)
		{
			return;
		}
		EnsureCustomButtonsRoot();
		RectTransform rectTransform = ((_customButtonsRootTransform != null) ? _customButtonsRootTransform : _buttonGroupTransform);
		if (rectTransform == null)
		{
			return;
		}
		while (_customButtonObjects.Count < count)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(buttons[0], rectTransform, worldPositionStays: false);
			gameObject.name = $"Custom Message Button {_customButtonObjects.Count}";
			_customButtonObjects.Add(gameObject);
			InitializeCustomButton(gameObject);
		}
		for (int i = 0; i < _customButtonObjects.Count; i++)
		{
			if (!(_customButtonObjects[i] == null))
			{
				_customButtonObjects[i].transform.SetParent(rectTransform, worldPositionStays: false);
				_customButtonObjects[i].transform.SetSiblingIndex(i);
			}
		}
	}

	private void InitializeCustomButton(GameObject buttonObject)
	{
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		if (!buttonObject.TryGetComponent<Button>(out var btn))
		{
			return;
		}
		UI_PointerEvents uI_PointerEvents = ((Component)(object)btn).gameObject.GetComponent<UI_PointerEvents>();
		if (uI_PointerEvents == null)
		{
			uI_PointerEvents = ((Component)(object)btn).gameObject.AddComponent<UI_PointerEvents>();
		}
		uI_PointerEvents.onPointerClick = new UnityEvent();
		uI_PointerEvents.onPointerDown = new UnityEvent();
		uI_PointerEvents.onPointerUp = new UnityEvent();
		uI_PointerEvents.onPointerEnter = new UnityEvent();
		uI_PointerEvents.onPointerExit = new UnityEvent();
		uI_PointerEvents.onPointerEnter.AddListener(() =>
		{
			if (_navigateFrame != Time.frameCount)
			{
				SetSelectedButton(null);
				_selectedButton = ((Component)(object)btn).gameObject;
			}
		});
		uI_PointerEvents.onPointerExit.AddListener(() =>
		{
			if (_navigateFrame != Time.frameCount && _selectedButton == ((Component)(object)btn).gameObject)
			{
				_selectedButton = null;
			}
		});
		Button val = btn;
		Navigation navigation = default;
		navigation.mode = (Mode)0;
		((Selectable)val).navigation = navigation;
	}

	private bool EnsureCustomButtonsRoot()
	{
		if (_buttonGroupTransform == null)
		{
			return false;
		}
		if (_customButtonsRootTransform == null)
		{
			GameObject gameObject = new GameObject("Custom Buttons Grid", typeof(RectTransform), typeof(LayoutElement), typeof(GridLayoutGroup));
			gameObject.transform.SetParent(_buttonGroupTransform, worldPositionStays: false);
			_customButtonsRootTransform = gameObject.GetComponent<RectTransform>();
			_customButtonsLayoutElement = gameObject.GetComponent<LayoutElement>();
			_customButtonsGridLayout = gameObject.GetComponent<GridLayoutGroup>();
			_customButtonsRootTransform.SetSiblingIndex(0);
			_customButtonsRootTransform.gameObject.SetActive(value: false);
		}
		if (_customButtonsRootTransform != null && (UnityEngine.Object)(object)_customButtonsLayoutElement != null)
		{
			return (UnityEngine.Object)(object)_customButtonsGridLayout != null;
		}
		return false;
	}

	private bool UseCustomButtonGridLayout(int columnCount, float cellWidth, int buttonCount)
	{
		buttons[0].transform.parent.parent.GetComponent<DewContentSizeFitter>().maxWidth = 3000f;
		if (!EnsureCustomButtonsRoot())
		{
			return false;
		}
		int num = Mathf.CeilToInt((float)buttonCount / (float)columnCount);
		float num2 = (float)columnCount * cellWidth + _defaultButtonSpacing * (float)Mathf.Max(0, columnCount - 1);
		float num3 = (float)num * 72f + _defaultButtonSpacing * (float)Mathf.Max(0, num - 1);
		_customButtonsRootTransform.gameObject.SetActive(value: true);
		((Behaviour)(object)_customButtonsGridLayout).enabled = true;
		_customButtonsGridLayout.startCorner = (Corner)0;
		_customButtonsGridLayout.startAxis = (Axis)0;
		((LayoutGroup)_customButtonsGridLayout).childAlignment = (TextAnchor)4;
		_customButtonsGridLayout.constraint = (Constraint)1;
		_customButtonsGridLayout.constraintCount = columnCount;
		_customButtonsGridLayout.spacing = new Vector2(_defaultButtonSpacing, _defaultButtonSpacing);
		_customButtonsGridLayout.cellSize = new Vector2(cellWidth, 72f);
		_customButtonsLayoutElement.minWidth = num2;
		_customButtonsLayoutElement.preferredWidth = num2;
		_customButtonsLayoutElement.minHeight = num3;
		_customButtonsLayoutElement.preferredHeight = num3;
		_customButtonsRootTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, num2);
		_customButtonsRootTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, num3);
		return true;
	}

	private void HideCustomButtonsRoot()
	{
		if ((UnityEngine.Object)(object)_customButtonsGridLayout != null)
		{
			((Behaviour)(object)_customButtonsGridLayout).enabled = false;
		}
		if (_customButtonsRootTransform != null)
		{
			_customButtonsRootTransform.gameObject.SetActive(value: false);
		}
	}

	private void HideCustomButtons()
	{
		for (int i = 0; i < _customButtonObjects.Count; i++)
		{
			if (_customButtonObjects[i] != null)
			{
				_customButtonObjects[i].SetActive(value: false);
			}
		}
		HideCustomButtonsRoot();
	}

	private void CloseCustomMessage(int index)
	{
		if (_currentMessage == null || !IsUsingCustomButtons())
		{
			return;
		}
		if (_currentMessage.IsValid() && index >= 0 && index < _currentMessage.customButtons.Count)
		{
			try
			{
				_currentMessage.customButtons[index].onClick?.Invoke();
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
		CloseMessage(DewMessageSettings.ButtonType.None);
	}

	private void HandleGridNavigation(Vector2 direction)
	{
		_navigateFrame = Time.frameCount;
		if (_selectedButton == null)
		{
			SetSelectedButton(GetDefaultButton());
			return;
		}
		Vector3 position = _selectedButton.transform.position;
		float num = float.PositiveInfinity;
		float num2 = float.PositiveInfinity;
		GameObject gameObject = null;
		for (int i = 0; i < _customButtonObjects.Count; i++)
		{
			if (!(_customButtonObjects[i] == null) && !(_customButtonObjects[i] == _selectedButton) && _customButtonObjects[i].activeInHierarchy && TryGetNavigationScore(direction, position, _customButtonObjects[i].transform.position, out var primaryDiff, out var secondaryDiff) && (primaryDiff < num - 0.001f || (Mathf.Abs(primaryDiff - num) < 0.001f && secondaryDiff < num2)))
			{
				gameObject = _customButtonObjects[i];
				num = primaryDiff;
				num2 = secondaryDiff;
			}
		}
		if (gameObject != null)
		{
			SetSelectedButton(gameObject);
		}
	}

	private static bool TryGetNavigationScore(Vector2 direction, Vector3 startPosition, Vector3 candidatePosition, out float primaryDiff, out float secondaryDiff)
	{
		Vector3 vector = candidatePosition - startPosition;
		if (Mathf.Abs(direction.x) > 0.5f)
		{
			if (vector.x * Mathf.Sign(direction.x) <= 0f)
			{
				primaryDiff = 0f;
				secondaryDiff = 0f;
				return false;
			}
			primaryDiff = Mathf.Abs(vector.x);
			secondaryDiff = Mathf.Abs(vector.y);
			return true;
		}
		if (vector.y * Mathf.Sign(direction.y) <= 0f)
		{
			primaryDiff = 0f;
			secondaryDiff = 0f;
			return false;
		}
		primaryDiff = Mathf.Abs(vector.y);
		secondaryDiff = Mathf.Abs(vector.x);
		return true;
	}
}
