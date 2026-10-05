using System;
using IngameDebugConsole;
using UnityEngine;

public class DevCanvas : SingletonBehaviour<DevCanvas>, ISettingsChangedCallback
{
	public enum DevCanvasState
	{
		Hidden,
		Shown,
		ShownWithoutPopup
	}

	[NonSerialized]
	public DevCanvasState state = DevCanvasState.ShownWithoutPopup;

	public GameObject consoleObject;

	private void Start()
	{
		RefreshVisibility();
	}

	private void Update()
	{
		if (DewSave.platformSettings.gameplay.enableDeveloperMode && Input.GetKeyDown(KeyCode.F1))
		{
			SwitchState();
		}
	}

	private void SwitchState()
	{
		state = (DevCanvasState)((int)(state + 1) % 3);
		ManagerBase<GlobalUIManager>.instance.ShowDevText(DewLocalization.GetUIValue($"Dev_DevCanvasState_{state}"), log: false);
		RefreshVisibility();
	}

	private void RefreshVisibility()
	{
		bool enableDeveloperMode = DewSave.platformSettings.gameplay.enableDeveloperMode;
		consoleObject.SetActive((state != DevCanvasState.Hidden) & enableDeveloperMode);
		((Component)(object)UnityEngine.Object.FindAnyObjectByType<DebugLogPopup>(FindObjectsInactive.Include)).gameObject.SetActive((state == DevCanvasState.Shown) & enableDeveloperMode);
	}

	public void OnSettingsChanged()
	{
		RefreshVisibility();
	}
}
