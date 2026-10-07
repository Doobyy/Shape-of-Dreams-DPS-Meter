using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_GamepadTextInput_Key : UI_GamepadFocusable, IPointerClickHandler, IEventSystemHandler, IGamepadFocusableOverrideInput
{
	public TextMeshProUGUI keyText;

	public string key = "";

	private UI_GamepadTextInput _parent;

	private void Awake()
	{
		_parent = GetComponentInParent<UI_GamepadTextInput>();
		UI_GamepadTextInput parent = _parent;
		parent.onIsShiftPressedChanged = (Action)Delegate.Combine(parent.onIsShiftPressedChanged, new Action(UpdateKeyText));
	}

	private void Start()
	{
		UpdateKeyText();
	}

	private void OnDestroy()
	{
		UI_GamepadTextInput parent = _parent;
		parent.onIsShiftPressedChanged = (Action)Delegate.Remove(parent.onIsShiftPressedChanged, new Action(UpdateKeyText));
	}

	private void UpdateKeyText()
	{
		if (!((UnityEngine.Object)(object)keyText == null))
		{
			if (key.Length > 1)
			{
				((TMP_Text)keyText).text = key;
			}
			else
			{
				((TMP_Text)keyText).text = (_parent.isShiftPressed ? _parent.GetShiftSubstitute(key) : key);
			}
		}
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		if ((int)eventData.button == 0)
		{
			if (key.Length == 1)
			{
				_parent.PressCharacter(key);
			}
			else if (key == "Shift")
			{
				_parent.ToggleShift();
			}
			else if (key == "Backspace")
			{
				_parent.EraseOne();
			}
		}
	}

	public bool OnGamepadDpadUp()
	{
		ManagerBase<GlobalUIManager>.instance.PlayTickSFX();
		return MoveFocus(Vector3.up);
	}

	public bool OnGamepadDpadLeft()
	{
		ManagerBase<GlobalUIManager>.instance.PlayTickSFX();
		return MoveFocus(Vector3.left, isExplicit: true);
	}

	public bool OnGamepadDpadDown()
	{
		ManagerBase<GlobalUIManager>.instance.PlayTickSFX();
		return MoveFocus(Vector3.down);
	}

	public bool OnGamepadDpadRight()
	{
		ManagerBase<GlobalUIManager>.instance.PlayTickSFX();
		return MoveFocus(Vector3.right, isExplicit: true);
	}

	private bool MoveFocus(Vector3 direction, bool isExplicit = false)
	{
		if (isExplicit)
		{
			Vector3 pos = transform.position;
			if (!ManagerBase<GlobalUIManager>.instance.MoveFocus(direction, 0.25f, 0.001f, float.PositiveInfinity))
			{
				ManagerBase<GlobalUIManager>.instance.MoveFocus(-direction, 0.25f, 0.001f, float.PositiveInfinity, null, (IGamepadFocusable f) => Vector3.Distance(f.GetTransform().position, pos));
			}
		}
		else if (!ManagerBase<GlobalUIManager>.instance.MoveFocus(direction, 45.1f, 1f, float.PositiveInfinity))
		{
			return false;
		}
		return true;
	}
}
