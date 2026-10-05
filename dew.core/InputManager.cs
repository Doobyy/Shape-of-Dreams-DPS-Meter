using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;

public class InputManager : ManagerBase<InputManager>, ISettingsChangedCallback
{
	private enum BindType
	{
		PC,
		Gamepad
	}

	private struct InputTriggerCheckItem
	{
		public DewInputTrigger trigger;

		public BindType bindType;

		public int bindIndex;

		public int priority;

		public int order;
	}

	private struct BucketCursor
	{
		public List<InputTriggerCheckItem> items;

		public int index;

		public int order;
	}

	private enum ConsumedKind : byte
	{
		Key,
		MouseButton,
		GamepadButtonEx
	}

	private struct ConsumedEntry
	{
		public ConsumedKind kind;

		public int code;

		public InputTriggerCheckItem item;
	}

	private readonly List<DewInputTrigger> _inputTriggers = new List<DewInputTrigger>();

	private readonly List<InputTriggerCheckItem> _checkedItems = new List<InputTriggerCheckItem>();

	private readonly List<InputTriggerCheckItem> _checkedItemsPC = new List<InputTriggerCheckItem>();

	private readonly List<InputTriggerCheckItem> _checkedItemsGamepad = new List<InputTriggerCheckItem>();

	private readonly Dictionary<int, List<InputTriggerCheckItem>> _checkedItemsPCByCode = new Dictionary<int, List<InputTriggerCheckItem>>();

	private readonly Dictionary<int, List<InputTriggerCheckItem>> _checkedItemsGamepadByCode = new Dictionary<int, List<InputTriggerCheckItem>>();

	private readonly List<int> _registeredPCCodes = new List<int>();

	private readonly List<int> _registeredGamepadCodes = new List<int>();

	private readonly List<BucketCursor> _activeBucketCursors = new List<BucketCursor>();

	private bool _checkedItemsSplitDirty = true;

	private readonly List<ConsumedEntry> _consumedInputs = new List<ConsumedEntry>();

	private const int KeyCacheSize = 512;

	private readonly int[] _keyStamp = new int[512];

	private readonly bool[] _keyPressed = new bool[512];

	private readonly int[] _mouseStamp = new int[8];

	private readonly bool[] _mousePressed = new bool[8];

	private int _lastUpdateFrameCount;

	internal int _lastResetInputDevicesFrameCount;

	private const int CleanupIntervalFrames = 120;

	private int _cleanupCounter;

	internal bool _LUDown;

	internal bool _LU;

	internal bool _LUUp;

	internal bool _LDDown;

	internal bool _LD;

	internal bool _LDUp;

	internal bool _LLDown;

	internal bool _LL;

	internal bool _LLUp;

	internal bool _LRDown;

	internal bool _LR;

	internal bool _LRUp;

	internal bool _RUDown;

	internal bool _RU;

	internal bool _RUUp;

	internal bool _RDDown;

	internal bool _RD;

	internal bool _RDUp;

	internal bool _RLDown;

	internal bool _RL;

	internal bool _RLUp;

	internal bool _RRDown;

	internal bool _RR;

	internal bool _RRUp;

	private const uint SPI_GETMOUSESPEED = 112u;

	private const uint SPI_SETMOUSESPEED = 113u;

	private const uint SPIF_UPDATEINIFILE = 1u;

	private const uint SPIF_SENDCHANGE = 2u;

	private int prevSensitivity = -1;

	private List<ShakeInstance> _shakes = new List<ShakeInstance>();

	private void Start()
	{
		DewSteam.onGameOverlayShownChanged += new Action<bool>(OnGameOverlayShownChanged);
	}

	private void OnGameOverlayShownChanged(bool obj)
	{
		ResetInputDevices();
	}

	public void ResetInputDevices()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		_lastResetInputDevicesFrameCount = Time.frameCount;
		Enumerator<InputDevice> enumerator = InputSystem.devices.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				InputSystem.ResetDevice(enumerator.Current, false);
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
	}

	public void ClearConsumedInputsAndForceReset()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		_lastResetInputDevicesFrameCount = Time.frameCount;
		int count = _consumedInputs.Count;
		_consumedInputs.Clear();
		Enumerator<InputDevice> enumerator = InputSystem.devices.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				InputSystem.ResetDevice(enumerator.Current, false);
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
		for (int i = 0; i < _inputTriggers.Count; i++)
		{
			_inputTriggers[i]?.Reset();
		}
		Debug.LogError($"[PS5IME] InputManager.ClearConsumedInputsAndForceReset: cleared {count} consumed inputs, reset {_inputTriggers.Count} triggers, reset all devices");
	}

	public override void FrameUpdate()
	{
		base.FrameUpdate();
		DewInput.UpdateScrollAxis();
		if (Mathf.Abs(Input.GetAxis("Mouse X")) > 0.01f || Mathf.Abs(Input.GetAxis("Mouse Y")) > 0.01f)
		{
			DewInput.SetInputMode(InputMode.KeyboardAndMouse);
		}
		if (new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")).magnitude > DewSave.platformSettings.controls.joystick0Dead || DewInput.GetButtonDownAnyGamepad())
		{
			DewInput.SetInputMode(InputMode.Gamepad);
		}
		UpdateJoystickStates();
		DoRumbleFrameUpdate();
		PrepareInputs();
		if (DewInput._currentAnyButtonListenerAction != null && Mouse.current != null && Mathf.Abs(((Vector2)((InputControl<Vector2>)(object)Mouse.current.scroll).value).y) > 0f)
		{
			DewInput._currentAnyButtonListenerAction?.Invoke((InputControl)(object)Mouse.current.scroll);
		}
	}

	public void RemoveTriggersByOwner(object owner)
	{
		if (owner == null)
		{
			return;
		}
		for (int num = _consumedInputs.Count - 1; num >= 0; num--)
		{
			DewInputTrigger trigger = _consumedInputs[num].item.trigger;
			if (trigger == null || trigger.owner == owner)
			{
				_consumedInputs.RemoveAt(num);
			}
		}
		for (int num2 = _checkedItems.Count - 1; num2 >= 0; num2--)
		{
			DewInputTrigger trigger2 = _checkedItems[num2].trigger;
			if (trigger2 == null || trigger2.owner == owner)
			{
				_checkedItems.RemoveAt(num2);
			}
		}
		for (int num3 = _inputTriggers.Count - 1; num3 >= 0; num3--)
		{
			DewInputTrigger dewInputTrigger = _inputTriggers[num3];
			if (dewInputTrigger == null || dewInputTrigger.owner == owner)
			{
				_inputTriggers.RemoveAt(num3);
			}
		}
		_checkedItemsSplitDirty = true;
	}

	internal void AddTrigger(DewInputTrigger t)
	{
		StartCoroutine(Routine());
		IEnumerator Routine()
		{
			yield return null;
			if (DewInputTrigger.MockTrigger != t)
			{
				_inputTriggers.Add(t);
				t._binding = t.binding();
				AppendCheckedItemsForTrigger(t);
				_checkedItems.Sort((InputTriggerCheckItem a, InputTriggerCheckItem b) => a.priority.CompareTo(b.priority));
				_checkedItemsSplitDirty = true;
			}
		}
	}

	internal void PrepareInputs()
	{
		if (Time.frameCount != _lastUpdateFrameCount)
		{
			_lastUpdateFrameCount = Time.frameCount;
			CleanupConsumedInputs();
			CleanupDeadOwners_IfNeeded();
			if (_checkedItemsSplitDirty)
			{
				RebuildCheckedItemSplits();
			}
			ValidateTriggersAndResetFlags();
			ApplyConsumedFlagsToTriggers();
			switch (DewInput.currentMode)
			{
			case InputMode.KeyboardAndMouse:
				ProcessCheckedItems_KBM();
				break;
			case InputMode.Gamepad:
				ProcessCheckedItems_Gamepad();
				break;
			}
			UpdateTriggers();
		}
	}

	private void AppendCheckedItemsForTrigger(DewInputTrigger t)
	{
		DewBinding binding = t._binding;
		if (binding.canAssignGamepad)
		{
			int count = binding.gamepadBinds.Count;
			for (int i = 0; i < count; i++)
			{
				_checkedItems.Add(new InputTriggerCheckItem
				{
					trigger = t,
					priority = t.GetCheckPriorityOfGamepadBind(i),
					bindIndex = i,
					bindType = BindType.Gamepad
				});
			}
		}
		if (binding.canAssignKeyboard || binding.canAssignMouse)
		{
			int count2 = binding.pcBinds.Count;
			for (int j = 0; j < count2; j++)
			{
				_checkedItems.Add(new InputTriggerCheckItem
				{
					trigger = t,
					priority = t.GetCheckPriorityOfPCBind(j),
					bindIndex = j,
					bindType = BindType.PC
				});
			}
		}
	}

	private void RebuildCheckedItemsFromTriggers()
	{
		_checkedItems.Clear();
		int num = 0;
		for (int i = 0; i < _inputTriggers.Count; i++)
		{
			DewInputTrigger dewInputTrigger = _inputTriggers[i];
			if (dewInputTrigger != null)
			{
				DewBinding binding = dewInputTrigger._binding;
				if (binding.canAssignGamepad)
				{
					num += binding.gamepadBinds.Count;
				}
				if (binding.canAssignKeyboard || binding.canAssignMouse)
				{
					num += binding.pcBinds.Count;
				}
			}
		}
		if (_checkedItems.Capacity < num)
		{
			_checkedItems.Capacity = num;
		}
		for (int j = 0; j < _inputTriggers.Count; j++)
		{
			DewInputTrigger dewInputTrigger2 = _inputTriggers[j];
			if (dewInputTrigger2 != null)
			{
				AppendCheckedItemsForTrigger(dewInputTrigger2);
			}
		}
		_checkedItems.Sort((InputTriggerCheckItem a, InputTriggerCheckItem b) => a.priority.CompareTo(b.priority));
		_checkedItemsSplitDirty = true;
	}

	private void RebuildCheckedItemSplits()
	{
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Expected I4, but got Unknown
		_checkedItemsPC.Clear();
		_checkedItemsGamepad.Clear();
		_checkedItemsPCByCode.Clear();
		_checkedItemsGamepadByCode.Clear();
		_registeredPCCodes.Clear();
		_registeredGamepadCodes.Clear();
		for (int i = 0; i < _checkedItems.Count; i++)
		{
			InputTriggerCheckItem item = _checkedItems[i];
			item.order = i;
			if (item.bindType == BindType.PC)
			{
				_checkedItemsPC.Add(item);
				PCBind pCBind = item.trigger._binding.pcBinds[item.bindIndex];
				if ((int)pCBind.key != 0)
				{
					AddCheckedItemByCode(_checkedItemsPCByCode, _registeredPCCodes, (int)pCBind.key, item);
				}
				if (pCBind.mouse != MouseButton.None)
				{
					AddCheckedItemByCode(_checkedItemsPCByCode, _registeredPCCodes, (int)(pCBind.mouse + 512), item);
				}
			}
			else
			{
				_checkedItemsGamepad.Add(item);
				GamepadButtonEx code = item.trigger._binding.gamepadBinds[item.bindIndex];
				AddCheckedItemByCode(_checkedItemsGamepadByCode, _registeredGamepadCodes, (int)code, item);
			}
		}
		_checkedItemsSplitDirty = false;
	}

	private static void AddCheckedItemByCode(Dictionary<int, List<InputTriggerCheckItem>> buckets, List<int> registeredCodes, int code, InputTriggerCheckItem item)
	{
		if (!buckets.TryGetValue(code, out var value))
		{
			value = new List<InputTriggerCheckItem>();
			buckets.Add(code, value);
			registeredCodes.Add(code);
		}
		value.Add(item);
	}

	private void CleanupDeadOwners_IfNeeded()
	{
		if (++_cleanupCounter < 120)
		{
			return;
		}
		_cleanupCounter = 0;
		int count = _inputTriggers.Count;
		if (count == 0)
		{
			return;
		}
		int num = 0;
		bool flag = false;
		for (int i = 0; i < count; i++)
		{
			DewInputTrigger dewInputTrigger = _inputTriggers[i];
			if (dewInputTrigger == null)
			{
				flag = true;
				continue;
			}
			if (dewInputTrigger.owner == null)
			{
				flag = true;
				continue;
			}
			if (num != i)
			{
				_inputTriggers[num] = dewInputTrigger;
			}
			num++;
		}
		if (flag)
		{
			_inputTriggers.RemoveRange(num, count - num);
			RebuildCheckedItemsFromTriggers();
		}
	}

	private void ValidateTriggersAndResetFlags()
	{
		for (int i = 0; i < _inputTriggers.Count; i++)
		{
			DewInputTrigger dewInputTrigger = _inputTriggers[i];
			if (dewInputTrigger.owner == null)
			{
				dewInputTrigger._isValid = false;
				dewInputTrigger._flag = false;
				continue;
			}
			try
			{
				dewInputTrigger._isValid = dewInputTrigger.isValidCheck == null || dewInputTrigger.isValidCheck();
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
			dewInputTrigger._flag = false;
		}
	}

	private void ApplyConsumedFlagsToTriggers()
	{
		for (int i = 0; i < _consumedInputs.Count; i++)
		{
			DewInputTrigger trigger = _consumedInputs[i].item.trigger;
			trigger._flag = true;
			if (!trigger._isValid)
			{
				trigger._isSuppressed = true;
			}
		}
	}

	private bool GetKeyPressedCached(Keyboard kb, Key key)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Expected I4, but got Unknown
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		int num = (int)key;
		if ((uint)num >= 512u)
		{
			return ((ButtonControl)kb[key]).isPressed;
		}
		int frameCount = Time.frameCount;
		if (_keyStamp[num] == frameCount)
		{
			return _keyPressed[num];
		}
		_keyStamp[num] = frameCount;
		bool isPressed = ((ButtonControl)kb[key]).isPressed;
		_keyPressed[num] = isPressed;
		return isPressed;
	}

	private bool GetMousePressedCached(MouseButton mb)
	{
		if ((uint)mb >= 8u)
		{
			return DewInput.GetMouseButton_Imp(mb);
		}
		int frameCount = Time.frameCount;
		if (_mouseStamp[(int)mb] == frameCount)
		{
			return _mousePressed[(int)mb];
		}
		_mouseStamp[(int)mb] = frameCount;
		bool mouseButton_Imp = DewInput.GetMouseButton_Imp(mb);
		_mousePressed[(int)mb] = mouseButton_Imp;
		return mouseButton_Imp;
	}

	private bool IsConsumed(ConsumedKind kind, int code)
	{
		for (int i = 0; i < _consumedInputs.Count; i++)
		{
			ConsumedEntry consumedEntry = _consumedInputs[i];
			if (consumedEntry.kind == kind && consumedEntry.code == code)
			{
				return true;
			}
		}
		return false;
	}

	private void AddConsumed(ConsumedKind kind, int code, InputTriggerCheckItem item)
	{
		_consumedInputs.Add(new ConsumedEntry
		{
			kind = kind,
			code = code,
			item = item
		});
	}

	private void CleanupConsumedInputs()
	{
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		Keyboard current = Keyboard.current;
		Mouse current2 = Mouse.current;
		Gamepad current3 = Gamepad.current;
		for (int num = _consumedInputs.Count - 1; num >= 0; num--)
		{
			ConsumedEntry consumedEntry = _consumedInputs[num];
			switch (consumedEntry.kind)
			{
			case ConsumedKind.Key:
			{
				Key key = (Key)consumedEntry.code;
				if (current == null || !GetKeyPressedCached(current, key))
				{
					_consumedInputs.RemoveAt(num);
				}
				break;
			}
			case ConsumedKind.MouseButton:
			{
				MouseButton code2 = (MouseButton)consumedEntry.code;
				if (current2 == null || !GetMousePressedCached(code2))
				{
					_consumedInputs.RemoveAt(num);
				}
				break;
			}
			case ConsumedKind.GamepadButtonEx:
			{
				GamepadButtonEx code = (GamepadButtonEx)consumedEntry.code;
				if (IsStickDirection(code))
				{
					if (!GetStickDirection(code))
					{
						_consumedInputs.RemoveAt(num);
					}
				}
				else if (current3 == null || !current3[(GamepadButton)code].isPressed)
				{
					_consumedInputs.RemoveAt(num);
				}
				break;
			}
			}
		}
	}

	private void ProcessCheckedItems_KBM()
	{
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Expected I4, but got Unknown
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		Keyboard current = Keyboard.current;
		Mouse current2 = Mouse.current;
		if (current == null && current2 == null)
		{
			return;
		}
		CollectActivePCBuckets(current, current2);
		InputTriggerCheckItem item;
		while (TryGetNextActiveItem(out item))
		{
			try
			{
				DewInputTrigger trigger = item.trigger;
				if (trigger._flag || !trigger._isValid)
				{
					continue;
				}
				PCBind pCBind = trigger._binding.pcBinds[item.bindIndex];
				if ((int)pCBind.key != 0 && current != null)
				{
					int code = (int)pCBind.key;
					bool flag = IsConsumed(ConsumedKind.Key, code);
					if ((!flag || trigger.ignoreConsumeCheck) && GetKeyPressedCached(current, pCBind.key))
					{
						bool flag2 = true;
						foreach (Key modifier in pCBind.modifiers)
						{
							if (!GetKeyPressedCached(current, modifier))
							{
								flag2 = false;
								break;
							}
						}
						if (flag2)
						{
							if (trigger.canConsume && !flag)
							{
								AddConsumed(ConsumedKind.Key, code, item);
							}
							trigger._flag = true;
						}
					}
				}
				if (pCBind.mouse == MouseButton.None || current2 == null)
				{
					continue;
				}
				int mouse = (int)pCBind.mouse;
				bool flag3 = IsConsumed(ConsumedKind.MouseButton, mouse);
				if ((flag3 && !trigger.ignoreConsumeCheck) || !GetMousePressedCached(pCBind.mouse))
				{
					continue;
				}
				if (trigger.checkGameAreaForMouse && !DewInput.IsGameRelatedMouseInputValid(pCBind.mouse))
				{
					trigger._isSuppressed = true;
				}
				bool flag4 = true;
				foreach (Key modifier2 in pCBind.modifiers)
				{
					if (current == null || !GetKeyPressedCached(current, modifier2))
					{
						flag4 = false;
						break;
					}
				}
				if (flag4)
				{
					if (trigger.canConsume && !flag3)
					{
						AddConsumed(ConsumedKind.MouseButton, mouse, item);
					}
					trigger._flag = true;
				}
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
	}

	private void ProcessCheckedItems_Gamepad()
	{
		Gamepad current = Gamepad.current;
		if (current == null)
		{
			return;
		}
		CollectActiveGamepadBuckets(current);
		InputTriggerCheckItem item;
		while (TryGetNextActiveItem(out item))
		{
			try
			{
				DewInputTrigger trigger = item.trigger;
				if (trigger._flag || !trigger._isValid)
				{
					continue;
				}
				GamepadButtonEx gamepadButtonEx = trigger._binding.gamepadBinds[item.bindIndex];
				bool flag = ((!IsStickDirection(gamepadButtonEx)) ? current[(GamepadButton)gamepadButtonEx].isPressed : GetStickDirection(gamepadButtonEx));
				int code = (int)gamepadButtonEx;
				bool flag2 = IsConsumed(ConsumedKind.GamepadButtonEx, code);
				if ((!flag2 || trigger.ignoreConsumeCheck) & flag)
				{
					if (trigger.canConsume && !flag2)
					{
						AddConsumed(ConsumedKind.GamepadButtonEx, code, item);
					}
					trigger._flag = true;
				}
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
	}

	private void CollectActivePCBuckets(Keyboard kb, Mouse ms)
	{
		_activeBucketCursors.Clear();
		for (int i = 0; i < _registeredPCCodes.Count; i++)
		{
			int num = _registeredPCCodes[i];
			if (!_checkedItemsPCByCode.TryGetValue(num, out var value))
			{
				continue;
			}
			bool num2;
			if (num < 512)
			{
				if (kb == null)
				{
					continue;
				}
				num2 = GetKeyPressedCached(kb, (Key)num);
			}
			else
			{
				if (ms == null)
				{
					continue;
				}
				num2 = GetMousePressedCached((MouseButton)(num - 512));
			}
			if (num2)
			{
				PushActiveBucket(value);
			}
		}
	}

	private void CollectActiveGamepadBuckets(Gamepad gp)
	{
		_activeBucketCursors.Clear();
		for (int i = 0; i < _registeredGamepadCodes.Count; i++)
		{
			int num = _registeredGamepadCodes[i];
			if (_checkedItemsGamepadByCode.TryGetValue(num, out var value))
			{
				GamepadButtonEx gamepadButtonEx = (GamepadButtonEx)num;
				if (IsStickDirection(gamepadButtonEx) ? GetStickDirection(gamepadButtonEx) : gp[(GamepadButton)gamepadButtonEx].isPressed)
				{
					PushActiveBucket(value);
				}
			}
		}
	}

	private bool TryGetNextActiveItem(out InputTriggerCheckItem item)
	{
		if (_activeBucketCursors.Count == 0)
		{
			item = default;
			return false;
		}
		BucketCursor value = _activeBucketCursors[0];
		item = value.items[value.index++];
		if (value.index < value.items.Count)
		{
			value.order = value.items[value.index].order;
			_activeBucketCursors[0] = value;
			HeapifyActiveBucketsDown(0);
			return true;
		}
		int num = _activeBucketCursors.Count - 1;
		if (num == 0)
		{
			_activeBucketCursors.Clear();
			return true;
		}
		_activeBucketCursors[0] = _activeBucketCursors[num];
		_activeBucketCursors.RemoveAt(num);
		HeapifyActiveBucketsDown(0);
		return true;
	}

	private void PushActiveBucket(List<InputTriggerCheckItem> bucket)
	{
		_activeBucketCursors.Add(new BucketCursor
		{
			items = bucket,
			index = 0,
			order = bucket[0].order
		});
		HeapifyActiveBucketsUp(_activeBucketCursors.Count - 1);
	}

	private void HeapifyActiveBucketsUp(int index)
	{
		while (index > 0)
		{
			int num = (index - 1) / 2;
			if (_activeBucketCursors[num].order <= _activeBucketCursors[index].order)
			{
				break;
			}
			SwapActiveBuckets(num, index);
			index = num;
		}
	}

	private void HeapifyActiveBucketsDown(int index)
	{
		int count = _activeBucketCursors.Count;
		while (true)
		{
			int num = index * 2 + 1;
			if (num >= count)
			{
				break;
			}
			int num2 = num + 1;
			int num3 = num;
			if (num2 < count && _activeBucketCursors[num2].order < _activeBucketCursors[num].order)
			{
				num3 = num2;
			}
			if (_activeBucketCursors[index].order <= _activeBucketCursors[num3].order)
			{
				break;
			}
			SwapActiveBuckets(index, num3);
			index = num3;
		}
	}

	private void SwapActiveBuckets(int a, int b)
	{
		BucketCursor value = _activeBucketCursors[a];
		_activeBucketCursors[a] = _activeBucketCursors[b];
		_activeBucketCursors[b] = value;
	}

	private void UpdateTriggers()
	{
		for (int i = 0; i < _inputTriggers.Count; i++)
		{
			_inputTriggers[i].Update();
		}
	}

	public void OnSettingsChanged()
	{
		try
		{
			OnSettingsChanged_MouseSensitivity();
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
		_lastUpdateFrameCount = -1;
		_consumedInputs.Clear();
		for (int num = _inputTriggers.Count - 1; num >= 0; num--)
		{
			DewInputTrigger dewInputTrigger = _inputTriggers[num];
			if (dewInputTrigger == null || dewInputTrigger.owner == null)
			{
				_inputTriggers.RemoveAt(num);
			}
			else
			{
				try
				{
					dewInputTrigger._binding = dewInputTrigger.binding();
				}
				catch (Exception exception2)
				{
					Debug.LogException(exception2);
				}
				dewInputTrigger.Reset();
			}
		}
		_checkedItemsPC.Clear();
		_checkedItemsGamepad.Clear();
		RebuildCheckedItemsFromTriggers();
	}

	public bool IsStickDirection(GamepadButtonEx g)
	{
		if (g >= GamepadButtonEx.LeftStickUp)
		{
			return g <= GamepadButtonEx.RightStickRight;
		}
		return false;
	}

	public bool GetStickDirection(GamepadButtonEx g)
	{
		if (DewLaunchOptions.forceKeyboardAndMouse)
		{
			return false;
		}
		return g switch
		{
			GamepadButtonEx.LeftStickUp => _LU, 
			GamepadButtonEx.LeftStickDown => _LD, 
			GamepadButtonEx.LeftStickLeft => _LL, 
			GamepadButtonEx.LeftStickRight => _LR, 
			GamepadButtonEx.RightStickUp => _RU, 
			GamepadButtonEx.RightStickDown => _RD, 
			GamepadButtonEx.RightStickLeft => _RL, 
			GamepadButtonEx.RightStickRight => _RR, 
			_ => false, 
		};
	}

	public bool GetStickDirectionDown(GamepadButtonEx g)
	{
		if (DewLaunchOptions.forceKeyboardAndMouse)
		{
			return false;
		}
		return g switch
		{
			GamepadButtonEx.LeftStickUp => _LUDown, 
			GamepadButtonEx.LeftStickDown => _LDDown, 
			GamepadButtonEx.LeftStickLeft => _LLDown, 
			GamepadButtonEx.LeftStickRight => _LRDown, 
			GamepadButtonEx.RightStickUp => _RUDown, 
			GamepadButtonEx.RightStickDown => _RDDown, 
			GamepadButtonEx.RightStickLeft => _RLDown, 
			GamepadButtonEx.RightStickRight => _RRDown, 
			_ => false, 
		};
	}

	public bool GetStickDirectionUp(GamepadButtonEx g)
	{
		if (DewLaunchOptions.forceKeyboardAndMouse)
		{
			return false;
		}
		return g switch
		{
			GamepadButtonEx.LeftStickUp => _LUUp, 
			GamepadButtonEx.LeftStickDown => _LDUp, 
			GamepadButtonEx.LeftStickLeft => _LLUp, 
			GamepadButtonEx.LeftStickRight => _LRUp, 
			GamepadButtonEx.RightStickUp => _RUUp, 
			GamepadButtonEx.RightStickDown => _RDUp, 
			GamepadButtonEx.RightStickLeft => _RLUp, 
			GamepadButtonEx.RightStickRight => _RRUp, 
			_ => false, 
		};
	}

	private void UpdateJoystickStates()
	{
		Vector2 leftJoystick = DewInput.GetLeftJoystick();
		Vector2 leftJoystick2 = DewInput.GetLeftJoystick();
		float num = 0.8f;
		bool flag = leftJoystick.magnitude > num;
		bool flag2 = leftJoystick2.magnitude > num;
		DoJoystickDirection(flag && Vector2.Angle(Vector2.left, leftJoystick) < 67.5f, out _LLDown, ref _LL, out _LLUp);
		DoJoystickDirection(flag && Vector2.Angle(Vector2.right, leftJoystick) < 67.5f, out _LRDown, ref _LR, out _LRUp);
		DoJoystickDirection(flag && Vector2.Angle(Vector2.down, leftJoystick) < 67.5f, out _LDDown, ref _LD, out _LDUp);
		DoJoystickDirection(flag && Vector2.Angle(Vector2.up, leftJoystick) < 67.5f, out _LUDown, ref _LU, out _LUUp);
		DoJoystickDirection(flag2 && Vector2.Angle(Vector2.left, leftJoystick2) < 67.5f, out _RLDown, ref _RL, out _RLUp);
		DoJoystickDirection(flag2 && Vector2.Angle(Vector2.right, leftJoystick2) < 67.5f, out _RRDown, ref _RR, out _RRUp);
		DoJoystickDirection(flag2 && Vector2.Angle(Vector2.down, leftJoystick2) < 67.5f, out _RDDown, ref _RD, out _RDUp);
		DoJoystickDirection(flag2 && Vector2.Angle(Vector2.up, leftJoystick2) < 67.5f, out _RUDown, ref _RU, out _RUUp);
		static void DoJoystickDirection(bool condition, out bool down, ref bool held, out bool up)
		{
			if (condition)
			{
				down = !held;
				held = true;
				up = false;
			}
			else
			{
				up = held;
				held = false;
				down = false;
			}
		}
	}

	[DllImport("user32.dll")]
	private static extern bool SystemParametersInfo(uint uiAction, uint uiParam, ref int pvParam, uint fWinIni);

	[DllImport("user32.dll")]
	private static extern bool SystemParametersInfo(uint uiAction, uint uiParam, int pvParam, uint fWinIni);

	public static int GetMouseSensitivity()
	{
		int pvParam = 0;
		SystemParametersInfo(112u, 0u, ref pvParam, 0u);
		return pvParam;
	}

	public static void SetMouseSensitivity(int sensitivity)
	{
		sensitivity = Math.Clamp(sensitivity, 1, 20);
		SystemParametersInfo(113u, 0u, sensitivity, 1u);
	}

	private void OnApplicationFocus(bool hasFocus)
	{
		if (hasFocus)
		{
			if (prevSensitivity == -1)
			{
				prevSensitivity = GetMouseSensitivity();
			}
			SetMouseSensitivity(DewSave.profileMain.controls.mouseSensitivity);
		}
		else if (prevSensitivity != -1)
		{
			SetMouseSensitivity(prevSensitivity);
			prevSensitivity = -1;
		}
		ResetInputDevices();
	}

	private void OnApplicationQuit()
	{
		if (prevSensitivity != -1)
		{
			SetMouseSensitivity(prevSensitivity);
			prevSensitivity = -1;
		}
	}

	private void OnSettingsChanged_MouseSensitivity()
	{
		if (prevSensitivity == -1)
		{
			prevSensitivity = GetMouseSensitivity();
		}
		SetMouseSensitivity(DewSave.profileMain.controls.mouseSensitivity);
	}

	public void AddShakeInstance(ShakeInstance si)
	{
		si.attackTime *= 0.7f;
		si.sustainTime *= 0.9f;
		si.decayTime *= 0.3f;
		_shakes.Add(si);
	}

	public void ClearShakeInstances()
	{
		_shakes.Clear();
	}

	private void DoRumbleFrameUpdate()
	{
		float num = 0f;
		float num2 = 0f;
		float gamepadVibrationStrength = DewSave.profileMain.gameplay.gamepadVibrationStrength;
		float num3 = 2.5f * gamepadVibrationStrength;
		for (int num4 = _shakes.Count - 1; num4 >= 0; num4--)
		{
			ShakeInstance shakeInstance = _shakes[num4];
			float num5 = Time.time - shakeInstance.startTime;
			if (gamepadVibrationStrength < 1f)
			{
				num5 /= 0.5f + gamepadVibrationStrength * 0.5f;
			}
			if (num5 > shakeInstance.attackTime + shakeInstance.sustainTime + shakeInstance.decayTime)
			{
				_shakes.RemoveAt(num4);
			}
			else if (!(Time.timeScale < 0.001f))
			{
				float num6;
				if (num5 < shakeInstance.attackTime)
				{
					num6 = 1f;
				}
				else
				{
					num6 = ((!(num5 < shakeInstance.attackTime + shakeInstance.sustainTime)) ? (1f - (num5 - shakeInstance.attackTime - shakeInstance.sustainTime) / shakeInstance.decayTime) : 1f);
				}
				num += shakeInstance.amplitude * 0.1f * num6 * num3;
				num2 += shakeInstance.amplitude * 0f * num6 * num3;
			}
		}
		if (DewInput.currentMode != InputMode.Gamepad)
		{
			num = 0f;
			num2 = 0f;
		}
		if (Gamepad.current != null)
		{
			Gamepad.current.SetMotorSpeeds(num, num2);
		}
	}

	private void OnDestroy()
	{
		if (Gamepad.current != null)
		{
			Gamepad.current.SetMotorSpeeds(0f, 0f);
		}
	}
}
