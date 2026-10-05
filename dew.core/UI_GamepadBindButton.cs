using System;
using UnityEngine;
using UnityEngine.UI;

public class UI_GamepadBindButton : LogicBehaviour
{
	public enum ButtonType
	{
		Apply,
		Secondary,
		Tertiary,
		LeftShoulder,
		RightShoulder,
		LeftTrigger,
		RightTrigger
	}

	public ButtonType buttonType;

	public GameObject buttonDisplayObject;

	public bool ignoreInteractableForObject = true;

	public int priority = -10;

	private DewInputTrigger it_trigger;

	private Button _button;

	private void Start()
	{
		_button = GetComponent<Button>();
		it_trigger = new DewInputTrigger
		{
			binding = () => buttonType switch
			{
				ButtonType.Apply => DewSave.profileMain.controls.gamepadApply, 
				ButtonType.Secondary => DewSave.profileMain.controls.gamepadSecondary, 
				ButtonType.Tertiary => DewSave.profileMain.controls.gamepadTertiary, 
				ButtonType.LeftShoulder => DewBinding.GamepadOnly(GamepadButtonEx.LeftShoulder), 
				ButtonType.RightShoulder => DewBinding.GamepadOnly(GamepadButtonEx.RightShoulder), 
				ButtonType.LeftTrigger => DewBinding.GamepadOnly(GamepadButtonEx.LeftTrigger), 
				ButtonType.RightTrigger => DewBinding.GamepadOnly(GamepadButtonEx.RightTrigger), 
				_ => throw new ArgumentOutOfRangeException(), 
			},
			priority = priority,
			owner = this,
			canConsume = true,
			isValidCheck = () => (UnityEngine.Object)(object)_button != null && ((Behaviour)(object)_button).isActiveAndEnabled && ((Selectable)_button).IsInteractable()
		};
	}

	public override void FrameUpdate()
	{
		base.FrameUpdate();
		if (buttonDisplayObject != null)
		{
			buttonDisplayObject.SetActive(DewInput.currentMode == InputMode.Gamepad && (ignoreInteractableForObject || ((Selectable)_button).IsInteractable()));
		}
		if (it_trigger.down)
		{
			ManagerBase<GlobalUIManager>.instance.SimulateClickOnUIElement((Component)(object)_button);
		}
	}
}
