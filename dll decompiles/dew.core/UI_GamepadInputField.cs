using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_GamepadInputField : UI_GamepadFocusable, IGamepadFocusable, IGamepadFocusListener, IPointerClickHandler, IEventSystemHandler
{
	private TMP_InputField _inputField;

	private void Awake()
	{
		_inputField = GetComponent<TMP_InputField>();
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		if (focusOnEnable && ((Selectable)_inputField).IsInteractable())
		{
			ManagerBase<GlobalUIManager>.instance.SetFocus(this);
		}
	}

	public override bool CanBeFocused()
	{
		if (base.CanBeFocused() && ((Behaviour)(object)_inputField).isActiveAndEnabled && ((Selectable)_inputField).IsInteractable())
		{
			return !_inputField.readOnly;
		}
		return false;
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		if ((int)eventData.button == 0 && DewInput.currentMode == InputMode.Gamepad && CanBeFocused())
		{
			SingletonBehaviour<UI_GamepadTextInput>.instance.StartInput(_inputField);
		}
	}

	public void StartInput(Action<string> onConfirm = null, Action onCancel = null)
	{
		if (!_inputField.readOnly && ((Behaviour)(object)_inputField).isActiveAndEnabled && ((Selectable)_inputField).IsInteractable())
		{
			SingletonBehaviour<UI_GamepadTextInput>.instance.StartInput(_inputField, onConfirm, onCancel);
		}
	}
}
