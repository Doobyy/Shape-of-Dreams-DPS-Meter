using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;

public static class DewInput
{
	private enum ValidStatus
	{
		No,
		Yes,
		YesExceptLeft
	}

	private static UnityEngine.Object _currentAnyButtonListener;

	internal static Action<InputControl> _currentAnyButtonListenerAction;

	private static IDisposable _currentAnyButtonListenerDisposable;

	public static SafeAction<InputMode, InputMode> onCurrentModeChanged;

	private static bool _shouldCheckInputModeConflict = true;

	private static List<float> _inputModeChangeTimes = new List<float>();

	internal static readonly IReadOnlyDictionary<Key, string> KeyReadableTextBindings = new Dictionary<Key, string>
	{
		{
			(Key)0,
			"!None"
		},
		{
			(Key)60,
			"Esc"
		},
		{
			(Key)2,
			"Enter"
		},
		{
			(Key)50,
			"0"
		},
		{
			(Key)41,
			"1"
		},
		{
			(Key)42,
			"2"
		},
		{
			(Key)43,
			"3"
		},
		{
			(Key)44,
			"4"
		},
		{
			(Key)45,
			"5"
		},
		{
			(Key)46,
			"6"
		},
		{
			(Key)47,
			"7"
		},
		{
			(Key)48,
			"8"
		},
		{
			(Key)49,
			"9"
		},
		{
			(Key)8,
			"."
		},
		{
			(Key)4,
			"`"
		},
		{
			(Key)5,
			"'"
		},
		{
			(Key)79,
			"Num*"
		},
		{
			(Key)80,
			"Num+"
		},
		{
			(Key)81,
			"Num-"
		},
		{
			(Key)82,
			"Num."
		},
		{
			(Key)77,
			"NumEnter"
		},
		{
			(Key)13,
			"-"
		},
		{
			(Key)9,
			"/"
		},
		{
			(Key)10,
			"\\"
		},
		{
			(Key)6,
			":"
		},
		{
			(Key)7,
			","
		},
		{
			(Key)14,
			"="
		},
		{
			(Key)11,
			"["
		},
		{
			(Key)12,
			"]"
		},
		{
			(Key)61,
			"Left"
		},
		{
			(Key)62,
			"Right"
		},
		{
			(Key)63,
			"Up"
		},
		{
			(Key)64,
			"Down"
		},
		{
			(Key)51,
			"LShift"
		},
		{
			(Key)52,
			"RShift"
		},
		{
			(Key)53,
			"LAlt"
		},
		{
			(Key)54,
			"RAlt"
		},
		{
			(Key)55,
			"LCtrl"
		},
		{
			(Key)56,
			"RCtrl"
		}
	};

	internal static readonly IReadOnlyDictionary<GamepadButtonEx, string> GamepadButtonReadableTextBindings = new Dictionary<GamepadButtonEx, string>
	{
		{
			GamepadButtonEx.LeftShoulder,
			"LB"
		},
		{
			GamepadButtonEx.RightShoulder,
			"RB"
		},
		{
			GamepadButtonEx.LeftStick,
			"LS"
		},
		{
			GamepadButtonEx.RightStick,
			"RS"
		},
		{
			GamepadButtonEx.LeftTrigger,
			"LT"
		},
		{
			GamepadButtonEx.RightTrigger,
			"RT"
		},
		{
			GamepadButtonEx.A,
			"A"
		},
		{
			GamepadButtonEx.B,
			"B"
		},
		{
			GamepadButtonEx.Square,
			"X"
		},
		{
			GamepadButtonEx.North,
			"Y"
		}
	};

	internal static readonly IReadOnlyDictionary<MouseButton, string> MouseButtonReadableTextBindings = new Dictionary<MouseButton, string>
	{
		{
			MouseButton.None,
			"!None"
		},
		{
			MouseButton.Left,
			"LMB"
		},
		{
			MouseButton.Right,
			"RMB"
		},
		{
			MouseButton.Middle,
			"MMB"
		},
		{
			MouseButton.Forward,
			"MB5"
		},
		{
			MouseButton.Back,
			"MB4"
		},
		{
			MouseButton.ScrollDown,
			"Scroll-"
		},
		{
			MouseButton.ScrollUp,
			"Scroll+"
		}
	};

	public static readonly IReadOnlyDictionary<string, MouseButton> NameToMouseButton = new Dictionary<string, MouseButton>
	{
		{
			"leftButton",
			MouseButton.Left
		},
		{
			"rightButton",
			MouseButton.Right
		},
		{
			"middleButton",
			MouseButton.Middle
		},
		{
			"forwardButton",
			MouseButton.Forward
		},
		{
			"backButton",
			MouseButton.Back
		}
	};

	public static readonly List<Key> ModifierKeys = new List<Key>
	{
		(Key)51,
		(Key)53,
		(Key)57,
		(Key)55,
		(Key)57,
		(Key)57,
		(Key)57,
		(Key)52,
		(Key)54,
		(Key)58,
		(Key)56,
		(Key)58,
		(Key)58,
		(Key)58
	};

	private static int _mouseScrollAxisFrameCount;

	private static float _currentFrameMouseScrollAxis;

	private static float _previousFrameMouseScrollAxis;

	private const int ButtonKeyCacheSize = 512;

	private const int ButtonMouseCacheSize = 8;

	private const int ButtonGamepadCacheSize = 1014;

	private static readonly int[] _buttonKeyFrame = new int[512];

	private static readonly bool[] _buttonKeyValue = new bool[512];

	private static readonly int[] _buttonMouseFrameNoCheck = new int[8];

	private static readonly bool[] _buttonMouseValueNoCheck = new bool[8];

	private static readonly int[] _buttonMouseFrameCheck = new int[8];

	private static readonly bool[] _buttonMouseValueCheck = new bool[8];

	private static readonly int[] _buttonGamepadFrame = new int[1014];

	private static readonly bool[] _buttonGamepadValue = new bool[1014];

	private static int _isInputDisabledFrame;

	private static bool _isInputDisabledCached;

	private static ValidStatus _isMouseClickValid;

	private static int _isMouseClickValidFrame;

	private static List<RaycastResult> _isMouseClickValidHits = new List<RaycastResult>();

	public static ControllerButtonType controllerButtonType => DewSave.platformSettings.controls.controllerButtonType;

	public static InputMode currentMode { get; private set; }

	private static bool isInputDisabled
	{
		get
		{
			int num = Time.frameCount + 1;
			if (_isInputDisabledFrame != num)
			{
				_isInputDisabledFrame = num;
				_isInputDisabledCached = _currentAnyButtonListener != null;
			}
			return _isInputDisabledCached;
		}
	}

	public static void StopListenAnyButton()
	{
		if (_currentAnyButtonListenerDisposable != null)
		{
			_currentAnyButtonListenerDisposable.Dispose();
			_currentAnyButtonListenerDisposable = null;
			_currentAnyButtonListener = null;
			_currentAnyButtonListenerAction = null;
		}
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	private static void Initialize()
	{
		_currentAnyButtonListenerDisposable = null;
		_currentAnyButtonListener = null;
		_currentAnyButtonListenerAction = null;
		onCurrentModeChanged = null;
		_shouldCheckInputModeConflict = !DewBuildProfile.current.HasFeature(BuildFeatureTag.Booth);
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
	private static void InitializeLate()
	{
		_shouldCheckInputModeConflict = !DewLaunchOptions.forceGamepad && !DewLaunchOptions.forceKeyboardAndMouse;
		_inputModeChangeTimes.Clear();
		if (DewLaunchOptions.forceKeyboardAndMouse)
		{
			SetInputMode(InputMode.KeyboardAndMouse);
		}
		if (DewLaunchOptions.forceGamepad)
		{
			SetInputMode(InputMode.Gamepad);
		}
	}

	public static void ListenAnyButtonRaw(UnityEngine.Object caller, Func<InputControl, bool> callback)
	{
		StopListenAnyButton();
		_currentAnyButtonListener = caller;
		_currentAnyButtonListenerAction = (InputControl ctrl) =>
		{
			if (_currentAnyButtonListener == null)
			{
				StopListenAnyButton();
				return;
			}
			try
			{
				if (callback(ctrl))
				{
					StopListenAnyButton();
				}
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
				StopListenAnyButton();
			}
		};
		_currentAnyButtonListenerDisposable = Observable.Call<InputControl>(InputSystem.onAnyButtonPress, _currentAnyButtonListenerAction);
	}

	public static string GetReadableTextOfGamepad(DewBinding b, out BindingType type)
	{
		if (b.HasGamepadAssigned())
		{
			type = BindingType.Gamepad;
			return $"<sprite name=\"{controllerButtonType}.{(int)b.gamepadBinds[0]}\">";
		}
		type = BindingType.None;
		return DewLocalization.GetUIValue("Key_None");
	}

	public static string GetReadableTextOfGamepad(DewBinding b)
	{
		BindingType type;
		return GetReadableTextOfGamepad(b, out type);
	}

	public static string GetReadableTextOfPC(DewBinding b, out BindingType type, int i = 0)
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		string text = "";
		if (b.HasPCAssigned())
		{
			if (b.pcBinds[i].mouse != MouseButton.None)
			{
				type = BindingType.Mouse;
				text = b.pcBinds[i].mouse.GetReadableText();
			}
			else
			{
				if ((int)b.pcBinds[i].key == 0)
				{
					type = BindingType.None;
					return DewLocalization.GetUIValue("Key_None");
				}
				type = BindingType.Keyboard;
				text = b.pcBinds[i].key.GetReadableText();
			}
			if (b.pcBinds[i].modifiers.Count > 0)
			{
				foreach (Key modifier in b.pcBinds[0].modifiers)
				{
					text = ((!KeyReadableTextBindings.TryGetValue(modifier, out var value)) ? $"{modifier} + {text}" : (value + " + " + text));
				}
			}
			return text;
		}
		type = BindingType.None;
		return DewLocalization.GetUIValue("Key_None");
	}

	public static string GetReadableTextOfPC(DewBinding b, int i = 0)
	{
		BindingType type;
		return GetReadableTextOfPC(b, out type, i);
	}

	public static string GetReadableTextForCurrentMode(DewBinding b)
	{
		BindingType type;
		return GetReadableTextForCurrentMode(b, out type);
	}

	public static string GetReadableTextForCurrentMode(DewBinding b, out BindingType type)
	{
		if (currentMode == InputMode.Gamepad)
		{
			return GetReadableTextOfGamepad(b, out type);
		}
		return GetReadableTextOfPC(b, out type);
	}

	public static bool GetButtonDownAnyKey()
	{
		if (Keyboard.current != null)
		{
			return ((ButtonControl)Keyboard.current.anyKey).wasPressedThisFrame;
		}
		return false;
	}

	public static bool GetButtonDownAnyGamepad()
	{
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		if (Gamepad.current == null)
		{
			return false;
		}
		if (!((InputDevice)Gamepad.current).wasUpdatedThisFrame)
		{
			return false;
		}
		for (int i = 0; i < ((InputDevice)Gamepad.current).allControls.Count; i++)
		{
			InputControl val = ((InputDevice)Gamepad.current).allControls[i];
			ButtonControl val2 = (ButtonControl)(object)((val is ButtonControl) ? val : null);
			if (val2 != null && val2.wasPressedThisFrame)
			{
				return true;
			}
		}
		return false;
	}

	public static bool GetButtonDownAnyMouse()
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		if (Mouse.current == null)
		{
			return false;
		}
		for (int i = 0; i < ((InputDevice)Mouse.current).allControls.Count; i++)
		{
			InputControl val = ((InputDevice)Mouse.current).allControls[i];
			ButtonControl val2 = (ButtonControl)(object)((val is ButtonControl) ? val : null);
			if (val2 != null && val2.wasPressedThisFrame)
			{
				return true;
			}
		}
		return false;
	}

	public static void SetInputMode(InputMode mode)
	{
		if (DewLaunchOptions.forceGamepad)
		{
			mode = InputMode.Gamepad;
		}
		if (DewLaunchOptions.forceKeyboardAndMouse)
		{
			mode = InputMode.KeyboardAndMouse;
		}
		if (currentMode == mode)
		{
			return;
		}
		if (_shouldCheckInputModeConflict)
		{
			_inputModeChangeTimes.Add(Time.unscaledTime);
			if (_inputModeChangeTimes.Count > 15)
			{
				_inputModeChangeTimes.RemoveAt(0);
				if (_inputModeChangeTimes.All((float t) => Time.unscaledTime - t < 3f) && !(UnityEngine.Object)(object)NetworkedManagerBase<GameManager>.softInstance)
				{
					_shouldCheckInputModeConflict = false;
					DewLaunchOptions.forceKeyboardAndMouse = true;
					mode = InputMode.KeyboardAndMouse;
					ManagerBase<MessageManager>.instance.ShowMessage(new DewMessageSettings
					{
						rawContent = DewLocalization.GetUIValue("Message_ControlConflictConfirmation"),
						buttons = (DewMessageSettings.ButtonType.Yes | DewMessageSettings.ButtonType.No),
						defaultButton = DewMessageSettings.ButtonType.No,
						onClose = (DewMessageSettings.ButtonType b) =>
						{
							if (b == DewMessageSettings.ButtonType.Yes)
							{
								DewLaunchOptions.forceKeyboardAndMouse = true;
								ManagerBase<MessageManager>.instance.ShowMessageLocalized("Message_ControlConflictConfirmation_Confirmed");
							}
							else
							{
								DewLaunchOptions.forceKeyboardAndMouse = false;
							}
						}
					});
				}
			}
		}
		switch (mode)
		{
		case InputMode.KeyboardAndMouse:
			Mouse.current.WarpCursorPosition(new Vector2((float)Screen.width * 0.5f, (float)Screen.height * 0.5f));
			Cursor.visible = true;
			break;
		case InputMode.Gamepad:
			Mouse.current.WarpCursorPosition(new Vector2((float)Screen.width * 0.02f, (float)Screen.height * 0.98f));
			Cursor.visible = false;
			break;
		}
		InputMode a = currentMode;
		currentMode = mode;
		onCurrentModeChanged?.Invoke(a, mode);
	}

	private static KeyCode GetMouseButtonKey_Imp(MouseButton m)
	{
		return m switch
		{
			MouseButton.None => KeyCode.None, 
			MouseButton.Left => KeyCode.Mouse0, 
			MouseButton.Right => KeyCode.Mouse1, 
			MouseButton.Middle => KeyCode.Mouse2, 
			MouseButton.Forward => KeyCode.Mouse4, 
			MouseButton.Back => KeyCode.Mouse3, 
			_ => throw new ArgumentOutOfRangeException("m", m, null), 
		};
	}

	internal static void UpdateScrollAxis()
	{
		if (_mouseScrollAxisFrameCount != Time.frameCount)
		{
			_mouseScrollAxisFrameCount = Time.frameCount;
			_previousFrameMouseScrollAxis = _currentFrameMouseScrollAxis;
			_currentFrameMouseScrollAxis = Input.GetAxis("Mouse ScrollWheel");
		}
	}

	internal static bool GetMouseButtonDown_Imp(MouseButton m)
	{
		UpdateScrollAxis();
		switch (m)
		{
		case MouseButton.Left:
		case MouseButton.Right:
		case MouseButton.Middle:
		case MouseButton.Forward:
		case MouseButton.Back:
			return Input.GetKeyDown(GetMouseButtonKey_Imp(m));
		case MouseButton.ScrollUp:
			return _currentFrameMouseScrollAxis > 0f;
		case MouseButton.ScrollDown:
			return _currentFrameMouseScrollAxis < 0f;
		default:
			return false;
		}
	}

	internal static bool GetMouseButton_Imp(MouseButton m)
	{
		UpdateScrollAxis();
		switch (m)
		{
		case MouseButton.Left:
		case MouseButton.Right:
		case MouseButton.Middle:
		case MouseButton.Forward:
		case MouseButton.Back:
			return Input.GetKey(GetMouseButtonKey_Imp(m));
		case MouseButton.ScrollUp:
			return _currentFrameMouseScrollAxis > 0f;
		case MouseButton.ScrollDown:
			return _currentFrameMouseScrollAxis < 0f;
		default:
			return false;
		}
	}

	internal static bool GetMouseButtonUp_Imp(MouseButton m)
	{
		UpdateScrollAxis();
		switch (m)
		{
		case MouseButton.Left:
		case MouseButton.Right:
		case MouseButton.Middle:
		case MouseButton.Forward:
		case MouseButton.Back:
			return Input.GetKeyUp(GetMouseButtonKey_Imp(m));
		case MouseButton.ScrollUp:
			if (_previousFrameMouseScrollAxis > 0f)
			{
				return _currentFrameMouseScrollAxis == 0f;
			}
			return false;
		case MouseButton.ScrollDown:
			if (_previousFrameMouseScrollAxis < 0f)
			{
				return _currentFrameMouseScrollAxis == 0f;
			}
			return false;
		default:
			return false;
		}
	}

	private static bool IsKeyPressedCached(Key key, Keyboard keyboard)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Expected I4, but got Unknown
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		if (keyboard == null || (int)key == 0)
		{
			return false;
		}
		int num = (int)key;
		if ((uint)num < 512u)
		{
			int num2 = Time.frameCount + 1;
			if (_buttonKeyFrame[num] != num2)
			{
				_buttonKeyFrame[num] = num2;
				_buttonKeyValue[num] = ((ButtonControl)keyboard[key]).isPressed;
			}
			return _buttonKeyValue[num];
		}
		return ((ButtonControl)keyboard[key]).isPressed;
	}

	private static bool IsMousePressedCached(MouseButton button, bool checkGameArea, Mouse mouse)
	{
		if (mouse == null || button == MouseButton.None)
		{
			return false;
		}
		if ((uint)button < 8u)
		{
			int[] array = (checkGameArea ? _buttonMouseFrameCheck : _buttonMouseFrameNoCheck);
			bool[] array2 = (checkGameArea ? _buttonMouseValueCheck : _buttonMouseValueNoCheck);
			int num = Time.frameCount + 1;
			if (array[(int)button] != num)
			{
				array[(int)button] = num;
				bool flag = !checkGameArea || (button != MouseButton.Left && button != MouseButton.Right) || IsGameRelatedMouseInputValid(button);
				array2[(int)button] = flag && GetMouseButton_Imp(button);
			}
			return array2[(int)button];
		}
		if (checkGameArea && (button == MouseButton.Left || button == MouseButton.Right) && !IsGameRelatedMouseInputValid(button))
		{
			return false;
		}
		return GetMouseButton_Imp(button);
	}

	private static bool IsGamepadPressedCached(GamepadButtonEx button, Gamepad gamepad)
	{
		if (gamepad == null)
		{
			return false;
		}
		if ((uint)button < 1014u)
		{
			int num = Time.frameCount + 1;
			if (_buttonGamepadFrame[(int)button] != num)
			{
				_buttonGamepadFrame[(int)button] = num;
				if (ManagerBase<InputManager>.instance.IsStickDirection(button))
				{
					_buttonGamepadValue[(int)button] = ManagerBase<InputManager>.instance.GetStickDirection(button);
				}
				else
				{
					_buttonGamepadValue[(int)button] = gamepad[(GamepadButton)button].isPressed;
				}
			}
			return _buttonGamepadValue[(int)button];
		}
		if (ManagerBase<InputManager>.instance.IsStickDirection(button))
		{
			return ManagerBase<InputManager>.instance.GetStickDirection(button);
		}
		return gamepad[(GamepadButton)button].isPressed;
	}

	public static Vector2 GetLeftJoystick()
	{
		return GetJoystick(isLeft: true, DewSave.platformSettings.controls.joystick0Dead, DewSave.platformSettings.controls.joystick0Max);
	}

	public static Vector2 GetRightJoystick()
	{
		return GetJoystick(isLeft: false, DewSave.platformSettings.controls.joystick1Dead, DewSave.platformSettings.controls.joystick1Max);
	}

	private unsafe static Vector2 GetJoystick(bool isLeft, float dead, float max)
	{
		if (Gamepad.current == null || DewLaunchOptions.forceKeyboardAndMouse)
		{
			return Vector2.zero;
		}
		Vector2 vector = (isLeft ? (*(Vector2*)((InputControl<Vector2>)(object)Gamepad.current.leftStick).value) : (*(Vector2*)((InputControl<Vector2>)(object)Gamepad.current.rightStick).value));
		if (vector.magnitude < dead)
		{
			return Vector2.zero;
		}
		Vector2 result = vector.normalized * Mathf.Clamp01((vector.magnitude - dead) / (max - dead));
		if (ControlManager.AreControlsInverted())
		{
			result *= -1f;
		}
		return result;
	}

	public static bool GetButtonDown(Key k)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		if (isInputDisabled)
		{
			return false;
		}
		if (DewLaunchOptions.forceGamepad)
		{
			return false;
		}
		if (Keyboard.current == null || (int)k == 0)
		{
			return false;
		}
		if (((ButtonControl)Keyboard.current[k]).wasPressedThisFrame)
		{
			SetInputMode(InputMode.KeyboardAndMouse);
			return true;
		}
		return false;
	}

	public static bool GetButtonDown(MouseButton m, bool checkGameArea)
	{
		if (isInputDisabled)
		{
			return false;
		}
		if (DewLaunchOptions.forceGamepad)
		{
			return false;
		}
		if (Mouse.current == null || m == MouseButton.None)
		{
			return false;
		}
		if (checkGameArea && (m == MouseButton.Left || m == MouseButton.Right) && !IsGameRelatedMouseInputValid(m))
		{
			return false;
		}
		if (GetMouseButtonDown_Imp(m))
		{
			SetInputMode(InputMode.KeyboardAndMouse);
			return true;
		}
		return false;
	}

	public static bool GetButtonDown(GamepadButtonEx? g)
	{
		if (isInputDisabled)
		{
			return false;
		}
		if (DewLaunchOptions.forceKeyboardAndMouse)
		{
			return false;
		}
		if (Gamepad.current == null || !g.HasValue)
		{
			return false;
		}
		if (ManagerBase<InputManager>.instance.IsStickDirection(g.Value))
		{
			if (ManagerBase<InputManager>.instance.GetStickDirectionDown(g.Value))
			{
				SetInputMode(InputMode.Gamepad);
				return true;
			}
		}
		else if (Gamepad.current[(GamepadButton)g.Value].wasPressedThisFrame)
		{
			SetInputMode(InputMode.Gamepad);
			return true;
		}
		return false;
	}

	public static bool GetButtonDown(DewBinding b, bool checkGameAreaForMouse)
	{
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		if (isInputDisabled)
		{
			return false;
		}
		Keyboard current = Keyboard.current;
		Mouse current2 = Mouse.current;
		if (!DewLaunchOptions.forceGamepad && b.HasPCAssigned())
		{
			for (int num = 0; num < b.pcBinds.Count; num++)
			{
				PCBind pCBind = b.pcBinds[num];
				if (pCBind.mouse != MouseButton.None && current2 != null)
				{
					if (checkGameAreaForMouse)
					{
						MouseButton mouse = pCBind.mouse;
						if ((mouse == MouseButton.Left || mouse == MouseButton.Right) && !IsGameRelatedMouseInputValid(pCBind.mouse))
						{
							goto IL_0085;
						}
					}
					if (GetMouseButtonDown_Imp(pCBind.mouse))
					{
						SetInputMode(InputMode.KeyboardAndMouse);
						return true;
					}
				}
				goto IL_0085;
				IL_0085:
				if ((int)pCBind.key != 0 && current != null && ((ButtonControl)current[pCBind.key]).wasPressedThisFrame)
				{
					SetInputMode(InputMode.KeyboardAndMouse);
					return true;
				}
			}
		}
		Gamepad current3 = Gamepad.current;
		if (!DewLaunchOptions.forceKeyboardAndMouse && current3 != null && b.HasGamepadAssigned())
		{
			for (int i = 0; i < b.gamepadBinds.Count; i++)
			{
				GamepadButtonEx gamepadButtonEx = b.gamepadBinds[i];
				if (ManagerBase<InputManager>.instance.IsStickDirection(gamepadButtonEx))
				{
					if (ManagerBase<InputManager>.instance.GetStickDirectionDown(gamepadButtonEx))
					{
						SetInputMode(InputMode.Gamepad);
						return true;
					}
				}
				else if (current3[(GamepadButton)gamepadButtonEx].wasPressedThisFrame)
				{
					SetInputMode(InputMode.Gamepad);
					return true;
				}
			}
		}
		return false;
	}

	public static bool GetButton(Key k)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		if (isInputDisabled)
		{
			return false;
		}
		if (DewLaunchOptions.forceGamepad)
		{
			return false;
		}
		Keyboard current = Keyboard.current;
		if (current == null || (int)k == 0)
		{
			return false;
		}
		if (IsKeyPressedCached(k, current))
		{
			SetInputMode(InputMode.KeyboardAndMouse);
			return true;
		}
		return false;
	}

	public static bool GetButton(MouseButton m, bool checkGameArea)
	{
		if (isInputDisabled)
		{
			return false;
		}
		if (DewLaunchOptions.forceGamepad)
		{
			return false;
		}
		Mouse current = Mouse.current;
		if (current == null || m == MouseButton.None)
		{
			return false;
		}
		if (IsMousePressedCached(m, checkGameArea, current))
		{
			SetInputMode(InputMode.KeyboardAndMouse);
			return true;
		}
		return false;
	}

	public static bool GetButton(GamepadButtonEx? g)
	{
		if (isInputDisabled)
		{
			return false;
		}
		if (DewLaunchOptions.forceKeyboardAndMouse)
		{
			return false;
		}
		Gamepad current = Gamepad.current;
		if (current == null || !g.HasValue)
		{
			return false;
		}
		if (IsGamepadPressedCached(g.Value, current))
		{
			SetInputMode(InputMode.Gamepad);
			return true;
		}
		return false;
	}

	public static bool GetButton(DewBinding b, bool checkGameAreaForMouse)
	{
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		if (isInputDisabled)
		{
			return false;
		}
		Keyboard current = Keyboard.current;
		Mouse current2 = Mouse.current;
		if (!DewLaunchOptions.forceGamepad && b.HasPCAssigned())
		{
			for (int i = 0; i < b.pcBinds.Count; i++)
			{
				PCBind pCBind = b.pcBinds[i];
				if (pCBind.mouse != MouseButton.None && IsMousePressedCached(pCBind.mouse, checkGameAreaForMouse, current2))
				{
					SetInputMode(InputMode.KeyboardAndMouse);
					return true;
				}
				if ((int)pCBind.key != 0 && IsKeyPressedCached(pCBind.key, current))
				{
					SetInputMode(InputMode.KeyboardAndMouse);
					return true;
				}
			}
		}
		Gamepad current3 = Gamepad.current;
		if (!DewLaunchOptions.forceKeyboardAndMouse && current3 != null && b.HasGamepadAssigned())
		{
			for (int j = 0; j < b.gamepadBinds.Count; j++)
			{
				if (IsGamepadPressedCached(b.gamepadBinds[j], current3))
				{
					SetInputMode(InputMode.Gamepad);
					return true;
				}
			}
		}
		return false;
	}

	public static bool GetButtonUp(Key k)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		if (isInputDisabled)
		{
			return false;
		}
		if (DewLaunchOptions.forceGamepad)
		{
			return false;
		}
		if (Keyboard.current == null || (int)k == 0)
		{
			return false;
		}
		if (((ButtonControl)Keyboard.current[k]).wasReleasedThisFrame)
		{
			SetInputMode(InputMode.KeyboardAndMouse);
			return true;
		}
		return false;
	}

	public static bool GetButtonUp(MouseButton m, bool checkGameArea)
	{
		if (isInputDisabled)
		{
			return false;
		}
		if (DewLaunchOptions.forceGamepad)
		{
			return false;
		}
		if (Mouse.current == null || m == MouseButton.None)
		{
			return false;
		}
		if (checkGameArea && (m == MouseButton.Left || m == MouseButton.Right) && !IsGameRelatedMouseInputValid(m))
		{
			return false;
		}
		if (GetMouseButtonUp_Imp(m))
		{
			SetInputMode(InputMode.KeyboardAndMouse);
			return true;
		}
		return false;
	}

	public static bool GetButtonUp(GamepadButtonEx? g)
	{
		if (isInputDisabled)
		{
			return false;
		}
		if (DewLaunchOptions.forceKeyboardAndMouse)
		{
			return false;
		}
		if (Gamepad.current == null || !g.HasValue)
		{
			return false;
		}
		if (ManagerBase<InputManager>.instance.IsStickDirection(g.Value))
		{
			if (ManagerBase<InputManager>.instance.GetStickDirectionUp(g.Value))
			{
				SetInputMode(InputMode.Gamepad);
				return true;
			}
		}
		else if (Gamepad.current[(GamepadButton)g.Value].wasReleasedThisFrame)
		{
			SetInputMode(InputMode.Gamepad);
			return true;
		}
		return false;
	}

	public static bool GetButtonUp(DewBinding b, bool checkGameAreaForMouse)
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		if (b.HasPCAssigned())
		{
			foreach (PCBind pcBind in b.pcBinds)
			{
				if (pcBind.mouse != MouseButton.None && GetButtonUp(pcBind.mouse, checkGameAreaForMouse))
				{
					return true;
				}
				if ((int)pcBind.key != 0 && GetButtonUp(pcBind.key))
				{
					return true;
				}
			}
		}
		if (b.HasGamepadAssigned())
		{
			using List<GamepadButtonEx>.Enumerator enumerator2 = b.gamepadBinds.GetEnumerator();
			if (enumerator2.MoveNext())
			{
				return GetButtonUp((GamepadButtonEx?)enumerator2.Current);
			}
		}
		return false;
	}

	public static bool IsGameRelatedMouseInputValid(MouseButton button)
	{
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		if (button == MouseButton.Forward || button == MouseButton.Back)
		{
			return true;
		}
		if (Time.frameCount != _isMouseClickValidFrame)
		{
			if (SingletonBehaviour<GameArea>.softInstance == null)
			{
				_isMouseClickValid = ValidStatus.Yes;
			}
			else
			{
				Dew.RaycastAllUIElementsBelowCursor(_isMouseClickValidHits);
				_isMouseClickValid = ValidStatus.No;
				bool flag = false;
				foreach (RaycastResult isMouseClickValidHit in _isMouseClickValidHits)
				{
					RaycastResult current = isMouseClickValidHit;
					UI_GameInputPassThrough componentInParent = current.gameObject.GetComponentInParent<UI_GameInputPassThrough>();
					if (componentInParent != null)
					{
						if (componentInParent.exceptLeftClick)
						{
							flag = true;
						}
						continue;
					}
					if (current.gameObject == SingletonBehaviour<GameArea>.instance.gameObject)
					{
						_isMouseClickValid = ((!flag) ? ValidStatus.Yes : ValidStatus.YesExceptLeft);
					}
					break;
				}
				_isMouseClickValidFrame = Time.frameCount;
			}
		}
		if (_isMouseClickValid != ValidStatus.Yes)
		{
			if (_isMouseClickValid == ValidStatus.YesExceptLeft)
			{
				return button != MouseButton.Left;
			}
			return false;
		}
		return true;
	}
}
