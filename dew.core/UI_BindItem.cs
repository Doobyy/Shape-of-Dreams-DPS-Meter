using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_BindItem : LogicBehaviour, ILangaugeChangedCallback, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	private DewBinding _value;

	public BindingType allowedTypes;

	public bool allowModifiers = true;

	public GameObject hoverObject;

	public GameObject notHoverObject;

	public Action<DewBinding> onValueChanged;

	public TextMeshProUGUI keyDisplayText;

	public DewBinding value
	{
		get
		{
			return _value;
		}
		set
		{
			_value = value;
			UpdateKeyDisplayText();
			onValueChanged?.Invoke(_value);
		}
	}

	public void StartListening()
	{
		DewBinding newBinding = (DewBinding)value.Clone();
		SingletonBehaviour<UI_BindingWindow>.instance.Show(newBinding, allowedTypes & value.GetBindingTypes(), allowModifiers, () =>
		{
			value = newBinding;
		}, null);
	}

	public void OnLanguageChanged()
	{
		UpdateKeyDisplayText();
	}

	private void UpdateKeyDisplayText()
	{
		if (_value != null)
		{
			((TMP_Text)keyDisplayText).text = (allowedTypes.HasFlag(BindingType.Gamepad) ? DewInput.GetReadableTextOfGamepad(_value) : DewInput.GetReadableTextOfPC(_value));
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (hoverObject != null)
		{
			hoverObject.SetActive(value: true);
		}
		if (notHoverObject != null)
		{
			notHoverObject.SetActive(value: false);
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (hoverObject != null)
		{
			hoverObject.SetActive(value: false);
		}
		if (notHoverObject != null)
		{
			notHoverObject.SetActive(value: true);
		}
	}

	public void UpdateValueDontNotify(DewBinding newReference)
	{
		_value = newReference;
		UpdateKeyDisplayText();
	}
}
