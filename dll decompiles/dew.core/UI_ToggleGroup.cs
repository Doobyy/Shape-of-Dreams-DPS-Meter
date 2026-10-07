using System;
using UnityEngine;
using UnityEngine.Events;

public class UI_ToggleGroup : MonoBehaviour
{
	public UnityEvent<int> onCurrentIndexChanged;

	[SerializeField]
	private int _currentIndex;

	public int currentIndex
	{
		get
		{
			return _currentIndex;
		}
		set
		{
			if (_currentIndex == value)
			{
				return;
			}
			_currentIndex = value;
			UI_Toggle[] componentsInChildren = GetComponentsInChildren<UI_Toggle>();
			foreach (UI_Toggle uI_Toggle in componentsInChildren)
			{
				bool flag = _currentIndex == uI_Toggle.index;
				if (uI_Toggle.isChecked != flag)
				{
					uI_Toggle.isChecked = flag;
				}
			}
			try
			{
				onCurrentIndexChanged?.Invoke(value);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception, this);
			}
		}
	}

	private void Start()
	{
		UI_Toggle[] componentsInChildren = GetComponentsInChildren<UI_Toggle>();
		foreach (UI_Toggle uI_Toggle in componentsInChildren)
		{
			bool flag = _currentIndex == uI_Toggle.index;
			if (uI_Toggle.isChecked != flag)
			{
				uI_Toggle.isChecked = flag;
			}
		}
	}
}
