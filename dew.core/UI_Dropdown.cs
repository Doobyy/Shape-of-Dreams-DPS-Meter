using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class UI_Dropdown : MonoBehaviour
{
	public SafeAction<object> onValueChanged;

	private bool _didInit;

	private TMP_Dropdown _dropdown;

	private List<object> _values = new List<object>();

	public int index
	{
		get
		{
			MakeSureInit();
			return _dropdown.value;
		}
		set
		{
			MakeSureInit();
			_dropdown.value = value;
		}
	}

	public object value
	{
		get
		{
			MakeSureInit();
			return _values.GetOrDefault(index);
		}
		set
		{
			MakeSureInit();
			int num = _values.IndexOf(value);
			if (num >= 0)
			{
				_dropdown.value = num;
			}
		}
	}

	private void MakeSureInit()
	{
		if (!_didInit)
		{
			_didInit = true;
			_dropdown = GetComponent<TMP_Dropdown>();
			((UnityEvent<int>)(object)_dropdown.onValueChanged).AddListener((UnityAction<int>)((int _) =>
			{
				onValueChanged?.Invoke(value);
			}));
		}
	}

	public void ClearOptions()
	{
		MakeSureInit();
		_dropdown.ClearOptions();
		_values.Clear();
	}

	public void SetValueWithoutNotify(object val)
	{
		int num = _values.IndexOf(val);
		if (num >= 0)
		{
			_dropdown.SetValueWithoutNotify(num);
		}
	}

	public void AddOption(string text, object val)
	{
		MakeSureInit();
		_values.Add(val);
		_dropdown.AddOptions(new List<string> { text });
	}
}
