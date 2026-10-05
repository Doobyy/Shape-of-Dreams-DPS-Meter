using System;
using UnityEngine;

[RequireComponent(typeof(VisibilityController))]
public class PerControlVisibility : MonoBehaviour, IVisibilityComponent
{
	public bool keyboardAndMouse;

	public bool gamepad;

	private VisibilityController _controller;

	bool IVisibilityComponent.shouldBeShown
	{
		get
		{
			if (DewInput.currentMode != InputMode.KeyboardAndMouse || !keyboardAndMouse)
			{
				if (DewInput.currentMode == InputMode.Gamepad)
				{
					return gamepad;
				}
				return false;
			}
			return true;
		}
	}

	private void Start()
	{
		_controller = GetComponent<VisibilityController>();
		_controller.AddVisbilityComponent(this);
		_controller.UpdateVisibility();
		DewInput.onCurrentModeChanged += new Action<InputMode, InputMode>(OnCurrentModeChanged);
	}

	private void OnDestroy()
	{
		if ((bool)_controller)
		{
			_controller.RemoveVisbilityComponent(this);
		}
		DewInput.onCurrentModeChanged -= new Action<InputMode, InputMode>(OnCurrentModeChanged);
	}

	private void OnCurrentModeChanged(InputMode arg1, InputMode arg2)
	{
		_controller.UpdateVisibility();
	}
}
