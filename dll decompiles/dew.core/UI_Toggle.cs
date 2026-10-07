using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UI_Toggle : MonoBehaviour
{
	public int index;

	public GameObject onObject;

	public GameObject offObject;

	public UnityEvent<bool> onIsCheckedChanged;

	public UnityEvent onClick;

	public bool doNotToggleOnClick;

	[SerializeField]
	private bool _isChecked;

	private Button _button;

	public bool interactable
	{
		get
		{
			return ((Selectable)_button).interactable;
		}
		set
		{
			((Selectable)_button).interactable = value;
		}
	}

	public bool isChecked
	{
		get
		{
			return _isChecked;
		}
		set
		{
			if (_isChecked == value)
			{
				return;
			}
			UI_ToggleGroup componentInParent = GetComponentInParent<UI_ToggleGroup>();
			if (componentInParent != null && componentInParent.currentIndex == index && !value)
			{
				return;
			}
			_isChecked = value;
			if (onObject != null)
			{
				onObject.SetActive(_isChecked);
			}
			if (offObject != null)
			{
				offObject.SetActive(!_isChecked);
			}
			if (_isChecked && componentInParent != null)
			{
				componentInParent.currentIndex = index;
			}
			try
			{
				onIsCheckedChanged?.Invoke(value);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception, this);
			}
		}
	}

	private void Awake()
	{
		_button = GetComponent<Button>();
	}

	private void Start()
	{
		UI_ToggleGroup componentInParent = GetComponentInParent<UI_ToggleGroup>();
		if (componentInParent != null && isChecked != (componentInParent.currentIndex == index))
		{
			isChecked = componentInParent.currentIndex == index;
		}
		if (onObject != null)
		{
			onObject.SetActive(_isChecked);
		}
		if (offObject != null)
		{
			offObject.SetActive(!_isChecked);
		}
		((UnityEvent)(object)_button.onClick).AddListener((UnityAction)(() =>
		{
			if (!doNotToggleOnClick)
			{
				isChecked = !isChecked;
			}
			try
			{
				onClick?.Invoke();
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}));
	}
}
