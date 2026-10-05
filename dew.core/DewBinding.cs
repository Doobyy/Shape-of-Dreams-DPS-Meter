using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

[Serializable]
public class DewBinding : ICloneable
{
	public static readonly DewBinding MockBinding = new DewBinding();

	public bool canAssignKeyboard;

	public bool canAssignMouse;

	public bool canAssignGamepad;

	public List<PCBind> pcBinds = new List<PCBind>();

	public List<GamepadButtonEx> gamepadBinds = new List<GamepadButtonEx>();

	[Obsolete]
	public List<Key> keyModifiers = new List<Key>();

	[Obsolete]
	public Key keyboard;

	[Obsolete]
	public MouseButton mouse;

	[Obsolete]
	public GamepadButtonEx? gamepad;

	public static DewBinding KeyboardOnly(params object[] keys)
	{
		DewBinding dewBinding = new DewBinding
		{
			canAssignGamepad = false,
			canAssignKeyboard = true,
			canAssignMouse = false
		};
		AddBindings(dewBinding, keys);
		return dewBinding;
	}

	public static DewBinding KeyboardAndMouseOnly(params object[] keys)
	{
		DewBinding dewBinding = new DewBinding
		{
			canAssignGamepad = false,
			canAssignKeyboard = true,
			canAssignMouse = true
		};
		AddBindings(dewBinding, keys);
		return dewBinding;
	}

	public static DewBinding GamepadOnly(params object[] keys)
	{
		DewBinding dewBinding = new DewBinding
		{
			canAssignGamepad = true,
			canAssignKeyboard = false,
			canAssignMouse = false
		};
		AddBindings(dewBinding, keys);
		return dewBinding;
	}

	public static DewBinding PCAndGamepad(params object[] keys)
	{
		DewBinding dewBinding = new DewBinding
		{
			canAssignGamepad = true,
			canAssignKeyboard = true,
			canAssignMouse = true
		};
		AddBindings(dewBinding, keys);
		return dewBinding;
	}

	private static void AddBindings(DewBinding b, object[] objs)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Expected I4, but got Unknown
		foreach (object obj in objs)
		{
			if (obj is Key val)
			{
				b.pcBinds.Add(val);
			}
			else if (obj is MouseButton mouseButton)
			{
				b.pcBinds.Add(mouseButton);
			}
			else if (obj is GamepadButtonEx item)
			{
				b.gamepadBinds.Add(item);
			}
			else if (obj is GamepadButton val2)
			{
				b.gamepadBinds.Add((GamepadButtonEx)val2);
			}
			else if (obj is Key[] arr)
			{
				AddObjWithModifiers(arr);
			}
			else if (obj is object[] arr2)
			{
				AddObjWithModifiers(arr2);
			}
		}
		void AddObjWithModifiers(IEnumerable enumerable)
		{
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0073: Unknown result type (might be due to invalid IL or missing references)
			//IL_0059: Unknown result type (might be due to invalid IL or missing references)
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			PCBind pCBind = new PCBind();
			b.pcBinds.Add(pCBind);
			foreach (object item2 in enumerable)
			{
				if (item2 is Key val3)
				{
					if (DewInput.ModifierKeys.Contains(val3))
					{
						pCBind.modifiers.Add(val3);
					}
					else
					{
						pCBind.mouse = MouseButton.None;
						pCBind.key = val3;
					}
				}
				else if (item2 is MouseButton mouseButton2)
				{
					pCBind.key = (Key)0;
					pCBind.mouse = mouseButton2;
				}
			}
		}
	}

	public object Clone()
	{
		DewBinding dewBinding = (DewBinding)MemberwiseClone();
		dewBinding.pcBinds = new List<PCBind>();
		foreach (PCBind pcBind in pcBinds)
		{
			dewBinding.pcBinds.Add((PCBind)pcBind.Clone());
		}
		dewBinding.gamepadBinds = new List<GamepadButtonEx>(gamepadBinds);
		dewBinding.keyModifiers = new List<Key>(keyModifiers);
		return dewBinding;
	}

	public bool HasAssignedForCurrentMode()
	{
		if (DewInput.currentMode != InputMode.Gamepad)
		{
			return HasPCAssigned();
		}
		return HasGamepadAssigned();
	}

	public bool HasPCAssigned()
	{
		if (canAssignMouse || canAssignKeyboard)
		{
			return pcBinds.Count > 0;
		}
		return false;
	}

	public bool HasGamepadAssigned()
	{
		if (canAssignGamepad)
		{
			return gamepadBinds.Count > 0;
		}
		return false;
	}

	public BindingType GetBindingTypes()
	{
		BindingType bindingType = BindingType.None;
		if (canAssignGamepad)
		{
			bindingType |= BindingType.Gamepad;
		}
		if (canAssignMouse)
		{
			bindingType |= BindingType.Mouse;
		}
		if (canAssignKeyboard)
		{
			bindingType |= BindingType.Keyboard;
		}
		return bindingType;
	}

	public DewBinding CloneWith(GamepadButtonEx added)
	{
		DewBinding dewBinding = (DewBinding)Clone();
		dewBinding.canAssignGamepad = true;
		dewBinding.gamepadBinds.Add(added);
		return dewBinding;
	}

	public DewBinding CloneWith(MouseButton added)
	{
		DewBinding dewBinding = (DewBinding)Clone();
		dewBinding.canAssignMouse = true;
		dewBinding.pcBinds.Add(new PCBind(added));
		return dewBinding;
	}

	public DewBinding CloneWith(Key added)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		DewBinding dewBinding = (DewBinding)Clone();
		dewBinding.canAssignKeyboard = true;
		dewBinding.pcBinds.Add(new PCBind(added));
		return dewBinding;
	}
}
