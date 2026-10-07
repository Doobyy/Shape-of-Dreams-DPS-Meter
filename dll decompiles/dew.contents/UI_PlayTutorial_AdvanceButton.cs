public class UI_PlayTutorial_AdvanceButton : UI_GamepadFocusable, IGamepadFocusableOverrideInput
{
	private void Update()
	{
		if (DewInput.currentMode == InputMode.Gamepad && CanBeFocused() && ManagerBase<GlobalUIManager>.instance.focused != this)
		{
			ManagerBase<GlobalUIManager>.instance.SetFocus(this);
		}
	}

	public bool OnGamepadDpadDown()
	{
		return true;
	}

	public bool OnGamepadDpadLeft()
	{
		return true;
	}

	public bool OnGamepadDpadRight()
	{
		return true;
	}

	public bool OnGamepadDpadUp()
	{
		return true;
	}

	public bool OnGamepadBack()
	{
		return true;
	}
}
