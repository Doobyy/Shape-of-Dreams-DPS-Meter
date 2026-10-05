using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UI_GamepadTextInput : SingletonBehaviour<UI_GamepadTextInput>
{
	public Action onIsShiftPressedChanged;

	[Multiline]
	public string qwertyLayout;

	public string qwertyShiftAlpha = "!@#$%^&*()";

	[Multiline]
	public string azertyLayout;

	public string azertyShiftAlpha = "1&é\"'(§è!çà";

	public TMP_InputField inputField;

	public Transform[] rowTransforms;

	public UI_GamepadTextInput_Key regularKeyPrefab;

	public UI_GamepadTextInput_Key leftShiftPrefab;

	public UI_GamepadTextInput_Key rightShiftPrefab;

	public UI_GamepadTextInput_Key backspacePrefab;

	public Button confirmButton;

	public Button clearButton;

	public Button cancelButton;

	private Action<string> _onConfirm;

	private Action _onCancel;

	private string _previousText;

	private DewInputTrigger it_gamepadTextInputLeftShift;

	private DewInputTrigger it_gamepadTextInputRightShift;

	private DewInputTrigger it_gamepadTextInputBackspace;

	private UI_GamepadTextInput_Key _backspaceButton;

	public bool isAzerty { get; private set; }

	public bool isShiftPressed { get; private set; }

	private void Start()
	{
		SetAzertyMode(value: false);
		if (_onConfirm == null && _onCancel == null)
		{
			gameObject.SetActive(value: false);
		}
		((UnityEvent)(object)confirmButton.onClick).AddListener((UnityAction)Confirm);
		((UnityEvent)(object)clearButton.onClick).AddListener((UnityAction)Clear);
		((UnityEvent)(object)cancelButton.onClick).AddListener((UnityAction)Cancel);
		it_gamepadTextInputLeftShift = new DewInputTrigger
		{
			owner = this,
			priority = -10,
			binding = () => DewSave.profileMain.controls.gamepadTextInputLeftShift,
			isValidCheck = () => gameObject.activeSelf
		};
		it_gamepadTextInputRightShift = new DewInputTrigger
		{
			owner = this,
			priority = -10,
			binding = () => DewSave.profileMain.controls.gamepadTextInputRightShift,
			isValidCheck = () => gameObject.activeSelf
		};
		it_gamepadTextInputBackspace = new DewInputTrigger
		{
			owner = this,
			priority = -10,
			binding = () => DewSave.profileMain.controls.gamepadTextInputBackspace,
			isValidCheck = () => gameObject.activeSelf
		};
		DewInput.onCurrentModeChanged += new Action<InputMode, InputMode>(OnCurrentModeChanged);
	}

	private void OnDestroy()
	{
		DewInput.onCurrentModeChanged -= new Action<InputMode, InputMode>(OnCurrentModeChanged);
	}

	private void OnCurrentModeChanged(InputMode arg1, InputMode arg2)
	{
		if (arg2 == InputMode.KeyboardAndMouse && gameObject.activeInHierarchy)
		{
			Cancel();
		}
	}

	public void EraseOne()
	{
		if (inputField.text.Length > 0)
		{
			inputField.text = inputField.text.Substring(0, inputField.text.Length - 1);
		}
	}

	private void Update()
	{
		if (it_gamepadTextInputLeftShift.down || it_gamepadTextInputRightShift.down)
		{
			SetShift(value: true);
		}
		if (it_gamepadTextInputLeftShift.up || it_gamepadTextInputRightShift.up)
		{
			SetShift(value: false);
		}
		if (it_gamepadTextInputBackspace.downRepeated)
		{
			ManagerBase<GlobalUIManager>.instance.SimulateClickOnUIElement(_backspaceButton);
		}
		if (ManagerBase<TransitionManager>.instance.state == TransitionManager.StateType.Loading)
		{
			CancelImmediately();
		}
	}

	public void StartInput(TMP_InputField target, Action<string> onConfirm = null, Action onCancel = null)
	{
		Graphic placeholder = target.placeholder;
		TextMeshProUGUI val = (TextMeshProUGUI)(object)((placeholder is TextMeshProUGUI) ? placeholder : null);
		string placeholderText = ((val != null) ? ((TMP_Text)val).text : "");
		StartInput(target.text, placeholderText, target.multiLine, isPassword: false, target.characterLimit, (string res) =>
		{
			if ((UnityEngine.Object)(object)target != null && ((Selectable)target).IsInteractable() && ((Behaviour)(object)target).isActiveAndEnabled)
			{
				target.text = res;
			}
			onConfirm?.Invoke(res);
		}, onCancel);
	}

	public void StartInput(string previousText, string placeholderText, bool isMultiline, bool isPassword, int maxCharacters, Action<string> onConfirm, Action onCancel)
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		if (!DewSteam.TryStartGamepadInput(previousText, placeholderText, isMultiline, isPassword, maxCharacters, onConfirm, onCancel))
		{
			_onConfirm = onConfirm;
			_onCancel = onCancel;
			_previousText = previousText;
			inputField.lineType = (LineType)(isMultiline ? 2 : 0);
			inputField.text = previousText;
			((TMP_Text)(TextMeshProUGUI)inputField.placeholder).text = placeholderText;
			inputField.characterLimit = maxCharacters;
			isShiftPressed = false;
			onIsShiftPressedChanged?.Invoke();
			gameObject.SetActive(value: true);
		}
	}

	public void SetAzertyMode(bool value)
	{
		isAzerty = value;
		string[] array = (value ? azertyLayout : qwertyLayout).Split("\n", StringSplitOptions.None);
		bool flag = false;
		for (int i = 0; i < array.Length; i++)
		{
			string[] array2 = array[i].Trim().Split(" ", StringSplitOptions.None);
			if (array2.Length == 0)
			{
				continue;
			}
			for (int num = rowTransforms[i].childCount - 1; num >= 0; num--)
			{
				UnityEngine.Object.Destroy(rowTransforms[i].GetChild(num).gameObject);
			}
			string[] array3 = array2;
			foreach (string text in array3)
			{
				if (text == "Shift")
				{
					UnityEngine.Object.Instantiate(flag ? rightShiftPrefab : leftShiftPrefab, rowTransforms[i]).key = "Shift";
					flag = true;
				}
				else if (text == "Backspace")
				{
					UI_GamepadTextInput_Key uI_GamepadTextInput_Key = UnityEngine.Object.Instantiate(backspacePrefab, rowTransforms[i]);
					uI_GamepadTextInput_Key.key = "Backspace";
					_backspaceButton = uI_GamepadTextInput_Key;
				}
				else
				{
					UnityEngine.Object.Instantiate(regularKeyPrefab, rowTransforms[i]).key = text;
				}
			}
		}
	}

	public void ToggleShift()
	{
		SetShift(!isShiftPressed);
	}

	public void SetShift(bool value)
	{
		if (isShiftPressed != value)
		{
			isShiftPressed = value;
			onIsShiftPressedChanged?.Invoke();
		}
	}

	public void PressCharacter(string key)
	{
		if (inputField.characterLimit <= 0 || inputField.text.Length < inputField.characterLimit)
		{
			TMP_InputField val = inputField;
			val.text += (isShiftPressed ? GetShiftSubstitute(key) : key);
			inputField.ForceLabelUpdate();
		}
	}

	public string GetShiftSubstitute(string key)
	{
		if (int.TryParse(key, out var result))
		{
			return (isAzerty ? azertyShiftAlpha[result] : qwertyShiftAlpha[result]).ToString();
		}
		return key.ToUpper();
	}

	private void CancelImmediately()
	{
		gameObject.SetActive(value: false);
		try
		{
			_onCancel?.Invoke();
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
		_onCancel = null;
		_onConfirm = null;
	}

	public void Cancel()
	{
		if (_previousText != inputField.text)
		{
			ManagerBase<MessageManager>.instance.ShowMessage(new DewMessageSettings
			{
				owner = this,
				validator = () => gameObject.activeSelf,
				buttons = (DewMessageSettings.ButtonType.Yes | DewMessageSettings.ButtonType.Cancel),
				defaultButton = DewMessageSettings.ButtonType.Cancel,
				destructiveConfirm = true,
				rawContent = DewLocalization.GetUIValue("GamepadTextInput_Message_ConfirmDiscardChanges"),
				onClose = (DewMessageSettings.ButtonType b) =>
				{
					if (b == DewMessageSettings.ButtonType.Yes)
					{
						CancelImmediately();
					}
				}
			});
		}
		else
		{
			CancelImmediately();
		}
	}

	public void Confirm()
	{
		gameObject.SetActive(value: false);
		try
		{
			_onConfirm?.Invoke(inputField.text);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
		_onCancel = null;
		_onConfirm = null;
	}

	public void Clear()
	{
		if (inputField.text.Length == 0)
		{
			return;
		}
		ManagerBase<MessageManager>.instance.ShowMessage(new DewMessageSettings
		{
			owner = this,
			validator = () => gameObject.activeSelf,
			buttons = (DewMessageSettings.ButtonType.Yes | DewMessageSettings.ButtonType.Cancel),
			defaultButton = DewMessageSettings.ButtonType.Cancel,
			destructiveConfirm = true,
			rawContent = DewLocalization.GetUIValue("GamepadTextInput_Message_ConfirmClear"),
			onClose = (DewMessageSettings.ButtonType b) =>
			{
				if (b == DewMessageSettings.ButtonType.Yes)
				{
					inputField.text = "";
				}
			}
		});
	}
}
