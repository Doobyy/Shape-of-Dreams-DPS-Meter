using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;

public class UI_BindingWindow : SingletonBehaviour<UI_BindingWindow>
{
	public TextMeshProUGUI keyText;

	private DewBinding _binding;

	private Action _onFinish;

	private Action _onCancel;

	private BindingType _allowedTypes;

	public void Show(DewBinding newBinding, BindingType allowedTypes, bool allowModifiers, Action onFinish, Action onCancel)
	{
		allowedTypes &= newBinding.GetBindingTypes();
		gameObject.SetActive(value: false);
		_onFinish = onFinish;
		_onCancel = onCancel;
		_binding = newBinding;
		_allowedTypes = allowedTypes;
		DewInput.ListenAnyButtonRaw(this, (InputControl ctrl) =>
		{
			//IL_017b: Unknown result type (might be due to invalid IL or missing references)
			//IL_009f: Unknown result type (might be due to invalid IL or missing references)
			//IL_01fd: Unknown result type (might be due to invalid IL or missing references)
			//IL_0202: Unknown result type (might be due to invalid IL or missing references)
			//IL_0204: Unknown result type (might be due to invalid IL or missing references)
			//IL_0208: Invalid comparison between Unknown and I4
			//IL_00df: Unknown result type (might be due to invalid IL or missing references)
			//IL_020a: Unknown result type (might be due to invalid IL or missing references)
			//IL_020e: Invalid comparison between Unknown and I4
			//IL_011c: Unknown result type (might be due to invalid IL or missing references)
			//IL_013d: Unknown result type (might be due to invalid IL or missing references)
			//IL_05f2: Unknown result type (might be due to invalid IL or missing references)
			//IL_05f7: Unknown result type (might be due to invalid IL or missing references)
			//IL_05ff: Unknown result type (might be due to invalid IL or missing references)
			//IL_0293: Unknown result type (might be due to invalid IL or missing references)
			//IL_062d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0634: Expected I4, but got Unknown
			//IL_0587: Unknown result type (might be due to invalid IL or missing references)
			//IL_04ab: Unknown result type (might be due to invalid IL or missing references)
			//IL_03e2: Unknown result type (might be due to invalid IL or missing references)
			//IL_03e7: Unknown result type (might be due to invalid IL or missing references)
			//IL_02b0: Unknown result type (might be due to invalid IL or missing references)
			//IL_02b6: Unknown result type (might be due to invalid IL or missing references)
			//IL_04eb: Unknown result type (might be due to invalid IL or missing references)
			//IL_02d9: Unknown result type (might be due to invalid IL or missing references)
			//IL_0528: Unknown result type (might be due to invalid IL or missing references)
			//IL_0549: Unknown result type (might be due to invalid IL or missing references)
			//IL_0325: Unknown result type (might be due to invalid IL or missing references)
			//IL_0362: Unknown result type (might be due to invalid IL or missing references)
			//IL_039f: Unknown result type (might be due to invalid IL or missing references)
			if (ctrl.noisy)
			{
				return false;
			}
			if (allowedTypes.HasFlag(BindingType.Mouse))
			{
				DeltaControl val = (DeltaControl)(object)((ctrl is DeltaControl) ? ctrl : null);
				if (val != null)
				{
					if (_binding.pcBinds.Count != 1)
					{
						_binding.pcBinds.Clear();
						_binding.pcBinds.Add(new PCBind());
					}
					if (allowModifiers && DewInput.ModifierKeys.Contains(_binding.pcBinds[0].key) && !_binding.pcBinds[0].modifiers.Contains(_binding.pcBinds[0].key))
					{
						_binding.pcBinds[0].modifiers.Add(_binding.pcBinds[0].key);
						_binding.pcBinds[0].key = (Key)0;
					}
					else
					{
						_binding.pcBinds[0].modifiers.Clear();
						_binding.pcBinds[0].key = (Key)0;
					}
					_binding.pcBinds[0].mouse = ((((Vector2)((InputControl<Vector2>)(object)val).value).y > 0f) ? MouseButton.ScrollUp : MouseButton.ScrollDown);
					UpdateText();
					return false;
				}
			}
			ButtonControl val2 = (ButtonControl)(object)((ctrl is ButtonControl) ? ctrl : null);
			if (val2 == null)
			{
				return false;
			}
			if (allowedTypes.HasFlag(BindingType.Keyboard))
			{
				KeyControl val3 = (KeyControl)(object)((ctrl is KeyControl) ? ctrl : null);
				if (val3 != null)
				{
					if (((ButtonControl)val3).isPressed)
					{
						return false;
					}
					Key keyCode = val3.keyCode;
					if ((int)keyCode == 57 || (int)keyCode == 58)
					{
						return false;
					}
					if (_binding.pcBinds.Count != 1)
					{
						_binding.pcBinds.Clear();
						_binding.pcBinds.Add(new PCBind());
					}
					_binding.pcBinds[0].mouse = MouseButton.None;
					if (!allowModifiers || (int)_binding.pcBinds[0].key == 0 || _binding.pcBinds[0].key == val3.keyCode || _binding.pcBinds[0].modifiers.Contains(val3.keyCode))
					{
						_binding.pcBinds[0].modifiers.Clear();
					}
					else if (DewInput.ModifierKeys.Contains(_binding.pcBinds[0].key) && !_binding.pcBinds[0].modifiers.Contains(_binding.pcBinds[0].key))
					{
						_binding.pcBinds[0].modifiers.Add(_binding.pcBinds[0].key);
					}
					else
					{
						_binding.pcBinds[0].modifiers.Clear();
					}
					_binding.pcBinds[0].key = val3.keyCode;
				}
			}
			if (allowedTypes.HasFlag(BindingType.Mouse) && ctrl.device is Mouse && DewInput.NameToMouseButton.TryGetValue(((InputControl)val2).name, out var value))
			{
				if (value == MouseButton.Left)
				{
					return false;
				}
				if (val2.isPressed)
				{
					return false;
				}
				if (_binding.pcBinds.Count != 1)
				{
					_binding.pcBinds.Clear();
					_binding.pcBinds.Add(new PCBind());
				}
				if (allowModifiers && DewInput.ModifierKeys.Contains(_binding.pcBinds[0].key) && !_binding.pcBinds[0].modifiers.Contains(_binding.pcBinds[0].key))
				{
					_binding.pcBinds[0].modifiers.Add(_binding.pcBinds[0].key);
					_binding.pcBinds[0].key = (Key)0;
				}
				else
				{
					_binding.pcBinds[0].modifiers.Clear();
					_binding.pcBinds[0].key = (Key)0;
				}
				_binding.pcBinds[0].mouse = value;
			}
			if (allowedTypes.HasFlag(BindingType.Gamepad) && ctrl.device is Gamepad)
			{
				foreach (GamepadButton value2 in Enum.GetValues(typeof(GamepadButton)))
				{
					if ((object)ctrl == Gamepad.current[value2])
					{
						_binding.gamepadBinds.Clear();
						_binding.gamepadBinds.Add((GamepadButtonEx)value2);
						break;
					}
				}
			}
			UpdateText();
			return false;
		});
		gameObject.SetActive(value: true);
		UpdateText();
	}

	private void UpdateText()
	{
		((TMP_Text)keyText).text = DewInput.GetReadableTextOfPC(_binding);
	}

	public void DoLeftClick()
	{
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		if (_binding.pcBinds.Count != 1)
		{
			_binding.pcBinds.Clear();
			_binding.pcBinds.Add(new PCBind());
		}
		_binding.pcBinds[0].mouse = MouseButton.Left;
		if (DewInput.ModifierKeys.Contains(_binding.pcBinds[0].key))
		{
			_binding.pcBinds[0].modifiers.Add(_binding.pcBinds[0].key);
			_binding.pcBinds[0].key = (Key)0;
		}
		else
		{
			_binding.pcBinds[0].modifiers.Clear();
			_binding.pcBinds[0].key = (Key)0;
		}
		UpdateText();
	}

	public void Clear()
	{
		if (_allowedTypes.HasFlag(BindingType.Gamepad))
		{
			_binding.gamepadBinds.Clear();
		}
		if (_allowedTypes.HasFlag(BindingType.Keyboard) || _allowedTypes.HasFlag(BindingType.Mouse))
		{
			_binding.pcBinds.Clear();
		}
		UpdateText();
	}

	public void Confirm()
	{
		try
		{
			_onFinish?.Invoke();
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
		gameObject.SetActive(value: false);
	}

	public void Cancel()
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
	}

	private void OnEnable()
	{
		ManagerBase<GlobalUIManager>.instance.isBackDisabled = true;
	}

	private void OnDisable()
	{
		ManagerBase<GlobalUIManager>.instance.isBackDisabled = false;
		DewInput.StopListenAnyButton();
	}
}
